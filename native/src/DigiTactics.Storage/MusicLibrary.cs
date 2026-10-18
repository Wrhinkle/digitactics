using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed record MusicTrack(string Id, string Name, string Use, string Sha256, long Size, double Added, JsonObject Metadata, JsonObject Blob);

/// <summary>Local music bytes are immutable and content-addressed; a single atomic index commits library edits.</summary>
public sealed class MusicLibrary
{
    private readonly string directory, indexPath;
    private string? loadedText;
    private JsonObject index = new() { ["version"] = 1, ["tracks"] = new JsonArray(), ["imports"] = new JsonArray() };
    private bool readOnly;
    public string? Error { get; private set; }
    public IReadOnlyList<MusicTrack> Tracks { get; private set; } = [];
    public MusicLibrary(string directory)
    {
        this.directory = Path.GetFullPath(directory); indexPath = Path.Combine(this.directory,"library.json");
        try
        {
            if (!File.Exists(indexPath)) return;
            loadedText = File.ReadAllText(indexPath);
            index = JsonNode.Parse(loadedText) as JsonObject ?? throw new InvalidDataException("Invalid music library.");
            Tracks = ReadIndex(index);
        }
        catch (Exception e) when (Expected(e)) { readOnly = true; Error = "Music library was preserved: " + e.Message; }
    }
    private static bool Expected(Exception e) => e is IOException or InvalidDataException or KeyNotFoundException or UnauthorizedAccessException or JsonException or InvalidOperationException or FormatException or ArgumentException or OverflowException;
    private static bool Digest(string? value) => value is { Length:64 } && value.All(c=>c is >= '0' and <= '9' or >= 'a' and <= 'f');
    private static IReadOnlyList<MusicTrack> ReadIndex(JsonObject data)
    {
        if (data["version"]?.GetValue<int>() != 1 || data["tracks"] is not JsonArray tracks || data["imports"] is not JsonArray imports || imports.Any(v=>!Digest(v?.GetValue<string>()))) throw new InvalidDataException("Unsupported music library index.");
        var result = new List<MusicTrack>(); var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in tracks)
        {
            var metadata = value?["metadata"] as JsonObject ?? throw new InvalidDataException("Missing track metadata.");
            string id=metadata["id"]?.GetValue<string>() ?? throw new InvalidDataException("Missing track identifier."), sha=value!["sha256"]?.GetValue<string>() ?? throw new InvalidDataException("Missing audio digest.");
            long size=value["size"]?.GetValue<long>() ?? throw new InvalidDataException("Missing track size.");
            if (string.IsNullOrEmpty(id) || !ids.Add(id) || !Digest(sha) || size < 0 || size > 256L*1024*1024) throw new InvalidDataException("Invalid music track.");
            string use=metadata["use"]?.GetValue<string>() ?? "both";
            if (use is not ("both" or "planning" or "combat")) use="both";
            double added=metadata["added"]?.Deserialize<double>() ?? 0;
            if(!double.IsFinite(added)) throw new InvalidDataException("Invalid track date.");
            result.Add(new(id,metadata["name"]?.GetValue<string>() ?? id,use,sha,size,added,(JsonObject)metadata.DeepClone(),value["blob"] is JsonObject blob?(JsonObject)blob.DeepClone():new()));
        }
        return result.OrderBy(t=>t.Added).ToArray();
    }
    private string AudioPath(string sha) => Digest(sha) ? Path.Combine(directory,sha+".audio") : throw new InvalidDataException("Invalid audio identifier.");
    public byte[] ReadAudio(string id)
    {
        var track=Tracks.Single(t=>t.Id==id);
        var file=new FileInfo(AudioPath(track.Sha256));
        if(file.Length!=track.Size) throw new InvalidDataException("Music file size changed; the file was preserved.");
        var bytes=File.ReadAllBytes(file.FullName);
        if(!string.Equals(Convert.ToHexString(SHA256.HashData(bytes)),track.Sha256,StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Music integrity check failed; the file was preserved.");
        return bytes;
    }
    private void SaveAudio(string sha,byte[] bytes)
    {
        string path=AudioPath(sha);
        if(File.Exists(path))
        {
            if(new FileInfo(path).Length!=bytes.Length || !File.ReadAllBytes(path).AsSpan().SequenceEqual(bytes)) throw new InvalidDataException("An existing music file changed; it was preserved.");
            return;
        }
        WriteAtomic(path,bytes,false);
    }
    private static void WriteAtomic(string path,byte[] bytes,bool replace)
    {
        string temporary=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        try
        {
            using(var file=new FileStream(temporary,FileMode.CreateNew,FileAccess.Write,FileShare.None)) { file.Write(bytes); file.Flush(true); }
            if(replace) File.Replace(temporary,path,path+".bak"); else File.Move(temporary,path);
        }
        finally { if(File.Exists(temporary)) File.Delete(temporary); }
    }
    private bool Edit(Action<JsonObject> edit)
    {
        if(readOnly) return false;
        try
        {
            Directory.CreateDirectory(directory);
            using var guard=new FileStream(indexPath+".lock",FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
            string? current=File.Exists(indexPath)?File.ReadAllText(indexPath):null;
            if(current!=loadedText) { readOnly=true; throw new IOException("Music changed in another app instance. Reload the library before editing."); }
            var next=(JsonObject)index.DeepClone(); edit(next); var tracks=ReadIndex(next);
            if(JsonNode.DeepEquals(index,next)) { Error=null; return true; }
            string json=next.ToJsonString(new JsonSerializerOptions {WriteIndented=true})+"\n";
            WriteAtomic(indexPath,System.Text.Encoding.UTF8.GetBytes(json),current is not null);
            loadedText=json; index=next; Tracks=tracks; Error=null; return true;
        }
        catch(Exception e) when(Expected(e)) { Error="Music library was not changed: "+e.Message; return false; }
    }
    public bool ImportLegacy(string archivePath,string? expectedSha256 = null)
    {
        return Edit(next=>
        {
            if(new FileInfo(archivePath).Length>LegacyProfileArchive.MaximumArchiveBytes) throw new InvalidDataException("Music backup exceeds the supported size.");
            byte[] bytes=File.ReadAllBytes(archivePath); var summary=LegacyProfileArchive.Inspect(bytes);
            if(expectedSha256 is not null && summary.Sha256!=expectedSha256) throw new InvalidDataException("Imported profile archive changed; it was preserved.");
            if(next["imports"]!.AsArray().Any(v=>v!.GetValue<string>()==summary.Sha256)) return;
            using var document=JsonDocument.Parse(bytes);
            var tracks=next["tracks"]!.AsArray(); var ids=tracks.Select(t=>t!["metadata"]!["id"]!.GetValue<string>()).ToHashSet(StringComparer.Ordinal);
            foreach(var source in document.RootElement.GetProperty("music").EnumerateArray())
            {
                string id=source.GetProperty("metadata").GetProperty("id").GetString()!;
                if(!ids.Add(id)) throw new InvalidDataException("An imported track identifier already exists. Existing music was preserved.");
                byte[] audio=Convert.FromBase64String(source.GetProperty("data").GetString()!);
                string sha=Convert.ToHexString(SHA256.HashData(audio)).ToLowerInvariant(); SaveAudio(sha,audio);
                tracks.Add(new JsonObject { ["metadata"]=JsonNode.Parse(source.GetProperty("metadata").GetRawText()),["blob"]=source.TryGetProperty("blob",out var blob)?JsonNode.Parse(blob.GetRawText()):new JsonObject(),["sha256"]=sha,["size"]=audio.LongLength });
            }
            next["imports"]!.AsArray().Add(summary.Sha256);
        });
    }
    public bool AddFile(string path) => Edit(next=>
    {
        string extension=Path.GetExtension(path).ToLowerInvariant();
        if(extension is not (".mp3" or ".ogg" or ".oga" or ".m4a" or ".aac" or ".wav" or ".flac" or ".opus" or ".webm")) throw new InvalidDataException("Choose an audio file.");
        if(new FileInfo(path).Length>256L*1024*1024) throw new InvalidDataException("Music file exceeds 256 MB.");
        byte[] bytes=File.ReadAllBytes(path);
        string sha=Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(); SaveAudio(sha,bytes);
        next["tracks"]!.AsArray().Add(new JsonObject
        {
            ["metadata"]=new JsonObject { ["id"]=Guid.NewGuid().ToString("N"),["name"]=Path.GetFileNameWithoutExtension(path),["use"]="both",["size"]=bytes.LongLength,["added"]=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() },
            ["blob"]=new JsonObject { ["name"]=Path.GetFileName(path),["lastModified"]=new DateTimeOffset(File.GetLastWriteTimeUtc(path)).ToUnixTimeMilliseconds() },
            ["sha256"]=sha,["size"]=bytes.LongLength
        });
    });
    public bool SetUse(string id,string use)
    {
        if(use is not ("both" or "planning" or "combat")) { Error="Choose planning, combat or both."; return false; }
        return Edit(next=> { var track=next["tracks"]!.AsArray().Single(t=>t!["metadata"]!["id"]!.GetValue<string>()==id)!; track["metadata"]!["use"]=use; });
    }
    // Removing an index entry leaves its bytes available to older index backups; filenames never come from user metadata.
    public bool Remove(string id) => Edit(next=> { var tracks=next["tracks"]!.AsArray(); var track=tracks.Single(t=>t!["metadata"]!["id"]!.GetValue<string>()==id); tracks.Remove(track); });
}

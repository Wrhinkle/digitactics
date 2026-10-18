using System.Security.Cryptography;
using System.Text.Json;

namespace DigiTactics.Storage;

public sealed record LegacyProfileSummary(int StorageEntries, int MusicTracks, long MusicBytes, string Sha256);

/// <summary>Validates and stages a complete legacy backup without applying or merging player data.</summary>
public static class LegacyProfileArchive
{
    public const long MaximumArchiveBytes = 384L * 1024 * 1024;
    public static LegacyProfileSummary Inspect(ReadOnlyMemory<byte> bytes)
    {
        if (bytes.Length > MaximumArchiveBytes) throw new InvalidDataException("Profile backup exceeds the supported size.");
        using var document = JsonDocument.Parse(bytes);
        var root = document.RootElement;
        void Unique(JsonElement value)
        {
            if (value.ValueKind == JsonValueKind.Object)
            {
                var names = new HashSet<string>(StringComparer.Ordinal);
                foreach (var property in value.EnumerateObject())
                { if (!names.Add(property.Name)) throw new InvalidDataException("Duplicate backup field."); Unique(property.Value); }
            }
            else if (value.ValueKind == JsonValueKind.Array) foreach (var item in value.EnumerateArray()) Unique(item);
        }
        Unique(root);
        if (root.ValueKind != JsonValueKind.Object || root.GetProperty("format").GetString() != "digitactics-profile" || root.GetProperty("version").GetInt32() != 1)
            throw new InvalidDataException("Unsupported profile backup format.");
        var storage = root.GetProperty("storage");
        if (storage.ValueKind != JsonValueKind.Object) throw new InvalidDataException("Invalid saved storage.");
        int entries = 0, tracks = 0; long total = 0, textLength = 0;
        foreach (var entry in storage.EnumerateObject())
        {
            if (!(entry.Name.StartsWith("dt.", StringComparison.Ordinal) || entry.Name.StartsWith("dt-", StringComparison.Ordinal)) || entry.Value.ValueKind != JsonValueKind.String)
                throw new InvalidDataException("Invalid saved storage entry.");
            textLength += entry.Name.Length + entry.Value.GetString()!.Length; entries++;
        }
        if (textLength > 16 * 1024 * 1024) throw new InvalidDataException("Saved settings exceed the supported size.");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var track in root.GetProperty("music").EnumerateArray())
        {
            var metadata = track.GetProperty("metadata");
            string? id = metadata.GetProperty("id").GetString();
            if (string.IsNullOrEmpty(id) || !ids.Add(id)) throw new InvalidDataException("Missing or duplicate music identifier.");
            byte[] audio = Convert.FromBase64String(track.GetProperty("data").GetString()!);
            if (!string.Equals(Convert.ToHexString(SHA256.HashData(audio)), track.GetProperty("sha256").GetString(), StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("A music file failed its integrity check.");
            if (metadata.TryGetProperty("size", out var size) && size.GetInt64() != audio.LongLength)
                throw new InvalidDataException("A music file has the wrong size.");
            total += audio.LongLength; tracks++;
            if (total > 256L * 1024 * 1024) throw new InvalidDataException("Music exceeds the supported backup size.");
        }
        return new(entries, tracks, total, Convert.ToHexString(SHA256.HashData(bytes.Span)).ToLowerInvariant());
    }

    public static (string Path, LegacyProfileSummary Summary) Stage(string source, string directory)
    {
        if (new FileInfo(source).Length > MaximumArchiveBytes) throw new InvalidDataException("Profile backup exceeds the supported size.");
        byte[] bytes = File.ReadAllBytes(source);
        var summary = Inspect(bytes);
        Directory.CreateDirectory(directory);
        string destination = Path.Combine(directory, summary.Sha256 + ".json");
        if (File.Exists(destination))
        {
            if (!File.ReadAllBytes(destination).AsSpan().SequenceEqual(bytes)) throw new InvalidDataException("Stored backup changed; it has been preserved.");
            return (destination, summary);
        }
        string temporary = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            { file.Write(bytes); file.Flush(flushToDisk: true); }
            File.Move(temporary, destination);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
        return (destination, summary);
    }
}

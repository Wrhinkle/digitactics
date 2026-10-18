using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed record PlayerProgress(string Name, long? Bits, long? Games, long? Wins, long? TopFour);

public sealed class ProfileImport
{
    internal byte[] Bytes { get; }
    public LegacyProfileSummary Summary { get; }
    public IReadOnlyDictionary<string, string> Storage { get; }
    public PlayerProgress Progress => NativeProfileStore.ReadProgress(Storage);
    public ProfileImport(string path)
    {
        if (new FileInfo(path).Length > LegacyProfileArchive.MaximumArchiveBytes) throw new InvalidDataException("Profile backup exceeds the supported size.");
        Bytes = File.ReadAllBytes(path); Summary = LegacyProfileArchive.Inspect(Bytes);
        using var doc = JsonDocument.Parse(Bytes);
        Storage = new ReadOnlyDictionary<string, string>(doc.RootElement.GetProperty("storage").EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.GetString()!, StringComparer.Ordinal));
    }
}

/// <summary>First-import transaction. The preserved archive is durable before the native profile
/// becomes visible. Existing native profiles are never merged or replaced implicitly.</summary>
public sealed partial class NativeProfileStore
{
    private readonly string directory, path;
    private string? loadedText;
    private bool readOnly;
    private string? generation;
    private bool freshReset;
    public string DataDirectory => generation is null ? directory : Path.Combine(directory, "profiles", generation);
    public bool CanImport => !Exists || !readOnly && freshReset && Storage.Count == 0;
    public bool Exists => File.Exists(path);
    public string? Error { get; private set; }
    public IReadOnlyDictionary<string, string> Storage { get; private set; } = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
    public PlayerProgress Progress => ReadProgress(Storage);
    public NativeProfileStore(string directory)
    {
        this.directory = Path.GetFullPath(directory); path = Path.Combine(this.directory, "profile.json");
        try
        {
            if (!Exists) return;
            loadedText = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(loadedText);
            if (doc.RootElement.GetProperty("version").GetInt32() != 1) throw new InvalidDataException("Unsupported native profile version.");
            if (doc.RootElement.TryGetProperty("generation", out var savedGeneration))
            {
                string? value = savedGeneration.GetString();
                if (value is null || !Guid.TryParseExact(value, "N", out _)) throw new InvalidDataException("Invalid profile generation.");
                generation = value;
            }
            freshReset = doc.RootElement.TryGetProperty("freshReset", out var reset) && reset.GetBoolean();
            Storage = new ReadOnlyDictionary<string, string>(doc.RootElement.GetProperty("storage").EnumerateObject()
                .ToDictionary(p => p.Name, p => p.Value.GetString() ?? throw new JsonException("Saved values must be strings."), StringComparer.Ordinal));
        }
        catch (Exception error) when (error is IOException or InvalidDataException or UnauthorizedAccessException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException or FormatException)
        { readOnly = true; Error = "Your profile could not be loaded and has been preserved: " + error.Message; }
    }
    public void Import(ProfileImport candidate)
    {
        Directory.CreateDirectory(directory);
        using var guard = new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        string? current = Exists ? File.ReadAllText(path) : null;
        if (!CanImport || current != loadedText) throw new IOException("A native profile already exists or changed. It has not been replaced.");
        var staged = LegacyProfileArchive.Stage(candidate.Bytes, Path.Combine(directory, "imports"));
        if (staged.Summary != candidate.Summary) throw new InvalidDataException("The profile backup changed after preview.");
        var document = new JsonObject { ["version"] = 1, ["sourceSha256"] = candidate.Summary.Sha256, ["storage"] = JsonSerializer.SerializeToNode(candidate.Storage) };
        if (generation is not null) document["generation"] = generation;
        byte[] data = JsonSerializer.SerializeToUtf8Bytes(document, new JsonSerializerOptions { WriteIndented = true });
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)) { file.Write(data); file.Flush(true); }
            if (current is null) File.Move(temporary, path); else File.Replace(temporary, path, path + ".bak");
            Storage = candidate.Storage; loadedText = System.Text.Encoding.UTF8.GetString(data); freshReset = false; Error = null;
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
    public bool UpdateStorage(IReadOnlyDictionary<string, string> changes)
    {
        if (readOnly) return false;
        string? temporary = null;
        try
        {
            Directory.CreateDirectory(directory);
            using var guard = new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            string? current = Exists ? File.ReadAllText(path) : null;
            if (current != loadedText) { readOnly = true; Error = "Your profile changed in another app instance. Restart before saving more progress."; return false; }
            var next = current is null ? new JsonObject { ["version"] = 1, ["storage"] = new JsonObject() } : JsonNode.Parse(current)!.AsObject();
            var values = next["storage"]!.AsObject();
            if (changes.Count > 0) next.Remove("freshReset");
            var updated = new Dictionary<string, string>(Storage, StringComparer.Ordinal);
            foreach (var (key, value) in changes)
            {
                if (!(key.StartsWith("dt.", StringComparison.Ordinal) || key.StartsWith("dt-", StringComparison.Ordinal)) || value is null)
                    throw new InvalidDataException("Invalid profile storage entry.");
                values[key] = value; updated[key] = value;
            }
            string json = next.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            { file.Write(System.Text.Encoding.UTF8.GetBytes(json)); file.Flush(true); }
            if (current is null) File.Move(temporary, path); else File.Replace(temporary, path, path + ".bak");
            temporary = null; loadedText = json; Storage = new ReadOnlyDictionary<string, string>(updated); if (changes.Count > 0) freshReset = false; Error = null; return true;
        }
        catch (Exception error) when (error is IOException or InvalidDataException or UnauthorizedAccessException or JsonException)
        { Error = "Progress could not be saved: " + error.Message; return false; }
        finally
        {
            if (temporary is not null) try { File.Delete(temporary); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }
    public MusicLibrary OpenMusicLibrary()
    {
        var library = new MusicLibrary(Path.Combine(DataDirectory, "music"));
        if (loadedText is null || readOnly) return library;
        using var document = JsonDocument.Parse(loadedText);
        if (!document.RootElement.TryGetProperty("sourceSha256", out var source)) return library;
        string? digest = source.GetString();
        if (digest is not { Length: 64 } || !digest.All(Uri.IsHexDigit)) throw new InvalidDataException("Invalid imported profile archive reference.");
        digest = digest.ToLowerInvariant();
        library.ImportLegacy(Path.Combine(directory, "imports", digest + ".json"), digest);
        return library;
    }
    public static PlayerProgress ReadProgress(IReadOnlyDictionary<string, string> storage)
    {
        long? Number(string key, string? parent, string field)
        {
            if (!storage.TryGetValue(key, out string? value)) return 0;
            try
            {
                using var doc = JsonDocument.Parse(value); var node = doc.RootElement;
                if (parent is not null) { if (!node.TryGetProperty(parent, out node)) return 0; }
                if (!node.TryGetProperty(field, out node)) return 0;
                return node.TryGetInt64(out long n) && n >= 0 ? n : null;
            }
            catch (Exception error) when (error is JsonException or InvalidOperationException) { return null; }
        }
        string name = storage.GetValueOrDefault("dt.name", "You").Trim();
        if (string.IsNullOrEmpty(name)) name = "You";
        name = new string(name.Where(c => !char.IsControl(c)).Take(32).ToArray());
        if (name.Length == 0) name = "You";
        return new(name, Number("dt.cosmetics.v1", null, "bits"), Number("dt.client.v1", "totals", "games"),
            Number("dt.client.v1", "totals", "wins"), Number("dt.client.v1", "totals", "top4"));
    }
}

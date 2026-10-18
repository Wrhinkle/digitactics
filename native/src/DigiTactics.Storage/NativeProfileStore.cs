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
public sealed class NativeProfileStore
{
    private readonly string directory, path;
    private string? loadedText;
    private bool readOnly;
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
        if (Exists) throw new IOException("A native profile already exists. It has not been replaced.");
        var staged = LegacyProfileArchive.Stage(candidate.Bytes, Path.Combine(directory, "imports"));
        if (staged.Summary != candidate.Summary) throw new InvalidDataException("The profile backup changed after preview.");
        byte[] data = JsonSerializer.SerializeToUtf8Bytes(new { version = 1, sourceSha256 = candidate.Summary.Sha256, storage = candidate.Storage }, new JsonSerializerOptions { WriteIndented = true });
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)) { file.Write(data); file.Flush(true); }
            File.Move(temporary, path);
            Storage = candidate.Storage; loadedText = System.Text.Encoding.UTF8.GetString(data); Error = null;
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
            temporary = null; loadedText = json; Storage = new ReadOnlyDictionary<string, string>(updated); Error = null; return true;
        }
        catch (Exception error) when (error is IOException or InvalidDataException or UnauthorizedAccessException or JsonException)
        { Error = "Progress could not be saved: " + error.Message; return false; }
        finally
        {
            if (temporary is not null) try { File.Delete(temporary); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
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

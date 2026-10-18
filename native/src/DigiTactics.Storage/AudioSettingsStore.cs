using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed record AudioSettings(float Music = .6f, float Effects = .8f, bool Muted = false, bool MusicOff = false, float Board = .8f, bool MuteOthers = false);

/// <summary>Versioned native preferences. Atomic replacement retains the previous file;
/// unreadable/newer documents and changes from another running instance are never overwritten.</summary>
public sealed class AudioSettingsStore
{
    private readonly string path;
    private JsonObject document = new() { ["version"] = 1 };
    private string? loadedText;
    public AudioSettings Settings { get; private set; } = new();
    public string? Error { get; private set; }
    public bool ReadOnly { get; private set; }
    public AudioSettingsStore(string path)
    {
        this.path = Path.GetFullPath(path);
        try
        {
            if (!File.Exists(this.path)) return;
            loadedText = File.ReadAllText(this.path);
            document = JsonNode.Parse(loadedText) as JsonObject ?? throw new JsonException("Settings must be an object.");
            if (document["version"]?.GetValue<int>() != 1) throw new JsonException("Unsupported settings version.");
            var audio = document["audio"] as JsonObject;
            if (document["audio"] is not null && audio is null) throw new JsonException("Invalid audio settings.");
            Settings = new(audio?["music"]?.GetValue<float>() ?? .6f, audio?["sfx"]?.GetValue<float>() ?? .8f,
                audio?["muted"]?.GetValue<bool>() ?? false, audio?["musicOff"]?.GetValue<bool>() ?? false,
                audio?["board"]?.GetValue<float>() ?? .8f, audio?["muteOthers"]?.GetValue<bool>() ?? false);
            Validate(Settings);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or InvalidOperationException or FormatException)
        { Settings = new(); ReadOnly = true; Error = "Settings could not be loaded. The original file has been preserved: " + error.Message; }
    }
    private static void Validate(AudioSettings settings)
    {
        if (!float.IsFinite(settings.Music) || !float.IsFinite(settings.Effects) || !float.IsFinite(settings.Board) || settings.Music is < 0 or > 1 || settings.Effects is < 0 or > 1 || settings.Board is < 0 or > 1)
            throw new JsonException("Audio volume must be between 0 and 1.");
    }
    public bool Save(AudioSettings settings)
    {
        if (ReadOnly) return false;
        string? temporary = null;
        try
        {
            Validate(settings);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var guard = new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            string? current = File.Exists(path) ? File.ReadAllText(path) : null;
            if (current != loadedText) { ReadOnly = true; Error = "Settings changed in another app instance. Restart to load them; your saved file was preserved."; return false; }
            var next = (JsonObject)document.DeepClone();
            var audio = next["audio"] as JsonObject ?? new JsonObject();
            if (next["audio"] is null) next["audio"] = audio;
            audio["music"] = settings.Music; audio["sfx"] = settings.Effects;
            audio["muted"] = settings.Muted; audio["musicOff"] = settings.MusicOff;
            audio["board"] = settings.Board; audio["muteOthers"] = settings.MuteOthers;
            string json = next.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n";
            temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(json); file.Write(bytes); file.Flush(flushToDisk: true);
            }
            if (current is null) File.Move(temporary, path); else File.Replace(temporary, path, path + ".bak");
            temporary = null; loadedText = json; document = next; Settings = settings; Error = null; return true;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException)
        { Error = "Settings could not be saved: " + error.Message; return false; }
        finally
        {
            if (temporary is not null) try { File.Delete(temporary); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }
}

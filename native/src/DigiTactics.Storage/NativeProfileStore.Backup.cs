using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed partial class NativeProfileStore
{
    public static bool ConfirmsReset(string? text) => string.Equals(text?.Trim(), "RESET", StringComparison.OrdinalIgnoreCase);

    /// <summary>A single durable profile replacement switches progress, settings and music together.
    /// Existing migration archives and filesystem backups remain recovery artifacts, never active data.</summary>
    public bool Reset(string? confirmation)
    {
        if (!ConfirmsReset(confirmation) || readOnly) return false;
        string? temporary = null;
        try
        {
            Directory.CreateDirectory(directory);
            using var guard = new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            string? current = Exists ? File.ReadAllText(path) : null;
            if (current != loadedText) throw new IOException("Your profile changed in another app instance. Restart before resetting it.");
            string nextGeneration = Guid.NewGuid().ToString("N");
            string json = new JsonObject { ["version"] = 1, ["generation"] = nextGeneration, ["freshReset"] = true, ["storage"] = new JsonObject() }.ToJsonString();
            temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            { file.Write(System.Text.Encoding.UTF8.GetBytes(json)); file.Flush(true); }
            if (current is null) File.Move(temporary, path); else File.Replace(temporary, path, path + ".bak");
            temporary = null; loadedText = json; generation = nextGeneration; freshReset = true;
            Storage = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()); Error = null; return true;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        { Error = "Your save was not reset: " + error.Message; return false; }
        finally { if (temporary is not null) try { File.Delete(temporary); } catch (IOException) { } catch (UnauthorizedAccessException) { } }
    }

    /// <summary>Creates the original portable v1 archive. A canceled picker (null path) has no side effects.
    /// Source snapshots are rechecked before publishing; the destination is replaced atomically.</summary>
    public LegacyProfileSummary? ExportBackup(string? destination, AudioSettings? currentAudio = null)
    {
        if (destination is null) return null;
        if (readOnly) throw new InvalidDataException(Error ?? "Your profile could not be read.");
        destination = Path.GetFullPath(destination);
        if (destination.Equals(directory, StringComparison.OrdinalIgnoreCase) || destination.StartsWith(Path.TrimEndingDirectorySeparator(directory) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new IOException("Choose a backup location outside the game's profile folder.");
        string? profileBefore = Exists ? File.ReadAllText(path) : null;
        if (profileBefore != loadedText) throw new IOException("Your save changed. Restart before exporting it.");
        var values = new Dictionary<string, string>(Storage, StringComparer.Ordinal);
        string dataDirectory = DataDirectory;
        var library = OpenMusicLibrary();
        if (library.Error is not null) throw new InvalidDataException(library.Error);
        string libraryPath = Path.Combine(dataDirectory, "music", "library.json"), settingsPath = Path.Combine(dataDirectory, "audio-settings.json");
        static string? Read(string file) => File.Exists(file) ? File.ReadAllText(file) : null;
        string? musicBefore = Read(libraryPath), audioBefore = Read(settingsPath);
        library.VerifySnapshot();
        var settings = new AudioSettingsStore(settingsPath);
        values["dt.audio.v1"] = settings.ExportLegacy(currentAudio, values.GetValueOrDefault("dt.audio.v1"));
        if (library.Tracks.Sum(track => track.Size) > 256L * 1024 * 1024) throw new InvalidDataException("Your music exceeds the 256 MB backup limit. Keep a separate copy before reducing your music library.");
        var music = new JsonArray();
        foreach (var track in library.Tracks)
        {
            byte[] bytes = library.ReadAudio(track.Id);
            music.Add(new JsonObject { ["metadata"] = track.Metadata.DeepClone(), ["blob"] = track.Blob.DeepClone(), ["data"] = Convert.ToBase64String(bytes), ["sha256"] = track.Sha256 });
        }
        var archive = new JsonObject { ["format"] = "digitactics-profile", ["version"] = 1, ["exportedAt"] = DateTimeOffset.UtcNow.ToString("O"), ["storage"] = JsonSerializer.SerializeToNode(values), ["music"] = music };
        byte[] serialized = JsonSerializer.SerializeToUtf8Bytes(archive);
        var summary = LegacyProfileArchive.Inspect(serialized);
        string temporary = destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var file = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)) { file.Write(serialized); file.Flush(true); }
            if (Read(path) != profileBefore || Read(libraryPath) != musicBefore || Read(settingsPath) != audioBefore)
                throw new IOException("Your save changed during backup. Try again while the game is idle.");
            if (File.Exists(destination)) File.Replace(temporary, destination, null); else File.Move(temporary, destination);
            return summary;
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}

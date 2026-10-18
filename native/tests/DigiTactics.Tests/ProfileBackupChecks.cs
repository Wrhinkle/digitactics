using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Storage;

internal static class ProfileBackupChecks
{
    public static int Run(string root)
    {
        string directory = Path.Combine(Path.GetTempPath(), "digitactics-backup-check-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory); int checks = 0;
        void Check(bool ok, string message) { checks++; if (!ok) throw new InvalidOperationException("Profile backup: " + message); }
        void Reject(Action action, string message)
        {
            try { action(); } catch (Exception error) when (error is IOException or InvalidDataException or JsonException) { checks++; return; }
            throw new InvalidOperationException("Profile backup accepted " + message);
        }
        try
        {
            string sourceDirectory = Path.Combine(directory, "source"), destination = Path.Combine(directory, "export.json");
            var profile = new NativeProfileStore(sourceDirectory);
            Check(profile.ExportBackup(null) is null && !Directory.Exists(sourceDirectory), "canceled export performs no writes");
            foreach (string? text in new string?[] { null, "", " ", "RESE", "RESET!", "RESET everything" })
                Check(!profile.Reset(text) && !Directory.Exists(sourceDirectory), "unconfirmed reset leaves filesystem untouched");
            var candidate = new ProfileImport(Path.Combine(root, "tests/fixtures/legacy-profile.json")); profile.Import(candidate);
            Check(profile.UpdateStorage(new Dictionary<string, string> { ["dt.future"] = "{\"unknown\":[1,2,3]}", ["dt.name"] = "Backup Tamer", ["dt.client.v1"] = "{\"prefs\":{\"motion\":false,\"uiSfx\":false,\"social\":false},\"totals\":{\"games\":12}}" }), "current progress persisted");
            var settings = new AudioSettingsStore(Path.Combine(profile.DataDirectory, "audio-settings.json"));
            Check(settings.ImportLegacy(profile.Storage), "legacy settings migrated");
            var latest = new AudioSettings(.2f, .3f, true, true, .4f, true, "own"); Check(settings.Save(latest), "native settings changed after migration");
            var library = profile.OpenMusicLibrary();
            Check(library.Tracks.Count == 1 && library.Error is null, "imported music available");
            string trackId = library.Tracks[0].Id; byte[] trackBytes = library.ReadAudio(trackId);
            Check(library.SetUse(trackId, "combat"), "current music metadata changed after migration");
            string rawProfile = File.ReadAllText(Path.Combine(sourceDirectory, "profile.json"));
            string rawSettings = File.ReadAllText(Path.Combine(sourceDirectory, "audio-settings.json"));
            string rawMusic = File.ReadAllText(Path.Combine(sourceDirectory, "music/library.json"));
            var summary = profile.ExportBackup(destination)!;
            Check(summary.MusicTracks == 1 && summary.MusicBytes == trackBytes.Length, "archive includes actual current audio");
            Check(File.ReadAllText(Path.Combine(sourceDirectory, "profile.json")) == rawProfile && File.ReadAllText(Path.Combine(sourceDirectory, "audio-settings.json")) == rawSettings && File.ReadAllText(Path.Combine(sourceDirectory, "music/library.json")) == rawMusic, "export never changes current source data");
            var exported = new ProfileImport(destination);
            Check(exported.Storage.Where(x => x.Key != "dt.audio.v1").All(x => profile.Storage[x.Key] == x.Value), "all raw non-audio game storage preserved exactly");
            Check(exported.Storage["dt.future"] == profile.Storage["dt.future"] && exported.Storage["dt.client.v1"] == profile.Storage["dt.client.v1"], "unknown future values and interface prefs retained");
            string restoredDirectory = Path.Combine(directory, "restored"); var restored = new NativeProfileStore(restoredDirectory); restored.Import(exported);
            var restoredAudio = new AudioSettingsStore(Path.Combine(restored.DataDirectory, "audio-settings.json"));
            Check(restoredAudio.ImportLegacy(restored.Storage) && restoredAudio.Settings == latest, "backup restores current native mixer settings");
            var restoredMusic = restored.OpenMusicLibrary();
            Check(restoredMusic.Tracks.Single().Use == "combat" && restoredMusic.ReadAudio(trackId).SequenceEqual(trackBytes), "backup restores current metadata and exact music bytes");
            Check(restored.Progress == profile.Progress && restored.Storage.Count == exported.Storage.Count, "backup restores name and progression");
            var live = latest with { Music = .91f, Muted = false };
            profile.ExportBackup(destination, live); var liveBackup = new ProfileImport(destination);
            Check(JsonNode.Parse(liveBackup.Storage["dt.audio.v1"])!["music"]!.GetValue<float>() == .91f, "pending slider value captured before debounce flush");
            byte[] previousExport = File.ReadAllBytes(destination);
            using (var lease = new FileStream(destination, FileMode.Open, FileAccess.Read, FileShare.Read))
                Reject(() => profile.ExportBackup(destination), "locked destination overwrite");
            Check(File.ReadAllBytes(destination).SequenceEqual(previousExport), "failed export preserves existing destination");
            Check(Directory.GetFiles(directory, "*.tmp").Length == 0, "failed export removes staged temporary data");
            Reject(() => profile.ExportBackup(Path.Combine(sourceDirectory, "profile.json")), "active profile as export destination");
            Check(File.ReadAllText(Path.Combine(sourceDirectory, "profile.json")) == rawProfile, "internal export path cannot overwrite active save");
            var stale = new NativeProfileStore(sourceDirectory);
            Check(profile.UpdateStorage(new Dictionary<string, string> { ["dt.name"] = "Newest Tamer" }), "independent edit for conflict fixture");
            string beforeReset = File.ReadAllText(Path.Combine(sourceDirectory, "profile.json"));
            Check(!stale.Reset("RESET") && File.ReadAllText(Path.Combine(sourceDirectory, "profile.json")) == beforeReset, "stale reset refuses to erase newer save");
            Reject(() => stale.ExportBackup(destination), "stale snapshot");
            using (var guard = new FileStream(Path.Combine(sourceDirectory, "profile.json.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                Check(!profile.Reset("RESET") && profile.Storage.Count > 0 && profile.DataDirectory == sourceDirectory, "locked reset cannot partially clear progress/settings/music");
            Check(File.ReadAllText(Path.Combine(sourceDirectory, "profile.json")) == beforeReset, "failed reset preserves exact profile bytes");
            var priorInstance = new NativeProfileStore(sourceDirectory);
            Check(NativeProfileStore.ConfirmsReset(" \treset\n") && profile.Reset(" \treset\n"), "trimmed case-insensitive confirmation matches source");
            string resetDirectory = profile.DataDirectory;
            Check(profile.Storage.Count == 0 && profile.Progress == new PlayerProgress("You", 0, 0, 0, 0), "reset clears all saved values and counters");
            Check(resetDirectory != sourceDirectory && resetDirectory.StartsWith(sourceDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase), "reset selects an isolated descendant generation");
            Check(new AudioSettingsStore(Path.Combine(resetDirectory, "audio-settings.json")).Settings == new AudioSettings(), "reset returns every mixer setting to defaults");
            Check(profile.OpenMusicLibrary().Tracks.Count == 0, "reset has no old music or implicit migration");
            Check(profile.CanImport && new NativeProfileStore(sourceDirectory).CanImport, "fresh reset can restore explicit backup after restart");
            Check(new NativeProfileStore(sourceDirectory).DataDirectory == resetDirectory, "atomic generation selection survives restart");
            Check(!priorInstance.UpdateStorage(new Dictionary<string, string> { ["dt.name"] = "Stale rewrite" }), "stale instance cannot revive pre-reset progress");
            Check(File.Exists(Path.Combine(sourceDirectory, "imports", candidate.Summary.Sha256 + ".json")), "original migration archive preserved");
            Check(File.ReadAllBytes(destination).SequenceEqual(previousExport), "reset cannot delete chosen external backup");
            string afterReset = Path.Combine(directory, "empty.json"); profile.ExportBackup(afterReset);
            Check(new ProfileImport(afterReset).Summary.MusicTracks == 0 && !new ProfileImport(afterReset).Storage.ContainsKey("dt.future"), "backup after reset excludes inactive prior data");
            profile.Import(liveBackup);
            Check(!profile.CanImport && profile.Progress.Name == "Backup Tamer" && profile.DataDirectory == resetDirectory, "explicit restore after reset preserves new generation");
            Check(profile.OpenMusicLibrary().ReadAudio(trackId).SequenceEqual(trackBytes), "restore after reset imports music into active generation");
            Reject(() => profile.Import(candidate), "replacement of populated existing profile");
            string corruptDirectory = Path.Combine(directory, "corrupt"); Directory.CreateDirectory(corruptDirectory);
            foreach (string raw in new[] { "{", "{\"version\":2}", "{\"version\":1,\"generation\":\"../../outside\",\"storage\":{}}" })
            {
                File.WriteAllText(Path.Combine(corruptDirectory, "profile.json"), raw); var corrupt = new NativeProfileStore(corruptDirectory);
                Check(corrupt.Error is not null && !corrupt.Reset("RESET") && File.ReadAllText(Path.Combine(corruptDirectory, "profile.json")) == raw, "unreadable/newer/invalid-generation profile preserved");
                Reject(() => corrupt.ExportBackup(destination), "unreadable source");
            }
            var corruptedLibrary = profile.OpenMusicLibrary(); var item = corruptedLibrary.Tracks.Single();
            File.WriteAllBytes(Path.Combine(profile.DataDirectory, "music", item.Sha256 + ".audio"), [5, 6]);
            Reject(() => profile.ExportBackup(destination), "corrupt music payload");
            Check(File.ReadAllBytes(destination).SequenceEqual(previousExport), "corrupt source cannot replace a good backup");
        }
        finally
        {
            string resolved = Path.GetFullPath(directory);
            if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("digitactics-backup-check-", StringComparison.Ordinal)) throw new InvalidOperationException("Unsafe backup test cleanup.");
            Directory.Delete(resolved, true);
        }
        Console.WriteLine($"Profile backup/reset: {checks} round-trip, cancellation, atomic replacement, generation and preservation checks passed."); return checks;
    }
}

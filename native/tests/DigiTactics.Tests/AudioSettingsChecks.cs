using System.Text.Json.Nodes;
using DigiTactics.Storage;

internal static class AudioSettingsChecks
{
    public static int Run()
    {
        string directory = Path.Combine(Path.GetTempPath(), "digitactics-settings-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        int checks = 0;
        void Check(bool ok, string message) { checks++; if (!ok) throw new InvalidOperationException("Settings: " + message); }
        try
        {
            string path = Path.Combine(directory, "audio.json");
            var first = new AudioSettingsStore(path);
            Check(first.Settings == new AudioSettings() && !File.Exists(path), "defaults do not write files");
            var edit = new AudioSettings(.2f, .4f, true, true, .3f, true, "own");
            Check(first.Save(edit), "first save");
            string saved = File.ReadAllText(path);
            Check(new AudioSettingsStore(path).Settings == edit, "restart retains all values");
            Check(first.Save(edit with { Muted = false }), "replace");
            Check(File.ReadAllText(path + ".bak") == saved, "backup is previous file");
            var slow = new AudioSettingsStore(path); var fast = new AudioSettingsStore(path);
            Check(fast.Save(new()), "second instance save");
            string latest = File.ReadAllText(path);
            Check(!slow.Save(edit) && slow.ReadOnly && File.ReadAllText(path) == latest, "concurrent instance cannot overwrite newer data");
            Check(!fast.Save(edit with { Music = float.NaN }) && File.ReadAllText(path) == latest, "non-finite volume rejected");
            Check(!fast.Save(edit with { Source = "invalid" }) && File.ReadAllText(path) == latest, "invalid source rejected without overwriting preferences");
            Check(!fast.Save(edit with { Effects = 2 }) && File.ReadAllText(path) == latest, "out of range volume rejected");
            Check(!fast.Save(edit with { Board = float.NaN }) && File.ReadAllText(path) == latest, "non-finite soundboard volume rejected");
            foreach (string malformed in new[] { "{", "null", "[]", "{\"version\":2}", "{\"version\":1,\"audio\":{\"muted\":\"yes\"}}", "{\"version\":1,\"audio\":{\"music\":-1}}" })
            {
                File.WriteAllText(path, malformed); var bad = new AudioSettingsStore(path);
                Check(bad.ReadOnly && bad.Error is not null && !bad.Save(edit) && File.ReadAllText(path) == malformed, "invalid/newer file preserved");
            }
            File.WriteAllText(path, "{\"version\":1,\"future\":{\"keep\":3},\"audio\":{\"board\":0.3,\"source\":\"own\"}}");
            var extended = new AudioSettingsStore(path); Check(extended.Save(edit), "extended document saved");
            var doc = JsonNode.Parse(File.ReadAllText(path))!;
            Check(doc["future"]!["keep"]!.GetValue<int>() == 3 && doc["audio"]!["board"]!.GetValue<float>() == .3f && doc["audio"]!["source"]!.GetValue<string>() == "own", "unknown fields retained");
            using (var locked = new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                Check(!extended.Save(new()) && extended.Error is not null, "write contention is reported without crashing");
            Check(extended.Save(new()) && extended.Error is null, "retry after contention");
            Check(Directory.GetFiles(directory, "*.tmp").Length == 0, "temporary files cleaned");
            string importedPath = Path.Combine(directory, "imported.json");
            var migrated = new AudioSettingsStore(importedPath);
            var legacy = new Dictionary<string, string> { ["dt.audio.v1"] = "{\"music\":0.1,\"sfx\":0.2,\"board\":0.3,\"muted\":true,\"musicOff\":true,\"muteOthers\":true,\"source\":\"own\",\"future\":{\"keep\":7}}" };
            string original = legacy["dt.audio.v1"];
            Check(migrated.ImportLegacy(legacy) && migrated.Settings == new AudioSettings(.1f, .2f, true, true, .3f, true, "own"), "all legacy audio controls imported");
            Check(legacy["dt.audio.v1"] == original && new AudioSettingsStore(importedPath).Settings == migrated.Settings, "source unchanged and imported values survive restart");
            Check(migrated.Save(migrated.Settings with { Music = .7f }), "native edits after import");
            string edited = File.ReadAllText(importedPath);
            var importedDoc = JsonNode.Parse(edited)!;
            Check(importedDoc["audio"]!["source"]!.GetValue<string>() == "own" && importedDoc["audio"]!["future"]!["keep"]!.GetValue<int>() == 7, "music source and future fields preserved");
            Check(!migrated.ImportLegacy(legacy) && !new AudioSettingsStore(importedPath).ImportLegacy(legacy) && File.ReadAllText(importedPath) == edited, "migration does not overwrite native preferences");
            foreach (string mode in new[] { "off", "effects", "all" })
            {
                var older = new AudioSettingsStore(Path.Combine(directory, "old-" + mode + ".json"));
                Check(older.ImportLegacy(new Dictionary<string,string> { ["dt.sound"] = mode }) && older.Settings == new AudioSettings(Muted: mode == "off", MusicOff: mode == "effects"), "old mode " + mode);
            }
            string retryPath = Path.Combine(directory, "retry-import.json");
            var retry = new AudioSettingsStore(retryPath);
            using (var guard = new FileStream(retryPath + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                Check(!retry.ImportLegacy(legacy) && !retry.HasSavedSettings && !File.Exists(retryPath) && retry.Settings == new AudioSettings(), "failed import leaves native settings untouched");
            Check(retry.ImportLegacy(legacy) && retry.Error is null, "import retries after lock release");
            foreach (string badAudio in new[] { "{", "[]", "{\"music\":2}", "{\"board\":\"loud\"}", "{\"muteOthers\":3}" })
            {
                string badPath = Path.Combine(directory, Guid.NewGuid() + ".json");
                var badImport = new AudioSettingsStore(badPath);
                Check(!badImport.ImportLegacy(new Dictionary<string,string> { ["dt.audio.v1"] = badAudio }) && badImport.Error is not null && !File.Exists(badPath), "invalid imported audio does not create native settings");
            }
            var empty = new AudioSettingsStore(Path.Combine(directory, "empty-import.json"));
            Check(!empty.ImportLegacy(new Dictionary<string,string>()) && !empty.HasSavedSettings && empty.Error is null, "unrelated profile does not force preferences");
            var partial = new AudioSettingsStore(Path.Combine(directory, "partial-import.json"));
            Check(partial.ImportLegacy(new Dictionary<string,string> { ["dt.audio.v1"] = "{\"music\":0.2}" }) && partial.Settings == new AudioSettings(Music: .2f), "partial backup keeps original defaults");
            var existingBad = new AudioSettingsStore(path);
            File.WriteAllText(path, "external edit");
            Check(!existingBad.Save(new()), "external edit detected");
            Check(!existingBad.ImportLegacy(legacy) && File.ReadAllText(path) == "external edit", "readonly settings cannot be overwritten by import");
        }
        finally
        {
            string resolved = Path.GetFullPath(directory);
            if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) ||
                !Path.GetFileName(resolved).StartsWith("digitactics-settings-test-", StringComparison.Ordinal))
                throw new InvalidOperationException("Refusing cleanup outside the generated settings test directory.");
            Directory.Delete(resolved, recursive: true);
        }
        Console.WriteLine($"Audio settings: {checks} persistence and preservation checks passed.");
        return checks;
    }
}

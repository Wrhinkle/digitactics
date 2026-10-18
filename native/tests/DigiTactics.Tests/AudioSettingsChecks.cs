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
            var edit = new AudioSettings(.2f, .4f, true, true, .3f, true);
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

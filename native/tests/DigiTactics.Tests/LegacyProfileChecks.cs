using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Storage;

internal static class LegacyProfileChecks
{
    public static int Run(string root)
    {
        string fixture = Path.Combine(root, "tests/fixtures/legacy-profile.json");
        byte[] original = File.ReadAllBytes(fixture);
        var summary = LegacyProfileArchive.Inspect(original);
        int checks = 0;
        void Check(bool ok, string message) { checks++; if (!ok) throw new InvalidOperationException("Legacy profile: " + message); }
        Check(summary.StorageEntries == 5 && summary.MusicTracks == 1 && summary.MusicBytes == 5, "browser fixture summary");
        void Reject(string json)
        {
            try { LegacyProfileArchive.Inspect(Encoding.UTF8.GetBytes(json)); }
            catch (Exception error) when (error is InvalidDataException or JsonException or FormatException or InvalidOperationException or KeyNotFoundException)
            { checks++; return; }
            throw new InvalidOperationException("Invalid backup accepted.");
        }
        var doc = JsonNode.Parse(original)!;
        var bad = doc.DeepClone(); bad["version"] = 2; Reject(bad.ToJsonString());
        bad = doc.DeepClone(); bad["format"] = "other"; Reject(bad.ToJsonString());
        bad = doc.DeepClone(); bad["music"]![0]!["data"] = "AAEC/v4="; Reject(bad.ToJsonString());
        bad = doc.DeepClone(); bad["music"]![0]!["data"] = "invalid-base64"; Reject(bad.ToJsonString());
        bad = doc.DeepClone(); bad["music"]![0]!["metadata"]!["size"] = 100; Reject(bad.ToJsonString());
        bad = doc.DeepClone(); bad["music"]!.AsArray().Add(bad["music"]![0]!.DeepClone()); Reject(bad.ToJsonString());
        bad = doc.DeepClone(); bad["storage"]!["unrelated"] = "excluded"; Reject(bad.ToJsonString());
        bad = doc.DeepClone(); bad["storage"]!["dt.future"] = 5; Reject(bad.ToJsonString());
        Reject("{\"format\":\"digitactics-profile\",\"version\":1,\"version\":2,\"storage\":{},\"music\":[]}");
        Reject("{");
        string directory = Path.Combine(Path.GetTempPath(), "digitactics-profile-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string sentinel = Path.Combine(directory, "audio-settings.json"); File.WriteAllText(sentinel, "existing native settings");
            var staged = LegacyProfileArchive.Stage(fixture, directory);
            Check(Path.GetFileName(staged.Path) == summary.Sha256 + ".json", "content-addressed destination ignores music names");
            Check(File.ReadAllBytes(staged.Path).AsSpan().SequenceEqual(original), "exact backup bytes preserved");
            Check(LegacyProfileArchive.Stage(fixture, directory).Path == staged.Path, "idempotent restaging");
            Check(File.ReadAllText(sentinel) == "existing native settings", "native preferences untouched");
            Check(File.ReadAllBytes(fixture).AsSpan().SequenceEqual(original), "exported source untouched");
            File.WriteAllText(staged.Path, "modified backup");
            try { LegacyProfileArchive.Stage(fixture, directory); throw new Exception("Modified backup overwritten"); }
            catch (InvalidDataException) { Check(File.ReadAllText(staged.Path) == "modified backup", "conflicting backup preserved"); }
            Check(Directory.GetFiles(directory, "*.tmp").Length == 0, "no temporary files left");
        }
        finally
        {
            string resolved = Path.GetFullPath(directory);
            if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) ||
                !Path.GetFileName(resolved).StartsWith("digitactics-profile-test-", StringComparison.Ordinal))
                throw new InvalidOperationException("Refusing cleanup outside the generated profile test directory.");
            Directory.Delete(resolved, recursive: true);
        }
        Console.WriteLine($"Legacy profile staging: {checks} validation and preservation checks passed.");
        return checks;
    }
}

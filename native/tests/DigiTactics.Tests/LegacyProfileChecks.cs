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
            string nativeDirectory = Path.Combine(directory, "native");
            string candidatePath = Path.Combine(directory, "candidate.json");
            var importDocument = doc.DeepClone();
            importDocument["storage"]!["dt.name"] = "Saved Tamer";
            importDocument["storage"]!["dt.client.v1"] = "{\"totals\":{\"games\":17,\"wins\":3,\"top4\":9},\"unknown\":42}";
            byte[] candidateBytes = Encoding.UTF8.GetBytes(importDocument.ToJsonString());
            File.WriteAllBytes(candidatePath, candidateBytes);
            var candidate = new ProfileImport(candidatePath);
            Check(candidate.Progress == new PlayerProgress("Saved Tamer", 491, 17, 3, 9), "preview reads real saved progress");
            var native = new NativeProfileStore(nativeDirectory);
            var competing = new NativeProfileStore(nativeDirectory);
            Check(!native.Exists && !Directory.Exists(nativeDirectory), "new profile read does not write");
            File.WriteAllText(candidatePath, "changed after preview");
            native.Import(candidate);
            var reopened = new NativeProfileStore(nativeDirectory);
            Check(reopened.Error is null && reopened.Progress == candidate.Progress, "import survives restart");
            Check(candidate.Storage.All(pair => reopened.Storage[pair.Key] == pair.Value), "every raw save entry retained");
            Check(File.ReadAllBytes(Path.Combine(nativeDirectory, "imports", candidate.Summary.Sha256 + ".json")).AsSpan().SequenceEqual(candidateBytes), "preview bytes preserved despite source changes");
            Check(File.ReadAllText(candidatePath) == "changed after preview", "import never edits source file");
            string nativePath = Path.Combine(nativeDirectory, "profile.json"), imported = File.ReadAllText(nativePath);
            try { competing.Import(candidate); throw new Exception("Existing profile replaced"); }
            catch (IOException) { Check(File.ReadAllText(nativePath) == imported, "competing import cannot replace profile"); }
            foreach (string invalid in new[] { "{", "{\"version\":2}", "{\"version\":1,\"storage\":{\"dt.name\":null}}" })
            {
                File.WriteAllText(nativePath, invalid); var protectedProfile = new NativeProfileStore(nativeDirectory);
                Check(protectedProfile.Error is not null && protectedProfile.Exists, "damaged or future profile reported");
                try { protectedProfile.Import(candidate); throw new Exception("Damaged profile replaced"); }
                catch (IOException) { Check(File.ReadAllText(nativePath) == invalid, "damaged profile preserved"); }
            }
            Check(NativeProfileStore.ReadProgress(new Dictionary<string, string> { ["dt.cosmetics.v1"] = "corrupt" }).Bits is null, "corrupt progress is unavailable, not silently reset");
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

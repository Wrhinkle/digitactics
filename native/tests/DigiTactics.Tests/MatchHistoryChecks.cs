using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Presentation;
using DigiTactics.Rules;
using DigiTactics.Storage;

internal static class MatchHistoryChecks
{
    public static int Run(string root, ContentCatalog catalog)
    {
        int checks = 0;
        void Check(bool ok, string message) { checks++; if (!ok) throw new InvalidOperationException("Match history: " + message); }
        using var inputFile = File.OpenRead(Path.Combine(root, "tests/fixtures/match-history.json.gz"));
        using var gzip = new System.IO.Compression.GZipStream(inputFile, System.IO.Compression.CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        string saved = fixture.RootElement.GetProperty("initial").GetRawText();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        foreach (var test in fixture.RootElement.GetProperty("cases").EnumerateArray())
        {
            var input = test.GetProperty("input").Deserialize<HistoryMatch>(options)!;
            saved = MatchHistory.Append(saved, input);
            Check(JsonNode.DeepEquals(JsonNode.Parse(saved), JsonNode.Parse(test.GetProperty("expected").GetRawText())), "original source update " + input.Key);
            Check(MatchHistory.Append(saved, input) == saved, "duplicate match is idempotent");
        }
        string directory = Path.Combine(Path.GetTempPath(), "digitactics-history-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            foreach (var rules in catalog.Variants) foreach (bool duos in new[] { false, true })
            {
                string destination = Path.Combine(directory, rules.Id.Replace('/', '_'), duos ? "duos" : "solo");
                var profile = new NativeProfileStore(destination);
                profile.Import(new ProfileImport(Path.Combine(root, "tests/fixtures/legacy-profile.json")));
                var raw = profile.Storage.ToDictionary(p => p.Key, p => p.Value);
                var session = new MatchPlanningSession(rules, 143, Enumerable.Range(0, 8).Select(i => new MatchSeat("Tamer " + i, false)).ToArray(), duos);
                var recorder = new MatchHistoryRecorder(session, profile);
                Check(!recorder.Complete(1000), "unfinished match cannot be recorded");
                int rounds = 0;
                while (session.State.Players[0].Placement == 0 && rounds++ < 200)
                {
                    session.StartCombat(); recorder.Capture(); session.FinishCombat(); recorder.Observe();
                    if (session.State.Players[0].Placement == 0) session.Advance();
                }
                Check(recorder.Complete(2000), "placement saved after native match");
                Check(recorder.Complete(3000), "repeated completion");
                var reopened = new NativeProfileStore(destination);
                Check(reopened.Progress.Games == 1, "one game across restart");
                Check(raw.Where(p => p.Key != "dt.client.v1").All(p => reopened.Storage[p.Key] == p.Value), "other saved domains untouched");
                using var client = JsonDocument.Parse(reopened.Storage["dt.client.v1"]);
                Check(client.RootElement.GetProperty("matches")[0].GetProperty("at").GetInt64() == 2000, "result timestamp stable");
                Check(client.RootElement.GetProperty("matches")[0].GetProperty("units").GetArrayLength() > 0, "last living board retained");
                var stats = client.RootElement.GetProperty("matches")[0].GetProperty("stats");
                Check(stats.GetProperty("place").GetInt32() == session.State.Players[0].Placement, "reward placement survives restart");
                Check(stats.GetProperty("maxLevel").GetInt32() >= 1 && stats.GetProperty("maxStars").GetInt32() >= 1, "reward peaks survive restart");
                Check(File.Exists(Path.Combine(destination, "profile.json.bak")), "profile backup retained");
                var competing = new NativeProfileStore(destination);
                Check(reopened.UpdateStorage(new Dictionary<string, string> { ["dt.future"] = "new" }), "update raw entry");
                Check(!competing.UpdateStorage(new Dictionary<string, string> { ["dt.future"] = "stale" }), "stale writer rejected");
                Check(new NativeProfileStore(destination).Storage["dt.future"] == "new", "newer write preserved");
            }
        }
        finally
        {
            string resolved = Path.GetFullPath(directory);
            if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("digitactics-history-test-", StringComparison.Ordinal))
                throw new InvalidOperationException("Refusing cleanup outside the generated test directory.");
            Directory.Delete(resolved, true);
        }
        Console.WriteLine($"Match history: {checks} source parity and native persistence checks passed.");
        return checks;
    }
}

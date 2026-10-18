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
                Check(raw.Where(p => p.Key is not ("dt.client.v1" or "dt.cosmetics.v1" or "dt.shop.v1" or "dt.legendevo.v1")).All(p => reopened.Storage[p.Key] == p.Value), "other saved domains untouched");
                using var client = JsonDocument.Parse(reopened.Storage["dt.client.v1"]);
                Check(client.RootElement.GetProperty("matches")[0].GetProperty("at").GetInt64() == 2000, "result timestamp stable");
                Check(client.RootElement.GetProperty("matches")[0].GetProperty("units").GetArrayLength() > 0, "last living board retained");
                var stats = client.RootElement.GetProperty("matches")[0].GetProperty("stats");
                Check(stats.GetProperty("place").GetInt32() == session.State.Players[0].Placement, "reward placement survives restart");
                Check(stats.GetProperty("maxLevel").GetInt32() >= 1 && stats.GetProperty("maxStars").GetInt32() >= 1, "reward peaks survive restart");
                Check(recorder.Award is { Total: > 0 } && reopened.Progress.Bits == 491 + recorder.Award.Total, "Bits awarded once across restart");
                Check(client.RootElement.GetProperty("matches")[0].GetProperty("bits").GetInt32() == recorder.Award!.Total, "history Bits matches wallet award");
                using var legend = JsonDocument.Parse(reopened.Storage["dt.legendevo.v1"]);
                Check(legend.RootElement.GetProperty("games").GetArrayLength() == 1 && recorder.LegendAward is { Data: > 0 }, "legend awarded once with match");
                Check(File.Exists(Path.Combine(destination, "profile.json.bak")), "profile backup retained");
                var competing = new NativeProfileStore(destination);
                Check(reopened.UpdateStorage(new Dictionary<string, string> { ["dt.future"] = "new" }), "update raw entry");
                Check(!competing.UpdateStorage(new Dictionary<string, string> { ["dt.future"] = "stale" }), "stale writer rejected");
                Check(new NativeProfileStore(destination).Storage["dt.future"] == "new", "newer write preserved");
            }
            var finished = new MatchPlanningSession(catalog.Variants[0], 143, Enumerable.Range(0, 8).Select(i => new MatchSeat("Tamer " + i, false)).ToArray(), false);
            while (finished.State.Players[0].Placement == 0) { finished.StartCombat(); finished.FinishCombat(); if (finished.State.Players[0].Placement == 0) finished.Advance(); }
            string companionTarget = Path.Combine(directory, "companion");
            var companionProfile = new NativeProfileStore(companionTarget);
            const string companion = "mystery_daily_legend_4";
            Check(companionProfile.UpdateStorage(new Dictionary<string, string> { ["dt.cosmetics.v1"] = "{\"bits\":0,\"owned\":{\"legend\":[\"" + companion + "\"]},\"equipped\":{\"legend\":\"" + companion + "\"}}" }), "seed discovered companion loadout");
            var companionRecorder = new MatchHistoryRecorder(finished, companionProfile);
            Check(companionRecorder.Complete(2000) && companionRecorder.LegendAward?.Legend == companion, "match award targets equipped discovered companion");
            using var companionSaved = JsonDocument.Parse(new NativeProfileStore(companionTarget).Storage["dt.legendevo.v1"]);
            Check(companionSaved.RootElement.GetProperty("legends").EnumerateObject().Single().Name == companion, "companion progression persists without default legend fallback");
            foreach (string domain in new[] { "dt.client.v1", "dt.shop.v1", "dt.cosmetics.v1", "dt.legendevo.v1" })
            {
                string target = Path.Combine(directory, "invalid-" + domain);
                var profile = new NativeProfileStore(target);
                Check(profile.UpdateStorage(new Dictionary<string, string> { [domain] = "null" }), "seed malformed domain");
                string before = File.ReadAllText(Path.Combine(target, "profile.json"));
                var recorder = new MatchHistoryRecorder(finished, profile);
                Check(!recorder.Complete(2000) && recorder.Error is not null, "invalid domain refuses whole award");
                Check(File.ReadAllText(Path.Combine(target, "profile.json")) == before && recorder.Award is null, "no partial progress on invalid domain");
            }
            string lockedTarget = Path.Combine(directory, "locked");
            Directory.CreateDirectory(lockedTarget);
            var lockedProfile = new NativeProfileStore(lockedTarget);
            var retry = new MatchHistoryRecorder(finished, lockedProfile);
            using (var guard = new FileStream(Path.Combine(lockedTarget, "profile.json.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                Check(!retry.Complete(2000), "locked profile refuses entire payout");
                Check(!lockedProfile.Exists && lockedProfile.Storage.Count == 0 && retry.Award is null, "failed payout leaves no partial domains");
            }
            Check(retry.Complete(90000000), "retry succeeds after writer releases lock");
            using var paidClient = JsonDocument.Parse(lockedProfile.Storage["dt.client.v1"]);
            Check(paidClient.RootElement.GetProperty("matches")[0].GetProperty("at").GetInt64() == 2000, "retry keeps initial timestamp");
            using var paidShop = JsonDocument.Parse(lockedProfile.Storage["dt.shop.v1"]);
            Check(paidShop.RootElement.GetProperty("lastDay").GetString() == DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(2000).LocalDateTime).ToString("yyyy-MM-dd"), "retry keeps initial reward day");
            string completed = File.ReadAllText(Path.Combine(lockedTarget, "profile.json"));
            var duplicate = new MatchHistoryRecorder(finished, new NativeProfileStore(lockedTarget));
            Check(duplicate.Complete(90000000) && duplicate.Award?.Total == retry.Award?.Total, "reopened match returns cached award");
            Check(File.ReadAllText(Path.Combine(lockedTarget, "profile.json")) == completed, "reopened match does not write or pay again");
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

using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Presentation;
using DigiTactics.Rules;
using DigiTactics.Storage;

internal static class MatchRewardChecks
{
    public static int Run(string root, ContentCatalog catalog)
    {
        int checks = 0;
        void Check(bool ok, string message) { checks++; if (!ok) throw new InvalidOperationException("Rewards: " + message); }
        static JsonDocument Read(string path) { using var file = File.OpenRead(path); using var gzip = new GZipStream(file, CompressionMode.Decompress); return JsonDocument.Parse(gzip); }
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        using var awards = Read(Path.Combine(root, "tests/fixtures/match-rewards.json.gz"));
        string shop = awards.RootElement.GetProperty("initialShop").GetRawText(), wallet = awards.RootElement.GetProperty("initialWallet").GetRawText();
        foreach (var test in awards.RootElement.GetProperty("cases").EnumerateArray())
        {
            var result = MatchRewards.Calculate(shop, wallet, test.GetProperty("key").GetString()!, test.GetProperty("duo").GetBoolean(),
                test.GetProperty("stats").Deserialize<RewardStats>(options)!, DateOnly.ParseExact(test.GetProperty("day").GetString()!, "yyyy-MM-dd"), test.GetProperty("time").GetInt64());
            Check(JsonNode.DeepEquals(JsonNode.Parse(result.Shop), JsonNode.Parse(test.GetProperty("shop").GetRawText())), "shop update " + test.GetProperty("key"));
            Check(JsonNode.DeepEquals(JsonNode.Parse(result.Wallet), JsonNode.Parse(test.GetProperty("wallet").GetRawText())), "wallet update " + test.GetProperty("key"));
            JsonNode? actual = JsonSerializer.SerializeToNode(result.Award, options);
            if (actual is not null) actual["fresh"] = result.Fresh;
            Check(JsonNode.DeepEquals(actual, JsonNode.Parse(test.GetProperty("award").GetRawText())), "award breakdown");
            shop = result.Shop; wallet = result.Wallet;
        }
        using var traces = Read(Path.Combine(root, "tests/fixtures/reward-tracker.json.gz"));
        foreach (var trace in traces.RootElement.EnumerateArray())
        {
            string variant = trace.GetProperty("variant").GetString()!;
            var session = new MatchPlanningSession(catalog.Variants.Single(v => v.Id == variant), 143,
                Enumerable.Range(0, 8).Select(i => new MatchSeat("Player " + i, i == 0)).ToArray(), trace.GetProperty("duo").GetBoolean());
            session.State.PlayOut = true;
            var tracker = new MatchRewardTracker(session); int observation = 0;
            var expected = trace.GetProperty("observations");
            void Observe()
            {
                tracker.Update(); var reference = expected[observation++].Deserialize<RewardStats>(options)!;
                Check(tracker.Stats == reference, $"{variant}/{observation}: {tracker.Stats} != {reference}");
            }
            Observe();
            for (int n = 0; !session.State.Over && n < 100; n++)
            {
                if (session.Rounds.RoundType != "carousel" && n % 3 != 0) session.Ai.Turn(session.State.Players[0], session.Rounds.Position, session.Rounds.Phase, session.Rounds.RoundType);
                session.StartCombat(); Observe(); session.FinishCombat(); Observe(); Observe();
                if (!session.State.Over) session.Advance();
            }
            Check(session.State.Over && observation == expected.GetArrayLength(), "complete tracker trace");
        }
        Console.WriteLine($"Match rewards: {checks} source award and tracker comparisons passed.");
        return checks;
    }
}

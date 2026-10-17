using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class RoundFlowParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/round-flow.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        var json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        int checks = 0;
        void Check(bool condition, string message) { checks++; if (!condition) throw new InvalidOperationException("Round flow parity: " + message); }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string id = trace.GetProperty("variant").GetString()!;
            var source = catalog.Variants.Single(v => v.Id == id);
            var rules = source with { Offers = source.Offers! with { CustomArmory = false } };
            bool duo = trace.GetProperty("duo").GetBoolean();
            var state = new MatchState(rules, 143, Enumerable.Range(0, 8).Select(i => new MatchSeat($"Player {i}", i == 0)).ToArray(), duo) { PlayOut = true };
            foreach (var p in state.Players) { p.Power = new("fixture_power"); p.Shop.Locked = p.Id % 2 == 0; p.Shop.FreeRolls = 3; }
            if (duo) state.DamageTeam(3, state.Teams[3].Health); else state.Players[7].Health = 0;
            state.Eliminate();
            void Snapshot(MatchPlayer player, JsonElement expected)
            {
                Check(player.Health == expected.GetProperty("hp").GetInt32(), id + " HP");
                Check(player.Economy.Gold == expected.GetProperty("gold").GetInt32(), id + " gold");
                Check(player.Economy.Level == expected.GetProperty("level").GetInt32() && player.Economy.Xp == expected.GetProperty("xp").GetInt32(), id + " leveling");
                Check(player.Shop.FreeRolls == expected.GetProperty("freeRolls").GetInt32(), id + " free rolls");
                Check(player.Power!.Rank == expected.GetProperty("rank").GetInt32(), id + " rank");
                Check(player.Shop.Slots.SequenceEqual(expected.GetProperty("shop").EnumerateArray().Select(e => e.GetString())), id + " offers in shop");
                var offer = expected.GetProperty("offer");
                Check(player.Offer is null ? offer.ValueKind == JsonValueKind.Null : offer.ValueKind != JsonValueKind.Null &&
                    player.Offer.Kind == offer.GetProperty("kind").GetString() && player.Offer.Rerolls == offer.GetProperty("rerolls").GetInt32() &&
                    player.Offer.Options.SequenceEqual(offer.GetProperty("options").EnumerateArray().Select(e => e.GetString()!)), id + " pending choice");
            }
            JsonElement expectedLog = default;
            int logIndex = 0;
            MatchRounds? rounds = null;
            var hooks = new Hooks((name, player) =>
            {
                var expected = expectedLog[logIndex++];
                Check(expected.GetProperty("name").GetString() == name, id + " hook order");
                if (player is not null) { Check(player.Id == expected.GetProperty("id").GetInt32(), id + " hook player"); Snapshot(player, expected.GetProperty("state")); }
                else Check(rounds!.Phase.ToString().ToLowerInvariant() == expected.GetProperty("phase").GetString(), id + " planning phase");
            });
            rounds = new(state, new MatchOffers(state, hooks), hooks, phase: MatchPhase.Result);
            foreach (var step in trace.GetProperty("steps").EnumerateArray())
            {
                expectedLog = step.GetProperty("log"); logIndex = 0;
                int resets = hooks.Resets;
                Check(rounds.Advance(), id + " advancement");
                Check(hooks.Resets - resets == (duo ? 8 : 0), id + " transfer resets include eliminated players");
                Check(logIndex == expectedLog.GetArrayLength(), id + " all hooks");
                Check(rounds.Position.Stage == step.GetProperty("stage").GetInt32() && rounds.Position.Round == step.GetProperty("round").GetInt32(), id + " round position");
                Check(rounds.RoundType == step.GetProperty("type").GetString() && rounds.Phase == MatchPhase.Planning, id + " round type/phase");
                Check(rounds.Pairs.SequenceEqual(step.GetProperty("pairs").Deserialize<MatchPair[]>(json)!), id + " pairing order");
                for (int i = 0; i < 8; i++) Snapshot(state.Players[i], step.GetProperty("players")[i]);
            }
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), id + " RNG");
        }
        Console.WriteLine($"Round flow parity: {checks:N0} checks across 44 Solo/Duos traces.");
        return checks;
    }
    private sealed class Hooks(Action<string, MatchPlayer?> log) : IMatchRoundHooks, IMatchOfferHooks
    {
        public int Resets { get; private set; }
        public void ResetTransfers(MatchPlayer player) => Resets++;
        public IncomeBreakdown Income(MatchPlayer player, RoundPosition round, IncomeBreakdown income)
        { log("onIncome", player); return income with { Bonus = income.Bonus + player.Id % 3 }; }
        public void RankUp(MatchPlayer player, RoundPosition round) => log("onRankUp", player);
        public void RoundStart(MatchPlayer player, RoundPosition round)
        { log("onRoundStart", player); player.Shop.FreeRolls++; if (player.Id == 0) player.Health++; }
        public void StartCarousel(RoundPosition round) => log("carousel", null);
        public void PlanAi(RoundPosition round) => log("ai", null);
        public string[] Armory(MatchPlayer player, RoundPosition round) => throw new InvalidOperationException("Unexpected custom armory.");
        public string[] CustomOptions(MatchPlayer player, RoundPosition round, OfferDefinition definition, IReadOnlyList<string> exclude) => ["custom_a", "custom_b", "custom_c"];
        public void AugmentPicked(MatchPlayer player, string id) => throw new InvalidOperationException("Unexpected pick.");
        public void PowerPicked(MatchPlayer player, string id) => throw new InvalidOperationException("Unexpected pick.");
        public void CustomChosen(MatchPlayer player, PlayerOffer offer, string id) => throw new InvalidOperationException("Unexpected pick.");
    }
}

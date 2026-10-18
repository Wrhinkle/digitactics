using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class SettlementEffectsParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        int checks = 0;
        void Check(bool ok, string detail) { checks++; if (!ok) throw new InvalidOperationException("Settlement effects: " + detail); }
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/faction-settlement.json.gz"));
        using var zip = new GZipStream(input, CompressionMode.Decompress);
        using var doc = JsonDocument.Parse(zip);
        foreach (var sample in doc.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == sample.GetProperty("variant").GetString());
            var state = new MatchState(rules, 143, [new("Player", true)]);
            var units = new MatchUnitLifecycle(state, () => new(4, 1));
            var effects = new MatchSettlementEffects(state, units, new(4, 1));
            var counts = new Dictionary<string, int> { ["FUN"] = sample.GetProperty("count").GetInt32() };
            var match = new ResolvedMatch(0, null, sample.GetProperty("pve").GetBoolean(), false,
                sample.GetProperty("won").GetBoolean() ? 0 : 1, [], counts, counts);
            Check(effects.RoundEnd(new(match, state.Players[0], 0, null, [], counts)) == sample.GetProperty("gold").GetInt32(), rules.Id + " Fungus gold");
            // MatchState creates initial shop offers; compare the hook's RNG separately.
            var reference = new MatchState(rules, 143, [new("Player", true)]);
            Check(state.Random.Next() == reference.Random.Next(), rules.Id + " no RNG consumed");
        }
        foreach (var rules in catalog.Variants)
        {
            var state = new MatchState(rules, 143, [new("Player", true), new("AI", false)]) { PlayOut = true };
            var units = new MatchUnitLifecycle(state, () => RoundPosition.First);
            var effects = new MatchSettlementEffects(state, units, RoundPosition.First);
            foreach (var player in state.Players) player.NextBuffs = new(.3, 35, 40);
            new MatchSettlement(state, 1, effects).Settle([]);
            Check(state.Players.All(p => p.NextBuffs is null), rules.Id + " clears buffs even without a fight");
            state.Players[0].Wisp = new("gold", 5); state.Players[0].Health = 0;
            state.Eliminate(effects.Eliminated);
            Check(!state.Players[0].Alive && state.Players[0].Wisp is null, rules.Id + " elimination clears Wisp");
        }
        Console.WriteLine($"Settlement effects parity: {checks:N0} checks.");
        return checks;
    }
}

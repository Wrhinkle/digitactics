using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class AiShoppingParity
{
    private sealed class Hooks : IMatchShopHooks
    {
        public PlanningUnit Bought(MatchPlayer p, PlanningUnit u, int slot, MatchPhase phase) => u;
        public void Sold(MatchPlayer p, PlanningUnit u) { }
        public void Combined(MatchPlayer p, PlanningUnit u, IReadOnlyList<PlanningUnit> sources) { }
    }
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/ai-shopping.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool ok, string message) { checks++; if (!ok) throw new InvalidOperationException("AI shopping: " + message); }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var state = new MatchState(rules, trace.GetProperty("seed").GetUInt32(), [new("AI", false)]);
            var p = state.Players[0]; IMatchShopHooks hooks = trace.GetProperty("concreteHooks").GetBoolean() ? new ColiseumPlanning(state) : new Hooks();
            var ai = new AiShopping(rules, new(hooks)); int step = 0;
            foreach (var sample in trace.GetProperty("steps").EnumerateArray())
            {
                string context = $"{rules.Id}/{trace.GetProperty("seed")}/{step++}";
                var before = sample.GetProperty("before"); p.Economy.Credit(before.GetProperty("gold").GetInt32() - p.Economy.Gold);
                p.Shop.FreeRolls = before.GetProperty("freeRolls").GetInt32(); int rolls = 0;
                int stage = sample.GetProperty("stage").GetInt32();
                ai.Shop(p, MatchPhase.Planning, stage, sample.GetProperty("round").GetInt32(), () => p.Economy.Level, _ => 0,
                    () => { rolls++; p.Shop.Reroll(); });
                ai.TrimBench(p, MatchPhase.Planning);
                var expected = sample.GetProperty("after");
                Check(p.Mechanics.ColiseumCores == expected.GetProperty("cores").GetInt32(), context + " cores");
                Check(ai.Keep(stage) == sample.GetProperty("keep").GetInt32(), context + " reserve");
                Check(rolls == sample.GetProperty("rolls").GetInt32(), context + " rolls");
                Check(p.Economy.Gold == expected.GetProperty("gold").GetInt32(), context + " gold");
                Check(p.Economy.Level == expected.GetProperty("level").GetInt32() && p.Economy.Xp == expected.GetProperty("xp").GetInt32(), context + " experience");
                Check(p.Shop.FreeRolls == expected.GetProperty("freeRolls").GetInt32(), context + " free rolls");
                Check(p.Shop.Slots.SequenceEqual(expected.GetProperty("shop").EnumerateArray().Select(e => e.GetString())), context + " shop");
                Check(p.Items.SequenceEqual(expected.GetProperty("items").EnumerateArray().Select(e => e.GetString())), context + " items");
                foreach (var pair in expected.GetProperty("pool").EnumerateObject()) Check(state.Pool.Remaining(pair.Name) == pair.Value.GetInt32(), context + " pool " + pair.Name);
                for (int i = 0; i < 9; i++)
                {
                    var original = expected.GetProperty("bench")[i]; var unit = p.Roster.Bench[i];
                    Check(original.ValueKind == JsonValueKind.Null ? unit is null : unit is not null && unit.Id == original.GetProperty("uid").GetInt32() && unit.Line == original.GetProperty("line").GetString() && unit.Stage == original.GetProperty("stage").GetInt32(), context + " bench");
                    if (unit is not null && original.TryGetProperty("mods", out var mods))
                        Check(unit.Mods is { } actualMods && JsonElement.DeepEquals(actualMods, mods), context + " modifiers");
                }
            }
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), rules.Id + " RNG");
        }
        Console.WriteLine($"AI shopping parity: {checks:N0} checks across {fixture.RootElement.GetArrayLength()} traces.");
        return checks;
    }
}

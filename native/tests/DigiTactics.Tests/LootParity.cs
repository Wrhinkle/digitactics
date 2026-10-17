using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class LootParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/loot.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        int checks = 0;
        void Check(bool condition, string message) { checks++; if (!condition) throw new InvalidOperationException("Loot parity: " + message); }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string id = trace.GetProperty("variant").GetString()!;
            var source = catalog.Variants.Single(v => v.Id == id);
            // Reference exercises the generic generator plus injected custom specs, not set reward handlers.
            var rules = source with { Loot = source.Loot! with { CustomPve = false } };
            var state = new MatchState(rules, 143, [new("Human", true), new("AI", false)]);
            var loot = new MatchLoot(state);
            string lineId = trace.GetProperty("line").GetString()!;
            var line = rules.Lines.Single(l => l.Id == lineId);
            if (trace.GetProperty("full").GetBoolean())
                foreach (var player in state.Players) for (int j = 0; j < 9; j++)
                {
                    var unit = state.CreateUnit(lineId, 2); player.Roster.AddToBench(unit); state.Pool.Take(lineId, UnitValues.Copies(line, 2));
                }
            int delta = 3 - state.Pool.Remaining(lineId);
            if (delta > 0) state.Pool.Return(lineId, delta); else state.Pool.Take(lineId, -delta);
            void State(JsonElement expected)
            {
                foreach (var pair in expected.GetProperty("pool").EnumerateObject()) Check(state.Pool.Remaining(pair.Name) == pair.Value.GetInt32(), id + " pool");
                for (int i = 0; i < 2; i++)
                {
                    var player = state.Players[i]; var p = expected.GetProperty("players")[i];
                    Check(player.Economy.Gold == p.GetProperty("gold").GetInt32(), id + " gold");
                    Check(player.Items.SequenceEqual(p.GetProperty("items").EnumerateArray().Select(e => e.GetString()!)), id + " inventory");
                    Check(player.Shop.Slots.SequenceEqual(p.GetProperty("shop").EnumerateArray().Select(e => e.GetString())), id + " maxed offers");
                    Check(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(player.Orbs, jsonOptions), p.GetProperty("orbs")), id + " orb IDs/positions/contents");
                    for (int j = 0; j < 9; j++)
                    {
                        var unit = player.Roster.Bench[j]; var u = p.GetProperty("bench")[j];
                        Check(unit is null ? u.ValueKind == JsonValueKind.Null : u.ValueKind != JsonValueKind.Null &&
                            unit.Id == u.GetProperty("uid").GetInt32() && unit.Line == u.GetProperty("line").GetString() &&
                            unit.Stage == u.GetProperty("stage").GetInt32(), id + " unit collection");
                    }
                }
            }
            foreach (var step in trace.GetProperty("steps").EnumerateArray())
            {
                var player = state.Players[step.GetProperty("index").GetInt32()];
                var round = new RoundPosition(step.GetProperty("stage").GetInt32(), step.GetProperty("round").GetInt32());
                var specs = step.GetProperty("specs").Deserialize<LootSpec[]>(jsonOptions);
                var summary = loot.Reward(player, round, step.GetProperty("won").GetBoolean(), specs is null ? null : (_, _) => specs);
                Check(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(summary, jsonOptions), step.GetProperty("reward")), id + " reward summary");
                State(step.GetProperty("before"));
                if (step.GetProperty("collect").GetBoolean()) loot.CollectAll(player, player.IsHuman);
                State(step.GetProperty("after"));
            }
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), id + " RNG consumption");
            Check(loot.Collect(state.Players[0], -1) is null, id + " invalid orb");
        }
        Console.WriteLine($"Loot parity: {checks:N0} checks across 44 generic/custom drop traces.");
        return checks;
    }
}

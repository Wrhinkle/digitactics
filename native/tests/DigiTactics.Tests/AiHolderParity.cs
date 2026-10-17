using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class AiHolderParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/ai-holders.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool condition, string context) { checks++; if (!condition) throw new InvalidOperationException("AI holder parity: " + context); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var state = new MatchState(rules, 143, [new("AI", false)]); var p = state.Players[0]; var ai = new PlanningAi(rules);
            int step = 0;
            foreach (var sample in trace.GetProperty("cases").EnumerateArray())
            {
                foreach (var entry in p.Roster.Entries.ToArray()) p.Roster.Remove(entry.Unit.Id, false);
                foreach (var spec in sample.GetProperty("setup").EnumerateArray())
                {
                    var u = spec.GetProperty("unit");
                    var unit = new PlanningUnit(u.GetProperty("uid").GetInt32(), u.GetProperty("line").GetString()!, u.GetProperty("stage").GetInt32(),
                        Strings(u.GetProperty("items")), Mods: u.GetProperty("mods").Clone(), NoItems: u.GetProperty("noItems").GetBoolean());
                    int slot = p.Roster.FreeBenchSlot; p.Roster.AddToBench(unit);
                    if (spec.GetProperty("board").GetBoolean()) { int i = spec.GetProperty("index").GetInt32(); p.Roster.Move(UnitSpot.Bench(slot), UnitSpot.Board(i % 7, 4 + i / 7), false, () => 100); }
                }
                p.Items.Clear(); p.Items.AddRange(Strings(sample.GetProperty("inventory")));
                string context = $"{rules.Id}/{step++}"; var holders = ai.Holders(p); var expected = sample.GetProperty("holders");
                Check(holders.Count == expected.GetArrayLength(), context + " holder count");
                for (int i = 0; i < holders.Count; i++) Check(holders[i].Unit.Id == expected[i].GetProperty("uid").GetInt32() && holders[i].Kind == expected[i].GetProperty("kind").GetString(), context + " holder order/role");
                if (rules.Id.StartsWith("coliseum/", StringComparison.Ordinal)) new ColiseumPlanning(state).AiTurn(p);
                var after = sample.GetProperty("after");
                Check(p.Items.SequenceEqual(Strings(after.GetProperty("items"))), context + " inventory");
                var units = p.Roster.Entries.Select(e => e.Unit).ToArray(); var original = after.GetProperty("units");
                Check(units.Length == original.GetArrayLength(), context + " unit count");
                for (int i = 0; i < units.Length; i++)
                {
                    Check(units[i].Id == original[i].GetProperty("uid").GetInt32() && units[i].Items.SequenceEqual(Strings(original[i].GetProperty("items"))), context + " ordered equipment");
                    Check(JsonElement.DeepEquals(units[i].Mods!.Value, original[i].GetProperty("mods")), context + " modifiers");
                }
                Check(state.Random.Next() == sample.GetProperty("nextRandom").GetDouble(), context + " RNG");
            }
        }
        Console.WriteLine($"AI holder/core parity: {checks:N0} checks across 22 variants.");
        return checks;
    }
}

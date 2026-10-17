using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class AiInventoryParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/ai-inventory.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool condition, string context) { checks++; if (!condition) throw new InvalidOperationException("AI inventory parity: " + context); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        void Snapshot(MatchPlayer p, JsonElement expected, string context)
        {
            Check(p.Items.SequenceEqual(Strings(expected.GetProperty("items"))), context + " inventory");
            Check(p.Globals.SequenceEqual(Strings(expected.GetProperty("globals"))), context + " globals");
            var units = p.Roster.Entries.Select(e => e.Unit).ToArray(); var original = expected.GetProperty("units");
            Check(units.Length == original.GetArrayLength(), context + " unit count");
            for (int i = 0; i < units.Length; i++)
                Check(units[i].Id == original[i].GetProperty("uid").GetInt32() && units[i].Items.SequenceEqual(Strings(original[i].GetProperty("items"))), context + " ordered equipment");
        }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var state = new MatchState(rules, 143, [new("AI", false)]); var p = state.Players[0];
            var equipment = new PlanningItems(state, new Hooks()); var ai = new AiInventory(rules, equipment);
            int step = 0;
            foreach (var sample in trace.GetProperty("cases").EnumerateArray())
            {
                foreach (var entry in p.Roster.Entries.ToArray()) p.Roster.Remove(entry.Unit.Id, false);
                foreach (var spec in sample.GetProperty("setup").EnumerateArray())
                {
                    var u = spec.GetProperty("unit");
                    var unit = new PlanningUnit(u.GetProperty("uid").GetInt32(), u.GetProperty("line").GetString()!, u.GetProperty("stage").GetInt32(), Strings(u.GetProperty("items")), NoItems: u.GetProperty("noItems").GetBoolean());
                    int slot = p.Roster.FreeBenchSlot; p.Roster.AddToBench(unit);
                    if (spec.GetProperty("board").GetBoolean()) { int i = spec.GetProperty("index").GetInt32(); p.Roster.Move(UnitSpot.Bench(slot), UnitSpot.Board(i % 7, 4 + i / 7), false, () => 100); }
                }
                p.Items.Clear(); p.Items.AddRange(Strings(sample.GetProperty("inventory"))); p.Globals.Clear();
                string context = $"{rules.Id}/{step++}";
                var recipe = equipment.RecipeFrom(p.Items, sample.GetProperty("recipeId").GetString()!); var originalRecipe = sample.GetProperty("recipe");
                Check((recipe is null) == (originalRecipe.ValueKind == JsonValueKind.Null), context + " recipe presence");
                if (recipe is { } r) Check(r.First == originalRecipe[0].GetInt32() && r.Second == originalRecipe[1].GetInt32(), context + " recipe indexes");
                ai.Assign(p, sample.GetProperty("stage").GetInt32(), sample.GetProperty("hasEquipRules").GetBoolean());
                Snapshot(p, sample.GetProperty("after"), context + " after AI");
                Check(equipment.Craft(p, sample.GetProperty("craftId").GetString()!) == sample.GetProperty("crafted").GetBoolean(), context + " craft");
                Check(p.Items.SequenceEqual(Strings(sample.GetProperty("afterCraft"))), context + " after craft");
                bool given = sample.GetProperty("giveUid").ValueKind != JsonValueKind.Null && equipment.GiveFromBench(p, sample.GetProperty("giveId").GetString()!, sample.GetProperty("giveUid").GetInt32());
                Check(given == sample.GetProperty("given").GetBoolean(), context + " give from bench");
                Snapshot(p, sample.GetProperty("final"), context + " final");
                Check(state.Random.Next() == sample.GetProperty("nextRandom").GetDouble(), context + " RNG");
            }
        }
        Console.WriteLine($"AI inventory parity: {checks:N0} checks across 22 variants.");
        return checks;
    }
    private sealed class Hooks : IMatchItemHooks
    {
        public string UseConsumable(MatchPlayer player, int itemIndex, PlanningUnit unit) => throw new InvalidOperationException("Unexpected consumable use");
        public string? EquipVeto(MatchPlayer player, PlanningUnit unit, string item, string? combined) => null;
        public void Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources) => throw new InvalidOperationException("Unexpected merge");
    }
}

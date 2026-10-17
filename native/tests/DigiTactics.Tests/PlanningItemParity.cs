using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class PlanningItemParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/planning-items.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool condition, string message) { checks++; if (!condition) throw new InvalidOperationException("Planning item parity: " + message); }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string id = trace.GetProperty("variant").GetString()!;
            var rules = catalog.Variants.Single(v => v.Id == id);
            var state = new MatchState(rules, 143, [new("Human", true)]);
            var player = state.Players[0];
            var hooks = new Hooks(id.StartsWith("underlords", StringComparison.Ordinal));
            var commands = new PlanningItems(rules, hooks);
            foreach (var recipe in trace.GetProperty("recipes").EnumerateArray())
                Check(commands.Combine(recipe.GetProperty("a").GetString()!, recipe.GetProperty("b").GetString()!) == recipe.GetProperty("result").GetString(), id + " recipe");
            int step = 0;
            foreach (var sample in trace.GetProperty("cases").EnumerateArray())
            {
                foreach (var owned in player.Roster.Entries.ToArray()) player.Roster.Remove(owned.Unit.Id, false);
                player.Items.Clear(); player.Globals.Clear();
                var before = sample.GetProperty("before"); var unit = before.GetProperty("unit");
                var ownedUnit = new PlanningUnit(1, unit.GetProperty("line").GetString()!, unit.GetProperty("stage").GetInt32(),
                    unit.GetProperty("items").EnumerateArray().Select(e => e.GetString()!).ToArray(), NoItems: unit.GetProperty("noItems").GetBoolean());
                player.Roster.AddToBench(ownedUnit);
                if (before.GetProperty("board").GetBoolean()) player.Roster.Move(UnitSpot.Bench(0), UnitSpot.Board(3, 4), false, () => 100);
                player.Items.AddRange(before.GetProperty("items").EnumerateArray().Select(e => e.GetString()!));
                hooks.Deny = sample.GetProperty("deny").GetBoolean();
                string context = $"{id}/{step++}";
                Check(commands.Equip(player, sample.GetProperty("index").GetInt32(), sample.GetProperty("uid").GetInt32(), sample.GetProperty("locked").GetBoolean()) == sample.GetProperty("result").GetString(), context + " equip result");
                var current = player.Roster.Entries.Single().Unit;
                Check(commands.CanHold(player, current, player.Items.FirstOrDefault() ?? rules.Items[0].Id) == sample.GetProperty("hold").GetBoolean(), context + " can hold");
                Check(commands.Swap(player, sample.GetProperty("swapA").GetInt32(), sample.GetProperty("swapB").GetInt32()) == sample.GetProperty("swap").GetBoolean(), context + " swap result");
                var after = sample.GetProperty("after");
                Check(player.Items.SequenceEqual(after.GetProperty("items").EnumerateArray().Select(e => e.GetString()!)), context + " inventory");
                Check(current.Items.SequenceEqual(after.GetProperty("unitItems").EnumerateArray().Select(e => e.GetString()!)), context + " equipped items");
                Check(player.Globals.SequenceEqual(after.GetProperty("globals").EnumerateArray().Select(e => e.GetString()!)), context + " globals");
            }
        }
        Console.WriteLine($"Planning item parity: {checks:N0} checks across 22 variants.");
        return checks;
    }
    private sealed class Hooks(bool underlords) : IMatchItemHooks
    {
        public bool Deny { get; set; }
        public string UseConsumable(MatchPlayer player, int itemIndex, PlanningUnit unit)
        { string id = player.Items[itemIndex]; player.Items.RemoveAt(itemIndex); return "used:" + id; }
        public string? EquipVeto(MatchPlayer player, PlanningUnit unit, string item, string? combined) => !underlords && Deny ? "testVeto" : null;
    }
}

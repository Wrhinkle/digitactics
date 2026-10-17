using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class CopyConsumableParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/copy-consumables.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool condition, string context) { checks++; if (!condition) throw new InvalidOperationException("Copy consumable parity: " + context); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string variant = trace.GetProperty("variant").GetString()!;
            var rules = catalog.Variants.Single(v => v.Id == variant);
            int step = 0;
            foreach (var sample in trace.GetProperty("cases").EnumerateArray())
            {
                var state = new MatchState(rules, sample.GetProperty("seed").GetUInt32(), [new("Human", true)]);
                var player = state.Players[0];
                int board = 0;
                foreach (var spec in sample.GetProperty("setup").EnumerateArray())
                {
                    var unit = state.CreateUnit(spec.GetProperty("line").GetString()!, spec.GetProperty("stage").GetInt32()) with { Items = Strings(spec.GetProperty("items")) };
                    int slot = player.Roster.FreeBenchSlot;
                    player.Roster.AddToBench(unit);
                    if (spec.GetProperty("board").GetBoolean()) player.Roster.Move(UnitSpot.Bench(slot), UnitSpot.Board(board++, 4), false, () => 100);
                }
                var target = player.Roster.Bench[0]!;
                if (sample.GetProperty("emptyPool").GetBoolean()) state.Pool.Take(target.Line, state.Pool.Remaining(target.Line));
                player.Items.AddRange(Strings(sample.GetProperty("inventory")));
                var hooks = new Hooks();
                var commands = new PlanningItems(state, hooks);
                string context = $"{variant}/{step++}";
                Check(commands.Equip(player, 1, target.Id, sample.GetProperty("locked").GetBoolean() ? MatchPhase.Combat : MatchPhase.Planning) == sample.GetProperty("result").GetString(), context + " result");
                var after = sample.GetProperty("after");
                Check(player.Items.SequenceEqual(Strings(after.GetProperty("items"))), context + " inventory");
                var entries = player.Roster.Entries.ToArray(); var expected = after.GetProperty("entries");
                Check(entries.Length == expected.GetArrayLength(), context + " unit count");
                for (int i = 0; i < entries.Length; i++)
                {
                    var entry = entries[i]; var e = expected[i]; var u = e.GetProperty("unit");
                    string spot = entry.Spot.IsBoard ? $"board:{entry.Spot.Column},{entry.Spot.Row}" : $"bench:{entry.Spot.Index}";
                    Check(spot == e.GetProperty("spot").GetString(), context + " spot");
                    Check(entry.Unit.Id == u.GetProperty("uid").GetInt32() && entry.Unit.Line == u.GetProperty("line").GetString() && entry.Unit.Stage == u.GetProperty("stage").GetInt32(), context + " unit");
                    Check(entry.Unit.Items.SequenceEqual(Strings(u.GetProperty("items"))), context + " equipment");
                }
                foreach (var pair in after.GetProperty("pool").EnumerateObject()) Check(state.Pool.Remaining(pair.Name) == pair.Value.GetInt32(), context + " pool " + pair.Name);
                Check(player.Shop.Slots.SequenceEqual(after.GetProperty("shop").EnumerateArray().Select(x => x.GetString())), context + " shop");
                var events = after.GetProperty("events");
                Check(hooks.Events.Count == events.GetArrayLength(), context + " hook count");
                for (int i = 0; i < hooks.Events.Count; i++)
                    Check(hooks.Events[i].Id == events[i].GetProperty("uid").GetInt32() && hooks.Events[i].Sources.SequenceEqual(events[i].GetProperty("from").EnumerateArray().Select(x => x.GetInt32())), context + " combine callback");
                Check(state.CreateUnit(target.Line, 0).Id == after.GetProperty("nextId").GetInt32(), context + " next ID");
                Check(state.Random.Next() == after.GetProperty("nextRandom").GetDouble(), context + " RNG");
            }
        }
        Console.WriteLine($"Copy consumable parity: {checks:N0} checks.");
        return checks;
    }
    private sealed class Hooks : IMatchItemHooks
    {
        public List<(int Id, int[] Sources)> Events { get; } = [];
        public void Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources) => Events.Add((unit.Id, sources.Select(u => u.Id).ToArray()));
        public string UseConsumable(MatchPlayer player, int itemIndex, PlanningUnit unit) => throw new InvalidOperationException("Unexpected delegation");
        public string? EquipVeto(MatchPlayer player, PlanningUnit unit, string item, string? combined) => null;
    }
}

using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class ConsumableParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/consumables.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool condition, string context) { checks++; if (!condition) throw new InvalidOperationException("Consumable parity: " + context); }
        static string[] Strings(JsonElement element) => element.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string variant = trace.GetProperty("variant").GetString()!;
            var state = new MatchState(catalog.Variants.Single(v => v.Id == variant), trace.GetProperty("seed").GetUInt32(), [new("Human", true)]);
            var player = state.Players[0];
            var unit = new PlanningUnit(1, trace.GetProperty("line").GetString()!, 0, []);
            player.Roster.AddToBench(unit);
            var commands = new PlanningItems(state, new Hooks());
            int step = 0;
            foreach (var sample in trace.GetProperty("cases").EnumerateArray())
            {
                var before = sample.GetProperty("before");
                player.Roster.UpdateUnit(unit with { Items = Strings(before.GetProperty("held")) });
                player.Items.Clear(); player.Items.AddRange(Strings(before.GetProperty("items")));
                string context = $"{variant}/{step++}";
                Check(commands.Equip(player, sample.GetProperty("index").GetInt32(), 1, false) == sample.GetProperty("result").GetString(), context + " result");
                var after = sample.GetProperty("after");
                Check(player.Items.SequenceEqual(Strings(after.GetProperty("items"))), context + " inventory");
                Check(player.Roster.Entries.Single().Unit.Items.SequenceEqual(Strings(after.GetProperty("held"))), context + " held items");
                Check(state.Random.Next() == sample.GetProperty("nextRandom").GetDouble(), context + " random stream");
            }
        }
        Console.WriteLine($"Consumable parity: {checks:N0} checks.");
        return checks;
    }
    private sealed class Hooks : IMatchItemHooks
    {
        public void Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources) => throw new InvalidOperationException("Unexpected combine");
        public string UseConsumable(MatchPlayer player, int itemIndex, PlanningUnit unit) => throw new InvalidOperationException("Native consumable unexpectedly delegated");
        public string? EquipVeto(MatchPlayer player, PlanningUnit unit, string item, string? combined) => null;
    }
}

using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class SpecialConsumableParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/special-consumables.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool condition, string context) { checks++; if (!condition) throw new InvalidOperationException("Special consumable parity: " + context); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string variant = trace.GetProperty("variant").GetString()!;
            var rules = catalog.Variants.Single(v => v.Id == variant);
            int step = 0;
            foreach (var sample in trace.GetProperty("cases").EnumerateArray())
            {
                var state = new MatchState(rules, sample.GetProperty("seed").GetUInt32(), [new("Player", sample.GetProperty("human").GetBoolean())]);
                var p = state.Players[0];
                if (rules.Economy!.Xp is not null) p.Economy.AddXp(sample.GetProperty("xp").GetInt32());
                else p.Economy.BeginRound(sample.GetProperty("roundStage").GetInt32(), 1);
                var mods = sample.GetProperty("mods");
                var u = state.CreateUnit(sample.GetProperty("line").GetString()!, sample.GetProperty("stage").GetInt32()) with { Mods = mods.ValueKind == JsonValueKind.Null ? null : mods.Clone() };
                p.Roster.AddToBench(u);
                var commands = new PlanningItems(state, new Hooks());
                if (sample.GetProperty("preUse").GetBoolean()) { p.Items.Add("rec_tome"); commands.Equip(p, 0, u.Id, MatchPhase.Planning); }
                if (sample.GetProperty("board").GetBoolean()) p.Roster.Move(UnitSpot.Bench(0), UnitSpot.Board(0, 4), false, () => 100);
                p.Mechanics.FatesPath = sample.GetProperty("fallen").GetString();
                p.Mechanics.ColiseumCores = sample.GetProperty("cores").GetInt32();
                var inventory = Strings(sample.GetProperty("inventory"));
                int mode = sample.GetProperty("poolMode").GetInt32();
                if (inventory[1] == "fat_dice") foreach (var l in rules.Lines)
                    if (mode == 0 || mode == 1 && l.Tier != 5 || mode == 2 && l.Tier != 1) state.Pool.Take(l.Id, state.Pool.Remaining(l.Id));
                p.Items.Clear(); p.Items.AddRange(inventory);
                var phase = Enum.Parse<MatchPhase>(sample.GetProperty("phase").GetString()!, true);
                string context = $"{variant}/{step++}";
                Check(commands.Equip(p, 1, u.Id, phase) == sample.GetProperty("result").GetString(), context + " result");
                var after = sample.GetProperty("after");
                Check(p.Items.SequenceEqual(Strings(after.GetProperty("items"))), context + " inventory");
                var currentMods = p.Roster.Entries.Single().Unit.Mods;
                Check(JsonElement.DeepEquals(currentMods ?? JsonSerializer.SerializeToElement<object?>(null), after.GetProperty("mods")), context + " modifiers");
                Check(p.Mechanics.FatesPath == after.GetProperty("fallen").GetString(), context + " path");
                Check(p.Mechanics.ColiseumCores == after.GetProperty("cores").GetInt32(), context + " stored cores");
                var offer = after.GetProperty("offer");
                Check((p.Offer is null) == (offer.ValueKind == JsonValueKind.Null), context + " offer presence");
                if (p.Offer is { } o)
                    Check(o.Kind == offer.GetProperty("kind").GetString() && o.Rerolls == offer.GetProperty("rerolls").GetInt32() && o.Options.SequenceEqual(Strings(offer.GetProperty("options"))), context + " offer");
                foreach (var pair in after.GetProperty("pool").EnumerateObject()) Check(state.Pool.Remaining(pair.Name) == pair.Value.GetInt32(), context + " pool " + pair.Name);
                Check(p.Shop.Slots.SequenceEqual(after.GetProperty("shop").EnumerateArray().Select(x => x.GetString())), context + " shop");
                Check(state.Random.Next() == after.GetProperty("nextRandom").GetDouble(), context + " RNG");
            }
        }
        Console.WriteLine($"Special consumable parity: {checks:N0} checks.");
        return checks;
    }
    private sealed class Hooks : IMatchItemHooks
    {
        public void Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources) => throw new InvalidOperationException("Unexpected combine");
        public string UseConsumable(MatchPlayer player, int itemIndex, PlanningUnit unit) => throw new InvalidOperationException("Unexpected delegation");
        public string? EquipVeto(MatchPlayer player, PlanningUnit unit, string item, string? combined) => null;
    }
}

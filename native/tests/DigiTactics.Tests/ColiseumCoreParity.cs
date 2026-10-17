using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class ColiseumCoreParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/coliseum-core.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool condition, string context) { checks++; if (!condition) throw new InvalidOperationException("Coliseum core parity: " + context); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        void Snapshot(MatchPlayer p, JsonElement e, string context)
        {
            Check(p.Items.SequenceEqual(Strings(e.GetProperty("items"))), context + " inventory");
            Check(p.Mechanics.ColiseumCores == e.GetProperty("cores").GetInt32(), context + " stored cores");
            Check(p.Mechanics.ColiseumCoreUnitId == (e.GetProperty("coreUid").ValueKind == JsonValueKind.Null ? null : e.GetProperty("coreUid").GetInt32()), context + " pending target");
            var offer = e.GetProperty("offer");
            Check((p.Offer is null) == (offer.ValueKind == JsonValueKind.Null), context + " offer presence");
            if (p.Offer is { } o) Check(o.Kind == offer.GetProperty("kind").GetString() && o.Rerolls == offer.GetProperty("rerolls").GetInt32() && o.Options.SequenceEqual(Strings(offer.GetProperty("options"))), context + " offer");
            var unit = e.GetProperty("unit"); var actual = p.Roster.Entries.FirstOrDefault()?.Unit;
            Check((actual is null) == (unit.ValueKind == JsonValueKind.Null), context + " unit presence");
            if (actual is not null) Check(JsonElement.DeepEquals(actual.Mods!.Value, unit.GetProperty("mods")), context + " modifiers");
        }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string variant = trace.GetProperty("variant").GetString()!;
            var rules = catalog.Variants.Single(v => v.Id == variant);
            int step = 0;
            foreach (var sample in trace.GetProperty("cases").EnumerateArray())
            {
                var state = new MatchState(rules, sample.GetProperty("seed").GetUInt32(), [new("Player", sample.GetProperty("human").GetBoolean())]);
                var p = state.Players[0];
                var u = state.CreateUnit(sample.GetProperty("line").GetString()!, sample.GetProperty("stage").GetInt32()) with { Mods = sample.GetProperty("mods").Clone() };
                p.Roster.AddToBench(u);
                if (sample.GetProperty("board").GetBoolean()) p.Roster.Move(UnitSpot.Bench(0), UnitSpot.Board(0, 4), false, () => 100);
                p.Items.AddRange(["col_core", "col_core"]);
                p.Mechanics.ColiseumCores = sample.GetProperty("cores").GetInt32();
                var commands = new PlanningItems(state, new Hooks());
                var offers = new MatchOffers(state, new Hooks());
                var phase = Enum.Parse<MatchPhase>(sample.GetProperty("phase").GetString()!, true);
                string context = $"{variant}/{step++}";
                Check(commands.Equip(p, 0, u.Id, phase) == sample.GetProperty("used").GetString(), context + " use");
                Snapshot(p, sample.GetProperty("afterUse"), context + " after use");
                Check(commands.Equip(p, 0, u.Id, phase) == sample.GetProperty("busy").GetString(), context + " busy");
                Snapshot(p, sample.GetProperty("afterBusy"), context + " after busy");
                if (sample.GetProperty("remove").GetBoolean()) p.Roster.Remove(u.Id, false);
                Check(offers.Reroll(p, new(2, 1)) == sample.GetProperty("rolled").GetBoolean(), context + " reroll");
                Snapshot(p, sample.GetProperty("afterRoll"), context + " after reroll");
                Check(offers.Reroll(p, new(2, 1)) == sample.GetProperty("rolledAgain").GetBoolean(), context + " second reroll");
                if (sample.GetProperty("removeLate").GetBoolean()) p.Roster.Remove(u.Id, false);
                if (sample.GetProperty("full").GetBoolean()) { p.Items.Clear(); p.Items.AddRange(Enumerable.Repeat("col_core", rules.Loot!.ItemCap)); }
                Check(offers.Choose(p, -1, false) == sample.GetProperty("invalid").GetString(), context + " invalid index");
                Check(offers.Choose(p, sample.GetProperty("choice").GetInt32(), false) == sample.GetProperty("selected").GetString(), context + " choice");
                Snapshot(p, sample.GetProperty("afterChoice"), context + " after choice");
                if (sample.GetProperty("directInvalid").GetBoolean()) new ColiseumPlanning(state).ChooseCore(p, "missing_power");
                Snapshot(p, sample.GetProperty("after"), context + " final");
                Check(state.Random.Next() == sample.GetProperty("nextRandom").GetDouble(), context + " RNG");
            }
        }
        Console.WriteLine($"Coliseum core parity: {checks:N0} checks.");
        return checks;
    }
    private sealed class Hooks : IMatchItemHooks, IMatchOfferHooks
    {
        public void Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources) => throw new InvalidOperationException("Unexpected combine");
        public string UseConsumable(MatchPlayer player, int itemIndex, PlanningUnit unit) => throw new InvalidOperationException("Unexpected consumable delegation");
        public string? EquipVeto(MatchPlayer player, PlanningUnit unit, string item, string? combined) => null;
        public string[] Armory(MatchPlayer player, RoundPosition round) => throw new InvalidOperationException("Unexpected armory");
        public string[] CustomOptions(MatchPlayer player, RoundPosition round, OfferDefinition definition, IReadOnlyList<string> exclude) => throw new InvalidOperationException("Unexpected options delegation");
        public void CustomChosen(MatchPlayer player, PlayerOffer offer, string id) => throw new InvalidOperationException("Unexpected choice delegation");
        public void AugmentPicked(MatchPlayer player, string id) => throw new InvalidOperationException("Unexpected augment");
        public void PowerPicked(MatchPlayer player, string id) => throw new InvalidOperationException("Unexpected power");
    }
}

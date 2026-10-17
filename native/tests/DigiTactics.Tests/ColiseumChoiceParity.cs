using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class ColiseumChoiceParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/coliseum-choices.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool condition, string context) { checks++; if (!condition) throw new InvalidOperationException("Coliseum choice parity: " + context); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        void Snapshot(MatchPlayer p, JsonElement e, string context)
        {
            Check(p.Items.SequenceEqual(Strings(e.GetProperty("items"))), context + " inventory");
            Check(p.Economy.Gold == e.GetProperty("gold").GetInt32(), context + " gold");
            var m = e.GetProperty("mech"); var g = p.Mechanics.ColiseumGambit;
            Check(p.Mechanics.ColiseumFormUnitId == (m.GetProperty("formUid").ValueKind == JsonValueKind.Null ? null : m.GetProperty("formUid").GetInt32()), context + " pending target");
            Check(g.Gems == m.GetProperty("gem").GetDouble() && g.Level == m.GetProperty("gLv").GetInt32() && g.Count == m.GetProperty("gCount").GetInt32() && g.DoubleDown == m.GetProperty("dd").GetBoolean() && g.Ready == m.GetProperty("gReady").GetBoolean(), context + " gambit state");
            var offer = e.GetProperty("offer");
            Check((p.Offer is null) == (offer.ValueKind == JsonValueKind.Null), context + " offer presence");
            if (p.Offer is { } o) Check(o.Kind == offer.GetProperty("kind").GetString() && o.Rerolls == offer.GetProperty("rerolls").GetInt32() && o.Options.SequenceEqual(Strings(offer.GetProperty("options"))), context + " offer");
            var unit = e.GetProperty("unit"); var actual = p.Roster.Entries.FirstOrDefault()?.Unit;
            Check((actual is null) == (unit.ValueKind == JsonValueKind.Null), context + " unit presence");
            if (actual is not null)
            {
                Check(JsonElement.DeepEquals(actual.Mods!.Value, unit.GetProperty("mods")), context + " modifiers");
                Check(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(actual.BonusTraits), unit.GetProperty("bonusTraits")), context + " traits");
            }
        }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            var original = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            int step = 0;
            foreach (var sample in trace.GetProperty("cases").EnumerateArray())
            {
                string kind = sample.GetProperty("kind").GetString()!;
                // A controlled schedule routes both reference and native choices through their real offer engines.
                var rules = original with { Offers = original.Offers! with { Schedule = [new(kind, 1, 1, Rerolls: 1)] } };
                var state = new MatchState(rules, sample.GetProperty("seed").GetUInt32(), [new("Human", true)]);
                var p = state.Players[0];
                var u = state.CreateUnit(sample.GetProperty("line").GetString()!, sample.GetProperty("stage").GetInt32()) with
                    { Mods = JsonSerializer.SerializeToElement(new { pu = "warp", other = 7 }), BonusTraits = new() { ["fixture"] = 2 } };
                p.Roster.AddToBench(u); p.Items.AddRange(Strings(sample.GetProperty("inventory")));
                var initial = sample.GetProperty("initial"); var g = p.Mechanics.ColiseumGambit;
                p.Mechanics.ColiseumFormUnitId = initial.GetProperty("formUid").GetInt32();
                g.Gems = initial.GetProperty("gem").GetDouble(); g.Level = initial.GetProperty("gLv").GetInt32();
                g.Count = initial.GetProperty("gCount").GetInt32(); g.DoubleDown = initial.GetProperty("dd").GetBoolean(); g.Ready = initial.GetProperty("gReady").GetBoolean();
                var offers = new MatchOffers(state, new Hooks());
                string context = $"{rules.Id}/{step++}";
                offers.Make(p, new(1, 1)); Snapshot(p, sample.GetProperty("made"), context + " made");
                Check(offers.Reroll(p, new(1, 1)) == sample.GetProperty("rolled").GetBoolean(), context + " reroll");
                Snapshot(p, sample.GetProperty("afterRoll"), context + " after reroll");
                if (sample.GetProperty("remove").GetBoolean()) p.Roster.Remove(u.Id, false);
                Check(offers.Choose(p, sample.GetProperty("choice").GetInt32(), false) == sample.GetProperty("selected").GetString(), context + " choice");
                Snapshot(p, sample.GetProperty("afterChoice"), context + " after choice");
                if (sample.GetProperty("direct").GetString() is { } direct)
                {
                    var planning = new ColiseumPlanning(state);
                    if (kind == "col_form") planning.ChooseForm(p, direct); else planning.ChooseGambit(p, direct);
                }
                Snapshot(p, sample.GetProperty("after"), context + " final");
                Check(state.Random.Next() == sample.GetProperty("nextRandom").GetDouble(), context + " RNG");
            }
        }
        Console.WriteLine($"Coliseum form/gambit parity: {checks:N0} checks.");
        return checks;
    }
    private sealed class Hooks : IMatchOfferHooks
    {
        public string[] Armory(MatchPlayer player, RoundPosition round) => throw new InvalidOperationException("Unexpected armory");
        public string[] CustomOptions(MatchPlayer player, RoundPosition round, OfferDefinition definition, IReadOnlyList<string> exclude) => throw new InvalidOperationException("Unexpected options delegation");
        public void CustomChosen(MatchPlayer player, PlayerOffer offer, string id) => throw new InvalidOperationException("Unexpected choice delegation");
        public void AugmentPicked(MatchPlayer player, string id) => throw new InvalidOperationException("Unexpected augment");
        public void PowerPicked(MatchPlayer player, string id) => throw new InvalidOperationException("Unexpected power");
    }
}

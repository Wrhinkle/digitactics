using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class UnderlordsPlanningParity
{
    private sealed class Hooks : IMatchOfferHooks
    {
        public string[] Armory(MatchPlayer p, RoundPosition r) => throw new NotSupportedException();
        public string[] CustomOptions(MatchPlayer p, RoundPosition r, OfferDefinition d, IReadOnlyList<string> e) => throw new NotSupportedException();
        public void AugmentPicked(MatchPlayer p, string id) => throw new NotSupportedException();
        public void PowerPicked(MatchPlayer p, string id) => throw new NotSupportedException();
        public void CustomChosen(MatchPlayer p, PlayerOffer o, string id) => throw new NotSupportedException();
    }
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/underlords-planning.json.gz"));
        using var zip = new GZipStream(input, CompressionMode.Decompress);
        using var doc = JsonDocument.Parse(zip);
        int checks = 0;
        void Check(bool ok, string c) { checks++; if (!ok) throw new InvalidOperationException("Underlords planning parity: " + c); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in doc.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            uint seed = trace.GetProperty("seed").GetUInt32(); string c = $"{rules.Id}/{seed}";
            var state = new MatchState(rules, seed, [new("Player", true)]); var p = state.Players[0];
            var mode = new UnderlordsPlanning(state); var offers = new MatchOffers(state, new Hooks());
            var round = new RoundPosition(trace.GetProperty("stage").GetInt32(), trace.GetProperty("round").GetInt32());
            var before = trace.GetProperty("before");
            int streak = before.GetProperty("streak").GetInt32();
            for (int i = 0; i < Math.Abs(streak); i++) p.Economy.RecordOutcome(streak > 0, false);
            p.Economy.Credit(before.GetProperty("gold").GetInt32() - p.Economy.Gold);
            p.Globals.AddRange(Strings(before.GetProperty("globals"))); p.Shop.FreeRolls = before.GetProperty("rolls").GetInt32();
            p.Mechanics.UnderlordsFreeRoll = before.GetProperty("free").GetBoolean();
            var income = mode.Income(p, round, new(9, 3, 5, 2, 1)); var expectedIncome = trace.GetProperty("income");
            Check(income.Base == expectedIncome.GetProperty("base").GetInt32() && income.Streak == expectedIncome.GetProperty("streak").GetInt32() && income.Interest == 3 && income.Augment == 2 && income.Bonus == 1, c + " income");
            offers.Make(p, round); int index = 0;
            foreach (var snapshot in trace.GetProperty("snapshots").EnumerateArray())
            {
                if (index++ == 2) Check(offers.Reroll(p, round), c + " reroll");
                mode.RoundStart(p, round);
                Check(p.Economy.Gold == snapshot.GetProperty("gold").GetInt32() && p.Economy.Streak == snapshot.GetProperty("streak").GetInt32(), c + " gold/streak");
                Check(p.Shop.FreeRolls == snapshot.GetProperty("rolls").GetInt32() && p.Mechanics.UnderlordsFreeRoll == snapshot.GetProperty("free").GetBoolean(), c + " free rolls");
                var o = snapshot.GetProperty("offer");
                Check(o.ValueKind == JsonValueKind.Null ? p.Offer is null : p.Offer is not null && p.Offer.Kind == o.GetProperty("kind").GetString() && p.Offer.Options.SequenceEqual(Strings(o.GetProperty("options"))) && p.Offer.Rerolls == o.GetProperty("rerolls").GetInt32(), c + " offer");
                if (p.Offer?.Kind == "itemTier") Check(p.Offer.Tier == o.GetProperty("tier").GetInt32() && p.Offer.UnderlordsRolled == o.GetProperty("ulRolled").GetBoolean(), c + " tier/once");
            }
            var unit = state.CreateUnit(rules.Lines[0].Id, 1) with { Items = Strings(trace.GetProperty("items")) };
            p.Roster.AddToBench(unit); p.Items.AddRange(Strings(trace.GetProperty("inventory"))); mode.Combined(p, unit);
            Check(p.Roster.Bench[0]!.Items.SequenceEqual(Strings(trace.GetProperty("afterItems"))) && p.Items.SequenceEqual(Strings(trace.GetProperty("afterInventory"))), c + " combined item order/cap");
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), c + " RNG");
        }
        Console.WriteLine($"Underlords planning parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} traces."); return checks;
    }
}

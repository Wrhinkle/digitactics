using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class ArcaneAugmentParity
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
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/arcane-augments.json.gz"));
        using var zip = new GZipStream(input, CompressionMode.Decompress);
        using var doc = JsonDocument.Parse(zip);
        int checks = 0;
        void Check(bool ok, string c) { checks++; if (!ok) throw new InvalidOperationException("Arcane augment parity: " + c); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in doc.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            int n = trace.GetProperty("n").GetInt32(); bool human = trace.GetProperty("human").GetBoolean();
            string id = trace.GetProperty("id").GetString()!, c = $"{rules.Id}/{id}/{n}";
            var state = new MatchState(rules, (uint)(n + 1), [new("Player", human)]);
            var p = state.Players[0]; var mode = new ArcanePlanning(state);
            p.Economy.Credit(17 - p.Economy.Gold); p.TeamBonus = n % 3;
            var m = p.Mechanics.Arcane; var initial = trace.GetProperty("before").GetProperty("mech");
            m.Shimmer = initial.GetProperty("arcShimmer").GetInt32(); m.MarketsQueued = initial.GetProperty("arcMarkets").GetInt32(); m.ExtraAnomalies = initial.GetProperty("arcExtra").GetInt32(); m.Anomaly = initial.GetProperty("arcAnomaly").GetString();
            p.Items.AddRange(Strings(trace.GetProperty("before").GetProperty("items")));
            foreach (var entry in trace.GetProperty("added").EnumerateArray())
            {
                var e = entry.GetProperty("unit");
                var unit = state.CreateUnit(e.GetProperty("line").GetString()!, 0) with { Items = Strings(e.GetProperty("items")), Mods = e.GetProperty("mods").Clone(), BonusTraits = e.TryGetProperty("bonusTraits", out var traits) ? traits.Deserialize<Dictionary<string, int>>() : null };
                int slot = p.Roster.FreeBenchSlot; p.Roster.AddToBench(unit);
                if (entry.GetProperty("board").GetBoolean()) p.Roster.Move(UnitSpot.Bench(slot), UnitSpot.Board(entry.GetProperty("col").GetInt32(), 4), false, () => 100);
            }
            foreach (var pair in trace.GetProperty("before").GetProperty("pool").EnumerateObject()) state.Pool.Take(pair.Name, state.Pool.Remaining(pair.Name) - pair.Value.GetInt32());
            int combines = 0;
            void Combine(MatchPlayer player) { combines++; player.Roster.Combine(human && trace.GetProperty("phase").GetString() == "combat", player.GiveItem, onCombine: (u, sources) => mode.Combined(player, u, sources)); player.Shop.ClearMaxed(); }
            p.Augments.Add(id); mode.AugmentPicked(p, id, Combine); Combine(p);
            void Mechanic(JsonElement e)
            {
                Check(m.Shimmer == e.GetProperty("arcShimmer").GetInt32() && m.MarketsQueued == e.GetProperty("arcMarkets").GetInt32() && m.SyndicateAugment == e.GetProperty("arcSynAug").GetBoolean() && m.MarketPlus == e.GetProperty("arcMarketPlus").GetBoolean(), c + " Shimmer/market");
                Check(m.ExtraAnomalies == e.GetProperty("arcExtra").GetInt32() && m.PendingAnomaly == e.GetProperty("arcPendingAnom").GetBoolean() && m.Anomaly == e.GetProperty("arcAnomaly").GetString(), c + " anomaly queue");
            }
            Mechanic(trace.GetProperty("picked").GetProperty("mech"));
            Check(mode.Income(p, new(5,3,2,1)).Augment == trace.GetProperty("income").GetProperty("augment").GetInt32(), c + " income");
            mode.RoundStart(p, new(3,1), new(state, new Hooks()));
            var offer = trace.GetProperty("offer"); Check(offer.ValueKind == JsonValueKind.Null ? p.Offer is null : p.Offer is not null && p.Offer.Kind == offer.GetProperty("kind").GetString() && p.Offer.Options.SequenceEqual(Strings(offer.GetProperty("options"))), c + " queued offer");
            var expected = trace.GetProperty("after");
            Mechanic(expected.GetProperty("mech"));
            Check(p.Health == expected.GetProperty("hp").GetInt32() && p.Shop.FreeRolls == expected.GetProperty("rolls").GetInt32(), c + " health/rolls");
            Check(combines == trace.GetProperty("combines").GetInt32(), c + " combine timing");
            Check(p.Economy.Gold == expected.GetProperty("gold").GetInt32() && p.TeamBonus == expected.GetProperty("teamBonus").GetInt32(), c + " gold/capacity");
            Check(p.Items.SequenceEqual(Strings(expected.GetProperty("items"))), c + " inventory");
            void Unit(PlanningUnit? unit, JsonElement e)
            {
                Check(e.ValueKind == JsonValueKind.Null ? unit is null : unit is not null && unit.Id == e.GetProperty("uid").GetInt32() && unit.Line == e.GetProperty("line").GetString() && unit.Stage == e.GetProperty("stage").GetInt32() && unit.Items.SequenceEqual(Strings(e.GetProperty("items"))), c + " unit");
                if (unit is not null) Check(e.TryGetProperty("mods", out var expectedMods) ? unit.Mods is { } mods && expectedMods.EnumerateObject().All(prop => mods.TryGetProperty(prop.Name, out var value) && value.ToString() == prop.Value.ToString()) && mods.EnumerateObject().Count() == expectedMods.EnumerateObject().Count() : unit.Mods is null, c + " carried modifiers");
                if (unit is not null) Check(e.TryGetProperty("bonusTraits", out var traits) ? unit.BonusTraits is not null && unit.BonusTraits.OrderBy(x => x.Key).SequenceEqual(traits.Deserialize<Dictionary<string, int>>()!.OrderBy(x => x.Key)) : unit.BonusTraits is null, c + " bonus traits");
            }
            var board = expected.GetProperty("board").EnumerateObject().ToArray(); Check(board.Length == p.Roster.Board.Count, c + " board count");
            for (int i = 0; i < board.Length; i++) { var entry = p.Roster.Board[i]; Check($"{entry.Spot.Column},{entry.Spot.Row}" == board[i].Name, c + " board order"); Unit(entry.Unit, board[i].Value); }
            for (int i = 0; i < 9; i++) Unit(p.Roster.Bench[i], expected.GetProperty("bench")[i]);
            Check(p.Shop.Slots.SequenceEqual(expected.GetProperty("shop").EnumerateArray().Select(e => e.GetString())), c + " shop");
            foreach (var pair in expected.GetProperty("pool").EnumerateObject()) Check(state.Pool.Remaining(pair.Name) == pair.Value.GetInt32(), c + " pool");
            Check(state.CreateUnit(rules.Lines[0].Id, 0).Id == trace.GetProperty("nextId").GetInt32(), c + " next ID");
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), c + " RNG");
        }
        Console.WriteLine($"Arcane augment parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} traces."); return checks;
    }
}

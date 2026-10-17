using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class ColiseumShopParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/coliseum-shop.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool condition, string context) { checks++; if (!condition) throw new InvalidOperationException("Coliseum shop parity: " + context); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        void Unit(PlanningUnit? u, JsonElement expected, string context)
        {
            Check((u is null) == (expected.ValueKind == JsonValueKind.Null), context + " presence");
            if (u is null) return;
            Check(u.Id == expected.GetProperty("uid").GetInt32() && u.Line == expected.GetProperty("line").GetString() && u.Stage == expected.GetProperty("stage").GetInt32(), context + " unit");
            Check(u.Items.SequenceEqual(Strings(expected.GetProperty("items"))), context + " items");
            Check((u.Mods is null) == !expected.TryGetProperty("mods", out var mods), context + " modifier presence");
            if (u.Mods is { } actualMods) Check(JsonElement.DeepEquals(actualMods, mods), context + " modifiers");
            Check((u.BonusTraits is null) == !expected.TryGetProperty("bonusTraits", out var bonus), context + " bonus presence");
            if (u.BonusTraits is not null) Check(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(u.BonusTraits), bonus), context + " bonus traits");
        }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var state = new MatchState(rules, trace.GetProperty("seed").GetUInt32(), [new("Player", trace.GetProperty("human").GetBoolean())]);
            var p = state.Players[0]; p.Augments.Add("col_a_arena"); p.Economy.Credit(50000);
            if (rules.Economy!.Xp is not null) p.Economy.AddXp(10000);
            else
            {
                int gold = p.Economy.Gold;
                p.Economy.BeginRound(10, 1);
                p.Economy.Spend(p.Economy.Gold - gold);
            }
            var commands = new PlanningShopCommands(new ColiseumPlanning(state));
            var offers = new MatchOffers(state, new UnusedOffers());
            int step = 0;
            foreach (var sample in trace.GetProperty("steps").EnumerateArray())
            {
                string context = $"{rules.Id}/{trace.GetProperty("human")}/{trace.GetProperty("seed")}/{step++}";
                var phase = Enum.Parse<MatchPhase>(sample.GetProperty("phase").GetString()!, true);
                var edit = sample.GetProperty("edit");
                if (edit.ValueKind != JsonValueKind.Null)
                {
                    var unit = p.Roster.Entries.Single(e => e.Unit.Id == edit.GetProperty("uid").GetInt32()).Unit;
                    p.Roster.UpdateUnit(unit with { Mods = edit.GetProperty("mods").Clone(), Items = Strings(edit.GetProperty("items")),
                        BonusTraits = edit.GetProperty("bonusTraits").EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetInt32()) });
                }
                if (sample.GetProperty("fill").GetBoolean()) { p.Items.Clear(); p.Items.AddRange(Enumerable.Repeat(rules.CoreItems[0], rules.Loot!.ItemCap)); }
                var result = sample.GetProperty("result");
                switch (sample.GetProperty("kind").GetString())
                {
                    case "refresh": p.Shop.Refresh(); break;
                    case "buy": Check(commands.Buy(p, sample.GetProperty("arg").GetInt32(), phase) == result.GetBoolean(), context + " buy"); break;
                    case "sell": Check(commands.Sell(p, sample.GetProperty("arg").GetInt32(), phase) == result.GetBoolean(), context + " sell"); break;
                    case "choose": Check(offers.Choose(p, sample.GetProperty("arg").GetInt32(), phase == MatchPhase.Combat && p.IsHuman) == result.GetString(), context + " choose"); break;
                    case "move":
                        var arg = sample.GetProperty("arg"); int from = arg.GetProperty("from").GetInt32();
                        if (from >= 0) Check((p.Roster.Move(UnitSpot.Bench(from), UnitSpot.Board(arg.GetProperty("col").GetInt32(), arg.GetProperty("row").GetInt32()), phase == MatchPhase.Combat && p.IsHuman, () => 100) == MoveResult.Moved) == (result.ValueKind == JsonValueKind.True), context + " move");
                        break;
                }
                var after = sample.GetProperty("after");
                Check(p.Economy.Gold == after.GetProperty("gold").GetInt32(), context + " gold");
                Check(p.Items.SequenceEqual(Strings(after.GetProperty("items"))), context + " inventory");
                Check(p.Mechanics.ColiseumCores == after.GetProperty("cores").GetInt32(), context + " cores");
                Check(p.Mechanics.ColiseumFormUnitId == (after.GetProperty("formUid").ValueKind == JsonValueKind.Null ? null : after.GetProperty("formUid").GetInt32()), context + " form target");
                var offer = after.GetProperty("offer");
                Check((p.Offer is null) == (offer.ValueKind == JsonValueKind.Null), context + " offer presence");
                if (p.Offer is { } o) Check(o.Kind == offer.GetProperty("kind").GetString() && o.Rerolls == offer.GetProperty("rerolls").GetInt32() && o.Options.SequenceEqual(Strings(offer.GetProperty("options"))), context + " offer");
                Check(p.Shop.Slots.SequenceEqual(after.GetProperty("shop").EnumerateArray().Select(x => x.GetString())), context + " shop");
                foreach (var pair in after.GetProperty("pool").EnumerateObject()) Check(state.Pool.Remaining(pair.Name) == pair.Value.GetInt32(), context + " pool " + pair.Name);
                var board = after.GetProperty("board"); Check(p.Roster.Board.Count == board.GetArrayLength(), context + " board size");
                for (int i = 0; i < board.GetArrayLength(); i++)
                {
                    var entry = p.Roster.Board[i]; Check($"{entry.Spot.Column},{entry.Spot.Row}" == board[i].GetProperty("spot").GetString(), context + " board order");
                    Unit(entry.Unit, board[i].GetProperty("u"), context);
                }
                for (int i = 0; i < 9; i++) Unit(p.Roster.Bench[i], after.GetProperty("bench")[i], context);
            }
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), rules.Id + " RNG");
        }
        Console.WriteLine($"Coliseum shop parity: {checks:N0} checks.");
        return checks;
    }
    private sealed class UnusedOffers : IMatchOfferHooks
    {
        public string[] Armory(MatchPlayer player, RoundPosition round) => throw new InvalidOperationException();
        public string[] CustomOptions(MatchPlayer player, RoundPosition round, OfferDefinition definition, IReadOnlyList<string> exclude) => throw new InvalidOperationException();
        public void CustomChosen(MatchPlayer player, PlayerOffer offer, string id) => throw new InvalidOperationException();
        public void AugmentPicked(MatchPlayer player, string id) => throw new InvalidOperationException();
        public void PowerPicked(MatchPlayer player, string id) => throw new InvalidOperationException();
    }
}

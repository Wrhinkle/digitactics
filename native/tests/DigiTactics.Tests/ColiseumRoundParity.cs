using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class ColiseumRoundParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/coliseum-rounds.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool condition, string context) { checks++; if (!condition) throw new InvalidOperationException("Coliseum round parity: " + context); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        void Snapshot(MatchState state, JsonElement expected, string context)
        {
            for (int index = 0; index < state.Players.Count; index++)
            {
                var p = state.Players[index]; var e = expected[index]; var m = p.Mechanics; var g = m.ColiseumGambit; var pirate = m.ColiseumPirate;
                Check(p.Economy.Gold == e.GetProperty("gold").GetInt32() && p.Health == e.GetProperty("hp").GetInt32() && p.Economy.Level == e.GetProperty("level").GetInt32() && p.Economy.Xp == e.GetProperty("xp").GetInt32(), context + " economy");
                Check(p.Shop.FreeRolls == e.GetProperty("rolls").GetInt32(), context + " rerolls");
                Check(p.Items.SequenceEqual(Strings(e.GetProperty("items"))), context + " items");
                Check(p.Shop.Slots.SequenceEqual(e.GetProperty("shop").EnumerateArray().Select(x => x.GetString())), context + " shop");
                Check(m.ColiseumCores == e.GetProperty("cores").GetInt32(), context + " cores");
                Check(m.ColiseumFormUnitId == (e.GetProperty("formUid").ValueKind == JsonValueKind.Null ? null : e.GetProperty("formUid").GetInt32()), context + " form target");
                Check(g.Gems == e.GetProperty("gems").GetDouble() && g.Level == e.GetProperty("gLv").GetInt32() && g.Count == e.GetProperty("gCount").GetInt32() && g.Ready == e.GetProperty("ready").GetBoolean() && g.DoubleDown == e.GetProperty("dd").GetBoolean(), context + " gambit");
                Check(pirate.Count == e.GetProperty("pirateCount").GetInt32() && pirate.LastReward == e.GetProperty("pirateLast").GetString() && pirate.AbilityPower == e.GetProperty("pirateAP").GetDouble(), context + " bounty");
                var buffs = e.GetProperty("buffs"); Check((p.NextBuffs is null) == (buffs.ValueKind == JsonValueKind.Null), context + " buffs presence");
                if (p.NextBuffs is { } b) Check(b.Armor == (buffs.TryGetProperty("armor", out var armor) ? armor.GetDouble() : 0) && b.AttackSpeed == (buffs.TryGetProperty("as", out var speed) ? speed.GetDouble() : 0) && b.Mana == (buffs.TryGetProperty("mana", out var mana) ? mana.GetDouble() : 0), context + " buffs");
                var offer = e.GetProperty("offer"); Check((p.Offer is null) == (offer.ValueKind == JsonValueKind.Null), context + " offer presence");
                if (p.Offer is { } o) Check(o.Kind == offer.GetProperty("kind").GetString() && o.Rerolls == offer.GetProperty("rerolls").GetInt32() && o.Options.SequenceEqual(Strings(offer.GetProperty("options"))), context + " offer");
                var units = p.Roster.Entries.ToArray(); var expUnits = e.GetProperty("units"); Check(units.Length == expUnits.GetArrayLength(), context + " units");
                for (int j = 0; j < units.Length; j++)
                {
                    var u = units[j].Unit; var eu = expUnits[j];
                    Check(u.Id == eu.GetProperty("uid").GetInt32() && u.Line == eu.GetProperty("line").GetString(), context + " ordered unit");
                    Check((u.Mods is null) == !eu.TryGetProperty("mods", out var mods), context + " modifier presence");
                    if (u.Mods is { } um) Check(JsonElement.DeepEquals(um, mods), context + " modifiers");
                    Check((u.BonusTraits is null) == !eu.TryGetProperty("bonusTraits", out var bonus), context + " bonus presence");
                    if (u.BonusTraits is not null) Check(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(u.BonusTraits), bonus), context + " bonus traits");
                }
            }
        }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var state = new MatchState(rules, trace.GetProperty("seed").GetUInt32(), [new("Player", trace.GetProperty("human").GetBoolean()), new("Other", true)]);
            foreach (var player in state.Players) player.Augments.Add("col_a_arena");
            var p = state.Players[0]; int col = 0;
            foreach (var spec in trace.GetProperty("setup").EnumerateArray())
            {
                var u = state.CreateUnit(spec.GetProperty("line").GetString()!, spec.GetProperty("stage").GetInt32());
                int slot = p.Roster.FreeBenchSlot; p.Roster.AddToBench(u);
                if (spec.GetProperty("board").GetBoolean()) p.Roster.Move(UnitSpot.Bench(slot), UnitSpot.Board(col++, 4), false, () => 100);
            }
            p.Mechanics.ColiseumCores = 3; p.Economy.RecordOutcome(false, false);
            var offers = new MatchOffers(state, new OfferHooks()); var rounds = new MatchRounds(state, offers, new RoundHooks(state), phase: MatchPhase.Result);
            foreach (var sample in trace.GetProperty("steps").EnumerateArray())
            {
                int i = sample.GetProperty("i").GetInt32(); p.Health = i % 2 == 0 ? 25 : 8;
                var g = p.Mechanics.ColiseumGambit; g.Gems = new[] { 0, 24, 45, 80 }[i % 4]; g.Level = i % 4; g.Ready = i % 3 != 0;
                p.Items.Clear(); p.Items.AddRange(Enumerable.Repeat(rules.CoreItems[0], i % 4 == 0 ? rules.Loot!.ItemCap : i % 4 == 1 ? rules.Loot!.ItemCap - 1 : 0));
                if (sample.GetProperty("reset").GetBoolean()) foreach (var e in p.Roster.Entries.ToArray())
                    if (rules.Forms!.ContainsKey(e.Unit.Line)) p.Roster.UpdateUnit(e.Unit with { Mods = JsonSerializer.SerializeToElement(new { pu = "warp" }), BonusTraits = [] });
                if (sample.GetProperty("buffs").GetBoolean()) p.NextBuffs = new(AttackSpeed: .1, Mana: 5);
                string context = $"{rules.Id}/{trace.GetProperty("human")}/{trace.GetProperty("seed")}/{i}";
                Check(rounds.Advance(), context + " advance");
                Check(rounds.Position.Stage == sample.GetProperty("stage").GetInt32() && rounds.Position.Round == sample.GetProperty("round").GetInt32(), context + " round");
                Snapshot(state, sample.GetProperty("after"), context + " after start");
                string? chosen = p.Offer?.Kind.StartsWith("col_", StringComparison.Ordinal) == true ? offers.Choose(p, sample.GetProperty("choice").GetInt32(), false) : null;
                Check(chosen == sample.GetProperty("chosen").GetString(), context + " choice");
                Snapshot(state, sample.GetProperty("final"), context + " after choice");
            }
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), rules.Id + " RNG");
        }
        Console.WriteLine($"Coliseum round-start parity: {checks:N0} checks.");
        return checks;
    }
    private sealed class RoundHooks(MatchState state) : IMatchRoundHooks
    {
        private readonly ColiseumPlanning coliseum = new(state);
        public void RoundStart(MatchPlayer player, RoundPosition round) => coliseum.RoundStart(player, round);
        public IncomeBreakdown Income(MatchPlayer player, RoundPosition round, IncomeBreakdown income) => income;
        public void ResetTransfers(MatchPlayer player) => throw new InvalidOperationException();
        public void RankUp(MatchPlayer player, RoundPosition round) => throw new InvalidOperationException();
        public void StartCarousel(RoundPosition round) { }
        public void PlanAi(RoundPosition round) { }
    }
    private sealed class OfferHooks : IMatchOfferHooks
    {
        public string[] Armory(MatchPlayer player, RoundPosition round) => throw new InvalidOperationException();
        public string[] CustomOptions(MatchPlayer player, RoundPosition round, OfferDefinition definition, IReadOnlyList<string> exclude) => throw new InvalidOperationException();
        public void CustomChosen(MatchPlayer player, PlayerOffer offer, string id) => throw new InvalidOperationException();
        public void AugmentPicked(MatchPlayer player, string id) => throw new InvalidOperationException();
        public void PowerPicked(MatchPlayer player, string id) => throw new InvalidOperationException();
    }
}

using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class UnitLifecycleParity
{
    private sealed class SettlementHooks : IMatchSettlementHooks
    {
        public PveRewardSummary Reward(MatchPlayer p, bool won) => throw new NotSupportedException();
        public int RoundEnd(RoundEndContext c) => throw new NotSupportedException();
        public void AfterCombat(MatchPlayer p) { }
        public void Eliminated(MatchPlayer p) => throw new NotSupportedException();
    }
    private sealed class OfferHooks : IMatchOfferHooks
    {
        public string[] Armory(MatchPlayer p, RoundPosition r) => throw new NotSupportedException();
        public string[] CustomOptions(MatchPlayer p, RoundPosition r, OfferDefinition d, IReadOnlyList<string> e) => throw new NotSupportedException();
        public void AugmentPicked(MatchPlayer p, string id) { if (id != "duo_proud") throw new NotSupportedException(); }
        public void PowerPicked(MatchPlayer p, string id) => throw new NotSupportedException();
        public void CustomChosen(MatchPlayer p, PlayerOffer o, string id) => throw new NotSupportedException();
    }

    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/unit-lifecycle.json.gz"));
        using var zip = new GZipStream(input, CompressionMode.Decompress);
        using var document = JsonDocument.Parse(zip);
        int checks = 0;
        void Check(bool ok, string context) { checks++; if (!ok) throw new InvalidOperationException("Unit lifecycle parity: " + context); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in document.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            string route = trace.GetProperty("route").GetString()!, context = $"{rules.Id}/{route}/{trace.GetProperty("duo")}/{trace.GetProperty("n")}";
            var phase = Enum.Parse<MatchPhase>(trace.GetProperty("phase").GetString()!, true);
            var round = new RoundPosition(trace.GetProperty("stage").GetInt32(), 1);
            var state = new MatchState(rules, trace.GetProperty("seed").GetUInt32(),
                Enumerable.Range(0, 8).Select(id => new MatchSeat($"Player {id}", trace.GetProperty("human").GetBoolean())).ToArray(), trace.GetProperty("duo").GetBoolean());
            var lifecycle = new MatchUnitLifecycle(state, () => round);
            if (route == "carousel") lifecycle.Carousel.Start(round.Stage, phase);
            foreach (var added in trace.GetProperty("added").EnumerateArray())
            {
                var p = state.Players[added.GetProperty("player").GetInt32()]; var e = added.GetProperty("unit");
                var unit = state.CreateUnit(e.GetProperty("line").GetString()!, e.GetProperty("stage").GetInt32()) with
                {
                    Items = Strings(e.GetProperty("items")), Mods = e.GetProperty("mods").Clone(),
                    BonusTraits = e.TryGetProperty("bonusTraits", out var traits) ? traits.Deserialize<Dictionary<string, int>>() : null
                };
                Check(unit.Id == e.GetProperty("uid").GetInt32(), context + " setup ID");
                int slot = p.Roster.FreeBenchSlot; p.Roster.AddToBench(unit);
                if (added.GetProperty("board").GetBoolean()) p.Roster.Move(UnitSpot.Bench(slot), UnitSpot.Board(0, 4), false, () => 100);
            }
            foreach (var p in state.Players)
            {
                p.Augments.Add("duo_proud"); p.Economy.Credit(100 - p.Economy.Gold);
                p.Items.AddRange(Strings(trace.GetProperty("before")[p.Id].GetProperty("items")));
            }
            var source = state.Players[0]; var player = route == "portal" ? lifecycle.Portal.Partner(source)! : route == "carousel" ? lifecycle.Carousel.Turn! : source;
            string line = trace.GetProperty("line").GetString()!;
            switch (route)
            {
                case "direct": lifecycle.Combine(player, phase); break;
                case "carousel": lifecycle.Carousel.Pick(player, 0, phase); break;
                case "settlement": new MatchSettlement(state, round.Stage, new SettlementHooks()).Settle([]); break;
                case "offer":
                    typeof(MatchPlayer).GetProperty(nameof(MatchPlayer.Offer))!.SetValue(player, new PlayerOffer("augment", ["duo_proud"], 0, null));
                    new MatchOffers(state, new OfferHooks()).Choose(player, 0, phase == MatchPhase.Combat && player.IsHuman); break;
                case "loot":
                    state.Pool.Take(line); player.Orbs.Add(new(-1, "blue", 0, 0, 0, [], line));
                    new MatchLoot(state).Collect(player, -1, phase == MatchPhase.Combat && player.IsHuman); break;
                case "shop":
                    Check(player.Shop.ReplaceOffer(0, line), context + " replace offer");
                    lifecycle.Shop.Buy(player, 0, phase); break;
                case "portal": lifecycle.Portal.Send(source, new(Uid: trace.GetProperty("send").GetInt32()), phase, "pvp"); break;
                case "copy": lifecycle.Items.UseConsumable(player, 0, player.Roster.Entries.First().Unit.Id, phase); break;
                default: throw new NotSupportedException(route);
            }
            foreach (var expected in trace.GetProperty("after").EnumerateArray())
            {
                var p = state.Players[expected.GetProperty("id").GetInt32()]; string c = context + "/player" + p.Id;
                Check(p.Economy.Gold == expected.GetProperty("gold").GetInt32(), c + " gold");
                Check(p.Items.SequenceEqual(Strings(expected.GetProperty("items"))), c + " inventory");
                Check(p.Shop.Slots.SequenceEqual(expected.GetProperty("shop").EnumerateArray().Select(x => x.GetString())), c + " shop");
                Check(p.ProudPartnerRound == (expected.GetProperty("proudRound").ValueKind == JsonValueKind.Null ? null : expected.GetProperty("proudRound").GetInt32()) &&
                    p.ProudPartnerCount == expected.GetProperty("proudCount").GetInt32(), c + " partner rewards");
                Check(p.Mechanics.ColiseumCores == expected.GetProperty("cores").GetInt32(), c + " cores");
                void Unit(PlanningUnit? unit, JsonElement e)
                {
                    Check((unit is null) == (e.ValueKind == JsonValueKind.Null), c + " unit presence"); if (unit is null) return;
                    Check(unit.Id == e.GetProperty("uid").GetInt32() && unit.Line == e.GetProperty("line").GetString() &&
                        unit.Stage == e.GetProperty("stage").GetInt32() && unit.Items.SequenceEqual(Strings(e.GetProperty("items"))), c + " unit");
                    bool hasMods = e.TryGetProperty("mods", out var mods) && mods.ValueKind != JsonValueKind.Null;
                    Check((unit.Mods is not null) == hasMods, c + " modifier presence");
                    if (unit.Mods is { } actual) Check(JsonElement.DeepEquals(actual, mods), c + " modifiers");
                    bool hasTraits = e.TryGetProperty("bonusTraits", out var traits) && traits.ValueKind != JsonValueKind.Null;
                    Check((unit.BonusTraits is not null) == hasTraits, c + " trait presence");
                    if (unit.BonusTraits is not null) Check(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(unit.BonusTraits), traits), c + " traits");
                }
                var board = expected.GetProperty("board").EnumerateObject().ToArray();
                Check(board.Length == p.Roster.Board.Count, c + " board count");
                for (int i = 0; i < board.Length; i++)
                { Check($"{p.Roster.Board[i].Spot.Column},{p.Roster.Board[i].Spot.Row}" == board[i].Name, c + " board order"); Unit(p.Roster.Board[i].Unit, board[i].Value); }
                for (int i = 0; i < 9; i++) Unit(p.Roster.Bench[i], expected.GetProperty("bench")[i]);
            }
            foreach (var pair in trace.GetProperty("pool").EnumerateObject()) Check(state.Pool.Remaining(pair.Name) == pair.Value.GetInt32(), context + " pool " + pair.Name);
            Check(state.CreateUnit(line, 0).Id == trace.GetProperty("nextId").GetInt32(), context + " next ID");
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), context + " RNG");
        }
        Console.WriteLine($"Unit lifecycle parity: {checks:N0} checks across {document.RootElement.GetArrayLength()} traces.");
        return checks;
    }
}

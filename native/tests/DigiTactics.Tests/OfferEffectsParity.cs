using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class OfferEffectsParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/offer-effects.json.gz"));
        using var zip = new GZipStream(input, CompressionMode.Decompress);
        using var document = JsonDocument.Parse(zip);
        int checks = 0;
        void Check(bool ok, string context) { checks++; if (!ok) throw new InvalidOperationException("Offer effects parity: " + context); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in document.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            string context = $"{rules.Id}/{trace.GetProperty("id")}/{trace.GetProperty("duo")}/{trace.GetProperty("n")}";
            var phase = Enum.Parse<MatchPhase>(trace.GetProperty("phase").GetString()!, true);
            var round = new RoundPosition(3, 1);
            var state = new MatchState(rules, trace.GetProperty("seed").GetUInt32(),
                Enumerable.Range(0, 8).Select(id => new MatchSeat($"Player {id}", trace.GetProperty("human").GetBoolean())).ToArray(), trace.GetProperty("duo").GetBoolean());
            var lifecycle = new MatchUnitLifecycle(state, () => round);
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
                if (added.GetProperty("board").GetBoolean()) p.Roster.Move(UnitSpot.Bench(slot), UnitSpot.Board(added.GetProperty("col").GetInt32(), 4), false, () => 100);
            }
            foreach (var p in state.Players)
            {
                p.Augments.UnionWith(Strings(trace.GetProperty("before")[p.Id].GetProperty("augs"))); p.Economy.Credit(17 - p.Economy.Gold);
                p.Items.AddRange(Strings(trace.GetProperty("before")[p.Id].GetProperty("items")));
            }
            foreach (var pair in trace.GetProperty("poolBefore").EnumerateObject()) state.Pool.Take(pair.Name, state.Pool.Remaining(pair.Name) - pair.Value.GetInt32());
            var player = state.Players[0];
            typeof(MatchPlayer).GetProperty(nameof(MatchPlayer.Offer))!.SetValue(player,
                new PlayerOffer(trace.GetProperty("kind").GetString()!, [trace.GetProperty("id").GetString()!], 0, null));
            var offers = new MatchOffers(state, lifecycle, () => phase);
            Check(offers.Choose(player, 0, phase == MatchPhase.Combat && player.IsHuman) == trace.GetProperty("result").GetString(), context + " choice result");
            Check(player.Offer is null, context + " offer consumed");
            foreach (var expected in trace.GetProperty("after").EnumerateArray())
            {
                var p = state.Players[expected.GetProperty("id").GetInt32()]; string c = context + "/player" + p.Id;
                Check(p.Economy.Gold == expected.GetProperty("gold").GetInt32(), c + " gold");
                Check(p.Health == expected.GetProperty("hp").GetInt32() && p.Shop.FreeRolls == expected.GetProperty("rolls").GetInt32() && p.TeamBonus == expected.GetProperty("teamBonus").GetInt32(), c + " health/rolls/capacity");
                var power = expected.GetProperty("power");
                Check(power.ValueKind == JsonValueKind.Null ? p.Power is null : p.Power is not null && p.Power.Id == power.GetProperty("id").GetString() && p.Power.Rank == power.GetProperty("rank").GetInt32(), c + " power");
                var m = p.Mechanics.Arcane;
                Check(m.Shimmer == expected.GetProperty("shimmer").GetInt32() && m.MarketsQueued == expected.GetProperty("markets").GetInt32() && m.SyndicateAugment == expected.GetProperty("syn").GetBoolean() && m.MarketPlus == expected.GetProperty("marketPlus").GetBoolean(), c + " Shimmer/market");
                Check(m.ExtraAnomalies == expected.GetProperty("extra").GetInt32() && m.PendingAnomaly == expected.GetProperty("pending").GetBoolean() && p.Mechanics.Reckoning.EarlyBlessing == expected.GetProperty("early").GetBoolean(), c + " pending offers");
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
                    Check(unit.NoSell == (e.TryGetProperty("noSell", out var noSell) && noSell.GetBoolean()) && unit.NoItems == (e.TryGetProperty("noItems", out var noItems) && noItems.GetBoolean()), c + " unit flags");
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
            Check(state.CreateUnit(rules.Lines[0].Id, 0).Id == trace.GetProperty("nextId").GetInt32(), context + " next ID");
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), context + " RNG");
        }
        Console.WriteLine($"Offer effects parity: {checks:N0} checks across {document.RootElement.GetArrayLength()} traces.");
        return checks;
    }
}

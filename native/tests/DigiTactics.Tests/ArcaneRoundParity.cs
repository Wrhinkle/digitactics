using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class ArcaneRoundParity
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
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/arcane-rounds.json.gz"));
        using var zip = new GZipStream(input, CompressionMode.Decompress); using var doc = JsonDocument.Parse(zip);
        int checks = 0;
        void Check(bool ok, string c) { checks++; if (!ok) throw new InvalidOperationException("Arcane round parity: " + c); }
        static int Int(JsonElement e, string k) => e.GetProperty(k).GetInt32();
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in doc.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString()); int n = Int(trace, "n");
            var state = new MatchState(rules, (uint)(n + 1), [new("Player", n % 2 == 0)]) { ArcanePortal = trace.GetProperty("portal").GetString() };
            var p = state.Players[0]; var mode = new ArcanePlanning(state); var offers = new MatchOffers(state, new Hooks()); var m = p.Mechanics.Arcane;
            string c = $"{rules.Id}/{state.ArcanePortal}/{n}"; var before = trace.GetProperty("before"); var initial = before.GetProperty("mech");
            p.Shop.FreeRolls = Int(before, "rolls"); p.Augments.UnionWith(Strings(before.GetProperty("augs")));
            m.MarketsQueued = Int(initial, "arcMarkets"); m.MarketsOpened = Int(initial, "arcMarketsOpened"); m.MarketPlus = initial.GetProperty("arcMarketPlus").GetBoolean(); m.SyndicateLevel = Int(initial, "arcSynLv"); m.PendingAnomaly = initial.GetProperty("arcPendingAnom").GetBoolean(); m.JunkRounds = Int(initial, "arcJunkT"); m.Junk.AddRange(Strings(initial.GetProperty("arcJunk"))); m.Seen.AddRange(Strings(initial.GetProperty("arcSeen")));
            if (n % 4 != 0) { var u = state.CreateUnit("arc_chaosdromon", n % 3); p.Roster.AddToBench(u); if (n % 4 == 1) p.Roster.Move(UnitSpot.Bench(0), UnitSpot.Board(0, 4), false, () => 100); }
            void Snapshot(JsonElement e)
            {
                var em = e.GetProperty("mech"); Check(p.Shop.FreeRolls == Int(e, "rolls"), c + " rolls");
                Check(m.MarketsQueued == Int(em, "arcMarkets") && m.MarketsOpened == Int(em, "arcMarketsOpened"), c + " market queue");
                Check(m.PendingAnomaly == em.GetProperty("arcPendingAnom").GetBoolean() && m.JunkRounds == Int(em, "arcJunkT"), c + " deferred progress");
                Check(m.Seen.SequenceEqual(Strings(em.GetProperty("arcSeen"))) && m.Junk.SequenceEqual(Strings(em.GetProperty("arcJunk"))), c + " seen/junk");
                var o = e.GetProperty("offer"); Check(o.ValueKind == JsonValueKind.Null ? p.Offer is null : p.Offer is not null && p.Offer.Kind == o.GetProperty("kind").GetString() && p.Offer.Rerolls == Int(o, "rerolls") && p.Offer.Options.SequenceEqual(Strings(o.GetProperty("options"))), c + " offer");
            }
            foreach (var step in trace.GetProperty("steps").EnumerateArray())
            {
                var round = new RoundPosition(Int(step, "stage"), Int(step, "round")); offers.Make(p, round);
                mode.RoundStart(p, round, offers); Snapshot(step.GetProperty("first"));
                mode.RoundStart(p, round, offers); Snapshot(step.GetProperty("second"));
            }
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), c + " RNG");
        }
        Console.WriteLine($"Arcane round parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} traces."); return checks;
    }
}

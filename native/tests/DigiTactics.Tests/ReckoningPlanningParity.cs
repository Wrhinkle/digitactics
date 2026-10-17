using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class ReckoningPlanningParity
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
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/reckoning-planning.json.gz"));
        using var zip = new GZipStream(input, CompressionMode.Decompress); using var doc = JsonDocument.Parse(zip);
        var json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true }; int checks = 0;
        void Check(bool ok, string c) { checks++; if (!ok) throw new InvalidOperationException("Reckoning planning parity: " + c); }
        static int Int(JsonElement e, string k) => e.GetProperty(k).GetInt32();
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in doc.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString()); int n = Int(trace, "n"); string id = trace.GetProperty("id").GetString()!, c = $"{rules.Id}/{id}/{n}";
            var state = new MatchState(rules, (uint)(n + 1), [new("Player", trace.GetProperty("human").GetBoolean())]); var p = state.Players[0]; var mode = new ReckoningPlanning(state); var offers = new MatchOffers(state, new Hooks()); var m = p.Mechanics.Reckoning;
            var before = trace.GetProperty("before"); var initial = before.GetProperty("mech");
            p.Economy.Credit(Int(before, "gold") - p.Economy.Gold); p.Health = Int(before, "hp"); p.Shop.FreeRolls = Int(before, "rolls"); p.Items.AddRange(Strings(before.GetProperty("items")));
            m.Blessed = initial.GetProperty("blessed").GetBoolean(); m.EarlyBlessing = initial.GetProperty("blessEarly").GetBoolean(); m.Eggs.AddRange(initial.GetProperty("eggs").Deserialize<ReckoningEgg[]>(json)!);
            foreach (var entry in trace.GetProperty("added").EnumerateArray()) { var e = entry.GetProperty("unit"); var u = state.CreateUnit(e.GetProperty("line").GetString()!, Int(e, "stage")) with { Items = Strings(e.GetProperty("items")) }; p.Roster.AddToBench(u); if (entry.GetProperty("board").GetBoolean()) p.Roster.Move(UnitSpot.Bench(0), UnitSpot.Board(0,4), false, () => 100); }
            void Snapshot(JsonElement e)
            {
                Check(p.Economy.Gold == Int(e,"gold") && p.Health == Int(e,"hp") && p.Shop.FreeRolls == Int(e,"rolls") && p.TeamBonus == Int(e,"teamBonus"), c + " economy/health");
                Check(p.Items.SequenceEqual(Strings(e.GetProperty("items"))), c + " inventory");
                var em=e.GetProperty("mech"); Check(m.Blessed == em.GetProperty("blessed").GetBoolean() && m.EarlyBlessing == em.GetProperty("blessEarly").GetBoolean(), c + " blessing flags"); Check(m.Eggs.SequenceEqual(em.GetProperty("eggs").Deserialize<ReckoningEgg[]>(json)!), c + " eggs");
                var o=e.GetProperty("offer"); Check(o.ValueKind==JsonValueKind.Null?p.Offer is null:p.Offer is not null&&p.Offer.Kind==o.GetProperty("kind").GetString()&&p.Offer.Rerolls==Int(o,"rerolls")&&p.Offer.Options.SequenceEqual(Strings(o.GetProperty("options"))),c+" offer");
                var orbs=e.GetProperty("orbs"); Check(p.Orbs.Count==orbs.GetArrayLength(),c+" orb count"); for(int i=0;i<p.Orbs.Count;i++){var actual=p.Orbs[i];var expected=orbs[i];Check(actual.Id==Int(expected,"id")&&actual.Kind==expected.GetProperty("kind").GetString()&&actual.Gold==Int(expected,"gold")&&actual.X==expected.GetProperty("x").GetDouble()&&actual.Z==expected.GetProperty("z").GetDouble()&&actual.Items.SequenceEqual(Strings(expected.GetProperty("items")))&&actual.Unit==expected.GetProperty("unit").GetString(),c+" orb");}
            }
            p.Augments.Add(id);mode.AugmentPicked(p,id);p.Roster.Combine(false,p.GiveItem);p.Shop.ClearMaxed();Snapshot(trace.GetProperty("picked"));
            Check(mode.Income(p,new(5,3,2,1)).Augment==Int(trace.GetProperty("income"),"augment"),c+" income");
            foreach(var step in trace.GetProperty("steps").EnumerateArray())
            {
                var counts=step.GetProperty("counts").Deserialize<Dictionary<string,int>>()!;var round=new RoundPosition(Int(step,"stage"),Int(step,"round"));
                var match=new ResolvedMatch(0,null,step.GetProperty("pve").GetBoolean(),false,step.GetProperty("won").GetBoolean()?0:1,[],counts,counts);
                mode.RoundEnd(new(match,p,0,null,[],counts),round.Stage);offers.Make(p,round);mode.RoundStart(p,step.GetProperty("type").GetString()!);Snapshot(step.GetProperty("after"));
            }
            Check(state.CreateUnit(rules.Lines[0].Id,0).Id==Int(trace,"nextId"),c+" IDs");Check(state.Random.Next()==trace.GetProperty("nextRandom").GetDouble(),c+" RNG");
        }
        Console.WriteLine($"Reckoning planning parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} traces."); return checks;
    }
}

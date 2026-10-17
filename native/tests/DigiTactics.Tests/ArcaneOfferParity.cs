using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class ArcaneOfferParity
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
        using var input = File.OpenRead(Path.Combine(root,"tests/fixtures/arcane-offers.json.gz"));
        using var gzip = new GZipStream(input,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        int checks=0;void Check(bool ok,string c){checks++;if(!ok)throw new InvalidOperationException("Arcane offers: "+c);}
        static string[] Strings(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetString()!).ToArray();
        foreach(var trace in fixture.RootElement.EnumerateArray())
        {
            var basis=catalog.Variants.Single(v=>v.Id==trace.GetProperty("variant").GetString());
            var rules=basis with {Offers=basis.Offers! with {Schedule=new[]{"arcAnomaly","arcMarket","arcJunk"}.Select((kind,i)=>new OfferDefinition(kind,1,i+1,2)).ToArray()}};
            var state=new MatchState(rules,trace.GetProperty("seed").GetUInt32(),[new("Player",trace.GetProperty("human").GetBoolean())]);var p=state.Players[0];var m=p.Mechanics.Arcane;var offers=new MatchOffers(state,new Hooks());int n=0;
            void Offer(JsonElement e){Check(e.ValueKind==JsonValueKind.Null?p.Offer is null:p.Offer is not null&&p.Offer.Options.SequenceEqual(Strings(e.GetProperty("options")))&&p.Offer.Rerolls==e.GetProperty("rerolls").GetInt32(),rules.Id+" offer");}
            foreach(var step in trace.GetProperty("steps").EnumerateArray())
            {
                string c=$"{rules.Id}/{n}";
                foreach(var e in p.Roster.Entries.ToArray())p.Roster.Remove(e.Unit.Id,false);
                foreach(var entry in step.GetProperty("setup").EnumerateArray())
                {
                    var e=entry.GetProperty("u");var unit=state.CreateUnit(e.GetProperty("line").GetString()!,e.GetProperty("stage").GetInt32()) with {Items=Strings(e.GetProperty("items")),Mods=e.GetProperty("mods").Clone(),NoItems=e.GetProperty("noItems").GetBoolean()};
                    Check(unit.Id==e.GetProperty("uid").GetInt32(),c+" ID");int slot=p.Roster.FreeBenchSlot;p.Roster.AddToBench(unit);
                    if(entry.GetProperty("board").GetBoolean())p.Roster.Move(UnitSpot.Bench(slot),UnitSpot.Board(entry.GetProperty("c").GetInt32(),entry.GetProperty("r").GetInt32()),false,()=>100);
                }
                var before=step.GetProperty("before");p.Economy.Credit(before.GetProperty("gold").GetInt32()-p.Economy.Gold);p.Items.Clear();p.Items.AddRange(Strings(before.GetProperty("items")));
                m.MarketsOpened=before.GetProperty("opened").GetInt32();m.MarketPlus=before.GetProperty("plus").GetBoolean();m.SyndicateLevel=before.GetProperty("lv").GetInt32();m.ExtraAnomalies=before.GetProperty("extra").GetInt32();m.PendingAnomaly=false;
                int i=0;foreach(var query in step.GetProperty("offers").EnumerateArray())
                {
                    var round=new RoundPosition(1,++i);offers.Make(p,round);Offer(query.GetProperty("made"));Check(offers.Reroll(p,round)==query.GetProperty("reroll").GetBoolean(),c+" reroll");Offer(query.GetProperty("rolled"));
                    Check(offers.Choose(p,n%3,false)==query.GetProperty("chosen").GetString(),c+" choice");
                    var after=query.GetProperty("after");var mech=after.GetProperty("mech");
                    double Number(string key)=>mech.TryGetProperty(key,out var value)?value.GetDouble():0;
                    string[] List(string key)=>mech.TryGetProperty(key,out var value)?Strings(value):[];
                    Check(p.Economy.Gold==after.GetProperty("gold").GetInt32(),c+" gold");Check(p.Items.SequenceEqual(Strings(after.GetProperty("items"))),c+" inventory");
                    Check(m.SyndicateHealth==Number("arcSynHp")&&m.Skips==Number("arcSkips"),c+" market");Check(m.Seen.SequenceEqual(List("arcSeen")),c+" seen");Check(m.Junk.SequenceEqual(List("arcJunk")),c+" junk");Check(m.Anomalies.SequenceEqual(List("arcAnoms")),c+" anomalies");
                    Check(m.Anomaly==(mech.TryGetProperty("arcAnomaly",out var anomaly)?anomaly.GetString():null),c+" latest anomaly");Check(m.ExtraAnomalies==Number("arcExtra")&&m.PendingAnomaly==mech.GetProperty("arcPendingAnom").GetBoolean(),c+" extra");
                    var units=p.Roster.Entries.Select(e=>e.Unit).ToArray();var expected=after.GetProperty("units");Check(units.Length==expected.GetArrayLength(),c+" units");
                    for(int j=0;j<units.Length;j++)Check(units[j].Id==expected[j].GetProperty("uid").GetInt32()&&JsonElement.DeepEquals(units[j].Mods!.Value,expected[j].GetProperty("mods")),c+" modifier");
                }
                n++;
            }
            Check(state.Random.Next()==trace.GetProperty("nextRandom").GetDouble(),rules.Id+" RNG");
        }
        Console.WriteLine($"Arcane offer parity: {checks:N0} checks across 8 traces.");return checks;
    }
}

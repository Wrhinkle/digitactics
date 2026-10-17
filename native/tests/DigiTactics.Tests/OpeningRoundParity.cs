using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class OpeningRoundParity
{
    private sealed class Hooks(MatchState state) : IMatchRoundHooks, IMatchOfferHooks, IMatchCarouselHooks
    {
        public List<string> Log { get; }=[];
        public MatchCarousel Carousel { get; set; }=null!;
        public void ResetTransfers(MatchPlayer p)=>throw new InvalidOperationException("Opening reset transfers");
        public IncomeBreakdown Income(MatchPlayer p,RoundPosition r,IncomeBreakdown e)=>throw new InvalidOperationException("Opening income");
        public void RankUp(MatchPlayer p,RoundPosition r)=>throw new InvalidOperationException("Opening rank up");
        public void RoundStart(MatchPlayer p,RoundPosition r){Log.Add($"round:{p.Id}");p.Shop.FreeRolls++;if(p.Id==0)p.Health++;}
        public void StartCarousel(RoundPosition r){Log.Add("carousel");Carousel.Start(r.Stage,MatchPhase.Planning);}
        public void PlanAi(RoundPosition r){Log.Add("ai");state.Players[3].Health+=2;}
        public string[] Armory(MatchPlayer p,RoundPosition r)=>throw new NotSupportedException();
        public string[] CustomOptions(MatchPlayer p,RoundPosition r,OfferDefinition d,IReadOnlyList<string> e)=>throw new NotSupportedException();
        public void AugmentPicked(MatchPlayer p,string id)=>throw new NotSupportedException();
        public void PowerPicked(MatchPlayer p,string id)=>throw new NotSupportedException();
        public void CustomChosen(MatchPlayer p,PlayerOffer o,string id)=>throw new NotSupportedException();
        public void Combined(MatchPlayer p,PlanningUnit u,IReadOnlyList<PlanningUnit> sources)=>throw new InvalidOperationException("Unexpected opening merge");
        public void PartnerCombined(MatchPlayer q,MatchPlayer p,PlanningUnit u)=>throw new InvalidOperationException("Unexpected opening partner merge");
    }
    public static int Run(string root,ContentCatalog catalog)
    {
        using var input=File.OpenRead(Path.Combine(root,"tests/fixtures/opening-rounds.json.gz"));using var zip=new GZipStream(input,CompressionMode.Decompress);using var doc=JsonDocument.Parse(zip);
        int checks=0;void Check(bool ok,string c){checks++;if(!ok)throw new InvalidOperationException("Opening round parity: "+c);}
        static string[] Strings(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetString()!).ToArray();
        foreach(var trace in doc.RootElement.EnumerateArray())
        {
            var rules=catalog.Variants.Single(v=>v.Id==trace.GetProperty("variant").GetString());uint seed=trace.GetProperty("seed").GetUInt32();bool duos=trace.GetProperty("duos").GetBoolean();string c=$"{rules.Id}/{seed}/{duos}";
            MatchRounds? rounds=null;var state=MatchStart.Create(rules,seed,Enumerable.Range(0,8).Select(i=>new MatchSeat($"Player {i}",i is 0 or 3)).ToArray(),()=>rounds?.Position??RoundPosition.First,duos);
            var hooks=new Hooks(state);hooks.Carousel=new(state,hooks);rounds=new(state,new(state,hooks),hooks);
            Check(rounds.Start(),c+" start");Check(!rounds.Start(),c+" start once");Check(rounds.Position==RoundPosition.First&&rounds.Phase==MatchPhase.Planning&&rounds.RoundType==trace.GetProperty("type").GetString(),c+" phase/round");Check(hooks.Log.SequenceEqual(Strings(trace.GetProperty("log"))),c+" callback order");
            foreach(var p in state.Players)
            {
                var e=trace.GetProperty("players")[p.Id];Check(p.Economy.Gold==e.GetProperty("gold").GetInt32()&&p.Health==e.GetProperty("hp").GetInt32()&&p.Shop.FreeRolls==e.GetProperty("rolls").GetInt32(),c+" economy/health");Check(p.Items.SequenceEqual(Strings(e.GetProperty("items"))),c+" items");Check(p.Shop.Slots.SequenceEqual(e.GetProperty("shop").EnumerateArray().Select(x=>x.GetString())),c+" shop");
                var offer=e.GetProperty("offer");Check(offer.ValueKind==JsonValueKind.Null?p.Offer is null:p.Offer is not null&&p.Offer.Kind==offer.GetProperty("kind").GetString()&&p.Offer.Rerolls==offer.GetProperty("rerolls").GetInt32()&&p.Offer.Options.SequenceEqual(Strings(offer.GetProperty("options"))),c+" offer");
                for(int i=0;i<9;i++){var u=p.Roster.Bench[i];var v=e.GetProperty("bench")[i];Check(v.ValueKind==JsonValueKind.Null?u is null:u is not null&&u.Id==v.GetProperty("uid").GetInt32()&&u.Line==v.GetProperty("line").GetString()&&u.Stage==v.GetProperty("stage").GetInt32()&&u.Items.SequenceEqual(Strings(v.GetProperty("items"))),c+" roster");}
            }
            var expectedCarousel=trace.GetProperty("carousel");var carousel=hooks.Carousel;Check(carousel.Active==(expectedCarousel.ValueKind!=JsonValueKind.Null),c+" carousel active");
            if(carousel.Active){Check(carousel.Index==expectedCarousel.GetProperty("index").GetInt32(),c+" pick index");Check(carousel.Order.Select(p=>p.Id).SequenceEqual(expectedCarousel.GetProperty("order").EnumerateArray().Select(x=>x.GetInt32())),c+" pick order");var options=expectedCarousel.GetProperty("options");Check(carousel.Options.Count==options.GetArrayLength(),c+" options count");for(int i=0;i<options.GetArrayLength();i++){var a=carousel.Options[i];var b=options[i];Check(a.Line==b.GetProperty("line").GetString()&&a.Item==(b.TryGetProperty("item",out var item)?item.GetString():null)&&a.PickedBy==b.GetProperty("by").GetString(),c+" option");}}
            Check(rounds.Pairs.SequenceEqual(trace.GetProperty("pairs").EnumerateArray().Select(e=>new MatchPair(e.GetProperty("a").GetInt32(),e.GetProperty("b").GetInt32(),e.GetProperty("ghost").GetBoolean()))),c+" pairs");
            foreach(var pair in trace.GetProperty("pool").EnumerateObject())Check(state.Pool.Remaining(pair.Name)==pair.Value.GetInt32(),c+" pool");Check(state.Random.Next()==trace.GetProperty("nextRandom").GetDouble(),c+" RNG");
        }
        Console.WriteLine($"Opening round parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} Solo/Duos starts.");return checks;
    }
}

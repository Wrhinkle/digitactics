using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class CarouselParity
{
    private sealed class Hooks : IMatchCarouselHooks
    {
        public List<(string,int,int)> Events { get; } = [];
        public void Combined(MatchPlayer p,PlanningUnit u,IReadOnlyList<PlanningUnit> sources)=>Events.Add(("onCombine",p.Id,u.Id));
        public void PartnerCombined(MatchPlayer partner,MatchPlayer p,PlanningUnit u)=>Events.Add(("onPartnerCombine",partner.Id,u.Id));
    }
    public static int Run(string root,ContentCatalog catalog)
    {
        using var input=File.OpenRead(Path.Combine(root,"tests/fixtures/carousels.json.gz"));using var gzip=new GZipStream(input,CompressionMode.Decompress);using var fixture=JsonDocument.Parse(gzip);
        int checks=0;void Check(bool ok,string c){checks++;if(!ok)throw new InvalidOperationException("Carousel parity: "+c);}
        static string[] Strings(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetString()!).ToArray();
        foreach(var trace in fixture.RootElement.EnumerateArray())
        {
            var rules=catalog.Variants.Single(v=>v.Id==trace.GetProperty("variant").GetString());var state=new MatchState(rules,143,Enumerable.Range(0,8).Select(i=>new MatchSeat("Player "+i,i==0||i==3)).ToArray(),trace.GetProperty("duo").GetBoolean());var hooks=new Hooks();var carousel=new MatchCarousel(state,hooks);int n=0;
            void Snapshot(JsonElement e,string c)
            {
                var expected=e.GetProperty("carousel");Check(carousel.Active==(expected.ValueKind!=JsonValueKind.Null),c+" active");
                if(carousel.Active)
                {
                    Check(carousel.Index==expected.GetProperty("idx").GetInt32(),c+" turn index");Check(carousel.Order.Select(p=>p.Id).SequenceEqual(expected.GetProperty("order").EnumerateArray().Select(x=>x.GetInt32())),c+" order");
                    var options=expected.GetProperty("opts");Check(carousel.Options.Count==options.GetArrayLength(),c+" count");
                    for(int j=0;j<carousel.Options.Count;j++){var a=carousel.Options[j];var b=options[j];Check(a.Line==b.GetProperty("line").GetString()&&a.Item==(b.TryGetProperty("item",out var item)?item.GetString():null)&&a.PickedBy==b.GetProperty("by").GetString(),c+" option");}
                }
                Check(hooks.Events.SequenceEqual(e.GetProperty("events").EnumerateArray().Select(x=>(x.GetProperty("name").GetString()!,x.GetProperty("player").GetInt32(),x.GetProperty("uid").GetInt32()))),c+" hooks");
                foreach(var pair in e.GetProperty("pool").EnumerateObject())Check(state.Pool.Remaining(pair.Name)==pair.Value.GetInt32(),c+" pool "+pair.Name);
                for(int i=0;i<8;i++)
                {
                    var p=state.Players[i];var b=e.GetProperty("players")[i];Check(p.Economy.Gold==b.GetProperty("gold").GetInt32()&&p.Items.SequenceEqual(Strings(b.GetProperty("items"))),c+" economy/items");Check(p.Shop.Slots.SequenceEqual(b.GetProperty("shop").EnumerateArray().Select(x=>x.GetString())),c+" shop");
                    Check(p.Roster.Board.Count==b.GetProperty("board").EnumerateObject().Count(),c+" board");
                    for(int j=0;j<9;j++){var u=p.Roster.Bench[j];var v=b.GetProperty("bench")[j];Check(v.ValueKind==JsonValueKind.Null?u is null:u is not null&&u.Id==v.GetProperty("uid").GetInt32()&&u.Line==v.GetProperty("line").GetString()&&u.Stage==v.GetProperty("stage").GetInt32()&&u.Items.SequenceEqual(Strings(v.GetProperty("items"))),c+" bench");}
                }
            }
            foreach(var step in trace.GetProperty("steps").EnumerateArray())
            {
                string c=$"{rules.Id}/{state.Duos}/{n++}";hooks.Events.Clear();var phase=Enum.Parse<MatchPhase>(step.GetProperty("phase").GetString()!,true);
                for(int i=0;i<8;i++)state.Players[i].Health=step.GetProperty("health")[i].GetInt32();foreach(var pair in step.GetProperty("drained").EnumerateObject())state.Pool.Take(pair.Name,pair.Value.GetInt32());
                carousel.Start(step.GetProperty("stage").GetInt32(),phase);Snapshot(step.GetProperty("started"),c+" start");
                foreach(var action in step.GetProperty("actions").EnumerateArray())
                {
                    var p=state.Players[action.GetProperty("player").GetInt32()];Check(carousel.Pick(p,99,phase)==action.GetProperty("invalid").GetBoolean(),c+" invalid");Check(carousel.Pick(state.Players[(p.Id+1)%8],carousel.Options.ToList().FindIndex(o=>string.IsNullOrEmpty(o.PickedBy)),phase)==action.GetProperty("wrong").GetBoolean(),c+" wrong player");Check(carousel.Pick(p,action.GetProperty("slot").GetInt32(),phase)==action.GetProperty("result").GetBoolean(),c+" pick");carousel.Step(phase);Snapshot(action.GetProperty("state"),c+" pick state");
                }
                carousel.Finish(phase);Snapshot(step.GetProperty("finished"),c+" finish");carousel.Finish(phase);Check(carousel.Done,c+" done");
                foreach(var pair in step.GetProperty("drained").EnumerateObject())state.Pool.Return(pair.Name,pair.Value.GetInt32());
            }
            Check(state.Random.Next()==trace.GetProperty("nextRandom").GetDouble(),rules.Id+" RNG");
        }
        Console.WriteLine($"Carousel parity: {checks:N0} checks across 44 Solo/Duos traces.");return checks;
    }
}

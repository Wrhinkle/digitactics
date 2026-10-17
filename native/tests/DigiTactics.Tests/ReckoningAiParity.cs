using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class ReckoningAiParity
{
    private sealed class Hooks : IMatchItemHooks
    {
        public string UseConsumable(MatchPlayer p,int i,PlanningUnit u)=>throw new NotSupportedException();
        public string? EquipVeto(MatchPlayer p,PlanningUnit u,string i,string? c)=>null;
        public void Combined(MatchPlayer p,PlanningUnit u,IReadOnlyList<PlanningUnit> from)=>throw new NotSupportedException();
    }
    public static int Run(string root,ContentCatalog catalog)
    {
        using var input=File.OpenRead(Path.Combine(root,"tests/fixtures/reckoning-ai.json.gz"));using var zip=new GZipStream(input,CompressionMode.Decompress);using var doc=JsonDocument.Parse(zip);
        int checks=0;void Check(bool ok,string c){checks++;if(!ok)throw new InvalidOperationException("Reckoning AI parity: "+c);}
        static string[] Strings(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetString()!).ToArray();
        foreach(var trace in doc.RootElement.EnumerateArray())
        {
            var rules=catalog.Variants.Single(v=>v.Id==trace.GetProperty("variant").GetString());bool human=trace.GetProperty("human").GetBoolean();string c=$"{rules.Id}/{human}/{trace.GetProperty("n")}";
            var state=new MatchState(rules,trace.GetProperty("seed").GetUInt32(),[new("Player",human)]);var p=state.Players[0];
            var mode=new ReckoningPlanning(state);var inventory=new PlanningItems(state,new Hooks());var phase=Enum.Parse<MatchPhase>(trace.GetProperty("phase").GetString()!,true);
            foreach(var added in trace.GetProperty("added").EnumerateArray())
            {
                var e=added.GetProperty("unit");var u=state.CreateUnit(e.GetProperty("line").GetString()!,e.GetProperty("stage").GetInt32()) with {Items=Strings(e.GetProperty("items")),NoItems=e.GetProperty("noItems").GetBoolean(),Mods=e.TryGetProperty("mods",out var mods)?mods:null,BonusTraits=e.TryGetProperty("bonusTraits",out var bonus)?bonus.Deserialize<Dictionary<string,int>>():null};Check(u.Id==e.GetProperty("uid").GetInt32(),c+" ID");int slot=p.Roster.FreeBenchSlot;p.Roster.AddToBench(u);if(added.GetProperty("board").GetBoolean())p.Roster.Move(UnitSpot.Bench(slot),UnitSpot.Board(added.GetProperty("col").GetInt32(),4),false,()=>100);
            }
            var before=trace.GetProperty("before");p.Items.AddRange(Strings(before));if(trace.GetProperty("drained").GetString() is {} drained)state.Pool.Take(drained,state.Pool.Remaining(drained));
            mode.AiTurn(p,inventory,trace.GetProperty("stage").GetInt32());
            var after=trace.GetProperty("after");Check(p.Economy.Gold==after.GetProperty("gold").GetInt32(),c+" gold");Check(p.Items.SequenceEqual(Strings(after.GetProperty("items"))),c+" items");Check(p.Shop.Slots.SequenceEqual(after.GetProperty("shop").EnumerateArray().Select(x=>x.GetString())),c+" shop");
            void Unit(PlanningUnit? u,JsonElement e)
            {
                Check((u is null)==(e.ValueKind==JsonValueKind.Null),c+" unit presence");if(u is null)return;
                Check(u.Id==e.GetProperty("uid").GetInt32()&&u.Line==e.GetProperty("line").GetString()&&u.Stage==e.GetProperty("stage").GetInt32()&&u.Items.SequenceEqual(Strings(e.GetProperty("items"))),c+" unit");
                Check((u.Mods is null)==!e.TryGetProperty("mods",out var m),c+" modifier presence");if(u.Mods is {} actual)Check(JsonElement.DeepEquals(actual,m),c+" mods");Check((u.BonusTraits is null)==!e.TryGetProperty("bonusTraits",out var b),c+" trait presence");if(u.BonusTraits is not null)Check(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(u.BonusTraits),b),c+" traits");
            }
            var board=after.GetProperty("board").EnumerateObject().ToArray();Check(board.Length==p.Roster.Board.Count,c+" board count");for(int i=0;i<board.Length;i++){Check($"{p.Roster.Board[i].Spot.Column},{p.Roster.Board[i].Spot.Row}"==board[i].Name,c+" order");Unit(p.Roster.Board[i].Unit,board[i].Value);}for(int i=0;i<9;i++)Unit(p.Roster.Bench[i],after.GetProperty("bench")[i]);foreach(var pair in after.GetProperty("pool").EnumerateObject())Check(state.Pool.Remaining(pair.Name)==pair.Value.GetInt32(),c+" pool "+pair.Name);Check(state.CreateUnit(rules.Lines[0].Id,0).Id==trace.GetProperty("nextId").GetInt32(),c+" next ID");Check(state.Random.Next()==trace.GetProperty("nextRandom").GetDouble(),c+" RNG");
        }
        Console.WriteLine($"Reckoning AI parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} traces.");return checks;
    }
}


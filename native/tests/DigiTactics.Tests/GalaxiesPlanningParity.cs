using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class GalaxiesPlanningParity
{
    private sealed class Items : IMatchItemHooks
    {
        public string UseConsumable(MatchPlayer p,int i,PlanningUnit u)=>throw new NotSupportedException();
        public string? EquipVeto(MatchPlayer p,PlanningUnit u,string i,string? c)=>null;
        public void Combined(MatchPlayer p,PlanningUnit u,IReadOnlyList<PlanningUnit> from) { }
    }
    public static int Run(string root,ContentCatalog catalog)
    {
        using var input=File.OpenRead(Path.Combine(root,"tests/fixtures/galaxies-planning.json.gz"));using var zip=new GZipStream(input,CompressionMode.Decompress);using var doc=JsonDocument.Parse(zip);
        int checks=0;void Check(bool ok,string c){checks++;if(!ok)throw new InvalidOperationException("Galaxies planning parity: "+c);}
        static string[] Strings(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetString()!).ToArray();
        foreach(var trace in doc.RootElement.EnumerateArray())
        {
            var rules=catalog.Variants.Single(v=>v.Id==trace.GetProperty("variant").GetString());int n=trace.GetProperty("n").GetInt32();string galaxy=trace.GetProperty("galaxy").GetString()!;bool human=trace.GetProperty("human").GetBoolean();string c=$"{rules.Id}/{galaxy}/{human}/{n}";
            var state=new MatchState(rules,trace.GetProperty("seed").GetUInt32(),[new("Player",human)],initialize:s=>{s.Galaxy=galaxy;s.Players[0].Economy.Credit(500-s.Players[0].Economy.Gold);s.Players[0].Shop.FreeRolls=2;new MatchShopRolls(s,()=>RoundPosition.First).Install();});
            var p=state.Players[0];var mode=new GalaxiesPlanning(state);var commands=new PlanningShopCommands(mode);var inventory=new PlanningItems(state,new Items());var phase=Enum.Parse<MatchPhase>(trace.GetProperty("phase").GetString()!,true);
            string bounty=trace.GetProperty("bounty").GetString()!;p.Mechanics.GalaxiesBounties[bounty]=n%11;
            foreach(var added in trace.GetProperty("added").EnumerateArray())
            {
                var e=added.GetProperty("unit");var u=state.CreateUnit(e.GetProperty("line").GetString()!,e.GetProperty("stage").GetInt32()) with {Items=Strings(e.GetProperty("items"))};Check(u.Id==e.GetProperty("uid").GetInt32(),c+" injected ID");int slot=p.Roster.FreeBenchSlot;p.Roster.AddToBench(u);if(added.GetProperty("board").GetBoolean())p.Roster.Move(UnitSpot.Bench(slot),UnitSpot.Board(added.GetProperty("col").GetInt32(),4),false,()=>100);
            }
            var before=trace.GetProperty("before");p.Items.AddRange(Strings(before.GetProperty("items")));foreach(var pair in before.GetProperty("pool").EnumerateObject()){state.Pool.Take(pair.Name,state.Pool.Remaining(pair.Name));state.Pool.Return(pair.Name,pair.Value.GetInt32());}
            void Snapshot(JsonElement e,string label)
            {
                Check(p.Economy.Gold==e.GetProperty("gold").GetInt32()&&p.Shop.FreeRolls==e.GetProperty("rolls").GetInt32(),label+" economy");Check(p.Items.SequenceEqual(Strings(e.GetProperty("items"))),label+" items");Check(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(p.Mechanics.GalaxiesBounties),e.GetProperty("bounty")),label+" bounty");Check(p.Shop.Slots.SequenceEqual(e.GetProperty("shop").EnumerateArray().Select(x=>x.GetString())),label+" shop");
                void Unit(PlanningUnit? u,JsonElement expected)=>Check(expected.ValueKind==JsonValueKind.Null?u is null:u is not null&&u.Id==expected.GetProperty("uid").GetInt32()&&u.Line==expected.GetProperty("line").GetString()&&u.Stage==expected.GetProperty("stage").GetInt32()&&u.Items.SequenceEqual(Strings(expected.GetProperty("items"))),label+" unit");
                var board=e.GetProperty("board").EnumerateObject().ToArray();Check(p.Roster.Board.Count==board.Length,label+" board size");for(int i=0;i<board.Length;i++){var a=p.Roster.Board[i];Check($"{a.Spot.Column},{a.Spot.Row}"==board[i].Name,label+" board order");Unit(a.Unit,board[i].Value);}for(int i=0;i<9;i++)Unit(p.Roster.Bench[i],e.GetProperty("bench")[i]);
                foreach(var pair in e.GetProperty("pool").EnumerateObject())Check(state.Pool.Remaining(pair.Name)==pair.Value.GetInt32(),label+" pool "+pair.Name);
            }
            var round=new RoundPosition(trace.GetProperty("stage").GetInt32(),trace.GetProperty("round").GetInt32());mode.RoundStart(p,round);Check(mode.TeamSize(5,round.Stage)==trace.GetProperty("size").GetInt32(),c+" team size");Snapshot(trace.GetProperty("afterRound"),c+" round");
            Check(commands.Sell(p,trace.GetProperty("sold").GetInt32(),phase)==trace.GetProperty("sale").GetBoolean(),c+" sale");Snapshot(trace.GetProperty("afterSale"),c+" sale");
            if(trace.GetProperty("forced").GetBoolean())Check(p.Shop.ReplaceOffer(0,bounty),c+" forced offer");Check(commands.Buy(p,0,phase)==trace.GetProperty("bought").GetBoolean(),c+" purchase");Snapshot(trace.GetProperty("afterBuy"),c+" purchase");
            mode.AiTurn(p,inventory,phase);Snapshot(trace.GetProperty("final"),c+" AI");Check(state.Random.Next()==trace.GetProperty("nextRandom").GetDouble(),c+" RNG");
        }
        Console.WriteLine($"Galaxies planning parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} cases.");return checks;
    }
}

using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class UnderlordsTamerParity
{
    public static int Run(string root,ContentCatalog catalog)
    {
        using var input=File.OpenRead(Path.Combine(root,"tests/fixtures/underlords-tamers.json.gz"));using var zip=new GZipStream(input,CompressionMode.Decompress);using var doc=JsonDocument.Parse(zip);
        int checks=0;void Check(bool ok,string c){checks++;if(!ok)throw new InvalidOperationException("Underlords Tamer parity: "+c);}
        foreach(var trace in doc.RootElement.EnumerateArray())
        {
            var rules=catalog.Variants.Single(v=>v.Id==trace.GetProperty("variant").GetString());bool human=trace.GetProperty("human").GetBoolean();int n=trace.GetProperty("n").GetInt32();string power=trace.GetProperty("power").GetString()!,c=$"{rules.Id}/{human}/{power}/{n}";
            var state=new MatchState(rules,143,[new("Player",human)]);var p=state.Players[0];var mode=new UnderlordsPlanning(state);var placement=new MatchPlacement(state);
            foreach(var added in trace.GetProperty("added").EnumerateArray())
            {
                var e=added.GetProperty("unit");var u=state.CreateUnit(e.GetProperty("line").GetString()!,2);Check(u.Id==e.GetProperty("uid").GetInt32(),c+" ID");int slot=p.Roster.FreeBenchSlot;p.Roster.AddToBench(u);if(added.GetProperty("board").GetBoolean())p.Roster.Move(UnitSpot.Bench(slot),UnitSpot.Board(added.GetProperty("col").GetInt32(),added.GetProperty("row").GetInt32()),false,()=>100);
            }
            void Snapshot(JsonElement expected)
            {
                var ep=expected.GetProperty("power");Check(p.Power?.Id==ep.GetProperty("id").GetString()&&p.Power?.Rank==ep.GetProperty("rank").GetInt32(),c+" power");Check(placement.Capacity(p,1)==expected.GetProperty("capacity").GetInt32(),c+" capacity");Check(p.Shop.Slots.SequenceEqual(expected.GetProperty("shop").EnumerateArray().Select(e=>e.GetString())),c+" shop");
                void Unit(PlanningUnit? u,JsonElement e)=>Check(e.ValueKind==JsonValueKind.Null?u is null:u is not null&&u.Id==e.GetProperty("uid").GetInt32()&&u.Line==e.GetProperty("line").GetString()&&u.Stage==e.GetProperty("stage").GetInt32()&&u.Items.Length==0,c+" unit");
                var board=expected.GetProperty("board").EnumerateObject().ToArray();Check(board.Length==p.Roster.Board.Count,c+" board size");for(int i=0;i<board.Length;i++){Check($"{p.Roster.Board[i].Spot.Column},{p.Roster.Board[i].Spot.Row}"==board[i].Name,c+" placement order");Unit(p.Roster.Board[i].Unit,board[i].Value);}for(int i=0;i<9;i++)Unit(p.Roster.Bench[i],expected.GetProperty("bench")[i]);foreach(var pair in expected.GetProperty("pool").EnumerateObject())Check(state.Pool.Remaining(pair.Name)==pair.Value.GetInt32(),c+" pool");
            }
            void Pick(){p.Power=new(power);mode.PowerPicked(p,power);p.Roster.Combine(human&&n%2==1,p.GiveItem);p.Shop.ClearMaxed();}
            Pick();Snapshot(trace.GetProperty("picked"));Pick();Snapshot(trace.GetProperty("repeated"));
            foreach(var entry in trace.GetProperty("ranks").EnumerateArray()){int rank=entry.GetProperty("rank").GetInt32();p.Power=p.Power! with {Rank=rank};mode.RankUp(p,rank);Snapshot(entry.GetProperty("after"));}
            Check(state.CreateUnit(rules.Lines[0].Id,0).Id==trace.GetProperty("nextId").GetInt32(),c+" allocation after pick");Check(state.Random.Next()==trace.GetProperty("nextRandom").GetDouble(),c+" RNG");
        }
        Console.WriteLine($"Underlords Tamer parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} traces.");return checks;
    }
}

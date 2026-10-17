using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class MatchPlacementParity
{
    public static int Run(string root,ContentCatalog catalog)
    {
        using var input=File.OpenRead(Path.Combine(root,"tests/fixtures/match-placement.json.gz"));using var zip=new GZipStream(input,CompressionMode.Decompress);using var doc=JsonDocument.Parse(zip);var json=new JsonSerializerOptions{PropertyNameCaseInsensitive=true};
        int checks=0;void Check(bool ok,string c){checks++;if(!ok)throw new InvalidOperationException("Match placement parity: "+c);}
        static string[] Strings(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetString()!).ToArray();
        foreach(var trace in doc.RootElement.EnumerateArray())
        {
            var rules=catalog.Variants.Single(v=>v.Id==trace.GetProperty("variant").GetString());string mode=rules.Id.Split('/')[0];int n=trace.GetProperty("n").GetInt32(),stage=trace.GetProperty("stage").GetInt32();bool human=trace.GetProperty("human").GetBoolean();string c=$"{rules.Id}/{human}/{n}";
            var state=new MatchState(rules,143,[new("Player",human)]){Galaxy="bigger"};var p=state.Players[0];var placement=new MatchPlacement(state);
            if(rules.Economy!.Xp is not null)p.Economy.AddXp(trace.GetProperty("xp").GetInt32());else p.Economy.BeginRound(stage,1);p.TeamBonus=n%4;
            if(mode=="gizmos"&&n%2==1)p.Augments.Add("giz_a_recruit");if(mode.StartsWith("underlords",StringComparison.Ordinal))p.Globals.AddRange(Enumerable.Repeat("ul_g_roster",n%3));if(mode=="autochess")p.Globals.AddRange(Enumerable.Repeat("ac_r_key",n%3));
            var power=trace.GetProperty("power");if(power.ValueKind!=JsonValueKind.Null)p.Power=new(power.GetProperty("id").GetString()!,power.GetProperty("rank").GetInt32());
            foreach(var added in trace.GetProperty("added").EnumerateArray())
            {
                var e=added.GetProperty("unit");var u=state.CreateUnit(e.GetProperty("line").GetString()!,e.GetProperty("stage").GetInt32()) with {Items=Strings(e.GetProperty("items"))};Check(u.Id==e.GetProperty("uid").GetInt32(),c+" ID");int slot=p.Roster.FreeBenchSlot;p.Roster.AddToBench(u);if(added.GetProperty("board").GetBoolean())p.Roster.Move(UnitSpot.Bench(slot),UnitSpot.Board(added.GetProperty("col").GetInt32(),4),false,()=>100);
            }
            void Snapshot(JsonElement e)
            {
                Check(placement.Capacity(p,stage)==e.GetProperty("cap").GetInt32(),c+" capacity");Check(p.Roster.BoardCount==e.GetProperty("boardCount").GetInt32(),c+" slots used");
                void Unit(PlanningUnit? u,JsonElement v)=>Check(v.ValueKind==JsonValueKind.Null?u is null:u is not null&&u.Id==v.GetProperty("uid").GetInt32()&&u.Line==v.GetProperty("line").GetString()&&u.Stage==v.GetProperty("stage").GetInt32()&&u.Items.SequenceEqual(Strings(v.GetProperty("items"))),c+" unit");
                var board=e.GetProperty("board").EnumerateObject().ToArray();Check(board.Length==p.Roster.Board.Count,c+" board size");for(int i=0;i<board.Length;i++){Check($"{p.Roster.Board[i].Spot.Column},{p.Roster.Board[i].Spot.Row}"==board[i].Name,c+" order");Unit(p.Roster.Board[i].Unit,board[i].Value);}for(int i=0;i<9;i++)Unit(p.Roster.Bench[i],e.GetProperty("bench")[i]);
            }
            Snapshot(trace.GetProperty("initial"));foreach(var step in trace.GetProperty("steps").EnumerateArray())
            {
                var from=step.GetProperty("from").Deserialize<UnitSpot>(json);var to=step.GetProperty("to").Deserialize<UnitSpot>(json);var phase=Enum.Parse<MatchPhase>(step.GetProperty("phase").GetString()!,true);
                Check(placement.Move(p,from,to,phase,stage).ToString()==step.GetProperty("result").GetString(),c+" move");Snapshot(step.GetProperty("after"));
            }
        }
        Console.WriteLine($"Match placement parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} traces.");return checks;
    }
}

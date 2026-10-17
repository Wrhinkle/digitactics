using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class AutoChessPlanningParity
{
    private sealed class Hooks : IMatchItemHooks
    {
        public string UseConsumable(MatchPlayer p, int i, PlanningUnit u) => throw new NotSupportedException();
        public string? EquipVeto(MatchPlayer p, PlanningUnit u, string i, string? c) => null;
        public void Combined(MatchPlayer p, PlanningUnit u, IReadOnlyList<PlanningUnit> sources) { }
    }
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root,"tests/fixtures/autochess-planning.json.gz"));
        using var gzip = new GZipStream(input,CompressionMode.Decompress); using var doc = JsonDocument.Parse(gzip);
        int checks = 0; void Check(bool ok,string label) { checks++; if(!ok) throw new InvalidOperationException("Auto Chess planning parity: " + label); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x=>x.GetString()!).ToArray();
        foreach(var trace in doc.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v=>v.Id==trace.GetProperty("variant").GetString());
            var state = new MatchState(rules,319,[new("Player",trace.GetProperty("human").GetBoolean())]);
            var p = state.Players[0]; var mode = new AutoChessPlanning(state); var items = new PlanningItems(state,new Hooks()); int n=0;
            foreach(var step in trace.GetProperty("steps").EnumerateArray())
            {
                string c=$"{rules.Id}/{p.IsHuman}/{n}";
                foreach(var e in p.Roster.Entries.ToArray()) p.Roster.Remove(e.Unit.Id,false);
                foreach(var added in step.GetProperty("added").EnumerateArray())
                {
                    var e=added.GetProperty("unit"); var u=state.CreateUnit(e.GetProperty("line").GetString()!,e.GetProperty("stage").GetInt32());
                    Check(u.Id==e.GetProperty("uid").GetInt32(),c+" allocated ID"); int slot=p.Roster.FreeBenchSlot; p.Roster.AddToBench(u);
                    if(added.GetProperty("board").GetBoolean()) p.Roster.Move(UnitSpot.Bench(slot),UnitSpot.Board(added.GetProperty("col").GetInt32(),4),false,()=>100);
                }
                var before=step.GetProperty("before"); p.Economy.Credit(before.GetProperty("gold").GetInt32()-p.Economy.Gold); p.Shop.FreeRolls=before.GetProperty("freeRolls").GetInt32();
                p.Items.Clear();p.Items.AddRange(Strings(before.GetProperty("items")));p.Globals.Clear();p.Globals.AddRange(Strings(before.GetProperty("globals")));
                foreach(var pair in before.GetProperty("pool").EnumerateObject()) {state.Pool.Take(pair.Name,state.Pool.Remaining(pair.Name));state.Pool.Return(pair.Name,pair.Value.GetInt32());}
                var income=mode.Income(p,new(step.GetProperty("stage").GetInt32(),step.GetProperty("round").GetInt32()),new(9,3,n%4,2,1));var expected=step.GetProperty("income");
                Check(income==new IncomeBreakdown(expected.GetProperty("base").GetInt32(),expected.GetProperty("interest").GetInt32(),expected.GetProperty("streak").GetInt32(),expected.GetProperty("augment").GetInt32(),expected.GetProperty("bonus").GetInt32()),c+" income");
                Check(mode.ShopLevel(p,n%13)==step.GetProperty("shop").GetInt32(),c+" shop level");Check(mode.TeamSize(p,n%10)==step.GetProperty("size").GetInt32(),c+" capacity");
                mode.RoundStart(p); mode.Sold(p);
                var target=step.GetProperty("target");Check(mode.EggTarget(p)?.Id==(target.ValueKind==JsonValueKind.Null?(int?)null:target.GetInt32()),c+" egg target");
                mode.AiTurn(p,items,step.GetProperty("phase").GetString()=="combat"?MatchPhase.Combat:MatchPhase.Planning);
                var after=step.GetProperty("after");Check(p.Economy.Gold==after.GetProperty("gold").GetInt32(),c+" gold");Check(p.Shop.FreeRolls==after.GetProperty("freeRolls").GetInt32(),c+" rolls");Check(p.Items.SequenceEqual(Strings(after.GetProperty("items"))),c+" items");
                Check(p.Shop.Slots.SequenceEqual(after.GetProperty("shop").EnumerateArray().Select(e=>e.GetString())),c+" shop offers");
                foreach(var pair in after.GetProperty("pool").EnumerateObject())Check(state.Pool.Remaining(pair.Name)==pair.Value.GetInt32(),c+" pool "+pair.Name+" actual="+state.Pool.Remaining(pair.Name)+" expected="+pair.Value.GetInt32());
                void Unit(PlanningUnit? u,JsonElement e) => Check(e.ValueKind==JsonValueKind.Null?u is null:u is not null&&u.Id==e.GetProperty("uid").GetInt32()&&u.Line==e.GetProperty("line").GetString()&&u.Stage==e.GetProperty("stage").GetInt32()&&u.Items.SequenceEqual(Strings(e.GetProperty("items"))),c+" unit");
                var board=after.GetProperty("board").EnumerateObject().ToArray();Check(board.Length==p.Roster.Board.Count,c+" board count");
                for(int i=0;i<board.Length;i++){Check($"{p.Roster.Board[i].Spot.Column},{p.Roster.Board[i].Spot.Row}"==board[i].Name,c+" board order");Unit(p.Roster.Board[i].Unit,board[i].Value);}
                for(int i=0;i<9;i++)Unit(p.Roster.Bench[i],after.GetProperty("bench")[i]); n++;
            }
            Check(state.Random.Next()==trace.GetProperty("nextRandom").GetDouble(),rules.Id+" RNG");
        }
        Console.WriteLine($"Auto Chess planning parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} traces."); return checks;
    }
}

using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class GalaxiesSettlementParity
{
    public static int Run(string root,ContentCatalog catalog)
    {
        using var input=File.OpenRead(Path.Combine(root,"tests/fixtures/galaxies-settlement.json.gz"));using var zip=new GZipStream(input,CompressionMode.Decompress);using var doc=JsonDocument.Parse(zip);var json=new JsonSerializerOptions{PropertyNameCaseInsensitive=true};
        int checks=0;void Check(bool ok,string c){checks++;if(!ok)throw new InvalidOperationException("Galaxies settlement parity: "+c);}
        foreach(var trace in doc.RootElement.EnumerateArray())
        {
            var rules=catalog.Variants.Single(v=>v.Id==trace.GetProperty("variant").GetString());var a=trace.GetProperty("sideA").Deserialize<CombatEntry[]>(json)!;var b=trace.GetProperty("sideB").Deserialize<CombatEntry[]>(json)!;
            var combat=new CombatSimulation(rules,a,b,trace.GetProperty("seed").GetUInt32(),setup:new(new(),new()));int ticks=0;while(!combat.Done&&ticks<2000){combat.Step();ticks++;}
            string battle=$"{rules.Id}/{trace.GetProperty("seed")}";Check(combat.Done&&ticks==trace.GetProperty("ticks").GetInt32()&&combat.Winner==trace.GetProperty("winner").GetInt32(),battle+" result");
            Check(combat.Units.Count==trace.GetProperty("units").GetArrayLength(),battle+" units");for(int i=0;i<combat.Units.Count;i++){var u=combat.Units[i];var e=trace.GetProperty("units")[i];Check(u.Line.Id==e.GetProperty("line").GetString()&&u.Team==e.GetProperty("team").GetInt32()&&u.Alive==e.GetProperty("alive").GetBoolean()&&u.Summoned==e.GetProperty("summoned").GetBoolean(),battle+" terminal state");}
            for(int team=0;team<2;team++){Check(combat.PirateKillCount(team)==trace.GetProperty("pirates")[team].GetInt32(),battle+" pirate kills");Check(combat.MeepCount(team)==trace.GetProperty("meeps")[team].GetInt32(),battle+" meeps");}
            var state=new MatchState(rules,143,[new("A",true),new("B",true)]);var mode=new GalaxiesPlanning(state);int n=0;
            foreach(var sample in trace.GetProperty("cases").EnumerateArray())
            {
                int team=sample.GetProperty("team").GetInt32();var p=state.Players[team];state.Galaxy=sample.GetProperty("galaxy").GetString();p.Items.Clear();p.Items.AddRange(sample.GetProperty("inventory").EnumerateArray().Select(x=>x.GetString()!));
                var counts=sample.GetProperty("counts").EnumerateObject().ToDictionary(e=>e.Name,e=>e.Value.GetInt32());
                var match=ResolvedMatch.FromCombat(0,1,sample.GetProperty("pve").GetBoolean(),false,combat,counts,counts) with {Combat=sample.GetProperty("missing").GetBoolean()?null:combat};
                bool view=sample.GetProperty("view").GetBoolean(),first=sample.GetProperty("first").GetBoolean();var scoped=match.Units.Where(u=>!view||u.Survivor.Team!=team||u.Owner==p.Id||u.Owner is null&&first).ToArray();
                int gold=mode.RoundEnd(new(match,p,team,view?p.Id:null,scoped,counts),sample.GetProperty("stage").GetInt32());string c=$"{battle}/{n++}";
                Check(gold==sample.GetProperty("gold").GetInt32(),c+" gold");Check(p.Items.SequenceEqual(sample.GetProperty("items").EnumerateArray().Select(x=>x.GetString()!)),c+" rewards");Check(state.Random.Next()==sample.GetProperty("nextRandom").GetDouble(),c+" RNG");
            }
        }
        Console.WriteLine($"Galaxies settlement parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} battles.");return checks;
    }
}

using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class MatchStartParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input=File.OpenRead(Path.Combine(root,"tests/fixtures/match-start.json.gz"));using var zip=new GZipStream(input,CompressionMode.Decompress);using var doc=JsonDocument.Parse(zip);
        int checks=0;void Check(bool ok,string c){checks++;if(!ok)throw new InvalidOperationException("Match start parity: "+c);}
        static string[] Strings(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetString()!).ToArray();
        foreach(var trace in doc.RootElement.EnumerateArray())
        {
            var rules=catalog.Variants.Single(v=>v.Id==trace.GetProperty("variant").GetString());uint seed=trace.GetProperty("seed").GetUInt32();bool duos=trace.GetProperty("duos").GetBoolean();string c=$"{rules.Id}/{seed}/{duos}";
            var seats=Enumerable.Range(0,8).Select(i=>new MatchSeat($"Player {i}",i is 0 or 3)).ToArray();
            var state=MatchStart.Create(rules,seed,seats,()=>RoundPosition.First,duos,duos?[[0,7],[1,6],[2,5],[3,4]]:null);
            var mech=trace.GetProperty("mechanics");
            void Mechanic(string key,string? actual)=>Check(actual==(mech.TryGetProperty(key,out var e)?e.GetString():null),c+" "+key);
            Mechanic("galaxy",state.Galaxy);Mechanic("mutation",state.GizmosMutation);Mechanic("mirage",state.DragonMirage);Mechanic("treasure",state.TreasureVariant);Mechanic("arcPortal",state.ArcanePortal);
            Check(mech.TryGetProperty("gates",out var gates)?state.GizmosGates is not null&&state.GizmosGates.SequenceEqual(gates.EnumerateArray().Select(e=>e.GetInt32())):state.GizmosGates is null,c+" gates");
            Check(state.ArcaneSponsor.SequenceEqual(mech.TryGetProperty("arcSponsor",out var sponsor)?Strings(sponsor):[]),c+" sponsor");
            Check(state.Duos==duos&&state.Seed==seed,c+" identity");
            foreach(var p in state.Players)
            {
                var e=trace.GetProperty("players")[p.Id];Check(p.Name==e.GetProperty("name").GetString()&&p.IsHuman==e.GetProperty("human").GetBoolean(),c+" seat");Check(p.Team==(e.GetProperty("team").ValueKind==JsonValueKind.Null?(int?)null:e.GetProperty("team").GetInt32()),c+" team membership");
                Check(p.Health==e.GetProperty("hp").GetInt32()&&p.Economy.Gold==e.GetProperty("gold").GetInt32()&&p.Economy.Level==e.GetProperty("level").GetInt32()&&p.Economy.Xp==e.GetProperty("xp").GetInt32(),c+" economy/health");
                Check(p.Items.SequenceEqual(Strings(e.GetProperty("items"))),c+" items");Check(p.Shop.Slots.SequenceEqual(e.GetProperty("shop").EnumerateArray().Select(x=>x.GetString())),c+" starting shop");
                var meta=e.GetProperty("meta");for(int i=0;i<p.Shop.Slots.Count;i++)Check(meta.ValueKind==JsonValueKind.Null||meta[i].ValueKind==JsonValueKind.Null?p.Shop.Metadata[i] is null:p.Shop.Metadata[i]?.Cost==meta[i].GetProperty("cost").GetInt32()&&p.Shop.Metadata[i]?.Tag==meta[i].GetProperty("tag").GetString(),c+" metadata");
                Check(p.Roster.Board.Count==0,c+" board");for(int i=0;i<9;i++){var u=p.Roster.Bench[i];var expected=e.GetProperty("bench")[i];Check(expected.ValueKind==JsonValueKind.Null?u is null:u is not null&&u.Id==expected.GetProperty("uid").GetInt32()&&u.Line==expected.GetProperty("line").GetString()&&u.Stage==expected.GetProperty("stage").GetInt32()&&u.Items.SequenceEqual(Strings(expected.GetProperty("items"))),c+" unit");}
            }
            Check(state.Teams.Count==trace.GetProperty("teams").GetArrayLength(),c+" teams");foreach(var t in state.Teams){var e=trace.GetProperty("teams")[t.Id];Check(t.Health==e.GetProperty("hp").GetInt32()&&t.Members.Select(p=>p.Id).SequenceEqual(e.GetProperty("members").EnumerateArray().Select(x=>x.GetInt32())),c+" team");}
            foreach(var pair in trace.GetProperty("pool").EnumerateObject())Check(state.Pool.Remaining(pair.Name)==pair.Value.GetInt32(),c+" pool "+pair.Name);
            Check(state.Random.Next()==trace.GetProperty("nextRandom").GetDouble(),c+" RNG");
        }
        Console.WriteLine($"Match start parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} Solo/Duos starts.");return checks;
    }
}

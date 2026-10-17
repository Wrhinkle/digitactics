using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class DragonShopParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input=File.OpenRead(Path.Combine(root,"tests/fixtures/dragonlands-shops.json.gz"));using var zip=new GZipStream(input,CompressionMode.Decompress);using var doc=JsonDocument.Parse(zip);
        int checks=0;void Check(bool ok,string c){checks++;if(!ok)throw new InvalidOperationException("Dragonlands shop parity: "+c);}
        foreach(var trace in doc.RootElement.EnumerateArray())
        {
            var rules=catalog.Variants.Single(v=>v.Id==trace.GetProperty("variant").GetString());int n=trace.GetProperty("n").GetInt32();
            var state=new MatchState(rules,trace.GetProperty("seed").GetUInt32(),[new("Player",false)]);var p=state.Players[0];p.Shop.RollHooks=new DragonlandsShop(state,p);
            if(rules.Economy!.Xp is not null)p.Economy.AddXp(trace.GetProperty("xp").GetInt32());else p.Economy.BeginRound(trace.GetProperty("stage").GetInt32(),1);
            p.Economy.Credit(500-p.Economy.Gold);
            foreach(var e in trace.GetProperty("added").EnumerateArray()){var u=state.CreateUnit(e.GetProperty("line").GetString()!,2);Check(u.Id==e.GetProperty("uid").GetInt32(),"injected ID");p.Roster.AddToBench(u);}
            void Snapshot(JsonElement expected,string c)
            {
                Check(p.Economy.Gold==expected.GetProperty("gold").GetInt32(),c+" gold");Check(p.Mechanics.DragonStarShop==expected.GetProperty("star").GetBoolean(),c+" star guarantee");Check(p.Shop.FreeRolls==expected.GetProperty("rolls").GetInt32(),c+" free rolls");
                Check(p.Shop.Slots.SequenceEqual(expected.GetProperty("shop").EnumerateArray().Select(e=>e.GetString())),c+" offers");
                var meta=expected.GetProperty("meta");
                for(int i=0;i<p.Shop.Slots.Count;i++)
                {
                    var actual=p.Shop.Metadata[i];var m=meta.ValueKind==JsonValueKind.Null?default:meta[i];
                    Check(m.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null?actual is null:actual is not null,c+" metadata presence");if(actual is not null)Check(actual.Cost==m.GetProperty("cost").GetInt32()&&actual.Tag==m.GetProperty("tag").GetString(),c+" cost/tag");
                    var id=p.Shop.Slots[i];Check(p.Shop.Cost(i)==(id is null?0:m.ValueKind==JsonValueKind.Object?m.GetProperty("cost").GetInt32():rules.Lines.Single(l=>l.Id==id).Tier),c+" price");
                }
                for(int i=0;i<9;i++)
                {
                    var u=p.Roster.Bench[i];var e=expected.GetProperty("bench")[i];Check((u is null)==(e.ValueKind==JsonValueKind.Null),c+" presence");if(u is null)continue;
                    Check(u.Id==e.GetProperty("uid").GetInt32()&&u.Line==e.GetProperty("line").GetString()&&u.Stage==e.GetProperty("stage").GetInt32(),c+" unit");
                    Check(u.Items.SequenceEqual(e.GetProperty("items").EnumerateArray().Select(x=>x.GetString()!)),c+" items");
                }
                foreach(var pair in expected.GetProperty("pool").EnumerateObject())Check(state.Pool.Remaining(pair.Name)==pair.Value.GetInt32(),c+" pool "+pair.Name);
            }
            int k=0;foreach(var step in trace.GetProperty("steps").EnumerateArray())
            {
                string c=$"{rules.Id}/{n}/{k}";p.Mechanics.DragonDoots=step.GetProperty("doots").GetDouble();p.Mechanics.DragonStarShop=step.GetProperty("star").GetBoolean();
                foreach(var e in step.GetProperty("drained").EnumerateArray()){string line=e.GetString()!;state.Pool.Take(line,state.Pool.Remaining(line));}
                p.Shop.FreeRolls=k%4==0?1:0;bool rolled=true;if(k%2==1)rolled=p.Shop.Reroll();else p.Shop.Refresh(k%3);
                Check(rolled==step.GetProperty("rolled").GetBoolean(),c+" refresh");Snapshot(step.GetProperty("afterRoll"),c+" rolled");
                Check(p.Shop.Buy(step.GetProperty("slot").GetInt32(),false)==step.GetProperty("bought").GetBoolean(),c+" buy");Snapshot(step.GetProperty("after"),c+" bought");
                if(step.GetProperty("sold").ValueKind!=JsonValueKind.Null)Check(p.Shop.Sell(step.GetProperty("sold").GetInt32(),false),c+" sell");Snapshot(step.GetProperty("final"),c+" final");k++;
            }
            Check(state.Random.Next()==trace.GetProperty("nextRandom").GetDouble(),rules.Id+" RNG");
        }
        Console.WriteLine($"Dragonlands shop parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} traces.");return checks;
    }
}

using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class FatesPartnerParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input=File.OpenRead(Path.Combine(root,"tests/fixtures/fates-partners.json.gz"));using var zip=new GZipStream(input,CompressionMode.Decompress);using var doc=JsonDocument.Parse(zip);
        int checks=0;void Check(bool ok,string c){checks++;if(!ok)throw new InvalidOperationException("Fates Partner parity: "+c);}
        foreach(var trace in doc.RootElement.EnumerateArray())
        {
            var rules=catalog.Variants.Single(v=>v.Id==trace.GetProperty("variant").GetString());int n=trace.GetProperty("n").GetInt32();
            var state=new MatchState(rules,trace.GetProperty("seed").GetUInt32(),[new("Player",trace.GetProperty("human").GetBoolean())]);var p=state.Players[0];
            var mode=new FatesPlanning(state);var commands=new PlanningShopCommands(mode);var round=new RoundPosition(trace.GetProperty("stage").GetInt32(),trace.GetProperty("round").GetInt32());
            p.Shop.RollHooks=mode.ShopHooks(p,()=>round);var phase=Enum.Parse<MatchPhase>(trace.GetProperty("phase").GetString()!,true);
            if(rules.Economy!.Xp is not null)p.Economy.AddXp(trace.GetProperty("xp").GetInt32());else p.Economy.BeginRound(round.Stage,1);
            p.Economy.Credit(500-p.Economy.Gold);if(n%2==1)p.Augments.Add("fat_seeker");p.Mechanics.FatesPartnerSeen=n%5==0;
            void Snapshot(JsonElement expected,string c)
            {
                Check(p.Economy.Gold==expected.GetProperty("gold").GetInt32(),c+" gold");Check(p.Mechanics.FatesPartnerSeen==expected.GetProperty("seen").GetBoolean(),c+" seen");
                Check(p.Shop.Slots.SequenceEqual(expected.GetProperty("shop").EnumerateArray().Select(e=>e.GetString())),c+" offers");
                var meta=expected.GetProperty("meta");
                for(int i=0;i<p.Shop.Slots.Count;i++)
                {
                    var actual=p.Shop.Metadata[i];var m=meta.ValueKind==JsonValueKind.Null?default:meta[i];
                    Check(m.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null?actual is null:actual is not null,c+" metadata presence");if(actual is null)continue;
                    Check(actual.Cost==m.GetProperty("cost").GetInt32()&&actual.Tag==m.GetProperty("tag").GetString(),c+" cost/tag");
                    var unit=m.GetProperty("unit");Check(actual.Unit!.Stage==unit.GetProperty("stage").GetInt32(),c+" stage override");Check(JsonElement.DeepEquals(actual.Unit.Mods!.Value,unit.GetProperty("mods")),c+" modifier override");Check(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(actual.Unit.BonusTraits),unit.GetProperty("bonusTraits")),c+" trait override");
                }
                for(int i=0;i<9;i++)
                {
                    var u=p.Roster.Bench[i];var e=expected.GetProperty("bench")[i];Check((u is null)==(e.ValueKind==JsonValueKind.Null),c+" presence");if(u is null)continue;
                    Check(u.Id==e.GetProperty("uid").GetInt32()&&u.Line==e.GetProperty("line").GetString()&&u.Stage==e.GetProperty("stage").GetInt32(),c+" unit");
                    Check(u.Items.SequenceEqual(e.GetProperty("items").EnumerateArray().Select(x=>x.GetString()!)),c+" items");
                    Check((u.Mods is null)==!e.TryGetProperty("mods",out var mods),c+" modifier presence");if(u.Mods is {} actual)Check(JsonElement.DeepEquals(actual,mods),c+" modifiers");
                    Check((u.BonusTraits is null)==!e.TryGetProperty("bonusTraits",out var bonus),c+" trait presence");if(u.BonusTraits is not null)Check(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(u.BonusTraits),bonus),c+" traits");
                }
                foreach(var pair in expected.GetProperty("pool").EnumerateObject())Check(state.Pool.Remaining(pair.Name)==pair.Value.GetInt32(),c+" pool "+pair.Name);
            }
            int k=0;foreach(var step in trace.GetProperty("steps").EnumerateArray())
            {
                string c=$"{rules.Id}/{p.IsHuman}/{n}/{k}";p.Shop.Refresh(k%3);Snapshot(step.GetProperty("rolled"),c+" rolled");
                foreach(var e in step.GetProperty("added").EnumerateArray()){var u=state.CreateUnit(e.GetProperty("line").GetString()!,e.GetProperty("stage").GetInt32());Check(u.Id==e.GetProperty("uid").GetInt32(),c+" injected ID");p.Roster.AddToBench(u);}
                if(step.GetProperty("drain").GetString() is {} drained)state.Pool.Take(drained,state.Pool.Remaining(drained));
                int slot=step.GetProperty("slot").GetInt32();Check((slot>=0&&commands.Buy(p,slot,phase))==step.GetProperty("bought").GetBoolean(),c+" buy");Snapshot(step.GetProperty("after"),c+" bought");
                if(step.GetProperty("sold").ValueKind!=JsonValueKind.Null)Check(commands.Sell(p,step.GetProperty("sold").GetInt32(),phase),c+" sell");Snapshot(step.GetProperty("final"),c+" final");k++;
            }
            Check(state.Random.Next()==trace.GetProperty("nextRandom").GetDouble(),rules.Id+" RNG");
        }
        Console.WriteLine($"Fates Partner parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} traces.");return checks;
    }
}

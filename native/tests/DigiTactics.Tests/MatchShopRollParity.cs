using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class MatchShopRollParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input=File.OpenRead(Path.Combine(root,"tests/fixtures/match-shop-rolls.json.gz"));using var zip=new GZipStream(input,CompressionMode.Decompress);using var doc=JsonDocument.Parse(zip);
        int checks=0;void Check(bool ok,string c){checks++;if(!ok)throw new InvalidOperationException("Match shop dispatch parity: "+c);}
        foreach(var trace in doc.RootElement.EnumerateArray())
        {
            var rules=catalog.Variants.Single(v=>v.Id==trace.GetProperty("variant").GetString());string mode=rules.Id.Split('/')[0];var round=RoundPosition.First;
            void Setup(MatchPlayer p,int n)
            {
                p.Augments.Clear();if(mode=="gizmos"){if(n%3==0)p.Augments.Add("giz_a_highend");if(n%3<2)p.Augments.Add("giz_a_ticket");}
                p.Globals.Clear();if(mode is "autochess" or "underlords")p.Globals.AddRange(Enumerable.Repeat(mode=="autochess"?"ac_r_recruiter":"ul_g_criminal",n%3));
                p.Mechanics.DragonDoots=n%5*5;p.Mechanics.DragonStarShop=n%4==0;
            }
            var state=new MatchState(rules,trace.GetProperty("seed").GetUInt32(),[new("Player",true),new("AI",false)],initialize:s=>
            {
                s.Galaxy="server";foreach(var p in s.Players){p.Economy.Credit(500-p.Economy.Gold);Setup(p,p.Id+3);}new MatchShopRolls(s,()=>round).Install();
            });
            void Snapshot(JsonElement expected,string c)
            {
                foreach(var p in state.Players)
                {
                    var e=expected.GetProperty("players")[p.Id];Check(p.Economy.Gold==e.GetProperty("gold").GetInt32(),c+" gold");Check(p.Shop.FreeRolls==e.GetProperty("rolls").GetInt32(),c+" free rolls");Check(p.Mechanics.FatesPartnerSeen==e.GetProperty("seen").GetBoolean(),c+" partner seen");Check(p.Mechanics.DragonStarShop==e.GetProperty("star").GetBoolean(),c+" star guarantee");
                    Check(p.Shop.Slots.SequenceEqual(e.GetProperty("shop").EnumerateArray().Select(x=>x.GetString())),c+" slots");var meta=e.GetProperty("meta");
                    for(int i=0;i<p.Shop.Slots.Count;i++)
                    {
                        var m=meta.ValueKind==JsonValueKind.Null?default:meta[i];var actual=p.Shop.Metadata[i];Check(m.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined?actual is null:actual is not null,c+" metadata presence");if(actual is null)continue;
                        Check(actual.Cost==m.GetProperty("cost").GetInt32()&&actual.Tag==m.GetProperty("tag").GetString(),c+" special cost/tag");
                        if(m.TryGetProperty("unit",out var unit)){Check(actual.Unit?.Stage==unit.GetProperty("stage").GetInt32(),c+" special stage");Check(JsonElement.DeepEquals(actual.Unit!.Mods!.Value,unit.GetProperty("mods")),c+" modifiers");Check(JsonElement.DeepEquals(JsonSerializer.SerializeToElement(actual.Unit.BonusTraits),unit.GetProperty("bonusTraits")),c+" bonus traits");}else Check(actual.Unit is null,c+" plain unit");
                    }
                }
                foreach(var pair in expected.GetProperty("pool").EnumerateObject())Check(state.Pool.Remaining(pair.Name)==pair.Value.GetInt32(),c+" pool "+pair.Name);
            }
            Snapshot(trace.GetProperty("initial"),rules.Id+" initial");int n=0;
            foreach(var step in trace.GetProperty("steps").EnumerateArray())
            {
                string c=$"{rules.Id}/{trace.GetProperty("seed")}/{n}";var p=state.Players[n%2];Setup(p,n);round=new(step.GetProperty("stage").GetInt32(),step.GetProperty("round").GetInt32());state.Galaxy=n%4<2?"server":"island";
                if(rules.Economy!.Xp is not null)p.Economy.AddXp(4);else {int gold=p.Economy.Gold;p.Economy.BeginRound(round.Stage,1);p.Economy.Credit(gold-p.Economy.Gold);}
                var added=step.GetProperty("added");if(added.ValueKind!=JsonValueKind.Null){var u=state.CreateUnit(added.GetProperty("line").GetString()!,2);Check(u.Id==added.GetProperty("uid").GetInt32(),c+" ID");p.Roster.AddToBench(u);}
                bool result=true;if(n%3==0)p.Shop.Refresh(n%4);else result=p.Shop.Reroll();Check(result==step.GetProperty("result").GetBoolean(),c+" roll result");Snapshot(step.GetProperty("after"),c);n++;
            }
            Check(state.Random.Next()==trace.GetProperty("nextRandom").GetDouble(),rules.Id+" RNG");
        }
        Console.WriteLine($"Match shop dispatch parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} traces.");return checks;
    }
}

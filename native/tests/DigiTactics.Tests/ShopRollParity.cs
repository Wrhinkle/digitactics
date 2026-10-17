using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class ShopRollParity
{
    private sealed class Hooks(Ruleset rules) : IShopRollHooks
    {
        public int Index { get; set; }
        public ShopRollRequest BeforeRoll(ShopRollRequest r) => r with { Level=r.Level+Index%3, Size=Index%12-2, Odds=Index%2==1?[10.5,20.5,30,25,14]:null };
        public void Rolled(PlayerShop shop)
        {
            shop.ReplaceOffer(0,rules.Lines[Index%rules.Lines.Length].Id);
            for(int i=0;i<shop.Slots.Count;i++)if(shop.Slots[i] is not null)
                shop.SetMetadata(i,new((Index+i)%6,"Special",new(Index%3,new(){["test"]=1},JsonSerializer.SerializeToElement(new{marker=Index}))));
        }
    }
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input=File.OpenRead(Path.Combine(root,"tests/fixtures/shop-rolls.json.gz"));using var zip=new GZipStream(input,CompressionMode.Decompress);using var doc=JsonDocument.Parse(zip);
        int checks=0;void Check(bool ok,string c){checks++;if(!ok)throw new InvalidOperationException("Shop roll parity: "+c);}
        foreach(var trace in doc.RootElement.EnumerateArray())
        {
            var rules=catalog.Variants.Single(v=>v.Id==trace.GetProperty("variant").GetString());var rng=new DeterministicRandom(91);var pool=new ShopPool(rules,rng);var economy=new PlayerEconomy(rules.Economy!);economy.Credit(500-economy.Gold);int uid=0;
            PlanningUnit Create(string line,int stage)=>new(++uid,line,stage,[]);
            var roster=new PlanningRoster(rules,Create);var shop=new PlayerShop(rules,pool,economy,roster,Create,_=>{});var hooks=new Hooks(rules);shop.RollHooks=hooks;int n=0;
            void Offers(JsonElement e,string c)=>Check(shop.Slots.SequenceEqual(e.EnumerateArray().Select(x=>x.GetString())),c+" offers");
            void Metadata(JsonElement e,string c)
            {
                if(e.ValueKind==JsonValueKind.Null){Check(shop.Metadata.All(m=>m is null),c+" cleared metadata");return;}
                Check(shop.Metadata.Count==e.GetArrayLength(),c+" metadata count");
                for(int i=0;i<e.GetArrayLength();i++)
                {
                    var m=shop.Metadata[i];var expected=e[i];Check(expected.ValueKind==JsonValueKind.Null?m is null:m is not null&&m.Cost==expected.GetProperty("cost").GetInt32()&&m.Tag==expected.GetProperty("tag").GetString()&&m.Unit?.Stage==expected.GetProperty("unit").GetProperty("stage").GetInt32()&&m.Unit.BonusTraits!["test"]==1&&m.Unit.Mods!.Value.GetProperty("marker").GetInt32()==expected.GetProperty("unit").GetProperty("mods").GetProperty("marker").GetInt32(),c+" metadata");
                }
            }
            foreach(var step in trace.GetProperty("steps").EnumerateArray())
            {
                string c=$"{rules.Id}/{n}";hooks.Index=n;
                if(n%10==0)foreach(var e in roster.Entries.ToArray())roster.Remove(e.Unit.Id,false);
                shop.FreeRolls=n%3==0?1:0;bool rolled=true;if(n%2==1)rolled=shop.Reroll();else shop.Refresh(n%4);
                Check(rolled==step.GetProperty("rolled").GetBoolean(),c+" roll result");
                if(step.GetProperty("dice").GetBoolean())Check(shop.RefreshMatchingTraits(rules.Lines.Single(l=>l.Id==step.GetProperty("diceLine").GetString()).Traits)==(step.GetProperty("diceResult").GetString()=="fat_dice"),c+" Loaded Dice");
                Offers(step.GetProperty("shop"),c);Metadata(step.GetProperty("meta"),c);
                int slot=0;foreach(var cost in step.GetProperty("costs").EnumerateArray())Check(shop.Cost(slot++)==cost.GetInt32(),c+" cost");
                Check(shop.Buy(step.GetProperty("slot").GetInt32(),false)==step.GetProperty("bought").GetBoolean(),c+" purchase");
                var after=step.GetProperty("after");Offers(after.GetProperty("shop"),c);Metadata(after.GetProperty("meta"),c);
                Check(economy.Gold==after.GetProperty("gold").GetInt32(),c+" gold");Check(shop.FreeRolls==after.GetProperty("freeRolls").GetInt32(),c+" rerolls");
                foreach(var pair in after.GetProperty("pool").EnumerateObject())Check(pool.Remaining(pair.Name)==pair.Value.GetInt32(),c+" pool "+pair.Name);
                for(int i=0;i<9;i++)
                {
                    var u=roster.Bench[i];var e=after.GetProperty("bench")[i];Check(e.ValueKind==JsonValueKind.Null?u is null:u is not null&&u.Id==e.GetProperty("uid").GetInt32()&&u.Line==e.GetProperty("line").GetString()&&u.Stage==e.GetProperty("stage").GetInt32(),c+" unit");
                    if(u is not null){Check(u.Items.Length==0,c+" items");Check(e.TryGetProperty("mods",out var mods)?u.Mods?.GetProperty("marker").GetInt32()==mods.GetProperty("marker").GetInt32():u.Mods is null,c+" modifiers");Check(e.TryGetProperty("bonusTraits",out var bonus)?u.BonusTraits?.GetValueOrDefault("test")==bonus.GetProperty("test").GetInt32():u.BonusTraits is null,c+" bonus traits");}
                }
                n++;
            }
            Check(rng.Next()==trace.GetProperty("nextRandom").GetDouble(),rules.Id+" RNG");
        }
        Console.WriteLine($"Shop roll parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} traces.");return checks;
    }
}

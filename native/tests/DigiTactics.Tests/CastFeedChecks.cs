using System.Net;
using System.Text.Json;
using DigiTactics.Presentation;
using DigiTactics.Rules;

internal static class CastFeedChecks
{
    public static void Run(string root)
    {
        using var doc=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"tests/fixtures/cast-feed.json")));
        int checks=0;
        void Check(bool ok,string reason){if(!ok)throw new InvalidOperationException("Cast feed: "+reason);checks++;}
        foreach(var color in doc.RootElement.GetProperty("colors").EnumerateObject())Check(CombatCastFeed.ElementColor(color.Name)==color.Value.GetString(),"original elemental color");
        foreach(var sample in doc.RootElement.GetProperty("scenarios").EnumerateArray())
        {
            var units=sample.GetProperty("casts").EnumerateArray().Select(c=>new CastFeedUnit(c.GetProperty("line").GetString()!,c.GetProperty("stage").GetInt32(),c.GetProperty("team").GetInt32(),c.GetProperty("name").GetString()!,c.GetProperty("ability").GetString()!,c.GetProperty("element").GetString()!)).ToArray();
            int speed=sample.GetProperty("speed").GetInt32();double life=sample.GetProperty("life").GetDouble(),removed=sample.GetProperty("removedAt").GetDouble();
            List<CombatEvent> events=[];
            var feed=new CombatCastFeed(events,id=>id>=0&&id<units.Length?units[id]:null);
            for(int i=0;i<units.Length;i++){events.Add(new(0,"damage",i));events.Add(new(0,"cast",i));}
            events.Add(new(0,"cast",-1));feed.Observe(speed);
            Check(feed.Rows.Count==5,"five newest casts only");
            var expected=sample.GetProperty("rows").EnumerateArray().ToArray();
            for(int i=0;i<5;i++)
            {
                var row=feed.Rows[i];var source=expected[i];
                Check(row.Unit==i+2&&row.Line+"/"+row.Stage==source.GetProperty("portrait").GetString(),"event order and portrait identity");
                Check(row.Name==source.GetProperty("name").GetString()&&row.Ability==WebUtility.HtmlDecode(source.GetProperty("ability").GetString()),"literal unit and ability label");
                Check(row.Color==source.GetProperty("color").GetString()&&row.Life==life,"element and speed at arrival");
                Check((row.Team==1)==source.GetProperty("className").GetString()!.Contains("enemy",StringComparison.Ordinal),"ally/enemy styling");
            }
            feed.Observe(4);Check(feed.Rows.Count==5&&feed.Rows.All(r=>r.Life==life),"no replay and no retroactive speed change");
            feed.Advance(life+.1);Check(feed.Rows.Count==5,"rows remain through original fade interval, even without simulation updates");
            feed.Advance(removed-life-.1+1e-8);Check(feed.Rows.Count==0,"wall-clock expiry");
            var scouting=new CombatCastFeed(events,id=>id>=0&&id<units.Length?units[id]:null,true);scouting.Observe(speed);Check(scouting.Rows.Count==0,"scouting skips past casts");
            events.Add(new(3,"cast",0));scouting.Observe(speed);Check(scouting.Rows.Single().Name=="Enemy "+units[0].Name&&scouting.Rows[0].Team==1,"mirrored team and enemy prefix");
            events.Add(new(3,"cast",1));scouting.Clear();scouting.Observe(speed);Check(scouting.Rows.Count==0,"skip clears queued and shown casts");
        }
        var catalog=ContentCatalog.Load(Path.Combine(root,"content/catalog.json"));var elements=new AbilityElements(Path.Combine(root,"content/ability-elements.json"));
        int actualCasts=0;
        foreach(var rules in catalog.Variants)
        {
            var side=rules.Lines.Take(7).Select((line,i)=>new CombatEntry(line.Id,i%3,i,4)).ToArray();
            var combat=new CombatSimulation(rules,side,side,143);
            var feed=new CombatCastFeed(combat,rules.Id,elements);
            for(int tick=0;tick<600&&!combat.Done;tick++)
            {
                combat.Step();feed.Advance(CombatSimulation.Tick);feed.Observe(1);
                foreach(var row in feed.Rows)
                {
                    var caster=combat.Units[row.Unit];
                    Check(row.Line==caster.Line.Id&&row.Stage==caster.Stage&&row.Ability==caster.Line.Ability.Names[caster.Stage],"actual combat cast identity");
                }
            }
            actualCasts+=combat.Events.Count(e=>e.Type=="cast");
        }
        Check(actualCasts>100,"live simulation casts across all variants");
        Console.WriteLine($"Cast feed passed {checks} checks: source callback ordering/colors/labels/1x-4x lifetimes, mirror, scouting/skip, wall-clock fade and {actualCasts} actual casts across 22 variants.");
    }
}

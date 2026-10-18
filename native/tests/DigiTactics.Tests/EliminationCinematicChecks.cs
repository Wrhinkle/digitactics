using System.Text.Json;
using System.Text.Json.Nodes;
using System.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;

internal static class EliminationCinematicChecks
{
    public static int Run(string root,ContentCatalog catalog)
    {
        var options=new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase,PropertyNameCaseInsensitive=true};
        using var data=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"tests/fixtures/elimination-cinematics.json")));
        int checks=0;EliminationEvent? sample=null;
        void Check(bool value,string message){if(!value)throw new InvalidOperationException(message);checks++;}
        foreach(var row in data.RootElement.GetProperty("records").EnumerateArray())
        {
            var input=row.GetProperty("rounds").Deserialize<EliminationRound[]>(options)!;
            var actual=EliminationEvents.Resolve(input);
            Check(JsonNode.DeepEquals(JsonSerializer.SerializeToNode(actual,options),JsonNode.Parse(row.GetProperty("expected").GetRawText())),"Elimination attribution/lineup differs from source scenario "+checks);
            sample??=actual.FirstOrDefault(e=>e.Killer is not null&&e.FinalHit is not null);
        }
        foreach(var row in data.RootElement.GetProperty("layouts").EnumerateArray())
        {
            var layout=EliminationLayout.Create(row.GetProperty("n").GetInt32(),row.GetProperty("aspect").GetDouble());
            Check(Math.Abs(layout.HalfWidth-row.GetProperty("halfWidth").GetDouble())<1e-12&&Math.Abs(layout.Top-row.GetProperty("top").GetDouble())<1e-12&&layout.Narrow==row.GetProperty("narrow").GetBoolean(),"Source lineup dimensions differ.");
            Check(JsonNode.DeepEquals(JsonSerializer.SerializeToNode(layout.Slots,options),JsonNode.Parse(row.GetProperty("slots").GetRawText())),"Source lineup placements differ.");
        }
        foreach(var row in data.RootElement.GetProperty("pops").EnumerateArray())
        {
            double size=EliminationLayout.PopScale(row.GetProperty("time").GetDouble(),row.GetProperty("born").GetDouble(),new(0,0,0,row.GetProperty("slotScale").GetDouble()));
            Check(Math.Abs(size-row.GetProperty("scale").GetDouble())<1e-12,"Source lineup pop-in differs.");
        }
        foreach(double aspect in new[]{.8,16d/9})
        {
            var camera=new EliminationCamera();var layout=EliminationLayout.Create(2,aspect);
            var original=camera.Lineup(layout,aspect,1.05,true,1d/60);
            Vector3[] ordinary=[new(-.3f,0,0),new(.3f,1,0)];
            Check(original==camera.Contain(ordinary,aspect,1d/60),"Ordinary dance bodies changed the source camera framing.");
            // The original upright estimate misses spinning tails and downward weapons.
            Vector3[] expanded=[new(-2.4f,-1.6f,.5f),new(2.4f,2.7f,.5f),new(-2.4f,2.7f,-1),new(2.4f,-1.6f,-1)];
            var safe=camera.Contain(expanded,aspect,1d/60);
            foreach(var point in expanded)
            {
                var projected=Vector4.Transform(new Vector4(point,1),safe);
                Check(projected.W>0&&Math.Abs(projected.X/projected.W)<=.841&&Math.Abs(projected.Y/projected.W)<=.841,"Animated body or weapon leaves the fitted dance viewport.");
            }
            double before=Vector3.Distance(camera.Eye,camera.Target);
            camera.Lineup(layout,aspect,1.05,true,1d/60);camera.Contain(ordinary,aspect,1d/60);
            Check(Vector3.Distance(camera.Eye,camera.Target)>before*.9,"Dance camera snaps inward after a large pose.");
        }
        var ev=sample!;var big=new EliminationPlayback(ev,ev.Victim.Id);var small=new EliminationPlayback(ev,-1);
        Check(big.Big&&big.Cinematic&&big.Duration==7.5&&small.Duration==2.8&&!small.Cinematic,"Local final-hit cinematic durations differ.");
        var pve=ev with{Killer=null,Killers=[],FinalHit=null,Team=[]};
        Check(new EliminationPlayback(pve,pve.Victim.Id).Duration==2&&!new EliminationPlayback(pve,pve.Victim.Id).Cinematic,"PvE must be a neutral two-second sting.");
        var partner=ev with{Victims=[ev.Victim,ev.Victim with{Id=99}],Killers=[ev.Killer!,ev.Killer! with{Id=100}]};
        Check(new EliminationPlayback(partner,99).Big&&new EliminationPlayback(partner,100).Cinematic,"Both Duos partners must see the full cinematic.");
        var queue=new EliminationQueue(ev.Victim.Id);var second=pve with{Victim=pve.Victim with{Id=87}};
        queue.Enqueue([ev,second]);queue.Enqueue([ev,second]);Check(queue.PendingCount==1,"Repeated round-end must not replay eliminations.");
        queue.Advance(10);Check(Math.Abs(queue.Current!.Time-.1)<1e-12,"Long frames must not consume the cinematic.");
        queue.Advance(.1,true);Check(Math.Abs(queue.Current!.Time-.1)<1e-12,"Pause must freeze cinematic time.");
        queue.Skip();Check(queue.Current!.Event==second&&queue.PendingCount==0,"Skip must advance exactly one event.");
        queue.Clear();Check(queue.Current is null&&queue.PendingCount==0,"Leaving game must clear pending scenes.");
        for(int i=0;i<35;i++)big.Advance(.1);Check(!big.Attack&&Math.Abs(big.DanceTime)<1e-12,"Attack must hand off to dancing at 3.5 seconds.");
        for(int i=0;i<41;i++)big.Advance(.1);Check(big.Complete&&big.Opacity==0,"Cinematic must expire and fade.");
        var rules=catalog.Variants[0];var line=rules.Lines[0];
        var battle=new CombatSimulation(rules,[new(line.Id,0,2,4,111,0),new(line.Id,0,3,4,112,0)],[new(line.Id,0,2,4,211,1)],1,
            configureHooks:registry=>
            {
                var context=registry.Context(CombatHookSource.Mechanic,"final-hit-test");
                registry.Add(context,CombatHook.CombatReady,(ctx,_)=>
                {
                    var killer=ctx.Combat.Units[0];ctx.Kill(ctx.Combat.Units[1],killer);
                    Check(ctx.Combat.FinalHits[0] is null,"Friendly casualties must not get final-hit credit.");
                    ctx.Kill(ctx.Combat.Units[2],killer);
                });
                registry.Add(context,CombatHook.Death,(ctx,e)=>{if(e.Unit?.Team==1&&e.Source is { } attacker)ctx.Grow(attacker);});
            });
        var final=battle.FinalHits[0];Check(final is{Uid:111,Owner:0,Victim:2,Stage:0}&&final.Line==line.Id&&battle.Units[0].Stage==1,"Final hit must freeze the attacker before death hooks transform it.");
        Check(battle.FinalHits[1] is null,"Losing side received fabricated final-hit credit.");
        using var creatures=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"content/placeholder-creatures.json")));
        Check(creatures.RootElement.GetProperty("parts").GetArrayLength()==11&&creatures.RootElement.GetProperty("colors").EnumerateObject().Count()==catalog.Variants.Length,"Original fallback creatures were not exported for every ruleset.");
        Console.WriteLine($"Elimination cinematics: {checks} source attribution/layout/pop-in, queue, duration, Duos, PvE and final-hit snapshot checks passed.");return checks;
    }
}

using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed class EliminationCinematicSmoke
{
    private int step,frames;
    private bool mouseDown,keyDown;
    private readonly HashSet<string> captured=[];
    private readonly Dictionary<int,double> signatures=[];
    private bool animated,seenAttack,seenEffects,seenDance,seenSmall,seenPve,seenDuos;
    private int pauseStep,pauseFrames;
    private double? pausedTime;
    public void Input(MatchScreen screen)
    {
        if(mouseDown){ImGui.GetIO().AddMouseButtonEvent(0,false);mouseDown=false;return;}
        if(keyDown){ImGui.GetIO().AddKeyEvent(ImGuiKey.Escape,false);keyDown=false;return;}
        if(step==0)
        {
            if(!screen.Buttons.TryGetValue("start",out var start))return;
            ImGui.GetIO().AddMousePosEvent(start.X,start.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);mouseDown=true;step++;return;
        }
        if(step==1&&screen.Session is { } session)
        {
            // The source default now starts with an opening carousel. Finish it
            // through the real controls before exercising the game menu pause.
            var carousel=session.Units.Carousel;
            if(carousel.Active)
            {
                if(carousel.Done&&screen.Buttons.TryGetValue("carouselGo",out var next))Click(next);
                else if(carousel.Turn?.Id==0)
                {
                    int choice=carousel.Options.Select((option,index)=>(option,index)).First(pair=>pair.option.PickedBy is null).index;
                    if(screen.Buttons.TryGetValue("carousel/"+choice,out var pick))Click(pick);
                }
                return;
            }
            var rules=session.State.Rules;var models=ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory,"content/models.json"));
            var lines=rules.Lines.Take(16).ToArray();
            EliminationMember[] team=lines.Select((l,i)=>new EliminationMember(l.Id,i%3,i+1,0,i==0)).ToArray();
            // Include the source fallback creature if this ruleset has an unassigned form.
            var missing=rules.Lines.SelectMany(l=>Enumerable.Range(0,3).Select(s=>(Line:l,Stage:s))).FirstOrDefault(p=>models.Resolve(rules.Id,p.Line.Id,p.Stage) is null);
            if(missing.Line is not null)team[^1]=new(missing.Line.Id,missing.Stage,99,0);
            EliminationPlayer P(int id,bool alive)=>new(id,$"Tamer {id}",alive,alive?0:8-id,session.State.Players[id].Team,alive?team:[]);
            var killer=P(0,true);var victim=P(1,false);
            screen.DemoElimination(new(victim,killer,false,[victim],[killer],team[0],team.Take(4).ToArray()));
            killer=P(3,true);victim=P(2,false);
            screen.DemoElimination(new(victim,killer,true,[victim],[killer],team[0],team.Take(3).ToArray()));
            victim=P(4,false);screen.DemoElimination(new(victim,null,false,[victim],[],null,[]));
            killer=P(6,true);victim=P(5,false);screen.DemoElimination(new(victim,killer,false,[victim,P(7,false)],[killer,P(0,true)],team[0],team));
            step++;
        }
        if(step==2 && screen.EliminationPlayback is { } playback)
        {
            if(playback.Event.Victim.Id==2)
            {
                // The original top bar has no Pause button: pause through the game menu.
                if(pauseStep==0&&playback.Time>.55&&screen.Buttons.TryGetValue("gameMenu",out var menu))
                {Click(menu);pauseStep=3;return;}
                if(pauseStep==3&&screen.Buttons.TryGetValue("menuPause",out var pause))
                {Click(pause);pauseStep=1;return;}
                if(pauseStep==1&&screen.IsPaused)
                {
                    pausedTime??=playback.Time;
                    if(Math.Abs(playback.Time-pausedTime.Value)>1e-12)throw new InvalidOperationException("Paused cinematic kept advancing.");
                    if(++pauseFrames>15&&screen.Buttons.TryGetValue("resume",out var resume)){Click(resume);pauseStep=2;}
                    return;
                }
            }
            // The small card is dismissed by a real pointer click after its render assertions.
            if(playback.Event.Victim.Id==2 && pauseStep==2 && !screen.IsPaused && playback.Time>.8 && screen.Buttons.TryGetValue("skipElimination",out var skip))
            {Click(skip);}
            // The neutral sting is dismissed with the same Escape shortcut as the original.
            if(playback.Event.Victim.Id==4 && playback.Time>.7){ImGui.GetIO().AddKeyEvent(ImGuiKey.Escape,true);keyDown=true;}
        }
        void Click(float2 at){ImGui.GetIO().AddMousePosEvent(at.X,at.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);mouseDown=true;}
    }
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(++frames>1000)throw new InvalidOperationException($"Elimination cinematic smoke stalled: step={step}, pauseStep={pauseStep}, paused={screen.IsPaused}, victim={screen.EliminationPlayback?.Event.Victim.Id}.");
        if(screen.EliminationPlayback is { } p)
        {
            int victim=p.Event.Victim.Id;
            if(p.Event.Killer is not null && p.Time>.3 && (screen.EliminationMembers<1||screen.EliminationDrawCalls<1))throw new InvalidOperationException("Elimination lineup did not render native models.");
            if(p.Attack&&p.Time>.8)
            {
                if(!screen.EliminationFired)throw new InvalidOperationException("Final attack never fired.");
                seenEffects|=screen.EliminationEffectDrawCalls>0;
                seenAttack=true;Capture("attack");
            }
            if(!p.Attack&&p.Event.Killer is not null&&p.DanceTime>.5)
            {
                int expected=p.Event.Team.Length+1;
                if(screen.EliminationMembers!=expected)throw new InvalidOperationException($"Elimination lineup is incomplete: {screen.EliminationMembers}/{expected}.");
                double signature=screen.EliminationPoseSignature;
                if(signatures.TryGetValue(victim,out var previous)&&Math.Abs(signature-previous)>1e-6)animated=true;
                signatures[victim]=signature;seenDance=true;
                if(victim!=5||p.DanceTime>1.8)Capture(victim==5?"duos-lineup":"lineup");
            }
            if(victim==2&&p.Time>.5){if(p.Big||p.Cinematic)throw new InvalidOperationException("Other-player elimination took over the arena.");seenSmall=true;Capture("small");}
            if(victim==2&&screen.IsPaused)Capture("paused");
            if(victim==4&&p.Time>.4){if(p.Event.Killer is not null||screen.EliminationMembers!=0)throw new InvalidOperationException("PvE elimination displayed a killer.");seenPve=true;Capture("pve");}
            if(victim==5&&!p.Attack&&p.DanceTime>.7){if(!p.Big||screen.EliminationMembers!=17)throw new InvalidOperationException("Duos final hit omitted partner or board actors.");seenDuos=true;}
            void Capture(string key){if(capture is not null&&captured.Add(key))FrameCapture.SaveBitmap(Path.ChangeExtension(capture,key+".bmp"),size.X,size.Y);}
        }
        else if(step==2)
        {
            if(!seenAttack||!seenEffects||!seenDance||!seenSmall||!seenPve||!seenDuos||!animated||pauseStep!=2)throw new InvalidOperationException($"Elimination smoke missed a required scene: attack={seenAttack}, effects={seenEffects}, dance={seenDance}, small={seenSmall}, pve={seenPve}, duos={seenDuos}, animated={animated}, pauseStep={pauseStep}.");
            if(screen.Session is null||screen.Session.Rounds.Phase!=MatchPhase.Planning)throw new InvalidOperationException("Client-only cinematics changed the match state.");
            PlaceholderCreatureSmoke.Run(size,capture);
            Console.WriteLine("Elimination cinematic UI: final attack with native move effects, complete 17-actor Duos dance, animated poses, small/PvE cards, real pause/resume, click/Escape skips and queue expiry passed.");return true;
        }
        return false;
    }
}

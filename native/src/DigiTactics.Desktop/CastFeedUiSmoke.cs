using System.Reflection;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Real simulation casts and live playback, followed by scouting and the actual Skip button.</summary>
internal sealed class CastFeedUiSmoke(ContentCatalog catalog)
{
    private static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    private int step,frames,click;
    private MatchBattle? match;
    public string Pending=>$"cast feed step {step}, frame {frames}";
    private static void Invoke(MatchScreen screen,string method,params object[] args)=>typeof(MatchScreen).GetMethod(method,Private)!.Invoke(screen,args);
    private static void Pause(MatchScreen screen,bool value)=>typeof(MatchScreen).GetField("localPaused",Private)!.SetValue(screen,value);
    private static void Prime(CombatSimulation combat)
    {
        foreach(var unit in combat.Units)
        {
            typeof(CombatUnit).GetProperty(nameof(CombatUnit.MaxHealth))!.SetValue(unit,1e6);
            typeof(CombatUnit).GetProperty(nameof(CombatUnit.Health))!.SetValue(unit,1e6);
            typeof(CombatUnit).GetProperty(nameof(CombatUnit.Mana))!.SetValue(unit,unit.MaxMana);
            typeof(CombatUnit).GetProperty(nameof(CombatUnit.AttackCooldown))!.SetValue(unit,0d);
            typeof(CombatUnit).GetProperty(nameof(CombatUnit.CastLock))!.SetValue(unit,0d);
        }
    }
    public void Input(MatchScreen screen)
    {
        frames++;
        if(step==0)
        {
            var rules=catalog.Variants.Single(r=>r.Id=="fates/normal");
            var session=new MatchPlanningSession(rules,143,Enumerable.Range(0,8).Select(i=>new MatchSeat("Cast Tamer "+i,true)).ToArray());
            while(session.Rounds.RoundType!="pvp"){session.StartCombat();session.FinishCombat();session.Advance();}
            foreach(var player in session.State.Players)
            {
                foreach(var old in player.Roster.Entries.ToArray())player.Roster.Remove(old.Unit.Id,false);
                for(int i=0;i<5;i++){var unit=session.State.CreateUnit(rules.Lines[i].Id,i%3);player.Roster.AddToBench(unit);player.Roster.Move(UnitSpot.Bench(0),UnitSpot.Board(i,4),false,()=>9);}
            }
            typeof(MatchScreen).GetProperty(nameof(MatchScreen.Session))!.SetValue(screen,session);
            session.StartCombat();Invoke(screen,"CombatStarted");
            match=session.Battles.First(b=>b.A==0||b.B==0);Invoke(screen,"Watch",match.A);Prime(match.Combat);Pause(screen,false);
            ImGui.GetIO().AddMousePosEvent(0,0);step=1;frames=0;
        }
        else if(step==1&&screen.CastFeedRows.Count==5){Pause(screen,true);step=2;frames=0;}
        else if(step==3)
        {
            if(Click(screen,"replaySpeed")){if(screen.ReplaySpeed!=2)throw new InvalidOperationException("Cast feed playback speed control failed.");step=4;frames=0;}
        }
        else if(step==4&&screen.CastFeedRows.Count==0){Prime(match!.Combat);Pause(screen,false);step=5;frames=0;}
        else if(step==5&&screen.CastFeedRows.Count==5&&screen.CastFeedRows.All(r=>r.Life==1.3)){Pause(screen,true);step=6;frames=0;}
        else if(step==7)
        {
            Invoke(screen,"Watch",match!.B!.Value);
            if(screen.CastFeedRows.Count!=0||!screen.WatchingMirrored)throw new InvalidOperationException("Scouting did not clear cast history/mirror the feed.");
            Prime(match.Combat);Pause(screen,false);step=8;frames=0;
        }
        else if(step==8&&screen.CastFeedRows.Count==5){Pause(screen,true);step=9;frames=0;}
        else if(step==10&&Click(screen,"finish"))step=11;
        if(frames>240)throw new InvalidOperationException("Cast feed live playback timed out: "+Pending);
    }
    private bool Click(MatchScreen screen,string key)
    {
        if(click==2){click=0;return true;}
        if(!screen.Buttons.TryGetValue(key,out var at))throw new InvalidOperationException("Missing cast-feed control "+key);
        ImGui.GetIO().AddMousePosEvent(at.X,at.Y);ImGui.GetIO().AddMouseButtonEvent(0,click==0);click++;return false;
    }
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(step is 2 or 6 or 9&&frames>=8)
        {
            int expected=Math.Min(5,Math.Max(0,(size.Y-506-16)/46));
            if(screen.CastFeedRows.Count!=5||screen.CastFeedDraws!=expected)throw new InvalidOperationException("The bounded cast feed did not render all available rows.");
            var bounds=screen.CastFeedBounds;
            if(bounds.Min.X<266||bounds.Min.Y<144||bounds.Max.X>size.X-266||bounds.Max.Y>size.Y-362)throw new InvalidOperationException("Cast feed escaped the arena or overlapped match/shop controls.");
            foreach(var row in screen.CastFeedRows)
            {
                var unit=match!.Combat.Units[row.Unit];int team=screen.WatchingMirrored?1-unit.Team:unit.Team;
                if(row.Team!=team||row.Name!=(team==0?"":"Enemy ")+unit.Line.Forms[unit.Stage]||row.Ability!=unit.Line.Ability.Names[unit.Stage])throw new InvalidOperationException("Rendered cast feed identity differs from live combat.");
            }
            if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,(step==2?"1x":step==6?"2x":"mirrored")+".bmp"),size.X,size.Y);
            step++;frames=0;
        }
        if(step!=11)return false;
        if(screen.CastFeedRows.Count!=0||screen.CastFeedDraws!=0)throw new InvalidOperationException("Skip replay left cast feed rows behind.");
        Console.WriteLine("Cast feed UI passed: actual combat event feed, newest five, visible bounded portraits/labels, 1x/2x speed, expiry while paused, mirrored scouting reset, and actual Skip replay control.");return true;
    }
}

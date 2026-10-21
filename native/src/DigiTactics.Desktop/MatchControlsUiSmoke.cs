using System.Reflection;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Source-shaped match controls, independent of client-page navigation.</summary>
internal sealed class MatchControlsUiSmoke(ContentCatalog catalog,bool duos)
{
    private static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    private readonly MatchHudSmoke hud=new();
    private readonly ShopControlSmoke shop=new();
    private readonly ItemDragSmoke inventory=new();
    private readonly HashSet<string> captured=[];
    private int phase,wait=3;
    private bool down;
    private string? captureRequested;
    internal string Pending=>$"match controls phase {phase}";
    private static void Invoke(MatchScreen screen,string name)=>typeof(MatchScreen).GetMethod(name,Private)!.Invoke(screen,null);

    internal void Input(MatchScreen screen)
    {
        if(wait-->0)return;
        if(phase==0)
        {
            Invoke(screen,"LeaveMatch");
            var rules=catalog.Variants.Single(r=>r.Id=="faction-wars/normal");
            var session=new MatchPlanningSession(rules,461u,Enumerable.Range(0,8).Select(i=>new MatchSeat("Control Tamer "+i,i==0)).ToArray(),duos);
            if(session.Rounds.RoundType=="carousel"){session.StartCombat();session.FinishCombat();session.Advance();}
            var player=session.State.Players[0];
            foreach(var old in player.Roster.Entries.ToArray())player.Roster.Remove(old.Unit.Id,false);
            foreach(var line in rules.Lines.Where(l=>!l.NoItems&&!l.NoSell).Take(3))player.Roster.AddToBench(session.State.CreateUnit(line.Id,0));
            player.Roster.Move(UnitSpot.Bench(0),UnitSpot.Board(3,4),false,()=>9);
            player.Economy.Credit(100);player.Items.Clear();
            player.Items.AddRange(rules.Items.Where(item=>item.Component).Take(3).Select(item=>item.Id));
            if(player.Offer is not null)session.Offers.Choose(player,0,false);
            typeof(MatchScreen).GetProperty(nameof(MatchScreen.Session))!.SetValue(screen,session);
            typeof(MatchScreen).GetField("localPaused",Private)!.SetValue(screen,true);
            typeof(MatchScreen).GetProperty("PlanningTimerEnabled",Private)!.SetValue(screen,false);
            ImGui.GetIO().AddMousePosEvent(0,0);phase=1;wait=4;captureRequested="planning";return;
        }
        if(phase==1)
        {
            hud.Input(screen);if(hud.CaptureRequested is {} capture){captureRequested=capture;hud.CaptureRequested=null;}
            if(hud.Done){phase=2;wait=4;}return;
        }
        if(phase==2)
        {
            if(shop.Input(screen)){phase=3;captureRequested="shop-controls";}
            wait=3;return;
        }
        if(phase==3)
        {
            bool done=inventory.Input(screen);
            if(inventory.CaptureName is {} capture)captureRequested=capture;
            if(done){phase=4;wait=4;}else wait=3;
            return;
        }
        if(phase is 4 or 5)
        {
            if(down)
            {
                ImGui.GetIO().AddMouseButtonEvent(0,false);down=false;phase++;wait=4;return;
            }
            string key=phase==4?"fight":"finish";
            if(!screen.Buttons.TryGetValue(key,out var at))throw new InvalidOperationException("Missing match control: "+key);
            ImGui.GetIO().AddMousePosEvent(at.X,at.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);down=true;wait=3;
        }
    }

    internal bool Verify(MatchScreen screen,int2 viewport,string? capture)
    {
        if(captureRequested is {} scene)
        {
            if(capture is not null&&captured.Add(scene))FrameCapture.SaveBitmap(Path.ChangeExtension(capture,scene+".bmp"),viewport.X,viewport.Y);
            captureRequested=null;
        }
        if(phase<6||wait>0)return false;
        if(screen.Session?.Rounds.Phase!=MatchPhase.Result||screen.ResultBannerMax.Y<=screen.ResultBannerMin.Y)
            throw new InvalidOperationException("Fight and Skip controls did not reach a visible round result.");
        if(screen.ResultBannerMin.X<screen.TeamSidebarBounds.Max.X||screen.ResultBannerMax.X>screen.ScoutingBounds.Min.X||screen.ResultBannerMin.Y<screen.TeamSidebarBounds.Min.Y||screen.ResultBannerMax.Y>screen.TeamSidebarBounds.Max.Y)
            throw new InvalidOperationException("Round result banner escaped the arena.");
        if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"round-result.bmp"),viewport.X,viewport.Y);
        Console.WriteLine("Match controls UI passed: timeline/opponent/standings hover and scouting, XP and refresh mouse/keyboard, hovered-unit sale, inventory drawer, item recipes/swap/equip/partner/overflow, drawer close, Fight/Skip and bounded round result.");
        return true;
    }
}

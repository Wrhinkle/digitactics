using System.Reflection;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;
using DigiTactics.Storage;
using DigiTactics.Networking;
using System.Text.Json.Nodes;

namespace DigiTactics.Desktop;

/// <summary>Real rendered result panels with isolated deterministic knockout fixtures and mouse-driven navigation.</summary>
internal sealed class PostgameSmoke(ContentCatalog catalog,NativeProfileStore profile)
{
    private int scenario,step;
    private string saved="";
    private MatchPlanningSession? original;
    private NetworkRoom? originalRoom;
    private static readonly FieldInfo NetworkBinding=typeof(MatchScreen).GetField("networkRoom",BindingFlags.Instance|BindingFlags.NonPublic)!;
    private bool outNow=>scenario is 1 or 3 or 9 or 10;
    private bool online=>scenario>=6;
    private bool dismiss=>scenario is 4 or 5 or 6 or 7 or 10;
    public string Pending=>$"postgame scenario {scenario}, step {step}";
    public void Input(MatchScreen screen)
    {
        void Click(string key)
        {
            if(!screen.Buttons.TryGetValue(key,out var point))throw new InvalidOperationException("Postgame button missing: "+key);
            ImGui.GetIO().AddMousePosEvent(point.X,point.Y); ImGui.GetIO().AddMouseButtonEvent(0,true);
        }
        if(step==0)
        {
            screen.DetachNetworkMatch();originalRoom=null;
            if(online)
            {
                var lobby=JsonNode.Parse("{\"host\":\"123\",\"hostName\":\"Result Tamer\",\"name\":\"Result room\",\"status\":\"playing\",\"slots\":{\"0\":{\"uid\":\"123\",\"name\":\"Result Tamer\"},\"1\":{\"uid\":\"234\",\"name\":\"Peer\"}}}"u8)!.AsObject();
                originalRoom=new("999","123",lobby);originalRoom.Members(["123","234"],"123",10000);
                screen.AttachNetworkMatch(originalRoom,()=>10000);
            }
            var session=new MatchPlanningSession(catalog.Variants[0],(uint)(900+scenario),Enumerable.Range(0,8).Select(i=>new MatchSeat(i==0?"Result Tamer":"Opponent "+i,i==0)).ToArray(),scenario is 2 or 3);
            var player=session.State.Players[0];
            foreach(var old in player.Roster.Entries.ToArray())player.Roster.Remove(old.Unit.Id,false);
            for(int i=0;i<3;i++)
            {
                var unit=session.State.CreateUnit(session.State.Rules.Lines[i].Id,i);
                player.Roster.AddToBench(unit); player.Roster.Move(UnitSpot.Bench(0),UnitSpot.Board(i,4),false,()=>9);
            }
            var recorder=new MatchHistoryRecorder(session,profile); recorder.Capture();
            session.StartCombat();
            if(session.State.Duos)
            {
                foreach(var team in session.State.Teams.Where(t=>outNow?t.Id==0:t.Id!=0))session.State.DamageTeam(team.Id,10000);
            }
            else foreach(var target in session.State.Players.Where(p=>outNow?p.Id==0:p.Id!=0))target.Health=-10000;
            session.FinishCombat();
            if(!session.State.Over || !recorder.Complete(DateTimeOffset.Now.ToUnixTimeMilliseconds()))throw new InvalidOperationException("Postgame fixture failed to complete.");
            typeof(MatchScreen).GetProperty(nameof(MatchScreen.Session))!.SetValue(screen,session);
            typeof(MatchScreen).GetField("history",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(screen,recorder);
            original=session; saved=profile.Storage["dt.client.v1"];
        }
        if(step==95)Click("resultHistory");
        if(step==96 || step==100 || step==106 || step==110 || step==115)ImGui.GetIO().AddMouseButtonEvent(0,false);
        if(step==99)Click("closeProfile");
        if(step==105)Click("resultStore");
        if(step==109)Click("closeCollection");
        if(step==114)Click(scenario is 4 or 6 or 10?"resultDismiss":scenario is 5 or 7 or 9?"menu":outNow?"spectateRemaining":"resultAgain");
        if(step==140 && dismiss)Click("start");
        if(step==141 && dismiss)ImGui.GetIO().AddMouseButtonEvent(0,false);
        step++;
    }
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(step==94)
        {
            if(!screen.PostgameVisible || screen.PostgameStandings!=8 || screen.PostgameUnitPortraits!=3 || screen.PostgameMissionCount!=2 || screen.PostgameAvatarCount!=8 || screen.PostgameXpGain<=0 || screen.PostgameBits is not >0 || screen.PostgameDisplayedBits!=screen.PostgameBits)
                throw new InvalidOperationException($"Incomplete postgame panel: visible={screen.PostgameVisible}, rows={screen.PostgameStandings}, units={screen.PostgameUnitPortraits}, missions={screen.PostgameMissionCount}, avatars={screen.PostgameAvatarCount}, xp={screen.PostgameXpGain}, bits={screen.PostgameBits}.");
            if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,$"{scenario}.bmp"),size.X,size.Y);
        }
        if(step==98 && !screen.PostgameHistoryVisible)throw new InvalidOperationException("Match history navigation failed.");
        if(step==102 && (screen.PostgameHistoryVisible || profile.Storage["dt.client.v1"]!=saved))throw new InvalidOperationException("Return/redraw altered saved match rewards.");
        if(step==108 && !screen.PostgameStoreVisible)throw new InvalidOperationException("Result Store navigation failed.");
        if(step==112 && screen.PostgameStoreVisible)throw new InvalidOperationException("Store did not return to results.");
        if(step<138)return false;
        if(dismiss && step==138)
        {
            if(!ReferenceEquals(screen.Session,original) || !screen.PostgameHomeVisible || screen.PostgameVisible || !screen.Buttons.ContainsKey("homeOverview"))
                throw new InvalidOperationException("Dismissing results did not retain the session on Home without reopening results.");
            if(!ReferenceEquals(NetworkBinding.GetValue(screen),originalRoom))throw new InvalidOperationException("Dismissing results detached the multiplayer room.");
            if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,$"{scenario}.home.bmp"),size.X,size.Y);
        }
        else if(!dismiss && online)
        {
            if(screen.Session is not null || NetworkBinding.GetValue(screen) is not null || screen.PostgameHomeVisible)
                throw new InvalidOperationException("Explicit result Leave/Back to lobbies did not detach the multiplayer session.");
        }
        else if(!dismiss && (outNow ? !screen.WatchingAfterElimination || screen.Session!.State.Over : ReferenceEquals(screen.Session,original) || screen.Session is null || screen.Session.State.Over))
            throw new InvalidOperationException("Postgame play again/spectate action did not resume the expected match.");
        if(dismiss && step<146)return false;
        if(dismiss && (online ? !ReferenceEquals(screen.Session,original) || !ReferenceEquals(NetworkBinding.GetValue(screen),originalRoom)
            : ReferenceEquals(screen.Session,original) || screen.Session is null || screen.Session.State.Over || screen.PostgameHomeVisible))
            throw new InvalidOperationException("Home Play did not replace a finished local result or failed to protect the retained multiplayer session.");
        if(profile.Storage["dt.client.v1"]!=saved)throw new InvalidOperationException("Postgame navigation paid rewards again.");
        if(++scenario<11){step=0;return false;}
        Console.WriteLine("Postgame UI passed: Solo/Duos victory and elimination, standings, frozen compositions, XP, missions, Bits count-up, profile/store navigation, play again and spectate. X/Home retains local and multiplayer sessions; explicit Leave/Back to lobbies detaches. Eleven scenarios without duplicate payouts.");return true;
    }
}

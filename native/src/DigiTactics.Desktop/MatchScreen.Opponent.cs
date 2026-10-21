using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal string? HeaderOpponentName {get;private set;}
    internal string? HeaderOpponentTooltip {get;private set;}
    internal (float2 Min,float2 Max) HeaderOpponentBounds {get;private set;}
    private void OpponentCard()
    {
        HeaderOpponentName=null;HeaderOpponentTooltip=null;
        var session=Session!;var opponent=StandingOpponent(session.State.Players[viewed]);
        bool pve=session.Rounds.RoundType=="pve";
        if(!pve&&opponent is null)return;
        string label=pve?"Opponent":session.Rounds.Phase==DigiTactics.Rules.MatchPhase.Planning?"Next opponent":"Fighting";
        string name=pve?"Wild Digimon":opponent!.Value.Player.Name;
        if(!pve&&session.State.Duos)name+=" + "+session.State.Teams[opponent!.Value.Player.Team!.Value].Members.First(p=>p.Id!=opponent.Value.Player.Id).Name;
        string health=pve?"":Math.Max(0,opponent!.Value.Player.Health)+" HP";
        string detail=label+(opponent?.Ghost==true?" (ghost copy)":"")+" / "+name+(pve?"":" / "+health);
        var at=new float2(TimelineBounds.Max.X+132,10);var end=at+new float2(154,44);
        HeaderOpponentName=name;HeaderOpponentBounds=(at,end);
        ImGui.SetCursorScreenPos(at);ImGui.InvisibleButton("headerOpponent"u8,end-at);
        Buttons["headerOpponent"]=(at+end)*.5f;
        var draw=ImGui.GetWindowDrawList();draw.AddRectFilled(at,end,new ImColor8(16,29,43),3);
        float x=at.X+7;
        if(!pve){portraits.DrawCompanion(ResultCompanion(opponent!.Value.Player),new(x,at.Y+7),30);x+=36;}
        HudClipText(draw,new(x,at.Y+4),end.X-x-6,label,HudMuted,12);
        HudClipText(draw,new(x,at.Y+21),end.X-x-6,name,opponent?.Ghost==true?new ImColor8(192,168,255):HudGold,16);
        if(ImGui.IsItemHovered()){HeaderOpponentTooltip=detail;ResultTooltip(detail);}
    }
}

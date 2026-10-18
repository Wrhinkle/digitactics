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
        var at=new float2(TimelineBounds.Min.X,61);var end=new float2(TimelineBounds.Max.X,81);
        HeaderOpponentName=name;HeaderOpponentBounds=(at,end);
        ImGui.SetCursorScreenPos(at);ImGui.InvisibleButton("headerOpponent"u8,end-at);
        Buttons["headerOpponent"]=(at+end)*.5f;
        var draw=ImGui.GetWindowDrawList();draw.AddRectFilled(at,end,new ImColor8(16,29,43),3);
        float x=at.X+5;
        if(!pve){portraits.DrawCompanion(ResultCompanion(opponent!.Value.Player),new(x,at.Y),20);x+=25;}
        ImGui.PushFont(default,14);float labelWidth=ImGui.CalcTextSize(label).X,healthWidth=ImGui.CalcTextSize(health).X;ImGui.PopFont();
        HudClipText(draw,new(x,at.Y+2),labelWidth+1,label,HudMuted,14);x+=labelWidth+8;
        float nameWidth=end.X-x-(pve?5:healthWidth+13);
        HudClipText(draw,new(x,at.Y+1),nameWidth,name,opponent?.Ghost==true?new ImColor8(192,168,255):HudGold,16);
        if(!pve)HudClipText(draw,new(end.X-healthWidth-5,at.Y+2),healthWidth+1,health,HudInk,14);
        if(ImGui.IsItemHovered()){HeaderOpponentTooltip=detail;ResultTooltip(detail);}
    }
}

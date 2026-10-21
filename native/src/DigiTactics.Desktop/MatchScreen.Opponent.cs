using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal string? HeaderOpponentName {get;private set;}
    internal string? HeaderOpponentTooltip {get;private set;}
    internal (float2 Min,float2 Max) HeaderOpponentBounds {get;private set;}
    private readonly record struct OpponentLabel(bool Pve,string Label,string Name,string Health,bool Ghost,MatchPlayer? Player);
    private OpponentLabel? CurrentOpponentLabel()
    {
        var session=Session!;var opponent=StandingOpponent(session.State.Players[viewed]);
        bool pve=session.Rounds.RoundType=="pve";
        if(!pve&&opponent is null)return null;
        string label=pve?"Opponent":session.Rounds.Phase==MatchPhase.Planning?"Next opponent":"Fighting";
        if(opponent?.Ghost==true)label+=" (ghost copy)";
        string name=pve?"Wild Digimon":opponent!.Value.Player.Name;
        if(!pve&&session.State.Duos)name+=" + "+session.State.Teams[opponent!.Value.Player.Team!.Value].Members.First(p=>p.Id!=opponent.Value.Player.Id).Name;
        return new(pve,label,name,pve?"":Math.Max(0,opponent!.Value.Player.Health)+" HP",opponent?.Ghost==true,pve?null:opponent!.Value.Player);
    }
    // #opponentLabel: .ol 11px/1.1, .on 700 14px capped at 150px; <small> is the browser's "smaller" step.
    private const float OpponentSmall=14/1.2f;
    private float OpponentTextWidth(OpponentLabel card)
    {
        float name=HudWidth(HudWeight(700),14,card.Name)+(card.Pve?0:4+HudWidth(HudWeight(600),OpponentSmall,card.Health));
        return Math.Max(HudWidth(HudWeight(400),11,card.Label),Math.Min(150,name));
    }
    private float OpponentWidth()=>CurrentOpponentLabel() is {} card?(card.Pve?20:3+34+8+10)+OpponentTextWidth(card)+2:0;
    private float OpponentHeight()=>CurrentOpponentLabel() is {} card?(card.Pve?11*1.1f+14*1.35f:34)+8:0;
    private void OpponentCard(float x,float center)
    {
        if(CurrentOpponentLabel() is not {} card)return;
        string detail=card.Label+" / "+card.Name+(card.Pve?"":" / "+card.Health);
        float width=OpponentWidth(),height=OpponentHeight();
        var at=new float2(x,center-height/2);var end=at+new float2(width,height);
        HeaderOpponentName=card.Name;HeaderOpponentBounds=(at,end);
        ImGui.SetCursorScreenPos(at);ImGui.InvisibleButton("headerOpponent"u8,end-at);
        Buttons["headerOpponent"]=(at+end)*.5f;
        var draw=ImGui.GetWindowDrawList();
        draw.AddRectFilled(at,end,card.Pve?HudHex("261f14"):HudHex("2a1820"),8);
        draw.AddRect(at,end,card.Pve?HudHex("5a4a26"):HudHex("6a2c36"),8);
        float textX=at.X+11;
        if(!card.Pve){portraits.DrawCompanion(ResultCompanion(card.Player!),new(at.X+4,at.Y+4),34);textX=at.X+4+34+8;}
        float textTop=center-(11*1.1f+14*1.35f)/2,nameTop=textTop+11*1.1f;
        HudRun(draw,HudWeight(400),11,textX,textTop,11*1.1f,card.Label,CssMuted);
        var nameInk=card.Ghost?new ImColor8(192,168,255):CssText;
        if(card.Pve)HudRun(draw,HudWeight(700),14,textX,nameTop,14*1.35f,card.Name,nameInk,150);
        else
        {
            float healthWidth=4+HudWidth(HudWeight(600),OpponentSmall,card.Health);
            float nameWidth=HudRun(draw,HudWeight(700),14,textX,nameTop,14*1.35f,card.Name,nameInk,Math.Max(20,150-healthWidth));
            // <small> shares the name's baseline.
            var bold=HudWeight(700);var small=HudWeight(600);
            float baseline=HudBaseline(bold,14,nameTop,14*1.35f);
            float smallTop=baseline-MathF.Ceiling(OpponentSmall*small.Ascent);
            draw.AddText(small.Font,OpponentSmall*small.Factor,new(textX+nameWidth+4,smallTop),CssBad,card.Health);
        }
        if(ImGui.IsItemHovered()){HeaderOpponentTooltip=detail;ResultTooltip(detail);}
    }
}

using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal string? ResultTitle { get; private set; }
    internal int ResultHealthLoss { get; private set; }
    internal float2 ResultBannerMin { get; private set; }
    internal float2 ResultBannerMax { get; private set; }

    private void ResultBanner()
    {
        ResultTitle = null; ResultHealthLoss = 0;
        if (Session!.Rounds.Phase != MatchPhase.Result) return;
        var player = Human;
        var own = PlayerRoundResult.Find(Session.Rounds.Results, player);
        if (own is null) return;
        var result = own.Result; bool a = own.SideA, won = own.Won, lost = own.Lost;
        ResultTitle = own.Title;
        ResultHealthLoss = own.HealthLoss;
        var opponentPlayer = result.Match.Pve ? null : Session.State.Players[(a ? result.Match.B : result.Match.A)!.Value];
        string opponent = opponentPlayer?.Name ?? "Wild Digimon";
        var details = new List<string>();
        if (ResultHealthLoss > 0) details.Add($"-{ResultHealthLoss} HP");
        if (won && !result.Match.Pve) details.Add($"+{Rules.Economy!.WinGold} gold");
        if (a && result.Reward is { Orbs: > 0 } reward) details.Add($"{reward.Orbs} loot orb{(reward.Orbs == 1 ? "" : "s")} dropped: right-click to walk your Little Legend over them");
        // Original #banner is a compact, centered arena overlay at 34% height.
        // Keep the board visible instead of placing a full-width client card on it.
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,30);
        float titleWidth=ImGui.CalcTextSize(ResultTitle).X;ImGui.PopFont();
        ImGui.PushFont(NativeUiTheme.BodyFont,22.5f);
        float opponentWidth=ImGui.CalcTextSize("vs "+opponent).X;ImGui.PopFont();
        ImGui.PushFont(NativeUiTheme.BodyFont,17.5f);
        string detail=string.Join(" · ",details);
        float detailWidth=ImGui.CalcTextSize(detail).X;
        float avatarWidth=opponentPlayer is null?0:56;
        float width=Math.Min(Math.Min(540,Width-24),Math.Max(240,Math.Max(avatarWidth+titleWidth+10+opponentWidth,detailWidth)+60));
        float bodyWidth=width-60;
        float detailHeight=details.Count==0?0:Math.Max(15,ImGui.CalcTextSize(detail,false,bodyWidth).Y)+6;
        float height=72+detailHeight;
        var position=new float2(Left+(Width-width)/2,Top+Height*.34f-height/2);
        Window("Round result",position.X,position.Y,width,height,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoInputs|ImGuiWindowFlags.NoFocusOnAppearing|ImGuiWindowFlags.NoBackground|ImGuiWindowFlags.NoScrollbar);
        var draw=ImGui.GetWindowDrawList();ResultBannerMin=position;ResultBannerMax=position+new float2(width,height);
        draw.AddRectFilled(position+new float2(0,6),position+new float2(width,height+6),new ImColor8(0,0,0,90),12);
        draw.AddRectFilled(position,position+new float2(width,height),new ImColor8(11,14,21,235),12);
        draw.AddRect(position,position+new float2(width,height),won?new ImColor8(47,122,82):lost?new ImColor8(138,51,64):NativeUiTheme.GoldBorder,12);
        float rowWidth=avatarWidth+titleWidth+10+opponentWidth;
        var row=position+new float2(Math.Max(18,(width-rowWidth)/2),14);
        if(opponentPlayer is not null)portraits.DrawCompanion(ResultCompanion(opponentPlayer),row,44);
        row.X+=avatarWidth;
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,30);
        draw.AddText(row+new float2(0,9),won?new ImColor8(90,223,138):lost?new ImColor8(255,90,106):NativeUiTheme.GoldBright,ResultTitle);ImGui.PopFont();
        ImGui.PushFont(NativeUiTheme.BodyFont,22.5f);
        draw.AddText(row+new float2(titleWidth+10,13),NativeUiTheme.Muted,ResultTrim("vs "+opponent,Math.Max(40,width-30-avatarWidth-titleWidth-10)));ImGui.PopFont();
        if(details.Count>0)
        {
            ImGui.SetCursorPos(new(Math.Max(30,(width-detailWidth)/2),64));
            ImGui.PushTextWrapPos(width-30);ImGui.PushStyleColor(ImGuiCol.Text,NativeUiTheme.Muted);
            UiText.Wrapped(detail);ImGui.PopStyleColor();ImGui.PopTextWrapPos();
        }
        ImGui.PopFont();
        ImGui.End();
    }
}

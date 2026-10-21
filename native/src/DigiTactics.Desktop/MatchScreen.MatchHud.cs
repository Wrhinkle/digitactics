using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    // Original match palette: style.css .card, .trait, #econ and #modal.
    private static readonly ImColor8 HudGold=new(255,207,74),HudInk=new(238,242,250),HudMuted=new(143,154,176),HudSurface=new(22,28,42),HudLine=new(34,48,73);

    private string? hudLastStatus;
    private long hudStatusUntil;
    private void DrawMatchStatus()
    {
        if(status!=hudLastStatus){hudLastStatus=status;hudStatusUntil=Environment.TickCount64+3400;}
        if(showPlayers||showGameMenu||showAudio||showSoundboard||showEmotes||CodexVisible||Environment.TickCount64>=hudStatusUntil||string.IsNullOrWhiteSpace(status)||
            status.StartsWith("Select a unit, then a hex",StringComparison.Ordinal)||
            status is "Battle in progress." or "Round complete.")return;
        ImGui.PushFont(default,16);
        float maximum=Math.Max(120,Math.Min(620,Width-32));
        var textSize=ImGui.CalcTextSize(status,false,maximum);
        var extent=new float2(Math.Min(maximum,textSize.X)+24,textSize.Y+14);
        var at=new float2(Left+(Width-extent.X)/2,Top+Height-extent.Y-12);
        var draw=ImGui.GetForegroundDrawList();
        draw.AddRectFilled(at,at+extent,new ImColor8(11,14,21,235),8);
        draw.AddText(ImGui.GetFont(),16,at+new float2(12,7),HudGold,status,maximum);
        ImGui.PopFont();
    }
    private static void HudText(string value,float font=16,ImColor8? color=null)
    {
        ImGui.PushFont(default,font);ImGui.PushStyleColor(ImGuiCol.Text,color??HudInk);
        UiText.Wrapped(value);ImGui.PopStyleColor();ImGui.PopFont();
    }
    private bool HudButton(string key,string label,float width=0,float height=38,bool active=false,float font=16)
    {
        ImGui.PushFont(default,font);
        ImGui.PushStyleColor(ImGuiCol.Button,active?new ImColor8(55,48,27):new ImColor8(28,39,58));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new ImColor8(43,65,91));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive,new ImColor8(31,99,176));
        ImGui.PushStyleColor(ImGuiCol.Border,active?HudGold:HudLine);
        ImGui.PushStyleColor(ImGuiCol.Text,active?HudGold:HudInk);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,1);ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,6);
        bool pressed=Button(key,label,width,height);ImGui.PopStyleVar(2);ImGui.PopStyleColor(5);ImGui.PopFont();return pressed;
    }
    private bool HudPrimaryButton(string key,string label,float width,float height)
    {
        ImGui.PushFont(default,16);ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(31,99,176));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new ImColor8(38,114,200));
        ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(74,146,224));
        ImGui.PushStyleColor(ImGuiCol.Text,HudInk);ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,5);
        bool pressed=Button(key,label,width,height);ImGui.PopStyleVar();ImGui.PopStyleColor(4);ImGui.PopFont();return pressed;
    }
    private static void HudClipText(ImDrawListPtr draw,float2 at,float width,string value,ImColor8 ink,float font=16)
    {
        // Ellipsis preserves the price/count column and communicates truncation.
        ImGui.PushFont(default,font);
        if(ImGui.CalcTextSize(value).X>width)
        {
            while(value.Length>0&&ImGui.CalcTextSize(value+"...").X>width)value=value[..^1];
            value+="...";
        }
        draw.PushClipRect(at,at+new float2(Math.Max(1,width),font+3),true);
        draw.AddText(ImGui.GetFont(),font,at,ink,value);draw.PopClipRect();ImGui.PopFont();
    }
}

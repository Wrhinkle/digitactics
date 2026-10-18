using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    // Original match palette: style.css .card, .trait, #econ and #modal.
    private static readonly ImColor8 HudGold=new(255,207,74),HudInk=new(235,240,248),HudMuted=new(160,173,194),HudSurface=new(22,28,42),HudLine=new(55,68,91);
    private const float ShopCardHeight=140;

    private static void HudText(string value,float font=16,ImColor8? color=null)
    {
        ImGui.PushFont(default,font);ImGui.PushStyleColor(ImGuiCol.Text,color??HudInk);
        UiText.Wrapped(value);ImGui.PopStyleColor();ImGui.PopFont();
    }
    private bool HudButton(string key,string label,float width=0,float height=38,bool active=false)
    {
        ImGui.PushFont(default,16);
        ImGui.PushStyleColor(ImGuiCol.Button,active?new ImColor8(55,48,27):new ImColor8(28,39,58));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new ImColor8(43,65,91));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive,new ImColor8(31,99,176));
        ImGui.PushStyleColor(ImGuiCol.Border,active?HudGold:HudLine);
        ImGui.PushStyleColor(ImGuiCol.Text,active?HudGold:HudInk);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,1);ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,6);
        bool pressed=Button(key,label,width,height);ImGui.PopStyleVar(2);ImGui.PopStyleColor(5);ImGui.PopFont();return pressed;
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

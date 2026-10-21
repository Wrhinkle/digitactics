using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    // js/codexstyle.js structure with the navy/gold overrides from js/theme.js.
    private static readonly ImColor8 CodexNavy=new(1,10,19),CodexPanel=new(10,20,40),CodexCardTop=new(11,26,44),
        CodexStroke=new(49,66,95),CodexBody=new(198,205,219),CodexMuted=new(143,154,176),CodexGold=new(200,170,110),
        CodexInk=new(240,230,210),CodexAbility=new(217,184,255);

    private void CodexBeginWindow(string id,float2 at,float2 extent,bool scroll=false,bool compact=false,float2? padding=null)
    {
        ImGui.PushFont(default,16);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,padding??new float2(compact?8:14,compact?10:12));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(6,6));
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(10,7));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding,0);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize,8);
        ImGui.PushStyleColor(ImGuiCol.WindowBg,CodexNavy);ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(120,90,40));
        ImGui.PushStyleColor(ImGuiCol.Text,CodexBody);ImGui.PushStyleColor(ImGuiCol.Separator,new ImColor8(70,55,20));
        ImGui.PushStyleColor(ImGuiCol.FrameBg,CodexNavy);ImGui.PushStyleColor(ImGuiCol.FrameBgHovered,new ImColor8(20,32,48));
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive,new ImColor8(26,42,57));ImGui.PushStyleColor(ImGuiCol.ScrollbarBg,CodexNavy);
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab,new ImColor8(70,55,20));
        ImGui.SetNextWindowBgAlpha(1);
        Window(id,at.X,at.Y,extent.X,extent.Y,ImGuiWindowFlags.NoTitleBar|(scroll?ImGuiWindowFlags.None:ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse));
        ImGui.GetWindowDrawList().AddRectFilledMultiColor(at+new float2(1,1),at+extent-new float2(1,1),CodexPanel,CodexPanel,CodexNavy,CodexNavy);
    }
    private static void CodexEndWindow(){ImGui.End();ImGui.PopStyleColor(9);ImGui.PopStyleVar(5);ImGui.PopFont();}
    private static void CodexHeading(string text,float font=18,ImColor8? color=null)
    {
        ImGui.PushFont(NativeUiTheme.DisplayFont,font);ImGui.PushStyleColor(ImGuiCol.Text,color??CodexInk);
        UiText.Wrapped(text);ImGui.PopStyleColor();ImGui.PopFont();
    }
    private bool CodexButton(string key,string label,float width=0,float height=34,bool selected=false,ImColor8? color=null,float font=16)
    {
        ImGui.PushFont(default,font);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,1);ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,0);
        ImGui.PushStyleColor(ImGuiCol.Button,selected?new ImColor8(42,50,64):new ImColor8(24,30,39));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new ImColor8(38,49,62));ImGui.PushStyleColor(ImGuiCol.ButtonActive,new ImColor8(44,58,72));
        ImGui.PushStyleColor(ImGuiCol.Border,selected?color??CodexGold:new ImColor8(93,77,43));
        ImGui.PushStyleColor(ImGuiCol.Text,color??(selected?CodexInk:CodexBody));
        bool pressed=Button(key,label,width,Math.Max(30,height));
        ImGui.PopStyleColor(5);ImGui.PopStyleVar(2);ImGui.PopFont();return pressed;
    }
    private static bool CodexBeginCard(string id,ImColor8? accent=null,bool topStripe=false,bool compact=false,bool trait=false)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,trait&&!compact?new float2(11,8):new float2(compact?9:10,9));
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding,trait?7:0);ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize,1);
        ImGui.PushStyleColor(ImGuiCol.ChildBg,trait?new ImColor8(16,22,36):CodexNavy);ImGui.PushStyleColor(ImGuiCol.Border,CodexStroke);
        bool visible=ImGui.BeginChild(id,new(0,0),ImGuiChildFlags.Borders|ImGuiChildFlags.AutoResizeY|ImGuiChildFlags.AlwaysUseWindowPadding,
            ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleColor(2);ImGui.PopStyleVar(3);
        if(!visible)return false;
        var at=ImGui.GetWindowPos();var end=at+ImGui.GetWindowSize();var draw=ImGui.GetWindowDrawList();
        if(!trait)draw.AddRectFilledMultiColor(at+new float2(1,1),end-new float2(1,1),CodexCardTop,CodexCardTop,CodexNavy,CodexNavy);
        if(accent is {} tint&&trait)
        {
            // border-left: 3px on a rounded box: the colour runs around both left corners, tapering into the 1px top and bottom borders.
            draw.PushClipRect(at,new(at.X+9,end.Y),true);
            draw.AddRect(at+new float2(1.5f,.5f),end-new float2(-6,.5f),tint,7,ImDrawFlags.None,3);
            draw.PopClipRect();
            draw.PushClipRect(at,new(at.X+3,end.Y),true);draw.AddRectFilled(at+new float2(0,7),new(at.X+3,end.Y-7),tint);draw.PopClipRect();
        }
        else if(accent is {} stripe)draw.AddRectFilled(at+new float2(1,1),topStripe?new(end.X-1,at.Y+4):new(at.X+4,end.Y-1),stripe);
        return true;
    }
    private static void CodexEndCard()=>ImGui.EndChild();
    private static void CodexPill(string text,ImColor8 color)
    {
        ImGui.PushFont(default,14);var at=ImGui.GetCursorScreenPos();var textSize=ImGui.CalcTextSize(text);var extent=textSize+new float2(14,6);
        ImGui.Dummy(extent);var draw=ImGui.GetWindowDrawList();draw.AddRectFilled(at,at+extent,new ImColor8(14,28,44),3);draw.AddRect(at,at+extent,color,3);
        draw.AddText(at+new float2(7,3),color,text);ImGui.PopFont();
    }
    private static void CodexFlowNext(float width,float right)
    {
        if(ImGui.GetItemRectMax().X+8+width<=right)ImGui.SameLine(0,8);
    }
}

using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private readonly LootAwardNotices lootAwardNotices=new();
    private bool lootAwardHeld,lootAwardFocused;
    private long lootAwardEntranceSerial;
    private double lootAwardEntrance;
    internal LootAwardNotice? CurrentLootAward=>lootAwardNotices.Current;
    internal int QueuedLootAwards=>lootAwardNotices.PendingCount;
    internal double LootAwardElapsed=>lootAwardNotices.Elapsed;
    internal bool LootAwardVisible {get;private set;}
    internal (float2 Min,float2 Max) LootAwardBounds {get;private set;}
    private bool LootAwardBlocked=>EliminationCinematicActive||mysteryReward is not null||hatching is not null||confirmResetSave||CloseDeferred||fileCloseError is not null||ImGui.GetIO().AppFocusLost;
    private void UpdateLootAwardNotice(double delta)
    {
        lootAwardNotices.Advance(delta,LootAwardBlocked||lootAwardHeld);
        if(lootAwardNotices.Current?.Serial!=lootAwardEntranceSerial){lootAwardEntranceSerial=lootAwardNotices.Current?.Serial??0;lootAwardEntrance=0;}
        if(!LootAwardBlocked)lootAwardEntrance+=delta;
    }
    private bool LootAwardEscape()
    {
        if(!lootAwardFocused||lootAwardNotices.Current is null||LootAwardBlocked||KeyboardPopup||keyboardPopupLastFrame||ShortcutModifiers||!ImGui.IsKeyPressed(ImGuiKey.Escape,false))return false;
        lootAwardNotices.Dismiss();lootAwardHeld=lootAwardFocused=false;return true;
    }
    private void DrawMysteryLootNotice()
    {
        LootAwardVisible=false;lootAwardHeld=lootAwardFocused=false;
        if(LootAwardBlocked||lootAwardNotices.Current is not {} notice)return;
        LootAwardVisible=true;
        float width=Math.Min(780,size.X-48),height=notice.Items.Count==1?190:220;
        float enter=ClientMotion?(float)Math.Clamp(lootAwardEntrance/.22,0,1):1;
        float2 at=new((size.X-width)/2,size.Y-height-24+18*(1-enter));
        LootAwardBounds=(at,at+new float2(width,height));
        ImGui.PushStyleColor(ImGuiCol.WindowBg,new ImColor8(5,15,26,250));ImGui.PushStyleColor(ImGuiCol.Border,HomeGold);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding,10);ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize,2);
        Window("Loot earned",at.X,at.Y,width,height,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoFocusOnAppearing);
        ImGui.PopStyleVar(3);ImGui.PopStyleColor(2);
        var draw=ImGui.GetWindowDrawList();
        ImGui.SetCursorScreenPos(at+new float2(20,14));HomeLabel("LOOT EARNED",24,HomeGold);
        float tile=(width-40-16*(notice.Items.Count-1))/notice.Items.Count;
        for(int i=0;i<notice.Items.Count;i++)
        {
            var item=notice.Items[i];var pos=at+new float2(20+i*(tile+16),58);
            var color=item.Id=="daily"?new ImColor8(240,201,106):new ImColor8(97,213,239);
            LootAwardIcon(draw,item.Id,pos,44,color);
            ImGui.SetCursorScreenPos(pos+new float2(56,0));ImGui.BeginGroup();
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+tile-56);ImGui.PushFont(default,notice.Items.Count==1?24:20);
            ImGui.Text($"{item.Name} ×{item.Count}");ImGui.PopFont();
            ImGui.PushFont(default,16);ImGui.PushStyleColor(ImGuiCol.Text,new ImColor8(192,205,221));ImGui.Text(item.Source);ImGui.PopStyleColor();ImGui.PopFont();
            ImGui.PopTextWrapPos();ImGui.EndGroup();
        }
        ImGui.SetCursorScreenPos(at+new float2(20,height-54));
        ImGui.PushTextWrapPos(width-142);ImGui.PushFont(default,16);
        ImGui.Text("Saved in Loot. Open boxes to discover their contents."u8);ImGui.PopFont();ImGui.PopTextWrapPos();
        ImGui.SetCursorScreenPos(at+new float2(width-132,height-54));
        if(HomeButton("dismissLootAward","Dismiss",112,38)){lootAwardNotices.Dismiss();lootAwardHeld=lootAwardFocused=false;}
        lootAwardFocused=ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);
        lootAwardHeld=lootAwardFocused||ImGui.IsWindowHovered(ImGuiHoveredFlags.RootAndChildWindows);
        float progress=(float)Math.Clamp(1-lootAwardNotices.Elapsed/LootAwardNotices.Duration,0,1);
        draw.AddRectFilled(at+new float2(3,height-4),at+new float2(3+(width-6)*progress,height-1),HomeGold,2);
        ImGui.End();
    }
    private static void LootAwardIcon(ImDrawListPtr draw,string id,float2 at,float size,ImColor8 color)
    {
        float s=size/48;float2 P(float x,float y)=>at+new float2(x*s,y*s);
        if(id=="key")
        {
            draw.AddCircle(P(16,16),9*s,color,32,3*s);draw.AddLine(P(22,22),P(40,40),color,3*s);
            draw.AddLine(P(31,31),P(36,26),color,3*s);draw.AddLine(P(36,36),P(41,31),color,3*s);return;
        }
        draw.AddRect(P(7,7),P(41,25),color,6*s,0,3*s);draw.AddRectFilled(P(7,21),P(41,40),new ImColor8(5,15,26));
        draw.AddRect(P(7,21),P(41,40),color,0,0,3*s);draw.AddLine(P(5,21),P(43,21),color,3*s);draw.AddRect(P(21,21),P(27,29),color,0,0,3*s);
    }
}

using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private const string WispRoundLimit = "one use, this round only";
    private const string EmptyWispHint = "A Data Wisp appears here every other round from stage 2";
    internal (float2 Min,float2 Max) WispCardBounds { get; private set; }
    internal string? WispTooltipText { get; private set; }
    internal long WispTooltipSerial { get; private set; }
    internal string? DisplayedWisp { get; private set; }
    internal int WispArtworkDrawCalls { get; private set; }

    /// <summary>Original sixth shop slot: source category, effect, artwork and current offer price.</summary>
    private void WispShopCard(float width)
    {
        var offer=Human.Wisp;
        WispTooltipText=null;DisplayedWisp=offer?.Id;WispArtworkDrawCalls=0;
        bool pressed=HudButton("wisp","",width,ShopCardHeight);
        var min=ImGui.GetItemRectMin();var max=ImGui.GetItemRectMax();WispCardBounds=(min,max);
        bool hovered=ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled);
        if(offer is null)
        {
            if(hovered)WispTooltip(null,0,min,max);
            return;
        }
        var wisp=codexCatalog.Value.Variant(Rules.Id).Wisps.Single(w=>w.Id==offer.Id);
        bool affordable=Human.Economy.Gold>=offer.Cost;
        var ink=affordable?new ImColor8(214,201,255):HudMuted;
        var draw=ImGui.GetWindowDrawList();
        draw.PushClipRect(min,max,true);
        draw.AddRectFilledMultiColor(min,max,new ImColor8(29,34,64),new ImColor8(29,34,64),new ImColor8(37,29,58),new ImColor8(37,29,58));
        var border=hovered?new ImColor8(183,164,255):new ImColor8(138,120,255);
        for(float x=min.X+1;x<max.X-1;x+=9)
        {
            draw.AddLine(new(x,min.Y+1),new(Math.Min(x+5,max.X-1),min.Y+1),border);
            draw.AddLine(new(x,max.Y-1),new(Math.Min(x+5,max.X-1),max.Y-1),border);
        }
        for(float y=min.Y+1;y<max.Y-1;y+=9)
        {
            draw.AddLine(new(min.X+1,y),new(min.X+1,Math.Min(y+5,max.Y-1)),border);
            draw.AddLine(new(max.X-1,y),new(max.X-1,Math.Min(y+5,max.Y-1)),border);
        }
        itemArtwork.Draw(Rules.Id,"wisp:"+wisp.Id,new(max.X-38,min.Y+5),30);WispArtworkDrawCalls++;
        HudClipText(draw,min+new float2(10,8),width-52,(wisp.Kind+" Wisp").ToUpperInvariant(),new ImColor8(177,157,255),12);
        var nameLines=ChoiceLines(wisp.Name,16,width-20);int nameRows=Math.Min(2,nameLines.Length);
        for(int row=0;row<nameRows;row++)HudClipText(draw,min+new float2(10,32+row*19),width-20,nameLines[row]+(row==1&&nameLines.Length>2?"...":""),ink,16);
        float descriptionY=32+nameRows*19+4;
        int effectRows=Math.Clamp((int)((ShopCardHeight-descriptionY-28)/17),1,3);
        DrawWispDescription(draw,min+new float2(10,descriptionY),width-20,wisp.Description,effectRows);
        HudClipText(draw,new(max.X-77,max.Y-23),67,offer.Cost+" gold",affordable?HudGold:HudMuted);
        draw.PopClipRect();
        if(hovered)WispTooltip(wisp,offer.Cost,min,max);
        if(pressed)Act("wisp");
    }

    private static void DrawWispDescription(ImDrawListPtr draw,float2 at,float width,string description,int rows)
    {
        // The original card clamps the effect to three lines; the hover shows it in full.
        ImGui.PushFont(default,14);
        var words=description.Split(' ');int word=0;
        for(int row=0;row<rows&&word<words.Length;row++)
        {
            string line=words[word++];
            while(word<words.Length&&ImGui.CalcTextSize(line+" "+words[word]).X<=width)line+=" "+words[word++];
            if(row==rows-1&&word<words.Length)
            {
                while(line.Length>0&&ImGui.CalcTextSize(line+"...").X>width)line=line[..^1];
                line+="...";
            }
            draw.AddText(ImGui.GetFont(),14,at+new float2(0,row*17),HudMuted,line);
        }
        ImGui.PopFont();
    }

    private void WispTooltip(CodexWisp? wisp,int cost,float2 min,float2 max)
    {
        float width=Math.Min(420,size.X-24);
        ImGui.SetNextWindowSize(new(width,0));
        ImGui.SetNextWindowPos(new(Math.Clamp((min.X+max.X)*.5f,width*.5f+8,size.X-width*.5f-8),min.Y-8),ImGuiCond.Always,new(.5f,1));
        ImGui.BeginTooltip();
        if(wisp is null)
        {
            WispTooltipText=EmptyWispHint;HudText(EmptyWispHint);
        }
        else
        {
            string detail=$"{wisp.Kind} Wisp · {cost} gold · {WispRoundLimit}";
            WispTooltipText=wisp.Name+"\n"+detail+"\n"+wisp.Description;
            var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(64,64));
            itemArtwork.Draw(Rules.Id,"wisp:"+wisp.Id,at,64);WispArtworkDrawCalls++;
            ImGui.SameLine();ImGui.BeginGroup();
            HudText(wisp.Name,20,new ImColor8(214,201,255));HudText(detail,16,HudMuted);
            ImGui.EndGroup();ImGui.Separator();HudText(wisp.Description);
        }
        WispTooltipSerial++;ImGui.EndTooltip();
    }
}

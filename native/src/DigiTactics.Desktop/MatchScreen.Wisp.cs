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

    /// <summary>Original sixth shop slot (.card.wisp): kind, name, three-line effect, artwork and price.</summary>
    private void WispShopCard(float2 at,float width,bool wispsInSet)
    {
        var offer=Human.Wisp;float height=HudCardHeight;
        WispTooltipText=null;DisplayedWisp=offer?.Id;WispArtworkDrawCalls=0;
        if(!wispsInSet&&offer is null){WispCardBounds=(at,at);return;}
        ImGui.SetCursorScreenPos(at);
        bool pressed=ImGui.InvisibleButton("wisp"u8,new(width,height));
        Buttons["wisp"]=at+new float2(width,height)*.5f;
        bool hovered=ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled);
        var draw=ImGui.GetWindowDrawList();
        if(offer is null)
        {
            WispCardBounds=(at,at+new float2(width,height));
            EmptyShopCard(draw,at,new(width,height));
            if(hovered)WispTooltip(null,0,at,at+new float2(width,height));
            return;
        }
        var wisp=codexCatalog.Value.Variant(Rules.Id).Wisps.Single(w=>w.Id==offer.Id);
        bool affordable=Human.Economy.Gold>=offer.Cost;
        WispCardBounds=(at,at+new float2(width,height));
        if(hovered&&affordable)at.Y-=2;
        var min=at;var max=at+new float2(width,height);
        Rgb Paint(Rgb color)=>!affordable?HudPoor(color):hovered?HudBright(color):color;
        draw.AddRectFilled(min+new float2(-1,1),max+new float2(1,4),new ImColor8(0,0,0,40),10);
        // linear-gradient(160deg, #1d2240, #251d3a) under a 2px dashed #8a78ff border.
        draw.AddRectFilledMultiColor(min+new float2(1,1),max-new float2(1,1),(ImColor8)Paint(HudHex("1d2240")),(ImColor8)Paint(HudHex("1f2140")),(ImColor8)Paint(HudHex("251d3a")),(ImColor8)Paint(HudHex("231e3c")));
        HudDashedRect(draw,min,max,Paint(HudHex("8a78ff")),2);
        draw.PushClipRect(min+new float2(2,2),max-new float2(2,2),true);
        itemArtwork.Draw(Rules.Id,"wisp:"+wisp.Id,new(max.X-2-6-30,min.Y+2+5),30);WispArtworkDrawCalls++;
        float x=min.X+2+9,y=min.Y+2+7,inner=width-4-18;
        HudRun(draw,HudWeight(800),10,x,y,13.5f,(wisp.Kind+" Wisp").ToUpperInvariant(),Paint(HudHex("9f8cff")),inner-30,.5f);
        y+=13.5f+3;
        var nameLines=WrapHud(HudWeight(800),14,wisp.Name,inner);
        foreach(var nameLine in nameLines.Take(2)){HudRun(draw,HudWeight(800),14,x,y,18.9f,nameLine,Paint(HudHex("d6c9ff")));y+=18.9f;}
        y+=3;
        var effect=WrapHud(HudWeight(400),11,wisp.Description,inner);
        for(int row=0;row<Math.Min(3,effect.Length);row++)
        {
            string text=effect[row];
            if(row==2&&effect.Length>3)text+="…";
            HudRun(draw,HudWeight(400),11,x,y,14.85f,text,Paint(CssMuted),inner);y+=14.85f;
        }
        string price=offer.Cost.ToString();float priceWidth=11+3+HudWidth(HudWeight(900),14,price);
        var priceAt=new float2(max.X-2-8-priceWidth,max.Y-2-6-18.9f);
        HudCoin(draw,priceAt+new float2(5.5f,10),11);
        HudRun(draw,HudWeight(900),14,priceAt.X+14,priceAt.Y,18.9f,price,Paint(CssGold));
        draw.PopClipRect();
        if(hovered)WispTooltip(wisp,offer.Cost,min,max);
        if(pressed)Act("wisp");
    }
    private static string[] WrapHud(HudFace face,float css,string value,float width)
    {
        var lines=new List<string>();string line="";
        foreach(string word in value.Split(' ',StringSplitOptions.RemoveEmptyEntries))
        {
            string next=line.Length==0?word:line+" "+word;
            if(line.Length>0&&HudWidth(face,css,next)>width){lines.Add(line);line=word;}
            else line=next;
        }
        if(line.Length>0)lines.Add(line);
        return lines.ToArray();
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

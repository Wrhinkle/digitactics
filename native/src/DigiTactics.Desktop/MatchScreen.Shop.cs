using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private readonly PortraitArtwork portraits = new();
    internal int ShopPortraitDrawCalls {get;private set;}
    internal Dictionary<int,(float2 Min,float2 Max)> ShopCardBounds {get;}=[];
    internal (float2 Min,float2 Max) ShopViewportBounds {get;private set;}
    internal (float2 Min,float2 Max) ScoutingBounds {get;private set;}
    internal (float2 Min,float2 Max) TeamSidebarBounds {get;private set;}
    internal (float2 Min,float2 Max) AudioSettingsBounds {get;private set;}
    internal bool AudioSettingsVisible=>showAudio;
    internal string? ShopTooltipFooter {get;private set;}
    internal long ShopTooltipSerial {get;private set;}
    // DT.TIER_COLORS and DT.STAGE_COLORS.
    private static readonly Rgb[] tierColors = [HudHex("9aa4b1"),HudHex("9aa4b1"),HudHex("3fbf6a"),HudHex("3b8cff"),HudHex("b45cff"),HudHex("ffb02e")];
    private static readonly Rgb[] stageColors = [HudHex("d08a4a"),HudHex("d6dbe4"),HudHex("ffd84a")];
    private static readonly Rgb CardSurface=HudHex("161c2a"),CardBar=HudHex("0e121b");

    // render3d.js familyColor, exported as the codex portrait colour.
    private Rgb FamilyColor(string line)=>codexCatalog.Value.Variant(Rules.Id).Lines.FirstOrDefault(l=>l.Id==line) is {PortraitColor:{} color}?HudCss(color):HudHex("5b6b8c");
    private Rgb AttributeColor(string attribute)=>attribute switch{"Vaccine"=>HudHex("4fb3ff"),"Data"=>HudHex("4fe08a"),"Virus"=>HudHex("c45cff"),_=>HudHex("c8c8c8")};

    /// <summary>style.css .card: tier border, portrait art with the family glow, traits, attribute, name bar and price.</summary>
    private void ShopCard(int slot,float2 at,float width)
    {
        string? id=Human.Shop.Slots[slot];float height=HudCardHeight;
        ImGui.SetCursorScreenPos(at);
        bool buy=ImGui.InvisibleButton("buy/"+slot,new(width,height));
        bool hovered=ImGui.IsItemHovered();
        Buttons["buy/"+slot]=at+new float2(width,height)*.5f;
        if(ImGui.IsItemFocused())KeyboardFocusedButton="buy/"+slot;
        var draw=ImGui.GetWindowDrawList();
        if(id is null)
        {
            ShopCardBounds[slot]=(at,at+new float2(width,height));
            EmptyShopCard(draw,at,new(width,height));
            return;
        }
        var line=Rules.Lines.Single(l=>l.Id==id);var meta=Human.Shop.Metadata[slot];int stage=meta?.Unit?.Stage??0;
        int price=Human.Shop.Cost(slot); bool affordable=Human.Economy.Gold>=price;
        int owned=stage<2?Human.Roster.Entries.Count(e=>e.Unit.Line==id&&e.Unit.Stage==stage):0;
        bool evolves=owned>0&&owned>=line.Copies-1;bool tagged=meta?.Tag is {Length:>0};
        // .card:hover lifts 2px and brightens; .card.poor greys and darkens the whole card.
        if(hovered)at.Y-=2;
        var min=at;var max=at+new float2(width,height);
        ShopCardBounds[slot]=(min,max);
        Rgb Paint(Rgb color)=>!affordable?HudPoor(color):hovered?HudBright(color):color;
        var tier=tierColors[Math.Clamp(line.Tier,0,5)];
        var border=Paint(tagged?HudHex("e58bff"):tier);
        if(evolves){draw.AddRectFilled(min-new float2(2,2),max+new float2(2,2),Paint(CssGold),11);HudRadial(draw,(min+max)*.5f,width*.62f,CssGold,.22f,10);}
        else draw.AddRectFilled(min+new float2(-1,1),max+new float2(1,4),new ImColor8(0,0,0,40),10);
        draw.AddRectFilled(min,max,border,9);
        var inner=min+new float2(2,2);var innerMax=max-new float2(2,2);
        draw.AddRectFilled(inner,innerMax,Paint(CardSurface),7);
        // .art: everything above the 26px name bar, with the portrait hanging off the top-right corner.
        var artMax=new float2(innerMax.X,innerMax.Y-26);
        draw.PushClipRect(inner,artMax,true);
        float portrait=height==112?112:height==100?100:80;
        var portraitAt=new float2(artMax.X+4-portrait,inner.Y-6);
        // radial-gradient(circle at 60% 45%, mix(fc 60%, transparent), transparent 70%) on the .pt box
        HudRadialRect(draw,portraitAt,portraitAt+new float2(portrait,portrait),0,portraitAt+new float2(portrait*.6f,portrait*.45f),portrait*.57f,Paint(FamilyColor(id)),.6f,Paint(FamilyColor(id)),0);
        int beforePortrait=portraits.DrawCalls;
        portraits.Draw(Rules.Id,id,stage,portraitAt,portrait,255,affordable?null:new ImColor8(150,150,150));
        ShopPortraitDrawCalls+=portraits.DrawCalls-beforePortrait;
        // .art::after: rgba(14,18,28,.92) 0% -> .55 45% -> transparent 75%.
        float artWidth=artMax.X-inner.X;var shade=new Rgb(14,18,28);
        draw.AddRectFilledMultiColor(inner,new(inner.X+artWidth*.45f,artMax.Y),(ImColor8)HudAlpha(shade,.92f),(ImColor8)HudAlpha(shade,.55f),(ImColor8)HudAlpha(shade,.55f),(ImColor8)HudAlpha(shade,.92f));
        draw.AddRectFilledMultiColor(new(inner.X+artWidth*.45f,inner.Y),new(inner.X+artWidth*.75f,artMax.Y),(ImColor8)HudAlpha(shade,.55f),(ImColor8)HudAlpha(shade,0),(ImColor8)HudAlpha(shade,0),(ImColor8)HudAlpha(shade,.55f));
        draw.PopClipRect();
        draw.PushClipRect(inner,innerMax,true);
        // .ct: trait rows, 11px/600 with rotated square markers.
        var bonus=meta?.Unit?.BonusTraits;
        var traits=line.Traits.Concat(bonus?.Keys.AsEnumerable()??[]).Distinct().ToArray();
        var codexTraits=codexCatalog.Value.Variant(Rules.Id).Traits;
        float rowY=inner.Y+6;
        foreach(var traitId in traits)
        {
            var trait=codexTraits.FirstOrDefault(value=>value.Id==traitId);
            if(trait is null)continue;
            int count=(line.Traits.Contains(traitId)?1:0)+(bonus?.GetValueOrDefault(traitId)??0);
            float lineHeight=11*1.35f,cy=rowY+lineHeight/2;var marker=new float2(inner.X+7+4.5f,cy);
            draw.AddQuadFilled(marker+new float2(0,-6.36f),marker+new float2(6.36f,0),marker+new float2(0,6.36f),marker+new float2(-6.36f,0),Paint(HudCss(trait.Color)));
            var traitInk=Paint(bonus?.ContainsKey(traitId)==true?HudHex("ffffff"):HudHex("dfe4ee"));
            float tx=inner.X+7+13;
            tx+=HudShadowRun(draw,HudWeight(600),11,tx,rowY,lineHeight,Name(traitId),traitInk);
            if(bonus?.ContainsKey(traitId)==true&&count>1)HudShadowRun(draw,HudWeight(900),10,tx+4,rowY,lineHeight,$"x{count}",Paint(HudHex("ffd84a")));
            rowY+=lineHeight+2;
        }
        // .attr: 10px/800 with .5px tracking, 30px above the padding box bottom.
        HudShadowRun(draw,HudWeight(800),10,inner.X+7,innerMax.Y-30-13.5f,13.5f,line.Attribute,Paint(AttributeColor(line.Attribute)),0,.5f);
        if(tagged)
        {
            string tag=meta!.Tag!.ToUpperInvariant();float tagWidth=HudWidth(HudWeight(900),10,tag,.4f)+14;
            var tagMax=new float2(innerMax.X-6,innerMax.Y-31);var tagMin=tagMax-new float2(tagWidth,15);
            draw.AddRectFilledMultiColor(tagMin,tagMax,(ImColor8)HudHex("b45cff"),(ImColor8)HudHex("e58bff"),(ImColor8)HudHex("e58bff"),(ImColor8)HudHex("b45cff"));
            HudRun(draw,HudWeight(900),10,tagMin.X+7,tagMin.Y,15,tag,HudHex("ffffff"),0,.4f);
        }
        if(owned>0)
        {
            // .own badge: "1/3" copies owned, or gold "Digivolve" when this copy completes the set.
            string label=evolves?"Digivolve":$"{owned}/{line.Copies}";
            float badgeWidth=HudWidth(HudWeight(800),11,label)+14,bottom=tagged?50:31;
            var badgeMax=new float2(innerMax.X-6,innerMax.Y-bottom);var badgeMin=badgeMax-new float2(badgeWidth,17);
            draw.AddRectFilled(badgeMin,badgeMax,evolves?Paint(CssGold):HudAlpha(new Rgb(10,13,20),.85f),9);
            draw.AddRect(badgeMin,badgeMax,Paint(HudHex("7a6420")),9);
            HudRun(draw,HudWeight(800),11,badgeMin.X+7,badgeMin.Y+1,15,label,evolves?Paint(CssInk):Paint(CssGold));
        }
        // .nb name bar.
        var barMin=new float2(inner.X,innerMax.Y-26);
        draw.AddRectFilledMultiColor(barMin,innerMax,(ImColor8)Paint(HudMix(tier,CardBar,.35f)),(ImColor8)Paint(CardBar),(ImColor8)Paint(CardBar),(ImColor8)Paint(HudMix(tier,CardBar,.35f)));
        draw.AddLine(barMin+new float2(0,.5f),new(innerMax.X,barMin.Y+.5f),HudAlpha(Paint(tier),.6f));
        string priceText=price.ToString();float priceWidth=11+3+HudWidth(HudWeight(900),14,priceText);
        float priceX=innerMax.X-8-priceWidth;
        HudCoin(draw,new(priceX+5.5f,barMin.Y+13.5f),11);
        HudRun(draw,HudWeight(900),14,priceX+14,barMin.Y,26,priceText,Paint(CssGold));
        float nameMax=priceX-6-(barMin.X+8);
        float nameWidth=HudRun(draw,HudWeight(800),14,barMin.X+8,barMin.Y,26,line.Forms[stage],Paint(CssText),stage>0?nameMax-14*stage:nameMax);
        for(int s=0;s<stage+1&&stage>0;s++)HudStar(draw,new(barMin.X+8+nameWidth+7+s*8,barMin.Y+13.5f),3.8f,Paint(stageColors[Math.Min(stage,2)]));
        draw.PopClipRect();
        if(hovered)
        {
            // renderTooltip: tooltipFor plus the offer tag and pool footers, above the card.
            string offer=string.IsNullOrEmpty(meta?.Tag)?"":meta.Tag+" offer"+(string.IsNullOrEmpty(meta.Note)?"":": "+meta.Note);
            string pool=$"{Session!.State.Pool.Remaining(id)} more in the shared pool (of {Rules.PoolSizes[line.Tier]})";
            var parts=PlanningTip(id,stage,[],meta?.Unit?.BonusTraits,meta?.Unit?.Mods);
            if(offer.Length>0)parts.Add(new TipFoot(Runs(offer,HudHex("e58bff"),true)));
            parts.Add(new TipFoot(Runs(pool,CssMuted)));
            ShowTip(parts,TipSide.Top,min,max);
            ShopTooltipFooter=offer.Length>0?offer+"\n"+pool:pool;ShopTooltipSerial++;
        }
        if(buy)Act("buy",new {slot});
    }
    /// <summary>.card.empty: opacity .3 with a dashed line2 border.</summary>
    private static void EmptyShopCard(ImDrawListPtr draw,float2 at,float2 extent)
    {
        var under=HudHex("0f151f");
        draw.AddRectFilled(at,at+extent,HudOver(CardSurface,under,.3f),9);
        HudDashedRect(draw,at,at+extent,HudOver(CssLine2,under,.3f),2);
    }
}

using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private static readonly Lazy<HomeContent> homeContent=new(()=>HomeContent.Load(Path.Combine(AppContext.BaseDirectory,"content/home/content.json")));
    private string homeTab="overview";
    private readonly Dictionary<string,CosmeticScenePreview> homeOfferPreviews=[];
    internal Func<DateTimeOffset> HomeClock {get;set;}=()=>DateTimeOffset.Now;
    internal string HomeTab=>homeTab;
    internal int HomeHighlights {get;private set;}
    internal int HomeRotationSets {get;private set;}
    internal int HomeFeaturedCount {get;private set;}
    internal int HomeFeaturedOwned {get;private set;}
    internal string[] HomeFeaturedKeys {get;private set;}=[];
    internal string HomeCountdown {get;private set;}="";
    internal (float2 Min,float2 Max) HomeArticleBounds {get;private set;}
    internal (float2 Min,float2 Max) HomeStoreBounds {get;private set;}
    internal (float2 Min,float2 Max) HomeProgressBounds {get;private set;}
    internal (float2 Min,float2 Max) HomePatchBounds {get;private set;}
    internal float HomeArticleScroll {get;private set;}
    private DateOnly HomeDay=>DateOnly.FromDateTime(HomeClock().LocalDateTime);
    private DailyRewardProgress HomeDaily=>MatchRewards.Progress(profile?.Storage.GetValueOrDefault("dt.shop.v1"),profile?.Storage.GetValueOrDefault("dt.client.v1"),HomeDay);
    private void RefreshHomeOffers()
    {
        var day=HomeDay;if(homeOfferDay==day)return;
        ClearHomeOfferPreviews();
        homeOffers=CosmeticCollection.Featured(day).Select(offer=>(offer.Kind,offer.Item,Price:CosmeticCollection.Price(offer.Kind,offer.Item,day))).ToArray();
        HomeFeaturedKeys=homeOffers.Select(offer=>offer.Kind+"/"+offer.Item.Id).ToArray();homeOfferDay=day;
    }
    // .subnav: Chakra 600 14px .1em tabs (9px 14px 10px; 6px 12px 7px when short) over a gold-5 rule.
    private float HomeTabsHeight=>(socialWindowSize.Y<=820?13:19)+NativeUiTheme.LineHeight(NativeUiTheme.DisplayFont,14)+1;
    private void HomeTabs(float x,float y,float width)
    {
        bool shortWindow=socialWindowSize.Y<=820;float height=HomeTabsHeight;
        ClientLayer("Home tabs",new(x,y),new(width,height+4));
        var draw=ImGui.GetWindowDrawList();
        draw.AddLine(new(x,y+height-.5f),new(x+width,y+height-.5f),NativeUiPaint.Fade(NativeUiTheme.GoldDim));
        float left=x,padX=shortWindow?12:14,padTop=shortWindow?6:9;
        foreach(var tab in new[]{("overview","OVERVIEW"),("patch","PATCH NOTES"),("events","EVENTS")})
        {
            float tabWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayFont,14,tab.Item2,.1f)+padX*2;
            ImGui.SetCursorScreenPos(new(left,y));
            if(ClientHitButton("homeTab/"+tab.Item1,tabWidth,height-1))homeTab=tab.Item1;
            bool on=homeTab==tab.Item1,hot=ImGui.IsItemHovered();
            NativeUiPaint.Tracked(draw,NativeUiTheme.DisplayFont,14,new(left+padX,y+padTop),on||hot?NativeUiTheme.GoldBright:NativeUiTheme.LcMute,tab.Item2,.1f);
            if(on)
            {
                // ::after: left/right 12px, bottom -1px, 2px gold-2 with a 10px gold-3 glow.
                var lo=new float2(left+12,y+height-2);var hi=new float2(left+tabWidth-12,y+height);
                draw.AddRectFilled(lo,hi,NativeUiPaint.Fade(NativeUiTheme.Gold));
            }
            left+=tabWidth+4;
        }
        HomeCountdown=HomeRotation.Label(HomeRotation.UntilMidnight(HomeClock(),TimeZoneInfo.Local));ImGui.End();
    }
    private void HomeHighlightsList(int count)
    {
        foreach(var highlight in homeContent.Value.Patch.Take(count))
        {HomeLabel(highlight.Title,15,HomeInk);UiText.Wrapped(highlight.Description);ImGui.Spacing();HomeHighlights++;}
    }
    /// <summary>.patch-list: entries with a gold-4 left rule, 12.5px/700 title and a two-line 11px excerpt.</summary>
    private static float HomePatchList(ImDrawListPtr draw,float2 at,float width,IEnumerable<(string Title,string Text)> entries,float bottom)
    {
        float y=at.Y;
        foreach(var entry in entries)
        {
            float titleHeight=NativeUiTheme.TextLine(12.5f);
            // .tile-b clips with overflow: hidden, so a partly visible entry still shows.
            if(y>=bottom)break;
            float start=y;
            NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodyBoldFont,12.5f,new(at.X+12,y),titleHeight,NativeUiTheme.GoldBright,SocialEllipsis(entry.Title,NativeUiTheme.BodyBoldFont,12.5f,width-12));
            y+=titleHeight+1;
            y+=CssParagraph(draw,NativeUiTheme.BodyFont,11,new(at.X+12,y),width-12,NativeUiTheme.Muted,entry.Text,1.4f,2);
            draw.AddRectFilled(new(at.X,start),new(at.X+2,y),NativeUiPaint.Fade(NativeUiTheme.GoldBorder));
            y+=6;
        }
        return y-at.Y;
    }
    /// <summary>.tile-art: a 62px painting strip with a dark lower fade, an optional badge and set render.</summary>
    private void HomeTileArt(ImDrawListPtr draw,float2 at,float width,string art,string? badge,BadgeStyle style,string? render=null)
    {
        float height=HomeTileArtHeight;var hi=at+new float2(width,height);
        draw.PushClipRect(at,hi,true);
        homeArtwork.Draw(art,at,new(width,height),255,true,.5f,draw);
        NativeUiPaint.VerticalGradient(draw,at+new float2(0,height*.3f),hi,new ImColor8(1,10,19,0),new ImColor8(1,10,19,191));
        draw.PopClipRect();
        if(render is {Length:>0})
        {
            // .tile-render: right 6px, bottom -2px, 118% of the strip's height, drop shadow; it may rise above the strip.
            float renderHeight=height*1.18f;var natural=homeArtwork.Size(render);float renderWidth=renderHeight*natural.X/natural.Y;
            // .tile-card's overflow: hidden trims the part above the strip.
            draw.PushClipRect(at,hi+new float2(0,2),true);
            homeArtwork.Draw(render,new(hi.X-6-renderWidth,hi.Y+2-renderHeight),new(renderWidth,renderHeight),255,false,.5f,draw);
            draw.PopClipRect();
        }
        draw.AddLine(new(at.X,hi.Y-.5f),new(hi.X,hi.Y-.5f),NativeUiPaint.Fade(NativeUiTheme.GoldDim));
        if(badge is not null)
        {
            var size=Badge(draw,new(0,-1000),badge,style);
            Badge(draw,new(at.X+10,hi.Y-8-size.Y),badge,style);
        }
    }
    private void HomePatchPreview(float2 at,float2 extent)
    {
        HomePatchBounds=(at,at+extent);
        ClientLayer("Home patch preview",at,extent);var draw=ImGui.GetWindowDrawList();
        bool hovered=ImGui.IsMouseHoveringRect(at,at+extent)&&ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        PaintTile(draw,at,at+extent,hovered);
        var pad=HomeTilePadding;float inner=extent.X-pad.X*2;
        HomeTileArt(draw,at+new float2(1,1),extent.X-2,"hero-datastream-sky","Patch notes",BadgeStyle.New);
        float y=at.Y+HomeTileArtHeight+8;
        TileHeading(draw,new(at.X+pad.X,y),inner,"What's new");
        y+=NativeUiTheme.TextLine(12.5f)+6;
        float linkY=at.Y+extent.Y-pad.Y-NativeUiTheme.LineHeight(NativeUiTheme.DisplayBoldFont,11);
        draw.PushClipRect(at,new(at.X+extent.X,linkY-2),true);
        // A short window shows two patch entries; otherwise three, clipped by the tile.
        var entries=homeContent.Value.Patch.Take(socialWindowSize.Y<=820?2:3).Select(p=>(p.Title,p.Description)).ToArray();
        HomePatchList(draw,new(at.X+pad.X,y),inner,entries,linkY-2);HomeHighlights+=Math.Min(2,entries.Length);
        draw.PopClipRect();
        if(TileLink("homeReadPatch","Read the patch notes",new(at.X+pad.X,linkY)))homeTab="patch";
        ImGui.End();
    }
    private void HomeSetNews(float2 at,float2 extent)
    {
        ClientLayer("Home set news",at,extent);var draw=ImGui.GetWindowDrawList();
        bool hovered=ImGui.IsMouseHoveringRect(at,at+extent)&&ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        PaintTile(draw,at,at+extent,hovered);
        var set=HomeSets.Value[selectedSet];var pad=HomeTilePadding;float inner=extent.X-pad.X*2;
        HomeTileArt(draw,at+new float2(1,1),extent.X-2,set.Hero,set.Tag,BadgeStyle.Plain,set.Render);
        float y=at.Y+HomeTileArtHeight+8;
        TileHeading(draw,new(at.X+pad.X,y),inner,"This set: "+set.Name);
        y+=NativeUiTheme.TextLine(12.5f)+6;
        float linkY=at.Y+extent.Y-pad.Y-NativeUiTheme.LineHeight(NativeUiTheme.DisplayBoldFont,11);
        (string,string)[] bullets=selectedSet=="chaos"
            ?[("Any set","The seed picks the set. Everyone in the lobby plays the same one."),("Pool",string.Join(", ",SetChoices.Options.Where(o=>o.Id!="chaos").Select(o=>HomeSets.Value[o.Id].Name)))]
            :[(string.IsNullOrEmpty(set.Mechanic)?"Set mechanic":set.Mechanic,string.IsNullOrEmpty(set.MechanicDescription)?set.Description:set.MechanicDescription),("Roster",$"{Rules.Lines.Length} shop Digimon and {Rules.Traits.Length} traits.")];
        draw.PushClipRect(at,new(at.X+extent.X,linkY-2),true);
        HomePatchList(draw,new(at.X+pad.X,y),inner,bullets,linkY-2);
        draw.PopClipRect();
        if(TileLink("homeSetCodex","Open the Codex",new(at.X+pad.X,linkY)))OpenCodexFull();
        ImGui.End();
    }
    private void HomeTileExcerpt(string text,int lines)
    {
        var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;
        ImGui.PushFont(NativeUiTheme.BodyFont,14);float height=Math.Min(ImGui.CalcTextSize(text,false,width).Y,ImGui.GetTextLineHeight()*lines);
        var draw=ImGui.GetWindowDrawList();draw.PushClipRect(at,at+new float2(width,height),true);
        ImGui.PushStyleColor(ImGuiCol.Text,HomeMute);ImGui.PushTextWrapPos(0);ImGui.Text(text);ImGui.PopTextWrapPos();ImGui.PopStyleColor();draw.PopClipRect();ImGui.PopFont();
        if(ImGui.IsWindowHovered()&&ImGui.IsMouseHoveringRect(at,at+new float2(width,height)))ResultTooltip(text);
        ImGui.SetCursorScreenPos(at+new float2(0,height+4));ImGui.Dummy(new(width,0));
    }
    private void HomeArticleBanner(string image,string eyebrow,string title,string description,float height=150)
    {
        var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;
        homeArtwork.Draw(image,at,new(width,height),255,true);
        ImGui.GetWindowDrawList().AddRectFilled(at,at+new float2(width,height),new ImColor8(3,12,22,160));
        ImGui.SetCursorScreenPos(at+new float2(18,16));HomeLabel(eyebrow.ToUpperInvariant(),13);
        HomeLabel(title,30,HomeInk);ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+width-36);ImGui.Text(description);ImGui.PopTextWrapPos();
        // The artwork is drawn directly, so it still needs a layout item when
        // no optional Store/Farm button follows it (the profile-less client).
        ImGui.SetCursorScreenPos(at+new float2(0,height));
        ImGui.Dummy(new(width,Math.Max(0,12-ImGui.GetStyle().ItemSpacing.Y)));
    }
    private void HomePatchNotes(float2 at,float2 extent)
    {
        HomeArticleBounds=(at,at+extent);
        HomeCard("Home patch notes",at,extent);HomeArticleBanner("hero-digivice-energy","Patch notes","The new client","A launcher for everything outside the match.");
        HomeLabel("HIGHLIGHTS");HomeHighlightsList(homeContent.Value.Patch.Length);ImGui.Separator();HomeLabel("SETS IN THE ROTATION");
        foreach(var option in SetChoices.Options)
        {
            var set=HomeSets.Value[option.Id];var rules=catalog.Variants.First(r=>r.Id.Split('/')[0]==option.Id);
            HomeLabel(set.Name,20,HomeInk);UiText.Wrapped(set.Description);
            UiText.Wrapped($"{rules.Lines.Length} Digimon / {rules.Traits.Length} traits"+(set.Mechanic.Length>0?" / "+set.Mechanic:""));
            ImGui.Separator();HomeRotationSets++;
        }
        HomeArticleScroll=ImGui.GetScrollY();ImGui.End();
    }
    private void HomeMissions(DailyRewardProgress daily)
    {
        foreach(var mission in daily.Missions)
        {
            UiText.Wrapped(mission.Name);HomeLabel($"{mission.Bits} Bits  /  "+(mission.Done?"Complete":$"Best today {mission.Current}/{mission.Goal}"),12,HomeMute);
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram,new ImColor8(49,185,202));ImGui.ProgressBar((float)mission.Current/mission.Goal,new(-1,5),""u8);ImGui.PopStyleColor();
        }
        ImGui.Separator();UiText.Wrapped(daily.Claimed?"Daily bonus claimed. Back tomorrow.":$"Your first game today earns {daily.Bits} extra Bits.");
    }
    private void HomeEventsPanel(float2 at,float2 extent)
    {
        HomeArticleBounds=(at,at+extent);
        HomeCard("Home events",at,extent);var copy=homeContent.Value.Events;var daily=HomeDaily;
        HomeArticleBanner("events-bg","Events",copy.Title,copy.Countdown.Replace("{countdown}",HomeCountdown,StringComparison.Ordinal));
        HomeLabel(copy.Missions.ToUpperInvariant());HomeMissions(daily);ImGui.Spacing();
        HomeArticleBanner("store-hero","Store",copy.Store,daily.Claimed?"Four items at 25% off, new every day. Your daily bonus is claimed.":copy.StoreDescription.Replace("{bits}",daily.Bits.ToString(),StringComparison.Ordinal),130);
        if(profile is not null&&HomeButton("homeEventsStore","Open the Store",210,32))OpenHomeStore();
        ImGui.Spacing();HomeArticleBanner("hero-file-island","Digi-Farm",copy.Farm,copy.FarmDescription,130);
        if(farm is not null&&HomeButton("homeEventsFarm","Open the Digi-Farm",210,32)){NavigateClient("farm");}
        HomeArticleScroll=ImGui.GetScrollY();ImGui.End();
    }
    private void OpenHomeStore(string? kind=null,string? id=null)
    {
        if(profile is null)return;
        collectionOwnedOnly=false;collectionRarity="all";collectionSearch.SetValue("");
        OpenCollectionPage(store:true,kind:kind,item:id);
    }
    private void HomeOfferArt(string kind,CosmeticItem item,CosmeticCollection collection,float2 at,float2 extent)
    {
        if(!ImGui.IsRectVisible(at,at+extent))return;
        var draw=ImGui.GetWindowDrawList();
        if(kind=="emote")emoteArtwork.Draw(item.Id,(long)(boardSceneTime*1000),at,extent.Y);
        else if(kind=="killscreen")collectionTheme.Draw(item.Id,false,boardSceneTime%3.5,at,extent);
        else if(kind is "legend" or "dance" or "board")
        {
            string key=kind+":"+item.Id;if(!homeOfferPreviews.TryGetValue(key,out var scene))homeOfferPreviews[key]=scene=new();
            string legend=kind=="legend"?item.Id:collection.Equipped("legend")?.GetValue<string>()??"tokomon";
            var view=LegendEvolution.Inspect(profile?.Storage.GetValueOrDefault("dt.legendevo.v1"),profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"),profile?.Storage.GetValueOrDefault("dt.farm.v1"),legend);
            scene.Draw(kind,item.Id,LegendEvolution.VisualKey(legend,view.Form),boardSceneTime,ClientMotion);
            draw.AddImage(new ImTextureRef{_TexID=new((nint)scene.Texture)},at,at+extent,new(0,1),new(1,0));
        }
        else
        {var center=at+extent*.5f;draw.AddCircleFilled(center,extent.Y*.4f,new ImColor8(34,54,78));draw.AddTriangleFilled(center+new float2(-5,-9),center+new float2(-5,9),center+new float2(9,0),RarityColor(item.Rarity));}
    }
    /// <summary>style.css --r-* rarity tokens.</summary>
    private static ImColor8 RarityToken(string? rarity)=>rarity switch
    {
        "uncommon"=>new(95,211,106),"rare"=>new(79,179,255),"epic"=>new(199,125,255),"legendary"=>new(255,179,71),_=>new(154,166,187)
    };
    /// <summary>Bits amount: the .bit diamond and 800-weight gold-1 numerals. Returns the drawn width.</summary>
    private static float BitsAmount(ImDrawListPtr draw,float2 rightTop,int amount,float px=12.5f,bool measureOnly=false)
    {
        string text=amount.ToString("N0");float textWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.BodyExtraBoldFont,px,text,0),width=1+11+3+textWidth;
        if(measureOnly)return width;
        float height=NativeUiTheme.LineHeight(NativeUiTheme.BodyExtraBoldFont,px);
        NativeUiPaint.Bit(draw,new(rightTop.X-width+1+5.5f,rightTop.Y+height/2+.5f),11);
        NativeUiPaint.Tracked(draw,NativeUiTheme.BodyExtraBoldFont,px,new(rightTop.X-textWidth,rightTop.Y),NativeUiTheme.GoldBright,text);
        return width;
    }
    /// <summary>missionsHTML: each .mission (name, Bits, 8px progress, note) then the .daily bonus row.</summary>
    private void HomeMissions(DailyRewardProgress daily,float2 at,float width)
    {
        var draw=ImGui.GetWindowDrawList();float y=at.Y;
        float nameHeight=NativeUiTheme.TextLine(12.5f),noteHeight=NativeUiTheme.TextLine(11);
        foreach(var mission in daily.Missions)
        {
            y+=3;
            int current=mission.Done?mission.Goal:mission.Current;
            float bits=BitsAmount(draw,new(at.X+width,y+(nameHeight-NativeUiTheme.LineHeight(NativeUiTheme.BodyExtraBoldFont,12.5f))/2),mission.Bits);
            NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodySemiBoldFont,12.5f,new(at.X,y),nameHeight,mission.Done?NativeUiTheme.Muted:NativeUiTheme.Text1,SocialEllipsis(mission.Name,NativeUiTheme.BodySemiBoldFont,12.5f,width-bits-8));
            y+=nameHeight+4;
            Progress(draw,new(at.X,y),new(at.X+width,y+8),(float)current/Math.Max(1,mission.Goal),mission.Done);
            y+=8+3;
            string note=mission.Done?"Complete":mission.Goal>1?$"Best today {current}/{mission.Goal} · in one game":"Complete it in one game";
            NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodyFont,11,new(at.X,y),noteHeight,NativeUiTheme.Text3,SocialEllipsis(note,NativeUiTheme.BodyFont,11,width));
            y+=noteHeight+6;
        }
        // .daily: 26px hexagon icon | text | Bits, above a gold-5 rule.
        y+=2;draw.AddLine(new(at.X,y+.5f),new(at.X+width,y+.5f),NativeUiPaint.Fade(NativeUiTheme.GoldDim));y+=1+6;
        float rowHeight=Math.Max(26,nameHeight+3+noteHeight),iconTop=y+(rowHeight-26)/2;var icon=new float2(at.X+13,iconTop+13);
        float2[] hexagon=[icon+new float2(0,-13),icon+new float2(13,-6.5f),icon+new float2(13,6.5f),icon+new float2(0,13),icon+new float2(-13,6.5f),icon+new float2(-13,-6.5f)];
        var uv=ImGui.GetFontTexUvWhitePixel();var top=NativeUiPaint.Fade(daily.Claimed?new ImColor8(111,240,168):NativeUiTheme.PlayMiddle);var bottom=NativeUiPaint.Fade(daily.Claimed?new ImColor8(47,184,108):NativeUiTheme.Play);
        draw.PrimReserve(18,18);
        for(int i=0;i<6;i++){var p=hexagon[i];var q=hexagon[(i+1)%6];draw.PrimVtx(icon,uv,Mix(top,bottom,.5f));draw.PrimVtx(p,uv,Mix(top,bottom,(p.Y-icon.Y+13)/26));draw.PrimVtx(q,uv,Mix(top,bottom,(q.Y-icon.Y+13)/26));}
        string mark=daily.Claimed?"✓":"+";var markSize=NativeUiPaint.TrackedSize(NativeUiTheme.BodyExtraBoldFont,14,mark,0);
        if(daily.Claimed){var c=NativeUiPaint.Fade(NativeUiTheme.PlayText);draw.AddLine(icon+new float2(-5,0),icon+new float2(-1.5f,4),c,2.2f);draw.AddLine(icon+new float2(-1.5f,4),icon+new float2(5.5f,-4),c,2.2f);}
        else NativeUiPaint.Tracked(draw,NativeUiTheme.BodyExtraBoldFont,14,icon-markSize*.5f,NativeUiTheme.PlayText,mark);
        float dailyBits=BitsAmount(draw,new(at.X+width,y+(rowHeight-nameHeight)/2),daily.Bits);
        float textLeft=at.X+26+10,textWidth=width-36-dailyBits-10,textTop=y+(rowHeight-nameHeight-3-noteHeight)/2;
        NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodyBoldFont,12.5f,new(textLeft,textTop),nameHeight,NativeUiTheme.Text1,"Daily bonus");
        NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodyFont,11,new(textLeft,textTop+nameHeight+3),noteHeight,NativeUiTheme.Text3,
            SocialEllipsis(daily.Claimed?"Claimed. Back tomorrow.":$"First game today pays {daily.Bits} extra Bits.",NativeUiTheme.BodyFont,11,textWidth));
    }
    private const float HomeOfferHeight=50;
    /// <summary>.feat: 40px preview | name and kind | sale badge and price, framed in the item's rarity color.</summary>
    private void HomeOfferRow(string kind,CosmeticItem item,int price,CosmeticCollection collection,float2 at,float width)
    {
        bool owned=collection.Owns(kind,item.Id);HomeFeaturedCount++;if(owned)HomeFeaturedOwned++;
        ImGui.SetCursorScreenPos(at);
        bool clicked=ClientHitButton("homeFeatured/"+kind+"/"+item.Id,width,HomeOfferHeight);bool hot=ImGui.IsItemHovered();
        var draw=ImGui.GetWindowDrawList();var hi=at+new float2(width,HomeOfferHeight);var rarity=RarityToken(item.Rarity);
        draw.AddRectFilled(at,hi,NativeUiPaint.Fade(new ImColor8(1,10,19,153)));
        draw.AddRect(at,hi,NativeUiPaint.Fade(hot?rarity:Mix(NativeUiTheme.GoldDim,rarity,.35f)));
        var previewLo=at+new float2(5,5);var previewHi=previewLo+new float2(40,40);
        draw.AddRectFilled(previewLo,previewHi,NativeUiPaint.Fade(NativeUiTheme.Navy0));
        draw.PushClipRect(previewLo,previewHi,true);
        NativeUiPaint.Radial(draw,previewLo+new float2(20,20),new(28),Mix(new ImColor8(10,20,40),rarity,.3f),NativeUiTheme.Navy0,24);
        HomeOfferArt(kind,item,collection,previewLo,new(40,40));
        draw.PopClipRect();
        float priceWidth;
        float nameHeight=NativeUiTheme.LineHeight(NativeUiTheme.BodyBoldFont,12.5f),kindHeight=NativeUiTheme.LineHeight(NativeUiTheme.BodyFont,11);
        float middle=at.Y+HomeOfferHeight/2;
        if(owned)
        {
            var size=Badge(draw,new(0,-1000),"Owned",BadgeStyle.Owned);priceWidth=size.X;
            Badge(draw,new(hi.X-8-size.X,middle-size.Y/2),"Owned",BadgeStyle.Owned);
        }
        else
        {
            float bits=BitsAmount(draw,new(hi.X-8,MathF.Round(middle-NativeUiTheme.LineHeight(NativeUiTheme.BodyExtraBoldFont,12.5f)/2)),price);
            // .sale: 1px 6px, Chakra 800 10.5px, .06em, white on a red gradient, 6px before the price.
            string sale=$"-{Math.Round(100-100d*price/Math.Max(1,item.Price),MidpointRounding.AwayFromZero)}%";
            float saleWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayBoldFont,10.5f,sale,.06f)+12+2,saleHeight=NativeUiTheme.LineHeight(NativeUiTheme.DisplayBoldFont,10.5f)+2+2;
            var saleLo=new float2(hi.X-8-bits-6-saleWidth,MathF.Round(middle-saleHeight/2));var saleHi=saleLo+new float2(saleWidth,saleHeight);
            NativeUiPaint.VerticalGradient(draw,saleLo,saleHi,new ImColor8(232,67,63),new ImColor8(165,34,31));
            draw.AddRect(saleLo,saleHi,NativeUiPaint.Fade(new ImColor8(255,138,122)));
            NativeUiPaint.Tracked(draw,NativeUiTheme.DisplayBoldFont,10.5f,saleLo+new float2(7,2),new ImColor8(255,255,255),sale,.06f);
            priceWidth=bits+6+saleWidth;
        }
        float textLeft=previewHi.X+8,textWidth=hi.X-8-priceWidth-8-textLeft,textTop=MathF.Round(middle-(nameHeight+kindHeight)/2);
        NativeUiPaint.Tracked(draw,NativeUiTheme.BodyBoldFont,12.5f,new(textLeft,textTop),NativeUiTheme.GoldBright,SocialEllipsis(item.Name,NativeUiTheme.BodyBoldFont,12.5f,textWidth));
        string kindName=CollectionNames[Array.IndexOf(CosmeticCollection.Kinds,kind)];
        NativeUiPaint.Tracked(draw,NativeUiTheme.BodyFont,11,new(textLeft,textTop+nameHeight),NativeUiTheme.Text3,SocialEllipsis(HomeKindOne(kind,kindName),NativeUiTheme.BodyFont,11,textWidth));
        if(clicked)OpenHomeStore(kind,item.Id);
    }
    // client.js KIND_ONE: the singular kind label shown under featured items.
    private static string HomeKindOne(string kind,string fallback)=>kind switch
    {
        "legend"=>"Little Legend","board"=>"Board","killscreen"=>"Kill Screen","dance"=>"Dance","emote"=>"Emote","sound"=>"Sound","elimsound"=>"Elim Sound",_=>fallback
    };
    private void ClearHomeOfferPreviews(){foreach(var scene in homeOfferPreviews.Values)scene.Dispose();homeOfferPreviews.Clear();}
}

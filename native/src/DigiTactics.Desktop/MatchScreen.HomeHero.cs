using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private HomeCarousel? homeCarousel;
    private HomeHeroSlide[]? homeHeroSlides;
    private readonly Dictionary<int,(double Opacity,double Scale,double At,bool Active)> homeHeroArtStates=[];
    private int homeHeroVisualIndex=-1;
    private bool homeHeroScrollSelected,showHomePlay,homeHeroWasVisible,homeHeroDrawnThisFrame,homePlayParty,homePlayOnline;
    internal Func<double> HomeHeroClock {get;set;}=()=>Environment.TickCount64/1000d;
    internal string HomeHeroId=>homeHeroSlides is {} slides&&homeCarousel is {} carousel?slides[carousel.Index].Id:"";
    internal int HomeHeroCount=>homeHeroSlides?.Length??0;
    internal double HomeHeroDeadline=>homeCarousel?.Due??double.NaN;
    internal double HomeHeroAge=>homeCarousel is null?0:HomeHeroClock()-homeCarousel.RenderedAt;
    internal bool HomePlayVisible=>showHomePlay;
    internal bool HomePlayPartyVisible=>showHomePlay&&homePlayParty;
    internal (float2 Min,float2 Max) HomeHeroBounds {get;private set;}
    internal (float2 Min,float2 Max) HomeHeroThumbBounds {get;private set;}
    private bool HomeHeroPage=>!PostgamePending&&ClientHomeVisible&&!showMultiplayer&&!showFarm&&!showProfile&&!showSettings&&!showSaveData&&!showAbout&&!showHomePlay;
    private bool HomeHeroModule=>showCollection||CodexVisible||showMusicLibrary||showAudio||KeyboardPopup;
    private void TickHomeHero()
    {
        if(homeCarousel is null)return;
        homeHeroWasVisible=homeHeroDrawnThisFrame;homeHeroDrawnThisFrame=false;
        var mouse=ImGui.GetIO().MousePos;var bounds=HomeHeroBounds;
        bool hover=homeHeroWasVisible&&HomeHeroPage&&!HomeHeroModule&&homeTab=="overview"&&mouse.X>=bounds.Min.X&&mouse.X<bounds.Max.X&&mouse.Y>=bounds.Min.Y&&mouse.Y<bounds.Max.Y;
        int before=homeCarousel.Index;
        homeCarousel.Tick(HomeHeroClock(),HomeHeroPage,ClientHomeVisible,HomeHeroModule,PostgameVisible,homeTab=="overview",hover);
        if(before!=homeCarousel.Index)homeHeroScrollSelected=true;
    }
    private void SelectHomeHero(int index)
    {homeCarousel!.Render(HomeHeroClock(),index);homeHeroScrollSelected=true;}
    private void SelectHomeHeroSet(string id)
    {
        SelectSet(id);
        if(homeHeroSlides is {} slides&&Array.FindIndex(slides,s=>s.Act=="set:"+id) is var index&&index>=0)SelectHomeHero(index);
    }
    private (double Opacity,double Scale) HeroArtValue((double Opacity,double Scale,double At,bool Active) state,double now)
    {
        double opacity=HomeCarousel.Opacity(now-state.At,ClientMotion),zoom=Math.Clamp((now-state.At)/DigiTactics.Presentation.ClientMotion.CssDuration(9,ClientMotion),0,1);
        return(state.Opacity+((state.Active?1:0)-state.Opacity)*opacity,state.Scale+((state.Active?1:1.06)-state.Scale)*zoom);
    }
    private void DrawHomeHeroArt(float2 at,float2 extent,double now)
    {
        var slides=homeHeroSlides!;int active=homeCarousel!.Index;
        if(active!=homeHeroVisualIndex)
        {
            for(int i=0;i<slides.Length;i++)
            {
                var value=homeHeroArtStates.TryGetValue(i,out var old)?HeroArtValue(old,now):(Opacity:0d,Scale:1.06);
                homeHeroArtStates[i]=(value.Opacity,value.Scale,now,i==active);
            }
            homeHeroVisualIndex=active;
        }
        var draw=ImGui.GetWindowDrawList();draw.PushClipRect(at,at+extent,true);
        for(int i=0;i<slides.Length;i++)
        {
            var value=HeroArtValue(homeHeroArtStates[i],now);if(value.Opacity<=0)continue;
            var expanded=extent*(float)value.Scale;
            homeArtwork.Draw(slides[i].Art,at+(extent-expanded)*.5f,expanded,(byte)Math.Round(value.Opacity*255),true,.4f);
        }
        draw.PopClipRect();
    }
    private void HomeHeroText(float2 position,float width,float height,string text,float font,ImColor8 color,int child,double now)
    {
        float progress=(float)HomeCarousel.TextProgress(now-homeCarousel!.RenderedAt,child,ClientMotion);
        position+=new float2(0,14*(1-progress));
        var draw=ImGui.GetWindowDrawList();draw.PushClipRect(position,position+new float2(width,height),true);
        ImGui.SetCursorScreenPos(position);ImGui.PushStyleVar(ImGuiStyleVar.Alpha,progress);
        ImGui.PushFont(font>=22?NativeUiTheme.DisplayBoldFont:NativeUiTheme.BodyFont,font);ImGui.PushStyleColor(ImGuiCol.Text,color);ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+width);
        ImGui.Text(text);ImGui.PopTextWrapPos();ImGui.PopStyleColor();ImGui.PopFont();ImGui.PopStyleVar();draw.PopClipRect();
    }
    private void HomeHero(float2 at,float2 extent)
    {
        RefreshHomeOffers();
        double now=HomeHeroClock();homeHeroSlides??=homeContent.Value.Hero.Slides();
        if(homeCarousel is null){homeCarousel=new(homeHeroSlides.Length);homeCarousel.Render(now);}
        else if(!homeHeroWasVisible)homeCarousel.Render(now);
        HomeHeroBounds=(at,at+extent);homeHeroDrawnThisFrame=true;
        var slide=homeHeroSlides[homeCarousel.Index];HomeHeroArt=slide.Art;
        HomeCard("Home featured set",at,extent,false);var draw=ImGui.GetWindowDrawList();DrawHomeHeroArt(at,extent,now);
        draw.AddRectFilledMultiColor(at,at+extent,new ImColor8(1,10,19,235),new ImColor8(1,10,19,0),new ImColor8(1,10,19,45),new ImColor8(1,10,19,245));
        draw.AddRectFilledMultiColor(at+new float2(0,extent.Y-135),at+extent,new ImColor8(1,10,19,0),new ImColor8(1,10,19,0),new ImColor8(1,10,19,245),new ImColor8(1,10,19,245));
        draw.AddRect(at,at+extent,HomeGold,2);
        string title=slide.Title,description=slide.Desc,meta=slide.Meta??"";string? offerKind=null,offerId=null;
        float contentBottom=extent.Y-70,textWidth=Math.Min(560,extent.X*.52f)-48;
        if(slide.Feat)
        {
            int pick=(int)((HomeClock().ToUnixTimeMilliseconds()/86400000)%homeOffers.Length);var offer=homeOffers[pick];
            var collection=new CosmeticCollection(profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"));int price=offer.Price;
            offerKind=offer.Kind;offerId=offer.Item.Id;title=offer.Item.Name;description=!string.IsNullOrEmpty(offer.Item.Desc)?offer.Item.Desc:CollectionNames[Array.IndexOf(CosmeticCollection.Kinds,offer.Kind)]+", "+offer.Item.Rarity+".";
            meta=offer.Item.Rarity+" / "+(collection.Owns(offer.Kind,offer.Item.Id)?"Owned":$"{price} Bits / -{Math.Round(100-100d*price/Math.Max(1,offer.Item.Price),MidpointRounding.AwayFromZero)}%");
            float previewWidth=extent.X*.35f,previewHeight=Math.Min(contentBottom-16,previewWidth*.75f);
            HomeOfferArt(offer.Kind,offer.Item,collection,at+new float2(extent.X-previewWidth-20,12),new(previewWidth,previewHeight));
        }
        float titleFont=Math.Min(extent.Y<380?48:76,Math.Clamp(socialWindowSize.X*.0475f,42,76));
        float titleWidth=Math.Min(textWidth,extent.X*.42f);
        // Match the source: type is anchored above the thumbnail strip, leaving
        // the upper/right artwork unobstructed instead of a top-aligned text box.
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,titleFont);
        var titleLines=new List<string>();string titleLine="";
        foreach(string word in title.ToUpperInvariant().Split(' ',StringSplitOptions.RemoveEmptyEntries))
        {
            string next=titleLine.Length==0?word:titleLine+" "+word;
            if(titleLine.Length>0&&ImGui.CalcTextSize(next).X>titleWidth){titleLines.Add(titleLine);titleLine=word;}
            else titleLine=next;
        }
        if(titleLine.Length>0)titleLines.Add(titleLine);
        float titleLineHeight=titleFont*.82f,titleHeight=titleLines.Count*titleLineHeight;ImGui.PopFont();
        float descriptionHeight=extent.Y<380?32:60,metaHeight=meta.Length>0?(extent.Y<380?20:24):0;
        float blockHeight=20+8+titleHeight+8+metaHeight+descriptionHeight+14+40;
        float textTop=Math.Max(18,contentBottom-blockHeight);
        HomeHeroText(at+new float2(32,textTop),textWidth,20,slide.Eyebrow.ToUpperInvariant(),13,HomeGold,0,now);
        for(int line=0;line<titleLines.Count;line++)HomeHeroText(at+new float2(32,textTop+28+line*titleLineHeight),titleWidth,titleLineHeight,titleLines[line],titleFont,HomeInk,1,now);
        float metaTop=textTop+28+titleHeight+8,descriptionTop=metaTop+metaHeight;
        if(meta.Length>0)HomeHeroText(at+new float2(32,metaTop),textWidth,22,meta,15,HomeGold,2,now);
        HomeHeroText(at+new float2(32,descriptionTop),textWidth,descriptionHeight,description,17,HomeMute,meta.Length>0?3:2,now);
        float ctaAlpha=(float)HomeCarousel.TextProgress(now-homeCarousel.RenderedAt,meta.Length>0?4:3,ClientMotion);
        ImGui.SetCursorScreenPos(at+new float2(32,contentBottom-40+14*(1-ctaAlpha)));ImGui.PushStyleVar(ImGuiStyleVar.Alpha,ctaAlpha);
        if(HomeButton("homeHeroAction",slide.Cta.ToUpperInvariant(),Math.Min(textWidth,280),40,true))HomeHeroAction(slide,offerKind,offerId);
        ImGui.PopStyleVar();
        DrawHomeHeroThumbnails(at+new float2(32,extent.Y-56),new(extent.X-48,42));
        ImGui.End();
    }
    private void DrawHomeHeroThumbnails(float2 at,float2 extent)
    {
        ImGui.SetCursorScreenPos(at);
        if(HomeButton("homeHeroPrevious","<",36,42))SelectHomeHero((homeCarousel!.Index+homeHeroSlides!.Length-1)%homeHeroSlides.Length);
        ImGui.SetCursorScreenPos(at+new float2(extent.X-36,0));
        if(HomeButton("homeHeroNext",">",36,42))SelectHomeHero((homeCarousel!.Index+1)%homeHeroSlides!.Length);
        at+=new float2(44,0);extent-=new float2(88,0);
        HomeHeroThumbBounds=(at,at+extent);ImGui.SetCursorScreenPos(at);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));ImGui.BeginChild("Home hero thumbnails"u8,extent,ImGuiChildFlags.None,ImGuiWindowFlags.HorizontalScrollbar);ImGui.PopStyleVar();
        var draw=ImGui.GetWindowDrawList();float width=Math.Max(58,(extent.X-(homeHeroSlides!.Length-1)*6)/homeHeroSlides.Length);
        for(int i=0;i<homeHeroSlides!.Length;i++)
        {
            if(i>0)ImGui.SameLine(0,6);var position=ImGui.GetCursorScreenPos();var slide=homeHeroSlides[i];
            bool clicked=HomeButton("homeHeroThumb/"+slide.Id,"",width,42,homeCarousel!.Index==i);
            homeArtwork.Draw(slide.Art,position,new(width,42),homeCarousel.Index==i?(byte)255:(byte)170,true);
            draw.AddRectFilledMultiColor(position,position+new float2(width,42),new ImColor8(1,10,19,35),new ImColor8(1,10,19,35),new ImColor8(1,10,19,230),new ImColor8(1,10,19,230));
            draw.AddRect(position,position+new float2(width,42),homeCarousel.Index==i?HomeGold:HomeLine);
            draw.PushClipRect(position+new float2(4,2),position+new float2(width-4,40),true);ImGui.PushFont(NativeUiTheme.DisplayBoldFont,12);draw.AddText(position+new float2(5,23),HomeInk,LobbyEllipsis(slide.Thumb.ToUpperInvariant(),width-10));ImGui.PopFont();draw.PopClipRect();
            if(homeCarousel.Index==i)
            {draw.AddRectFilled(position+new float2(0,40),position+new float2(width*(float)homeCarousel.Progress(ClientMotion),42),new ImColor8(63,214,255));if(homeHeroScrollSelected)ImGui.SetScrollHereX(.5f);}
            if(ImGui.IsItemHovered())ResultTooltip(slide.Thumb);
            if(clicked)SelectHomeHero(i);
        }
        homeHeroScrollSelected=false;ImGui.EndChild();
    }
    private void HomeHeroAction(HomeHeroSlide slide,string? kind,string? id)
    {
        if(slide.Act.StartsWith("set:",StringComparison.Ordinal)){RememberClientRoute("play");SelectSet(slide.Act[4..]);homePlayParty=true;homePlayOnline=false;showHomePlay=true;}
        else if(slide.Feat)OpenHomeStore(kind,id);
        else if(slide.Act=="mp")OpenMultiplayerView();
        else if(slide.Act=="farm"&&farm is not null){NavigateClient("farm");}
        else if(slide.Act=="loot"&&profile is not null)OpenCollectionPage(loot:true);
    }
}

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
    private bool HomeHeroPage=>ClientHomeVisible&&!showMultiplayer&&!showFarm&&!showProfile&&!showSettings&&!showSaveData&&!showAbout&&!showHomePlay;
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
        float contentBottom=extent.Y-82,textWidth=extent.X*(slide.Feat ? .60f : .83f)-32;
        if(slide.Feat)
        {
            int pick=(int)((HomeClock().ToUnixTimeMilliseconds()/86400000)%homeOffers.Length);var offer=homeOffers[pick];
            var collection=new CosmeticCollection(profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"));int price=offer.Price;
            offerKind=offer.Kind;offerId=offer.Item.Id;title=offer.Item.Name;description=!string.IsNullOrEmpty(offer.Item.Desc)?offer.Item.Desc:CollectionNames[Array.IndexOf(CosmeticCollection.Kinds,offer.Kind)]+", "+offer.Item.Rarity+".";
            meta=offer.Item.Rarity+" / "+(collection.Owns(offer.Kind,offer.Item.Id)?"Owned":$"{price} Bits / -{Math.Round(100-100d*price/Math.Max(1,offer.Item.Price),MidpointRounding.AwayFromZero)}%");
            float previewWidth=extent.X*.35f,previewHeight=Math.Min(contentBottom-16,previewWidth*.75f);
            HomeOfferArt(offer.Kind,offer.Item,collection,at+new float2(extent.X-previewWidth-20,12),new(previewWidth,previewHeight));
        }
        HomeHeroText(at+new float2(24,18),textWidth,20,slide.Eyebrow.ToUpperInvariant(),15,HomeGold,0,now);
        float titleFont=36;
        HomeHeroText(at+new float2(24,48),textWidth,extent.Y<350?64:86,title.ToUpperInvariant(),titleFont,HomeInk,1,now);
        float metaTop=extent.Y<350?116:142,descriptionTop=meta.Length>0?metaTop+26:metaTop;
        if(meta.Length>0)HomeHeroText(at+new float2(24,metaTop),textWidth,22,meta,16,HomeInk,2,now);
        float descriptionHeight=Math.Max(44,contentBottom-descriptionTop-60);
        draw.AddRectFilledMultiColor(at+new float2(16,descriptionTop-6),at+new float2(textWidth+40,descriptionTop+descriptionHeight+6),new ImColor8(1,10,19,190),new ImColor8(1,10,19,160),new ImColor8(1,10,19,160),new ImColor8(1,10,19,190));
        HomeHeroText(at+new float2(24,descriptionTop),textWidth,descriptionHeight,description,17,HomeInk,meta.Length>0?3:2,now);
        float ctaAlpha=(float)HomeCarousel.TextProgress(now-homeCarousel.RenderedAt,meta.Length>0?4:3,ClientMotion);
        ImGui.SetCursorScreenPos(at+new float2(24,contentBottom-44+14*(1-ctaAlpha)));ImGui.PushStyleVar(ImGuiStyleVar.Alpha,ctaAlpha);
        if(HomeButton("homeHeroAction",slide.Cta,Math.Min(textWidth,280),40,true))HomeHeroAction(slide,offerKind,offerId);
        ImGui.PopStyleVar();
        DrawHomeHeroThumbnails(at+new float2(16,extent.Y-70),new(extent.X-32,60));
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
        var draw=ImGui.GetWindowDrawList();float width=120;
        for(int i=0;i<homeHeroSlides!.Length;i++)
        {
            if(i>0)ImGui.SameLine(0,6);var position=ImGui.GetCursorScreenPos();var slide=homeHeroSlides[i];
            bool clicked=HomeButton("homeHeroThumb/"+slide.Id,"",width,42,homeCarousel!.Index==i);
            homeArtwork.Draw(slide.Art,position,new(width,42),homeCarousel.Index==i?(byte)255:(byte)170,true);
            draw.AddRectFilledMultiColor(position,position+new float2(width,42),new ImColor8(1,10,19,35),new ImColor8(1,10,19,35),new ImColor8(1,10,19,230),new ImColor8(1,10,19,230));
            draw.AddRect(position,position+new float2(width,42),homeCarousel.Index==i?HomeGold:HomeLine);
            draw.PushClipRect(position+new float2(4,2),position+new float2(width-4,40),true);ImGui.PushFont(default,14);draw.AddText(position+new float2(5,23),HomeInk,slide.Thumb.ToUpperInvariant());ImGui.PopFont();draw.PopClipRect();
            if(homeCarousel.Index==i)
            {draw.AddRectFilled(position+new float2(0,40),position+new float2(width*(float)homeCarousel.Progress(ClientMotion),42),new ImColor8(63,214,255));if(homeHeroScrollSelected)ImGui.SetScrollHereX(.5f);}
            if(ImGui.IsItemHovered())ResultTooltip(slide.Thumb);
            if(clicked)SelectHomeHero(i);
        }
        homeHeroScrollSelected=false;ImGui.EndChild();
    }
    private void HomeHeroAction(HomeHeroSlide slide,string? kind,string? id)
    {
        if(slide.Act.StartsWith("set:",StringComparison.Ordinal)){SelectSet(slide.Act[4..]);homePlayParty=true;homePlayOnline=false;showHomePlay=true;}
        else if(slide.Feat)OpenHomeStore(kind,id);
        else if(slide.Act=="mp")OpenMultiplayerView();
        else if(slide.Act=="farm"&&farm is not null){farm.Refresh(HomeClock().ToUnixTimeMilliseconds());showFarm=true;}
        else if(slide.Act=="loot"&&profile is not null)OpenCollectionPage(loot:true);
    }
    private void HomePlayPanel()
    {
        var at=new float2(16,ClientPageTop);float width=size.X-32,height=size.Y-ClientPageTop-16;
        HomeCard("Choose your match",at,new(width,height),false,true);
        if(homePlayParty){HomeSoloParty(at,new(width,height));ImGui.End();return;}
        HomeLabel("CHOOSE A MODE",15);HomeLabel("PLAY",32,HomeInk);
        float footer=height-158;
        ImGui.BeginChild("Mode cards"u8,new(width-32,footer-92),ImGuiChildFlags.None);
        int columns=3,index=0;float cardWidth=(ImGui.GetContentRegionAvail().X-16)/columns;
        foreach(string id in SetChoices.Options.Select(s=>s.Id).Concat(SetChoices.HasChaos?["chaos"]:Array.Empty<string>()))
        {
            if(index++%columns!=0)ImGui.SameLine(0,8);
            var cardAt=ImGui.GetCursorScreenPos();var set=HomeSets.Value[id];
            bool chosen=HomeButton("homeMode/"+id,"",cardWidth,148,selectedSet==id);
            homeArtwork.Draw(set.Hero,cardAt,new(cardWidth,106),255,true);
            var draw=ImGui.GetWindowDrawList();draw.AddRect(cardAt,cardAt+new float2(cardWidth,148),selectedSet==id?HomeInk:HomeLine,1);
            draw.PushClipRect(cardAt+new float2(8,108),cardAt+new float2(cardWidth-8,146),true);
            ImGui.PushFont(NativeUiTheme.DisplayFont,19);draw.AddText(cardAt+new float2(12,118),HomeInk,set.Name);ImGui.PopFont();draw.PopClipRect();
            if(ImGui.IsItemHovered())ResultTooltip(set.Description);
            if(chosen)SelectHomeHeroSet(id);
        }
        ImGui.EndChild();
        ImGui.SetCursorScreenPos(at+new float2(16,footer));
        if(HomeButton("homeModeSolo","SOLO VS AI",(width-40)/2,42,!homePlayOnline))homePlayOnline=false;
        ImGui.SameLine(0,8);if(HomeButton("homeModeOnline","MULTIPLAYER",(width-40)/2,42,homePlayOnline))homePlayOnline=true;
        ImGui.SetCursorScreenPos(at+new float2(16,footer+52));
        ImGui.SetNextItemWidth(width*.40f);DrawSetChoice("homePlayRuleset",selectedSet,SelectHomeHeroSet);
        ImGui.SameLine();ImGui.SetNextItemWidth(width*.25f);DrawPaceChoice("homePlayPace",selectedPace,false,pace=>SelectPace(pace));
        ImGui.SameLine();ImGui.SetNextItemWidth(width*.25f);DrawFormatChoice("homePlayFormat",duos,SelectFormat);
        ImGui.SetCursorScreenPos(at+new float2(16,height-52));
        if(HomeButton("homePlayBack","Back to Home",160,40))showHomePlay=false;
        ImGui.SameLine();if(HomeButton("homePlayConfirm","CONFIRM",width-200,40,true)){if(homePlayOnline)OpenMultiplayerView();else homePlayParty=true;}
        ImGui.End();
    }
    private void HomeSoloParty(float2 at,float2 extent)
    {
        var set=HomeSets.Value[selectedSet];float width=extent.X,height=extent.Y;
        homeArtwork.Draw(set.Hero,at,new(width,160),255,true,.4f);
        ImGui.GetWindowDrawList().AddRectFilled(at,at+new float2(width,160),new ImColor8(1,10,19,180));
        ImGui.SetCursorScreenPos(at+new float2(24,18));
        if(HomeButton("homePlayModes","< Modes",120,36))homePlayParty=false;
        HomeLabel("SOLO VS AI / PARTY",15);HomeLabel(set.Name,36,HomeInk);
        ImGui.SetCursorScreenPos(at+new float2(24,180));
        ImGui.SetNextItemWidth(width*.40f);DrawSetChoice("homePlayRuleset",selectedSet,SelectHomeHeroSet);
        ImGui.SameLine();ImGui.SetNextItemWidth(width*.25f);DrawPaceChoice("homePlayPace",selectedPace,false,pace=>SelectPace(pace));
        ImGui.SameLine();ImGui.SetNextItemWidth(width*.25f);DrawFormatChoice("homePlayFormat",duos,SelectFormat);
        ImGui.SetCursorScreenPos(at+new float2(24,230));
        ImGui.BeginChild("Solo party instructions"u8,new(width-48,height-312),ImGuiChildFlags.None);
        HomeLabel(duos?"YOU + YOUR AI PARTNER":"YOU + SEVEN AI TAMERS",22,HomeInk);
        for(int seat=0;seat<8;seat++)
        {
            if(seat%4!=0)ImGui.SameLine(0,8);
            HomeButton("homePartySeat/"+seat,seat==0?"YOU":duos&&seat==1?"AI PARTNER":"AI TAMER",(width-72)/4,48,seat==0);
        }
        ImGui.Spacing();HomeLabel("HOW TO PLAY",18);
        UiText.Wrapped("Buy Digimon and place them on your half of the board. Your level sets your team size."u8);
        UiText.Wrapped("Three copies digivolve. Three 1-star copies make a 2-star, and three 2-stars make a 3-star."u8);
        UiText.Wrapped("Drag items onto your Digimon. Two components combine into a completed item."u8);
        UiText.Wrapped("Right-click to move your Little Legend. Walk into loot orbs to collect them."u8);
        ImGui.EndChild();
        ImGui.SetCursorScreenPos(at+new float2(24,height-60));
        if(HomeButton("homePlayBack","Back to Home",160,44))showHomePlay=false;
        ImGui.SameLine();bool inRoom=networkRoom is not null||steam?.Room is not null;ImGui.BeginDisabled(inRoom);
        if(HomeButton("homePlayStart",duos?"PLAY DUOS":"PLAY SOLO",width-216,44,true)){showHomePlay=false;StartLocalMatch(duos);}
        ImGui.EndDisabled();
    }
}

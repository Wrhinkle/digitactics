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
    private int homeModeScrollRequest=-1;
    private string? homeModeShownSet;
    private (float X,float Scroll)? homeModeDrag;
    private bool homeModeDragged;
    internal int HomeModeCardCount {get;private set;}
    internal int HomePartySeatCount {get;private set;}
    internal bool HomePlayLayoutFits {get;private set;}
    internal (float2 Min,float2 Max) HomeModeCarouselBounds {get;private set;}
    private void HomePlayPanel()
    {
        RefreshSetSelection();
        var at=new float2(24,ClientPageTop+12);float width=size.X-48,height=size.Y-ClientPageTop-28;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize,0);
        Window("Choose your match",at.X,at.Y,width,height,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoBackground|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleVar(2);HomeModeCardCount=HomePartySeatCount=0;HomePlayLayoutFits=width>=700&&height>=520;HomePlayRankLayoutFits=false;
        if(homePlayParty){HomeSoloParty(at,new(width,height));ImGui.End();return;}
        ImGui.SetCursorScreenPos(at);HomeLabel("CHOOSE A MODE",15);HomeLabel("PLAY",30,HomeInk);
        float rankHeight=DrawHomePlayRank(at,width),navigationTop=Math.Max(60,rankHeight+2),carouselTop=navigationTop+48;
        HomePlayLayoutFits&=HomePlayRankLayoutFits;
        string[] ids=SetChoices.Options.Select(s=>s.Id).Concat(SetChoices.HasChaos?["chaos"]:Array.Empty<string>()).ToArray();
        int selected=Math.Max(0,Array.IndexOf(ids,selectedSet));
        ImGui.SetCursorScreenPos(at+new float2(0,navigationTop+14));HomeLabel("Choose a set. Swipe or scroll to explore.",16,HomeMute);
        ImGui.SetCursorScreenPos(at+new float2(width-174,navigationTop));
        if(HomeButton("homeModePrevious","<",44,40))homeModeScrollRequest=Math.Max(0,selected-1);
        ImGui.SetCursorScreenPos(at+new float2(width-120,navigationTop+12));HomeLabel($"{selected+1} / {ids.Length}",16,HomeInk);
        ImGui.SetCursorScreenPos(at+new float2(width-44,navigationTop));if(HomeButton("homeModeNext",">",44,40))homeModeScrollRequest=Math.Min(ids.Length-1,selected+1);
        if(homeModeScrollRequest>=0&&homeModeScrollRequest<ids.Length)SelectHomeHeroSet(ids[homeModeScrollRequest]);
        float footer=height-138,carouselHeight=footer-carouselTop-8,cardHeight=carouselHeight-22;
        float cardWidth=Math.Clamp(socialWindowSize.X*(height<700?.23f:.27f),240,height<700?300:370);
        var carouselAt=at+new float2(0,carouselTop);HomeModeCarouselBounds=(carouselAt,carouselAt+new float2(width,carouselHeight));
        ImGui.SetCursorScreenPos(carouselAt);ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(4,4));
        ImGui.BeginChild("Mode cards"u8,new(width,carouselHeight),ImGuiChildFlags.None,ImGuiWindowFlags.HorizontalScrollbar|ImGuiWindowFlags.NoScrollWithMouse);ImGui.PopStyleVar();
        if(ImGui.IsWindowHovered()&&Math.Abs(ImGui.GetIO().MouseWheel)>0)ImGui.SetScrollX(ImGui.GetScrollX()-ImGui.GetIO().MouseWheel*(cardWidth+18));
        var mouse=ImGui.GetIO().MousePos;
        if(ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem)&&mouse.Y<HomeModeCarouselBounds.Max.Y-18&&ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {homeModeDrag=(mouse.X,ImGui.GetScrollX());homeModeDragged=false;}
        if(homeModeDrag is {} drag&&ImGui.IsMouseDown(ImGuiMouseButton.Left)&&Math.Abs(mouse.X-drag.X)>6)
        {homeModeDragged=true;ImGui.SetScrollX(drag.Scroll+drag.X-mouse.X);}
        var draw=ImGui.GetWindowDrawList();
        for(int index=0;index<ids.Length;index++)
        {
            if(index>0)ImGui.SameLine(0,18);
            string id=ids[index];var set=HomeSets.Value[id];var cardAt=ImGui.GetCursorScreenPos();
            bool chosen=HomeButton("homeMode/"+id,"",cardWidth,cardHeight,selectedSet==id);bool hover=ImGui.IsItemHovered();
            draw.AddRectFilled(cardAt,cardAt+new float2(cardWidth,cardHeight),new ImColor8(7,19,32),5);
            float textHeight=cardHeight<350?132:166,artHeight=cardHeight-textHeight,artWidth=artHeight*2/3;
            homeArtwork.Draw("mode-"+id,cardAt+new float2((cardWidth-artWidth)*.5f,0),new(artWidth,artHeight));
            var textAt=cardAt+new float2(0,artHeight);
            draw.AddRectFilledMultiColor(textAt,cardAt+new float2(cardWidth,cardHeight),new ImColor8(16,36,56),new ImColor8(7,19,32),new ImColor8(7,19,32),new ImColor8(7,19,32));
            draw.AddRect(cardAt,cardAt+new float2(cardWidth,cardHeight),selectedSet==id?HomeInk:hover?HomeGold:HomeLine,5,ImDrawFlags.None,selectedSet==id?2:1);
            string mode=SetChoices.Options.FirstOrDefault(option=>option.Id==id)?.Mode switch{"tft"=>"CLASSIC","underlords"=>"UNDERMONS","autochess"=>"AUTO CHESS",_=>"ANY SET"};
            draw.PushClipRect(textAt+new float2(12,8),cardAt+new float2(cardWidth-12,cardHeight-8),true);
            ImGui.SetCursorScreenPos(textAt+new float2(18,12));HomeLabel(mode,15,new ImColor8(255,174,76));
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,26);float titleHeight=ImGui.CalcTextSize(set.Name,false,cardWidth-36).Y;
            ImGui.PushStyleColor(ImGuiCol.Text,HomeInk);ImGui.SetCursorScreenPos(textAt+new float2(18,35));ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+cardWidth-36);ImGui.Text(set.Name);ImGui.PopTextWrapPos();ImGui.PopStyleColor();ImGui.PopFont();
            ImGui.SetCursorScreenPos(textAt+new float2(18,41+titleHeight));ImGui.PushFont(NativeUiTheme.BodyFont,16);ImGui.PushStyleColor(ImGuiCol.Text,new ImColor8(177,195,221));ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+cardWidth-36);ImGui.Text(set.Description);ImGui.PopTextWrapPos();ImGui.PopStyleColor();ImGui.PopFont();draw.PopClipRect();
            if(selectedSet==id)
            {
                var check=cardAt+new float2(cardWidth-26,25);draw.AddCircleFilled(check,14,new ImColor8(7,19,32,235));draw.AddCircle(check,14,HomeInk,0,2);draw.AddLine(check+new float2(-7,0),check+new float2(-2,5),HomeInk,2);draw.AddLine(check+new float2(-2,5),check+new float2(8,-6),HomeInk,2);
            }
            ImGui.SetCursorScreenPos(cardAt);ImGui.Dummy(new(cardWidth,cardHeight));
            if(selectedSet==id&&(homeModeShownSet!=selectedSet||homeModeScrollRequest==index))ImGui.SetScrollHereX(.5f);
            if(chosen&&!homeModeDragged)SelectHomeHeroSet(id);HomeModeCardCount++;
        }
        if(!ImGui.IsMouseDown(ImGuiMouseButton.Left)){homeModeDrag=null;homeModeDragged=false;}
        homeModeShownSet=selectedSet;homeModeScrollRequest=-1;ImGui.EndChild();draw=ImGui.GetWindowDrawList();
        ImGui.SetCursorScreenPos(at+new float2(0,footer));
        float setWidth=width*.42f,paceWidth=width*.25f,formatWidth=width-setWidth-paceWidth-20;
        ImGui.BeginGroup();HomeLabel("SET",15);ImGui.SetNextItemWidth(setWidth);DrawSetChoice("homePlayRuleset",selectedSet,SelectHomeHeroSet);ImGui.EndGroup();ImGui.SameLine(0,10);
        ImGui.BeginGroup();HomeLabel("PACE",15);ImGui.SetNextItemWidth(paceWidth);DrawPaceChoice("homePlayPace",selectedPace,false,pace=>SelectPace(pace));ImGui.EndGroup();ImGui.SameLine(0,10);
        ImGui.BeginGroup();HomeLabel("FORMAT",15);ImGui.SetNextItemWidth(formatWidth);DrawFormatChoice("homePlayFormat",duos,SelectFormat);ImGui.EndGroup();
        draw.AddLine(at+new float2(0,height-66),at+new float2(width,height-66),HomeLine);
        var modeAt=at+new float2(0,height-54);float modeWidth=Math.Min(270,(width-244)/2);
        DrawPlayMode("homeModeSolo","SOLO VS AI","You and seven AI Tamers","mode-solo",modeAt,new(modeWidth,52),!homePlayOnline,()=>homePlayOnline=false);
        DrawPlayMode("homeModeOnline","MULTIPLAYER","Create or join a lobby","mode-multiplayer",modeAt+new float2(modeWidth+10,0),new(modeWidth,52),homePlayOnline,()=>homePlayOnline=true);
        ImGui.SetCursorScreenPos(at+new float2(width-220,height-54));
        if(HomeButton("homePlayConfirm","CONFIRM",220,52,true)){if(homePlayOnline)OpenMultiplayerView();else homePlayParty=true;}
        ImGui.End();
    }
    private void DrawPlayMode(string key,string title,string description,string art,float2 at,float2 extent,bool selected,Action choose)
    {
        ImGui.SetCursorScreenPos(at);bool clicked=HomeButton(key,"",extent.X,extent.Y,selected);var draw=ImGui.GetWindowDrawList();
        homeArtwork.Draw(art,at+new float2(6,6),new(40,40),255,true);draw.AddCircle(at+new float2(26,26),21,selected?HomeInk:HomeLine);
        draw.PushClipRect(at+new float2(56,5),at+extent-new float2(6,5),true);ImGui.SetCursorScreenPos(at+new float2(56,7));HomeLabel(title,16,HomeInk);
        ImGui.SetCursorScreenPos(at+new float2(56,29));HomeLabel(description,15,HomeMute);draw.PopClipRect();if(clicked)choose();
    }
    private void HomeSoloParty(float2 at,float2 extent)
    {
        var set=HomeSets.Value[selectedSet];float width=extent.X,height=extent.Y;
        var draw=ImGui.GetWindowDrawList();homeArtwork.Draw("lobby-bg",at,extent,255,true,.4f);
        draw.AddRectFilledMultiColor(at,at+extent,new ImColor8(1,10,19,140),new ImColor8(1,10,19,140),new ImColor8(1,10,19,238),new ImColor8(1,10,19,238));draw.AddRect(at,at+extent,HomeLine);
        ImGui.SetCursorScreenPos(at+new float2(24,20));if(ClientTextButton("homePlayModes","< Modes",90,36,false,15))homePlayParty=false;
        ImGui.SetCursorScreenPos(at+new float2(126,18));HomeLabel("SOLO VS AI · PARTY",15);
        ImGui.SetCursorScreenPos(at+new float2(126,38));HomeLabel(set.Name.ToUpperInvariant(),30,HomeInk);
        ImGui.SetCursorScreenPos(at+new float2(126,68));ImGui.PushFont(NativeUiTheme.BodyFont,16);ImGui.PushStyleColor(ImGuiCol.Text,HomeMute);ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+Math.Min(800,width-150));ImGui.Text(set.Description);ImGui.PopTextWrapPos();ImGui.PopStyleColor();ImGui.PopFont();
        ImGui.SetCursorScreenPos(at+new float2(126,108));HomeLabel("PACE",15);ImGui.SameLine(0,10);
        if(HomeButton("homePartyPace/hyper","HYPER ROLL",125,34,selectedPace=="hyper"))SelectPace("hyper");ImGui.SameLine(0,6);
        if(HomeButton("homePartyPace/normal","NORMAL",100,34,selectedPace=="normal"))SelectPace("normal");
        ImGui.SetCursorScreenPos(at+new float2(126,150));HomeLabel(selectedPace=="hyper"?"20 HP, levels by stage, fixed income, 1-gold rerolls. About 25 rounds.":"100 HP, buy XP, interest and streaks, carousels each stage. About 35–40 rounds.",15,HomeMute);
        ImGui.SetCursorScreenPos(at+new float2(126,176));HomeLabel("FORMAT",15);ImGui.SameLine(0,10);
        if(HomeButton("homePartyFormat/solo","FREE FOR ALL",142,34,!duos))SelectFormat("solo");ImGui.SameLine(0,6);
        if(HomeButton("homePartyFormat/duos","DUOS · 2V2",125,34,duos))SelectFormat("duos");
        ImGui.SetCursorScreenPos(at+new float2(126,218));HomeLabel(duos?"Four teams of two. Share health and combine your traits in one team battle.":"Eight Tamers. Every board for itself.",15,HomeMute);
        const float contentLeft=24;float contentWidth=width-48,slotsTop=250;
        bool twoRows=socialWindowSize.X<=1100,showHelp=height>=700;float helpHeight=showHelp?56:0,slotsHeight=height-slotsTop-(showHelp?helpHeight+14:0)-102;
        var slotsAt=at+new float2(contentLeft,slotsTop);float gap=8;
        float baseWidth=twoRows?(contentWidth-gap*3)/4:(contentWidth-gap*7)/8.6f;
        float slotHeight=twoRows?(slotsHeight-gap)/2:slotsHeight;
        for(int seat=0;seat<8;seat++)
        {
            float cardWidth=twoRows?baseWidth:seat==0?baseWidth*1.6f:baseWidth;
            float offset=twoRows?(seat%4)*(baseWidth+gap):seat==0?0:baseWidth*1.6f+gap+(seat-1)*(baseWidth+gap);
            var cardAt=slotsAt+new float2(offset,twoRows?(seat/4)*(slotHeight+gap):0);
            DrawSoloPartySeat(seat,cardAt,new(cardWidth,slotHeight));
        }
        float helpTop=slotsTop+slotsHeight+14;string[] help=["Buy Digimon from the shop and drag them onto your half of the board. Your level sets your team size.","Three copies digivolve. Three 1-stars make a 2-star, and three 2-stars make a 3-star.","Drag items onto your Digimon. Two components make a completed item."];
        float helpWidth=(contentWidth-16)/3;
        if(showHelp)for(int i=0;i<help.Length;i++)
        {
            var helpAt=at+new float2(contentLeft+i*(helpWidth+8),helpTop);draw.AddRectFilled(helpAt,helpAt+new float2(helpWidth,helpHeight),new ImColor8(1,10,19,200));draw.AddRect(helpAt,helpAt+new float2(helpWidth,helpHeight),HomeLine);draw.AddRectFilled(helpAt,helpAt+new float2(2,helpHeight),HomeGold);
            draw.PushClipRect(helpAt+new float2(8,6),helpAt+new float2(helpWidth-8,helpHeight-6),true);ImGui.SetCursorScreenPos(helpAt+new float2(10,8));ImGui.PushFont(NativeUiTheme.BodyFont,15);ImGui.PushStyleColor(ImGuiCol.Text,HomeMute);ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+helpWidth-20);ImGui.Text(help[i]);ImGui.PopTextWrapPos();ImGui.PopStyleColor();ImGui.PopFont();draw.PopClipRect();
        }
        var footerAt=at+new float2(0,height-86);draw.AddRectFilled(footerAt,at+extent,new ImColor8(1,10,19,235));draw.AddLine(footerAt,footerAt+new float2(width,0),HomeLine);
        ImGui.SetCursorScreenPos(footerAt+new float2(24,26));if(ClientTextButton("homePlayBack","Home",64,32,false,15))NavigateClient("home");
        ImGui.SetCursorScreenPos(footerAt+new float2(100,33));HomeLabel("D  reroll    E  sell    Space  fight    T  emotes    C  codex",15,HomeMute);
        ImGui.SetCursorScreenPos(footerAt+new float2(width-244,12));bool inRoom=networkRoom is not null||steam?.Room is not null;ImGui.BeginDisabled(inRoom);
        if(HomeButton("homePlayStart","START",220,60,true)){showHomePlay=false;StartLocalMatch(duos);}
        ImGui.EndDisabled();HomePlayLayoutFits&=slotHeight>=100&&slotsTop+slotsHeight+(showHelp?helpHeight+14:0)<=height-86;
    }
    private void DrawSoloPartySeat(int seat,float2 at,float2 extent)
    {
        bool yours=seat==0;string name=yours?profile?.Progress.Name??"Tamer":LobbyAiNames[seat-1];var draw=ImGui.GetWindowDrawList();
        draw.AddRectFilledMultiColor(at,at+extent,new ImColor8(10,20,40,145),new ImColor8(10,20,40,145),new ImColor8(1,10,19,238),new ImColor8(1,10,19,238));draw.AddRect(at,at+extent,yours?HomeGold:HomeLine,0,ImDrawFlags.None,yours?2:1);
        const float plateHeight=54;float stageHeight=extent.Y-plateHeight;
        string? wallet=profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"),legends=profile?.Storage.GetValueOrDefault("dt.legendevo.v1");
        var look=yours?CosmeticLoadouts.Local(wallet,legends):CosmeticLoadouts.Ai(seat,name);string? key=CosmeticLoadouts.CompanionKey(look);
        string? ownForm=null;int ownLevel=1;
        if(yours)
        {
            string id=look["legend"]?.GetValue<string>()??"tokomon";
            var evolution=LegendEvolution.Inspect(legends,wallet,profile?.Storage.GetValueOrDefault("dt.farm.v1"),id);
            ownForm=LegendEvolution.Form(evolution.Form).Name;ownLevel=evolution.Level;
            // Frame the whole pedestal for this card instead of cropping a wide preview.
            homeLegend??=new(legendPedestal);homeLegend.Draw("legend",id,LegendEvolution.VisualKey(id,evolution.Form),boardSceneTime,ClientMotion,stageAspect:extent.X/stageHeight);
            draw.PushClipRect(at,at+new float2(extent.X,stageHeight),true);draw.AddImage(new ImTextureRef{_TexID=new((nint)homeLegend.Texture)},at,at+new float2(extent.X,stageHeight),new(0,1),new(1,0));draw.PopClipRect();
            var crown=at+new float2(9,9);float2[] points=[crown+new float2(0,13),crown+new float2(0,3),crown+new float2(5,8),crown+new float2(10,0),crown+new float2(15,8),crown+new float2(20,3),crown+new float2(20,13)];foreach(var point in points)draw.PathLineTo(point);draw.PathFillConcave(HomeGold);
        }
        else
        {
            float portrait=Math.Max(32,Math.Min(extent.X-12,stageHeight-12));var portraitAt=at+new float2((extent.X-portrait)/2,stageHeight-portrait-6);portraits.DrawCompanion(key,portraitAt,portrait);
            draw.AddEllipseFilled(at+new float2(extent.X/2,stageHeight-4),new float2(extent.X*.32f,4),new ImColor8(63,214,255,60));
        }
        var plateAt=at+new float2(0,stageHeight);draw.AddRectFilled(plateAt,at+extent,new ImColor8(1,10,19,228));draw.AddLine(plateAt,plateAt+new float2(extent.X,0),HomeLine);
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,16);string label=LobbyEllipsis(name.ToUpperInvariant(),extent.X-12);var measured=ImGui.CalcTextSize(label);draw.AddText(plateAt+new float2((extent.X-measured.X)/2,7),yours?new ImColor8(255,255,255):HomeInk,label);ImGui.PopFont();
        ImGui.PushFont(NativeUiTheme.BodyFont,15);
        string sub=duos&&seat==1?"AI Partner":"AI Tamer";
        if(ownForm is not null)
        {
            string level=" · Lv "+ownLevel;
            sub=LobbyEllipsis(ownForm,extent.X-12-ImGui.CalcTextSize(level).X)+level;
        }
        measured=ImGui.CalcTextSize(sub);draw.AddText(plateAt+new float2((extent.X-measured.X)/2,30),HomeMute,sub);ImGui.PopFont();
        Buttons["homePartySeat/"+seat]=at+extent*.5f;HomePartySeatCount++;
    }
}

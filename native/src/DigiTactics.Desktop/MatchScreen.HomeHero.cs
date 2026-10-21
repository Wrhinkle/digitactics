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
    private bool showHomePlay,homeHeroWasVisible,homeHeroDrawnThisFrame,homePlayParty,homePlayOnline;
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
        homeCarousel.Tick(HomeHeroClock(),HomeHeroPage,ClientHomeVisible,HomeHeroModule,PostgameVisible,homeTab=="overview",hover);
    }
    private void SelectHomeHero(int index)
    =>homeCarousel!.Render(HomeHeroClock(),index);
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
                // renderHero builds the slides and marks the first one .on in the same frame, so the
                // first painting appears at full opacity and scale(1) without a transition.
                var value=homeHeroArtStates.TryGetValue(i,out var old)?HeroArtValue(old,now):homeHeroVisualIndex<0&&i==active?(Opacity:1d,Scale:1d):(Opacity:0d,Scale:1.06);
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
        ClientLayer("Home featured set",at,extent);var draw=ImGui.GetWindowDrawList();
        var hi=at+extent;
        // .hero: #010a13 under the cover art (center 40%), then .hero-shade's two gradients and the inset vignette.
        draw.AddRectFilled(at,hi,NativeUiPaint.Fade(NativeUiTheme.Navy0));
        DrawHomeHeroArt(at,extent,now);
        float x34=at.X+extent.X*.34f,x62=at.X+extent.X*.62f;
        NativeUiPaint.HorizontalGradient(draw,at,new(x34,hi.Y),new ImColor8(1,10,19,235),new ImColor8(1,10,19,153));
        NativeUiPaint.HorizontalGradient(draw,new(x34,at.Y),new(x62,hi.Y),new ImColor8(1,10,19,153),new ImColor8(1,10,19,0));
        NativeUiPaint.VerticalGradient(draw,new(at.X,hi.Y-extent.Y*.38f),hi,new ImColor8(1,10,19,0),new ImColor8(1,10,19,217));
        draw.PushClipRect(at,hi,true);NativeUiPaint.InsetGlow(draw,at,hi,new ImColor8(1,10,19,153),60);draw.PopClipRect();
        draw.AddRect(at+new float2(1,1),hi-new float2(1,1),NativeUiPaint.Fade(new ImColor8(1,10,19,204)));
        draw.AddRect(at,hi,NativeUiPaint.Fade(NativeUiTheme.GoldBorder));
        string title=slide.Title,description=slide.Desc,meta=slide.Meta??"";string? offerKind=null,offerId=null;
        (string Rarity,string Kind,int Price,int Full,bool Owned)? offerMeta=null;
        if(slide.Feat&&homeOffers.Length>0)
        {
            int pick=(int)((HomeClock().ToUnixTimeMilliseconds()/86400000)%homeOffers.Length);var offer=homeOffers[pick];
            var collection=new CosmeticCollection(profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            offerKind=offer.Kind;offerId=offer.Item.Id;title=offer.Item.Name;
            string rarity=offer.Item.Rarity is "uncommon" or "rare" or "epic" or "legendary"?offer.Item.Rarity:"common";
            string kindName=HomeKindOne(offer.Kind,offer.Kind);
            description=!string.IsNullOrEmpty(offer.Item.Desc)?offer.Item.Desc:kindName+", "+rarity+".";
            offerMeta=(rarity,kindName,offer.Price,offer.Item.Price,collection.Owns(offer.Kind,offer.Item.Id));meta="";
            // .hero-prev: right 4%, top 6%, bottom 74px, 44% wide; a square up to 360px (16:10 for boards and kill screens).
            float boxWidth=extent.X*.44f,boxHeight=extent.Y*.94f-74-extent.Y*.06f;
            bool wide=offer.Kind is "board" or "killscreen";
            float previewWidth=wide?boxWidth:Math.Min(Math.Min(boxWidth,360),boxHeight),previewHeight=wide?Math.Min(boxHeight,boxWidth*10/16):previewWidth;
            if(wide&&previewHeight<boxWidth*10/16)previewWidth=previewHeight*16/10;
            var boxCenter=new float2(hi.X-extent.X*.04f-boxWidth/2,at.Y+extent.Y*.06f+boxHeight/2);
            HomeOfferArt(offer.Kind,offer.Item,collection,boxCenter-new float2(previewWidth,previewHeight)*.5f,new(previewWidth,previewHeight));
        }
        bool narrow=socialWindowSize.X<=1500,shortWindow=socialWindowSize.Y<=820;
        float padX=narrow?24:32,textWidth=Math.Min(extent.X*(narrow?.56f:.52f),560)-padX*2;
        float titlePx=Math.Clamp(socialWindowSize.X*.038f,34,64),titleLine=titlePx*.95f;
        var titleLines=WrapText(NativeUiTheme.DisplayBoldFont,titlePx,title.ToUpperInvariant(),textWidth,int.MaxValue,.03f);
        float descriptionWidth=Math.Min(textWidth,NativeUiPaint.TrackedWidth(NativeUiTheme.BodyFont,14,"0",0)*52);
        var descriptionLines=description.Length>0?WrapText(NativeUiTheme.BodyFont,14,description,descriptionWidth,shortWindow?2:3):[];
        float eyebrowHeight=NativeUiTheme.TextLine(11),metaHeight=meta.Length>0||offerMeta is not null?Math.Max(NativeUiTheme.TextLine(12.5f),offerMeta is null?0:19):0;
        float ctaHeight=MathF.Round(14*1.2f+20+4),ctaWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayBoldFont,14,slide.Cta.ToUpperInvariant(),.12f)+52+4;
        // .hero-txt is a bottom-anchored column (70px above the hero's bottom; 62px when short) with 8px gaps.
        float blockHeight=eyebrowHeight+8+titleLines.Count*titleLine+(metaHeight>0?8+metaHeight:0)+(descriptionLines.Count>0?8+descriptionLines.Count*14*1.45f:0)+8+6+ctaHeight;
        // .hero-txt and .hero-thumbs are positioned inside the hero's 1px border.
        float y=hi.Y-1-(shortWindow?62:70)-blockHeight,left=at.X+1+padX;
        float Rise(int child,out float alpha){alpha=(float)HomeCarousel.TextProgress(now-homeCarousel.RenderedAt,child,ClientMotion);return 14*(1-alpha);}
        int index=0;float lift,alpha;
        lift=Rise(index++,out alpha);ImGui.PushStyleVar(ImGuiStyleVar.Alpha,alpha);
        Eyebrow(draw,new(left,y+lift),slide.Eyebrow);ImGui.PopStyleVar();y+=eyebrowHeight+8;
        lift=Rise(index++,out alpha);ImGui.PushStyleVar(ImGuiStyleVar.Alpha,alpha);
        foreach(var line in titleLines)
        {
            // line-height .95 and text-shadow 0 4px 24px rgba(0,0,0,.7).
            var lineAt=new float2(left,MathF.Round(y+lift+(titleLine-NativeUiTheme.LineHeight(NativeUiTheme.DisplayBoldFont,titlePx))/2));
            var lineSize=NativeUiPaint.TrackedSize(NativeUiTheme.DisplayBoldFont,titlePx,line,.03f);
            NativeUiPaint.Radial(draw,lineAt+lineSize*.5f+new float2(0,4),new(lineSize.X*.6f+24,lineSize.Y*.6f+12),new ImColor8(0,0,0,70),new ImColor8(0,0,0,0),32);
            NativeUiPaint.Tracked(draw,NativeUiTheme.DisplayBoldFont,titlePx,lineAt,NativeUiTheme.GoldBright,line,.03f);
            y+=titleLine;
        }
        ImGui.PopStyleVar();
        if(metaHeight>0)
        {
            y+=8;lift=Rise(index++,out alpha);ImGui.PushStyleVar(ImGuiStyleVar.Alpha,alpha);
            if(offerMeta is {} m)HomeHeroOfferMeta(draw,new(left,y+lift),metaHeight,m);
            else NativeUiPaint.TrackedIn(draw,NativeUiTheme.BodySemiBoldFont,12.5f,new(left,y+lift),metaHeight,NativeUiTheme.LcText,meta);
            ImGui.PopStyleVar();y+=metaHeight;
        }
        if(descriptionLines.Count>0)
        {
            y+=8;lift=Rise(index++,out alpha);ImGui.PushStyleVar(ImGuiStyleVar.Alpha,alpha);
            y+=Paragraph(draw,NativeUiTheme.BodyFont,14,new(left,y+lift),descriptionWidth,NativeUiTheme.Muted,string.Join("\n",descriptionLines),1.45f,int.MaxValue,0,new ImColor8(0,0,0,170));
            ImGui.PopStyleVar();
        }
        y+=8+6;lift=Rise(index,out alpha);
        ImGui.SetCursorScreenPos(new(left,MathF.Round(y+lift)));ImGui.PushStyleVar(ImGuiStyleVar.Alpha,alpha);
        if(ClientButton("homeHeroAction",slide.Cta,ctaWidth,ctaHeight,ClientButtonKind.Cta,14))HomeHeroAction(slide,offerKind,offerId);
        ImGui.PopStyleVar();
        // .hero-thumbs: left 32px (24px), right 16px, bottom 14px, 42px tall, 6px gaps, right-aligned.
        DrawHomeHeroThumbnails(new(at.X+1+padX,hi.Y-1-14-42),new(extent.X-2-padX-16,42));
        ImGui.End();
    }
    private void HomeHeroOfferMeta(ImDrawListPtr draw,float2 at,float height,(string Rarity,string Kind,int Price,int Full,bool Owned) offer)
    {
        // <badge rarity> <kind> <price with the full price struck through> <sale>, 8px apart, centred on the row.
        float x=at.X,middle=at.Y+height/2;var rarity=RarityToken(offer.Rarity);
        string rarityText=offer.Rarity.ToUpperInvariant();float rarityWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.BodyExtraBoldFont,10.5f,rarityText,.08f)+18,badgeHeight=10.5f*1.5f+6;
        var badgeLo=new float2(x,MathF.Round(middle-badgeHeight/2));
        draw.AddRectFilled(badgeLo,badgeLo+new float2(rarityWidth,badgeHeight),NativeUiPaint.Fade(Mix(new ImColor8(1,10,19,217),NativeUiPaint.Alpha(rarity,255),.14f)));
        draw.AddRect(badgeLo,badgeLo+new float2(rarityWidth,badgeHeight),NativeUiPaint.Fade(NativeUiPaint.Alpha(rarity,140)));
        NativeUiPaint.Tracked(draw,NativeUiTheme.BodyExtraBoldFont,10.5f,badgeLo+new float2(9,MathF.Round((badgeHeight-NativeUiTheme.LineHeight(NativeUiTheme.BodyExtraBoldFont,10.5f))/2)),rarity,rarityText,.08f);
        x+=rarityWidth+8;
        float kindHeight=NativeUiTheme.LineHeight(NativeUiTheme.BodySemiBoldFont,12.5f);
        NativeUiPaint.Tracked(draw,NativeUiTheme.BodySemiBoldFont,12.5f,new(x,MathF.Round(middle-kindHeight/2)),NativeUiTheme.LcText,offer.Kind);
        x+=NativeUiPaint.TrackedWidth(NativeUiTheme.BodySemiBoldFont,12.5f,offer.Kind,0)+8;
        if(offer.Owned){Badge(draw,new(x,MathF.Round(middle-badgeHeight/2)),"Owned",BadgeStyle.Owned);return;}
        if(offer.Price<offer.Full)
        {
            string full=offer.Full.ToString();float fullHeight=NativeUiTheme.LineHeight(NativeUiTheme.BodyFont,12.5f),fullWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.BodyFont,12.5f,full,0);
            NativeUiPaint.Tracked(draw,NativeUiTheme.BodyFont,12.5f,new(x,MathF.Round(middle-fullHeight/2)),NativeUiTheme.Text3,full);
            draw.AddLine(new(x,middle+.5f),new(x+fullWidth,middle+.5f),NativeUiPaint.Fade(NativeUiTheme.Text3));x+=fullWidth+6;
        }
        float priceHeight=NativeUiTheme.LineHeight(NativeUiTheme.BodyExtraBoldFont,16);
        float priceWidth=BitsAmount(draw,new(0,0),offer.Price,16,true);
        BitsAmount(draw,new(x+priceWidth,MathF.Round(middle-priceHeight/2)),offer.Price,16);x+=priceWidth;
        string sale=$"-{Math.Round(100-100d*offer.Price/Math.Max(1,offer.Full),MidpointRounding.AwayFromZero)}%";
        float saleWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayBoldFont,10.5f,sale,.06f)+14,saleHeight=NativeUiTheme.LineHeight(NativeUiTheme.DisplayBoldFont,10.5f)+4;
        var saleLo=new float2(x+8,MathF.Round(middle-saleHeight/2));
        NativeUiPaint.VerticalGradient(draw,saleLo,saleLo+new float2(saleWidth,saleHeight),new ImColor8(232,67,63),new ImColor8(165,34,31));
        draw.AddRect(saleLo,saleLo+new float2(saleWidth,saleHeight),NativeUiPaint.Fade(new ImColor8(255,138,122)));
        NativeUiPaint.Tracked(draw,NativeUiTheme.DisplayBoldFont,10.5f,saleLo+new float2(7,2),new ImColor8(255,255,255),sale,.06f);
    }
    private void DrawHomeHeroThumbnails(float2 at,float2 extent)
    {
        var slides=homeHeroSlides!;int count=slides.Length;
        float basis=socialWindowSize.X<=1500?96:112,gaps=(count-1)*6;
        // flex: 0 1 basis with min-width 0 and justify-content: flex-end.
        float width=Math.Min(basis,(extent.X-gaps)/count),total=width*count+gaps;
        var start=new float2(at.X+extent.X-total,at.Y);
        HomeHeroThumbBounds=(start,start+new float2(total,extent.Y));
        var draw=ImGui.GetWindowDrawList();
        for(int i=0;i<count;i++)
        {
            var slide=slides[i];var position=start+new float2(i*(width+6),0);var hi=position+new float2(width,42);
            ImGui.SetCursorScreenPos(position);
            bool clicked=ClientHitButton("homeHeroThumb/"+slide.Id,width,42);
            bool on=homeCarousel!.Index==i,hot=ImGui.IsItemHovered();
            if(on)NativeUiPaint.BoxGlow(draw,position,hi,new ImColor8(240,210,140,77),12);
            draw.PushClipRect(position,hi,true);
            // filter: saturate(.6) brightness(.8) on idle thumbnails, including their label.
            float dim=on||hot?1:.8f;byte brightness=(byte)(255*dim);
            homeArtwork.Draw(slide.Art,position,new(width,42),255,true,.5f,draw,false,brightness);
            if(!on&&!hot)homeArtwork.Draw(slide.Art,position,new(width,42),102,true,.5f,draw,true,brightness);
            NativeUiPaint.VerticalGradient(draw,position,hi,new ImColor8(1,10,19,51),new ImColor8(1,10,19,230));
            string label=SocialEllipsis(slide.Thumb.ToUpperInvariant(),NativeUiTheme.DisplayBoldFont,10.5f,width-16,.08f);
            float labelHeight=NativeUiTheme.LineHeight(NativeUiTheme.DisplayBoldFont,10.5f);
            var ink=Mix(new ImColor8(0,0,0),NativeUiTheme.GoldBright,dim);
            NativeUiPaint.Tracked(draw,NativeUiTheme.DisplayBoldFont,10.5f,new(position.X+8,MathF.Round(hi.Y-6-labelHeight)),ink,label,.08f);
            if(on)
            {
                var barHi=new float2(position.X+width*(float)homeCarousel.Progress(ClientMotion),hi.Y);
                if(barHi.X>position.X){NativeUiPaint.BoxGlow(draw,new(position.X,hi.Y-2),barHi,new ImColor8(63,214,255,160),6);draw.AddRectFilled(new(position.X,hi.Y-2),barHi,NativeUiPaint.Fade(NativeUiTheme.Accent));}
            }
            draw.PopClipRect();
            draw.AddRect(position,hi,NativeUiPaint.Fade(on?NativeUiTheme.GoldBright:hot?NativeUiTheme.GoldDeep:NativeUiTheme.GoldDim));
            if(hot&&label!=slide.Thumb.ToUpperInvariant())ResultTooltip(slide.Thumb);
            if(clicked)SelectHomeHero(i);
        }
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

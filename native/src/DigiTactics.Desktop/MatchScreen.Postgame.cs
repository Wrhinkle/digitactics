using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private readonly PostgameArtwork postgameArtwork = new();
    private static readonly Dictionary<string,Dictionary<string,string>> resultTraitColors=System.Text.Json.JsonSerializer.Deserialize<Dictionary<string,Dictionary<string,string>>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"content/postgame/trait-colors.json")))!;
    private MatchPlanningSession? postgameSession;
    private MatchPlanningSession? dismissedHomeSession;
    private bool postgameDismissed;
    private bool postgameStore;
    private RewardBreakdown? postgameAnimatedAward;
    private double postgameAwardStart;
    private int postgameAwardSounds;
    internal bool PostgameVisible { get; private set; }
    internal int PostgameStandings { get; private set; }
    internal int PostgameVisibleStandings { get; private set; }
    internal int PostgameUnitPortraits { get; private set; }
    internal int PostgameMissionCount { get; private set; }
    internal int PostgameAvatarCount { get; private set; }
    internal long PostgameXpGain => history?.XpGained ?? 0;
    internal int? PostgameBits => history?.Award?.Total;
    internal int? PostgameDisplayedBits { get; private set; }
    internal bool PostgameStoreVisible => postgameStore;
    internal bool PostgameRewardsBelow { get; private set; }
    internal bool PostgameStoreButtonVisible { get; private set; }
    internal float2 PostgameRewardsMin { get; private set; }
    internal float2 PostgameRewardsMax { get; private set; }
    internal float2 PostgameStandingsMin { get; private set; }
    internal float2 PostgameStandingsMax { get; private set; }
    internal float2 PostgameProgressMin { get; private set; }
    internal float2 PostgameProgressMax { get; private set; }
    internal float2 PostgamePanelMin { get; private set; }
    internal float2 PostgamePanelMax { get; private set; }
    internal bool PostgameHistoryVisible => ProfileVisible && Session is not null;
    internal bool PostgameHomeVisible => Session is not null && ReferenceEquals(dismissedHomeSession,Session);
    private bool PostgameStillPlaying => Session is not null && (Session.State.Duos
        ? Session.State.Teams.Count(t => t.Alive) : Session.State.Players.Count(p => p.Alive)) > 1;
    internal bool PostgamePending => Session is not null && !PostgameHomeVisible
        && (Session.State.Over || !IsSpectating && Human.Placement > 0)
        && (!ReferenceEquals(postgameSession,Session) || !postgameDismissed || !PostgameStillPlaying);
    // The result belongs to Home, not to a replacement fullscreen match view.
    // Visiting another client page must retain it until an explicit result action.
    private bool PostgameClientOverlay => PostgamePending;
    private bool ClientHomeVisible => Session is null || PostgameHomeVisible || PostgameClientOverlay;
    private void PreparePostgameClient()
    {
        PostgameVisible=false;
        if(postgameStore && !showCollection)postgameStore=false;
        if(!PostgamePending || ReferenceEquals(postgameSession,Session))return;
        postgameSession=Session;dismissedHomeSession=null;
        postgameDismissed=postgameStore=false;postgameAnimatedAward=null;
        clientHistory.Clear();
        showMultiplayer=showProfile=showCollection=showFarm=showAbout=showSettings=showSaveData=showHomePlay=false;
        showGameMenu=showMusicLibrary=showAudio=showPlayers=showEmotes=showSoundboard=false;
        codexFull=codexSide=false;codexUnit=codexModalLine=null;
        CancelArenaDrag();cameraDrag=null;
    }
    private void DismissResults()
    {
        // The original result X/Home actions hide the result in the client shell.
        // They do not invoke the separate leave-match callback or drop the room.
        dismissedHomeSession=Session;postgameDismissed=true;PostgameVisible=false;
        postgameStore=showMultiplayer=showProfile=showCollection=showFarm=showAbout=false;
        CancelArenaDrag();cameraDrag=null;
    }

    private float postgameScroll;
    internal (float2 Min,float2 Max) PostgamePageBounds {get;private set;}
    private readonly Dictionary<string,(float2 Min,float2 Max)> postgameControls=[];
    /// <summary>
    /// For drivers: scrolls the result page so the control is visible and returns where it will be drawn
    /// on the next frame (the page moves by exactly the scroll change).
    /// </summary>
    internal float2 RevealPostgameControl(string key,float2 point)
    {
        if(!PostgameVisible||!postgameControls.TryGetValue(key,out var box))return point;
        var page=PostgamePageBounds;float delta=0;
        if(box.Min.Y<page.Min.Y)delta=box.Min.Y-page.Min.Y-8;else if(box.Max.Y>page.Max.Y)delta=box.Max.Y-page.Max.Y+8;
        postgameScroll+=delta;return point-new float2(0,delta);
    }

    // #homeResults .results (js/client.js showResults, theme.js "Results"), in CSS pixels. Like the original
    // Home page, the panel fills the window and the page scrolls when its content is taller.
    private bool DrawPostgame()
    {
        PostgameVisible = false;
        if (!PostgamePending || Session is null || showProfile || showCollection || showFarm || showAbout
            || showSettings || showSaveData || showHomePlay || showMultiplayer || codexFull) return false;
        PostgameVisible = true; PostgameStandings = PostgameVisibleStandings = PostgameUnitPortraits = PostgameMissionCount = PostgameAvatarCount = 0;
        postgameControls.Clear();
        bool outNow = !IsSpectating && !Human.Alive && PostgameStillPlaying;
        int place = Human.Placement;
        bool gold = !IsSpectating && place == 1;
        bool top = !IsSpectating && place <= (Session.State.Duos ? 2 : 4);
        string title = IsSpectating ? "Match complete" : outNow ? "Eliminated" : gold ? "Victory" : top ? Session.State.Duos ? "Top 2 teams" : "Top 4" : "Defeat";
        var accent = gold ? new ImColor8(255,213,106) : top ? new ImColor8(63,214,255) : new ImColor8(167,178,199);
        float viewWidth=socialWindowSize.X,viewHeight=socialWindowSize.Y;bool shortView=viewHeight<=820;
        PostgameRewardsBelow = viewWidth <= 1500;
        PostgameStoreButtonVisible = false;
        float pageWidth=Math.Min(size.X,1720),side=viewWidth<=1500?16:24,padTop=shortView?8:10,padBottom=shortView?10:16;
        float width=pageWidth-side*2,pageHeight=size.Y-ClientPageTop;
        float padX=shortView?16:24,padY=shortView?12:16,gap=shortView?8:12,emblemSize=shortView?96:130,numberSize=shortView?36:46,unitSize=shortView?32:40;
        float titlePx=Math.Clamp(viewWidth*.04f,38,64);
        var record=history?.Match;var validRules=record is null?null:catalog.Variants.FirstOrDefault(r=>r.Id==Rules.Id) ?? catalog.Variants.FirstOrDefault(r=>r.Id.Split('/')[0]==record.Set);
        var winningPlayer = Session.State.Players.FirstOrDefault(p => p.Placement == 1);
        string? winner = winningPlayer is not null && winningPlayer.Id != localSeat ? winningPlayer.Name : null;
        string summary=outNow?"The others are still playing. Spectate their fights or leave.":$"Reached round {Session.Rounds.Position.Label} at level {Human.Economy.Level}."+(winner is null?"":" Winner: "+winner+".");

        // Natural heights decide whether the panel fills the page or the page scrolls (min-height: 100%).
        float heroX=IsSpectating?0:170+24,heroWidth=width-2-padX*2-heroX;
        var traits=!IsSpectating&&record is not null&&validRules is not null?(record.Traits??[]).Take(6).Where(t=>validRules.Traits.Any(x=>x.Id==t.Id)).ToArray():[];
        var units=!IsSpectating&&record is not null&&validRules is not null?(record.Units??[]).Take(9).Where(u=>validRules.Lines.Concat(validRules.ExtraLines).FirstOrDefault(l=>l.Id==u.Line) is {} line&&u.Stage>=0&&u.Stage<line.Forms.Length).ToArray():[];
        float summaryHeight=ClientCss.ParagraphHeight(ClientCss.Face.Body,13,heroWidth,17.55f,summary);
        float bodyHeight=14.8f+6+titlePx+6+summaryHeight+(traits.Length>0?6+20.8f:0)+(units.Length>0?6+unitSize:0);
        float placeHeight=IsSpectating?0:emblemSize-14+numberSize+14.8f;
        float heroHeight=Math.Max(bodyHeight,placeHeight);
        int missions=2;
        try{missions=MatchRewards.Progress(profile?.Storage.GetValueOrDefault("dt.shop.v1"),profile?.Storage.GetValueOrDefault("dt.client.v1"),DateOnly.FromDateTime(DateTime.Now)).Missions.Count();}catch(Exception error) when(error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException or ArgumentOutOfRangeException){}
        float columnChrome=2+20+18.9f+6;
        float standingsNatural=columnChrome+Session.State.Players.Count*32-2;
        float progressNatural=columnChrome+31.5f+10+missions*37.9f+35;
        float awardNatural=history?.Award is {} naturalAward?2+20+16.9f+6+naturalAward.Lines.Length*21.5f+34.6f+8+24.2f:columnChrome+40;
        float columnsNatural=PostgameRewardsBelow?Math.Max(standingsNatural,progressNatural)+gap+awardNatural:Math.Max(Math.Max(standingsNatural,progressNatural),awardNatural);
        float natural=2+padY*2+heroHeight+gap+columnsNatural+gap+40.8f,fill=pageHeight-padTop-padBottom,height=Math.Max(fill,natural);
        var io=ImGui.GetIO();
        PostgamePageBounds=(new(0,ClientPageTop),new(size.X,size.Y));
        bool pageHovered=io.MousePos.X>=0&&io.MousePos.X<size.X&&io.MousePos.Y>=ClientPageTop&&io.MousePos.Y<size.Y;
        postgameScroll=Math.Clamp(postgameScroll,0,Math.Max(0,padTop+height+padBottom-pageHeight));
        float panelTop=ClientPageTop+padTop-postgameScroll;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize,0);
        Window("Match results", 0, ClientPageTop, size.X, pageHeight, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleVar(2);
        if(pageHovered&&io.MouseWheel!=0&&ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))postgameScroll-=io.MouseWheel*100;
        var origin=new float2((size.X-pageWidth)/2+side,panelTop);var draw=ImGui.GetWindowDrawList();var max=origin+new float2(width,height);
        PostgamePanelMin=origin;PostgamePanelMax=max;
        draw.PushClipRect(origin+new float2(1),max-new float2(1),true);
        postgameArtwork.Draw(gold && !outNow ? "postgame-bg" : "loading-bg", origin+new float2(1), new(width-2,height-2), 255, true, .3f);
        float fadeEnd=origin.Y+1+(height-2)*.55f;
        ClientCss.Gradient(draw,origin+new float2(1),new(max.X-1,fadeEnd),new ImColor8(1,10,19,140),new ImColor8(1,10,19,235));
        draw.AddRectFilled(new(origin.X+1,fadeEnd),max-new float2(1),new ImColor8(1,10,19,235));
        draw.PopClipRect();
        ClientCss.Border(draw,origin,max,gold?ClientCss.Gold2:ClientCss.Gold4);

        // .res-x
        var closeAt=new float2(max.X-1-10-34,origin.Y+1+10);
        postgameControls["resultDismiss"]=(closeAt,closeAt+new float2(34,34));
        if(IconButton("resultDismiss","✕",closeAt,34)) { DismissResults(); ImGui.End(); return true; }

        // .res-hero: place emblem, then eyebrow, title, summary, traits and units.
        var content=origin+new float2(1+padX,1+padY);
        if(!IsSpectating)
        {
            var placeAt=content+new float2(0,(heroHeight-placeHeight)/2);
            var emblemAt=placeAt+new float2((170-emblemSize)/2,0);
            ClientCss.Glow(draw,emblemAt+new float2(emblemSize*.3f),emblemAt+new float2(emblemSize*.7f),ClientCss.Alpha(accent,.35f),emblemSize*.35f,emblemSize*.2f);
            postgameArtwork.Draw(place == 1 ? "emblem-mega" : place <= 2 ? "emblem-ultimate" : place <= 4 ? "emblem-champion" : "emblem-rookie",emblemAt,new(emblemSize,emblemSize));
            string number=$"#{place}",label=PostgameSummary.PlaceLabel(place,Session.State.Duos).ToUpperInvariant();
            float numberWidth=ClientCss.Width(ClientCss.Face.DisplayBold,numberSize,number);var numberAt=placeAt+new float2((170-numberWidth)/2,emblemSize-14);
            ClientCss.Text(draw,ClientCss.Face.DisplayBold,numberSize,numberAt+new float2(0,2),new ImColor8(0,0,0),number,0,numberSize);
            ClientCss.Text(draw,ClientCss.Face.DisplayBold,numberSize,numberAt,accent,number,0,numberSize);
            ClientCss.TextCentered(draw,ClientCss.Face.DisplayBold,11,placeAt+new float2(0,emblemSize-14+numberSize),170,ClientCss.Gold2,label,1.76f,14.85f);
        }
        var body=content+new float2(heroX,(heroHeight-bodyHeight)/2);
        ClientCss.Text(draw,ClientCss.Face.DisplayBold,11,body,ClientCss.Gold2,$"{(networkRoom is null ? "Solo vs AI" : "Multiplayer")} · {MatchSetTitle} · results".ToUpperInvariant(),1.76f,14.85f);
        float y=14.8f+6;
        // text-shadow: 0 0 30px accent/40%, 0 3px 0 #000
        string upperTitle=title.ToUpperInvariant();float titleWidth=ClientCss.Width(ClientCss.Face.DisplayBold,titlePx,upperTitle,titlePx*.06f);
        ClientCss.Glow(draw,body+new float2(titleWidth*.1f,y+titlePx*.3f),body+new float2(titleWidth*.9f,y+titlePx*.7f),ClientCss.Alpha(accent,.22f),30,titlePx*.2f);
        ClientCss.Text(draw,ClientCss.Face.DisplayBold,titlePx,body+new float2(0,y+3),new ImColor8(0,0,0),upperTitle,titlePx*.06f,titlePx);
        ClientCss.Text(draw,ClientCss.Face.DisplayBold,titlePx,body+new float2(0,y),ClientCss.Gold1,upperTitle,titlePx*.06f,titlePx);
        y+=titlePx+6;
        ClientCss.Paragraph(draw,ClientCss.Face.Body,13,body+new float2(0,y),heroWidth,17.55f,ClientCss.Muted,summary);y+=summaryHeight;
        if(traits.Length>0)
        {
            y+=6;float x=0;
            foreach(var trait in traits)
            {
                string name=text.Get(validRules!.Id,trait.Id).Name,count=trait.N.ToString();
                float chipWidth=1+4+12+5+ClientCss.Width(ClientCss.Face.BodyBold,11,name)+ClientCss.Width(ClientCss.Face.Body,11," ")+ClientCss.Width(ClientCss.Face.BodyBold,11,count)+8+1;
                var chipAt=body+new float2(x,y);var chipMax=chipAt+new float2(chipWidth,20.8f);
                var tint=ResultTraitTint(resultTraitColors[validRules.Id].GetValueOrDefault(trait.Id,"#9aa4b1"));
                // color-mix(tc 14%, rgba(1,10,19,.8)) with premultiplied channels; border tc at 45%.
                draw.AddRectFilled(chipAt,chipMax,new ImColor8((byte)((tint.R*.14+.688)/.828),(byte)((tint.G*.14+6.88)/.828),(byte)((tint.B*.14+13.072)/.828),211));
                ClientCss.Border(draw,chipAt,chipMax,new ImColor8((byte)tint.R,(byte)tint.G,(byte)tint.B,115));
                var medal=trait.Lv>=3?new ImColor8(255,207,74):trait.Lv==2?new ImColor8(200,210,224):new ImColor8(201,138,82);
                var hex=chipAt+new float2(5+6,10.4f);
                draw.AddQuadFilled(hex+new float2(0,-6.5f),hex+new float2(6,-3.25f),hex+new float2(6,3.25f),hex+new float2(0,6.5f),medal);
                draw.AddQuadFilled(hex+new float2(0,-6.5f),hex+new float2(0,6.5f),hex+new float2(-6,3.25f),hex+new float2(-6,-3.25f),medal);
                float textX=ClientCss.Text(draw,ClientCss.Face.BodyBold,11,chipAt+new float2(5+12+5,3),ClientCss.Text1,name,0,14.85f)+ClientCss.Width(ClientCss.Face.Body,11," ");
                ClientCss.Text(draw,ClientCss.Face.BodyBold,11,chipAt+new float2(5+12+5+textX,3),ClientCss.Text2,count,0,14.85f);
                ImGui.SetCursorScreenPos(chipAt);ImGui.Dummy(new(chipWidth,20.8f));if(ImGui.IsItemHovered())ResultTooltip(name);
                x+=chipWidth+5;
            }
            y+=20.8f;
        }
        if(units.Length>0)
        {
            y+=6;float x=0;
            foreach(var unit in units)
            {
                var line=validRules!.Lines.Concat(validRules.ExtraLines).First(l=>l.Id==unit.Line);
                var position=body+new float2(x,y);var unitMax=position+new float2(unitSize);
                draw.AddRectFilled(position,unitMax,NativeUiTheme.Navy1,6);
                portraits.Draw(validRules.Id,unit.Line,unit.Stage,position+new float2(1),unitSize-2); PostgameUnitPortraits++;
                var tier=line.Tier switch{2=>new ImColor8(63,191,106),3=>new ImColor8(59,140,255),4=>new ImColor8(180,92,255),>=5=>new ImColor8(255,176,46),_=>new ImColor8(154,164,177)};
                ClientCss.Border(draw,position,unitMax,tier,1,6);
                ImGui.SetCursorScreenPos(position);ImGui.Dummy(new(unitSize,unitSize));
                if(ImGui.IsItemHovered())ResultTooltip(line.Forms[unit.Stage]+" "+new string('*',unit.Stage+1));
                x+=unitSize+5;
            }
        }

        // .res-cols: Standings / Progress / Bits earned (three columns, or two with the award below).
        float columnsTop=1+padY+heroHeight+gap,buttonsTop=height-1-padY-40.8f,columnsHeight=Math.Max(120,buttonsTop-gap-columnsTop),contentWidth=width-2-padX*2;
        float topHeight=PostgameRewardsBelow?Math.Max(Math.Max(standingsNatural,progressNatural),columnsHeight-gap-awardNatural):columnsHeight;
        float standingsWidth=PostgameRewardsBelow?(contentWidth-gap)/2:(contentWidth-gap*2)*1.1f/3.1f;
        float progressWidth=PostgameRewardsBelow?standingsWidth:(contentWidth-gap*2)/3.1f;
        var columnsAt=origin+new float2(1+padX,columnsTop);
        (PostgameStandingsMin,PostgameStandingsMax)=ResultColumn("Result standings",columnsAt,new(standingsWidth,topHeight),DrawResultStandings);
        (PostgameProgressMin,PostgameProgressMax)=ResultColumn("Result progress",columnsAt+new float2(standingsWidth+gap,0),new(progressWidth,topHeight),()=>
        {
            ResultHeading("Progress");
            if (profile is null || IsSpectating) ResultNote(IsSpectating ? "Spectating does not award match progress." : "No player profile is attached to this session.");
            else DrawResultProgress();
        });
        float2 rewardsAt=PostgameRewardsBelow?columnsAt+new float2(0,topHeight+gap):columnsAt+new float2(standingsWidth+progressWidth+gap*2,0);
        float2 rewardsSize=PostgameRewardsBelow?new(contentWidth,Math.Max(80,columnsHeight-topHeight-gap)):new(progressWidth,columnsHeight);
        (PostgameRewardsMin,PostgameRewardsMax)=ResultColumn("Result rewards",rewardsAt,rewardsSize,DrawResultRewards);

        // .res-btns: Match history (ghost), Home/Leave (secondary) and the hextech CTA, right-aligned.
        string secondary=outNow?"LEAVE":"HOME",cta=outNow?"SPECTATE":networkRoom is null?"PLAY AGAIN":"BACK TO LOBBIES";
        float ctaWidth=Math.Max(200,ClientCss.Width(ClientCss.Face.DisplayBold,14,cta,1.68f)+52+4);
        float secondaryWidth=ClientCss.Width(ClientCss.Face.Display,14,secondary,1.12f)+32+2,historyWidth=ClientCss.Width(ClientCss.Face.BodyBold,14,"Match history")+32+2;
        var buttonsAt=new float2(max.X-1-padX,origin.Y+buttonsTop);
        var ctaAt=buttonsAt-new float2(ctaWidth,0);var secondaryAt=ctaAt-new float2(8+secondaryWidth,0);var historyAt=secondaryAt-new float2(8+historyWidth,0);
        postgameControls["resultHistory"]=(historyAt,historyAt+new float2(historyWidth,40.8f));
        ImGui.BeginDisabled(profile is null);
        if(ResultGhost("resultHistory","Match history",historyAt,new(historyWidth,40.8f))) NavigateClient("profile");
        ImGui.EndDisabled();
        postgameControls["menu"]=(secondaryAt,secondaryAt+new float2(secondaryWidth,40.8f));
        if(SecondaryButton("menu",secondary,secondaryAt,new(secondaryWidth,40.8f)))
        {
            if(outNow){bool online=networkRoom is not null;LeaveMatch();if(online)OpenMultiplayerView();}
            else DismissResults();
        }
        if (Session is not null)
        {
            string key=outNow?"spectateRemaining":"resultAgain";postgameControls[key]=(ctaAt,ctaAt+new float2(ctaWidth,40.8f));
            if(ResultCta(key,cta,ctaAt,new(ctaWidth,40.8f)))
            {
                if (outNow) { postgameDismissed=true; if(networkRoom is null) ContinueAfterElimination(); }
                else
                {
                    bool online=networkRoom is not null; bool nextDuos=Session.State.Duos;
                    LeaveMatch();
                    if(online) OpenMultiplayerView();
                    else StartLocalMatch(nextDuos);
                }
            }
        }
        ImGui.End(); return true;
    }

    /// <summary>.res-col: a framed, independently scrolling column.</summary>
    private (float2 Min,float2 Max) ResultColumn(string id,float2 at,float2 extent,Action content)
    {
        var draw=ImGui.GetWindowDrawList();
        draw.AddRectFilled(at,at+extent,new ImColor8(1,10,19,204));ClientCss.Border(draw,at,at+extent,ClientCss.Gold5);
        ImGui.SetCursorScreenPos(at+new float2(1));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(12,10));ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(0,0));
        ImGui.BeginChild(id,extent-new float2(2),ImGuiChildFlags.AlwaysUseWindowPadding,ImGuiWindowFlags.NoBackground);
        content();
        ImGui.EndChild();ImGui.PopStyleVar(2);
        return (at,at+extent);
    }
    // .btn.btn-icon
    private bool IconButton(string key,string glyph,float2 at,float extent)
    {
        bool pressed=CssHit(key,at,new(extent,extent),out bool hover);var draw=ImGui.GetWindowDrawList();var max=at+new float2(extent);
        if(hover)ClientCss.Glow(draw,at,max,new ImColor8(200,170,110,46),10,2);
        ClientCss.Gradient(draw,at+new float2(1),max-new float2(1),hover?new ImColor8(38,45,54):new ImColor8(30,35,40),hover?new ImColor8(26,33,43):new ImColor8(20,26,34));
        ClientCss.Border(draw,at,max,hover?ClientCss.Gold2:ClientCss.Gold4,1,2);
        // Inter has no ✕ glyph: draw the multiplication cross as two strokes (14px, weight 600).
        var center=at+new float2(extent/2);float arm=4.6f;var color=hover?ClientCss.Gold1:ClientCss.LcText;
        draw.AddLine(center-new float2(arm),center+new float2(arm),color,1.8f);draw.AddLine(center+new float2(-arm,arm),center+new float2(arm,-arm),color,1.8f);
        return pressed;
    }
    private bool ResultGhost(string key,string label,float2 at,float2 extent)
    {
        bool pressed=CssHit(key,at,extent,out bool hover);var draw=ImGui.GetWindowDrawList();var max=at+extent;float alpha=ImGui.GetStyle().Alpha;
        if(hover){draw.AddRectFilled(at,max,new ImColor8(200,170,110,20),2);ClientCss.Border(draw,at,max,ClientCss.Gold5,1,2);}
        ClientCss.TextCentered(draw,ClientCss.Face.BodyBold,14,at+new float2(0,(extent.Y-16.8f)/2),extent.X,ClientCss.Alpha(hover?ClientCss.Gold1:ClientCss.LcMute,alpha),label,0,16.8f);
        return pressed;
    }
    // .btn.btn-secondary
    private bool SecondaryButton(string key,string label,float2 at,float2 extent)
    {
        bool pressed=CssHit(key,at,extent,out bool hover);var draw=ImGui.GetWindowDrawList();var max=at+extent;
        if(hover)ClientCss.Glow(draw,at,max,new ImColor8(200,170,110,46),10,2);
        ClientCss.Gradient(draw,at+new float2(1),max-new float2(1),hover?new ImColor8(38,45,54):new ImColor8(30,35,40),hover?new ImColor8(26,33,43):new ImColor8(20,26,34));
        ClientCss.Border(draw,at,max,hover?ClientCss.Gold2:ClientCss.Gold4,1,2);
        ClientCss.TextCentered(draw,ClientCss.Face.Display,14,at+new float2(0,(extent.Y-16.8f)/2),extent.X,hover?ClientCss.Gold1:ClientCss.LcText,label,1.12f,16.8f);
        return pressed;
    }
    // .btn.lc-cta: navy-teal fill, gold border-image, inset cyan glow.
    private bool ResultCta(string key,string label,float2 at,float2 extent)
    {
        bool pressed=CssHit(key,at,extent,out bool hovered);var draw=ImGui.GetWindowDrawList();var max=at+extent;
        if(hovered)ClientCss.Glow(draw,at,max,new ImColor8(63,214,255,77),18);
        ClientCss.Gradient(draw,at,max,hovered?ClientCss.Gold1:ClientCss.Gold2,hovered?ClientCss.Gold2:ClientCss.Gold4);
        ClientCss.Gradient(draw,at+new float2(2),max-new float2(2),hovered?new ImColor8(15,82,102):new ImColor8(11,58,74),hovered?new ImColor8(8,48,74):new ImColor8(6,32,51));
        draw.PushClipRect(at+new float2(2),max-new float2(2),true);ClientCss.InsetGlow(draw,at+new float2(2),max-new float2(2),new ImColor8(63,214,255,hovered?(byte)115:(byte)64),14);draw.PopClipRect();
        ClientCss.TextCentered(draw,ClientCss.Face.DisplayBold,14,at+new float2(0,(extent.Y-16.8f)/2),extent.X,hovered?new ImColor8(255,255,255):new ImColor8(205,250,250),label,1.68f,16.8f);
        if(ImGui.IsItemFocused())draw.AddRect(at+new float2(4),max-new float2(4),NativeUiTheme.GoldBright);
        return pressed;
    }

    // Child-window cursor helpers: the column contents are drawn at the cursor and then advance it.
    private static float2 ResultCursor=>ImGui.GetCursorScreenPos();
    private static float ResultWidth=>ImGui.GetContentRegionAvail().X;
    private static void ResultAdvance(float height){ImGui.Dummy(new(ResultWidth,height));}
    private static void ResultHeading(string label)
    {
        // .tile-h: 14px Chakra Petch, 0.1em tracking, 6px below inside a result column.
        ClientCss.Text(ImGui.GetWindowDrawList(),ClientCss.Face.DisplayBold,14,ResultCursor,ClientCss.Gold1,label.ToUpperInvariant(),1.4f,18.9f);ResultAdvance(18.9f+6);
    }
    private static void ResultNote(string value)
        =>ResultAdvance(ClientCss.Paragraph(ImGui.GetWindowDrawList(),ClientCss.Face.Body,12.5f,ResultCursor,ResultWidth,16.875f,ClientCss.Text2,value));
    /// <summary>The rotated Bits diamond (.bit): 11px square, light at the top, deep blue at the bottom.</summary>
    private static void BitIcon(ImDrawListPtr draw,float2 center)
    {
        const float r=7.8f;
        ClientCss.Glow(draw,center-new float2(4),center+new float2(4),new ImColor8(79,200,255,120),8,4);
        draw.AddTriangleFilled(center+new float2(0,-r),center+new float2(r,0),center+new float2(-r,0),new ImColor8(140,226,255));
        draw.AddTriangleFilled(center+new float2(-r,0),center+new float2(r,0),center+new float2(0,r),new ImColor8(53,147,231));
        draw.AddTriangleFilled(center+new float2(0,-r),center+new float2(-r*.45f,-r*.55f),center+new float2(r*.45f,-r*.55f),new ImColor8(200,253,255));
    }
    /// <summary>.progress: 8px track with the cyan fill and glow.</summary>
    private static void ResultProgress(float value,bool done=false)
    {
        var at=ResultCursor;float width=ResultWidth;var draw=ImGui.GetWindowDrawList();
        draw.AddRectFilled(at,at+new float2(width,8),new ImColor8(1,10,19,217));
        float filled=Math.Clamp(value,0,1)*(width-2);
        if(filled>0)
        {
            // .mission.done turns the fill green.
            ClientCss.Glow(draw,at+new float2(1,1),at+new float2(1+filled,7),done?new ImColor8(79,224,143,128):new ImColor8(63,214,255,128),10);
            ClientCss.GradientX(draw,at+new float2(1,1),at+new float2(1+filled,7),done?new ImColor8(47,184,108):new ImColor8(10,111,143),done?new ImColor8(79,224,143):new ImColor8(63,214,255));
        }
        ClientCss.Border(draw,at,at+new float2(width,8),ClientCss.Gold5);
        ResultAdvance(8);
    }

    private void DrawResultStandings()
    {
        ResultHeading("Standings");
        foreach (var player in Session!.State.Players.OrderBy(p => p.Placement))
        {
            var at = ResultCursor; float rowWidth=ResultWidth;var draw=ImGui.GetWindowDrawList();
            if(at.Y>=draw.GetClipRectMin().Y && at.Y+30<=draw.GetClipRectMax().Y)PostgameVisibleStandings++;
            bool me=player.Id == localSeat;
            if (me)
            {
                ClientCss.GradientX(draw,at,at+new float2(rowWidth,30),new ImColor8(200,170,110,46),new ImColor8(200,170,110,0));
                draw.AddRectFilled(at,at+new float2(2,30),ClientCss.Gold2);
            }
            ClientCss.Text(draw,ClientCss.Face.DisplayBold,12.5f,at+new float2(8,7),ClientCss.Gold2,player.Placement > 0 ? $"#{player.Placement}" : "-",0,16);
            var avatarAt=at+new float2(8+30+8,2);var avatarCenter=avatarAt+new float2(13);
            draw.AddCircleFilled(avatarCenter,13,new ImColor8(10,20,40),32);
            if(portraits.DrawCompanion(ResultCompanion(player),avatarAt+new float2(1),24))PostgameAvatarCount++;
            draw.AddCircle(avatarCenter,13.6f,new ImColor8(1,10,19),32,2.2f);draw.AddCircle(avatarCenter,12.5f,ClientCss.Gold5,32,1);
            string name = (Session.State.Duos ? $"Team {player.Team+1} · " : "") + player.Name;
            string status=player.Placement == 1 ? "Winner" : player.Placement > 0 ? "Eliminated" : $"{Math.Max(0,player.Health)} HP";
            float statusWidth=ClientCss.Width(ClientCss.Face.Body,11,status);
            ClientCss.Text(draw,ClientCss.Face.Body,11,at+new float2(rowWidth-6-statusWidth,7.6f),ClientCss.Muted,status,0,14.85f);
            float nameLeft=8+30+8+28+8;
            ClientCss.Text(draw,ClientCss.Face.BodyBold,12.5f,at+new float2(nameLeft,6.6f),me?ClientCss.Gold1:ClientCss.Text1,ClientCss.Ellipsis(ClientCss.Face.BodyBold,12.5f,name,Math.Max(20,rowWidth-6-statusWidth-8-nameLeft)),0,16.875f);
            ImGui.Dummy(new(rowWidth,30)); if (ImGui.IsItemHovered()) ResultTooltip(name);
            ResultAdvance(2); PostgameStandings++;
        }
    }

    private void DrawResultRewards()
    {
        var draw=ImGui.GetWindowDrawList();
        // .dts-award-h
        float headingWidth=ClientCss.Text(draw,ClientCss.Face.DisplayBold,12.5f,ResultCursor,ClientCss.Gold1,"BITS EARNED",1.25f,16.875f);
        if(history?.FreshAward==false)ClientCss.Text(draw,ClientCss.Face.Body,12.5f,ResultCursor+new float2(headingWidth+4,0),ClientCss.Muted,"(already added)",0,16.875f);
        ResultAdvance(16.9f+6);
        if (history?.Award is { } award)
        {
            if(!ReferenceEquals(postgameAnimatedAward,award)) {postgameAnimatedAward=award;postgameAwardStart=boardSceneTime;postgameAwardSounds=0;}
            double time=boardSceneTime-postgameAwardStart;
            PostgameDisplayedBits=history.FreshAward?PostgameAwardCount.Total(award.Total,award.Lines.Length,time):award.Total;
            for(int i=0;i<award.Lines.Length;i++)
            {
                var line=award.Lines[i]; int count=history.FreshAward?PostgameAwardCount.Line(line.N,i,time):line.N;
                var at=ResultCursor;float width=ResultWidth;string value=$"+{count}";
                ClientCss.Text(draw,ClientCss.Face.Body,13,at+new float2(0,2),ClientCss.Text1,ClientCss.Ellipsis(ClientCss.Face.Body,13,line.Label,width-60),0,17.55f);
                ClientCss.Text(draw,ClientCss.Face.BodyBold,13,at+new float2(width-ClientCss.Width(ClientCss.Face.BodyBold,13,value),2),new ImColor8(79,200,255),value,0,17.55f);
                ResultAdvance(21.5f);
            }
            {
                // .dts-aline.tot: rule, then Total and the Bits count in white.
                ResultAdvance(4);var at=ResultCursor;float width=ResultWidth;
                draw.AddLine(at+new float2(0,.5f),at+new float2(width,.5f),new ImColor8(34,48,73));
                string total=PostgameDisplayedBits.Value.ToString();float totalWidth=ClientCss.Width(ClientCss.Face.BodyBold,16,total);
                ClientCss.Text(draw,ClientCss.Face.BodyBold,16,at+new float2(0,7),ClientCss.Text1,"Total",0,21.6f);
                BitIcon(draw,at+new float2(width-totalWidth-4-5.5f,7+11));
                ClientCss.Text(draw,ClientCss.Face.BodyBold,16,at+new float2(width-totalWidth,7),new ImColor8(255,255,255),total,0,21.6f);
                ResultAdvance(30.6f);
            }
            {
                // .dts-award-f: wallet and the small Store button.
                ResultAdvance(8);var at=ResultCursor;float width=ResultWidth;
                float x=ClientCss.Text(draw,ClientCss.Face.Body,13,at+new float2(0,3.3f),ClientCss.Muted,"Wallet:",0,17.55f);
                BitIcon(draw,at+new float2(x+4+5.5f,3.3f+9.5f));
                ClientCss.Text(draw,ClientCss.Face.BodyBold,13,at+new float2(x+4+15,3.3f),ClientCss.Muted,(profile?.Progress.Bits ?? 0).ToString(),0,17.55f);
                float storeWidth=ClientCss.Width(ClientCss.Face.BodyBold,12,"Store")+22+2;var storeAt=at+new float2(width-storeWidth,0);
                postgameControls["resultStore"]=(storeAt,storeAt+new float2(storeWidth,24.2f));
                if(SmallButton("resultStore","Store",storeAt,new(storeWidth,24.2f)))
                {
                    // Shared navigation remembers Home so Back restores this result.
                    NavigateClient("store");postgameStore=true;
                }
                var clipMin=draw.GetClipRectMin();var clipMax=draw.GetClipRectMax();
                PostgameStoreButtonVisible=storeAt.Y>=clipMin.Y&&storeAt.Y+24.2f<=clipMax.Y;
                ImGui.SetCursorScreenPos(at);ResultAdvance(24.2f);
            }
        }
        else ResultNote(IsSpectating ? "Join a seat to earn rewards in your next match." : history?.Error ?? (networkRoom is null ? "Rewards are unavailable for this session." : "Verifying the shared match before saving rewards..."));
        ImGui.BeginDisabled(networkRoom is not null && networkReplica?.LastDesync is not null);
        if (history?.Error is not null && Button("retryHistory","Retry saving progress")) history.Complete(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        ImGui.EndDisabled();
    }
    private bool SmallButton(string key,string label,float2 at,float2 extent)
    {
        bool pressed=CssHit(key,at,extent,out bool hover);var draw=ImGui.GetWindowDrawList();var max=at+extent;
        ClientCss.Gradient(draw,at+new float2(1),max-new float2(1),hover?new ImColor8(38,45,54):new ImColor8(30,35,40),hover?new ImColor8(26,33,43):new ImColor8(20,26,34));
        ClientCss.Border(draw,at,max,hover?ClientCss.Gold2:ClientCss.Gold4,1,2);
        ClientCss.TextCentered(draw,ClientCss.Face.BodyBold,12,at+new float2(0,(extent.Y-16.2f)/2),extent.X,hover?ClientCss.Gold1:ClientCss.LcText,label,0,16.2f);
        return pressed;
    }

    private void DrawResultProgress()
    {
        try
        {
            var draw=ImGui.GetWindowDrawList();
            var level = TamerProgress.Read(profile!.Storage.GetValueOrDefault("dt.client.v1"));
            {
                // .res-xp: label row, 6px gap, the level bar, then 10px.
                var at=ResultCursor;float width=ResultWidth;
                float labelWidth=ClientCss.Text(draw,ClientCss.Face.Body,12.5f,at+new float2(0,.3f),ClientCss.Text1,$"Tamer level {level.Level}",0,16.875f);
                if(history?.LeveledUp==true)ClientCss.Text(draw,ClientCss.Face.BodyBold,12.5f,at+new float2(labelWidth+4,.3f),new ImColor8(79,224,143),"Level up!",0,16.875f);
                if(history?.XpGained>0){string gain=$"+{history.XpGained} XP";ClientCss.Text(draw,ClientCss.Face.Body,13,at+new float2(width-ClientCss.Width(ClientCss.Face.Body,13,gain),0),ClientCss.Muted,gain,0,17.55f);}
                ResultAdvance(17.5f+6);
                ResultProgress((float)((double)level.Xp/level.Need));
                if(ImGui.IsItemHovered())ResultTooltip($"{level.Xp} / {level.Need} XP");
                ResultAdvance(10);
            }
            var daily=MatchRewards.Progress(profile.Storage.GetValueOrDefault("dt.shop.v1"),profile.Storage.GetValueOrDefault("dt.client.v1"),DateOnly.FromDateTime(DateTime.Now));
            foreach(var mission in daily.Missions)
            {
                // .mission: padding 3 0 6; the name/bits row sits 4px above its bar.
                ResultAdvance(3);var at=ResultCursor;float width=ResultWidth;string bits=mission.Bits.ToString();
                float bitsWidth=ClientCss.Width(ClientCss.Face.BodyBold,12.5f,bits);
                ClientCss.Text(draw,ClientCss.Face.BodyBold,12.5f,at,mission.Done?ClientCss.Text2:ClientCss.Text1,ClientCss.Ellipsis(ClientCss.Face.BodyBold,12.5f,mission.Name,width-bitsWidth-30),0,16.875f);
                BitIcon(draw,at+new float2(width-bitsWidth-4-5.5f,8.6f));
                ClientCss.Text(draw,ClientCss.Face.BodyBold,12.5f,at+new float2(width-bitsWidth,0),ClientCss.Gold1,bits,0,16.875f);
                ResultAdvance(16.9f+4);
                ResultProgress(mission.Done?1:(float)mission.Current/mission.Goal,mission.Done);
                if(ImGui.IsItemHovered())ResultTooltip(mission.Done ? "Complete" : $"Best today {mission.Current}/{mission.Goal}");
                ResultAdvance(6); PostgameMissionCount++;
            }
            {
                // .daily: rule, orange hexagon with "+", label and the bonus.
                ResultAdvance(2);var at=ResultCursor;float width=ResultWidth;
                draw.AddLine(at+new float2(0,.5f),at+new float2(width,.5f),ClientCss.Gold5);
                var hex=at+new float2(13,7+13);
                var hexTop=daily.Claimed?new ImColor8(111,240,168):new ImColor8(255,194,74);var hexBottom=daily.Claimed?new ImColor8(47,184,108):new ImColor8(255,138,61);
                draw.AddQuadFilled(hex+new float2(0,-13),hex+new float2(13,-6.5f),hex+new float2(13,0),hex+new float2(-13,0),hexTop);
                draw.AddTriangleFilled(hex+new float2(0,-13),hex+new float2(-13,0),hex+new float2(-13,-6.5f),hexTop);
                draw.AddQuadFilled(hex+new float2(-13,0),hex+new float2(13,0),hex+new float2(13,6.5f),hex+new float2(0,13),hexBottom);
                draw.AddTriangleFilled(hex+new float2(-13,0),hex+new float2(0,13),hex+new float2(-13,6.5f),hexBottom);
                if(daily.Claimed){draw.AddLine(hex+new float2(-5,0),hex+new float2(-1.5f,3.5f),new ImColor8(20,13,5),2.2f);draw.AddLine(hex+new float2(-1.5f,3.5f),hex+new float2(5,-4),new ImColor8(20,13,5),2.2f);}
                else ClientCss.TextCentered(draw,ClientCss.Face.BodyBold,14,hex-new float2(13,9.45f),26,new ImColor8(20,13,5),"+",0,18.9f);
                ClientCss.Text(draw,ClientCss.Face.BodyBold,12.5f,at+new float2(36,10.6f),ClientCss.Text1,"Daily bonus",0,16.875f);
                string bits=daily.Bits.ToString();float bitsWidth=ClientCss.Width(ClientCss.Face.BodyBold,12.5f,bits);
                BitIcon(draw,at+new float2(width-bitsWidth-4-5.5f,11.6f+8.6f-2));
                ClientCss.Text(draw,ClientCss.Face.BodyBold,12.5f,at+new float2(width-bitsWidth,11.6f),ClientCss.Gold1,bits,0,16.875f);
                ImGui.Dummy(new(width,33));
                if(ImGui.IsItemHovered())ResultTooltip(daily.Claimed ? "Claimed. Back tomorrow." : "Complete your first game today.");
            }
        }
        catch(Exception error) when(error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException or ArgumentOutOfRangeException)
        { ResultNote("Saved progress could not be read. Your data has been preserved."); }
    }

    private void UpdatePostgameAward()
    {
        if(postgameAnimatedAward is not { } award || !ReferenceEquals(postgameSession,Session) || history?.FreshAward!=true)return;
        int completed=PostgameAwardCount.CompletedRows(award.Lines.Length,boardSceneTime-postgameAwardStart);
        while(postgameAwardSounds<completed)audio.Play(++postgameAwardSounds>award.Lines.Length?"pick":"tick_normal");
    }

    private static string ResultTrim(string value,float width)
    {
        if(ImGui.CalcTextSize(value).X<=width)return value;
        while(value.Length>0 && ImGui.CalcTextSize(value+"...").X>width)value=value[..^1];
        return value+"...";
    }
    private static void ResultTooltip(string value) { ImGui.BeginTooltip(); ImGui.Text(value); ImGui.EndTooltip(); }
    private static (double R,double G,double B) ResultTraitTint(string hex)
    {
        int rgb=int.Parse(hex.AsSpan(1),System.Globalization.NumberStyles.HexNumber,System.Globalization.CultureInfo.InvariantCulture);
        return ((rgb>>16)&255,(rgb>>8)&255,rgb&255);
    }

    private string? ResultCompanion(MatchPlayer player)
    {
        try
        {
            System.Text.Json.Nodes.JsonObject? look=null;
            if(player.IsHuman && player.Id==localSeat) look=CosmeticLoadouts.Local(profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"),profile?.Storage.GetValueOrDefault("dt.legendevo.v1"));
            else if(player.IsHuman && networkRoom is not null)
            {
                string? uid=networkRoom.Authority.Lobby?["slots"]?[player.Id.ToString()]?["uid"]?.GetValue<string>();
                if(uid is not null)look=networkRoom.Authority.Presence[uid]?["look"] as System.Text.Json.Nodes.JsonObject;
            }
            if(look is null && !aiLooks.TryGetValue((player.Id,player.Name),out look))aiLooks[(player.Id,player.Name)]=look=CosmeticLoadouts.Ai(player.Id,player.Name);
            return CosmeticLoadouts.CompanionKey(look);
        }
        catch(Exception error) when(error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException){return null;}
    }

    // Solo seats use the party's Tamer names (DT.DEFAULT_TAMERS), as the original game and the party screen do.
    internal static string LocalSeatName(int seat,string? own)=>seat==0?(string.IsNullOrWhiteSpace(own)?"You":own):LobbyAiNames[seat-1];
    private void StartLocalMatch(bool duo)
    {
        if(networkRoom is not null || steam?.Room is not null)return;
        if(Session is not null)LeaveMatch();
        audio.PlayClient("fight",false);
        uint matchSeed=seed??(uint)Random.Shared.Next(1_000_000_000);
        var resolved=SetChoices.Resolve(selectedSet,selectedPace,matchSeed);var rules=SetSelection.Rules(catalog,resolved);
        variant=Array.IndexOf(catalog.Variants,rules); duos=duo;
        Session=new(rules,matchSeed,Enumerable.Range(0,8).Select(i=>new MatchSeat(LocalSeatName(i,profile?.Progress.Name),i==0)).ToArray(),duo,chaos:resolved.Chaos,chaosPace:resolved.ChaosPace);
        history=profile is null?null:new(Session,profile); localPaused=showGameMenu=confirmLeave=false;
        viewed=localSeat=0; CompletedRounds=0; selectedUnit=selectedItem=null; backgroundTime=0;
        audio.Music("planning"); status="Select a unit, then a hex. Select an item, then its holder.";
        BeginMatchLoading();
    }
}

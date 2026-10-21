using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Networking;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    // The original client reserves one navigation row on every page: 80 px, or 72 px in windows 820 px tall or less.
    private float ClientPageTop => socialWindowSize.Y<=820?72:80;
    private bool ClientNavigationBlocked=>training is not null||EliminationCinematicActive||evolutionLegend is not null||CollectionModalVisible;
    private readonly List<string> clientHistory=[];
    private bool clientNavigationDispatch;
    internal string ClientPrimaryLabel=>steam?.Room is null?"PLAY":"LOBBY";
    internal string ClientPresenceLabel=>steam?.Room is null?"Online":"In lobby";
    internal string ClientRoomLabel=>steam?.Room is {} room?$"Lobby {(room.Authority.Lobby?["slots"] as System.Text.Json.Nodes.JsonObject)?.Count??0}/8":"SOLO";
    private string CurrentClientRoute=>showProfile?"profile":showSettings?"settings":showSaveData?"savedata":showAbout?"about":showMultiplayer?"multiplayer":showHomePlay?"play":showFarm?"farm":codexFull?"codex":showCollection?(showEggs?"loot":collectionStore?"store":"collection"):"home";

    private void NavigateClient(string destination,bool remember=true)
    {
        if (ClientNavigationBlocked) return;
        if(remember)RememberClientRoute(destination);
        postgameStore = false;
        if (evolutionLegend is not null) CloseEvolution();
        showProfile = showFarm = showCollection = showAbout = showSettings = showSaveData = showHomePlay = showMultiplayer = false;
        codexFull = codexSide = false; codexUnit = codexModalLine = null;
        showMusicLibrary = showAudio = showPlayers = showEmotes = showSoundboard = false;
        confirmResetSave = false; resetSaveText.SetValue("");
        CancelArenaDrag(); cameraDrag = null;
        clientNavigationDispatch=true;
        try
        {
        switch (destination)
        {
            case "play": homePlayParty = false; showHomePlay = true; break;
            case "collection": OpenCollectionPage(); break;
            case "store": OpenCollectionPage(store: true); break;
            case "loot": OpenCollectionPage(loot: true); break;
            case "farm": if (farm is not null) { farm.Refresh(HomeClock().ToUnixTimeMilliseconds()); showFarm = true; } break;
            case "codex": OpenCodexFull(); break;
            case "profile": OpenProfile(); break;
            case "settings": showSettings = true; break;
            case "savedata": OpenSaveData(); break;
            case "multiplayer": OpenMultiplayerView(); break;
            case "about": showAbout = true; break;
        }
        }
        finally {clientNavigationDispatch=false;}
    }

    private void RememberClientRoute(string destination)
    {
        if(clientNavigationDispatch)return;
        string origin=CurrentClientRoute;
        if(destination==origin)return;
        clientHistory.Add(origin);if(clientHistory.Count>20)clientHistory.RemoveAt(0);
    }

    private void BackClient()
    {
        if(ClientNavigationBlocked)return;
        if(showHomePlay&&homePlayParty){homePlayParty=false;return;}
        string current=CurrentClientRoute;
        while(clientHistory.Count>0)
        {
            string previous=clientHistory[^1];clientHistory.RemoveAt(clientHistory.Count-1);
            if(previous==current)continue;
            NavigateClient(previous,false);return;
        }
        NavigateClient("home",false);
    }

    private bool ClientTextButton(string key,string label,float width,float height,bool selected=false,float font=15)
    {
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,font);
        ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(0,0,0,0));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new ImColor8(200,170,110,18));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive,new ImColor8(200,170,110,30));
        ImGui.PushStyleColor(ImGuiCol.Text,selected?HomeInk:HomeMute);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,0);ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,0);
        bool clicked=Button(key,label,width,height);
        if(selected)
        {
            var lo=ImGui.GetItemRectMin();var hi=ImGui.GetItemRectMax();var draw=ImGui.GetWindowDrawList();
            draw.AddRectFilledMultiColor(new(lo.X,hi.Y-28),hi,new ImColor8(200,170,110,0),new ImColor8(200,170,110,0),new ImColor8(200,170,110,25),new ImColor8(200,170,110,25));
            draw.AddRectFilled(new(lo.X+10,hi.Y-2),new(hi.X-10,hi.Y),HomeInk);
        }
        ImGui.PopStyleVar(2);ImGui.PopStyleColor(4);ImGui.PopFont();return clicked;
    }

    // theme.js #cnav and its @media steps (1700/1500/1380/1180 px wide, 820 px tall).
    private readonly record struct ClientNavMetrics(float Height,float PlayMargin,float PlayLeft,float PlayMinWidth,float PlayPadding,float Mark,float PlayFont,
        float TabFont,float TabPadding,float TabTracking,float RightPadding,float RightGap,bool StatusText,bool StoreIcon,float ProfileWidth,float ProfilePadding,bool ProfileText);
    private ClientNavMetrics ClientNav
    {
        get
        {
            float width=socialWindowSize.X;bool narrow=width<1200;
            return new(socialWindowSize.Y<=820?72:80,socialWindowSize.Y<=820?12:14,width<=1500?10:14,narrow?120:width<=1500?140:162,narrow?26:width<=1500?28:34,narrow?28:36,width<=1500?17:20,
                width<=1380?11.5f:width<=1500?12:width<=1700?13:14,width<=1380?7:width<=1500?8:width<=1700?10:13,width<=1380?.06f:width<=1500?.08f:width<=1700?.10f:.12f,
                width<=1380?8:12,width<=1380?4:6,width>1500,width>1380,narrow?118:SocialWidth,width<=1380?10:14,!narrow);
        }
    }
    private string ClientDisplayName=>LobbyAuthority.CleanName(steam?.Name??profile?.Storage.GetValueOrDefault("dt.name")) is { Length: > 0 } name ? name : "Tamer";
    private string ClientStatusLabel=>steam?.Room is {} room?(room.Authority.Lobby?["status"]?.GetValue<string>()=="playing"?"In a game":ClientRoomLabel):steam is not null?"Online":"Solo";

    /// <summary>An invisible ImGui button (input, focus, sound, smoke anchor) that the caller paints itself.</summary>
    private bool ClientHitButton(string key,float width,float height)
    {
        ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new ImColor8(0,0,0,0));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(0,0,0,0));
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,0);
        bool clicked=Button(key,"",width,height);
        ImGui.PopStyleVar();ImGui.PopStyleColor(4);return clicked;
    }

    private void DrawClientNavigation()
    {
        if (!ClientHomeVisible) return;
        DrawClientBackdrop();
        var nav=ClientNav;
        float width=socialWindowSize.X,height=nav.Height,profileLeft=width-nav.ProfileWidth;
        ClientLayer("Client navigation",new(0,0),new(width,height),false,true);
        var draw=ImGui.GetWindowDrawList();
        NativeUiPaint.VerticalGradient(draw,new(0,0),new(width,height),NativeUiTheme.Navy0,new ImColor8(7,17,31),new ImColor8(10,20,40),.7f);
        // box-shadow: 0 6px 24px rgba(0,0,0,.55) falls over the page below the bar.
        var shadow=ImGui.GetForegroundDrawList();
        NativeUiPaint.VerticalGradient(shadow,new(0,height),new(profileLeft,height+26),new ImColor8(0,0,0,92),new ImColor8(0,0,0,0));
        ImGui.BeginDisabled(ClientNavigationBlocked);
        DrawClientPlay(nav);
        float x=nav.PlayLeft+ClientPlayWidth(nav)+10+(width<1200?0:4);
        foreach(var tab in new[]{("homeOverview","HOME","home"),("clientModes","MODES","play"),("collection","COLLECTION","collection"),("clientLoot","LOOT","loot"),
            ("clientStore","STORE","store"),("farm","DIGI-FARM","farm"),("codex","CODEX","codex")})
        {
            bool selected=tab.Item3 switch
            {
                "play"=>showHomePlay,"collection"=>showCollection&&!collectionStore&&!showEggs,
                "store"=>showCollection&&collectionStore&&!showEggs,"loot"=>showCollection&&showEggs,
                "farm"=>showFarm,"codex"=>codexFull,
                _=>!showHomePlay&&!showCollection&&!showFarm&&!codexFull&&!showMultiplayer&&!showProfile&&!showSettings&&!showSaveData&&!showAbout
            };
            float tabWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayBoldFont,nav.TabFont,tab.Item2,nav.TabTracking)+nav.TabPadding*2;
            ImGui.SetCursorPos(new(x,0));
            bool locked=profile is null&&tab.Item3 is "collection" or "loot" or "store" or "farm";
            ImGui.BeginDisabled(locked);
            if(ClientHitButton(tab.Item1,tabWidth,height))NavigateClient(tab.Item3);
            bool hovered=!locked&&ImGui.IsItemHovered();
            ImGui.EndDisabled();
            var lo=new float2(x,0);var hi=new float2(x+tabWidth,height);
            if(selected||hovered)
            {
                // radial-gradient(70% 70% at 50% 100%, rgba(200,170,110,.2), transparent); hover uses 60% and .14.
                draw.PushClipRect(lo,hi,true);
                NativeUiPaint.Radial(draw,new((lo.X+hi.X)/2,hi.Y),new float2(tabWidth,height)*(selected?.7f:.6f),new ImColor8(200,170,110,(byte)(selected?51:36)),new ImColor8(200,170,110,0));
                draw.PopClipRect();
            }
            var text=NativeUiPaint.TrackedSize(NativeUiTheme.DisplayBoldFont,nav.TabFont,tab.Item2,nav.TabTracking);
            NativeUiPaint.Tracked(draw,NativeUiTheme.DisplayBoldFont,nav.TabFont,new(x+nav.TabPadding,MathF.Round((height-text.Y)/2)),
                selected||hovered?NativeUiTheme.GoldBright:NativeUiTheme.LcMute,tab.Item2,nav.TabTracking);
            if(selected)
            {
                NativeUiPaint.Radial(draw,new((lo.X+hi.X)/2,hi.Y-2),new((tabWidth-20)*.55f,9),new ImColor8(200,170,110,70),new ImColor8(200,170,110,0));
                NativeUiPaint.FadedLine(draw,new(lo.X+10,hi.Y-3),new(hi.X-10,hi.Y-1),NativeUiTheme.GoldBright);
            }
            x+=tabWidth;
        }
        DrawClientUtilities(nav,profileLeft);
        DrawClientIdentity(nav,new(profileLeft,0),new(nav.ProfileWidth,height));
        // #cnav::after: a 1 px gold rule along the bottom edge.
        float rule=height-1;
        NativeUiPaint.HorizontalGradient(draw,new(0,rule),new(width*.4f,height),NativeUiTheme.GoldBorder,NativeUiTheme.GoldDim);
        draw.AddRectFilled(new(width*.4f,rule),new(width*.7f,height),NativeUiPaint.Fade(NativeUiTheme.GoldDim));
        NativeUiPaint.HorizontalGradient(draw,new(width*.7f,rule),new(width,height),NativeUiTheme.GoldDim,NativeUiTheme.GoldBorder);
        DrawClientWindowButtons(width);
        CollectionNavigationAnchors();
        ImGui.EndDisabled();ImGui.End();
    }

    private static float ClientPlayWidth(ClientNavMetrics nav)
        => Math.Max(nav.PlayMinWidth,10+nav.Mark+10+NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayBoldFont,nav.PlayFont,"PLAY",.16f)+nav.PlayPadding);

    // .cn-play: gold chevron frame, teal inner plate with an inset cyan glow, mark and glowing label.
    private void DrawClientPlay(ClientNavMetrics nav)
    {
        float playWidth=ClientPlayWidth(nav),playHeight=nav.Height-nav.PlayMargin*2;
        ImGui.SetCursorPos(new(nav.PlayLeft,nav.PlayMargin));
        if(ClientHitButton("clientPlay",playWidth,playHeight))NavigateClient(steam?.Room is null?"play":"multiplayer");
        bool hovered=ImGui.IsItemHovered(),active=showHomePlay||showMultiplayer;
        var lo=ImGui.GetItemRectMin();var hi=ImGui.GetItemRectMax();var draw=ImGui.GetWindowDrawList();
        float middle=(lo.Y+hi.Y)/2;
        var uv=ImGui.GetFontTexUvWhitePixel();
        void Chevron(float2 a,float2 b,float cut,ImColor8 top,ImColor8 mid,ImColor8 bottom,float split)
        {
            float midY=a.Y+(b.Y-a.Y)*split,cy=(a.Y+b.Y)/2;
            ImColor8 At(float y)=>y<=midY?Mix(top,mid,(y-a.Y)/(midY-a.Y)):Mix(mid,bottom,(y-midY)/(b.Y-midY));
            // Body rectangle in two gradient bands, then the arrow tip as gradient triangles.
            NativeUiPaint.VerticalGradient(draw,a,new(b.X-cut,midY),top,mid);NativeUiPaint.VerticalGradient(draw,new(a.X,midY),new(b.X-cut,b.Y),mid,bottom);
            draw.PrimReserve(6,6);
            draw.PrimVtx(new(b.X-cut,a.Y),uv,NativeUiPaint.Fade(top));draw.PrimVtx(new(b.X,cy),uv,NativeUiPaint.Fade(At(cy)));draw.PrimVtx(new(b.X-cut,cy),uv,NativeUiPaint.Fade(At(cy)));
            draw.PrimVtx(new(b.X-cut,cy),uv,NativeUiPaint.Fade(At(cy)));draw.PrimVtx(new(b.X,cy),uv,NativeUiPaint.Fade(At(cy)));draw.PrimVtx(new(b.X-cut,b.Y),uv,NativeUiPaint.Fade(bottom));
        }
        // Outer frame: linear-gradient(180deg, gold-1, gold-3 45%, gold-4).
        Chevron(lo,hi,22,NativeUiTheme.GoldBright,NativeUiTheme.GoldDeep,NativeUiTheme.GoldBorder,.45f);
        var innerLo=lo+new float2(2,2);var innerHi=hi-new float2(3,2);
        float bright=hovered?1.35f:active?1.25f:1;
        ImColor8 Lift(ImColor8 c){var v=c.AsByte4();return new((byte)Math.Min(255,v.X*bright),(byte)Math.Min(255,v.Y*bright),(byte)Math.Min(255,v.Z*bright),v.W);}
        // ::after: inset 2px 3px 2px 2px, its tip 1px inside that box (calc(100% - 1px) 50%).
        Chevron(innerLo,new(hi.X-4,innerHi.Y),20,Lift(new(15,74,92)),Lift(new(6,32,51)),Lift(new(4,22,36)),.6f);
        // The gradient fills have hard edges; antialias the diagonal tip edges like a clip-path edge.
        draw.AddLine(new(hi.X-22,lo.Y+.5f),new(hi.X-.5f,middle),NativeUiPaint.Fade(Mix(NativeUiTheme.GoldBright,NativeUiTheme.GoldDeep,.7f)),1.2f);
        draw.AddLine(new(hi.X-.5f,middle),new(hi.X-22,hi.Y-.5f),NativeUiPaint.Fade(Mix(NativeUiTheme.GoldDeep,NativeUiTheme.GoldBorder,.6f)),1.2f);
        // inset 0 0 18px rgba(63,214,255,.35) within the plate's rectangular body.
        draw.PushClipRect(innerLo,new(innerHi.X-18,innerHi.Y),true);
        NativeUiPaint.InsetGlow(draw,innerLo,new(innerHi.X+20,innerHi.Y),new ImColor8(63,214,255,(byte)Math.Min(255,89*bright)),18);
        draw.PopClipRect();
        var markAt=new float2(lo.X+10,middle-nav.Mark/2);
        NativeUiPaint.Radial(draw,markAt+new float2(nav.Mark/2,nav.Mark/2+2),new(nav.Mark*.75f),new ImColor8(255,138,61,90),new ImColor8(255,138,61,0),28);
        homeArtwork.Draw("client-mark",markAt,new(nav.Mark,nav.Mark),(byte)Math.Clamp(Math.Round(255*ImGui.GetStyle().Alpha),0,255));
        var label=ClientPrimaryLabel;var size=NativeUiPaint.TrackedSize(NativeUiTheme.DisplayBoldFont,nav.PlayFont,label,.16f);
        var textAt=new float2(markAt.X+nav.Mark+10,MathF.Round(middle-size.Y/2));
        NativeUiPaint.TextGlow(draw,NativeUiTheme.DisplayBoldFont,nav.PlayFont,textAt,label,.16f,new ImColor8(63,214,255,60),12);
        NativeUiPaint.Tracked(draw,NativeUiTheme.DisplayBoldFont,nav.PlayFont,textAt,hovered?new ImColor8(255,255,255):NativeUiTheme.AccentInk,label,.16f);
        if(ImGui.IsItemFocused()&&ImGui.GetIO().NavVisible)draw.AddRect(lo-new float2(2,2),hi+new float2(2,2),NativeUiPaint.Fade(NativeUiTheme.Accent),0,ImDrawFlags.None,2);
    }

    private static ImColor8 Mix(ImColor8 a,ImColor8 b,float t)
    {
        t=Math.Clamp(t,0,1);var x=a.AsByte4();var y=b.AsByte4();
        return new((byte)(x.X+(y.X-x.X)*t),(byte)(x.Y+(y.Y-x.Y)*t),(byte)(x.Z+(y.Z-x.Z)*t),(byte)(x.W+(y.W-x.W)*t));
    }

    // .cn-right: status chip, Bits chip and the round Store/Loot icons, right-aligned before the profile block.
    private void DrawClientUtilities(ClientNavMetrics nav,float profileLeft)
    {
        var draw=ImGui.GetWindowDrawList();float middle=nav.Height/2,right=profileLeft-nav.RightPadding;
        foreach(var icon in nav.StoreIcon?new[]{("clientLootIcon","loot"),("clientStoreIcon","store")}:new[]{("clientLootIcon","loot")})
        {
            right-=34;ImGui.SetCursorPos(new(right,middle-17));
            ImGui.BeginDisabled(profile is null);
            if(ClientHitButton(icon.Item1,34,34))NavigateClient(icon.Item2);
            bool iconHover=ImGui.IsItemHovered();
            bool on=iconHover||(icon.Item2=="store"?showCollection&&collectionStore&&!showEggs:showCollection&&showEggs);
            ImGui.EndDisabled();
            if(iconHover)ResultTooltip(icon.Item2=="store"?"Store":"Loot");
            var center=new float2(right+17,middle);
            draw.AddCircleFilled(center,17,NativeUiPaint.Fade(new ImColor8(1,10,19,153)),32);
            draw.AddCircle(center,16.5f,NativeUiPaint.Fade(on?NativeUiTheme.Gold:NativeUiTheme.GoldBorder),32,1);
            var ink=NativeUiPaint.Fade(on?NativeUiTheme.GoldBright:NativeUiTheme.Gold);
            if(icon.Item2=="store")DrawStoreGlyph(draw,center,ink);else DrawLootGlyph(draw,center,ink);
            right-=nav.RightGap;
        }
        // Bits: .chip-bits (border gold-5, navy fill, 800-weight gold-1 numerals, glowing diamond).
        string balance=$"{profile?.Progress.Bits??0:N0}";
        // The chip sets no font-size, so the label uses Chromium's 13.333px button default.
        const float bitsFont=13.333f;
        var balanceSize=NativeUiPaint.TrackedSize(NativeUiTheme.BodyExtraBoldFont,bitsFont,balance,0);
        float chipWidth=1+10+15+6+balanceSize.X+12+1,chipHeight=12+balanceSize.Y;
        right-=chipWidth;ImGui.SetCursorPos(new(right,MathF.Round(middle-chipHeight/2)));
        if(ClientHitButton("clientBalance",chipWidth,chipHeight))NavigateClient("store");
        bool bitsHover=ImGui.IsItemHovered();
        if(bitsHover)ResultTooltip("Bits: earned by playing games");
        var chipLo=new float2(right,MathF.Round(middle-chipHeight/2));var chipHi=chipLo+new float2(chipWidth,chipHeight);
        if(bitsHover)NativeUiPaint.BoxGlow(draw,chipLo,chipHi,new ImColor8(79,200,255,64),14);
        draw.AddRectFilled(chipLo,chipHi,NativeUiPaint.Fade(new ImColor8(1,10,19,179)));
        draw.AddRect(chipLo,chipHi,NativeUiPaint.Fade(bitsHover?NativeUiTheme.GoldDeep:NativeUiTheme.GoldDim));
        NativeUiPaint.Bit(draw,new(chipLo.X+1+10+1+5.5f,middle),11);
        NativeUiPaint.Tracked(draw,NativeUiTheme.BodyExtraBoldFont,bitsFont,new(chipLo.X+1+10+15+6,chipLo.Y+6),NativeUiTheme.GoldBright,balance);
        right-=nav.RightGap;
        // Status: .cn-status (dot + "Solo"/"Online"/"Lobby n/8"); the label hides at 1500 px and below.
        string status=ClientStatusLabel;bool live=steam?.Room is not null,online=steam is not null;
        var statusSize=NativeUiPaint.TrackedSize(NativeUiTheme.BodyFont,11,status,0);
        float statusWidth=1+10+7+(nav.StatusText?7+statusSize.X:0)+10+1;
        right-=statusWidth;ImGui.SetCursorPos(new(right,middle-14));
        if(ClientHitButton("clientMultiplayer",statusWidth,28))NavigateClient("multiplayer");
        bool statusHover=ImGui.IsItemHovered();
        if(statusHover)ResultTooltip(steam?.Room?.Authority.Lobby?["name"]?.GetValue<string>() is {} room?"Lobby: "+room:"Multiplayer lobbies");
        var statusLo=new float2(right,middle-14);var statusHi=statusLo+new float2(statusWidth,28);
        draw.AddRectFilled(statusLo,statusHi,NativeUiPaint.Fade(new ImColor8(1,10,19,153)));
        draw.AddRect(statusLo,statusHi,NativeUiPaint.Fade(live?new ImColor8(255,138,61,115):statusHover||showMultiplayer?NativeUiTheme.Gold:NativeUiTheme.GoldDim));
        var dot=new float2(statusLo.X+11+3.5f,middle);var dotColor=live?NativeUiTheme.Play:online?NativeUiTheme.Success:NativeUiTheme.Text3;
        if(live||online)NativeUiPaint.Radial(draw,dot,new(8),NativeUiPaint.Alpha(dotColor,150),NativeUiPaint.Alpha(dotColor,0),20);
        draw.AddCircleFilled(dot,3.5f,NativeUiPaint.Fade(dotColor),16);
        if(nav.StatusText)
            NativeUiPaint.Tracked(draw,NativeUiTheme.BodyFont,11,new(dot.X+3.5f+7,MathF.Round(middle-statusSize.Y/2)),live?NativeUiTheme.Text1:statusHover?NativeUiTheme.GoldBright:NativeUiTheme.LcMute,status);
    }

    // The original 24x24 SVG icons (index.html #cnStoreIc / #cnLootIc), redrawn at 19 px.
    private static void DrawStoreGlyph(ImDrawListPtr draw,float2 center,ImColor8 ink)
    {
        const float s=19f/24;float2 P(float x,float y)=>center+new float2((x-12)*s,(y-12)*s);
        draw.AddQuadFilled(P(4,7),P(20,7),P(19,21),P(5,21),ink);
        draw.PathArcTo(P(12,6),4*s,MathF.PI,MathF.Tau,12);draw.PathStroke(ink,ImDrawFlags.None,2*s);
        draw.AddLine(P(8,6),P(8,7),ink,2*s);draw.AddLine(P(16,6),P(16,7),ink,2*s);
        var hole=new ImColor8(1,10,19);draw.AddCircleFilled(P(8,10),s+.4f,hole,10);draw.AddCircleFilled(P(16,10),s+.4f,hole,10);
    }

    private static void DrawLootGlyph(ImDrawListPtr draw,float2 center,ImColor8 ink)
    {
        const float s=19f/24;float2 P(float x,float y)=>center+new float2((x-12)*s,(y-12)*s);
        draw.AddEllipseFilled(P(12,11.5f),new(7*s,9.5f*s),ink,0,28);
        var hole=new ImColor8(1,10,19);
        draw.AddQuadFilled(P(12,6.5f),P(14,10),P(12,13.5f),P(10,10),hole);
        draw.PathArcTo(P(12,11.5f),5.4f*s,MathF.PI*.22f,MathF.PI*.78f,10);draw.PathStroke(hole,ImDrawFlags.None,1.6f*s);
    }

    // .cn-win: Settings and hide-friends in the top-right corner over the profile block.
    private void DrawClientWindowButtons(float width)
    {
        var draw=ImGui.GetWindowDrawList();float x=width-6-24;
        void Paint(bool on,Action<float2,ImColor8> glyph)
        {
            var lo=ImGui.GetItemRectMin();var hi=ImGui.GetItemRectMax();bool hot=on||ImGui.IsItemHovered();
            if(hot)draw.AddRectFilled(lo,hi,NativeUiPaint.Fade(new ImColor8(200,170,110,31)));
            glyph((lo+hi)*.5f,NativeUiPaint.Fade(hot?NativeUiTheme.GoldBright:NativeUiTheme.LcMute));
        }
        ImGui.SetCursorPos(new(x,4));
        if(SocialEnabled&&!SocialDocked)
        {
            if(ClientHitButton("socialToggle",24,22))socialDrawer=!socialDrawer;
            Paint(socialDrawer,(c,ink)=>DrawPeopleGlyph(draw,c,ink));if(ImGui.IsItemHovered())ResultTooltip("Social / friends");
        }
        else
        {
            if(ClientHitButton("socialCollapse",24,22))SaveClientPreferences(clientPreferences with {Social=!SocialEnabled});
            Paint(false,(c,ink)=>{if(SocialEnabled)draw.AddRectFilled(c+new float2(-5.5f,-.8f),c+new float2(5.5f,.8f),ink);else DrawPeopleGlyph(draw,c,ink);});
            if(ImGui.IsItemHovered())ResultTooltip(SocialEnabled?"Hide the friends list":"Show the friends list");
        }
        x-=2+24;ImGui.SetCursorPos(new(x,4));
        if(ClientHitButton("settings",24,22))NavigateClient("settings");
        Paint(showSettings,(c,ink)=>DrawGearGlyph(draw,c,ink));
        if(ImGui.IsItemHovered())ResultTooltip("Settings");
    }

    private static void DrawGearGlyph(ImDrawListPtr draw,float2 center,ImColor8 ink)
    {
        // 15 px gear: eight teeth around a ring with a hollow hub.
        for(int i=0;i<8;i++)
        {
            float a=i*MathF.PI/4;var d=new float2(MathF.Cos(a),MathF.Sin(a));var n=new float2(-d.Y,d.X);
            draw.AddQuadFilled(center+d*3.6f-n*1.2f,center+d*5.7f-n*1f,center+d*5.7f+n*1f,center+d*3.6f+n*1.2f,ink);
        }
        draw.AddCircle(center,3.5f,ink,20,2.2f);
    }

    private static void DrawPeopleGlyph(ImDrawListPtr draw,float2 center,ImColor8 ink)
    {
        draw.AddCircleFilled(center+new float2(-2,-3.5f),2.8f,ink,14);
        draw.PathArcTo(center+new float2(-2,4.5f),5.2f,MathF.PI,MathF.Tau,12);draw.PathFillConvex(ink);
        draw.AddRectFilled(center+new float2(4,-3.5f),center+new float2(5.2f,1.5f),ink);draw.AddRectFilled(center+new float2(2.1f,-1.6f),center+new float2(7.1f,-.4f),ink);
    }

    private void DrawClientBackdrop()
    {
        // The farm scene is drawn directly before ImGui; its transparent room
        // window must reveal that render rather than an opaque client backdrop.
        if(showFarm)return;
        var draw=ImGui.GetBackgroundDrawList();float width=socialWindowSize.X,height=socialWindowSize.Y;
        // #overlay: #010a13 with a warm glow top-left and a cyan glow top-right.
        draw.AddRectFilled(new(0,0),new(width,height),NativeUiTheme.Navy0);
        NativeUiPaint.Radial(draw,new(width*.15f,0),new float2(900,500)*.6f,new ImColor8(255,138,61,15),new ImColor8(255,138,61,0),48);
        NativeUiPaint.Radial(draw,new(width*.9f,height*.1f),new float2(900,600)*.6f,new ImColor8(63,214,255,18),new ImColor8(63,214,255,0),48);
        // #overlay::before: tex-hex at 256 px, opacity .55, masked by radial-gradient(ellipse at 50% 40%, #000 20%, transparent 85%).
        var center=new float2(width*.5f,height*.4f);var radius=new float2(width*.5f,height*.6f)*MathF.Sqrt(2);
        homeArtwork.DrawTiled("tex-hex",new(0,0),new(width,height),256,.55f,point=>
        {
            var d=(point-center)/radius;float t=MathF.Sqrt(d.X*d.X+d.Y*d.Y);
            return Math.Clamp((.85f-t)/(.85f-.2f),0,1);
        },draw);
    }

    // .cn-profile: level ring with avatar and badge, name and presence; the window buttons sit over its corner.
    private void DrawClientIdentity(ClientNavMetrics nav,float2 at,float2 extent)
    {
        var draw=ImGui.GetWindowDrawList();
        ImGui.SetCursorScreenPos(at);ImGui.BeginDisabled(profile is null);
        // The Settings and hide-friends buttons (.cn-win) sit over the right edge; keep the profile hit area clear of them.
        bool clicked=ClientHitButton("profile",extent.X-58,extent.Y);bool hovered=ImGui.IsItemHovered();
        ImGui.EndDisabled();
        NativeUiPaint.VerticalGradient(draw,at,at+extent,showProfile?new ImColor8(8,24,42):hovered?new ImColor8(6,18,31):NativeUiTheme.Navy0,showProfile?new ImColor8(13,34,54):hovered?new ImColor8(11,26,44):new ImColor8(6,16,29));
        draw.AddLine(at,at+new float2(0,extent.Y),NativeUiPaint.Fade(NativeUiTheme.GoldDim));
        var center=at+new float2(nav.ProfilePadding+27,extent.Y/2);
        // .cn-av .pt: 42 px circle, radial-gradient(circle at 50% 30%, #2a3a5c, #0a1428).
        draw.AddCircleFilled(center,21,NativeUiPaint.Fade(NativeUiTheme.Navy1),40);
        NativeUiPaint.Radial(draw,center-new float2(0,8.4f),new(29),new ImColor8(42,58,92),new ImColor8(10,20,40,0),40);
        draw.AddCircle(center,22.5f,NativeUiPaint.Fade(NativeUiTheme.Navy0),40,3);
        if(profile is not null)
        {
            var wallet=new CosmeticCollection(profile.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            string? selected=ProfileSummary.Read(profile.Storage.GetValueOrDefault("dt.client.v1"),profile.Storage.GetValueOrDefault("dt.cosmetics.v1")).Avatar;
            string id=selected is not null&&wallet.Owns("legend",selected)?selected:wallet.Equipped("legend")?.GetValue<string>()??"tokomon";
            var discovered=LegendEvolution.Discovery(profile.Storage.GetValueOrDefault("dt.legendevo.v1"),id);
            portraits.DrawEvolution(discovered.Current,center-new float2(21,21),42,true);
        }
        // The 54 px progress ring: --lc-gold-5 track and a glowing --lc-gold-2 arc from twelve o'clock.
        var level=TamerProgress.Read(profile?.Storage.GetValueOrDefault("dt.client.v1"));
        const float ring=24.75f;draw.AddCircle(center,ring,NativeUiPaint.Fade(NativeUiTheme.GoldDim),48,3.4f);
        float progress=level.Need>0?Math.Clamp((float)level.Xp/level.Need,0,1):0;
        if(progress>0)
        {
            float start=-MathF.PI/2,end=start+progress*MathF.Tau;
            draw.PathArcTo(center,ring,start,end,48);draw.PathStroke(NativeUiPaint.Fade(new ImColor8(240,210,140,70)),ImDrawFlags.None,6);
            draw.PathArcTo(center,ring,start,end,48);draw.PathStroke(NativeUiPaint.Fade(NativeUiTheme.Gold),ImDrawFlags.None,3.4f);
        }
        string levelText=level.Level.ToString();
        var badgeText=NativeUiPaint.TrackedSize(NativeUiTheme.DisplayBoldFont,10,levelText,0);
        float badgeWidth=Math.Max(22,badgeText.X+12);
        var badgeLo=new float2(MathF.Round(center.X-badgeWidth/2),MathF.Round(center.Y+27+4-17));var badgeHi=badgeLo+new float2(badgeWidth,17);
        draw.AddRectFilled(badgeLo,badgeHi,NativeUiPaint.Fade(NativeUiTheme.Navy0));draw.AddRect(badgeLo,badgeHi,NativeUiPaint.Fade(NativeUiTheme.GoldDeep));
        NativeUiPaint.Tracked(draw,NativeUiTheme.DisplayBoldFont,10,new(MathF.Round(center.X-badgeText.X/2),MathF.Round(badgeLo.Y+(17-badgeText.Y)/2)),NativeUiTheme.GoldBright,levelText);
        if(nav.ProfileText)
        {
            float textLeft=center.X+27+(socialWindowSize.X<=1380?8:10),textRight=at.X+extent.X-44;
            var nameSize=NativeUiPaint.TrackedSize(NativeUiTheme.DisplayBoldFont,14,"Tg",.04f);
            float top=MathF.Round(extent.Y/2-(nameSize.Y+2+14)/2);
            NativeUiPaint.Tracked(draw,NativeUiTheme.DisplayBoldFont,14,new(textLeft,at.Y+top),NativeUiTheme.GoldBright,LobbyEllipsis(ClientDisplayName,textRight-textLeft),.04f);
            var presenceColor=steam?.Room is null?NativeUiTheme.Success:new ImColor8(106,185,255);var presenceDot=steam?.Room is null?NativeUiTheme.Success:new ImColor8(79,166,255);
            var dot=new float2(textLeft+3.5f,at.Y+top+nameSize.Y+2+7);
            NativeUiPaint.Radial(draw,dot,new(7),NativeUiPaint.Alpha(presenceDot,150),NativeUiPaint.Alpha(presenceDot,0),20);
            draw.AddCircleFilled(dot,3.5f,NativeUiPaint.Fade(presenceDot),16);
            var presence=NativeUiPaint.TrackedSize(NativeUiTheme.BodyFont,11,ClientPresenceLabel,0);
            NativeUiPaint.Tracked(draw,NativeUiTheme.BodyFont,11,new(dot.X+3.5f+6,MathF.Round(dot.Y-presence.Y/2)),presenceColor,ClientPresenceLabel);
        }
        if(clicked)NavigateClient("profile");
    }
}

using System.Text.Json;
using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Networking;
using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private float SocialWidth => socialWindowSize.X<=1500?210:232;
    private int2 socialWindowSize;
    private bool socialDrawer;
    private int socialDrawnModule,socialDisplayedModule;
    private long socialPollAt;
    private long socialProjectionAt;
    private SteamRoomSession? socialConnection;
    private IReadOnlyList<SteamFriendPresence> socialFriends = [];
    private IReadOnlyList<SocialRow> socialRows = [];
    private readonly Dictionary<SocialGroup, SocialRow[]> socialGroups = [];
    private string? socialError;
    private readonly HashSet<SocialGroup> socialOpen = [SocialGroup.Party, SocialGroup.Online, SocialGroup.InGame];
    private bool SocialDocked => SocialEnabled && socialWindowSize.X >= 1200;
    internal IReadOnlyList<SocialRow> DisplayedSocialRows { get; private set; } = [];
    internal bool SocialVisible { get; private set; }
    internal float SocialBodyFontSize { get; private set; }
    internal float SocialPanelTop { get; private set; }

    private int2 ClientContentSize(int2 windowSize)
    {
        socialWindowSize = windowSize;
        // Capture the module being drawn, before a click can navigate to the
        // next one. Its newly created window must stay below the open drawer.
        socialDrawnModule=codexFull?1:showSettings?2:showHomePlay?9:showSaveData?3:showProfile?4:showMultiplayer?5:showFarm?6:showCollection?7:showAbout?8:0;
        return ClientHomeVisible && SocialDocked ? new(windowSize.X - (int)SocialWidth, windowSize.Y) : windowSize;
    }
    private void DrawClientSocial()
    {
        var contentSize = size;
        size = socialWindowSize;
        try { DrawSocialPanel(home: true); }
        finally { size = contentSize; }
    }

    private void SocialNavigation()
    {
        if (!SocialEnabled || SocialDocked) return;
        if (HomeButton("socialToggle", socialDrawer ? "Hide friends" : "Friends", 122, 36)) socialDrawer = !socialDrawer;
    }

    private void RefreshSocial()
    {
        if (!ReferenceEquals(socialConnection, steam))
        { socialConnection = steam; socialFriends = []; socialPollAt = socialProjectionAt = 0; socialError = null; }
        if (steam is null || Environment.TickCount64 < socialPollAt) return;
        socialPollAt = Environment.TickCount64 + 8000;
        try
        {
            socialFriends = steam.Friends.ListPresence();
            socialProjectionAt = 0;
            listingFriendRooms ??= steam.Friends.ListRooms();
            socialError = null;
        }
        catch (Exception error) when (error is InvalidOperationException or IOException or EntryPointNotFoundException)
        { socialFriends = []; socialError = "Friends could not be refreshed: " + error.Message; }
    }

    private IReadOnlyList<SocialPartyMember> SocialParty()
    {
        if (steam?.Room is not { } room || room.Authority.Lobby is not { } lobby) return [];
        var slots = lobby["slots"] as JsonObject;
        var presence = room.Authority.Presence;
        var ids = room.Authority.Members.Concat(slots?.Select(slot => slot.Value?["uid"]?.GetValue<string>() ?? "") ?? []).Distinct();
        return ids.Where(id => ulong.TryParse(id, out _)).Select(id =>
        {
            string name = presence[id]?["name"]?.GetValue<string>() ??
                slots?.FirstOrDefault(slot => slot.Value?["uid"]?.GetValue<string>() == id).Value?["name"]?.GetValue<string>() ?? "Tamer";
            return new SocialPartyMember(ulong.Parse(id), name, id == lobby["host"]?.GetValue<string>(), id == room.Authority.LocalId, room.Authority.Members.Contains(id));
        }).ToArray();
    }

    private void DrawSocialPanel(bool home = false)
    {
        bool wasVisible = SocialVisible;
        SocialVisible = false; DisplayedSocialRows = [];
        if (!ClientHomeVisible || !SocialEnabled) { socialDrawer = false; return; }
        bool docked = home && SocialDocked;
        if (!docked && !socialDrawer) return;
        RefreshSocial();
        try
        {
            if (Environment.TickCount64 >= socialProjectionAt)
            {
                socialProjectionAt = Environment.TickCount64 + 250;
                ulong.TryParse(steam?.Room?.Authority.RoomId, out ulong lobby);
                socialRows = SocialRoster.Build(steam?.SteamId ?? 0, SteamName, lobby, false, SocialParty(), socialFriends, friendRooms,
                    RecentPlayers.Read(profile?.Storage.GetValueOrDefault("dt.client.v1")));
                socialGroups.Clear();
                foreach (var group in socialRows.GroupBy(row => row.Group)) socialGroups[group.Key] = group.ToArray();
            }
            DisplayedSocialRows = socialRows;
        }
        catch (Exception error) when (error is JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException)
        { socialError = "The friends list could not be displayed: " + error.Message; }
        SocialVisible = true;
        float top = ClientPageTop;
        if (!docked && (!wasVisible || socialDisplayedModule!=socialDrawnModule)) ImGui.SetNextWindowFocus();
        socialDisplayedModule=socialDrawnModule;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize,0);
        HomeCard("Friends", new(size.X - SocialWidth, top), new(SocialWidth, size.Y - top), false, bringToFront: !docked);
        ImGui.PopStyleVar(2);
        var at=new float2(size.X-SocialWidth,top);var extent=new float2(SocialWidth,size.Y-top);var draw=ImGui.GetWindowDrawList();
        // #csocial: linear-gradient(180deg, #06101d, #010a13 40%) with a gold-5 left rule.
        NativeUiPaint.VerticalGradient(draw,at,at+new float2(extent.X,extent.Y*.4f),new ImColor8(6,16,29),NativeUiTheme.Navy0);
        draw.AddRectFilled(at+new float2(0,extent.Y*.4f),at+extent,NativeUiPaint.Fade(NativeUiTheme.Navy0));
        draw.AddLine(at,at+new float2(0,extent.Y),NativeUiPaint.Fade(NativeUiTheme.GoldDim));
        ImGui.BeginDisabled(ClientNavigationBlocked);
        SocialBodyFontSize=NativeUiTheme.Css(NativeUiTheme.BodyFont,11);SocialPanelTop=top;
        // .cs-head: 8px 10px 8px 14px, Chakra 700 11px, .18em, gold-2, 26 px icon buttons, gold-5 rule.
        const float headHeight=42;
        var headText=NativeUiPaint.TrackedSize(NativeUiTheme.DisplayBoldFont,11,"SOCIAL",.18f);
        NativeUiPaint.Tracked(draw,NativeUiTheme.DisplayBoldFont,11,at+new float2(14,MathF.Round((headHeight-headText.Y)/2)),NativeUiTheme.Gold,"SOCIAL",.18f);
        draw.AddLine(at+new float2(0,headHeight-.5f),at+new float2(extent.X,headHeight-.5f),NativeUiPaint.Fade(NativeUiTheme.GoldDim));
        float buttonX=extent.X-10-28;
        void HeadButton(string key,string tip,Action<float2,ImColor8> glyph,Action act)
        {
            ImGui.SetCursorScreenPos(at+new float2(buttonX,8));
            if(ClientHitButton(key,28,26))act();
            bool hot=ImGui.IsItemHovered();
            if(hot){draw.AddRect(ImGui.GetItemRectMin(),ImGui.GetItemRectMax(),NativeUiPaint.Fade(NativeUiTheme.GoldBorder));ResultTooltip(tip);}
            glyph((ImGui.GetItemRectMin()+ImGui.GetItemRectMax())*.5f,NativeUiPaint.Fade(NativeUiTheme.Gold));
            buttonX-=4+28;
        }
        if(!docked)HeadButton("closeSocial","Close",(c,ink)=>{draw.AddLine(c-new float2(4.5f,4.5f),c+new float2(4.5f,4.5f),ink,1.6f);draw.AddLine(c+new float2(-4.5f,4.5f),c+new float2(4.5f,-4.5f),ink,1.6f);},()=>socialDrawer=false);
        if(steam is null)HeadButton("socialConnect","Connect to Steam friends",(c,ink)=>DrawInviteGlyph(draw,c,ink),ConnectSteam);
        else HeadButton("socialRefresh","Refresh friends",(c,ink)=>{draw.PathArcTo(c,5,-MathF.PI*.15f,MathF.PI*1.45f,16);draw.PathStroke(ink,ImDrawFlags.None,1.6f);
            draw.AddTriangleFilled(c+new float2(5.6f,-5.2f),c+new float2(5.6f,.4f),c+new float2(.6f,-2.2f),ink);},()=>{socialPollAt=0;RefreshSocial();});
        // .cs-foot: 10px 12px, gold-5 top rule, rgba(1,10,19,.85).
        // .cs-foot: 10px padding around a 12.5px/11px two-line block, plus its 1px rule.
        float footHeight=MathF.Ceiling(20+1+NativeUiTheme.TextLine(12.5f)+NativeUiTheme.TextLine(11));
        ImGui.SetCursorScreenPos(at+new float2(0,headHeight));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(0,0));
        ImGui.BeginChild("Social people"u8,new(extent.X,extent.Y-headHeight-footHeight),ImGuiChildFlags.None,ImGuiWindowFlags.NoBackground);
        var body=ImGui.GetWindowDrawList();
        ImGui.SetCursorPosY(4);
        foreach (var group in Enum.GetValues<SocialGroup>())
        {
            var rows = socialGroups.GetValueOrDefault(group) ?? [];
            if (group == SocialGroup.Party && steam?.Room is null || group==SocialGroup.Recent&&rows.Length==0) continue;
            string title = group switch { SocialGroup.InGame => "In Game", SocialGroup.Recent => "Recent players",
                SocialGroup.Party => steam?.Room?.Authority.Lobby?["status"]?.GetValue<string>()=="playing" ? "Match" : "Party", _ => group.ToString() };
            bool open = socialOpen.Contains(group);
            // .cs-sh: 7px 12px, caret, Chakra 700 11px .14em lc-mute; the count in c-text-3 at .04em.
            ImGui.SetCursorPosY(ImGui.GetCursorPosY()+2);
            var rowAt=ImGui.GetCursorScreenPos();float rowHeight=14+NativeUiTheme.LineHeight(NativeUiTheme.DisplayBoldFont,11);
            bool toggled=ClientHitButton("socialGroup/"+group,extent.X,rowHeight);
            SelectionButton("socialGroup/"+group);
            bool hot=ImGui.IsItemHovered();
            if(hot)body.AddRectFilled(rowAt,rowAt+new float2(extent.X,rowHeight),NativeUiPaint.Fade(new ImColor8(200,170,110,15)));
            var ink=hot?NativeUiTheme.GoldBright:NativeUiTheme.LcMute;
            NativeUiPaint.Caret(body,rowAt+new float2(12+2.5f,rowHeight/2),open,ink);
            string heading=title.ToUpperInvariant();
            var headingSize=NativeUiPaint.TrackedSize(NativeUiTheme.DisplayBoldFont,11,heading,.14f);
            float textY=rowAt.Y+7;
            NativeUiPaint.Tracked(body,NativeUiTheme.DisplayBoldFont,11,new(rowAt.X+12+5+6,textY),ink,heading,.14f);
            float space=NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayBoldFont,11," ",.14f);
            NativeUiPaint.Tracked(body,NativeUiTheme.DisplayBoldFont,11,new(rowAt.X+12+5+6+headingSize.X+space,textY),NativeUiTheme.Text3,"("+rows.Length+")",.04f);
            if (toggled)
            { if (open) socialOpen.Remove(group); else socialOpen.Add(group); open = !open; }
            if (!open) continue;
            if (rows.Length == 0)
            {
                string empty=group switch
                {
                    SocialGroup.Online => steam is null ? "Open Multiplayer to see who is in a lobby." : "No Steam friends in a Digi-Tactics lobby right now.",
                    SocialGroup.InGame => "Nobody is mid-game.", SocialGroup.Offline or SocialGroup.Recent => "Players from your lobbies show up here.", _ => "Nobody here yet"
                };
                // .cs-empty: 4px 14px 8px 26px, 11px, c-text-3, line-height 1.4.
                var emptyAt=ImGui.GetCursorScreenPos()+new float2(26,4);
                float emptyHeight=CssParagraph(body,NativeUiTheme.BodyFont,11,emptyAt,extent.X-26-14,NativeUiTheme.Text3,empty,1.4f);
                ImGui.Dummy(new(extent.X,4+emptyHeight+8));
            }
            foreach (var row in rows) DrawSocialRow(row, top);
        }
        ImGui.EndChild();ImGui.PopStyleVar();
        var footAt=at+new float2(0,extent.Y-footHeight);
        draw.AddRectFilled(footAt,at+extent,NativeUiPaint.Fade(new ImColor8(1,10,19,217)));
        draw.AddLine(footAt+new float2(1,.5f),footAt+new float2(extent.X,.5f),NativeUiPaint.Fade(NativeUiTheme.GoldDim));
        string roomTitle,roomNote,action;
        if (socialError is not null) {roomTitle="Friends unavailable";roomNote=socialError;action="Lobbies";}
        else if(steam?.Room?.Authority.Lobby is {} lobby)
        {
            roomTitle=lobby["name"]?.GetValue<string>()??"Your party";
            roomNote=lobby["status"]?.GetValue<string>()=="playing"?"Game in progress":$"{(lobby["slots"] as JsonObject)?.Count??0}/8 players";action="Open";
        }
        else if(steam is not null){roomTitle="Steam";roomNote="Friends come from your Steam friends list";action="Lobbies";}
        else {roomTitle="Not in a lobby";roomNote="Create a lobby to invite friends";action="Lobbies";}
        // .cs-fbtn: 5px 10px, Chakra 700 11px .1em uppercase, default button frame.
        string actionText=action.ToUpperInvariant();
        float actionWidth=NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayBoldFont,11,actionText,.1f)+20+2;
        float actionHeight=MathF.Round(11*1.3f)+10+2;
        float textRight=extent.X-12-actionWidth-8;
        var titleSize=new float2(0,NativeUiTheme.TextLine(12.5f));var noteSize=new float2(0,NativeUiTheme.TextLine(11));
        float textTop=MathF.Round(footAt.Y+(footHeight-titleSize.Y-noteSize.Y)/2);
        NativeUiPaint.Tracked(draw,NativeUiTheme.BodyBoldFont,12.5f,new(footAt.X+12,textTop),NativeUiTheme.GoldBright,SocialEllipsis(roomTitle,NativeUiTheme.BodyBoldFont,12.5f,textRight-12));
        NativeUiPaint.Tracked(draw,NativeUiTheme.BodyFont,11,new(footAt.X+12,textTop+titleSize.Y),NativeUiTheme.Text3,SocialEllipsis(roomNote,NativeUiTheme.BodyFont,11,textRight-12));
        if(socialError is not null&&ImGui.IsMouseHoveringRect(footAt,footAt+new float2(textRight,footHeight)))ResultTooltip(socialError);
        ImGui.SetCursorScreenPos(new(footAt.X+extent.X-12-actionWidth,MathF.Round(footAt.Y+(footHeight-actionHeight)/2)));
        if (ClientButton("socialLobby", actionText, actionWidth, actionHeight, ClientButtonKind.Default, 11, tracking: .1f, font: NativeUiTheme.DisplayBoldFont))
        { NavigateClient("multiplayer"); }
        ImGui.EndDisabled();ImGui.End();
    }

    /// <summary>CSS text-overflow: ellipsis: the longest prefix that fits, followed by U+2026.</summary>
    private static string SocialEllipsis(string value,ImFontPtr font,float px,float width,float tracking=0)
    {
        if(NativeUiPaint.TrackedWidth(font,px,value,tracking)<=width)return value;
        for(int length=value.Length-1;length>0;length--)
        {
            string candidate=value[..length]+"…";
            if(NativeUiPaint.TrackedWidth(font,px,candidate,tracking)<=width)return candidate;
        }
        return "…";
    }

    private static void DrawInviteGlyph(ImDrawListPtr draw,float2 center,ImColor8 ink)
    {
        // The original 16 px person-plus icon.
        float s=16f/24;float2 P(float x,float y)=>center+new float2((x-12)*s,(y-12)*s);
        draw.AddCircleFilled(P(10,8),4*s,ink,16);
        draw.PathArcTo(P(10,20),7*s,MathF.PI,MathF.Tau,14);draw.PathFillConvex(ink);
        draw.AddRectFilled(P(17,5),P(19,13),ink);draw.AddRectFilled(P(14,8),P(22,10),ink);
    }

    // .cs-row: 44 px, 34 px avatar ring with presence dot, 12.5px/700 name and 11px state; the action shows on hover.
    private void DrawSocialRow(SocialRow row, float top)
    {
        var at = ImGui.GetCursorScreenPos();
        float width=ImGui.GetWindowWidth(),height=44;
        at.X=ImGui.GetWindowPos().X;
        bool visible = at.Y > top - height && at.Y < size.Y - 24;
        var draw=ImGui.GetWindowDrawList();
        var stateColor = !row.Online ? NativeUiTheme.Text3 : row.Group == SocialGroup.InGame ? new ImColor8(106,185,255) : NativeUiTheme.Success;
        var dotColor = !row.Online ? NativeUiTheme.Text3 : row.Group == SocialGroup.InGame ? new ImColor8(79,166,255) : NativeUiTheme.Success;
        bool hovered=ImGui.IsMouseHoveringRect(at,at+new float2(width,height));
        if(hovered)NativeUiPaint.HorizontalGradient(draw,at,at+new float2(width,height),new ImColor8(200,170,110,26),new ImColor8(200,170,110,0));
        float alpha=row.Online?1:.6f;
        if (visible)
        {
            var center=at+new float2(14+17,22);
            draw.AddCircleFilled(center,17,NativeUiPaint.Fade(NativeUiTheme.Navy1,alpha),32);
            NativeUiPaint.Radial(draw,center-new float2(0,7),new(24),new ImColor8(42,58,92,(byte)(255*alpha)),new ImColor8(10,20,40,0),32);
            ImGui.SetCursorScreenPos(center-new float2(17,17));DrawLobbyAvatar(row.Id == 0 ? null : row.Id.ToString(),34,false);
            if (!lobbyAvatars.TryGetValue(row.Id, out var cached) || cached.Texture is null)
            {
                string initials=row.Name[..Math.Min(2,row.Name.Length)].ToUpperInvariant();
                var initialSize=NativeUiPaint.TrackedSize(NativeUiTheme.DisplayBoldFont,11,initials,0);
                NativeUiPaint.Tracked(draw,NativeUiTheme.DisplayBoldFont,11,center-initialSize*.5f,NativeUiPaint.Alpha(NativeUiTheme.GoldBright,(byte)(255*alpha)),initials);
            }
            draw.AddCircle(center,17,NativeUiPaint.Fade(NativeUiTheme.GoldBorder,alpha),32,1);
            var dot=center+new float2(14,13);
            draw.AddCircleFilled(dot,5,NativeUiPaint.Fade(NativeUiTheme.Navy0),16);draw.AddCircleFilled(dot,3,NativeUiPaint.Fade(dotColor,alpha),16);
        }
        float textLeft=at.X+14+34+9,actionWidth=row.JoinLobby is null?0:NativeUiPaint.TrackedWidth(NativeUiTheme.DisplayBoldFont,10.5f,"JOIN",.08f)+18;
        float textWidth=width-(textLeft-at.X)-10-(actionWidth>0?actionWidth+9:0);
        var nameSize=new float2(0,NativeUiTheme.TextLine(12.5f));var noteSize=new float2(0,NativeUiTheme.TextLine(11));
        float textTop=MathF.Round(at.Y+(height-nameSize.Y-noteSize.Y)/2);
        var nameColor=row.Id!=0&&row.Id==steam?.SteamId?NativeUiTheme.Accent:NativeUiTheme.GoldBright;
        NativeUiPaint.Tracked(draw,NativeUiTheme.BodyBoldFont,12.5f,new(textLeft,textTop),NativeUiPaint.Alpha(nameColor,(byte)(255*alpha)),SocialEllipsis(row.Name,NativeUiTheme.BodyBoldFont,12.5f,textWidth));
        NativeUiPaint.Tracked(draw,NativeUiTheme.BodyFont,11,new(textLeft,textTop+nameSize.Y),NativeUiPaint.Alpha(stateColor,(byte)(255*alpha)),SocialEllipsis(row.Note,NativeUiTheme.BodyFont,11,textWidth));
        if(hovered&&(NativeUiPaint.TrackedWidth(NativeUiTheme.BodyBoldFont,12.5f,row.Name,0)>textWidth||NativeUiPaint.TrackedWidth(NativeUiTheme.BodyFont,11,row.Note,0)>textWidth))
            ResultTooltip(row.Name+"\n"+row.Note);
        if (row.JoinLobby is { } lobby)
        {
            float actionHeight=NativeUiTheme.LineHeight(NativeUiTheme.DisplayBoldFont,10.5f)+6+2;
            ImGui.SetCursorScreenPos(new(at.X+width-10-actionWidth,MathF.Round(at.Y+(height-actionHeight)/2)));
            ImGui.BeginDisabled(!row.CanJoin || enteringRoom is not null || ClientNavigationBlocked);
            // .cs-act stays transparent until its row is hovered or it takes keyboard focus.
            float previous=ImGui.GetStyle().Alpha;
            bool shown=hovered||KeyboardFocusedButton=="socialJoin/" + row.Id;
            if(!shown)ImGui.GetStyle().Alpha=0;
            bool join=ClientButton("socialJoin/" + row.Id, "JOIN", actionWidth, actionHeight, ClientButtonKind.Default, 10.5f, tracking: .08f, font: NativeUiTheme.DisplayBoldFont);
            ImGui.GetStyle().Alpha=previous;
            if (join) { JoinSteamRoom(lobby); NavigateClient("multiplayer"); }
            ImGui.EndDisabled();
            if (!row.CanJoin && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) ResultTooltip("Leave your current party before joining another.");
        }
        ImGui.SetCursorScreenPos(at+new float2(0,height));ImGui.Dummy(new(width,0));
    }

}

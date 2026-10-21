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
        HomeCard("Friends", new(size.X - SocialWidth, top), new(SocialWidth, size.Y - top), false, bringToFront: !docked);
        ImGui.BeginDisabled(ClientNavigationBlocked);
        ImGui.PushFont(NativeUiTheme.BodyFont,17);ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(8,8));
        SocialBodyFontSize=17;SocialPanelTop=top;
        HomeLabel("SOCIAL",14,HomeGold);
        ImGui.SetCursorPos(new(SocialWidth-(docked?44:76),8));
        if(steam is null)
        {if(ClientTextButton("socialConnect","+",28,28,false,19))ConnectSteam();if(ImGui.IsItemHovered())ResultTooltip("Connect to Steam friends");}
        else
        {if(ClientTextButton("socialRefresh","R",28,28,false,14)){socialPollAt=0;RefreshSocial();}if(ImGui.IsItemHovered())ResultTooltip("Refresh friends");}
        if(!docked){ImGui.SameLine(0,4);if(ClientTextButton("closeSocial","X",28,28,false,14))socialDrawer=false;}
        ImGui.SetCursorPos(new(12,46));ImGui.Separator();
        ImGui.BeginChild("Social people"u8,new(0,-92));
        foreach (var group in Enum.GetValues<SocialGroup>())
        {
            var rows = socialGroups.GetValueOrDefault(group) ?? [];
            if (group == SocialGroup.Party && steam?.Room is null || group==SocialGroup.Recent&&rows.Length==0) continue;
            string title = group switch { SocialGroup.InGame => "In Game", SocialGroup.Recent => "Recent players", _ => group.ToString() };
            bool open = socialOpen.Contains(group);
            ImGui.PushStyleColor(ImGuiCol.Header,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.HeaderHovered,new ImColor8(200,170,110,24));
            bool toggled=ImGui.Selectable((open ? "v  " : ">  ") + title.ToUpperInvariant() + " (" + rows.Length + ")",false,ImGuiSelectableFlags.None,new(0,36));
            SelectionButton("socialGroup/"+group);ClientItemSound("socialGroup/"+group,toggled);ImGui.PopStyleColor(2);
            if (toggled)
            { if (open) socialOpen.Remove(group); else socialOpen.Add(group); open = !open; }
            if (!open) continue;
            if (rows.Length == 0) {ImGui.PushTextWrapPos(0);HomeLabel(group switch
            {
                SocialGroup.Online => "No friends online right now.", SocialGroup.InGame => "Nobody is mid-game.",
                SocialGroup.Offline => "No offline friends.", SocialGroup.Recent => "Players you meet appear here.", _ => "Nobody here yet."
            }, 15, HomeMute);ImGui.PopTextWrapPos();}
            foreach (var row in rows) DrawSocialRow(row, top);
        }
        ImGui.EndChild(); ImGui.Separator();
        if (socialError is not null) { HomeLabel("Friends unavailable",16,HomeMute); if(ImGui.IsItemHovered())ResultTooltip(socialError); }
        else if(steam?.Room?.Authority.Lobby is {} lobby)
        {
            HomeLabel(LobbyEllipsis(lobby["name"]?.GetValue<string>()??"Your party",ImGui.GetContentRegionAvail().X),17,HomeInk);
            HomeLabel(lobby["status"]?.GetValue<string>()=="playing"?"In game":$"{(lobby["slots"] as JsonObject)?.Count??0}/8 Tamers seated",16,HomeMute);
        }
        else {HomeLabel("Not in a lobby",15,HomeInk);}
        ImGui.BeginDisabled(ClientNavigationBlocked);
        if (HomeButton("socialLobby", steam?.Room is null ? "Lobbies" : "Open party", ImGui.GetContentRegionAvail().X, 38))
        { NavigateClient("multiplayer"); }
        ImGui.EndDisabled();
        ImGui.PopStyleVar();ImGui.PopFont();ImGui.EndDisabled();ImGui.End();
    }

    private void DrawSocialRow(SocialRow row, float top)
    {
        var at = ImGui.GetCursorScreenPos();
        float width=ImGui.GetContentRegionAvail().X,height=row.JoinLobby is null?62:102;
        bool visible = at.Y > top - height && at.Y < size.Y - 24;
        var draw=ImGui.GetWindowDrawList();var rowColor=row.Online?HomeInk:HomeMute;
        var stateColor = !row.Online ? HomeMute : row.Group == SocialGroup.InGame ? new ImColor8(79,166,255) : new ImColor8(112,196,132);
        if(ImGui.IsMouseHoveringRect(at,at+new float2(width,height)))draw.AddRectFilledMultiColor(at,at+new float2(width,height),new ImColor8(200,170,110,22),new ImColor8(200,170,110,0),new ImColor8(200,170,110,0),new ImColor8(200,170,110,22));
        if (visible)
        {
            DrawLobbyAvatar(row.Id == 0 ? null : row.Id.ToString(),40,false);
            if (!lobbyAvatars.TryGetValue(row.Id, out var cached) || cached.Texture is null)
            {string initials=row.Name[..Math.Min(2,row.Name.Length)].ToUpperInvariant();draw.AddText(at+(new float2(40)-ImGui.CalcTextSize(initials))*.5f,HomeGold,initials);}
            draw.AddCircleFilled(at+new float2(36,36),6,new ImColor8(1,10,19));draw.AddCircleFilled(at+new float2(36,36),4,stateColor);
        }
        ImGui.SetCursorScreenPos(at+new float2(50,0));HomeLabel(LobbyEllipsis(row.Name,width-50),17,rowColor);
        if(ImGui.IsItemHovered())ResultTooltip(row.Name);
        ImGui.SetCursorScreenPos(at+new float2(50,25));HomeLabel(LobbyEllipsis(row.Note,width-50),16,stateColor);
        if(ImGui.IsItemHovered())ResultTooltip(row.Note);
        if (row.JoinLobby is { } lobby)
        {
            ImGui.SetCursorScreenPos(at+new float2(50,52));
            ImGui.BeginDisabled(!row.CanJoin || enteringRoom is not null || ClientNavigationBlocked);
            if (HomeButton("socialJoin/" + row.Id, "Join lobby", width-50, 36)) { JoinSteamRoom(lobby); NavigateClient("multiplayer"); }
            ImGui.EndDisabled();
            if (!row.CanJoin && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) ResultTooltip("Leave your current party before joining another.");
        }
        ImGui.SetCursorScreenPos(at+new float2(0,height));ImGui.Dummy(new(width,1));
    }
}

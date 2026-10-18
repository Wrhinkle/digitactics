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
    private const float SocialWidth = 280;
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

    private int2 ClientContentSize(int2 windowSize)
    {
        socialWindowSize = windowSize;
        // Capture the module being drawn, before a click can navigate to the
        // next one. Its newly created window must stay below the open drawer.
        socialDrawnModule=codexFull?1:showSettings?2:showSaveData?3:showProfile?4:showMultiplayer?5:showFarm?6:showCollection?7:showAbout?8:0;
        return ClientHomeVisible && SocialDocked ? new(windowSize.X - (int)SocialWidth - 12, windowSize.Y) : windowSize;
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
        ImGui.SameLine();
        if (HomeButton("socialToggle", socialDrawer ? "Hide friends" : "Friends", 100, 32)) socialDrawer = !socialDrawer;
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
        float top = docked ? 16 : 124;
        if (!docked && (!wasVisible || socialDisplayedModule!=socialDrawnModule)) ImGui.SetNextWindowFocus();
        socialDisplayedModule=socialDrawnModule;
        HomeCard("Friends", new(size.X - SocialWidth - 12, top), new(SocialWidth, size.Y - top - 24), false, bringToFront: !docked);
        HomeLabel("FRIENDS", 18, HomeInk);
        if (!docked)
        { ImGui.SameLine(); if (HomeButton("closeSocial", "Close", 70, 24)) socialDrawer = false; }
        HomeLabel(steam is null ? "Steam is not connected" : "Steam friends", 12, HomeMute);
        if (steam is null)
        { if (HomeButton("socialConnect", "Connect to Steam", 160, 28)) ConnectSteam(); }
        else if (HomeButton("socialRefresh", "Refresh", 90, 26)) { socialPollAt = 0; RefreshSocial(); }
        ImGui.Separator();
        ImGui.BeginChild("Social people"u8, new(0, -70));
        foreach (var group in Enum.GetValues<SocialGroup>())
        {
            var rows = socialGroups.GetValueOrDefault(group) ?? [];
            if (group == SocialGroup.Party && steam?.Room is null) continue;
            string title = group switch { SocialGroup.InGame => "In Game", SocialGroup.Recent => "Recent players", _ => group.ToString() };
            bool open = socialOpen.Contains(group);
            if (HomeButton("socialGroup/" + group, (open ? "- " : "+ ") + title + " (" + rows.Length + ")", ImGui.GetContentRegionAvail().X, 28))
            { if (open) socialOpen.Remove(group); else socialOpen.Add(group); open = !open; }
            if (!open) continue;
            if (rows.Length == 0) HomeLabel(group switch
            {
                SocialGroup.Online => "No friends online right now.", SocialGroup.InGame => "Nobody is mid-game.",
                SocialGroup.Offline => "No offline friends.", SocialGroup.Recent => "Players you meet appear here.", _ => "Nobody here yet."
            }, 12, HomeMute);
            foreach (var row in rows) DrawSocialRow(row, top);
        }
        ImGui.EndChild(); ImGui.Separator();
        if (socialError is not null) UiText.Wrapped(socialError);
        if (HomeButton("socialLobby", steam?.Room is null ? "Open multiplayer" : "Open party", 170, 28))
        { OpenMultiplayerView(); }
        ImGui.End();
    }

    private void DrawSocialRow(SocialRow row, float top)
    {
        var at = ImGui.GetCursorScreenPos();
        bool visible = at.Y > top - 48 && at.Y < size.Y - 24;
        if (visible)
        {
            DrawLobbyAvatar(row.Id == 0 ? null : row.Id.ToString());
            if (!lobbyAvatars.TryGetValue(row.Id, out var cached) || cached.Texture is null)
                ImGui.GetWindowDrawList().AddText(at + new float2(3, 5), HomeGold, row.Name[..Math.Min(2, row.Name.Length)].ToUpperInvariant());
            var color = !row.Online ? HomeMute : row.Group == SocialGroup.InGame ? new ImColor8(112,196,132) : new ImColor8(79,187,220);
            ImGui.GetWindowDrawList().AddCircleFilled(at + new float2(24, 24), 3, color);
        }
        else { ImGui.Dummy(new(28, 28)); ImGui.SameLine(); }
        ImGui.BeginGroup();
        HomeLabel(row.Name, 13, HomeInk);
        ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + Math.Max(120, ImGui.GetContentRegionAvail().X)); HomeLabel(row.Note, 11, HomeMute); ImGui.PopTextWrapPos();
        if (row.JoinLobby is { } lobby)
        {
            ImGui.BeginDisabled(!row.CanJoin || enteringRoom is not null);
            if (HomeButton("socialJoin/" + row.Id, "Join", 70, 24)) { JoinSteamRoom(lobby); OpenMultiplayerView(); }
            ImGui.EndDisabled();
            if (!row.CanJoin && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) ResultTooltip("Leave your current party before joining another.");
        }
        ImGui.EndGroup(); ImGui.Spacing();
    }
}

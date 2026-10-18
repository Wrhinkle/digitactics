using System.Text.Json;
using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using DigiTactics.Networking;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    public string? SteamLibraryPath { get; set; }
    private SteamRoomSession? steam;
    private bool showMultiplayer;
    private string lobbyMessage = "";
    private Task<NetworkRoom>? enteringRoom;
    private Task<IReadOnlyList<SteamLobby>>? listingRooms;
    private Task<IReadOnlyList<SteamFriendRoom>>? listingFriendRooms;
    private IReadOnlyList<SteamFriendRoom> friendRooms = [];
    private IReadOnlyList<SteamLobby> availableRooms = [];
    private SteamLobbyVisibility visibility = SteamLobbyVisibility.Friends;
    private long presenceAt;
    private long recentPlayersAt;
    private string recentPlayersError = "";
    private ulong? invitedLobby;
    private sealed record LobbyAvatar(long At, int Handle, GpuTexture? Texture);
    private readonly Dictionary<ulong, LobbyAvatar> lobbyAvatars = [];
    private bool clearLobbyAvatars;
    private void ClearLobbyAvatars() { foreach (var item in lobbyAvatars.Values) item.Texture?.Dispose(); lobbyAvatars.Clear(); }
    private void DrawLobbyAvatar(string? uid)
    {
        GpuTexture? texture = null;
        if (steam is not null && ulong.TryParse(uid, out ulong id))
        {
            long now = Environment.TickCount64;
            if (!lobbyAvatars.TryGetValue(id, out var cached) || now - cached.At >= 2000)
            {
                if (cached is null && lobbyAvatars.Count >= 32)
                { ulong oldest = lobbyAvatars.MinBy(p => p.Value.At).Key; lobbyAvatars[oldest].Texture?.Dispose(); lobbyAvatars.Remove(oldest); }
                var image = steam.Friends.Avatar(id);
                texture = cached?.Texture;
                if (image is not null && image.Handle != cached?.Handle)
                {
                    texture?.Dispose(); texture = new(new(image.Width, image.Height, image.Rgba), new(MinFilter: 9729, WrapS: 33071, WrapT: 33071), false);
                }
                lobbyAvatars[id] = new(now, image?.Handle ?? cached?.Handle ?? 0, texture);
            }
            else texture = cached.Texture;
        }
        var point = ImGui.GetCursorScreenPos();
        if (texture is not null) ImGui.GetWindowDrawList().AddImage(new ImTextureRef { _TexID = new((nint)texture.Id) }, point, new(point.X + 28, point.Y + 28));
        ImGui.Dummy(new(28, 28)); ImGui.SameLine();
    }
    internal void UsePrivateLobbyForSmoke() => visibility = SteamLobbyVisibility.Private;
    internal bool InSteamRoom => steam?.Room is not null;
    private string SteamName => LobbyAuthority.CleanName(profile?.Progress.Name ?? steam?.Name) is { Length: > 0 } name ? name : "Tamer";
    private void ConnectSteam()
    {
        try
        {
            string library = SteamLibraryPath ?? Path.Combine(AppContext.BaseDirectory, "steam_api64.dll");
            if (!File.Exists(library)) throw new FileNotFoundException("Steam support is not installed. Launch this build with --steam-api followed by your steam_api64.dll path.");
            steam = new(library, 480);
            steam.ConnectionLost += reason => { lobbyMessage = reason; EndOnlineView(); };
            steam.JoinRequested += (lobby, _) => invitedLobby = lobby;
            lobbyMessage = "Connected as " + steam.Name; RefreshSteamRooms();
        }
        catch (Exception error) { lobbyMessage = "Could not connect: " + error.Message; }
    }
    private void RefreshSteamRooms()
    {
        if (steam is null || listingRooms is not null || listingFriendRooms is not null) return;
        try { listingRooms = steam.Matchmaking.ListCompatible(); listingFriendRooms = steam.Friends.ListRooms(); } catch (Exception error) { lobbyMessage = error.Message; }
    }
    private void CreateSteamRoom()
    {
        if (steam is null) return;
        var lobby = new JsonObject
        {
            ["host"] = steam.SteamId.ToString(), ["hostName"] = SteamName, ["name"] = SteamName + "'s room", ["status"] = "lobby",
            ["setId"] = Rules.Id.Split('/')[0], ["pace"] = Rules.Pace, ["format"] = duos ? "duos" : "solo", ["planTime"] = 30,
            ["slots"] = new JsonObject { ["0"] = new JsonObject { ["uid"] = steam.SteamId.ToString(), ["name"] = SteamName } }
        };
        try { enteringRoom = steam.Create(lobby, visibility); lobbyMessage = "Creating room..."; } catch (Exception error) { lobbyMessage = error.Message; }
    }
    private void JoinSteamRoom(ulong id)
    {
        try { enteringRoom = steam!.Join(id); lobbyMessage = "Joining room..."; } catch (Exception error) { lobbyMessage = error.Message; }
    }
    private void UpdateSteamLobby()
    {
        if (clearLobbyAvatars) { ClearLobbyAvatars(); clearLobbyAvatars = false; }
        if (steam is null) return;
        long now = Environment.TickCount64;
        try { steam.Pump(now); } catch (Exception error) { lobbyMessage = "Connection error: " + error.Message; LeaveSteamRoom(); return; }
        if (listingRooms?.IsCompleted == true)
        {
            try { availableRooms = listingRooms.GetAwaiter().GetResult(); } catch (Exception error) { lobbyMessage = "Could not list rooms: " + error.Message; }
            listingRooms = null;
        }
        if (listingFriendRooms?.IsCompleted == true)
        {
            try { friendRooms = listingFriendRooms.GetAwaiter().GetResult(); } catch (Exception error) { lobbyMessage = "Could not list friends' rooms: " + error.Message; }
            listingFriendRooms = null;
        }
        if (enteringRoom?.IsCompleted == true)
        {
            try
            {
                var entered = enteringRoom.GetAwaiter().GetResult();
                entered.SeatResult += (_, accepted) => lobbyMessage = accepted ? "Seat selected." : "That seat is not available.";
                lobbyMessage = "Choose a seat. Empty seats are controlled by AI."; presenceAt = 0; recentPlayersAt = 0;
            }
            catch (Exception error) { lobbyMessage = "Could not enter room: " + error.Message; }
            enteringRoom = null;
        }
        if (steam.Room is not { } room) return;
        if (now - presenceAt > 5000)
        {
            var presence = new JsonObject { ["name"] = SteamName };
            try { presence["look"] = CosmeticLoadouts.Local(profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"), profile?.Storage.GetValueOrDefault("dt.legendevo.v1")); }
            catch (Exception error) when (error is JsonException or InvalidDataException or InvalidOperationException or FormatException)
            { lobbyMessage = "Appearance could not be shared: " + error.Message; }
            room.SetPresence(presence, false, now); presenceAt = now;
        }
        if (profile is not null && now - recentPlayersAt > 5000)
        {
            recentPlayersAt = now;
            try
            {
                var presence = room.Authority.Presence;
                var slots = room.Authority.Lobby?["slots"] as JsonObject;
                var members = room.Authority.Members.Select(id => new EncounteredPlayer(
                    presence[id]?["name"]?.GetValue<string>() ?? slots?.FirstOrDefault(s => s.Value?["uid"]?.GetValue<string>() == id).Value?["name"]?.GetValue<string>(),
                    id == room.Authority.LocalId));
                string? previous = profile.Storage.GetValueOrDefault("dt.client.v1");
                string? updated = RecentPlayers.Remember(previous, SteamName, members, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                recentPlayersError = updated == previous || profile.UpdateStorage(new Dictionary<string, string> { ["dt.client.v1"] = updated! })
                    ? "" : profile.Error ?? "Recent players could not be saved.";
            }
            catch (Exception error) when (error is JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException)
            { recentPlayersError = "Recent players could not be saved: " + error.Message; }
        }
        if (room.Init is not null && networkRoom is null)
        {
            try { AttachNetworkMatch(room); } catch (Exception error) { lobbyMessage = "Could not load this match: " + error.Message; LeaveSteamRoom(); }
        }
    }
    private void EndOnlineView()
    {
        DetachNetworkMatch(); Session = null; history = null; battle?.Dispose(); battle = null; watching = null; combatAudio = null;
        selectedUnit = selectedItem = null; audio.Music(null);
    }
    private void LeaveSteamRoom()
    {
        EndOnlineView(); steam?.Leave(); enteringRoom = null; clearLobbyAvatars = true;
    }
    private void StartSteamMatch(NetworkRoom room)
    {
        var lobby = room.Authority.Lobby!;
        string[] names = ["Tai", "Matt", "Sora", "Izzy", "Mimi", "Joe", "Kari", "T.K."];
        var slots = Enumerable.Range(0, 8).Select(i => new { name = lobby["slots"]?[i.ToString()]?["name"]?.GetValue<string>() ?? names[i], human = lobby["slots"]?[i.ToString()] is not null }).ToArray();
        long now = (long)room.ServerTime(Environment.TickCount64);
        var init = JsonSerializer.SerializeToElement(new { seed = Random.Shared.Next(1, int.MaxValue), at = now, slots,
            setId = lobby["setId"]!.GetValue<string>(), pace = lobby["pace"]!.GetValue<string>(), format = lobby["format"]!.GetValue<string>(), planTime = 30 });
        if (room.PublishInit(init)) room.UpdateLobby(new JsonObject { ["status"] = "playing", ["round"] = "1-1", ["alive"] = new JsonArray(Enumerable.Repeat(true, 8).Select(v => JsonValue.Create(v)).ToArray()) });
    }
    private void MultiplayerMenu()
    {
        Window("Steam multiplayer", size.X * .15f, 70, size.X * .7f, size.Y - 140);
        if (steam is null)
        {
            UiText.Wrapped("Play with other Tamers through Steam."u8);
            if (Button("steamConnect", "Connect to Steam")) ConnectSteam();
        }
        else if (steam.Room is { } room)
        {
            var lobby = room.Authority.Lobby;
            if (lobby is not null)
            {
                ImGui.Text(lobby["name"]!.GetValue<string>()); ImGui.Text($"{lobby["setId"]} / {lobby["pace"]} / {lobby["format"]}");
                for (int i = 0; i < 8; i++)
                {
                    var seat = lobby["slots"]?[i.ToString()]; string? uid = seat?["uid"]?.GetValue<string>();
                    bool yours = uid == room.Authority.LocalId;
                    DrawLobbyAvatar(uid);
                    ImGui.Text($"Seat {i + 1}: {seat?["name"]?.GetValue<string>() ?? "AI"}{(yours ? " (you)" : "")}"); ImGui.SameLine();
                    if (uid is null && Button("steamSeat/" + i, "Take seat")) room.ClaimSeat(i, SteamName, Environment.TickCount64);
                    if (yours && Button("steamFree/" + i, "Leave seat")) room.FreeSeat(i);
                }
                if (room.Authority.IsHost && room.Init is null)
                {
                    bool seated = lobby["slots"]!.AsObject().Any(s => s.Value?["uid"]?.GetValue<string>() == room.Authority.LocalId);
                    ImGui.BeginDisabled(!seated);
                    if (Button("steamStart", "Start match", 220, 44)) StartSteamMatch(room);
                    ImGui.EndDisabled();
                    if (!seated) ImGui.Text("Take a seat before starting the match."u8);
                }
                if (room.Init is not null) UiText.Wrapped("Match in progress. Take an available seat to join."u8);
            }
            if (Button("steamLeave", "Leave room")) LeaveSteamRoom();
            ImGui.SameLine(); if (Button("players", "Players")) showPlayers = !showPlayers;
        }
        else
        {
            ImGui.Text("Connected as " + steam.Name);
            ImGui.BeginDisabled(enteringRoom is not null);
            if (ImGui.BeginCombo("Ruleset / pace"u8, $"{Rules.Name} / {Rules.Pace}"))
            {
                for (int i = 0; i < catalog.Variants.Length; i++) if (ImGui.Selectable($"{catalog.Variants[i].Name} / {catalog.Variants[i].Pace}", variant == i)) variant = i;
                ImGui.EndCombo();
            }
            ImGui.Checkbox("Duos"u8, ref duos);
            if (ImGui.BeginCombo("Visibility"u8, visibility.ToString()))
            {
                foreach (var item in Enum.GetValues<SteamLobbyVisibility>()) if (ImGui.Selectable(item.ToString(), visibility == item)) visibility = item;
                ImGui.EndCombo();
            }
            if (Button("steamCreate", "Create room")) CreateSteamRoom(); ImGui.SameLine();
            if (Button("steamRefresh", listingRooms is null && listingFriendRooms is null ? "Refresh rooms" : "Searching...")) RefreshSteamRooms();
            if (invitedLobby is { } invited && Button("steamAcceptInvite", "Join invited room")) { invitedLobby = null; JoinSteamRoom(invited); }
            ImGui.Separator();
            var displayed = friendRooms.Select(r => r.Lobby).Concat(availableRooms).DistinctBy(r => r.Id).Where(r => r.Data["state"] != "over").ToArray();
            foreach (var listed in displayed)
            {
                ImGui.Text($"{listed.Data["name"]}  {listed.Members}/{listed.Capacity}  {listed.Data["setId"]} / {listed.Data["format"]}"); ImGui.SameLine();
                if (Button("steamJoin/" + listed.Id, "Join")) JoinSteamRoom(listed.Id);
                if (friendRooms.FirstOrDefault(r => r.Lobby.Id == listed.Id) is { } roomFriends) ImGui.Text("Friends: " + string.Join(", ", roomFriends.Friends.Select(f => f.Name)));
            }
            if (displayed.Length == 0 && listingRooms is null && listingFriendRooms is null) ImGui.Text("No compatible rooms found."u8);
            ImGui.EndDisabled();
        }
        if (profile is not null && ImGui.CollapsingHeader("Recent players"u8))
        {
            try
            {
                var recent = RecentPlayers.Read(profile.Storage.GetValueOrDefault("dt.client.v1"));
                if (recent.Count == 0) ImGui.Text("Players you meet will appear here."u8);
                foreach (var player in recent) ImGui.Text(player.Name);
            }
            catch (Exception error) when (error is JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException)
            { UiText.Wrapped("Recent players could not be read: " + error.Message); }
        }
        if (recentPlayersError.Length > 0) UiText.Wrapped(recentPlayersError);
        ImGui.Separator(); UiText.Wrapped(lobbyMessage);
        if (Button("steamBack", "Back to menu")) { LeaveSteamRoom(); showMultiplayer = false; }
        ImGui.End();
    }
}

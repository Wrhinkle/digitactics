using System.Text.Json;
using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using DigiTactics.Networking;
using DigiTactics.Presentation;
using DigiTactics.Rules;
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
    private int lobbyPlanningSeconds = 30;
    private long presenceAt;
    private long recentPlayersAt;
    private string recentPlayersError = "";
    private ulong? invitedLobby;
    private sealed record LobbyAvatar(long At, int Handle, GpuTexture? Texture);
    private readonly Dictionary<ulong, LobbyAvatar> lobbyAvatars = [];
    private bool clearLobbyAvatars;
    private void ClearLobbyAvatars() { foreach (var item in lobbyAvatars.Values) item.Texture?.Dispose(); lobbyAvatars.Clear(); }
    private GpuTexture? LobbyAvatarTexture(string? uid)
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
        return texture;
    }
    internal void UsePrivateLobbyForSmoke() => visibility = SteamLobbyVisibility.Private;
    internal bool InSteamRoom => steam?.Room is not null;
    private string SteamName => LobbyAuthority.CleanName(steam?.Name ?? profile?.Progress.Name) is { Length: > 0 } name ? name : "Tamer";
    private void ConnectSteam()
    {
        if(steam is not null)return;
        try
        {
            string library = SteamLibraryPath ?? Path.Combine(AppContext.BaseDirectory, "steam_api64.dll");
            if (!File.Exists(library)) throw new FileNotFoundException("Steam support is missing from this installation. Reinstall the full game package and try again.");
            AdoptSteam(new(library,480));
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
            ["host"] = steam.SteamId.ToString(), ["hostName"] = SteamName, ["name"] = LobbyRoomName, ["status"] = "lobby",
            ["setId"] = selectedSet, ["pace"] = lobbyPaceChoice, ["format"] = lobbyDuos ? "duos" : "solo", ["planTime"] = lobbyPlanningSeconds,
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
        ApplySteamJoinRequest();
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
        EndOnlineView(); steam?.Leave(); enteringRoom = null; invitedLobby=null; clearLobbyAvatars = true;
        lobbyPaceChoice=selectedPace;lobbyDuos=duos;
    }
    private void StartSteamMatch(NetworkRoom room)
    {
        var lobby = room.Authority.Lobby!;
        string[] names = ["Tai", "Matt", "Sora", "Izzy", "Mimi", "Joe", "Kari", "T.K."];
        var slots = Enumerable.Range(0, 8).Select(i => new MatchSeat(lobby["slots"]?[i.ToString()]?["name"]?.GetValue<string>() ?? names[i],lobby["slots"]?[i.ToString()] is not null)).ToArray();
        long now = (long)room.ServerTime(Environment.TickCount64);
        var init = MatchInitialization.Host(SetChoices,lobby["setId"]?.GetValue<string>(),lobby["pace"]?.GetValue<string>(),(uint)Random.Shared.Next(1_000_000_000),slots,
            lobby["format"]?.GetValue<string>()=="duos",lobby["planTime"]?.GetValue<int>()??30,now);
        if (room.PublishInit(init)) room.UpdateLobby(new JsonObject { ["status"] = "playing", ["round"] = "1-1", ["alive"] = new JsonArray(Enumerable.Repeat(true, 8).Select(v => JsonValue.Create(v)).ToArray()) });
    }
}

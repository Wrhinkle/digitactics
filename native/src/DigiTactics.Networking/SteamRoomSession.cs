using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigiTactics.Networking;

/// <summary>Owns Steam and one active game room. Call Pump from the game thread each frame.
/// Async results are polled by that thread; no Steam work runs on task continuations.</summary>
public sealed class SteamRoomSession : IDisposable
{
    private readonly SteamClient client;
    private readonly HashSet<ulong> members = [];
    private readonly Queue<RoomPacket> outgoing = [];
    private readonly Dictionary<ulong, long> closing = [];
    private Task<ulong>? entering;
    private TaskCompletionSource<NetworkRoom>? completion;
    private JsonObject? creating;
    private ulong lobbyId;
    private long now, refreshed;
    private int queuedBytes;
    private bool disposed, membershipDirty = true;
    private string lastMetadata = "";
    public NetworkRoom? Room { get; private set; }
    public ulong SteamId => client.SteamId;
    public string Name => client.Name;
    public SteamMatchmaking Matchmaking => client.Matchmaking;
    public SteamFriends Friends => client.Friends;
    public event Action<ulong, ulong>? JoinRequested;
    public event Action<string>? ConnectionLost;
    public long FailedSends { get; private set; }
    public SteamRoomSession(string library, uint? developmentAppId = null)
    {
        client = new(library, id => members.Contains(id) && id != SteamId, developmentAppId);
        client.Matchmaking.MemberChanged += _ => membershipDirty = true;
        client.Matchmaking.JoinRequested += (lobby, friend) => JoinRequested?.Invoke(lobby, friend);
    }
    private void Verify() { ObjectDisposedException.ThrowIf(disposed, this); client.VerifyThread(); }
    private static string Id(ulong id) => id.ToString(CultureInfo.InvariantCulture);
    public Task<NetworkRoom> Create(JsonObject lobby, SteamLobbyVisibility visibility = SteamLobbyVisibility.Public)
    {
        Verify(); EnsureIdle();
        var copy = lobby.DeepClone().AsObject(); copy["host"] = Id(SteamId);
        if (!NetworkProtocol.Lobby(JsonSerializer.SerializeToElement(copy))) throw new ArgumentException("Invalid lobby.", nameof(lobby));
        creating = copy; entering = client.Matchmaking.Create(visibility);
        completion = new(TaskCreationOptions.RunContinuationsAsynchronously); return completion.Task;
    }
    public Task<NetworkRoom> Join(ulong lobby)
    {
        Verify(); EnsureIdle();
        entering = client.Matchmaking.Join(lobby); creating = null;
        completion = new(TaskCreationOptions.RunContinuationsAsynchronously); return completion.Task;
    }
    private void EnsureIdle()
    {
        if (entering is not null || completion is not null || Room is not null) throw new InvalidOperationException("Leave the current room before entering another.");
    }
    public void Pump(long milliseconds)
    {
        Verify(); now = milliseconds; client.PumpCallbacks();
        if (entering?.IsCompleted == true)
        {
            var operation = entering; entering = null;
            try
            {
                lobbyId = operation.GetAwaiter().GetResult();
                if (completion is null) { client.Matchmaking.Leave(lobbyId); lobbyId = 0; return; }
                if (creating is null)
                {
                    var data = client.Matchmaking.Read(lobbyId).Data;
                    if (data["dt_game"] != "digitactics" || data["dt_ver"] != NetworkProtocol.Version) throw new IOException("This lobby uses an incompatible game version.");
                }
                Room = new(Id(lobbyId), Id(SteamId), creating, now);
                Room.Send += Enqueue; Room.BlockPeer += Block; Room.SnapshotResult += Snapshot;
                RefreshMembers();
                if (Room is null) return;
                if (creating is not null) { if (!PublishMetadata()) throw new IOException("Steam could not publish room metadata."); completion.TrySetResult(Room); completion = null; }
                else if (members.Count < 2) throw new IOException("This room has no other players to restore its state.");
                else Room.RequestSnapshot(now);
                creating = null;
            }
            catch (Exception e) { completion?.TrySetException(e); completion = null; Leave(); }
        }
        if (Room is not null)
        {
            if (membershipDirty || now - refreshed >= 1000) RefreshMembers();
            client.Messages.Poll(now, (peer, text) => Room?.Receive(Id(peer), text, now));
            Room?.Tick(now); Flush();
            if (failedSnapshot) { failedSnapshot = false; Lose("Could not restore the room state."); }
            if (Room is { } room)
            {
                if (room.IsClosed || room.Authority.Banned(Id(SteamId)))
                {
                    if (room.IsClosed) client.Matchmaking.SetData(lobbyId, new Dictionary<string,string> { ["state"] = "over" });
                    Lose("The room was closed or you were removed.");
                }
                else
                {
                    if (client.Matchmaking.Owner(lobbyId) == SteamId && ulong.TryParse(room.Authority.Host, out ulong host) && host != SteamId && members.Contains(host)) client.Matchmaking.SetOwner(lobbyId, host);
                    PublishMetadata();
                }
            }
        }
        else client.Messages.Poll(now, (_, _) => { }); // Drain stale packets after leaving.
        foreach (ulong peer in closing.Where(p => now >= p.Value).Select(p => p.Key).ToArray()) { client.Messages.Block(peer); closing.Remove(peer); }
    }
    private void RefreshMembers()
    {
        if (Room is null) return;
        var verified = client.Matchmaking.Members(lobbyId);
        ulong owner = client.Matchmaking.Owner(lobbyId);
        if (verified.Length is < 1 or > 8 || !verified.Contains(SteamId) || !verified.Contains(owner)) { Lose("Steam lobby membership was lost."); return; }
        foreach (ulong old in members.Except(verified).ToArray()) { members.Remove(old); client.Messages.ClosePeer(old); }
        foreach (ulong added in verified) members.Add(added);
        foreach (ulong peer in verified) if (peer != SteamId && !Room.Authority.Banned(Id(peer))) client.Messages.Accept(peer);
        Room.Members(verified.Select(Id), Id(owner), now);
        refreshed = now; membershipDirty = false;
    }
    private void Snapshot(bool success)
    {
        if (completion is null) return;
        bool valid = success && Room is { IsClosed: false } && Room.Authority.Lobby is not null && !Room.Authority.Banned(Id(SteamId));
        if (valid) completion.TrySetResult(Room!);
        else completion.TrySetException(new IOException("Could not restore the room state."));
        completion = null;
        // Cleanup runs after message delivery, outside NetworkRoom's snapshot event.
        if (!valid) failedSnapshot = true;
    }
    private bool failedSnapshot;
    private void Enqueue(RoomPacket packet)
    {
        int bytes = System.Text.Encoding.UTF8.GetByteCount(packet.Text);
        if (outgoing.Count >= 2048 || queuedBytes + bytes > 16 * 1024 * 1024) throw new IOException("Steam room send queue exceeded its limit.");
        outgoing.Enqueue(packet); queuedBytes += bytes;
    }
    private void Flush()
    {
        while (outgoing.TryDequeue(out var packet))
        {
            queuedBytes -= System.Text.Encoding.UTF8.GetByteCount(packet.Text);
            if (!ulong.TryParse(packet.Recipient, out ulong peer) || !client.Messages.Send(peer, packet.Text, packet.Reliable)) FailedSends++;
        }
    }
    private void Block(string id)
    {
        if (!ulong.TryParse(id, out ulong peer)) return;
        Flush(); // Send the removal announcement before closing the relay session.
        closing.TryAdd(peer, now + 500);
    }
    private bool PublishMetadata()
    {
        if (Room?.Authority.Lobby is not { } lobby || !Room.Authority.IsHost || client.Matchmaking.Owner(lobbyId) != SteamId) return false;
        string Value(string key, string fallback = "") => lobby[key]?.ToString() ?? fallback;
        var data = new Dictionary<string, string>
        {
            ["dt_game"] = "digitactics", ["dt_ver"] = NetworkProtocol.Version,
            ["name"] = Value("name"), ["host"] = Value("host"), ["hostName"] = Value("hostName"),
            ["format"] = Value("format") == "duos" ? "duos" : "solo", ["setId"] = Value("setId"),
            ["pace"] = Value("pace", "hyper"), ["rounds"] = Value("planTime", "30"),
            ["state"] = Value("status", "lobby"), ["round"] = Value("round", "1-1"),
            ["seats"] = JsonSerializer.Serialize(Enumerable.Range(0,8).Select(i => lobby["slots"]?[i.ToString()]?["name"]?.GetValue<string>() ?? ""))
        };
        string serialized = JsonSerializer.Serialize(data);
        if (serialized == lastMetadata) return true;
        if (!client.Matchmaking.SetData(lobbyId, data)) return false;
        lastMetadata = serialized; return true;
    }
    private void Lose(string reason) { Leave(); ConnectionLost?.Invoke(reason); }
    public void Leave()
    {
        Verify(); completion?.TrySetCanceled(); completion = null; creating = null;
        if (Room is { } room)
        {
            room.LeavePresence(); Flush();
            room.Send -= Enqueue; room.BlockPeer -= Block; room.SnapshotResult -= Snapshot; Room = null;
        }
        if (lobbyId != 0) client.Matchmaking.Leave(lobbyId);
        lobbyId = 0; members.Clear(); outgoing.Clear(); queuedBytes = 0; closing.Clear();
        client.Messages.ResetRoom(); lastMetadata = ""; membershipDirty = true; failedSnapshot = false;
    }
    public void Dispose()
    {
        if (disposed) return; Verify(); Leave(); client.Dispose(); disposed = true;
    }
}

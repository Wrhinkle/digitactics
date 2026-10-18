using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigiTactics.Networking;

public sealed record RoomPacket(string Recipient, string Text, bool Reliable = true);
public sealed record RoomIntent(string Sender, int Seat, string Action, JsonElement Arguments, int Round);

/// <summary>Main-thread room controller. Transport enqueues Send packets and supplies authenticated Steam sender IDs.
/// Tick/Receive use local milliseconds; ServerTime compensates for the host clock.</summary>
public sealed class NetworkRoom
{
    public LobbyAuthority Authority { get; }
    public ReplicatedLog Log { get; }
    public JsonElement? Init { get; private set; }
    public event Action<RoomPacket>? Send;
    public event Action<RoomIntent>? Intent;
    public event Action<string, JsonElement>? Effect;
    public event Action? Changed;
    public event Action? Reset;
    public event Action<long, bool>? SeatResult;
    public event Action<bool>? SnapshotResult;
    public event Action<string>? BlockPeer;
    public long Dropped { get; private set; }
    public bool IsClosed { get; private set; }
    private readonly PeerMessageLimits limits = new();
    private long requestId, intentSequence, snapshotAt, lastPing;
    private long? snapshotRequest;
    private bool snapshotRetried;
    private double offset, bestRtt = double.PositiveInfinity;
    private JsonObject? myPresence;
    private readonly Dictionary<long, long> seatRequests = [];
    public double ServerTime(long localTime) => Math.Floor(localTime + offset + .5);
    public NetworkRoom(string roomId, string localId, JsonObject? lobby = null, long initialTime = 0)
    {
        Authority = new(roomId, localId, lobby); Log = new(roomId); intentSequence = initialTime * 1000;
    }
    private string Wire(object message)
    {
        var node = JsonSerializer.SerializeToNode(message)!.AsObject(); node["l"] = Authority.RoomId; return node.ToJsonString();
    }
    private void To(string? peer, object message, bool reliable = true)
    {
        if (peer is not null && peer != Authority.LocalId && Authority.Member(peer)) Send?.Invoke(new(peer, Wire(message), reliable));
    }
    private void Broadcast(object message, bool reliable = true)
    {
        foreach (string peer in Authority.Members) To(peer, message, reliable);
    }
    private void LobbyChanged() { Broadcast(new { t = "lobby", lobby = Authority.Lobby }); Changed?.Invoke(); }
    private void PresenceChanged() { Broadcast(new { t = "presence", presence = Authority.Presence }); Changed?.Invoke(); }
    public void Members(IEnumerable<string> members, string? owner, long now)
    {
        if (IsClosed) return;
        var priorPresence = Authority.Presence;
        if (!Authority.SetMembers(members, owner, now)) return;
        limits.Prune(Authority.Members); Changed?.Invoke();
        if (Authority.IsHost && !JsonNode.DeepEquals(priorPresence, Authority.Presence)) PresenceChanged();
    }
    public long RequestSnapshot(long now)
    {
        if (IsClosed) throw new InvalidOperationException("The room is closed.");
        if (Authority.IsHost && (Init is not null || Log.Count > 0)) { SnapshotResult?.Invoke(true); return 0; }
        if (Authority.IsHost && Init is null && Log.Count == 0) Authority.BeginHostRecovery();
        long rid = ++requestId; snapshotRequest = rid; snapshotAt = now; snapshotRetried = false; Log.BeginSnapshot(rid);
        if (Authority.Host is { } host && Authority.Member(host) && host != Authority.LocalId) To(host, new { t = "snapreq", rid });
        else Broadcast(new { t = "snapreq", rid });
        return rid;
    }
    public void Tick(long now)
    {
        if (IsClosed) return;
        foreach (var expired in seatRequests.Where(r => now >= r.Value).Select(r => r.Key).ToArray()) { seatRequests.Remove(expired); SeatResult?.Invoke(expired, false); }
        if (snapshotRequest is { } rid)
        {
            if (now - snapshotAt >= 9000) { Log.ExpireSnapshot(rid); snapshotRequest = null; Authority.CancelHostRecovery(); SnapshotResult?.Invoke(false); Changed?.Invoke(); }
            else if (!snapshotRetried && now - snapshotAt >= 2500) { snapshotRetried = true; Broadcast(new { t = "snapreq", rid }); }
        }
        if (Authority.TryClaimHost(now))
        {
            if (myPresence is not null) Authority.SetPresence(Authority.LocalId, myPresence, false, (long)ServerTime(now));
            LobbyChanged(); PresenceChanged(); Broadcast(new { t = "head", n = Log.Count });
        }
        if (!Authority.IsHost && Authority.Host is { } host && now - lastPing > 5000) { lastPing = now; To(host, new { t = "ping", t0 = now }); }
    }
    private void SnapshotTo(string peer, long rid, long now)
    {
        if (!Authority.IsHost) return;
        for (int start = 0; start < Math.Max(1, Log.Count); start += NetworkProtocol.Chunk)
            To(peer, new { t = "snap", rid, lobby = Authority.Lobby, presence = Authority.Presence, init = Init, now = ServerTime(now), total = Log.Count, start, entries = Log.Entries.Skip(start).Take(NetworkProtocol.Chunk).ToArray() });
    }
    public bool Receive(string sender, string text, long now)
    {
        if (IsClosed || sender == Authority.LocalId || !Authority.Member(sender)) return false;
        if (!limits.Allow(sender, "wire", 100, 300, now)) { Dropped++; return false; }
        if (NetworkProtocol.Parse(text) is not { } m || m.GetProperty("l").GetString() != Authority.RoomId) { Dropped++; return false; }
        string type = m.GetProperty("t").GetString()!;
        var group = type switch { "snapreq" or "logreq" => ("sync", .25, 2d), "fx" => ("fx", .5, 2d), "intent" => ("intent", 8d, 16d), "seat" or "unseat" or "pres" or "presup" or "presdel" or "ping" => ("request", 3d, 6d), _ => ("", 0d, 0d) };
        if (group.Item1 != "" && !limits.Allow(sender, group.Item1, group.Item2, group.Item3, now)) { Dropped++; return false; }
        JsonObject Object(string key) => JsonNode.Parse(m.GetProperty(key).GetRawText())!.AsObject();
        switch (type)
        {
            case "snapreq":
                if (!Authority.IsHost && !Log.Syncing && Authority.TakeOverRestartedHost(sender))
                {
                    if (myPresence is not null) Authority.SetPresence(Authority.LocalId, myPresence, false, (long)ServerTime(now));
                    LobbyChanged(); PresenceChanged(); Broadcast(new { t = "head", n = Log.Count });
                }
                SnapshotTo(sender, (long)m.GetProperty("rid").GetDouble(), now); return Authority.IsHost;
            case "seat":
                if (!Authority.IsHost) return false;
                bool seated = Authority.ClaimSeat(sender, (int)m.GetProperty("i").GetDouble(), m.GetProperty("name").GetString());
                if (seated) LobbyChanged(); To(sender, new { t = "seatres", rid = m.GetProperty("rid"), ok = seated }); return true;
            case "unseat": if (!Authority.FreeSeat(sender, (int)m.GetProperty("i").GetDouble())) return false; LobbyChanged(); return true;
            case "pres": case "presup":
                if (!Authority.SetPresence(sender, Object("rec"), type == "presup", (long)ServerTime(now))) return false;
                PresenceChanged(); return true;
            case "presdel": if (!Authority.DeletePresence(sender)) return false; PresenceChanged(); return true;
            case "intent":
                if (!Authority.AcceptIntent(sender, m)) return false;
                DeliverIntent(sender, m); return true;
            case "logreq":
                if (!Authority.IsHost) return false;
                for (int start = (int)m.GetProperty("from").GetDouble(); start < Log.Count; start += NetworkProtocol.Chunk)
                    To(sender, new { t = "logs", start, entries = Log.Entries.Skip(start).Take(NetworkProtocol.Chunk).ToArray() });
                return true;
            case "ping": To(sender, new { t = "pong", t0 = m.GetProperty("t0"), now = ServerTime(now) }); return true;
            case "pong":
                if (sender != Authority.Host) return false;
                double rtt = now - m.GetProperty("t0").GetDouble();
                if (rtt >= 0 && rtt <= bestRtt * 1.5 + 20) { bestRtt = Math.Min(bestRtt, rtt); offset = m.GetProperty("now").GetDouble() + rtt / 2 - now; } return true;
            case "seatres":
                long seatRequest = (long)m.GetProperty("rid").GetDouble();
                if (sender != Authority.Host || !seatRequests.Remove(seatRequest)) return false;
                SeatResult?.Invoke(seatRequest, m.GetProperty("ok").GetBoolean()); return true;
            case "lobby":
                bool recovering = Authority.RecoveringHost;
                if (!Authority.AcceptLobby(sender, m.GetProperty("lobby").ValueKind == JsonValueKind.Null ? null : Object("lobby"))) return false;
                // The former host confirms its chosen successor to peers that still see it in Steam membership.
                if (recovering && Authority.Lobby is not null) Broadcast(new { t = "lobby", lobby = Authority.Lobby });
                if (Authority.Lobby is null) FinishClose();
                ApplyBans(); Changed?.Invoke(); return true;
            case "presence": if (!Authority.AcceptPresence(sender, Object("presence"))) return false; Changed?.Invoke(); return true;
            case "init": if (sender != Authority.Host || Init is not null) return false; Init = m.GetProperty("init").Clone(); Changed?.Invoke(); return true;
            case "snap": case "log": case "logs": case "head":
                string? expected = type == "snap" && Authority.ExpectedHost(sender) ? sender : Authority.Host;
                if (expected is null) return false;
                var update = Log.Receive(sender, expected, text, now);
                if (!update.Accepted) return false;
                if (update.Snapshot is { } snapshot)
                {
                    Authority.AcceptLobby(sender, JsonNode.Parse(snapshot.Lobby.GetRawText())!.AsObject());
                    Authority.AcceptPresence(sender, JsonNode.Parse(snapshot.Presence.GetRawText())!.AsObject());
                    Init = snapshot.Init.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined ? null : snapshot.Init;
                    if (double.IsPositiveInfinity(bestRtt)) offset = snapshot.HostTime - now;
                    snapshotRequest = null; ApplyBans(); SnapshotResult?.Invoke(true);
                }
                if (update.RequestFrom is { } from) To(Authority.Host, new { t = "logreq", from });
                if (update.Reset) Reset?.Invoke(); Changed?.Invoke(); return true;
            case "fx":
                var effect = m.GetProperty("v"); string seat = ((int)effect.GetProperty("s").GetDouble()).ToString();
                if (Authority.Lobby?["slots"]?[seat]?["uid"]?.GetValue<string>() != sender) return false;
                Effect?.Invoke(sender, effect.Clone()); return true;
            default: return false;
        }
    }
    private void DeliverIntent(string sender, JsonElement message) => Intent?.Invoke(new(sender, (int)message.GetProperty("s").GetDouble(), message.GetProperty("a").GetString()!, message.TryGetProperty("g", out var g) ? g.Clone() : JsonSerializer.SerializeToElement(new { }), (int)message.GetProperty("round").GetDouble()));
    public long ClaimSeat(int seat, string name, long now)
    {
        if (IsClosed) throw new InvalidOperationException("The room is closed.");
        long rid = ++requestId;
        if (Authority.IsHost) { bool ok = Authority.ClaimSeat(Authority.LocalId, seat, name); if (ok) LobbyChanged(); SeatResult?.Invoke(rid, ok); }
        else { seatRequests[rid] = now + 6000; To(Authority.Host, new { t = "seat", rid, i = seat, name = LobbyAuthority.CleanName(name) }); }
        return rid;
    }
    public void SetPresence(JsonObject record, bool patch, long now)
    {
        if (IsClosed) return;
        if (!NetworkProtocol.Presence(JsonSerializer.SerializeToElement(record))) throw new ArgumentException("Invalid presence.");
        if (!patch || myPresence is null) myPresence = record.DeepClone().AsObject();
        else foreach (var field in record) myPresence[field.Key] = field.Value?.DeepClone();
        if (Authority.IsHost) { if (Authority.SetPresence(Authority.LocalId, record, patch, (long)ServerTime(now))) PresenceChanged(); }
        else To(Authority.Host, new { t = patch ? "presup" : "pres", rec = record });
    }
    public bool SubmitIntent(int seat, string action, JsonElement arguments, int round)
    {
        if (IsClosed || Authority.Host is null) return false;
        string wire = Wire(new { t = "intent", s = seat, a = action, g = arguments, round, q = ++intentSequence });
        if (NetworkProtocol.Parse(wire) is not { } message) return false;
        if (Authority.IsHost) { if (!Authority.AcceptIntent(Authority.LocalId, message)) return false; DeliverIntent(Authority.LocalId, message); }
        else To(Authority.Host, JsonNode.Parse(wire)!);
        return true;
    }
    public bool PublishInit(JsonElement init)
    {
        if (!Authority.IsHost || !NetworkProtocol.Init(init)) return false;
        Init = init.Clone(); Broadcast(new { t = "init", init }); Changed?.Invoke(); return true;
    }
    public bool Append(int sequence, JsonElement entry, long now)
    {
        if (Authority.IsHost && sequence >= NetworkProtocol.MaximumLog) { Close(); return false; }
        if (!Authority.IsHost || Log.Syncing || sequence != Log.Count || sequence >= NetworkProtocol.MaximumLog || !NetworkProtocol.Entry(entry)) return false;
        var message = new { t = "log", n = sequence, e = entry }; string wire = Wire(message);
        if (!Log.Receive(Authority.LocalId, Authority.LocalId, wire, now).Accepted) return false;
        Broadcast(message); Changed?.Invoke(); return true;
    }
    public bool Close()
    {
        if (!Authority.IsHost || IsClosed) return false;
        Broadcast(new { t = "lobby", lobby = (object?)null });
        Authority.AcceptLobby(Authority.LocalId, null); FinishClose(); Changed?.Invoke(); return true;
    }
    private void FinishClose()
    {
        IsClosed = true; Authority.CancelHostRecovery();
        if (snapshotRequest is { } rid) { Log.ExpireSnapshot(rid); snapshotRequest = null; SnapshotResult?.Invoke(false); }
        foreach (var seat in seatRequests.Keys.ToArray()) { seatRequests.Remove(seat); SeatResult?.Invoke(seat, false); }
    }
    public void FreeSeat(int seat)
    {
        if (seat is < 0 or > 7) return;
        if (Authority.IsHost) { if (Authority.FreeSeat(Authority.LocalId, seat)) LobbyChanged(); }
        else To(Authority.Host, new { t = "unseat", i = seat });
    }
    public void LeavePresence()
    {
        myPresence = null;
        if (Authority.IsHost) { if (Authority.DeletePresence(Authority.LocalId)) PresenceChanged(); }
        else To(Authority.Host, new { t = "presdel" });
    }
    public bool UpdateLobby(JsonObject patch)
    {
        if (!Authority.IsHost || Authority.Lobby is not { } next) return false;
        foreach (var field in patch) { if (field.Value is null) next.Remove(field.Key); else next[field.Key] = field.Value.DeepClone(); }
        if (!Authority.AcceptLobby(Authority.LocalId, next)) return false;
        LobbyChanged(); return true;
    }
    public bool SendEffect(JsonElement effect)
    {
        var message = new { t = "fx", v = effect };
        if (NetworkProtocol.Parse(Wire(message)) is null) return false;
        string seat = ((int)effect.GetProperty("s").GetDouble()).ToString();
        if (Authority.Lobby?["slots"]?[seat]?["uid"]?.GetValue<string>() != Authority.LocalId) return false;
        Broadcast(message, false); return true;
    }
    public bool Ban(string peer)
    {
        if (!Authority.Ban(peer)) return false;
        Send?.Invoke(new(peer, Wire(new { t = "lobby", lobby = Authority.Lobby })));
        ApplyBans(); LobbyChanged(); PresenceChanged(); return true;
    }
    private void ApplyBans()
    {
        if (Authority.Lobby?["banned"] is JsonArray banned)
            foreach (var peer in banned) if (peer?.GetValue<string>() is { } id && id != Authority.LocalId) BlockPeer?.Invoke(id);
    }
}

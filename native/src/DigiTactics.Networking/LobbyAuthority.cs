using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigiTactics.Networking;

/// <summary>Room identity and host-owned lobby mutations. Steam supplies membership; peers cannot invent it.</summary>
public sealed class LobbyAuthority
{
    private JsonObject? lobby;
    private readonly JsonObject presence = new();
    private string[] members = [];
    private readonly Dictionary<string, double> lastIntent = [];
    private long? hostGoneAt;
    public string LocalId { get; }
    public string RoomId { get; }
    public string? Owner { get; private set; }
    public string? Host => lobby?["host"]?.GetValue<string>();
    public bool IsHost => Host == LocalId;
    public JsonObject? Lobby => lobby?.DeepClone().AsObject();
    public JsonObject Presence => presence.DeepClone().AsObject();
    public IReadOnlyList<string> Members => System.Array.AsReadOnly(members);
    public LobbyAuthority(string roomId, string localId, JsonObject? initial = null)
    {
        if (!ValidId(roomId) || !ValidId(localId)) throw new ArgumentException("Invalid Steam identity.");
        if (initial is not null && !NetworkProtocol.Lobby(JsonSerializer.SerializeToElement(initial))) throw new ArgumentException("Invalid lobby.");
        RoomId = roomId; LocalId = localId; lobby = initial?.DeepClone().AsObject();
    }
    private static bool ValidId(string id) => id.Length is >= 1 and <= 20 && id[0] is >= '1' and <= '9' && id.All(char.IsAsciiDigit) && ulong.TryParse(id, out _);
    public bool Banned(string id) => lobby?["banned"] is JsonArray banned && banned.Any(x => x?.GetValue<string>() == id);
    public bool Member(string id) => members.Contains(id) && !Banned(id);
    public bool SetMembers(IEnumerable<string> values, string? owner, long now)
    {
        var next = values.Take(9).ToArray();
        if (next.Length is 0 or > 8 || next.Distinct().Count() != next.Length || next.Any(id => !ValidId(id)) || owner is not null && !ValidId(owner)) return false;
        members = next; Owner = owner;
        foreach (var id in presence.Select(p => p.Key).Where(id => !Member(id)).ToArray()) presence.Remove(id);
        foreach (var id in lastIntent.Keys.Where(id => !members.Contains(id)).ToArray()) lastIntent.Remove(id);
        if (Host is { } host && !members.Contains(host)) hostGoneAt ??= now; else hostGoneAt = null;
        return true;
    }
    public string? Successor(string? exclude)
    {
        for (int i = 0; i < 8; i++)
            if (lobby?["slots"]?[i.ToString()]?["uid"]?.GetValue<string>() is { } id && id != exclude && Member(id)) return id;
        return members.FirstOrDefault(id => id != exclude && !Banned(id));
    }
    public bool ExpectedHost(string id) => id == (Host ?? Owner) || Host is { } host && !members.Contains(host) && id == Successor(host);
    public bool AcceptLobby(string sender, JsonObject? next)
    {
        if (next is not null && !NetworkProtocol.Lobby(JsonSerializer.SerializeToElement(next))) return false;
        if (!Member(sender) || !ExpectedHost(sender) || sender != Host && next?["host"]?.GetValue<string>() != sender) return false;
        lobby = next?.DeepClone().AsObject();
        return true;
    }
    public bool TryClaimHost(long now, string? name = null)
    {
        if (lobby is null || IsHost || Host is not { } host || members.Contains(host) || hostGoneAt is null || now - hostGoneAt <= 6000 || Successor(host) != LocalId) return false;
        lobby["host"] = LocalId; lobby["hostName"] = string.IsNullOrEmpty(name) ? SeatName(LocalId) : CleanName(name);
        hostGoneAt = null; return true;
    }
    private string SeatName(string id) => lobby?["slots"]?.AsObject().Select(s => s.Value).FirstOrDefault(s => s?["uid"]?.GetValue<string>() == id)?["name"]?.GetValue<string>() ?? "Tamer";
    public static string CleanName(string? name)
    {
        string clean = new string((name ?? "").Where(c => c >= 32 && c is not ('<' or '>' or '&' or '"' or '\\')).ToArray()).Trim();
        return clean[..Math.Min(16, clean.Length)];
    }
    public bool ClaimSeat(string uid, int seat, string? name)
    {
        if (!IsHost || !Member(uid) || seat is < 0 or > 7 || lobby?["slots"] is not JsonObject slots || slots[seat.ToString()] is not null) return false;
        if (lobby["status"]?.GetValue<string>() == "playing" && lobby["alive"] is JsonArray alive && alive[seat]?.GetValue<bool>() == false) return false;
        var next = slots.DeepClone().AsObject();
        foreach (var key in next.Where(s => s.Value?["uid"]?.GetValue<string>() == uid).Select(s => s.Key).ToArray()) next.Remove(key);
        string cleaned = CleanName(name);
        next[seat.ToString()] = new JsonObject { ["uid"] = uid, ["name"] = cleaned.Length == 0 ? "Tamer" : cleaned };
        lobby["slots"] = next; return true;
    }
    public bool FreeSeat(string requester, int seat)
    {
        if (!IsHost || !Member(requester) || seat is < 0 or > 7 || lobby?["slots"]?[seat.ToString()] is not JsonObject slot || requester != LocalId && slot["uid"]?.GetValue<string>() != requester) return false;
        return lobby["slots"]!.AsObject().Remove(seat.ToString());
    }
    public bool Ban(string uid)
    {
        if (!IsHost || !ValidId(uid) || uid == LocalId || Banned(uid)) return false;
        var banned = lobby!["banned"] as JsonArray ?? new(); if (banned.Count >= 128) return false;
        var slots = lobby["slots"]!.AsObject();
        foreach (var key in slots.Where(s => s.Value?["uid"]?.GetValue<string>() == uid).Select(s => s.Key).ToArray()) slots.Remove(key);
        if (lobby["banned"] is null) lobby["banned"] = banned;
        banned.Add(uid); presence.Remove(uid); return true;
    }
    public bool SetPresence(string uid, JsonObject record, bool patch, long now)
    {
        if (!IsHost || !Member(uid) || !NetworkProtocol.Presence(JsonSerializer.SerializeToElement(record)) || patch && presence[uid] is null) return false;
        var next = patch ? presence[uid]!.DeepClone().AsObject() : new JsonObject { ["at"] = now };
        foreach (string key in new[] { "name", "look", "lobbySet", "lobbyPace", "lobbyFormat" })
            if (record.ContainsKey(key)) next[key] = key == "name" ? JsonValue.Create(CleanName(record[key]?.GetValue<string>())) : record[key]?.DeepClone();
        presence[uid] = next; return true;
    }
    public bool DeletePresence(string uid) => IsHost && Member(uid) && presence.Remove(uid);
    public bool AcceptIntent(string sender, JsonElement message)
    {
        if (!IsHost || !Member(sender) || !NetworkProtocol.Wire(message) || message.GetProperty("l").GetString() != RoomId || message.GetProperty("t").GetString() != "intent") return false;
        int seat = (int)message.GetProperty("s").GetDouble(); double sequence = message.GetProperty("q").GetDouble();
        if (lobby?["slots"]?[seat.ToString()]?["uid"]?.GetValue<string>() != sender || lastIntent.TryGetValue(sender, out double previous) && sequence <= previous) return false;
        lastIntent[sender] = sequence; return true;
    }
}

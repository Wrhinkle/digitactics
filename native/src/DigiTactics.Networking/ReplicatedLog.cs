using System.Text.Json;

namespace DigiTactics.Networking;

public sealed record ReplicaSnapshot(JsonElement Lobby, JsonElement Presence, JsonElement Init, double HostTime);
public sealed record ReplicaUpdate(bool Accepted, bool Reset = false, int? RequestFrom = null, ReplicaSnapshot? Snapshot = null);

/// <summary>Host-authoritative log and snapshot assembly. The room controller supplies its current expected host.
/// Returned entries own their JSON data. Transport timers call BeginSnapshot/ExpireSnapshot explicitly.</summary>
public sealed class ReplicatedLog(string roomId)
{
    private readonly List<JsonElement> entries = [];
    private readonly Dictionary<int, JsonElement> pending = [];
    private IReadOnlyList<JsonElement>? view;
    private long? request;
    private string? snapshotSender;
    private JsonElement?[]? snapshot;
    private int received;
    private long lastFill;
    public IReadOnlyList<JsonElement> Entries => view ??= entries.AsReadOnly();
    public int Count => entries.Count;
    public int PendingCount => pending.Count;
    public bool Syncing => request is not null;
    public void BeginSnapshot(long requestId)
    {
        if (requestId < 0 || requestId > 9007199254740991) throw new ArgumentOutOfRangeException(nameof(requestId));
        request = requestId; snapshotSender = null; snapshot = null; received = 0;
    }
    public bool ExpireSnapshot(long requestId)
    {
        if (request != requestId) return false;
        request = null; snapshot = null; snapshotSender = null; received = 0; Drain(); return true;
    }
    private void Drain()
    {
        foreach (int index in pending.Keys.Where(n => n < Count).ToArray()) pending.Remove(index);
        while (pending.Remove(Count, out var entry)) entries.Add(entry);
    }
    private ReplicaUpdate Entry(int index, JsonElement entry, long now)
    {
        if (index > Count + 512 || pending.Count >= 512) return new(false);
        entry = entry.Clone();
        if (Syncing) { pending[index] = entry; return new(true); }
        if (index == Count) { entries.Add(entry); Drain(); return new(true); }
        if (index > Count)
        {
            pending[index] = entry;
            if (now - lastFill > 1000) { lastFill = now; return new(true, RequestFrom: Count); }
            return new(true);
        }
        if (Comparable(entries[index]) == Comparable(entry)) return new(true);
        entries.RemoveRange(index, Count - index); entries.Add(entry); pending.Clear();
        return new(true, Reset: true);
    }
    public ReplicaUpdate Receive(string sender, string expectedHost, string wire, long now)
    {
        if (sender != expectedHost || NetworkProtocol.Parse(wire) is not { } message || message.GetProperty("l").GetString() != roomId) return new(false);
        switch (message.GetProperty("t").GetString())
        {
            case "log": return Entry((int)message.GetProperty("n").GetDouble(), message.GetProperty("e"), now);
            case "logs":
                int index = (int)message.GetProperty("start").GetDouble(); bool reset = false; int? fill = null;
                foreach (var e in message.GetProperty("entries").EnumerateArray()) { var result = Entry(index++, e, now); reset |= result.Reset; fill ??= result.RequestFrom; }
                return new(true, reset, fill);
            case "head":
                int length = (int)message.GetProperty("n").GetDouble();
                if (Count > length) { entries.RemoveRange(length, Count - length); pending.Clear(); return new(true, Reset: true); }
                return new(true, RequestFrom: Count < length ? Count : null);
            case "snap": return Snapshot(sender, message);
            default: return new(false);
        }
    }
    private ReplicaUpdate Snapshot(string sender, JsonElement message)
    {
        if (request is null || message.GetProperty("rid").GetDouble() != request || message.GetProperty("lobby").GetProperty("host").GetString() != sender) return new(false);
        int total = (int)message.GetProperty("total").GetDouble();
        if (snapshot is null || snapshotSender != sender) { snapshot = new JsonElement?[total]; snapshotSender = sender; received = 0; }
        if (snapshot.Length != total) return new(false);
        int index = (int)message.GetProperty("start").GetDouble();
        foreach (var entry in message.GetProperty("entries").EnumerateArray())
        {
            if (snapshot[index] is null) { snapshot[index] = entry.Clone(); received++; }
            index++;
        }
        if (received < total) return new(true);
        entries.Clear(); entries.AddRange(snapshot.Select(e => e!.Value));
        request = null; snapshot = null; snapshotSender = null; received = 0; Drain();
        return new(true, Snapshot: new(message.GetProperty("lobby").Clone(), message.GetProperty("presence").Clone(),
            message.TryGetProperty("init", out var init) ? init.Clone() : default, message.GetProperty("now").GetDouble()));
    }
    // JSON.stringify compares member order, but normalizes whitespace, escapes and numbers.
    private static string Comparable(JsonElement entry)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream)) Write(writer, entry);
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }
    private static void Write(Utf8JsonWriter writer, JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                var fields = value.EnumerateObject().GroupBy(p => p.Name).Select(g => g.Last()).ToArray();
                static uint? Index(string key) => uint.TryParse(key, out uint n) && n != uint.MaxValue && n.ToString(System.Globalization.CultureInfo.InvariantCulture) == key ? n : null;
                foreach (var field in fields.OrderBy(p => Index(p.Name) is null ? 1 : 0).ThenBy(p => Index(p.Name) ?? 0)) { writer.WritePropertyName(field.Name); Write(writer, field.Value); }
                writer.WriteEndObject(); break;
            case JsonValueKind.Array:
                writer.WriteStartArray(); foreach (var item in value.EnumerateArray()) Write(writer, item); writer.WriteEndArray(); break;
            case JsonValueKind.Number: writer.WriteNumberValue(value.GetDouble() == 0 ? 0 : value.GetDouble()); break;
            case JsonValueKind.String: writer.WriteStringValue(value.GetString()); break;
            default: value.WriteTo(writer); break;
        }
    }
}

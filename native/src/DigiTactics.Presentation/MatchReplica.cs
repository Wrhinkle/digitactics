using System.Text.Json;
using DigiTactics.Networking;
using DigiTactics.Rules;

namespace DigiTactics.Presentation;

public sealed record MatchDesync(int Sequence, bool AfterReplay);

/// <summary>Projects the authenticated room log into one native game. Pump on the game thread after room updates.
/// Rebuilds never emit live action effects; callers replace their visual match on Rebuilt.</summary>
public sealed class MatchReplica : IDisposable
{
    private readonly NetworkRoom room;
    private readonly ContentCatalog catalog;
    private bool reset = true, disposed;
    private string? initText;
    private long? lastResync;
    public LockstepMatch? Match { get; private set; }
    public int Rebuilds { get; private set; }
    public MatchDesync? LastDesync { get; private set; }
    public event Action<LockstepMatch>? Rebuilt;
    public event Action<JsonElement, LockstepResult>? Applied;
    public event Action<MatchDesync>? Desync;
    public event Action<LockstepMatch>? Building;
    public event Action<LockstepMatch, JsonElement>? Applying;
    public event Action<LockstepMatch, JsonElement, bool>? Advanced;
    public MatchReplica(NetworkRoom room, ContentCatalog catalog)
    {
        this.room = room; this.catalog = catalog; room.Reset += Reset; room.SnapshotResult += Snapshot;
    }
    private void Reset() => reset = true;
    private void Snapshot(bool success) { if (success) reset = true; }
    private LockstepMatch Create(JsonElement init)
    {
        if (!NetworkProtocol.Init(init)) throw new InvalidDataException("Invalid multiplayer match initialization.");
        string variant = init.GetProperty("setId").GetString() + "/" + init.GetProperty("pace").GetString();
        var rules = catalog.Variants.SingleOrDefault(v => v.Id == variant) ?? throw new InvalidDataException("This build does not contain the room's ruleset: " + variant);
        var seats = init.GetProperty("slots").EnumerateArray().Select(s => new MatchSeat(s.GetProperty("name").GetString()!,s.GetProperty("human").GetBoolean())).ToArray();
        int[][]? teams = init.TryGetProperty("teams",out var teamData) && teamData.ValueKind == JsonValueKind.Array
            ? teamData.EnumerateArray().Select(t => t.EnumerateArray().Select(s => (int)s.GetDouble()).ToArray()).ToArray() : null;
        return new(rules,(uint)init.GetProperty("seed").GetDouble(),seats,init.GetProperty("planTime").GetDouble(),(long)init.GetProperty("at").GetDouble(),
            init.TryGetProperty("format",out var format) && format.ValueKind == JsonValueKind.String && format.GetString() == "duos",teams);
    }
    private static bool Mismatch(LockstepMatch match, JsonElement entry) => entry.TryGetProperty("d",out var hash) && hash.ValueKind == JsonValueKind.String && hash.GetString() != match.Check();
    private void Rebuild(JsonElement init)
    {
        var next = Create(init); MatchDesync? bad = null;
        Building?.Invoke(next);
        foreach (var entry in room.Log.Entries)
        {
            if (bad is null && Mismatch(next,entry)) bad = new(next.Sequence,true);
            Applying?.Invoke(next, entry); next.Apply(entry); Advanced?.Invoke(next, entry, true);
        }
        Match = next; initText = init.GetRawText(); reset = false; Rebuilds++; LastDesync = bad;
        Rebuilt?.Invoke(next);
        if (bad is not null) Desync?.Invoke(bad);
    }
    public void Pump(long localTime)
    {
        ObjectDisposedException.ThrowIf(disposed,this);
        if (room.Log.Syncing || room.IsClosed) return;
        if (room.Init is not { } init) { Match = null; initText = null; reset = true; return; }
        if (reset || Match is null || initText != init.GetRawText() || Match.Sequence > room.Log.Count) { Rebuild(init); return; }
        while (Match.Sequence < room.Log.Count)
        {
            var entry = room.Log.Entries[Match.Sequence];
            if (!room.Authority.IsHost && Mismatch(Match,entry))
            {
                // A fresh replay that still disagrees is reported, not retried indefinitely.
                if (lastResync is { } last && localTime - last < 60000) return;
                lastResync = localTime; LastDesync = new(Match.Sequence,false); Desync?.Invoke(LastDesync);
                Rebuild(init); return;
            }
            Applying?.Invoke(Match, entry);
            var result = Match.Apply(entry); Advanced?.Invoke(Match, entry, false); Applied?.Invoke(entry,result);
        }
    }
    public void Dispose() { if (disposed) return; room.Reset -= Reset; room.SnapshotResult -= Snapshot; disposed = true; }
}

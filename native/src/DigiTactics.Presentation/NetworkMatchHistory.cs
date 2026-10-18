using System.Text.Json;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Presentation;

/// <summary>Reconstructs one seat's reward statistics and final living board during replay.
/// Saves only after a verified rebuild or live settlement; the original mp:seed:seat key deduplicates reconnects.</summary>
public sealed class NetworkMatchHistory : IDisposable
{
    private readonly MatchReplica replica;
    private readonly NativeProfileStore profile;
    private readonly int seat;
    private readonly Func<long> timestamp;
    public MatchHistoryRecorder? Recorder { get; private set; }
    public NetworkMatchHistory(MatchReplica replica, NativeProfileStore profile, int seat, Func<long>? timestamp = null)
    {
        if (seat is < 0 or > 7) throw new ArgumentOutOfRangeException(nameof(seat));
        this.replica = replica; this.profile = profile; this.seat = seat;
        this.timestamp = timestamp ?? (() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        replica.Building += Building; replica.Applying += Applying; replica.Advanced += Advanced; replica.Rebuilt += Rebuilt;
    }
    private void Building(LockstepMatch match) { Recorder = new(match.Session, profile, seat, true); Recorder.Capture(); }
    private void Applying(LockstepMatch _, JsonElement entry) { if (entry.GetProperty("k").GetString() == "end") Recorder?.Capture(); }
    private void Advanced(LockstepMatch _, JsonElement entry, bool replaying)
    {
        Recorder?.Observe();
        if (entry.GetProperty("k").GetString() == "start") Recorder?.Capture();
        if (!replaying && entry.GetProperty("k").GetString() == "end" && replica.LastDesync is null) Recorder?.Complete(timestamp());
    }
    private void Rebuilt(LockstepMatch _) { if (replica.LastDesync is null) Recorder?.Complete(timestamp()); }
    public void Dispose() { replica.Building -= Building; replica.Applying -= Applying; replica.Advanced -= Advanced; replica.Rebuilt -= Rebuilt; }
}

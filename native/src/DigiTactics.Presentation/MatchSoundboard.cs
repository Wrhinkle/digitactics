using DigiTactics.Storage;

namespace DigiTactics.Presentation;

public sealed record SoundboardLabel(int Seat, string Id, long Until);

/// <summary>Ephemeral soundboard events; never stored in or replayed from the match log.</summary>
public sealed class MatchSoundboard
{
    private readonly List<long> mine = [];
    private readonly Dictionary<int, List<long>> theirs = [];
    private readonly Dictionary<int, SoundboardLabel> labels = [];
    public event Action<string>? Play;
    private static void Prune(List<long> times, long now) => times.RemoveAll(at => now - at > 5000);
    public long Cooldown(long now) { Prune(mine, now); return mine.Count < 2 ? 0 : Math.Max(0, 5000 - (now - mine[0])); }
    public IReadOnlyList<SoundboardLabel> Labels(long now)
    {
        foreach (int seat in labels.Where(p => p.Value.Until <= now).Select(p => p.Key).ToArray()) labels.Remove(seat);
        return labels.Values.ToArray();
    }
    public bool Send(CosmeticCollection collection, int slot, int seat, long now, Func<string, bool>? transmit = null)
    {
        if (seat is < 0 or > 7 || slot is < 0 or > 5 || Cooldown(now) > 0) return false;
        string? id = collection.Equipped("sound")!.AsArray().ElementAtOrDefault(slot)?.GetValue<string>();
        if (id is null || CosmeticCollection.Item("sound", id) is null || transmit?.Invoke(id) == false) return false;
        mine.Add(now); Show(seat, id, now); return true;
    }
    public bool Receive(int seat, string id, int localSeat, bool muteOthers, long now)
    {
        if (seat is < 0 or > 7 || seat == localSeat || muteOthers || CosmeticCollection.Item("sound", id) is null) return false;
        if (!theirs.TryGetValue(seat, out var times)) theirs[seat] = times = [];
        Prune(times, now); if (times.Count >= 2) return false;
        times.Add(now); Show(seat, id, now); return true;
    }
    private void Show(int seat, string id, long now) { labels[seat] = new(seat, id, now + 2300); Play?.Invoke(id); }
}

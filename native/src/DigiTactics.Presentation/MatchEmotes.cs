using DigiTactics.Storage;

namespace DigiTactics.Presentation;

public sealed record ActiveEmote(int Seat, string Id, long Started);
public sealed class MatchEmotes
{
    private readonly Dictionary<int, List<long>> recent = [];
    private readonly Dictionary<int, ActiveEmote> active = [];
    public event Action? Played;
    public void ClearOthers(int localSeat) { foreach (int seat in active.Keys.Where(s => s != localSeat).ToArray()) active.Remove(seat); }
    public ActiveEmote? At(int seat, long now)
    {
        if (!active.TryGetValue(seat, out var emote)) return null;
        if (now - emote.Started >= 2600) { active.Remove(seat); return null; }
        return emote;
    }
    private bool Allow(int seat, long now)
    {
        if (!recent.TryGetValue(seat, out var times)) recent[seat] = times = [];
        times.RemoveAll(at => now - at >= 6000);
        if (times.Count >= 3) return false;
        times.Add(now); return true;
    }
    public bool Send(CosmeticCollection collection, string id, int seat, long now, Func<string, bool>? transmit = null)
    {
        if (seat is < 0 or > 7 || !collection.Owns("emote", id) || !Allow(seat, now)) return false;
        if (transmit?.Invoke(id) == false) return false;
        Show(seat, id, now); return true;
    }
    public bool Receive(int seat, string id, int localSeat, bool muted, long now)
    {
        if (seat is < 0 or > 7 || seat == localSeat || muted || CosmeticCollection.Item("emote", id) is null || !Allow(seat, now)) return false;
        Show(seat, id, now); return true;
    }
    private void Show(int seat, string id, long now) { active[seat] = new(seat, id, now); Played?.Invoke(); }
}

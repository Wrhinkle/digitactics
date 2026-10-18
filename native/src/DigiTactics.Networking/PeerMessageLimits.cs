namespace DigiTactics.Networking;

/// <summary>Original per-member token buckets. Caller time is monotonic milliseconds.</summary>
internal sealed class PeerMessageLimits
{
    private readonly Dictionary<(string Peer, string Group), (long At, double Tokens)> buckets = [];
    public bool Allow(string peer, string group, double rate, double burst, long now)
    {
        var key = (peer, group);
        var bucket = buckets.GetValueOrDefault(key, (At: now, Tokens: burst));
        double tokens = Math.Min(burst, bucket.Tokens + Math.Max(0, now - bucket.At) * rate / 1000);
        bool allowed = tokens >= 1;
        buckets[key] = (now, allowed ? tokens - 1 : tokens); return allowed;
    }
    public void Prune(IReadOnlyList<string> members)
    {
        foreach (var key in buckets.Keys.Where(k => !members.Contains(k.Peer)).ToArray()) buckets.Remove(key);
    }
}

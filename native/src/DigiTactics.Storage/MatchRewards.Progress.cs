using System.Text.Json;

namespace DigiTactics.Storage;

public sealed record DailyMissionProgress(string Id, string Name, int Bits, int Goal, int Current, bool Done);
public sealed record DailyRewardProgress(DailyMissionProgress[] Missions, bool Claimed, int Bits);

public static partial class MatchRewards
{
    /// <summary>The original Store displays this match's counters, including completed daily missions.</summary>
    public static DailyRewardProgress LiveProgress(string? shop,RewardStats? stats,DateOnly day)
    {
        var daily=Progress(shop,null,day);stats??=new(0,0,1,0,0);
        return daily with { Missions=daily.Missions.Select(m=>m with
        {Current=m.Done?m.Goal:Math.Clamp(Missions.Single(definition=>definition.Id==m.Id).Value(stats),0,m.Goal)}).ToArray() };
    }
    /// <summary>Read-only client mission view. Partial progress is the best single match today, not a sum.</summary>
    public static DailyRewardProgress Progress(string? shop, string? client, DateOnly day)
    {
        string date = day.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        using var document = JsonDocument.Parse(shop ?? "{}");
        var saved = document.RootElement;
        var done = new HashSet<string>(StringComparer.Ordinal);
        if (saved.TryGetProperty("missions", out var missions) && missions.TryGetProperty("day", out var savedDay) && savedDay.GetString() == date && missions.TryGetProperty("done", out var savedDone))
            foreach (var id in savedDone.EnumerateArray()) if (id.GetString() is { } value) done.Add(value);
        var history = TamerProgress.History(client).Where(match => DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(match.At).LocalDateTime) == day)
            .Select(match => match.Stats?.Deserialize<RewardStats>(Options)).Where(stats => stats is not null).Cast<RewardStats>().ToArray();
        var result = Missions.OrderBy(m => Hash(date + ":" + m.Id)).Take(2).Select(m =>
        {
            bool complete = done.Contains(m.Id);
            int current = complete ? m.Goal : history.Select(m.Value).Select(value => Math.Clamp(value, 0, m.Goal)).DefaultIfEmpty().Max();
            return new DailyMissionProgress(m.Id, m.Name, m.Bits, m.Goal, current, complete);
        }).ToArray();
        return new(result, saved.TryGetProperty("lastDay", out var last) && last.GetString() == date, 60);
    }
}

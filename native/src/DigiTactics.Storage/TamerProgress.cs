using System.Text.Json;

namespace DigiTactics.Storage;

public sealed record TamerLevel(int Level, long Xp, long Need, long Total);

/// <summary>The original client derives account XP from lifetime totals, never a second saved counter.</summary>
public static class TamerProgress
{
    public static TamerLevel Read(string? client)
    {
        using var document = JsonDocument.Parse(client ?? "{}");
        long Value(string key)
        {
            if (!document.RootElement.TryGetProperty("totals", out var totals) || !totals.TryGetProperty(key, out var value)) return 0;
            long number = value.GetInt64();
            return number >= 0 ? number : throw new InvalidDataException("Negative lifetime progress.");
        }
        long total = checked(Value("games") * 100 + Value("top4") * 60 + Value("wins") * 120);
        // Sum of costs for n completed levels is 50*n*(n+5). Decimal keeps large imported totals exact.
        int completed = (int)Math.Max(0, Math.Floor((Math.Sqrt(25 + total / 12.5) - 5) / 2));
        decimal Cost(int n) => 50m * n * (n + 5m);
        while (Cost(completed) > total) completed--;
        while (Cost(completed + 1) <= total) completed++;
        return new(completed + 1, total - (long)Cost(completed), 300L + 100L * completed, total);
    }

    public static HistoryMatch[] History(string? client)
    {
        using var document = JsonDocument.Parse(client ?? "{}");
        if (!document.RootElement.TryGetProperty("matches", out var matches)) return [];
        return matches.Deserialize<HistoryMatch[]>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
    }
}

namespace DigiTactics.Rules;

/// <summary>Shared inventory; exclusions belong to a player, never to the pool.</summary>
public sealed class ShopPool
{
    private readonly Ruleset rules;
    private readonly DeterministicRandom random;
    private readonly Dictionary<string, int> remaining;

    public ShopPool(Ruleset rules, DeterministicRandom random)
    {
        this.rules = rules;
        this.random = random;
        remaining = rules.Lines.ToDictionary(l => l.Id, l => rules.PoolSizes[l.Tier]);
    }

    public int Remaining(string id) => remaining[id];
    public void Take(string id, int count = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        remaining[id] = Math.Max(0, remaining[id] - count);
    }
    public void Return(string id, int count = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        remaining[id] = checked(remaining[id] + count);
    }

    public string? Roll(int level, IReadOnlySet<string> excluded, double[]? customOdds = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(level, 1);
        var standardOdds = customOdds is null ? rules.Odds[Math.Min(rules.Odds.Length - 1, level)]
            ?? throw new InvalidDataException("Missing shop odds.") : [];
        double roll = random.Next() * 100;
        int tier = 1;
        for (int i = 0; i < (customOdds?.Length ?? standardOdds.Length); i++)
        {
            double chance = customOdds is null ? standardOdds[i] : customOdds[i];
            if (roll < chance) { tier = i + 1; break; }
            roll -= chance;
        }
        for (int cost = tier; cost >= 1; cost--)
        {
            int total = 0;
            foreach (var line in rules.Lines)
                if (line.Tier == cost && !excluded.Contains(line.Id)) total += remaining[line.Id];
            if (total == 0) continue;
            double pick = random.Next() * total;
            foreach (var line in rules.Lines)
            {
                if (line.Tier != cost || excluded.Contains(line.Id) || remaining[line.Id] <= 0) continue;
                pick -= remaining[line.Id];
                if (pick < 0) { remaining[line.Id]--; return line.Id; }
            }
        }
        return null;
    }

    /// <summary>Loaded Dice chooses uniformly among lines after its tier roll, not by remaining copies.</summary>
    public string? RollMatching(int level, IReadOnlySet<string> eligible)
    {
        var odds = rules.Odds[Math.Min(rules.Odds.Length - 1, level)]!;
        double roll = random.Next() * 100;
        int tier = 1;
        for (int i = 0; i < odds.Length; i++)
        {
            if (roll < odds[i]) { tier = i + 1; break; }
            roll -= odds[i];
        }
        var available = rules.Lines.Where(l => eligible.Contains(l.Id) && remaining[l.Id] > 0).ToArray();
        var candidates = available.Where(l => l.Tier == tier).ToArray();
        if (candidates.Length == 0) candidates = available.Where(l => l.Tier <= tier).ToArray();
        if (candidates.Length == 0) candidates = available;
        if (candidates.Length == 0) return null;
        string id = candidates[(int)(random.Next() * candidates.Length)].Id;
        Take(id);
        return id;
    }
}

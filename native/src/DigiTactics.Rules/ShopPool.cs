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

    public string? Roll(int level, IReadOnlySet<string> excluded)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(level, 1);
        var odds = rules.Odds[Math.Min(rules.Odds.Length - 1, level)]
            ?? throw new InvalidDataException("Missing shop odds.");
        double roll = random.Next() * 100;
        int tier = 1;
        for (int i = 0; i < odds.Length; i++)
        {
            if (roll < odds[i]) { tier = i + 1; break; }
            roll -= odds[i];
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
}

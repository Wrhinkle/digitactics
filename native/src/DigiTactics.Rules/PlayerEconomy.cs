namespace DigiTactics.Rules;

public sealed record ExperienceRules(int[] Table, int PerRound, int BuyCost, int BuyXp, int StartLevel, int MaxLevel);
public sealed record InterestRules(int Per, int Max);
public sealed record EconomyRules(int StartHp, int StartGold, int[]? LevelByStage, int[]? IncomeByStage,
    bool StandardIncome, int BaseIncome, InterestRules? Interest, int[][] Streak,
    int WinGold, int WinGoldPve, ExperienceRules? Xp);
public sealed record IncomeBreakdown(int Base, int Interest, int Streak, int Augment = 0, int Bonus = 0)
{
    public int Total => Base + Interest + Streak + Augment + Bonus;
}

/// <summary>Player-owned economy. Match coordination supplies round order and mechanic income hooks.</summary>
public sealed class PlayerEconomy(EconomyRules rules)
{
    public int Gold { get; private set; } = rules.StartGold;
    public int Level { get; private set; } = rules.Xp?.StartLevel ?? rules.LevelByStage![1];
    public int Xp { get; private set; }
    public int Streak { get; private set; }

    public bool Spend(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        if (Gold < amount) return false;
        Gold -= amount;
        return true;
    }

    public void Credit(int amount) => Gold = checked(Gold + amount);

    public void AddXp(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        if (rules.Xp is not { } experience) return;
        Xp = checked(Xp + amount);
        while (Level < experience.MaxLevel && Level < experience.Table.Length &&
               experience.Table[Level] > 0 && Xp >= experience.Table[Level])
        {
            Xp -= experience.Table[Level];
            Level++;
        }
        if (Level >= experience.MaxLevel) Xp = 0;
    }

    public bool BuyXp()
    {
        if (rules.Xp is not { } experience || Level >= experience.MaxLevel || !Spend(experience.BuyCost)) return false;
        AddXp(experience.BuyXp);
        return true;
    }

    public IncomeBreakdown Income(int stage, int round)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(stage, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(round, 1);
        int baseline = rules.IncomeByStage is { } table ? table[Math.Min(stage, table.Length - 1)]
            : rules.StandardIncome ? stage == 1 ? round <= 3 ? 2 : 3 : stage == 2 && round == 1 ? 4 : 5
            : rules.BaseIncome;
        int interest = rules.Interest is { } rate ? Math.Min(Math.Max(0, Gold) / rate.Per, rate.Max) : 0;
        int streak = 0;
        foreach (var threshold in rules.Streak)
            if (Math.Abs(Streak) >= threshold[0]) streak = threshold[1];
        return new(baseline, interest, streak);
    }

    public IncomeBreakdown BeginRound(int stage, int round, Func<IncomeBreakdown, IncomeBreakdown>? incomeHook = null)
    {
        var income = Income(stage, round);
        if (rules.Xp is { } experience) AddXp(experience.PerRound);
        else if (round == 1) Level = rules.LevelByStage![Math.Min(stage, rules.LevelByStage.Length - 1)];
        if (incomeHook is not null) income = incomeHook(income);
        Credit(income.Total);
        return income;
    }

    public void RecordOutcome(bool won, bool pve)
    {
        if (won) Credit(pve ? rules.WinGoldPve : rules.WinGold);
        if (!pve) Streak = won ? Streak > 0 ? Streak + 1 : 1 : Streak < 0 ? Streak - 1 : -1;
    }
}

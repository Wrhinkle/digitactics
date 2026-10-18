namespace DigiTactics.Rules;

public sealed record MatchCombatRules(double Overtime, double Limit, Dictionary<string, int[][]> Waves, CombatEntry[][] Formations)
{
    public CombatEntry[] Wave(RoundPosition round, string? treasure)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(round.Stage, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(round.Round, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(round.Round, 7);
        var stages = Waves.Count == 1 ? Waves["classic"] : Waves[treasure ?? "classic"];
        return Formations[stages[Math.Min(round.Stage, stages.Length) - 1][round.Round - 1]].ToArray();
    }

    public void Validate(Ruleset rules)
    {
        var lines = rules.Lines.Concat(rules.ExtraLines).Select(l => l.Id).ToHashSet();
        string[] keys = rules.Id.StartsWith("dragonlands/", StringComparison.Ordinal) ? ["classic", "order", "chaos"] : ["classic"];
        if (!double.IsFinite(Overtime) || !double.IsFinite(Limit) || Overtime <= 0 || Limit <= Overtime ||
            !Waves.Keys.Order().SequenceEqual(keys.Order()) || Waves.Values.Any(stages => stages.Length != 10 ||
                stages.Any(rounds => rounds.Length != 7 || rounds.Any(i => i < 0 || i >= Formations.Length))) ||
            Formations.Any(wave => wave.Length == 0 || wave.Any(u => !lines.Contains(u.Line) || u.Stage is < 0 or > 2 ||
                u.Column is < 0 or > 6 || u.Row is < 4 or > 7 || u.Uid != -1)))
            throw new InvalidDataException("Invalid match combat definitions: " + rules.Id);
    }
}

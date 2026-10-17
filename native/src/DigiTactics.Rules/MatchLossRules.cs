namespace DigiTactics.Rules;

public readonly record struct Survivor(UnitLine Line, int Stage, int Team, bool Alive, bool Summoned);

public sealed record MatchLossRules(int[] StageDamage, bool UnderlordsRoundDamage, string UnitDamage,
    int? PveDamage, Dictionary<int, int> PveOverrides)
{
    public int BaseDamage(int stage)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(stage, 1);
        return UnderlordsRoundDamage ? (int)((5L * stage - 2) / 10) : StageDamage[Math.Min(stage, StageDamage.Length - 1)];
    }
    public int DamagePerUnit(UnitLine line, int stars, int stage) => UnitDamage switch
    {
        "none" => 0,
        "stars" => stars == 2 ? 2 : 1,
        "one" => 1,
        "galaxies" => stage >= 4 && (stars >= 2 || line.Tier >= 5) ? 1 : 0,
        "underlords" => line.Tamer is not null ? stage >= 7 ? 2 : 1 : 1 + ((line.Tier == 0 ? 1 : line.Tier) + 2 * stars) / 3,
        "autochess" => 1 + (line.Tier + 2 * stars) / 3,
        _ => throw new InvalidDataException($"Unknown unit loss rule: {UnitDamage}."),
    };

    public int Loss(int stage, int winnerTeam, bool pve, IEnumerable<Survivor> units, double? duoUnitScale = null)
    {
        int baseline = BaseDamage(stage);
        if (pve) baseline = PveOverrides.TryGetValue(stage, out int damage) ? damage : PveDamage ?? baseline;
        int unitDamage = winnerTeam < 0 ? 0 : units.Where(u => u.Alive && !u.Summoned && u.Team == winnerTeam)
            .Sum(u => DamagePerUnit(u.Line, u.Stage, stage));
        return baseline + (duoUnitScale is { } scale && !pve ? (int)Math.Floor(unitDamage * scale + .5) : unitDamage);
    }
    public int Loss(int stage, int winnerTeam, bool pve, IEnumerable<CombatUnit> units, double? duoUnitScale = null) =>
        Loss(stage, winnerTeam, pve, units.Select(u => new Survivor(u.Line, u.Stage, u.Team, u.Alive, u.Summoned)), duoUnitScale);
}

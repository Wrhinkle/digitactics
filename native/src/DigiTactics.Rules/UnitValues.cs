namespace DigiTactics.Rules;

public sealed record CombatTemplate(double Health, double AttackDamage, double AttackSpeed, int Range,
    double Armor, double MagicResist, double MaxMana, double StartMana, double HealthMultiplier, double AttackMultiplier, bool Leap);

public sealed record AbilityTemplate(string Kind, double BaseValue, int Radius, int Shots, string[] Names);

public readonly record struct UnitStats(double MaxHealth, double AttackDamage, double AttackSpeed, int Range,
    double Armor, double MagicResist, double MaxMana, double StartMana);

/// <summary>Pure unit formulas, independent of rendering, match state, and currently selected ruleset.</summary>
public static class UnitValues
{
    private static readonly double[] StatScale = [1, 1.8, 3.24];
    private static readonly double[] AbilityScale = [1, 1.7, 3];

    public static UnitStats Stats(UnitLine line, int stage)
    {
        ValidateStage(stage);
        var template = line.Combat;
        double tierScale = 1 + 0.18 * (line.Tier - 1);
        double starScale = StatScale[stage];
        return new(
            Round(template.Health * tierScale * starScale * template.HealthMultiplier),
            Round(template.AttackDamage * tierScale * starScale * template.AttackMultiplier),
            template.AttackSpeed, template.Range, template.Armor, template.MagicResist, template.MaxMana, template.StartMana);
    }

    public static double AbilityValue(UnitLine line, int stage)
    {
        ValidateStage(stage);
        return Round(line.Ability.BaseValue * AbilityScale[stage] * (1 + 0.25 * (line.Tier - 1)));
    }

    public static int Copies(UnitLine line, int stage)
    {
        ValidateStage(stage);
        return stage switch { 0 => 1, 1 => line.Copies, _ => checked(line.Copies * line.Copies) };
    }

    public static int SellValue(UnitLine line, int stage)
    {
        int copies = Copies(line, stage);
        int discount = stage switch { 0 => 0, 1 => line.Tier > 1 ? 1 : 0, _ => 1 };
        return checked((line.Tier + line.SellPremium) * copies - discount);
    }

    // JavaScript Math.round chooses the larger integer at a midpoint; .NET's default rounds to even.
    public static double Round(double value) => Math.Floor(value + 0.5);

    private static void ValidateStage(int stage)
    {
        if ((uint)stage > 2) throw new ArgumentOutOfRangeException(nameof(stage));
    }
}

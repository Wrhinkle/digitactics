namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private bool SupportsAbility(string kind) => SupportedAbilities.Contains(kind) ||
        (rules.Id.StartsWith("fates/", StringComparison.Ordinal) && FatesAbilities.Contains(kind)) ||
        (rules.Id.StartsWith("galaxies/", StringComparison.Ordinal) && GalaxiesAbilities.Contains(kind));

    private bool CastSetAbility(CombatUnit unit, CombatUnit target, double value)
    {
        if (rules.Id.StartsWith("fates/", StringComparison.Ordinal)) return CastFatesAbility(unit, target, value);
        if (rules.Id.StartsWith("galaxies/", StringComparison.Ordinal)) return CastGalaxiesAbility(unit, target, value);
        return false;
    }
}

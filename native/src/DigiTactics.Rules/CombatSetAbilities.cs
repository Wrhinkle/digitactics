namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private bool SupportsAbility(string kind) => SupportedAbilities.Contains(kind) ||
        (rules.Id.StartsWith("fates/", StringComparison.Ordinal) && FatesAbilities.Contains(kind)) ||
        (rules.Id.StartsWith("galaxies/", StringComparison.Ordinal) && GalaxiesAbilities.Contains(kind)) ||
        (rules.Id.StartsWith("gizmos/", StringComparison.Ordinal) && GizmosAbilities.Contains(kind)) ||
        (rules.Id.StartsWith("reckoning/", StringComparison.Ordinal) && ReckoningAbilities.Contains(kind)) ||
        (rules.Id.StartsWith("dragonlands/", StringComparison.Ordinal) && kind == "copy") ||
        (rules.Id.StartsWith("arcane/", StringComparison.Ordinal) && ArcaneAbilities.Contains(kind));

    private bool CastSetAbility(CombatUnit unit, CombatUnit target, double value)
    {
        if (rules.Id.StartsWith("fates/", StringComparison.Ordinal)) return CastFatesAbility(unit, target, value);
        if (rules.Id.StartsWith("galaxies/", StringComparison.Ordinal)) return CastGalaxiesAbility(unit, target, value);
        if (rules.Id.StartsWith("gizmos/", StringComparison.Ordinal)) return CastGizmosAbility(unit, target, value);
        if (rules.Id.StartsWith("reckoning/", StringComparison.Ordinal)) return CastReckoningAbility(unit, target, value);
        if (rules.Id.StartsWith("dragonlands/", StringComparison.Ordinal)) return CastDragonlandsAbility(unit, target, value);
        if (rules.Id.StartsWith("arcane/", StringComparison.Ordinal)) return CastArcaneAbility(unit, target, value);
        return false;
    }
}

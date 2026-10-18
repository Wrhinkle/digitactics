using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal static class TraitContributors
{
    public static CombatEntry[] Select(Ruleset rules, IEnumerable<CombatEntry> units, string trait, bool combined)
    {
        // Match the original Solo/combined trait lists: Solo chooses the highest form
        // per shop line and includes item traits; Duos lists both boards' base traits.
        if (combined)
            return units.Where(unit => rules.Lines.Concat(rules.ExtraLines).Single(line => line.Id == unit.Line).Traits.Contains(trait)).ToArray();
        return units.Where(unit => rules.Lines.Any(line => line.Id == unit.Line) && TraitRules.UnitTraits(unit, rules).Contains(trait))
            .GroupBy(unit => unit.Line).Select(group => group.OrderByDescending(unit => unit.Stage).First()).ToArray();
    }
}

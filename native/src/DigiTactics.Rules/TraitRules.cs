namespace DigiTactics.Rules;

public sealed record CombatSideEffects(bool NoTraits = false, Dictionary<string, int>? Counts = null, string[]? Augments = null);
public sealed record CombatSetup(CombatSideEffects SideA, CombatSideEffects SideB);

public static class TraitRules
{
    public static string[] UnitTraits(CombatEntry unit, Ruleset rules)
    {
        var traits = new List<string>(rules.Lines.Single(l => l.Id == unit.Line).Traits);
        foreach (var id in unit.Items ?? [])
        {
            string? emblem = rules.Items.FirstOrDefault(i => i.Id == id)?.Emblem;
            if (emblem is not null && !traits.Contains(emblem)) traits.Add(emblem);
        }
        return traits.ToArray();
    }

    public static int Level(TraitDefinition definition, int count)
    {
        int level = 0;
        for (int i = 0; i < definition.Breakpoints.Length; i++) if (count >= definition.Breakpoints[i]) level = i + 1;
        return level;
    }

    public static Dictionary<string, int> Count(Ruleset rules, IEnumerable<CombatEntry> units, string[]? augments = null)
    {
        // Ordered insertion is part of hook registration: duplicate lines contribute the union of their traits once.
        var lines = new Dictionary<string, List<string>>();
        var bonus = new Dictionary<string, int>();
        foreach (var unit in units)
        {
            if (!lines.TryGetValue(unit.Line, out var traits)) lines[unit.Line] = traits = [];
            foreach (var trait in UnitTraits(unit, rules)) if (!traits.Contains(trait)) traits.Add(trait);
            foreach (var pair in unit.BonusTraits ?? []) bonus[pair.Key] = Math.Max(bonus.GetValueOrDefault(pair.Key), pair.Value);
        }
        var counts = new Dictionary<string, int>();
        foreach (var traits in lines.Values) foreach (string trait in traits) counts[trait] = counts.GetValueOrDefault(trait) + 1;
        foreach (var pair in bonus) counts[pair.Key] = counts.GetValueOrDefault(pair.Key) + pair.Value;
        foreach (string augment in augments ?? [])
            if (augment == "crest" && rules.Id.StartsWith("faction-wars/", StringComparison.Ordinal))
                foreach (var trait in counts.Keys.ToArray())
                {
                    var definition = rules.Traits.Single(t => t.Id == trait);
                    if (definition.Kind == "family" && counts[trait] >= definition.Breakpoints[0]) counts[trait]++;
                }
        return counts;
    }
}

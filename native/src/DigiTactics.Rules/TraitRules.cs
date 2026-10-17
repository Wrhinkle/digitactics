namespace DigiTactics.Rules;

public sealed record ColiseumState(double RcAP = 0, string? Supreme = null);
public sealed record ArcaneState(double ArcSynHp = 0, bool ArcSynAug = false, double ArcChests = 0, string[]? ArcJunk = null);
public sealed record DragonlandsState(double Bank = 0, bool HpFull = false, double CtKills = 0);
public sealed record GizmosState(double GoldStart = 0);
public sealed record GalaxiesState(Dictionary<string, int>? Bounty = null);
public sealed record FatesState(int Deva = 0, string? Fallen = null, double Blossom = 0);
public sealed record CombatBuffs(double AttackSpeed = 0, double Armor = 0, double Mana = 0);
public sealed record CombatPartner(int Owner, string[]? Augments = null, CombatBuffs? Buffs = null, double PartnerTop = 0, GizmosState? Gizmos = null, DragonlandsState? Dragonlands = null, ArcaneState? Arcane = null, ColiseumState? Coliseum = null, UnderlordsState? Underlords = null, string[]? Globals = null);
public sealed record CombatSideEffects(bool NoTraits = false, Dictionary<string, int>? Counts = null, string[]? Augments = null,
    CombatPartner[]? Parts = null, CombatBuffs? Buffs = null, double PartnerTop = 0, FatesState? Fates = null, GalaxiesState? Galaxies = null, GizmosState? Gizmos = null, DragonlandsState? Dragonlands = null, ArcaneState? Arcane = null, ColiseumState? Coliseum = null, UnderlordsState? Underlords = null, string[]? Globals = null);
public sealed record CombatSetup(CombatSideEffects SideA, CombatSideEffects SideB, string? Galaxy = null, string? Mutation = null, string? RoundType = null, int RoundStage = 0, string? Mirage = null, string? Pace = null, string[]? ArcSponsor = null);

public static class TraitRules
{
    public static string[] UnitTraits(CombatEntry unit, Ruleset rules)
    {
        var traits = new List<string>(rules.Lines.Concat(rules.ExtraLines).Single(l => l.Id == unit.Line).Traits);
        foreach (var id in unit.Items ?? [])
        {
            string? emblem = rules.Items.FirstOrDefault(i => i.Id == id)?.Emblem;
            if (emblem is not null && !traits.Contains(emblem)) traits.Add(emblem);
        }
        return traits.ToArray();
    }

    public static Dictionary<string, int> TeamCount(Ruleset rules, CombatEntry[] units, CombatPartner[] parts)
    {
        var per = parts.Select(p => Count(rules, units.Where(u => u.Owner == p.Owner), p.Augments)).ToArray();
        var result = new Dictionary<string, int>();
        foreach (var counts in per) foreach (var key in counts.Keys) result.TryAdd(key, 0);
        foreach (var key in result.Keys.ToArray())
        {
            int high = per.Max(c => c.GetValueOrDefault(key));
            int rest = per.Sum(c => c.GetValueOrDefault(key)) - high;
            var trait = rules.Traits.FirstOrDefault(t => t.Id == key);
            var mode = trait?.Duo ?? default;
            result[key] = mode.ValueKind == System.Text.Json.JsonValueKind.Number ? high + (int)Math.Floor(rest * mode.GetDouble())
                : mode.ValueKind == System.Text.Json.JsonValueKind.String && mode.GetString() == "max" ? high : high + rest;
        }
        foreach (var part in parts)
            if ((part.Augments ?? []).Contains("duo_crest_link"))
                foreach (var unit in units.Where(u => u.Owner == part.Owner))
                    foreach (string item in unit.Items ?? [])
                    {
                        string? trait = rules.Items.FirstOrDefault(i => i.Id == item)?.Emblem;
                        if (trait is not null && !rules.Lines.Single(l => l.Id == unit.Line).Traits.Contains(trait))
                            result[trait] = result.GetValueOrDefault(trait) + 1;
                    }
        if (per.Length == 2 && parts.Any(p => (p.Augments ?? []).Contains("duo_sync")))
            foreach (var key in per[0].Keys)
            {
                var trait = rules.Traits.FirstOrDefault(t => t.Id == key);
                if (trait is not null && Level(trait, per[0][key]) > 0 && Level(trait, per[1].GetValueOrDefault(key)) > 0) result[key]++;
            }
        foreach (var key in result.Keys.ToArray())
        {
            var trait = rules.Traits.FirstOrDefault(t => t.Id == key);
            if (trait is not null && trait.Breakpoints.Length > 0)
                result[key] = Math.Min(result[key], Math.Max(trait.Breakpoints[^1], per.Max(c => c.GetValueOrDefault(key))));
        }
        return result;
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
            if (!rules.Lines.Any(l => l.Id == unit.Line)) continue;
            if (!lines.TryGetValue(unit.Line, out var traits)) lines[unit.Line] = traits = [];
            foreach (var trait in UnitTraits(unit, rules)) if (!traits.Contains(trait)) traits.Add(trait);
            foreach (var pair in unit.BonusTraits ?? []) bonus[pair.Key] = Math.Max(bonus.GetValueOrDefault(pair.Key), pair.Value);
        }
        var counts = new Dictionary<string, int>();
        foreach (var traits in lines.Values) foreach (string trait in traits) counts[trait] = counts.GetValueOrDefault(trait) + 1;
        foreach (var pair in bonus) counts[pair.Key] = counts.GetValueOrDefault(pair.Key) + pair.Value;
        foreach (string augment in augments ?? [])
        {
            if (augment == "crest" && rules.Id.StartsWith("faction-wars/", StringComparison.Ordinal))
                foreach (var trait in counts.Keys.ToArray())
                {
                    var definition = rules.Traits.Single(t => t.Id == trait);
                    if (definition.Kind == "family" && counts[trait] >= definition.Breakpoints[0]) counts[trait]++;
                }
            if (rules.Id.StartsWith("gizmos/", StringComparison.Ordinal))
            {
                if (augment == "giz_a_booster")
                {
                    string? best = null;
                    foreach (var key in counts.Keys)
                        if (rules.Traits.Any(t => t.Id == key && t.Kind == "origin") && (best is null || counts[key] > counts[best])) best = key;
                    if (best is not null) counts[best]++;
                }
                if (augment == "giz_a_burst") foreach (var key in counts.Keys.ToArray()) if (counts[key] >= 2) counts[key]++;
            }
            if (augment == "col_a_arena" && rules.Id.StartsWith("coliseum/", StringComparison.Ordinal))
                foreach (var trait in counts.Keys.ToArray())
                {
                    var definition = rules.Traits.Single(t => t.Id == trait);
                    if (trait != "MEN" && definition.Kind == "origin" && definition.Breakpoints.Length > 1 && counts[trait] >= definition.Breakpoints[0]) counts[trait]++;
                }
            if (augment == "gal_a_stars" && rules.Id.StartsWith("galaxies/", StringComparison.Ordinal))
                foreach (var trait in counts.Keys.ToArray())
                {
                    var definition = rules.Traits.Single(t => t.Id == trait);
                    if (definition.Kind == "origin" && definition.Breakpoints.Length > 1 && counts[trait] >= definition.Breakpoints[0]) counts[trait]++;
                }
            if (rules.Id.StartsWith("fates/", StringComparison.Ordinal) && augment is "fat_twin" or "fat_soulmates")
            {
                var partner = units.FirstOrDefault(u => u.Partner is not null);
                if (partner?.Partner?.Trait is { } trait && counts.GetValueOrDefault(trait) != 0) counts[trait]++;
            }
        }
        if (rules.Id.StartsWith("fates/", StringComparison.Ordinal) && counts.TryGetValue("NINJA", out int ninja) && ninja != 1 && ninja != 4)
            counts["NINJA"] = 0;
        if (rules.Id.StartsWith("dragonlands/", StringComparison.Ordinal))
            foreach (var lineId in lines.Keys)
                if (rules.Lines.Single(l => l.Id == lineId).Mark is { } mark) counts[mark] = counts.GetValueOrDefault(mark) + 2;
        if (rules.Id.StartsWith("arcane/", StringComparison.Ordinal) && counts.GetValueOrDefault("EMI") is 2 or 3) counts["EMI"] = 0;
        if (rules.Id.StartsWith("coliseum/", StringComparison.Ordinal) && counts.GetValueOrDefault("MEN") is 2 or 3) counts["MEN"] = 0;
        return counts;
    }
}

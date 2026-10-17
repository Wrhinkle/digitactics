using System.Text.Json;

namespace DigiTactics.Rules;

public sealed record UnitLine(string Id, int Tier, int Price, int Copies, string[] Forms, string[] Traits,
    string Attribute, int Slots, int SellPremium, CombatTemplate Combat, AbilityTemplate Ability, string? Summon = null, string[]? Rivals = null, string? Mark = null);
public sealed record ItemDefinition(string Id, Dictionary<string, double> Stats, Dictionary<string, JsonElement> Passives, string[] Hooks, string? Emblem, bool Component = false, bool Special = false, string? Category = null, string[]? CompletedFrom = null);
public sealed record TraitDefinition(string Id, string Kind, int[] Breakpoints, double[] Values, Dictionary<string, double[]> Parameters, JsonElement Duo);
public sealed record PartnerStats(double Hp, double BonusHp, double BonusAp, double BonusAd, double BonusMana);
public sealed record Ruleset(string Id, string Name, string Pace, UnitLine[] Lines, int[] PoolSizes, int[]?[] Odds, int RerollCost, int ShopSize,
    ItemDefinition[] Items, string[] CoreItems, TraitDefinition[] Traits, UnitLine[] ExtraLines, PartnerStats? Partner = null, double[]? PveMultipliers = null, Dictionary<string, int>? DragonTraitLimits = null);
public sealed record ContentCatalog(int SchemaVersion, Ruleset[] Variants)
{
    public static ContentCatalog Load(string path)
    {
        var catalog = JsonSerializer.Deserialize<ContentCatalog>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("Missing content catalog.");
        if (catalog.SchemaVersion != 11 || catalog.Variants.Length == 0)
            throw new InvalidDataException("Unsupported or empty content catalog.");
        foreach (var rules in catalog.Variants)
        {
            if (rules.Lines.Select(l => l.Id).Distinct().Count() != rules.Lines.Length)
                throw new InvalidDataException($"Duplicate unit IDs in {rules.Id}.");
            foreach (var line in rules.Lines)
                if (line.Tier <= 0 || line.Tier >= rules.PoolSizes.Length || line.Copies < 2 || line.Forms.Length < 3 ||
                    line.Combat is null || line.Ability is null || line.Slots < 1 || line.Combat.Health <= 0 || line.Combat.AttackSpeed <= 0)
                    throw new InvalidDataException($"Invalid unit definition: {rules.Id}/{line.Id}.");
        }
        return catalog;
    }
}

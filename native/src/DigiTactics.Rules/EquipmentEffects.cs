using System.Text.Json;

namespace DigiTactics.Rules;

internal sealed class Lifeline(double threshold, double shield, double speed, double attack, double armor)
{
    internal double Threshold = threshold, Shield = shield, Speed = speed, Attack = attack, Armor = armor;
    internal bool Used;
}

/// <summary>Compiled per-unit effects. JSON is read only during equipment setup, never during a combat tick.</summary>
internal sealed class EquipmentEffects
{
    internal double Amp, Slayer, Shojin, Rage, CastSpeed, Volt, Split, Kraken, Shred, Archangel, Zap,
        ManaRegen, Thorns, Gargoyle, Reduction, Regen, Veil, CrowdControlImmunity, GuardBreaker;
    internal bool AbilityCrit, Titan, Crown, Burn, BlueBuff, Redemption, Sunfire;
    internal List<Lifeline> Lifelines { get; } = [];

    internal void Apply(CombatUnit unit, IReadOnlyDictionary<string, ItemDefinition> definitions, string[] pool, DeterministicRandom random)
    {
        var equipped = new List<string>();
        foreach (string id in unit.Entry.Items ?? [])
        {
            equipped.Add(id);
            if (definitions.TryGetValue(id, out var item) && item.Passives.TryGetValue("thief", out var thief) && thief.GetBoolean())
                for (int k = 0; k < 2; k++) equipped.Add(pool[(int)(random.Next() * pool.Length)]);
        }
        unit.Items = equipped.AsReadOnly();
        var stats = new Dictionary<string, double>();
        var numbers = new Dictionary<string, double>();
        var flags = new Dictionary<string, bool>();
        foreach (string id in equipped)
        {
            if (!definitions.TryGetValue(id, out var item)) continue;
            foreach (var (key, value) in item.Stats) stats[key] = stats.GetValueOrDefault(key) + value;
            foreach (var (key, value) in item.Passives)
            {
                if (key == "lifeline")
                {
                    double V(string name) => value.TryGetProperty(name, out var v) ? v.GetDouble() : 0;
                    Lifelines.Add(new(V("at"), V("shield"), V("as"), V("ad"), V("armor")));
                }
                else if (value.ValueKind == JsonValueKind.Number) numbers[key] = numbers.GetValueOrDefault(key) + value.GetDouble();
                else if (value.ValueKind is JsonValueKind.True or JsonValueKind.False) flags[key] = value.GetBoolean();
            }
        }
        double S(string key) => stats.GetValueOrDefault(key);
        double N(string key) => numbers.GetValueOrDefault(key);
        bool B(string key) => flags.GetValueOrDefault(key);
        unit.AttackDamage += unit.BaseAttackDamage * S("ad"); unit.AttackSpeed *= 1 + S("as");
        unit.AbilityPower += S("ap"); unit.Mana += S("mana"); unit.Armor += S("armor"); unit.MagicResist += S("mr");
        unit.MaxHealth += S("hp"); unit.CriticalChance += S("crit"); unit.Dodge += S("dodge");
        unit.Omnivamp += S("omnivamp") + N("vamp"); unit.CriticalDamage += N("critDmg");
        Amp = N("amp"); Slayer = N("slayer"); Shojin = N("shojin"); Rage = N("rage"); CastSpeed = N("castAS");
        Volt = N("volt"); Split = N("split"); Kraken = N("kraken"); Shred = N("shred"); Archangel = N("archangel");
        Zap = N("zap"); ManaRegen = N("manaRegen"); Thorns = N("thorns"); Gargoyle = N("gargoyle");
        Reduction = N("dr"); Regen = N("regen"); Veil = N("veil"); CrowdControlImmunity = N("ccImmune"); GuardBreaker = N("guardbreak");
        AbilityCrit = B("abilCrit"); Titan = B("titan"); Crown = B("crown"); Burn = B("burn");
        BlueBuff = B("bluebuff"); Redemption = B("redemption"); Sunfire = B("sunfire");
        if (BlueBuff) unit.MaxMana = Math.Max(20, unit.MaxMana - 10);
    }
}

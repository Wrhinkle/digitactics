using System.Text.Json;

namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    // Temporary upgrades add the stat difference but do not equip or register the generated item's hooks.
    private void ApplyTemporaryItemUpgrade(CombatUnit u, ItemDefinition item, ItemDefinition? component = null, bool preserveExistingBlueBuff = false)
    {
        double S(string key) => item.Stats.GetValueOrDefault(key) - (component?.Stats.GetValueOrDefault(key) ?? 0);
        u.AttackDamage += u.BaseAttackDamage * S("ad"); u.AttackSpeed *= 1 + S("as");
        u.AbilityPower += S("ap"); u.Mana += S("mana"); u.Armor += S("armor"); u.MagicResist += S("mr");
        u.MaxHealth += S("hp"); u.CriticalChance += S("crit"); u.Dodge += S("dodge"); u.Omnivamp += S("omnivamp");
        foreach (var (key, value) in item.Passives)
        {
            if (key == "lifeline") {
                double V(string name) => value.TryGetProperty(name, out var v) ? v.GetDouble() : 0;
                u.Equipment.Lifelines.Add(new(V("at"), V("shield"), V("as"), V("ad"), V("armor"))); continue;
            }
            if (key is "thief" or "teamSize" or "emblem") continue;
            if (value.ValueKind == JsonValueKind.Number) {
                double n = value.GetDouble();
                switch (key) {
                    case "amp": u.Equipment.Amp += n; break;
                    case "slayer": u.Equipment.Slayer += n; break;
                    case "shojin": u.Equipment.Shojin += n; break;
                    case "rage": u.Equipment.Rage += n; break;
                    case "castAS": u.Equipment.CastSpeed += n; break;
                    case "volt": u.Equipment.Volt += n; break;
                    case "split": u.Equipment.Split += n; break;
                    case "kraken": u.Equipment.Kraken += n; break;
                    case "shred": u.Equipment.Shred += n; break;
                    case "archangel": u.Equipment.Archangel += n; break;
                    case "zap": u.Equipment.Zap += n; break;
                    case "manaRegen": u.Equipment.ManaRegen += n; break;
                    case "thorns": u.Equipment.Thorns += n; break;
                    case "gargoyle": u.Equipment.Gargoyle += n; break;
                    case "dr": u.Equipment.Reduction += n; break;
                    case "regen": u.Equipment.Regen += n; break;
                    case "veil": u.Equipment.Veil += n; break;
                    case "ccImmune": u.Equipment.CrowdControlImmunity += n; break;
                    case "guardbreak": u.Equipment.GuardBreaker += n; break;
                    case "vamp": u.Omnivamp += n; break;
                    case "critDmg": u.CriticalDamage += n; break;
                    default: throw new NotSupportedException($"Temporary item passive {key} is not supported.");
                }
            } else if (value.ValueKind is JsonValueKind.True or JsonValueKind.False) {
                bool flag = value.GetBoolean();
                switch (key) {
                    case "abilCrit": u.Equipment.AbilityCrit = flag; break;
                    case "titan": u.Equipment.Titan = flag; break;
                    case "crown": u.Equipment.Crown = flag; break;
                    case "burn": u.Equipment.Burn = flag; break;
                    case "bluebuff": if (flag && (!preserveExistingBlueBuff || !u.Equipment.BlueBuff)) u.MaxMana = Math.Max(20, u.MaxMana - 10); u.Equipment.BlueBuff = flag; break;
                    case "redemption": u.Equipment.Redemption = flag; break;
                    case "sunfire": u.Equipment.Sunfire = flag; break;
                    default: throw new NotSupportedException($"Temporary item flag {key} is not supported.");
                }
            }
        }
    }
}

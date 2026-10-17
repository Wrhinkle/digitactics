namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private void BindArcaneAugments(int team, string[] ids, int activeTraits, int? owner = null, double partnerTop = 0)
    {
        foreach (string id in ids)
        {
            var context = Hooks.Context(CombatHookSource.Augment, id, team, owner: owner);
            if (DuosCombatAugments.TryBind(Hooks, context, partnerTop)) continue;
            void Start(Action<CombatUnit> action) => Hooks.Add(context, CombatHook.CombatStart, (c, _) => { foreach (var u in c.Mine) action(u); });
            switch (id)
            {
                case "arc_a_training": Start(u => { u.AttackDamage += u.BaseAttackDamage * 0.12; u.AbilityPower += 12; }); break;
                case "arc_a_wind":
                    bool done = false;
                    Hooks.Add(context, CombatHook.Tick, (c, _) => {
                        if (done || Time < 10) return; done = true;
                        // Legacy survivors(team) includes summoned units and both partners.
                        foreach (var u in units.Where(u => u.Alive && u.Team == team)) c.Heal(u, (u.MaxHealth - u.Health) * 0.35);
                    }, order: -1); break;
                case "arc_a_feather": Start(u => { if (u.Line.Tier <= 2) u.AttackSpeed *= 1.25; }); break;
                case "arc_a_front": Start(u => { if (u.Entry.Row == 4) { u.Armor += 30; u.MagicResist += 30; u.MaxHealth += 200; } }); break;
                case "arc_a_back": Start(u => { if (u.Entry.Row >= 6) u.AttackSpeed *= 1.25; }); break;
                case "arc_a_blessing": Start(u => u.Omnivamp += 0.12); break;
                case "arc_a_lotus": Start(u => { u.Equipment.AbilityCrit = true; u.CriticalChance += 0.15; }); break;
                case "arc_a_overcharge": Start(u => u.Mana += 25); break;
                case "arc_a_unity": Start(u => { u.AttackDamage += u.BaseAttackDamage * 0.03 * activeTraits; u.AbilityPower += 3 * activeTraits; }); break;
                case "arc_a_giant": Start(u => u.MaxHealth = UnitValues.Round(u.MaxHealth * 1.25)); break;
                // No combat callbacks; pick grants, planning and settlement are tracked separately.
                case "arc_a_coins": case "arc_a_cache": case "arc_a_rolls": case "arc_a_heart": case "arc_a_shimmer":
                case "arc_a_bank": case "arc_a_trove": case "arc_a_tag": case "arc_a_device": case "arc_a_income":
                case "arc_a_evolve": case "arc_a_find": case "arc_a_forge": case "arc_a_jackpot": case "arc_a_armory":
                case "arc_a_crown": case "arc_a_mega": case "arc_a_mutant": case "arc_a_workshop": case "arc_a_market": break;
                default: throw new NotSupportedException($"Arcane augment {id} has not been migrated.");
            }
        }
    }
}

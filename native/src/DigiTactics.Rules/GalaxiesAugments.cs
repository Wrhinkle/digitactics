namespace DigiTactics.Rules;

internal static class GalaxiesAugments
{
    internal static void Bind(CombatSimulation combat, int team, string[] ids, int? owner = null, double partnerTop = 0)
    {
        foreach (string id in ids)
        {
            var context = combat.Hooks.Context(CombatHookSource.Augment, id, team, owner: owner);
            if (DuosCombatAugments.TryBind(combat.Hooks, context, partnerTop)) continue;
            void Start(Action<CombatUnit> action) => combat.Hooks.Add(context, CombatHook.CombatStart,
                (c, _) => { foreach (var u in c.Mine) action(u); });
            switch (id)
            {
                case "gal_a_training": Start(u => { u.AttackDamage += u.BaseAttackDamage * 0.12; u.AbilityPower += 12; }); break;
                case "gal_a_front": Start(u => { if (u.Entry.Row == 4) { u.Armor += 30; u.MagicResist += 30; u.MaxHealth += 200; } }); break;
                case "gal_a_back": Start(u => { if (u.Entry.Row >= 6) u.AttackSpeed *= 1.25; }); break;
                case "gal_a_starlight":
                    bool used = false;
                    combat.Hooks.Add(context, CombatHook.Tick, (c, _) => {
                        if (used || c.Time < 10) return; used = true;
                        // The shipped hook heals the entire team even when owned by one Duos partner.
                        foreach (var u in combat.Units.Where(u => u.Team == team && u.Alive)) c.Heal(u, (u.MaxHealth - u.Health) * 0.3);
                    }); break;
                case "gal_a_blessing": Start(u => u.Omnivamp += 0.12); break;
                case "gal_a_lotus": Start(u => { u.Equipment.AbilityCrit = true; u.CriticalChance += 0.15; }); break;
                case "gal_a_implants": Start(u => { if (u.Items.Count > 0) { u.MaxHealth += 250; u.AttackDamage += u.BaseAttackDamage * 0.15; } }); break;
                case "gal_a_giant": Start(u => u.MaxHealth = UnitValues.Round(u.MaxHealth * 1.25)); break;
                case "gal_a_overcharge": Start(u => { u.Mana += 30; u.AbilityPower += 20; }); break;
                // Count-only and planning/economy hooks have no battle callbacks.
                case "gal_a_stars": case "gal_a_cash": case "gal_a_cache": case "gal_a_rolls": case "gal_a_bank":
                case "gal_a_trove": case "gal_a_mental": case "gal_a_income": case "gal_a_jackpot": case "gal_a_armory":
                case "gal_a_golden": case "gal_a_megaEvolve": break;
                default: throw new NotSupportedException($"Galaxies augment {id} has not been migrated.");
            }
        }
    }
}

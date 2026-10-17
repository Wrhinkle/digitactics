namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private void BindColiseumAugments(int team, string[] ids, int activeTraits, int? owner = null, double partnerTop = 0)
    {
        foreach (string id in ids)
        {
            var context = Hooks.Context(CombatHookSource.Augment, id, team, owner: owner);
            if (DuosCombatAugments.TryBind(Hooks, context, partnerTop)) continue;
            void Start(Action<CombatUnit> action) => Hooks.Add(context, CombatHook.CombatStart, (c, _) => { foreach (var u in c.Mine) action(u); });
            switch (id)
            {
                case "col_a_warmup": Start(u => { u.AttackDamage += u.BaseAttackDamage * 0.12; u.AbilityPower += 12; }); break;
                case "col_a_second":
                    bool done = false;
                    Hooks.Add(context, CombatHook.Tick, (c, _) => {
                        if (done || Time < 10) return; done = true;
                        // Legacy survivors(team) includes summoned units and both partners.
                        foreach (var u in units.Where(u => u.Alive && u.Team == team)) c.Heal(u, (u.MaxHealth - u.Health) * 0.35);
                    }); break;
                case "col_a_ascension":
                    bool ascended = false;
                    Hooks.Add(context, CombatHook.Tick, (_, _) => {
                        if (ascended || Time < 15) return; ascended = true;
                        foreach (var u in units.Where(u => u.Alive && u.Team == team)) u.Equipment.Amp += 0.4;
                    }); break;
                case "col_a_front": Start(u => { if (u.Entry.Row == 4) { u.Armor += 30; u.MagicResist += 30; u.MaxHealth += 200; } }); break;
                case "col_a_ringside": Start(u => { if (u.Entry.Row >= 6) u.AttackSpeed *= 1.25; }); break;
                case "col_a_titan": Start(u => u.MaxHealth = UnitValues.Round(u.MaxHealth * 1.15)); break;
                case "col_a_lotus": Start(u => { u.Equipment.AbilityCrit = true; u.CriticalChance += 0.15; }); break;
                case "col_a_finals": Start(u => { u.Equipment.Amp += 0.12; u.Equipment.Reduction += 0.1; }); break;
                case "col_a_unity": Start(u => { u.AttackDamage += u.BaseAttackDamage * 0.03 * activeTraits; u.AbilityPower += 3 * activeTraits; }); break;
                case "col_a_giant": Start(u => u.MaxHealth = UnitValues.Round(u.MaxHealth * 1.25)); break;
                // Arena is applied by TraitRules; other entries have no combat callbacks.
                // Pick grants, planning and settlement remain separate migration work.
                case "col_a_coins": case "col_a_band": case "col_a_cache": case "col_a_rolls": case "col_a_heart": case "col_a_snack": case "col_a_prize": case "col_a_trove": case "col_a_sponsor": case "col_a_crest": case "col_a_tags": case "col_a_snacktime": case "col_a_evolve": case "col_a_artifact": case "col_a_jackpot": case "col_a_armory": case "col_a_golden": case "col_a_mega": case "col_a_feast": case "col_a_arena": break;
                default: throw new NotSupportedException($"Coliseum augment {id} has not been migrated.");
            }
        }
    }
}

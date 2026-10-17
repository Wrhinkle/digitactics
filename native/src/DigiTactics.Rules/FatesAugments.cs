namespace DigiTactics.Rules;

internal static class FatesAugments
{
    internal static void Bind(CombatSimulation combat, int team, string[] ids, int activeTraits, int? owner = null, double partnerTop = 0)
    {
        foreach (string id in ids)
        {
            var context = combat.Hooks.Context(CombatHookSource.Augment, id, team, owner: owner);
            if (DuosCombatAugments.TryBind(combat.Hooks, context, partnerTop)) continue;
            void Start(Action<CombatUnit> action) => combat.Hooks.Add(context, CombatHook.CombatStart,
                (c, _) => { foreach (var u in c.Mine.Where(u => !u.Summoned)) action(u); });
            switch (id)
            {
                case "fat_dojo": Start(u => { u.AttackDamage += u.BaseAttackDamage * 0.12; u.AbilityPower += 12; }); break;
                case "fat_lanterns": Start(u => u.Mana += 20); break;
                case "fat_wall": Start(u => { if (u.Entry.Row == 4) { u.Armor += 30; u.MagicResist += 30; u.MaxHealth += 200; } }); break;
                case "fat_nest": Start(u => { if (u.Entry.Row >= 6) u.AttackSpeed *= 1.25; }); break;
                case "fat_bond": Start(u => { if (u.Entry.Partner is not null) { u.MaxHealth += 300; u.AbilityPower += 25; } }); break;
                case "fat_shrine": Start(u => u.Omnivamp += 0.12); break;
                case "fat_united": Start(u => { u.AttackDamage += u.BaseAttackDamage * 0.03 * activeTraits; u.AbilityPower += 3 * activeTraits; }); break;
                case "fat_grace": combat.Hooks.Add(context, CombatHook.DamageTaken, (c, e) => {
                    var u = e.Unit!;
                    if (u.Team != c.Team || u.FatesGrace || u.Health >= u.MaxHealth * 0.5) return;
                    u.FatesGrace = true; c.Buff(u, CombatStat.DamageReduction, 0.3, 4);
                }); break;
                case "fat_giant": Start(u => u.MaxHealth = UnitValues.Round(u.MaxHealth * 1.25)); break;
                case "fat_soulmates": Start(u => { if (u.Entry.Partner is not null) { u.MaxHealth = UnitValues.Round(u.MaxHealth * 1.5); u.AbilityPower += 50; } }); break;
                case "fat_crowns": combat.Hooks.Add(context, CombatHook.CombatStart, (c, _) => {
                    var unit = c.Mine.Where(u => !u.Summoned && u.Stage < 2).OrderBy(u => u.Stage).ThenByDescending(u => u.Items.Count).ThenBy(u => u.Id).FirstOrDefault();
                    if (unit is not null) c.Grow(unit);
                }, -4); break;
                // Count-only or planning/economy effects; those game hooks are migrated with match flow.
                case "fat_twin": case "fat_envelope": case "fat_stall": case "fat_dice_aug": case "fat_charm": case "fat_seeker":
                case "fat_pouch": case "fat_chest": case "fat_egghunt": case "fat_offerings": case "fat_tome": case "fat_apprentice":
                case "fat_digivolve": case "fat_fortune": case "fat_grandarmory": case "fat_golden": case "fat_master": case "fat_mega": case "fat_festival": break;
                default: throw new NotSupportedException($"Fates augment {id} has not been migrated.");
            }
        }
    }
}

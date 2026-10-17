namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private void BindReckoningAugments(int team, string[] ids, int activeTraits, int? owner = null, double partnerTop = 0)
    {
        foreach (string id in ids)
        {
            var context = Hooks.Context(CombatHookSource.Augment, id, team, owner: owner);
            if (DuosCombatAugments.TryBind(Hooks, context, partnerTop)) continue;
            void Start(Action<CombatUnit> action) => Hooks.Add(context, CombatHook.CombatStart, (c, _) => { foreach (var u in c.Mine) action(u); });
            switch (id)
            {
                case "rec_a_training": Start(u => { u.AttackDamage += u.BaseAttackDamage * 0.1; u.AbilityPower += 10; }); break;
                case "rec_a_mist": Hooks.Add(context, CombatHook.Death, (c, e) => {
                    var u = e.Unit!; if (!u.Summoned) foreach (var t in EnemiesNear(u, u.Cell, 2)) c.Damage(u, t, 150);
                }); break;
                case "rec_a_front": Start(u => { if (u.Entry.Row == 4) { u.Armor += 25; u.MagicResist += 25; u.MaxHealth += 150; } }); break;
                case "rec_a_nest": Start(u => { if (u.Entry.Row >= 6) u.AttackSpeed *= 1.2; }); break;
                case "rec_a_xanti": Start(u => { if (u.Items.Any(item => items.TryGetValue(item, out var def) && def.Category == "shadow")) u.MaxHealth += 150; }); break;
                case "rec_a_vamp": Start(u => u.Omnivamp += 0.12); break;
                case "rec_a_mana": Start(u => u.Mana += 25); break;
                case "rec_a_unity": Start(u => { u.AttackDamage += u.BaseAttackDamage * 0.03 * activeTraits; u.AbilityPower += 3 * activeTraits; }); break;
                case "rec_a_ascend":
                    bool done = false;
                    Hooks.Add(context, CombatHook.Tick, (_, _) => {
                        if (done || Time < 12) return; done = true;
                        // The original callback affects both partners' surviving units.
                        foreach (var u in units.Where(u => u.Alive && u.Team == team)) u.Equipment.Amp += 0.35;
                    }); break;
                case "rec_a_giant": Start(u => u.MaxHealth = UnitValues.Round(u.MaxHealth * 1.25)); break;
                // These hooks belong to planning, offers and round settlement.
                case "rec_a_coins": case "rec_a_cache": case "rec_a_scan": case "rec_a_heart": case "rec_a_bank":
                case "rec_a_trove": case "rec_a_egg": case "rec_a_income": case "rec_a_jackpot": case "rec_a_blessing":
                case "rec_a_armory": case "rec_a_nature": case "rec_a_darkness": case "rec_a_tome": break;
                default: throw new NotSupportedException($"Reckoning augment {id} has not been migrated.");
            }
        }
    }
}

namespace DigiTactics.Rules;

internal static class FactionWarsAugments
{
    internal static void Bind(CombatSimulation combat, int team, string[] ids, int activeTraits, int? owner = null, double partnerTop = 0)
    {
        foreach (string id in ids)
        {
            var context = combat.Hooks.Context(CombatHookSource.Augment, id, team, owner: owner);
            void Start(Action<CombatUnit> action) => combat.Hooks.Add(context, CombatHook.CombatStart,
                (c, _) => { foreach (var unit in c.Mine) action(unit); });
            switch (id)
            {
                case "training": Start(u => { u.AttackDamage += u.BaseAttackDamage * 0.12; u.AbilityPower += 12; }); break;
                case "blessing": Start(u => u.Omnivamp += 0.12); break;
                case "lotus": Start(u => { u.Equipment.AbilityCrit = true; u.CriticalChance += 0.15; }); break;
                case "implants": Start(u => { if (u.Items.Count > 0) { u.MaxHealth += 250; u.AttackDamage += u.BaseAttackDamage * 0.15; } }); break;
                case "unity": Start(u => { u.AttackDamage += u.BaseAttackDamage * 0.03 * activeTraits; u.AbilityPower += 3 * activeTraits; }); break;
                case "featherweights": Start(u => { if (u.Line.Tier <= 2) u.AttackSpeed *= 1.25; }); break;
                case "frontline": Start(u => { if (u.Entry.Row == 4) { u.Armor += 30; u.MagicResist += 30; u.MaxHealth += 200; } }); break;
                case "backline": Start(u => { if (u.Entry.Row >= 6) u.AttackSpeed *= 1.25; }); break;
                case "overcharge": Start(u => u.Mana += 25); break;
                case "giant": Start(u => u.MaxHealth = UnitValues.Round(u.MaxHealth * 1.25)); break;
                case "thrill": combat.Hooks.Add(context, CombatHook.Kill, (c, e) => { if (e.Unit!.Alive) c.Heal(e.Unit, 350); }); break;
                case "secondWind":
                    bool done = false;
                    combat.Hooks.Add(context, CombatHook.Tick, (c, _) => {
                        if (done || c.Time < 10) return;
                        done = true;
                        foreach (var u in combat.Units.Where(u => u.Alive && u.Team == c.Team)) c.Heal(u, (u.MaxHealth - u.Health) * 0.35);
                    }, -1); break;
                case "duo_ace": Start(u => { double n = Math.Min(10, partnerTop); u.AttackDamage += u.BaseAttackDamage * 0.03 * n; u.AbilityPower += 3 * n; }); break;
                case "duo_sync": Start(u => u.Equipment.Amp += 0.08); break;
                case "duo_tag":
                    combat.Hooks.Add(context, CombatHook.AnyDeath, (c, e) => {
                        var dead = e.Unit!;
                        if (dead.Team != c.Team || dead.Entry.Owner is null || dead.Entry.Owner == c.Owner || c.Owner is null) return;
                        foreach (var u in c.Mine.Where(u => u.Alive)) { c.Heal(u, u.MaxHealth * 0.1); c.Buff(u, CombatStat.AttackSpeed, 0.15, 4); }
                    }); break;
                case "duo_crest_link": case "duo_wide_portal": case "duo_proud": case "duo_gear": case "duo_interest":
                case "duo_care": case "duo_guard": case "duo_trove": break;
                // These affect planning/economy or trait counts, not the combat hook table.
                case "crest": case "pocket": case "bank": case "jackpot": case "cache": case "trove": case "armory":
                case "egg": case "goldenEgg": case "heart": case "rolls": case "rolls2": case "income": case "evolve": case "megaEvolve": break;
                default: throw new NotSupportedException($"Faction Wars augment {id} has not been migrated.");
            }
        }
    }
}

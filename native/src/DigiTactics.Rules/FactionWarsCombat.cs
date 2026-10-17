namespace DigiTactics.Rules;

/// <summary>Native implementations of faction-wars-mech.js combat traits; numeric tuning stays in the catalog.</summary>
internal static class FactionWarsCombat
{
    internal static void Bind(CombatSimulation combat, Ruleset rules, CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
    {
        if (!rules.Id.StartsWith("faction-wars/", StringComparison.Ordinal))
            throw new NotSupportedException($"Production combat hooks for {rules.Id} have not been migrated.");
        for (int team = 0; team < 2; team++)
        {
            var side = team == 0 ? setup.SideA : setup.SideB;
            var counts = side.NoTraits ? [] : side.Counts ?? TraitRules.Count(rules, team == 0 ? sideA : sideB, side.Augments);
            int active = counts.Count(p => rules.Traits.Any(t => t.Id == p.Key && TraitRules.Level(t, p.Value) > 0));
            foreach (var (id, count) in counts)
            {
                var trait = rules.Traits.FirstOrDefault(t => t.Id == id);
                if (trait is null) continue;
                int level = TraitRules.Level(trait, count);
                if (level == 0) continue;
                var context = combat.Hooks.Context(CombatHookSource.Trait, id, team);
                double value = trait.Values[level - 1];
                double Parameter(string name) => trait.Parameters[name][level - 1];
                var members = combat.Units.Where(u => u.Team == team && u.Traits.Contains(id)).ToArray();
                void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action, int order = 0) => combat.Hooks.Add(context, hook, action, order);
                void Each(Action<CombatUnit> action) => On(CombatHook.CombatStart, (_, _) => { foreach (var u in members) action(u); });
                void All(Action<CombatUnit> action) => On(CombatHook.CombatStart, (c, _) => { foreach (var u in c.Mine) action(u); });
                bool Member(CombatUnit u) => members.Contains(u);
                switch (id)
                {
                    case "DR": Each(u => u.MaxHealth += value); break;
                    case "NSp": Each(u => u.AttackSpeed *= 1 + value); break;
                    case "JT": Each(u => u.Equipment.Regen += value); break;
                    case "WG": All(u => u.Dodge += value); break;
                    case "DS": Each(u => u.Shield += u.MaxHealth * value); break;
                    case "ME": Each(u => u.Equipment.Reduction += value); break;
                    case "NSo": Each(u => u.Omnivamp += value); break;
                    case "VB": All(u => u.Mana = Math.Min(u.MaxMana - 1, u.Mana + value)); break;
                    case "DL": Each(u => { u.AbilityPower += value; if (level >= 2) u.Equipment.Amp += level >= 3 ? 0.3 : 0.15; if (level >= 3) u.AbilityBurn = true; }); break;
                    case "RK":
                        On(CombatHook.CombatReady, (c, _) => { foreach (var u in c.Mine.Where(u => u.Traits.Contains(id)))
                            { u.MaxHealth = UnitValues.Round(u.MaxHealth * (1 + value * active)); u.Equipment.Amp += value * active; } }, 1); break;
                    case "DATS":
                        bool charged = false;
                        void Charge(CombatHookContext c)
                        {
                            charged = true;
                            foreach (var u in c.Mine.Where(u => u.Alive && u.Traits.Contains(id)))
                            { c.Heal(u, u.MaxHealth * Parameter("heal")); u.AttackDamage += u.BaseAttackDamage * value; u.AbilityPower += UnitValues.Round(value * 100); }
                        }
                        On(CombatHook.Tick, (c, _) => { if (!charged && c.Time >= 10) Charge(c); }, 1);
                        On(CombatHook.Death, (c, _) => { if (!charged) Charge(c); }); break;
                    case "LW":
                        var evolved = new HashSet<int>();
                        On(CombatHook.DamageTaken, (c, e) => {
                            var u = e.Unit!;
                            if (!Member(u) || evolved.Contains(u.Id) || u.Health >= u.MaxHealth * 0.5) return;
                            evolved.Add(u.Id); c.Heal(u, u.MaxHealth * value); c.Buff(u, CombatStat.AttackSpeed, value); u.Equipment.Reduction += Parameter("dr");
                        }); break;
                    case "XF":
                        On(CombatHook.CombatReady, (c, _) => {
                            var mem = c.Mine.Where(u => u.Traits.Contains(id)).ToArray();
                            double hp = mem.Sum(u => u.MaxHealth), ad = mem.Sum(u => u.AttackDamage);
                            foreach (var u in mem) { u.MaxHealth = UnitValues.Round(u.MaxHealth + (hp - u.MaxHealth) * value); u.AttackDamage += (ad - u.AttackDamage) * value; }
                        }); break;
                    case "INS":
                        double next = 2;
                        On(CombatHook.Tick, (c, _) => {
                            if (c.Time < next) return; next += 2;
                            var sources = c.Mine.Where(u => u.Alive && u.Traits.Contains(id)).ToArray();
                            if (sources.Length == 0) return;
                            for (int i = 0; i < level; i++) {
                                var enemies = combat.Units.Where(u => u.Alive && u.Team != c.Team).ToArray();
                                if (enemies.Length == 0) break;
                                var target = enemies[(int)(c.Random() * enemies.Length)];
                                c.Damage(sources[i % sources.Length], target, value); c.Blind(target);
                            }
                        }, 2); break;
                    case "FUN":
                        On(CombatHook.Attack, (c, e) => { if (Member(e.Unit!) && e.Unit!.Attacks % 2 == 0 && e.Target!.Alive) c.Weaken(e.Target, value, 3); }); break;
                    case "KN":
                        On(CombatHook.BeforeDamageTaken, (c, e) => {
                            var u = e.Target!;
                            if (Member(u)) e.Reduction += value * (combat.Units.Any(a => a != u && a.Alive && a.Team == u.Team && Member(a) && a.Cell.DistanceTo(u.Cell) == 1) ? 2 : 1);
                        }); break;
                    case "FL": Each(u => { u.AbilityBurn = true; u.BonusAgainstBurning = value; }); break;
                    case "FR": On(CombatHook.Attack, (c, e) => { if (Member(e.Unit!) && e.Target!.Alive && c.Random() < value) c.Stun(e.Target, 1); }); break;
                    case "UND":
                        var revived = new HashSet<int>();
                        On(CombatHook.Lethal, (c, e) => {
                            var u = e.Unit!;
                            if (!Member(u) || !revived.Add(u.Id)) return;
                            u.Health = 1; u.InvulnerableUntil = c.Time + 1; c.After(1, () => c.Heal(u, u.MaxHealth * value), u);
                        }); break;
                    case "HB": All(u => { double n = value * (u.Traits.Contains(id) ? 2 : 1); u.Armor += n; u.MagicResist += n; u.AbilityPower += n; }); break;
                    case "FA": On(CombatHook.CombatStart, (c, _) => { foreach (var u in combat.Units.Where(u => u.Team != c.Team)) { u.Armor -= value; u.MagicResist -= value; } }); break;
                    case "Guardian": Each(u => { u.Armor += value; u.MagicResist += value; }); break;
                    case "Striker": Each(u => u.AttackDamage *= 1 + value); break;
                    case "Sniper": Each(u => u.PerHexAmplification = value); break;
                    case "Mystic": Each(u => u.AbilityPower += value); break;
                    case "Assassin": Each(u => u.CriticalChance += value); break;
                    case "Bruiser": Each(u => u.MaxHealth = UnitValues.Round(u.MaxHealth * (1 + value))); break;
                    case "Duelist": Each(u => u.AttackSpeedPerAttack = value); break;
                    case "Support":
                        On(CombatHook.CombatStart, (c, _) => combat.HealingAmplification[c.Team!.Value] = value);
                        On(CombatHook.UnitTick, (c, e) => {
                            var u = e.Unit!;
                            if (!Member(u) || !c.Every(u, "support", 4)) return;
                            var target = combat.Units.Where(a => a.Alive && a.Team == u.Team).OrderBy(a => a.Health / a.MaxHealth).FirstOrDefault();
                            if (target is not null) c.Heal(target, target.MaxHealth * Parameter("heal"));
                        }); break;
                    default: throw new NotSupportedException($"Faction Wars trait {id} has not been migrated.");
                }
            }
            FactionWarsAugments.Bind(combat, team, side.Augments ?? [], active);
        }
    }
}

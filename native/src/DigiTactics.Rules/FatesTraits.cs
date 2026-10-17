namespace DigiTactics.Rules;

internal static class FatesTraits
{
    private sealed class Ascension { internal bool Used; internal int Attacks; internal double Until = -1; }
    private sealed class Pushups { internal bool Used, Done, Pumped; }

    internal static void Bind(CombatSimulation combat, Ruleset rules, CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
    {
        for (int team = 0; team < 2; team++)
        {
            var side = team == 0 ? setup.SideA : setup.SideB;
            var entries = team == 0 ? sideA : sideB;
            var counts = side.NoTraits ? [] : side.Counts ?? (side.Parts is { } parts ? TraitRules.TeamCount(rules, entries, parts) : TraitRules.Count(rules, entries, side.Augments));
            foreach (var (id, count) in counts)
            {
                var trait = rules.Traits.FirstOrDefault(t => t.Id == id);
                if (trait is null) continue;
                int level = TraitRules.Level(trait, count);
                if (level == 0) continue;
                var context = combat.Hooks.Context(CombatHookSource.Trait, id, team);
                double value = trait.Values[level - 1];
                double P(string key) => trait.Parameters[key][level - 1];
                var members = combat.Units.Where(u => u.Team == team && u.Traits.Contains(id)).ToArray();
                bool Member(CombatUnit u) => u.Traits.Contains(id);
                void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action, int order = 0) => combat.Hooks.Add(context, hook, action, order);
                void Each(Action<CombatUnit> action) => On(CombatHook.CombatStart, (_, _) => { foreach (var u in members) action(u); });
                void All(Action<CombatUnit> action) => On(CombatHook.CombatStart, (c, _) => { foreach (var u in c.Mine) action(u); });
                switch (id)
                {
                    case "NIGHT": All(u => u.AbilityPower += value + (Member(u) ? P("own") : 0)); break;
                    case "TREASURE": Each(u => u.Dodge += value); break;
                    case "BRAWLER": Each(u => u.MaxHealth += value); break;
                    case "VANGUARD": Each(u => u.Armor += value); break;
                    case "WARDER": All(u => u.MagicResist += value); break;
                    case "ASSASSIN": Each(u => { u.CriticalChance += value; u.CriticalDamage += P("critDmg"); u.Equipment.AbilityCrit = true; }); break;
                    case "DUELIST": Each(u => { u.AttackSpeedPerAttack = value; u.AttackSpeedStackLimit = 8; }); break;
                    case "DEVA": Each(u => { double m = 1 + 0.1 * Math.Min(5, side.Fates?.Deva ?? 0); u.MaxHealth += UnitValues.Round(value * m); u.AbilityPower += UnitValues.Round(P("ap") * m); }); break;
                    case "NINJA": if (count == 1 || count == 4) Each(u => { u.AttackDamage += u.BaseAttackDamage * value; u.AbilityPower += UnitValues.Round(value * 100); }); break;
                    case "FORGE": break; // Planning reward; no combat hook.
                    case "LUNAR": On(CombatHook.CombatStart, (c, _) => {
                        foreach (var u in members.Where(u => u.Stage < 2).OrderBy(u => u.Stage).ThenByDescending(u => u.Items.Count).ThenBy(u => u.Id).Take((int)value)) c.Grow(u);
                    }, -5); break;
                    case "ADEPT": On(CombatHook.CombatStart, (c, _) => { foreach (var u in combat.Units.Where(u => u.Team != c.Team)) c.Buff(u, CombatStat.AttackSpeed, -0.5, value); }); break;
                    case "FOREST":
                        double nextGrow = 2; int growth = 0;
                        On(CombatHook.Tick, (c, _) => {
                            if (growth >= 5 || c.Time < nextGrow) return;
                            growth++; nextGrow += 2;
                            foreach (var u in members.Where(u => u.Alive)) { u.Armor += value; u.MagicResist += value; u.AttackDamage += P("grow"); u.AbilityPower += P("grow"); }
                        }); break;
                    case "PILGRIM":
                        On(CombatHook.Attack, (_, e) => { var u = e.Unit!; if (Member(u) && u.MaxMana > 0) u.Mana = Math.Min(u.MaxMana, u.Mana + 10 * value); });
                        On(CombatHook.DamageTaken, (_, e) => { var u = e.Unit!; if (Member(u) && !e.IsBurn && u.MaxMana > 0) u.Mana = Math.Min(u.MaxMana, u.Mana + Math.Min(15, e.Amount * 0.06) * value); }); break;
                    case "RONIN": On(CombatHook.CombatReady, (c, _) => { foreach (var u in members) {
                        if (c.Adjacent(u).Any(a => a.Team == u.Team)) continue;
                        u.Shield += u.MaxHealth * value; if (level >= 2) u.Omnivamp += P("vamp");
                    } }); break;
                    case "SPIRIT":
                        var rushed = new HashSet<int>();
                        On(CombatHook.Cast, (c, e) => { var u = e.Unit!; if (!Member(u) || !rushed.Add(u.Id)) return;
                            foreach (var a in c.Mine.Where(a => a.Alive)) c.Buff(a, CombatStat.AttackSpeed, value * u.MaxMana / 100);
                        }); break;
                    case "MAGE":
                        var recast = new HashSet<int>();
                        On(CombatHook.CombatReady, (_, _) => { foreach (var u in members) u.AbilityPower *= value; }, 1);
                        On(CombatHook.Cast, (_, e) => { var u = e.Unit!; if (!Member(u)) return; if (recast.Add(u.Id)) u.Mana = u.MaxMana; else recast.Remove(u.Id); }); break;
                    case "SHADE": On(CombatHook.Attack, (c, e) => { var u = e.Unit!; if (!Member(u) || u.Attacks % 3 != 0 || !e.Target!.Alive) return;
                        c.Damage(u, e.Target, value); u.InvulnerableUntil = Math.Max(u.InvulnerableUntil, c.Time + 0.5);
                    }); break;
                    case "CHARMER": On(CombatHook.DamageDealt, (c, e) => { var target = e.Target!;
                        if (e.Kind != DamageKind.Magic || e.IsBurn || !target.Alive || !Member(e.Unit!) || target.CharmUntil > c.Time) return;
                        target.CharmUntil = c.Time + 8; c.Buff(target, CombatStat.AttackDamage, -target.BaseAttackDamage * value, 8);
                    }); break;
                    case "HUNTER":
                        double nextHunt = P("every");
                        On(CombatHook.Tick, (c, _) => { if (c.Time < nextHunt) return; nextHunt += P("every");
                            foreach (var u in members.Where(u => u.Alive)) {
                                var target = combat.Units.Where(e => e.Alive && e.Team != u.Team).OrderBy(e => e.Health / e.MaxHealth).FirstOrDefault();
                                if (target is null) return; c.Damage(u, target, u.AttackDamage * value, DamageKind.Physical);
                            }
                        }); break;
                    case "KEEPER": On(CombatHook.CombatStart, (c, _) => {
                        void Shield(CombatUnit u, double amount) { c.Shield(u, amount); c.After(P("dur"), () => u.Shield -= Math.Min(u.Shield, amount)); }
                        foreach (var u in members) { Shield(u, value * 1.5); foreach (var a in c.Adjacent(u).Where(a => a.Team == u.Team)) Shield(a, value); }
                    }, 2); break;
                    case "STYLE":
                        var attacks = new Dictionary<int, int>(); var doubles = new Dictionary<int, int>();
                        On(CombatHook.Attack, (c, e) => { var u = e.Unit!; if (!Member(u)) return;
                            int n = attacks.GetValueOrDefault(u.Id) + 1; attacks[u.Id] = n; if (n % 2 != 0) return;
                            c.Shield(u, u.MaxHealth * value); if (e.Target!.Alive) c.Damage(u, e.Target, e.Amount, DamageKind.Physical, e.Critical);
                            int s = doubles.GetValueOrDefault(u.Id) + 1; if (s >= 2) { s = 0; u.Mana = u.MaxMana; } doubles[u.Id] = s;
                        }); break;
                    case "SHARP":
                        CombatUnit[] Bounces(CombatUnit u, CombatUnit target) => combat.Units.Where(e => e.Alive && e.Team != u.Team && e != target && e.Cell.DistanceTo(target.Cell) <= 2)
                            .OrderBy(e => e.Cell.DistanceTo(target.Cell)).ThenBy(e => e.Id).Take((int)value).ToArray();
                        On(CombatHook.Attack, (c, e) => { if (Member(e.Unit!)) foreach (var target in Bounces(e.Unit!, e.Target!)) c.Damage(e.Unit!, target, e.Amount * P("frac"), DamageKind.Physical); });
                        On(CombatHook.Cast, (c, e) => { var u = e.Unit!;
                            if (!Member(u) || e.Target is null || new[] { "heal", "shield", "rally", "grow", "rewind", "frenzy" }.Contains(u.Line.Ability.Kind)) return;
                            foreach (var target in Bounces(u, e.Target)) c.AbilityHit(u, target, e.Value * P("frac") * 0.7);
                        }); break;
                    case "CELES":
                        var ascending = members.ToDictionary(u => u.Id, _ => new Ascension());
                        void Ascend(CombatUnit u, double time) { var s = ascending[u.Id]; s.Used = true; s.Until = time + value; u.Stun = 0; }
                        On(CombatHook.Attack, (c, e) => { var u = e.Unit!; if (ascending.TryGetValue(u.Id, out var s) && !s.Used && Member(u) && ++s.Attacks >= 6) Ascend(u, c.Time); });
                        On(CombatHook.DamageTaken, (c, e) => { var u = e.Unit!; if (ascending.TryGetValue(u.Id, out var s) && !s.Used && Member(u) && u.Health < u.MaxHealth * 0.5) Ascend(u, c.Time); });
                        On(CombatHook.BeforeDamage, (c, e) => { if (ascending.TryGetValue(e.Unit!.Id, out var s) && s.Until > c.Time && Member(e.Unit)) e.Multiplier *= 1 + P("amp"); });
                        On(CombatHook.BeforeDamageTaken, (c, e) => { if (ascending.TryGetValue(e.Unit!.Id, out var s) && s.Until > c.Time && Member(e.Unit)) e.Reduction += P("amp"); }); break;
                    case "FALLEN":
                        On(CombatHook.BeforeDamage, (c, e) => { if (side.Fates?.Fallen == "dark" && c.Time < 10 && Member(e.Unit!)) e.Multiplier *= 1.4; });
                        On(CombatHook.DamageDealt, (c, e) => { if (side.Fates?.Fallen == "holy" && Member(e.Unit!) && e.Unit!.Alive) c.Heal(e.Unit, e.Amount * 0.4, true); }); break;
                    case "BANCHO":
                        var pushups = members.ToDictionary(u => u.Id, _ => new Pushups());
                        On(CombatHook.DamageTaken, (c, e) => { var u = e.Unit!;
                            if (!pushups.TryGetValue(u.Id, out var s) || s.Used || !Member(u) || u.Health >= u.MaxHealth * 0.4) return;
                            s.Used = true; u.InvulnerableUntil = c.Time + 3; u.Stun = Math.Max(u.Stun, 3);
                            for (int k = 1; k <= 6; k++) { int step = k; c.After(0.5 * k, () => {
                                if (s.Done) return; c.Heal(u, u.MaxHealth * 0.08); c.Buff(u, CombatStat.AttackSpeed, 0.08);
                                if (u.Health >= u.MaxHealth - 1 || step == 6) { s.Done = s.Pumped = true; u.Stun = 0; u.InvulnerableUntil = Math.Min(u.InvulnerableUntil, c.Time); }
                            }, u); }
                        });
                        On(CombatHook.BeforeDamage, (_, e) => { if (pushups.TryGetValue(e.Unit!.Id, out var s) && s.Pumped && Member(e.Unit)) e.Multiplier *= 1.2; }); break;
                    case "SOVEREIGN":
                        var guards = new Dictionary<int, List<CombatUnit>>();
                        On(CombatHook.CombatReady, (c, _) => { foreach (var u in members.Where(u => !u.Summoned)) {
                            var foe = combat.Units.Where(e => e.Alive && e.Team != u.Team).OrderBy(e => e.Cell.DistanceTo(u.Cell) * 10000 + e.Health).FirstOrDefault();
                            guards[u.Id] = [];
                            for (int k = 0; k < 2; k++) {
                                if (c.FreeCellNear(u, foe?.Cell) is not { } cell) break;
                                var g = c.Summon(u.Team, "fat_statue", u.Stage, cell); if (g is null) break;
                                g.MaxHealth = g.Health = UnitValues.Round(u.MaxHealth * value); g.Armor = u.Armor; g.MagicResist = u.MagicResist; guards[u.Id].Add(g);
                            }
                        } }, 2);
                        On(CombatHook.Death, (c, e) => { if (!guards.Remove(e.Unit!.Id, out var list)) return;
                            foreach (var g in list.Where(g => g.Alive)) { g.InvulnerableUntil = 0; c.Damage(e.Unit, g, 1e9, DamageKind.True); }
                        }); break;
                    case "OCEAN":
                        double total = 0; int stars = 0; bool risen = false;
                        On(CombatHook.CombatReady, (c, _) => { total = c.Mine.Where(u => !u.Summoned).Sum(u => u.MaxHealth); stars = members.Sum(u => u.Stage); });
                        On(CombatHook.Tick, (c, _) => {
                            if (risen || total == 0 || c.Mine.Where(u => !u.Summoned).Sum(u => u.Alive ? Math.Max(0, u.Health) : 0) > total * 0.5) return;
                            risen = true; var foes = combat.Units.Where(u => u.Alive && u.Team != c.Team).ToArray(); if (foes.Length == 0) return;
                            var best = foes.OrderByDescending(u => foes.Count(e => e.Cell.DistanceTo(u.Cell) <= 1)).First();
                            HexCell? cell = c.FreeCellNear(best);
                            foreach (var foe in foes) { if (cell is not null) break; cell = c.FreeCellNear(foe); }
                            foreach (var u in c.Mine) { if (cell is not null) break; if (u.Alive) cell = c.FreeCellNear(u); }
                            if (cell is null) return; double boost = 1 + 0.08 * stars;
                            var shadow = c.Summon(c.Team!.Value, "fat_shadow", (int)value, cell.Value, new(HealthMultiplier: P("hpMul") * boost));
                            if (shadow is null) return; shadow.AttackDamage *= boost; shadow.BaseAttackDamage *= boost;
                            double slam = (level == 1 ? 90 : level == 2 ? 180 : 360) * boost;
                            foreach (var enemy in combat.Units.Where(u => u.Alive && u.Team != c.Team && u.Cell.DistanceTo(shadow.Cell) <= 1).ToArray()) { c.Stun(enemy, 1.5); c.Damage(shadow, enemy, slam); }
                        }, 3); break;
                    default: throw new NotSupportedException($"Fates trait {id} has not been migrated.");
                }
            }
            int active = counts.Count(pair => rules.Traits.Any(t => t.Id == pair.Key && TraitRules.Level(t, pair.Value) > 0));
            if (side.Parts is { } partners)
                foreach (var partner in partners) FatesAugments.Bind(combat, team, partner.Augments ?? [], active, partner.Owner, partner.PartnerTop);
            else FatesAugments.Bind(combat, team, side.Augments ?? [], active, partnerTop: side.PartnerTop);
        }
    }
}

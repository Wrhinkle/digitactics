namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private void BindArcaneTraits(CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
    {
        for (int team = 0; team < 2; team++)
        {
            var side = team == 0 ? setup.SideA : setup.SideB;
            if (side.NoTraits) continue;
            var entries = team == 0 ? sideA : sideB;
            var counts = side.Counts ?? (side.Parts is { } parts ? TraitRules.TeamCount(rules, entries, parts) : TraitRules.Count(rules, entries, side.Augments));
            foreach (var (id, count) in counts)
            {
                var trait = rules.Traits.FirstOrDefault(t => t.Id == id); if (trait is null) continue;
                int level = TraitRules.Level(trait, count); if (level == 0) continue;
                double value = trait.Values[level - 1]; double P(string key) => trait.Parameters[key][level - 1];
                var context = Hooks.Context(CombatHookSource.Trait, id, team);
                var members = units.Where(u => u.Team == team && u.Traits.Contains(id)).ToArray();
                bool Member(CombatUnit u) => u.Traits.Contains(id);
                void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action) => Hooks.Add(context, hook, action);
                void Start(Action<CombatHookContext> action) => On(CombatHook.CombatStart, (c, _) => action(c));
                void Each(Action<CombatUnit> action) => Start(_ => { foreach (var u in members) action(u); });
                switch (id)
                {
                    case "ACA": Start(c => {
                        foreach (var u in c.Mine) {
                            int n = u.Items.Count(item => (setup.ArcSponsor ?? []).Contains(item));
                            double f = value * n * (Member(u) ? 2 : 1) + (Member(u) ? P("base") : 0);
                            if (f > 0) { u.MaxHealth = UnitValues.Round(u.MaxHealth * (1 + f)); u.Equipment.Amp += f; }
                        }
                    }); break;
                    case "AUT":
                        Each(u => { u.Armor += P("arm"); u.MagicResist += P("arm"); u.Arcane.Automaton = value; });
                        On(CombatHook.DamageDealt, (c, e) => {
                            var s = e.Unit!.Arcane; if (s.Automaton == 0 || s.AutomatonBusy || e.IsBurn || !(e.Amount > 0)) return;
                            s.AutomatonHits++; s.AutomatonDamage += e.Amount;
                            if (s.AutomatonHits < 12 || !e.Target!.Alive) return;
                            s.AutomatonHits = 0; double damage = s.Automaton + s.AutomatonDamage * 0.25; s.AutomatonDamage = 0;
                            s.AutomatonBusy = true; c.Effect("beam", e.Unit, e.Target); c.Text(e.Unit, "Overload"); c.Damage(e.Unit, e.Target, damage); s.AutomatonBusy = false;
                        }); break;
                    case "BR":
                        CombatUnit? bel = null; int casts = 0;
                        void Free(CombatHookContext c) {
                            if (bel is null || !bel.Arcane.Chained || !bel.Alive) return;
                            bel.Arcane.Chained = false; bel.Stun = 0; c.Text(bel, "Unleashed");
                            if (level >= 3) { c.Heal(bel, bel.MaxHealth); c.Burst(bel.Cell, 2, bel.Team); foreach (var e in EnemiesNear(bel, bel.Cell, 2)) c.Damage(bel, e, bel.AttackDamage * 2.5); }
                        }
                        Start(c => {
                            double mul = (0.22 + 0.045 * members.Where(u => !u.Summoned).Sum(u => u.Stage + 1)) * value;
                            foreach (int row in c.Team == 0 ? new[] { 4, 5 } : new[] { 3, 2 }) {
                                foreach (int col in new[] { 3, 2, 4, 1, 5, 0, 6 }) if (UnitAt(new(col, row)) is null) {
                                    bel = c.Summon(c.Team!.Value, "arc_s_belphemon", 0, new(col, row), new(HealthMultiplier: mul, AttackMultiplier: mul, NoHooks: true)); break;
                                }
                                if (bel is not null) break;
                            }
                            if (bel is not null) { bel.Stun = 999; bel.Arcane.Chained = true; c.Text(bel, "Chained"); }
                        });
                        On(CombatHook.Cast, (c, e) => { if (bel is not null && bel.Arcane.Chained && Member(e.Unit!) && ++casts >= 5) Free(c); });
                        On(CombatHook.DamageTaken, (c, e) => { if (bel is not null && e.Unit == bel && bel.Arcane.Chained && bel.Health < bel.MaxHealth * 0.65) Free(c); });
                        On(CombatHook.BeforeDamage, (c, e) => { if (level >= 2 && Member(e.Unit!) && e.Target!.Team != c.Team) e.Multiplier *= 1.15; }); break;
                    case "SYN": Each(u => {
                        u.Omnivamp += value; u.MaxHealth = UnitValues.Round(u.MaxHealth * (1 + P("hp")));
                        u.MaxHealth += side.Arcane?.ArcSynHp ?? 0; if (side.Arcane?.ArcSynAug == true) u.MaxHealth += 150;
                    }); break;
                    case "CON":
                        Each(u => { double f = value + 0.06 * Math.Min(10, side.Arcane?.ArcChests ?? 0); u.AttackDamage += u.BaseAttackDamage * f; u.AbilityPower += UnitValues.Round(100 * f); });
                        On(CombatHook.Kill, (_, e) => { if (Member(e.Unit!)) e.Unit!.Arcane.ConquestKills++; }); break;
                    case "EMI":
                        bool nami = false;
                        Start(c => {
                            var envoys = members.Where(u => u.Line.Envoy is not null).ToArray();
                            foreach (var src in count >= 4 ? envoys : envoys.Take(1)) switch (src.Line.Envoy) {
                                case "tristana": foreach (var u in c.Mine) u.AttackSpeed *= 1 + 0.05 * (src.Stage + 1); break;
                                case "nami": nami = true; break;
                                case "ambessa": foreach (var u in c.Mine) { u.Armor += 10; u.MagicResist += 10; } break;
                                case "garen": foreach (var u in c.Mine) if (u.Cell.Row == src.Cell.Row && Math.Abs(u.Cell.Column - src.Cell.Column) <= 1) u.MaxHealth += UnitValues.Round(src.MaxHealth * 0.12); break;
                            }
                            if (count >= 4) foreach (var u in envoys) { u.MaxHealth += 250; u.Equipment.Amp += 0.2; }
                        });
                        On(CombatHook.Attack, (_, e) => { if (nami) e.Unit!.Mana = Math.Min(e.Unit.MaxMana, e.Unit.Mana + 3); }); break;
                    case "ENF":
                        Start(c => {
                            foreach (var u in members) { u.Shield += u.MaxHealth * P("sh"); u.Equipment.Amp += value; }
                            foreach (var e in units.Where(e => e.Team != c.Team && e.Alive).OrderByDescending(e => e.MaxHealth).ThenBy(e => e.Id).Take((int)P("n"))) { e.Arcane.Wanted |= 1 << c.Team!.Value; c.Text(e, "Wanted"); }
                        });
                        On(CombatHook.AnyDeath, (c, e) => {
                            if (e.Unit!.Team == c.Team || (e.Unit.Arcane.Wanted & (1 << c.Team!.Value)) == 0) return;
                            foreach (var u in members.Where(u => u.Alive)) { u.AttackSpeed *= 1.3; c.Text(u, "Bounty"); }
                        }); break;
                    case "EXP":
                        Start(_ => {
                            var lab = members.Where(u => !u.Summoned).OrderByDescending(u => u.Line.Tier).ThenByDescending(u => u.Stage).ThenBy(u => u.Id).Take((int)P("labs")).ToArray();
                            var bonuses = lab.Where(u => u.Line.Experiment is not null).Select(u => u.Line.Experiment!).ToHashSet();
                            foreach (var u in lab) { u.MaxHealth += value + (bonuses.Contains("mundo") ? 200 : 0); u.Arcane.Experiments = bonuses; }
                        });
                        On(CombatHook.DamageDealt, (c, e) => {
                            var u = e.Unit!; var s = u.Arcane; var b = s.Experiments; var t = e.Target!;
                            if (b is null || s.ExperimentBusy || e.IsBurn || !t.Alive) return;
                            s.ExperimentBusy = true;
                            if (b.Contains("zyra") && e.Kind == DamageKind.Magic) c.Damage(u, t, e.Amount * 0.2, DamageKind.True);
                            if (b.Contains("nunu") && t.Alive && s.NunuCooldown <= Time) { s.NunuCooldown = Time + 1; c.Damage(u, t, t.MaxHealth * 0.03); }
                            s.ExperimentBusy = false;
                        });
                        On(CombatHook.Cast, (c, e) => { var u = e.Unit!; if (u.Arcane.Experiments?.Contains("urgot") == true) { c.Shield(u, u.MaxHealth * 0.15); c.Buff(u, CombatStat.AttackSpeed, 0.3, 5); } });
                        On(CombatHook.Attack, (c, e) => {
                            var u = e.Unit!; if (u.Arcane.Experiments?.Contains("twitch") != true || u.Attacks % 5 != 0) return;
                            var t = TargetByDistance(u); if (t is not null) { u.Arcane.ExperimentBusy = true; c.Damage(u, t, t.MaxHealth * 0.06, DamageKind.Physical); u.Arcane.ExperimentBusy = false; }
                        });
                        On(CombatHook.Kill, (c, e) => { var u = e.Unit!; if (u.Arcane.Experiments?.Contains("mundo") == true && u.Alive) { u.MaxHealth += 60; c.Heal(u, 60); } }); break;
                    case "FAM": Each(u => { u.MaxMana = Math.Max(10, UnitValues.Round(u.MaxMana * (1 - value))); u.Equipment.Reduction += 0.12; if (level >= 2) u.AttackSpeed *= 1.2; }); break;
                    case "FIRE":
                        Each(u => { u.Arcane.Fire = value; u.Arcane.FireLevel = level; });
                        On(CombatHook.UnitTick, (c, e) => { var u = e.Unit!; var s = u.Arcane; if (s.Fire == 0 || !c.Every(u, "arcFire", 6)) return;
                            double healing = (u.Taken - s.FireLast) * s.Fire; s.FireLast = u.Taken; if (healing > 0) c.Heal(u, healing);
                            if (s.FireLevel >= 2) c.Buff(u, CombatStat.AttackSpeed, 0.6, 2); c.Text(u, "Surge");
                        }); break;
                    case "MODE": Each(u => { if (u.Entry.Row <= 5) u.Equipment.Reduction += P("dr"); else u.Equipment.Amp += value; }); break;
                    case "REB":
                        double total = 0; bool fired = false;
                        Each(u => u.MaxHealth = UnitValues.Round(u.MaxHealth * (1 + P("hp"))));
                        On(CombatHook.CombatReady, (c, _) => total = c.Mine.Sum(u => u.MaxHealth));
                        On(CombatHook.Tick, (c, _) => {
                            if (fired || total == 0 || units.Where(u => u.Alive && u.Team == c.Team).Sum(u => u.Health) > total * 0.75) return;
                            fired = true;
                            foreach (var u in members.Where(u => u.Alive)) { c.Buff(u, CombatStat.AttackSpeed, 0.6, 5); u.AttackDamage += u.BaseAttackDamage * value; u.AbilityPower += UnitValues.Round(100 * value); c.Text(u, "Flare"); }
                            if (level >= 3) foreach (var e in units.Where(e => e.Alive && e.Team != c.Team)) c.Stun(e, 2);
                        }); break;
                    case "SCR": Start(c => {
                        int components = 0;
                        foreach (var u in c.Mine) foreach (var item in u.Items) if (items.TryGetValue(item, out var def)) {
                            if (def.Component) { if (!def.Special) components++; }
                            else if (def.Ingredients is { } recipe && recipe.All(x => items.TryGetValue(x, out var component) && component.Component)) components += 2;
                        }
                        foreach (var u in members) {
                            u.Shield += P("base") + components * value; double left = P("n");
                            foreach (var item in u.Items.ToArray()) {
                                if (left <= 0) break;
                                if (!items.TryGetValue(item, out var def) || !def.Component || def.Special) continue;
                                var full = def.SelfUpgrade is { } upgrade ? items.GetValueOrDefault(upgrade) : null;
                                if (full is not null) { ApplyTemporaryItemUpgrade(u, full, def, preserveExistingBlueBuff: true); left--; }
                            }
                        }
                    }); break;
                    case "HR": On(CombatHook.Cast, (c, e) => {
                        var u = e.Unit!; if (!Member(u)) return;
                        c.Buff(u, CombatStat.DamageReduction, 0.5, 1.5); double roll = c.Random(), k = 1 + u.Stage;
                        if (roll < 0.3) { for (int i = 0; i < 8; i++) { var t = RandomEnemyTarget(u); if (t is null) break; c.Effect("shot", u, t); c.Damage(u, t, 35 * k, DamageKind.Physical); } c.Text(u, "Rockets"); }
                        else if (roll < 0.6) { c.Shield(u, 180 * k); c.Text(u, "Shield"); }
                        else if (roll < 0.85) { u.Arcane.Coins = Math.Min(3, u.Arcane.Coins + 1); c.Text(u, "Coin"); }
                        else { if (e.Target is { Alive: true }) c.AbilityHit(u, e.Target, e.Value); c.Text(u, "Jackpot"); }
                    }); break;
                    case "JUNK":
                        Start(_ => {
                            var ups = side.Arcane?.ArcJunk ?? []; if (ups.Length == 0) return;
                            var u = members.Where(u => u.Line.Id == "arc_chaosdromon").OrderByDescending(u => u.Stage).ThenBy(u => u.Id).FirstOrDefault(); if (u is null) return;
                            var j = ups.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
                            u.AbilityPower += 8 * j.GetValueOrDefault("spare"); u.MaxHealth += 100 * j.GetValueOrDefault("spare");
                            if (j.ContainsKey("destruct")) { u.Armor *= 1.25; u.MagicResist *= 1.25; }
                            if (new[] { "flame", "bots", "rod", "destruct", "buster" }.All(j.ContainsKey)) { u.AbilityPower += 25; u.MaxHealth += 300; }
                            u.Arcane.Junk = j;
                        });
                        On(CombatHook.UnitTick, (c, e) => {
                            var u = e.Unit!; var j = u.Arcane.Junk; if (j is null) return; double k = (1 + u.Stage) * (u.AbilityPower / 100);
                            if (j.ContainsKey("flame") && c.Every(u, "jFlame", 1)) foreach (var t in EnemiesNear(u, u.Cell, 2)) c.Damage(u, t, 30 * k);
                            if (j.ContainsKey("bots") && c.Every(u, "jBots", 3)) c.Heal(u, u.MaxHealth * 0.06);
                            if (j.ContainsKey("rod") && c.Every(u, "jRod", 5) && TargetByDistance(u, true) is { } far) { c.Effect("beam", u, far); c.Damage(u, far, 120 * k); }
                            if (j.ContainsKey("buster") && c.Every(u, "jBust", 3)) { var t = units.Where(t => t.Alive && t.Team != u.Team).OrderByDescending(t => t.Armor + t.MagicResist).FirstOrDefault(); if (t is not null) c.Damage(u, t, 80 * k, DamageKind.True); }
                        });
                        On(CombatHook.Death, (c, e) => { var u = e.Unit!; if (u.Arcane.Junk?.ContainsKey("destruct") == true) { c.Burst(u.Cell, 2, u.Team); foreach (var t in EnemiesNear(u, u.Cell, 2)) c.Damage(u, t, (u.Armor + u.MagicResist) * 1.6); } }); break;
                    case "AMB": Each(u => { u.Equipment.AbilityCrit = true; u.CriticalChance += value; u.CriticalDamage += P("cd"); if (level >= 4) u.Equipment.Reduction += 0.15; }); break;
                    case "ART":
                        Each(u => { u.AttackDamage += u.BaseAttackDamage * value; u.Arcane.Artillery = true; });
                        On(CombatHook.Attack, (c, e) => { var u = e.Unit!; if (u.Arcane.Artillery && u.Attacks % 4 == 0) { c.Burst(e.Target!.Cell, 1, u.Team); foreach (var t in EnemiesNear(u, e.Target.Cell, 1)) c.Damage(u, t, u.AttackDamage * 1.4, DamageKind.Physical); } }); break;
                    case "BRU": Start(c => { foreach (var u in c.Mine) u.MaxHealth += 80; foreach (var u in members) u.MaxHealth = UnitValues.Round(u.MaxHealth * (1 + value)); }); break;
                    case "DOM":
                        Each(u => { u.Shield += P("sh"); u.Arcane.Dominator = value; });
                        On(CombatHook.Cast, (_, e) => { var u = e.Unit!; if (u.Arcane.Dominator != 0) u.AbilityPower += UnitValues.Round(u.MaxMana * u.Arcane.Dominator * 0.6); }); break;
                    case "PIT":
                        Each(u => { u.Omnivamp += 0.1; u.Arcane.Pit = value; u.Arcane.PitHeal = P("heal"); });
                        On(CombatHook.DamageDealt, (c, e) => { var s = e.Unit!.Arcane;
                            if (s.Pit == 0 || s.PitBusy || e.IsBurn || e.Kind == DamageKind.True || !e.Target!.Alive) return;
                            s.PitBusy = true; c.Damage(e.Unit, e.Target, e.Amount * s.Pit, DamageKind.True); s.PitBusy = false;
                        });
                        On(CombatHook.DamageTaken, (c, e) => { var u = e.Unit!; var s = u.Arcane;
                            if (s.Pit == 0 || s.PitHealed || u.Health >= u.MaxHealth * 0.5) return;
                            s.PitHealed = true; c.Text(u, "Second Wind"); for (int k = 1; k <= 4; k++) c.After(0.5 * k, () => c.Heal(u, (u.MaxHealth * s.PitHeal) / 4), u);
                        }); break;
                    case "QS":
                        Each(u => { u.AttackSpeed *= 1 + value; u.Arcane.Quick = u.Arcane.QuickLow = value; u.Arcane.QuickHigh = P("hi"); u.Arcane.QuickLevel = level; });
                        On(CombatHook.Attack, (_, e) => { var u = e.Unit!; var s = u.Arcane; if (s.QuickLevel == 0) return;
                            var t = e.Target!; double v = s.QuickLow + (s.QuickHigh - s.QuickLow) * (1 - Math.Max(0, t.Health) / t.MaxHealth);
                            u.AttackSpeed = (u.AttackSpeed / (1 + s.Quick)) * (1 + v); s.Quick = v;
                        });
                        On(CombatHook.Kill, (c, e) => { if (e.Unit!.Arcane.QuickLevel >= 3 && e.Unit.Alive) c.Shield(e.Unit, 200); }); break;
                    case "SEN": Start(c => { foreach (var u in c.Mine) { double bonus = value * (Member(u) ? 3 : 1); u.Armor += bonus; u.MagicResist += bonus; } }); break;
                    case "SNI": Each(u => { u.Equipment.Amp += value; u.PerHexAmplification += P("hex"); }); break;
                    case "SOR":
                        Start(c => { foreach (var u in c.Mine) u.AbilityPower += 8 + (Member(u) ? value : 0); });
                        On(CombatHook.Cast, (c, e) => { if (level >= 3 && Member(e.Unit!) && e.Target is { Alive: true }) c.Weaken(e.Target, 0.2, 3); }); break;
                    case "VIS":
                        Each(u => { u.Arcane.Vision = value; u.Arcane.VisionLevel = level; });
                        On(CombatHook.Attack, (_, e) => { var u = e.Unit!; if (u.Arcane.VisionLevel > 0) u.Mana = Math.Min(u.MaxMana, u.Mana + 10 * u.Arcane.Vision); });
                        On(CombatHook.DamageTaken, (_, e) => { var u = e.Unit!; if (u.Arcane.VisionLevel > 0 && !e.IsBurn) u.Mana = Math.Min(u.MaxMana, u.Mana + Math.Min(15, e.Amount * 0.06) * u.Arcane.Vision); });
                        On(CombatHook.DamageDealt, (c, e) => { var u = e.Unit!; if (u.Arcane.VisionLevel < 3 || e.Kind != DamageKind.Magic || e.IsBurn) return;
                            var best = units.Where(a => a.Alive && a.Team == u.Team).OrderBy(a => a.Health / a.MaxHealth).FirstOrDefault(); if (best is not null) c.Heal(best, e.Amount * 0.16, true);
                        }); break;
                    case "WAT":
                        Each(u => { u.Arcane.Watcher = value; u.Arcane.WatcherHigh = P("hi"); });
                        On(CombatHook.BeforeDamageTaken, (_, e) => { var u = e.Unit!; e.Reduction += u.Health > u.MaxHealth * 0.5 ? u.Arcane.WatcherHigh : u.Arcane.Watcher; }); break;
                    default: throw new NotSupportedException($"Arcane trait {id} has not been migrated.");
                }
            }
        }
    }
}

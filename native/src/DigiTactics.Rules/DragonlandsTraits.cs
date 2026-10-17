namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private IEnumerable<HexCell> DragonNeighbors(HexCell at) => Enumerable.Range(0, grid.Count).Select(i => grid[i]).Where(cell => cell.DistanceTo(at) == 1);
    private void BindDragonlandsTraits(CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
    {
        for (int team = 0; team < 2; team++)
        {
            var side = team == 0 ? setup.SideA : setup.SideB;
            if (side.NoTraits) continue;
            var entries = team == 0 ? sideA : sideB;
            var counts = side.Counts ?? (side.Parts is { } parts ? TraitRules.TeamCount(rules, entries, parts) : TraitRules.Count(rules, entries, side.Augments));
            foreach (var (id, count) in counts)
            {
                var trait = rules.Traits.FirstOrDefault(t => t.Id == id);
                if (trait is null) continue;
                int level = TraitRules.Level(trait, count); if (level == 0) continue;
                double value = trait.Values[level - 1]; double P(string key) => trait.Parameters[key][level - 1];
                var context = Hooks.Context(CombatHookSource.Trait, id, team);
                var members = units.Where(u => u.Team == team && u.Traits.Contains(id)).ToArray();
                bool Member(CombatUnit u) => u.Traits.Contains(id);
                IEnumerable<CombatUnit> Real(CombatHookContext c) => c.Mine.Where(u => !u.Summoned);
                void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action) => Hooks.Add(context, hook, action);
                void Start(Action<CombatHookContext> action) => On(CombatHook.CombatStart, (c, _) => action(c));
                void Each(Action<CombatUnit> action) => Start(_ => { foreach (var u in members) action(u); });
                switch (id)
                {
                    case "STAR": Start(c => { foreach (var u in Real(c)) u.AbilityPower += value; }); break;
                    case "XAB": Start(c => {
                        var present = members.Select(u => u.Line.Id).ToHashSet();
                        double hp = present.Contains("drg_elecmon") ? 100 : 0, speed = present.Contains("drg_mushroomon") ? 0.08 : 0,
                            armor = present.Contains("drg_dorumon") ? 8 : 0, ad = present.Contains("drg_hackmon") ? 0.08 : 0,
                            mana = present.Contains("drg_baohuckmon") ? 2 : 0, vamp = 0;
                        foreach (var u in members) if (!u.Line.Traits.Contains(id)) vamp += 0.03;
                        foreach (var u in Real(c)) {
                            double k = value * (Member(u) ? 2 : 1); u.MaxHealth += UnitValues.Round(hp * k); u.AttackSpeed *= 1 + speed * k;
                            u.Armor += armor * k; u.MagicResist += armor * k; u.AttackDamage += u.BaseAttackDamage * ad * k; u.Omnivamp += vamp * k;
                            if (mana != 0) u.Dragonlands.XabMana = mana * k;
                        }
                    }); On(CombatHook.Attack, (_, e) => { var u = e.Unit!; if (u.Dragonlands.XabMana != 0) u.Mana = Math.Min(u.MaxMana, u.Mana + u.Dragonlands.XabMana); }); break;
                    case "STONE":
                        bool cleared = false;
                        Start(c => {
                            var allies = Real(c).ToArray(); if (allies.Length == 0) return;
                            double hp = 150 + 120 * (setup.RoundStage == 0 ? 3 : setup.RoundStage); var statues = new List<CombatUnit>();
                            for (int k = 0; k < value; k++) {
                                HexCell? best = null; int bestScore = 0;
                                foreach (var a in allies) foreach (var cell in DragonNeighbors(a.Cell)) {
                                    if ((c.Team == 0 ? cell.Row < 4 : cell.Row > 3) || UnitAt(cell) is not null) continue;
                                    int score = DragonNeighbors(cell).Count(n => UnitAt(n) is { } x && x.Team == c.Team && !x.Summoned);
                                    if (score > bestScore) { bestScore = score; best = cell; }
                                }
                                if (best is null) break;
                                var statue = c.Summon(c.Team!.Value, "drg_stone", 0, best.Value, new(NoHooks: true)); if (statue is null) break;
                                statue.MaxHealth = statue.Health = hp; statue.AttackDamage = statue.BaseAttackDamage = 0; statue.Armor = statue.MagicResist = 30;
                                statue.MoveCooldown = statue.AttackCooldown = double.PositiveInfinity; statue.MaxMana = statue.Mana = 0; statue.Dragonlands.Statue = true; statues.Add(statue);
                            }
                            foreach (var a in allies) if (statues.Any(s => s.Cell.DistanceTo(a.Cell) <= 1)) { a.Dragonlands.Jade = P("heal"); a.AttackSpeed *= 1 + P("as"); }
                        });
                        On(CombatHook.UnitTick, (c, e) => { var u = e.Unit!; if (u.Dragonlands.Jade != 0 && c.Every(u, "jade", 2)) c.Heal(u, u.MaxHealth * u.Dragonlands.Jade); });
                        On(CombatHook.Death, (c, e) => { var u = e.Unit!; if (u.Dragonlands.Statue) { c.Burst(u.Cell, 1, u.Team); foreach (var enemy in EnemiesNear(u, u.Cell, 1)) c.Damage(u, enemy, u.MaxHealth * 0.33); } });
                        On(CombatHook.Tick, (c, _) => {
                            if (cleared) return; var alive = c.Mine.Where(u => u.Alive).ToArray();
                            if (alive.Length == 0 || !alive.All(u => u.Dragonlands.Statue)) return;
                            cleared = true; foreach (var statue in alive) c.Damage(statue, statue, statue.Health + statue.Shield + 1, DamageKind.True);
                        }); break;
                    case "MIRAGE": Start(c => {
                        string mode = setup.Mirage ?? new[] { "dancer", "warlord", "overload", "executioner" }[(int)(c.Random() * 4)];
                        foreach (var u in members) switch (mode) {
                            case "dancer": u.Dodge += P("dodge"); u.AttackSpeed *= 1 + P("das"); break;
                            case "warlord": u.AttackDamage += u.BaseAttackDamage * P("war"); u.MaxHealth = UnitValues.Round(u.MaxHealth * (1 + P("war"))); break;
                            case "overload": u.Dragonlands.MirageZap = P("zap"); break;
                            default: u.CriticalChance += P("crit"); u.Equipment.AbilityCrit = true; u.Dragonlands.MirageExecute = true; break;
                        }
                    });
                        On(CombatHook.UnitTick, (c, e) => { var u = e.Unit!; if (u.Dragonlands.MirageZap != 0 && c.Every(u, "mirZap", 2)) { var near = EnemiesNear(u, u.Cell, 1); if (near.Length > 0) c.Burst(u.Cell, 1, u.Team); foreach (var enemy in near) c.Damage(u, enemy, u.Dragonlands.MirageZap); } });
                        On(CombatHook.DamageDealt, (c, e) => { var t = e.Target!; if (e.Unit!.Dragonlands.MirageExecute && t.Alive && !e.IsBurn && t.Health < t.MaxHealth * 0.1) { c.Text(t, "Finished"); c.Damage(e.Unit, t, t.Health + t.Shield + 1, DamageKind.True); } }); break;
                    case "RAGE": Each(u => u.Dragonlands.Rage = true);
                        On(CombatHook.Attack, (_, e) => { var u = e.Unit!; if (u.Dragonlands.Rage && !(u.Dragonlands.RagingUntil > Time)) u.Mana = Math.Min(u.MaxMana, u.Mana + 3); });
                        On(CombatHook.Cast, (c, e) => { var u = e.Unit!; if (!u.Dragonlands.Rage) return; u.Dragonlands.RagingUntil = Time + 4; c.Buff(u, CombatStat.AttackSpeed, 0.25 + value, 4); c.Buff(u, CombatStat.Omnivamp, P("vamp"), 4); c.Text(u, "Rage"); }); break;
                    case "SLAYER": bool noDragon = false; Start(c => { noDragon = !Real(c).Any(u => u.Line.Slots == 2); foreach (var u in members) u.Dragonlands.Slayer = true; });
                        On(CombatHook.BeforeDamageTaken, (_, e) => { if (e.Unit!.Dragonlands.Slayer && e.Source is { MaxHealth: >= 1800 }) e.Reduction += 0.2; });
                        On(CombatHook.BeforeDamage, (_, e) => { if (noDragon && e.Unit!.Dragonlands.Slayer && !e.IsBurn) e.Multiplier *= 1 + value; }); break;
                    case "GOLD": Each(u => { u.Armor += P("armor"); u.MagicResist += P("armor"); }); break;
                    case "THUNDER": bool struck = false;
                        On(CombatHook.Tick, (c, _) => {
                            if (struck || Time < 8) return; struck = true; var source = Real(c).FirstOrDefault(u => u.Alive); if (source is null) return;
                            foreach (var enemy in units.Where(u => u.Alive && u.Team != source.Team).ToArray()) { c.Effect("beam", source, enemy); c.Stun(enemy, 1); c.Damage(source, enemy, enemy.MaxHealth * value, DamageKind.True); }
                            foreach (var u in members.Where(u => u.Alive)) c.Buff(u, CombatStat.AttackSpeed, P("as"));
                        }); break;
                    case "WHISPER": Each(u => u.Dragonlands.Whisper = value);
                        On(CombatHook.DamageDealt, (c, e) => {
                            var u = e.Unit!; var t = e.Target!; if (u.Dragonlands.Whisper == 0 || e.IsBurn || !t.Alive) return;
                            if (!(t.Dragonlands.ShrunkUntil > Time)) { t.Dragonlands.ShrunkUntil = Time + 6; if (t.Armor > 0) c.Buff(t, CombatStat.Armor, -t.Armor * 0.4, 6); if (t.MagicResist > 0) c.Buff(t, CombatStat.MagicResist, -t.MagicResist * 0.4, 6); c.Text(t, "Shrunk"); }
                            else if (u.Dragonlands.WhisperStacks < 25) { u.Dragonlands.WhisperStacks++; u.AttackDamage += u.BaseAttackDamage * u.Dragonlands.Whisper; u.AbilityPower += UnitValues.Round(u.Dragonlands.Whisper * 100); }
                        }); break;
                    case "LAGOON": Each(u => { u.AbilityPower += value; u.AttackSpeed *= 1 + P("as"); u.Dragonlands.Lagoon = true; });
                        On(CombatHook.Cast, (_, e) => { if (e.Unit!.Dragonlands.Lagoon) dragonLagoon[e.Unit.Team]++; }); break;
                    case "ASSN": Each(u => { u.CriticalChance += value; u.CriticalDamage += P("cd"); u.Equipment.Amp += P("amp"); u.Equipment.AbilityCrit = true; }); break;
                    case "BRUISER": Start(c => { foreach (var u in Real(c)) u.MaxHealth += value * (Member(u) ? 2 : 1); }); break;
                    case "CANNON": Each(u => { u.Dragonlands.Cannon = value; u.Dragonlands.CannonFlat = P("flat"); });
                        On(CombatHook.Attack, (c, e) => { var u = e.Unit!; if (u.Dragonlands.Cannon == 0 || ++u.Dragonlands.CannonAttacks % 4 != 0) return; c.Burst(e.Target!.Cell, 1, u.Team); foreach (var enemy in EnemiesNear(u, e.Target!.Cell, 1)) c.Damage(u, enemy, u.AttackDamage * u.Dragonlands.Cannon + u.Dragonlands.CannonFlat, DamageKind.Physical); }); break;
                    case "RIDER": Start(c => { foreach (var u in members) { u.Armor += 2 * value; u.MagicResist += 2 * value; c.After(4, () => { u.Armor -= value; u.MagicResist -= value; }, u); } }); break;
                    case "TAMER": Start(_ => {
                        var hero = members.OrderByDescending(u => u.Items.Count * 100 + u.Line.Tier * 10 + u.Stage).FirstOrDefault(); if (hero is null) return;
                        double k = 1 + 0.1 * members.Sum(u => u.Stage), ap = P("ap") * k; hero.MaxHealth += UnitValues.Round(value * k); hero.AbilityPower += ap; hero.AttackDamage += hero.BaseAttackDamage * ap / 100; Emit("text", hero, text: "Hero");
                    }); break;
                    case "EVOKER": On(CombatHook.AnyCast, (_, _) => { foreach (var u in members.Where(u => u.Alive)) u.Mana = Math.Min(u.MaxMana, u.Mana + value); }); break;
                    case "GUARD": Each(u => u.Dragonlands.Guard = value);
                        On(CombatHook.DamageTaken, (c, e) => {
                            var u = e.Unit!; if (u.Dragonlands.Guard == 0 || u.Dragonlands.GuardUsed || u.Health >= u.MaxHealth * 0.5) return;
                            u.Dragonlands.GuardUsed = true; double amount = u.MaxHealth * u.Dragonlands.Guard; c.Shield(u, amount);
                            var ally = c.Mine.Where(a => a.Alive && a != u && !a.Summoned).OrderBy(a => a.Cell.DistanceTo(u.Cell)).FirstOrDefault(); if (ally is not null) c.Shield(ally, amount);
                        }); break;
                    case "MAGE": Each(u => { u.AbilityPower *= value; u.Dragonlands.Mage = true; });
                        On(CombatHook.Cast, (c, e) => {
                            var u = e.Unit!; if (!u.Dragonlands.Mage || u.Dragonlands.MageAgain || !u.Alive) return;
                            var target = e.Target is { Alive: true } t ? t : units.Where(t => t.Alive && t.Team != u.Team).OrderBy(t => u.Cell.DistanceTo(t.Cell) * 10000 + t.Health).FirstOrDefault();
                            if (target is null) return; u.Dragonlands.MageAgain = true; try { c.CastAgain(u, target); } finally { u.Dragonlands.MageAgain = false; }
                        }); break;
                    case "MYSTIC": Start(c => { foreach (var u in Real(c)) u.MagicResist += value; }); break;
                    case "MODE": Each(u => u.Dragonlands.Mode = value);
                        On(CombatHook.Cast, (c, e) => { var u = e.Unit!; if (u.Dragonlands.Mode == 0 || u.Dragonlands.ModeDone) return; u.Dragonlands.ModeDone = true; double add = UnitValues.Round(u.MaxHealth * u.Dragonlands.Mode); u.MaxHealth += add; c.Heal(u, add); c.Text(u, "Mode Change"); }); break;
                    case "SWIFT": Each(u => { u.Range++; u.Dragonlands.Swift = value; });
                        On(CombatHook.Attack, (_, e) => { var u = e.Unit!; if (u.Dragonlands.Swift != 0) u.AttackCooldown /= 1 + u.Dragonlands.Swift * u.Cell.DistanceTo(e.Target!.Cell); }); break;
                    case "WARRIOR": Each(u => u.Dragonlands.Warrior = value);
                        On(CombatHook.Attack, (c, e) => { if (e.Unit!.Dragonlands.Warrior != 0 && c.Random() < 0.5) e.Unit.Dragonlands.Empowered = true; });
                        On(CombatHook.BeforeDamage, (_, e) => { var u = e.Unit!; if (!u.Dragonlands.Empowered || e.Kind != DamageKind.Physical || e.IsBurn) return; u.Dragonlands.Empowered = false; e.Multiplier *= 1 + u.Dragonlands.Warrior; }); break;
                    case "DRAGON": Start(_ => {
                        int limit = rules.DragonTraitLimits?.GetValueOrDefault(setup.Pace ?? "default") ?? 0; if (limit != 0 && count > limit) return;
                        foreach (var u in members) { u.MaxHealth += value; if (level >= 2) { u.Armor += P("armor"); u.MagicResist += P("armor"); } if (level >= 3) u.Equipment.Amp += P("amp"); }
                    }); break;
                    case "STARCALL": bool wished = false;
                        On(CombatHook.Cast, (c, e) => {
                            var u = e.Unit!; if (!Member(u) || wished || setup.RoundType != "pvp") return; wished = true;
                            if (side.Dragonlands?.HpFull == true) { var foes = units.Where(x => x.Alive && x.Team != u.Team && !x.Summoned).ToArray(); if (foes.Length > 0) { var target = foes[(int)(c.Random() * foes.Length)]; c.Text(target, "Wished away"); c.Damage(u, target, target.Health + target.Shield + 1, DamageKind.True); } }
                            else dragonWish[u.Team] = u.Stage == 2 ? 2 : 1;
                            c.Text(u, "Wish");
                        }); break;
                    case "COPYCAT": break;
                    case "CORE": On(CombatHook.Cast, (_, e) => { if (Member(e.Unit!)) dragonDoots[e.Unit!.Team]++; });
                        On(CombatHook.CombatEnd, (c, _) => { if (setup.RoundType == "pvp") foreach (var u in Real(c).Where(u => u.Alive)) if (c.Random() < 0.02) dragonDoots[u.Team]++; }); break;
                    case "MONO": Start(c => { foreach (var u in Real(c)) { int col = (u.Team == 0 ? u.Cell.Column : Columns - 1 - u.Cell.Column) % 7; if ((u.Entry.Row == 4 && col is 2 or 4) || (u.Entry.Row == 6 && col == 3)) { u.Equipment.Reduction += value; c.Text(u, "Monolith"); } } }); break;
                    default: throw new NotSupportedException($"Dragonlands trait {id} has not been migrated.");
                }
            }
        }
    }
}

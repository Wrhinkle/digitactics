namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private readonly Dictionary<string, int>[] underlordsCounts = [[], []];
    private int UnderlordsTraitLevel(int team, string id) => rules.Traits.FirstOrDefault(t => t.Id == id) is { } trait ? TraitRules.Level(trait, underlordsCounts[team].GetValueOrDefault(id)) : 0;
    private void BindUnderlordsTraits(CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
    {
        for (int team = 0; team < 2; team++) {
            var side = team == 0 ? setup.SideA : setup.SideB;
            var entries = team == 0 ? sideA : sideB;
            var counts = underlordsCounts[team] = side.NoTraits ? [] : side.Counts ?? (side.Parts is { } parts ? TraitRules.TeamCount(rules, entries, parts) : TraitRules.Count(rules, entries, side.Augments));
            int active = counts.Count(p => UnderlordsTraitLevel(team, p.Key) > 0);
            foreach (var (id, count) in counts) {
                var trait = rules.Traits.FirstOrDefault(t => t.Id == id); if (trait is null) continue;
                int level = TraitRules.Level(trait, count); if (level == 0) continue;
                double value = trait.Values[level - 1]; double P(string key) => trait.Parameters[key][level - 1];
                var context = Hooks.Context(CombatHookSource.Trait, id, team);
                bool Member(CombatUnit u) => u.Traits.Contains(id);
                CombatUnit[] Members(CombatHookContext c) => c.Mine.Where(u => u.Alive && Member(u)).ToArray();
                void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action, int order = 0) => Hooks.Add(context, hook, action, order);
                void Start(Action<CombatHookContext> action, int order = 0) => On(CombatHook.CombatStart, (c, _) => action(c), order);
                void Each(Action<CombatUnit> action) => Start(c => { foreach (var u in Members(c)) action(u); });
                switch (id) {
                    case "SHINOBI":
                        Each(u => { u.CriticalChance += value; u.CriticalDamage = Math.Max(u.CriticalDamage, P("critDmg")); });
                        On(CombatHook.DamageDealt, (_, e) => { if (e.Critical && Member(e.Unit!) && e.Target!.Alive) e.Target.WoundUntil = Time + 3; }); break;
                    case "BEAST": Each(u => u.MaxHealth += P("base") + UnitValues.Round(value * (side.Underlords?.UlBeastKills ?? 0) * (u.Stage + 1))); break;
                    case "ONI": On(CombatHook.Attack, (c, e) => {
                        var u = e.Unit!; var target = e.Target!; if (!Member(u) || !target.Alive) return;
                        if (target.WeakUntil <= Time) c.Damage(u, target, P("dmg")); if (target.Alive) c.Weaken(target, value, 4);
                    }); break;
                    case "LEGEND":
                        var inherited = counts.Keys.Where(t => t != "LEGEND" && UnderlordsTraitLevel(team, t) > 0).ToArray();
                        Start(c => { foreach (var u in Members(c)) foreach (string t in inherited) if (!u.Traits.Contains(t)) u.Traits = u.Traits.Append(t).ToArray(); }, -100);
                        On(CombatHook.CombatReady, (c, _) => { foreach (var u in Members(c)) { u.Equipment.Amp += value * active; u.MaxHealth = UnitValues.Round(u.MaxHealth * (1 + P("hp") * active)); } }); break;
                    case "DEMON": On(CombatHook.Cast, (c, e) => {
                        var u = e.Unit!; if (!Member(u)) return; var demons = Members(c);
                        c.Buff(u, CombatStat.Amplification, value * demons.Length, 4); foreach (var d in demons) if (d != u) c.Silence(d, 2.5);
                    }); break;
                    case "DRAGON":
                        Each(u => { u.AbilityBurn = true; u.AbilityPower += 15; });
                        On(CombatHook.Cast, (c, e) => {
                            var u = e.Unit!; if (!Member(u) || e.Target is null || e.Value <= 0) return;
                            foreach (var x in EnemiesNear(u, e.Target.Cell, 1)) if (x != e.Target) c.Damage(u, x, e.Value * value);
                        }); break;
                    case "UNDEAD": On(CombatHook.Lethal, (c, e) => {
                        var u = e.Unit!; if (!Member(u) || u.Underlords.Fell) return;
                        u.Underlords.Fell = true;
                        foreach (var a in UnderlordsAllies(u, 2)) if (a != u) { c.Heal(a, value); a.Equipment.Amp += 0.06; }
                        if (level >= 2 && u.Health <= 0) u.Health = u.MaxHealth * 0.4;
                    }); break;
                    case "HOLY": Start(c => HealingAmplification[c.Team!.Value] += value); break;
                    case "NIGHT": Start(c => { foreach (var u in units) if (u.Team != c.Team) u.Armor -= value; }); break;
                    case "PARTNER":
                        double next = 1;
                        On(CombatHook.Tick, (c, _) => { if (Time < next) return; next += 1; foreach (var u in c.Mine) if (u.Alive && u.MaxMana > 0) u.Mana = Math.Min(u.MaxMana, u.Mana + value); }); break;
                    case "HUNTER": On(CombatHook.Attack, (c, e) => {
                        var u = e.Unit!; if (!Member(u) || c.Random() >= value) return;
                        for (int k = 0; k < level && e.Target!.Alive; k++) c.Damage(u, e.Target, u.AttackDamage, DamageKind.Physical);
                    }); break;
                    case "KNIGHT": On(CombatHook.BeforeDamageTaken, (c, e) => {
                        var u = e.Unit!; if (!Member(u)) return;
                        bool beside = c.Mine.Any(a => a.Alive && a.Line.Id == "ul_chaosknight" && a.Stage == 2) || c.Adjacent(u).Any(a => a != u && a.Alive && a.Team == u.Team && a.Traits.Contains("KNIGHT"));
                        e.Reduction += value * (beside ? 2 : 1);
                    }); break;
                    case "WIZARD": On(CombatHook.BeforeDamage, (c, e) => { if (e.Kind == DamageKind.Magic && e.Unit!.Team == c.Team) e.Multiplier *= 1 + value; }); break;
                    case "SAGE": On(CombatHook.DamageTaken, (_, e) => {
                        var u = e.Unit!; if (!Member(u) || u.Health >= u.MaxHealth * 0.5 || u.Underlords.Sage) return;
                        u.Underlords.Sage = true; u.InvulnerableUntil = Time + 3; u.CastLock = Math.Max(u.CastLock, 3);
                    }); break;
                    case "TOXIC": On(CombatHook.Attack, (_, e) => { if (Member(e.Unit!)) UnderlordsPoison(e.Unit!, e.Target!); }); break;
                    case "ROGUE": Each(u => u.Underlords.Rogue = new(value, P("ad"))); break;
                    case "NATURE": On(CombatHook.Attack, (_, e) => { var u = e.Unit!; if (level >= 3 || Member(u) || (level >= 2 && u.Summoned)) u.AttackDamage += value; }); break;
                    case "DEEP": Start(c => { foreach (var u in c.Mine) u.MagicResist += value; }); break;
                    case "SHAMAN": Start(c => {
                        var shaman = Members(c).OrderByDescending(u => u.Stage).ThenByDescending(u => u.Line.Tier).ThenBy(u => u.Id).FirstOrDefault();
                        if (shaman is null) return;
                        UnderlordsSummon(shaman, new[] { "ul_s_wolf", "ul_s_hatchling", "ul_s_blackdragon" }[level - 1], level == 3 ? 0 : shaman.Stage, shaman);
                    }, 10); break;
                    case "SPIRIT":
                        var charged = new List<CombatUnit>();
                        On(CombatHook.Cast, (c, e) => {
                            var u = e.Unit!; if (!Member(u)) return; if (!charged.Contains(u)) charged.Add(u);
                            var ready = charged.Where(s => s.Alive).ToArray(); if (ready.Length < 3) return;
                            var three = ready.Take(3).ToArray(); charged = ready.Skip(3).ToList();
                            var points = three.Select(s => s.Target is { Alive: true } t ? t : s).ToArray();
                            var center = new HexCell((int)UnitValues.Round(points.Sum(p => p.Cell.Column) / 3.0), (int)UnitValues.Round(points.Sum(p => p.Cell.Row) / 3.0));
                            double damage = 0, stun = 0;
                            foreach (var s in three) { double mult = UnderlordsValue(s, 1, 1.5, 2); damage += value * mult; stun += 1.5 * mult; }
                            foreach (var x in EnemiesNear(three[0], center, 2)) { c.Damage(three[0], x, damage); if (x.Alive) c.Stun(x, stun / 3); }
                        }); break;
                    case "SUMMONER": On(CombatHook.BeforeDamage, (c, e) => { if (e.Unit!.Summoned && e.Unit.Team == c.Team) e.Multiplier *= 1 + value; }); break;
                    case "BLADE": On(CombatHook.BeforeDamage, (c, e) => {
                        var u = e.Unit!; if (!Member(u) || e.IsBurn) return;
                        int adjacent = c.Adjacent(u).Count(x => x.Alive && x.Team != u.Team); if (adjacent > 0) e.Multiplier *= 1 + value * adjacent;
                    }); break;
                    case "JUNGLE": Start(c => { foreach (var u in c.Mine) u.AttackSpeed *= 1 + (Member(u) ? value : P("other")); }); break;
                    case "WIND":
                        On(CombatHook.AnyCast, (c, e) => {
                            var caster = e.Unit!; if (caster.Team == c.Team || !caster.Alive) return;
                            foreach (var u in c.Mine) {
                                if (!u.Alive || (level < 2 && !Member(u)) || u.Cell.DistanceTo(caster.Cell) > u.Range) continue;
                                u.Target = caster; u.Underlords.Vigilant = new(caster.Id, value);
                            }
                        });
                        On(CombatHook.BeforeDamage, (_, e) => {
                            var u = e.Unit!; if (u.Underlords.Vigilant is not { } vig || e.Kind != DamageKind.Physical || e.Target!.Id != vig.Target) return;
                            e.Multiplier *= 1 + vig.Amp; u.Underlords.Vigilant = null;
                        }); break;
                    case "DARK": On(CombatHook.Attack, (c, e) => {
                        var u = e.Unit!; if (u.Summoned || !e.Target!.Alive || u.Underlords.VoidCooldown > Time || c.Random() >= 0.25) return;
                        u.Underlords.VoidCooldown = Time + 0.5; c.Damage(u, e.Target, e.Target.MaxHealth * value, DamageKind.True);
                    }); break;
                    case "METAL": Each(u => u.Armor += value); break;
                    default: throw new NotSupportedException($"Underlords alliance {id} has not been migrated.");
                }
            }
        }
    }
}

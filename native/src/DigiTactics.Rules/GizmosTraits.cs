namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private static readonly string[] Mutations = ["cyber", "voracious", "voidborne", "adrenal", "synaptic", "leech", "meta"];
    private void BindGizmosTraits(CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
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
                void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action) => Hooks.Add(context, hook, action);
                void Start(Action<CombatHookContext> action) => On(CombatHook.CombatStart, (c, _) => action(c));
                void Each(Action<CombatUnit> action) => Start(_ => { foreach (var u in members) action(u); });
                double M(double low, double high) => level >= 2 ? high : low;
                switch (id)
                {
                    case "MASQ": Start(_ => {
                        var vip = members.OrderByDescending(u => u.Items.Count * 100 + u.Line.Tier * 10 + u.Stage).FirstOrDefault();
                        foreach (var u in members) { double k = u == vip && u.Items.Count > 0 ? 1.5 : 1; u.MaxHealth += UnitValues.Round(value * k); u.AbilityPower += UnitValues.Round(P("ap") * k); }
                    }); break;
                    case "DB": On(CombatHook.CombatReady, (c, _) => {
                        var foes = units.Where(e => e.Alive && e.Team != c.Team).ToArray();
                        var big = foes.OrderByDescending(e => e.MaxHealth).FirstOrDefault();
                        void Arrest(CombatUnit? e, double duration) { if (e is null) return; c.Stun(e, duration); if (e.Stun > 0) e.Gizmos.ArrestBreak = e.Health - e.MaxHealth * 0.4; }
                        Arrest(big, value);
                        if (level >= 2) { Arrest(foes.Where(e => e != big).OrderByDescending(e => e.AttackDamage).FirstOrDefault(), P("second")); foreach (var u in members) u.AttackSpeed *= 1 + P("as"); }
                    }); break;
                    case "CLOCK": Start(c => { foreach (var u in c.Mine) u.AttackSpeed *= 1 + value + P("per") * (side.Augments?.Length ?? 0); }); break;
                    case "DM": Start(c => {
                        var chosen = level >= 3 ? c.Mine : level >= 2 ? members : members.OrderBy(u => u.MaxHealth).ThenBy(u => u.Id).Take(2);
                        foreach (var u in chosen) { u.Armor += value; u.MagicResist += value; u.Omnivamp += P("vamp"); }
                    }); break;
                    case "JUNK": Start(c => {
                        int n = 0;
                        foreach (var u in c.Mine) foreach (var itemId in u.Items) {
                            if (!items.TryGetValue(itemId, out var item)) continue;
                            int k = Member(u) ? 2 : 1;
                            if (item.Component && !item.Special) n += k; else if (item.Category is "completed" or "radiant") n += 2 * k;
                        }
                        if (n != 0) foreach (var u in c.Mine) c.Shield(u, value * n);
                        foreach (var u in members) { u.Armor += P("armor"); u.MagicResist += P("armor"); }
                        double budget = P("conv");
                        foreach (var u in members) foreach (var itemId in u.Items) {
                            if (budget <= 0) break;
                            if (!items.TryGetValue(itemId, out var item) || !item.Component || item.Special || item.CompletedFrom is not { Length: > 0 } options) continue;
                            ApplyTemporaryItemUpgrade(u, items[options[(int)(c.Random() * options.Length)]], item); budget--;
                        }
                    }); break;
                    case "PIR": Each(u => u.Equipment.Amp += value); break;
                    case "CD":
                        double cdNext = 0, mult = 1;
                        Start(_ => mult = 1 + 0.15 * (side.Augments?.Length ?? 0));
                        On(CombatHook.Tick, (c, _) => {
                            if (Time < cdNext) return; cdNext += 6; double amount = UnitValues.Round(value * mult);
                            foreach (var u in members.Where(u => u.Alive)) { c.Shield(u, amount); u.Gizmos.CyberUntil = Time + 4; c.After(4, () => u.Shield -= Math.Min(u.Shield, amount), u); }
                        });
                        On(CombatHook.Attack, (c, e) => { if (Member(e.Unit!) && e.Unit!.Gizmos.CyberUntil > Time && e.Target!.Alive) c.Damage(e.Unit, e.Target, P("dmg") * mult); }); break;
                    case "MUT":
                        string mutation = ""; bool busy = false; double next = 4;
                        Start(c => {
                            mutation = setup.Mutation ?? Mutations[(int)(c.Random() * Mutations.Length)];
                            foreach (var u in members) {
                                if (mutation == "cyber") { double k = u.Items.Count > 0 ? 1.5 : 1; u.MaxHealth += UnitValues.Round(M(200, 380) * k); u.AttackDamage += u.BaseAttackDamage * M(0.12, 0.22) * k; }
                                if (mutation == "synaptic") u.MaxMana = Math.Max(20, u.MaxMana - M(20, 35));
                            }
                            if (mutation == "leech") foreach (var u in c.Mine) u.Omnivamp += M(0.12, 0.22);
                        });
                        On(CombatHook.Death, (c, _) => { if (mutation == "voracious") foreach (var u in members.Where(u => u.Alive)) { c.Buff(u, CombatStat.AttackSpeed, M(0.1, 0.18)); u.AttackDamage += u.BaseAttackDamage * M(0.04, 0.08); } });
                        On(CombatHook.DamageDealt, (c, e) => {
                            var target = e.Target!;
                            if (mutation != "voidborne" || busy || !Member(e.Unit!) || !target.Alive || target.Health > target.MaxHealth * M(0.1, 0.18)) return;
                            busy = true; try { c.Damage(e.Unit!, target, target.Health + target.Shield + 10, DamageKind.True); } finally { busy = false; }
                        });
                        On(CombatHook.Attack, (c, e) => { if (mutation == "adrenal" && Member(e.Unit!) && e.Target!.Alive && c.Random() < M(0.2, 0.35)) c.Damage(e.Unit!, e.Target, e.Amount, DamageKind.Physical, e.Critical); });
                        On(CombatHook.Tick, (_, _) => {
                            if (mutation != "meta" || Time < next) return; next += 4;
                            foreach (var u in members.Where(u => u.Alive)) { u.Armor += M(6, 12); u.MagicResist += M(6, 12); u.AttackDamage += u.BaseAttackDamage * M(0.05, 0.1); u.AbilityPower += M(6, 12); }
                        }); break;
                    case "PET": Each(u => { u.Dodge += value; if (level >= 2) u.MaxMana = Math.Max(20, UnitValues.Round(u.MaxMana * 0.8)); }); break;
                    case "XA":
                        void Trigger(CombatHookContext c, CombatUnit u, bool start) { u.Gizmos.Antibody = start ? 1 : 2; c.Buff(u, CombatStat.AttackSpeed, value, 8); c.Buff(u, CombatStat.DamageReduction, 0.15, 8); c.Buff(u, CombatStat.Regeneration, P("regen"), 8); }
                        Start(c => { foreach (var u in members) { u.Gizmos.Antibody = 1; if ((side.Augments ?? []).Contains("giz_a_injection")) Trigger(c, u, true); } });
                        On(CombatHook.DamageTaken, (c, e) => { var u = e.Unit!; if (u.Gizmos.Antibody == 1 && Member(u) && u.Health < u.MaxHealth * 0.75) Trigger(c, u, false); }); break;
                    case "SIS": Each(u => { if (u.Line.Id == "giz_sistermonb") u.MaxMana = Math.Max(20, u.MaxMana - 20); if (u.Line.Id == "giz_sistermonn") u.Gizmos.Noir = true; u.Equipment.Amp += value; });
                        On(CombatHook.Kill, (c, e) => { if (e.Unit!.Gizmos.Noir && e.Unit.Alive) c.Buff(e.Unit, CombatStat.AttackSpeed, 0.4, 3); }); break;
                    case "IDOL":
                        CombatUnit? spot = null; double spotNext = 1;
                        On(CombatHook.CombatReady, (c, _) => { spot = c.Mine.Where(u => !u.Summoned).OrderByDescending(u => u.Items.Count * 1000 + u.Line.Tier * 100 + u.Stage * 10 - u.Id * 0.001).FirstOrDefault(); if (spot is not null) { spot.Equipment.Amp += value; if (level >= 3) spot.Omnivamp += P("vamp"); } });
                        On(CombatHook.Tick, (_, _) => { if (spot is not { Alive: true } || level < 2 || Time < spotNext) return; spotNext++; spot.Mana = Math.Min(spot.MaxMana, spot.Mana + P("mana")); }); break;
                    case "GLUT": Each(u => { if (u.Entry.GluttonHealth != 0) { u.MaxHealth += u.Entry.GluttonHealth; u.AbilityPower += u.Entry.GluttonAbilityPower; } }); break;
                    case "TRANS": Each(u => { u.Gizmos.Melee = u.Entry.Row <= 5; if (u.Gizmos.Melee) { u.Range = 1; u.Armor += 40; u.MagicResist += 40; } else u.Range = 4; });
                        On(CombatHook.Cast, (c, e) => {
                            var u = e.Unit!; if (!Member(u)) return;
                            if (u.Gizmos.Melee) foreach (var x in EnemiesNear(u, u.Cell, 2)) { c.Buff(x, CombatStat.Armor, -UnitValues.Round(x.Armor * 0.3), 4); c.Buff(x, CombatStat.MagicResist, -UnitValues.Round(x.MagicResist * 0.3), 4); }
                            else foreach (var a in c.Mine.Where(a => a.Alive && a.Cell.Row == u.Cell.Row && a.Cell.DistanceTo(u.Cell) <= 6)) c.Buff(a, CombatStat.AttackSpeed, 0.4, 4);
                        }); break;
                    case "MIND": Start(c => {
                        foreach (var u in members) {
                            var others = c.Mine.Where(a => a != u).ToArray();
                            var chosen = others.Where(a => a.Cell.Row == u.Cell.Row + (u.Team == 0 ? -1 : 1) && a.Cell.DistanceTo(u.Cell) <= 1).ToList();
                            if (chosen.Count < 2) chosen.AddRange(others.Where(a => !chosen.Contains(a)).OrderBy(a => a.Cell.DistanceTo(u.Cell)).ThenBy(a => a.Id).Take(2 - chosen.Count));
                            foreach (var a in chosen.Take(2)) a.Mana = Math.Min(a.MaxMana - 1, a.Mana + value);
                        }
                    }); break;
                    case "ARC": Start(c => { foreach (var u in c.Mine) u.AbilityPower += value + (Member(u) ? P("own") : 0); }); break;
                    case "ASN": Each(u => { u.CriticalChance += value; u.CriticalDamage += P("critDmg"); u.Equipment.AbilityCrit = true; }); break;
                    case "BG":
                        Each(u => u.Armor += value); bool guarded = false;
                        On(CombatHook.Tick, (c, _) => { if (guarded || Time < 1.5) return; guarded = true; foreach (var u in members.Where(u => u.Alive)) { c.Shield(u, P("shield")); foreach (var e in EnemiesNear(u, u.Cell, 1)) e.Target = u; } }); break;
                    case "BRU": Start(c => { foreach (var u in c.Mine) u.MaxHealth += value * (Member(u) ? 2 : 1); }); break;
                    case "CHAL": Each(u => u.AttackSpeed *= 1 + value);
                        On(CombatHook.Kill, (c, e) => { if (Member(e.Unit!) && e.Unit!.Alive) c.Buff(e.Unit, CombatStat.AttackSpeed, value, 2.5); }); break;
                    case "COL": Each(u => { u.Equipment.Reduction += value; if (level >= 2) u.MaxHealth = UnitValues.Round(u.MaxHealth * (1 + P("hp"))); }); break;
                    case "ENCH": Start(c => { HealingAmplification[c.Team!.Value] += P("heal"); foreach (var u in c.Mine) { u.MagicResist += value; if (Member(u)) u.AbilityPower += P("ap"); } }); break;
                    case "INNO": On(CombatHook.CombatReady, (c, _) => {
                        var inns = members.Where(u => u.Alive && !u.Summoned).ToArray(); if (inns.Length == 0) return;
                        int stars = inns.Sum(u => u.Stage + 1); HexCell? cell = null;
                        foreach (var u in inns) { cell = c.FreeCellNear(u, u.Cell); if (cell is not null) break; }
                        if (cell is not null) c.Summon(c.Team!.Value, "giz_inno", (int)value, cell.Value, new(HealthMultiplier: 0.45 + 0.04 * stars, AttackMultiplier: 0.5 + 0.04 * stars, NoHooks: true));
                    }); break;
                    case "SCH": double scholarNext = 2;
                        On(CombatHook.Tick, (c, _) => { if (Time < scholarNext) return; scholarNext += 2; foreach (var u in c.Mine.Where(u => u.Alive && u.MaxMana > 0)) u.Mana = Math.Min(u.MaxMana, u.Mana + value); }); break;
                    case "SNI": Each(u => u.PerHexAmplification = value); break;
                    case "STR": Each(u => u.AttackDamage += u.BaseAttackDamage * value); break;
                    case "TWIN": Each(u => u.AttackDamage += u.BaseAttackDamage * P("ad"));
                        On(CombatHook.Attack, (c, e) => { if (Member(e.Unit!) && e.Target!.Alive && c.Random() < value) c.Damage(e.Unit!, e.Target, e.Amount, DamageKind.Physical, e.Critical); }); break;
                    default: throw new NotSupportedException($"Gizmos trait {id} has not been migrated.");
                }
            }
        }
    }
}

namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private sealed class ColiseumPlant(CombatUnit unit, string kind, HexCell cell)
    {
        internal CombatUnit Unit = unit;
        internal readonly string Kind = kind;
        internal readonly HexCell Cell = cell;
    }

    private CombatUnit? GrowColiseumPlant(CombatHookContext c, CombatUnit rose, string kind, HexCell cell)
    {
        var p = c.Summon(rose.Team, kind, Math.Max(0, rose.Stage - 1), cell, new(HealthMultiplier: 0.5 * (rose.AbilityPower / 100)));
        if (p is null) return null;
        p.Health = p.MaxHealth;
        if (kind == "col_spine") { p.AttackDamage *= 0.6 * (rose.AbilityPower / 100); p.AttackSpeed = rose.AttackSpeed; }
        return p;
    }

    private static void ColiseumLesson(CombatUnit u, string line)
    {
        switch (line) {
            case "col_kamemon": u.Equipment.Reduction += 0.06; break;
            case "col_reppamon": u.AttackDamage += u.BaseAttackDamage * 0.08; u.AbilityPower += 8; break;
            case "col_kotemon": u.AttackSpeed *= 1.1; break;
            case "col_wizardmon": u.Equipment.Shojin += 2; break;
        }
    }

    private void BindColiseumTraits(CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
    {
        int stage = setup.RoundStage == 0 ? 1 : setup.RoundStage;
        for (int team = 0; team < 2; team++) {
            var side = team == 0 ? setup.SideA : setup.SideB; if (side.NoTraits) continue;
            var entries = team == 0 ? sideA : sideB;
            var counts = side.Counts ?? (side.Parts is { } parts ? TraitRules.TeamCount(rules, entries, parts) : TraitRules.Count(rules, entries, side.Augments));
            foreach (var (id, count) in counts) {
                var trait = rules.Traits.FirstOrDefault(t => t.Id == id); if (trait is null) continue;
                int level = TraitRules.Level(trait, count); if (level == 0) continue;
                double value = trait.Values[level - 1]; double P(string key) => trait.Parameters[key][level - 1];
                var context = Hooks.Context(CombatHookSource.Trait, id, team);
                bool Member(CombatUnit u) => u.Traits.Contains(id);
                CombatUnit[] Members(CombatHookContext c) => c.Mine.Where(u => u.Alive && Member(u)).ToArray();
                void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action) => Hooks.Add(context, hook, action);
                void Start(Action<CombatHookContext> action) => On(CombatHook.CombatStart, (c, _) => action(c));
                void Each(Action<CombatUnit> action) => Start(c => { foreach (var u in Members(c)) action(u); });
                switch (id) {
                    case "BA":
                        Each(u => { u.MaxHealth += 50 * value; u.Coliseum.Academy = 0.09 * value; });
                        On(CombatHook.BeforeDamage, (_, e) => { if (e.Kind == DamageKind.Magic && e.Unit!.Coliseum.Academy != 0) e.Multiplier *= 1 + e.Unit.Coliseum.Academy; }); break;
                    case "CG": Each(u => { u.MaxHealth += P("hp"); u.Equipment.Amp += P("amp"); }); break;
                    case "MEN": Start(c => {
                        string[] lessons = ["col_kamemon", "col_reppamon", "col_kotemon", "col_wizardmon"];
                        var mentors = Members(c).Where(u => lessons.Contains(u.Line.Id)).ToArray(); if (mentors.Length == 0) return;
                        if (count == 1) { foreach (var u in c.Mine) ColiseumLesson(u, mentors[0].Line.Id); return; }
                        foreach (var u in mentors) { foreach (var lesson in lessons) ColiseumLesson(u, lesson); u.AbilityPower += 10; }
                    }); break;
                    case "MECH":
                        CombatUnit? mech = null;
                        Start(c => {
                            int stars = Members(c).Sum(u => u.Stage + 1); HexCell? cell = null;
                            foreach (int row in c.Team == 0 ? new[] { 4, 5, 6, 7 } : new[] { 3, 2, 1, 0 }) {
                                foreach (int col in new[] { 3, 2, 4, 1, 5, 0, 6 }) if (UnitAt(new(col, row)) is null) { cell = new(col, row); break; }
                                if (cell is not null) break;
                            }
                            if (cell is null) return;
                            mech = c.Summon(c.Team!.Value, "col_mech", 0, cell.Value); if (mech is null) return;
                            double late = 1 + 0.1 * Math.Max(0, stage - 2);
                            mech.MaxHealth = mech.Health = UnitValues.Round((400 + 145 * stars) * late);
                            mech.AttackDamage = mech.BaseAttackDamage = (35 + 10 * stars) * late;
                            if (level >= 2) { mech.AttackDamage *= 1.15; mech.Equipment.Split = 0.3; } c.Text(mech, "Mega Mech");
                            if (level >= 3) { mech.Omnivamp += 0.25; mech.Equipment.Reduction += 0.2; }
                        });
                        On(CombatHook.DamageDealt, (c, e) => { if (mech is { Alive: true } && e.Unit != mech && Member(e.Unit!)) c.Heal(mech, e.Amount * 0.12, true); }); break;
                    case "MT": case "CH": break;
                    case "RC": On(CombatHook.DamageDealt, (c, e) => {
                        if (e.Kind != DamageKind.True && !e.IsBurn && e.Unit!.Traits.Contains("CREW") && e.Target!.Alive) c.Damage(e.Unit, e.Target, e.Amount * 0.12, DamageKind.True);
                    }); break;
                    case "RM":
                        CombatUnit? rose = null; var plants = new List<ColiseumPlant>();
                        Start(c => {
                            rose = c.Mine.FirstOrDefault(u => u.Line.Id == "col_lotosmon"); if (rose is null) return;
                            for (int i = 0; i < (rose.Stage >= 2 ? 4 : 2); i++) {
                                string kind = i % 2 == 0 ? "col_root" : "col_spine"; int forward = c.Team == 0 ? -3 : 3;
                                var toward = new HexCell(rose.Cell.Column + (i >= 2 ? i == 2 ? -2 : 2 : 0), rose.Cell.Row + (kind == "col_root" ? forward : -forward));
                                var cell = c.FreeCellNear(rose, toward); if (cell is null) break;
                                var p = GrowColiseumPlant(c, rose, kind, cell.Value); if (p is not null) plants.Add(new(p, kind, cell.Value));
                            }
                        });
                        On(CombatHook.Cast, (c, e) => {
                            if (e.Unit != rose || rose is null) return;
                            foreach (var plant in plants) {
                                if (plant.Unit.Alive) { c.Heal(plant.Unit, plant.Unit.MaxHealth * 0.35); c.Buff(plant.Unit, CombatStat.AttackSpeed, 0.35); continue; }
                                HexCell? cell = UnitAt(plant.Cell) is not null ? c.FreeCellNear(rose, plant.Cell) : plant.Cell; if (cell is null) continue;
                                var p = GrowColiseumPlant(c, rose, plant.Kind, cell.Value); if (p is not null) plant.Unit = p;
                            }
                        }); break;
                    case "SF":
                        int stacks = 0; double nextStack = 1;
                        Each(u => u.MaxHealth += value);
                        On(CombatHook.Tick, (c, _) => {
                            if (stacks >= 8 || Time < nextStack) return; nextStack++; stacks++;
                            foreach (var u in Members(c)) { u.AttackDamage += u.BaseAttackDamage * P("sps"); u.AbilityPower += P("sps") * 100; if (stacks == 8) c.Text(u, "Max Spirit"); }
                        });
                        On(CombatHook.DamageDealt, (c, e) => { if (stacks >= 8 && e.Kind != DamageKind.True && !e.IsBurn && Member(e.Unit!) && e.Target!.Alive) c.Damage(e.Unit!, e.Target, e.Amount * P("td"), DamageKind.True); }); break;
                    case "SM": Hooks.Add(context, CombatHook.CombatStart, (c, _) => {
                        foreach (var u in c.Mine.Where(u => u.Line.Id == "col_justimon")) {
                            var form = rules.Forms?.GetValueOrDefault(u.Line.Id)?.FirstOrDefault(f => f.Id == (u.Entry.Coliseum?.Form ?? "blitz"));
                            if (form?.Trait is { } added && !u.Traits.Contains(added)) u.Traits = u.Traits.Append(added).ToArray();
                        }
                    }, order: -100); break;
                    case "SG":
                        Start(c => {
                            var lines = c.Mine.Where(u => u.Line.Traits.Contains("SG")).Select(u => u.Line.Id).ToHashSet();
                            if (lines.Contains("col_lalamon")) HealingAmplification[c.Team!.Value] += 0.1 * value;
                            foreach (var u in Members(c)) {
                                if (lines.Contains("col_gotsumon")) u.Shield += 150 * value;
                                if (lines.Contains("col_syakomon")) u.Coliseum.StarAp = 4 * value;
                                if (lines.Contains("col_hawkmon")) u.Coliseum.StarHit = (30 + 15 * stage) * value;
                                if (lines.Contains("col_kyubimon")) u.Coliseum.StarMana = 15 * value;
                                if (lines.Contains("col_bearmon")) u.Coliseum.StarHeal = 0.25 * value;
                                if (lines.Contains("col_gargomon")) u.AttackSpeed *= 1 + 0.12 * value;
                                if (lines.Contains("col_ophanimon")) { u.Armor += 25 * value; u.MagicResist += 25 * value; u.MaxHealth += 100 * value; u.AttackDamage += u.BaseAttackDamage * 0.08 * value; u.AttackSpeed *= 1 + 0.08 * value; u.CriticalChance += 0.08 * value; }
                            }
                        });
                        On(CombatHook.UnitTick, (c, e) => { var u = e.Unit!; if (u.Coliseum.StarAp != 0 && c.Every(u, "sgAp", 3)) u.AbilityPower += u.Coliseum.StarAp; });
                        On(CombatHook.Attack, (c, e) => { var u = e.Unit!; if (u.Coliseum.StarHit != 0 && u.Attacks % 3 == 0 && e.Target!.Alive) c.Damage(u, e.Target, u.Coliseum.StarHit); });
                        On(CombatHook.Cast, (_, e) => { var u = e.Unit!; if (u.Coliseum.StarMana != 0) u.Mana = Math.Min(u.MaxMana, u.Mana + u.Coliseum.StarMana); });
                        On(CombatHook.DamageTaken, (c, e) => { var u = e.Unit!; var s = u.Coliseum; if (s.StarHeal != 0 && !s.StarHealed && u.Health < u.MaxHealth * 0.4) { s.StarHealed = true; c.Heal(u, u.MaxHealth * s.StarHeal); } }); break;
                    case "SC":
                        double supremeBonus = P("sup") - value;
                        void Supreme(CombatUnit u) { u.Coliseum.Supreme = true; u.Equipment.Amp += supremeBonus; Emit("text", u, text: "Supreme"); }
                        Start(c => {
                            var members = Members(c); foreach (var u in members) u.Equipment.Amp += value;
                            foreach (var u in members.OrderByDescending(u => u.Line.Id == side.Coliseum?.Supreme).ThenByDescending(u => u.Line.Tier).ThenByDescending(u => u.Stage).ThenBy(u => u.Id).Take(level >= 3 ? 2 : 1)) Supreme(u);
                        });
                        On(CombatHook.DamageDealt, (c, e) => { var t = e.Target!; if (e.Unit!.Coliseum.Supreme && t.Alive && e.Kind != DamageKind.True && t.Health < t.MaxHealth * 0.1) c.Damage(e.Unit, t, (t.Health + t.Shield) * 3 + 10, DamageKind.True); });
                        On(CombatHook.Death, (c, e) => { if (!e.Unit!.Coliseum.Supreme) return; e.Unit.Coliseum.Supreme = false;
                            var next = Members(c).Where(u => !u.Coliseum.Supreme).OrderByDescending(u => u.Dealt).ThenBy(u => u.Id).FirstOrDefault(); if (next is not null) Supreme(next);
                        }); break;
                    case "CREW":
                        double accumulated = 0; bool firing = false;
                        Each(u => { u.MaxHealth = UnitValues.Round(u.MaxHealth * (1 + value)); u.AttackSpeed *= 1 + value; });
                        On(CombatHook.DamageDealt, (c, e) => {
                            if (firing || e.Kind == DamageKind.True || e.IsBurn || !Member(e.Unit!)) return;
                            accumulated += e.Amount;
                            while (accumulated >= 400) {
                                accumulated -= 400; var pool = units.Where(u => u.Alive && u.Team != c.Team).ToArray(); if (pool.Length == 0) break;
                                var t = pool[(int)(c.Random() * pool.Length)]; firing = true; c.Effect("shot", e.Unit!, t, big: true); c.Damage(e.Unit!, t, 50 + 25 * stage); firing = false;
                            }
                        }); break;
                    case "WR":
                        double phantomPool = 0, nextRealm = 4; bool realm = false;
                        On(CombatHook.DamageDealt, (_, e) => { if (!realm && Member(e.Unit!)) phantomPool += e.Amount; });
                        On(CombatHook.Tick, (c, _) => {
                            if (Time < nextRealm) return; nextRealm += 4; var ms = Members(c); if (ms.Length == 0 || phantomPool <= 0) return;
                            double amount = phantomPool * value, healing = phantomPool * 0.18; phantomPool = 0; realm = true;
                            foreach (var e in EnemiesOf(ms[0]).OrderBy(e => e.Cell.DistanceTo(ms[0].Cell) * 10000 + e.Id).Take(3).ToArray()) { c.Effect("beam", ms[0], e); c.Damage(ms[0], e, amount * 0.6); }
                            realm = false; var low = ms.Where(u => u.Alive).OrderBy(u => u.Health / u.MaxHealth).ThenBy(u => u.Id).FirstOrDefault(); if (low is not null) c.Heal(low, healing);
                        }); break;
                    case "LU":
                        Each(u => { u.AttackDamage += u.BaseAttackDamage * value; u.Coliseum.Luchador = P("heal"); });
                        On(CombatHook.DamageTaken, (c, e) => { var u = e.Unit!; var s = u.Coliseum;
                            if (s.Luchador == 0 || s.LuchadorUsed || u.Health >= u.MaxHealth * 0.5) return;
                            s.LuchadorUsed = true; u.Stun = 0; c.Heal(u, u.MaxHealth * s.Luchador); foreach (var a in c.Adjacent(u)) if (a.Alive && a.Team != u.Team) c.Stun(a, 1); c.Text(u, "Luchador!"); c.Burst(u.Cell, 1, u.Team);
                        }); break;
                    case "BAS": Start(c => { foreach (var u in c.Mine) {
                        double k = Member(u) ? value : 6 + (level >= 3 ? 25 : 0); u.Armor += k; u.MagicResist += k;
                        if (Member(u)) { c.Buff(u, CombatStat.Armor, value, 10); c.Buff(u, CombatStat.MagicResist, value, 10); }
                    } }); break;
                    case "DUE": Each(u => { u.AttackSpeedPerAttack = Math.Max(u.AttackSpeedPerAttack, value); if (level >= 3) u.Equipment.Reduction += 0.12; }); break;
                    case "EDG":
                        Each(u => { u.AttackDamage += u.BaseAttackDamage * value; u.Omnivamp += P("vamp"); u.Coliseum.Edge = true; });
                        On(CombatHook.Attack, (c, e) => { var u = e.Unit!; if (!u.Coliseum.Edge || !e.Target!.Alive || e.Target.Health >= e.Target.MaxHealth * 0.5 || u.Coliseum.EdgeUntil > Time) return;
                            u.Coliseum.EdgeUntil = Time + 1.5; c.Buff(u, CombatStat.AttackSpeed, 0.4, 1.5);
                        }); break;
                    case "EXE": Each(u => { u.CriticalChance += value; u.CriticalDamage += P("cdmg"); u.Equipment.AbilityCrit = true; }); break;
                    case "HEV": Start(c => { foreach (var u in c.Mine) u.MaxHealth += 60; foreach (var u in Members(c)) { u.MaxHealth = UnitValues.Round(u.MaxHealth * (1 + value)); u.AttackDamage += u.MaxHealth * P("k"); } }); break;
                    case "JUG":
                        Each(u => { u.Equipment.Reduction += value; u.Coliseum.Juggernaut = true; });
                        On(CombatHook.BeforeDamageTaken, (_, e) => { if (e.Unit!.Coliseum.Juggernaut && e.Unit.Health > e.Unit.MaxHealth * 0.5) e.Reduction += 0.08; });
                        On(CombatHook.Death, (c, e) => { if (e.Unit!.Coliseum.Juggernaut) foreach (var u in Members(c)) if (u != e.Unit) c.Heal(u, u.MaxHealth * 0.1); }); break;
                    case "PRO":
                        Start(c => { foreach (var u in c.Mine) u.Coliseum.Prodigy = Member(u) ? value : 1; });
                        On(CombatHook.UnitTick, (c, e) => { var u = e.Unit!; if (u.Coliseum.Prodigy != 0 && u.MaxMana > 0 && c.Every(u, "pro", 1)) u.Mana = Math.Min(u.MaxMana, u.Mana + u.Coliseum.Prodigy); });
                        On(CombatHook.DamageDealt, (c, e) => { if (level < 4 || e.Kind != DamageKind.Magic || !Member(e.Unit!)) return;
                            var low = units.Where(a => a.Alive && a.Team == e.Unit!.Team).OrderBy(a => a.Health / a.MaxHealth).FirstOrDefault(); if (low is not null) c.Heal(low, e.Amount * 0.12, true);
                        }); break;
                    case "PRT":
                        Each(u => u.Coliseum.Protector = value);
                        On(CombatHook.BeforeDamageTaken, (_, e) => { if (e.Unit!.Coliseum.Protector != 0 && e.Unit.Shield > 0) e.Reduction += 0.05; });
                        On(CombatHook.DamageTaken, (c, e) => { var u = e.Unit!; var s = u.Coliseum;
                            if (s.Protector == 0 || s.ProtectorUsed || u.Health >= u.MaxHealth * 0.5) return; s.ProtectorUsed = true;
                            double amount = u.MaxHealth * s.Protector; c.Shield(u, amount);
                            var a = units.Where(a => a.Alive && a.Team == u.Team && a != u).OrderBy(a => a.Cell.DistanceTo(u.Cell)).FirstOrDefault(); if (a is not null) c.Shield(a, amount);
                        }); break;
                    case "SNI": Each(u => { u.Equipment.Amp += value; u.PerHexAmplification = P("hex"); }); break;
                    case "SOR":
                        Each(u => u.AbilityPower += value);
                        On(CombatHook.DamageDealt, (c, e) => { if (Member(e.Unit!)) e.Target!.Coliseum.Sorcerer = c.Team!.Value + 1; });
                        On(CombatHook.AnyDeath, (c, e) => {
                            var v = e.Unit!; if (v.Team == c.Team || v.Coliseum.Sorcerer != c.Team + 1 || v.Summoned) return;
                            var src = Members(c).FirstOrDefault(); if (src is null) return; var pool = EnemiesOf(src).ToList();
                            for (int k = 0; k < (level >= 3 ? 2 : 1) && pool.Count > 0; k++) { int index = (int)(c.Random() * pool.Count); var t = pool[index]; pool.RemoveAt(index); c.Effect("beam", src, t); c.Damage(src, t, v.MaxHealth * P("burst")); }
                        }); break;
                    case "STR": Start(c => { foreach (var u in c.Mine) { int k = Member(u) ? 3 : 1; if (u.Entry.Row <= 5) u.Shield += value * k; else u.Equipment.Amp += P("amp") * k; } }); break;
                    default: throw new NotSupportedException($"Coliseum trait {id} has not been migrated.");
                }
            }
        }
    }
}

namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private void ReckoningHurt(CombatUnit u, double amount)
    {
        if (!u.Alive || !(amount > 0)) return;
        u.Health = Math.Max(1, u.Health - amount); u.Taken += amount; Emit("damage", u, amount: UnitValues.Round(amount));
    }

    private CombatUnit? ReckoningVoid(CombatUnit holder, HexCell at, bool unstable)
    {
        HexCell? cell = UnitAt(at) is { } occupant ? FreeNeighbor(occupant, at) : at;
        if (cell is null) return null;
        var spawn = Summon(holder.Team, "rec_voidspawn", Math.Clamp(holder.Stage, 0, 2), cell.Value, null);
        if (spawn is null) return null;
        spawn.Leap = false; spawn.Reckoning.Unstable = unstable;
        foreach (var enemy in EnemiesNear(spawn, spawn.Cell, 2)) enemy.Target = spawn;
        return spawn;
    }

    private void BindReckoningItems(CombatUnit u, double drawback)
    {
        foreach (string id in u.Items.Distinct())
        {
            if (!items.TryGetValue(id, out var item) || item.Hooks.Length == 0) continue;
            bool shadow = id.StartsWith("rec_s_", StringComparison.Ordinal), radiant = id.StartsWith("rec_r_", StringComparison.Ordinal);
            var context = Hooks.Context(CombatHookSource.HeldItem, id, holder: u, count: u.Items.Count(x => x == id));
            void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action) => Hooks.Add(context, hook, action);
            void Start(Action<CombatHookContext> action) => On(CombatHook.CombatStart, (c, _) => action(c));
            void Frail(double fraction) => u.MaxHealth = UnitValues.Round(u.MaxHealth * (1 - fraction * drawback));
            IEnumerable<CombatUnit> Row(int radius, bool self = true) => units.Where(a => a.Alive && a.Team == u.Team && a.Cell.Row == u.Cell.Row && a.Cell.DistanceTo(u.Cell) <= radius && (self || a != u));
            switch (id)
            {
                case "rec_victory_sword": case "rec_s_victory_sword": case "rec_r_victory_sword":
                    On(CombatHook.Kill, (_, _) => u.AttackDamage += u.BaseAttackDamage * (shadow || radiant ? 0.2 : 0.12));
                    if (shadow) Start(_ => Frail(0.15)); break;
                case "rec_s_titan_slayer": On(CombatHook.BeforeDamage, (_, e) => { if (e.Target!.MaxHealth >= 1100) e.Multiplier *= 1.5; }); break;
                case "rec_s_gunblade": On(CombatHook.DamageDealt, (c, e) => {
                    if (e.Kind != DamageKind.Magic || e.IsBurn || !(e.Amount > 0)) return;
                    var ally = units.Where(a => a.Alive && a.Team == u.Team && a != u).OrderBy(a => a.Health / a.MaxHealth).FirstOrDefault();
                    if (ally is not null) c.Heal(ally, e.Amount * 0.5, true);
                }); break;
                case "rec_guardian_feather": case "rec_s_guardian_feather": case "rec_r_guardian_feather":
                    On(CombatHook.Lethal, (c, _) => {
                        if (u.Reckoning.Feather || u.Health > 0) return;
                        u.Reckoning.Feather = true; u.Health = 1; u.InvulnerableUntil = Time + 2; u.Stun = Math.Max(u.Stun, 2); u.BurnSource = null; u.BurnUntil = 0; u.BurnAccumulated = 0; u.WeakUntil = 0;
                        c.After(2, () => {
                            double amount = u.MaxHealth * (radiant ? 1 : shadow ? 0.7 : 0.3);
                            u.Health = Math.Min(u.MaxHealth, Math.Max(u.Health, amount)); Emit("heal", u, amount: UnitValues.Round(amount));
                            if (shadow) u.SpeedBuffs.Add(new(-0.5 * drawback, double.PositiveInfinity));
                        }, u);
                    }); break;
                case "rec_s_blood_fang":
                    On(CombatHook.UnitTick, (_, _) => { if (!u.Reckoning.Risk) { u.Reckoning.Risk = true; u.Health = u.MaxHealth * (1 - 0.2 * drawback); } });
                    On(CombatHook.Heal, (c, _) => { if (u.Health < u.MaxHealth * 0.9 || u.Reckoning.RiskAt > Time) return; u.Reckoning.RiskAt = Time + 4; c.Buff(u, CombatStat.AttackSpeed, 0.4, 4); }); break;
                case "rec_war_banner": case "rec_s_war_banner": case "rec_r_war_banner": Start(_ => {
                    if (shadow) { var mates = Row(1, false).ToArray(); foreach (var a in mates) a.AttackSpeed *= 1 - 0.2 * drawback; u.AttackSpeed *= 1 + 0.35 * mates.Length; }
                    else foreach (var a in Row(1)) { a.AttackSpeed *= 1 + (radiant ? 0.45 : 0.25); if (radiant) a.Omnivamp += 0.1; }
                }); break;
                case "rec_s_crit_edge": case "rec_s_rage_wing": On(CombatHook.Attack, (_, _) => ReckoningHurt(u, u.MaxHealth * (id == "rec_s_crit_edge" ? 0.02 : 0.015) * drawback)); break;
                case "rec_rapid_cannon": case "rec_s_rapid_cannon": case "rec_r_rapid_cannon":
                    Start(_ => u.Range += radiant ? 2 : 1);
                    if (shadow) On(CombatHook.UnitTick, (c, _) => {
                        if (!c.Every(u, "rdc", 0.5)) return;
                        bool near = EnemiesNear(u, u.Cell, 2).Length > 0;
                        if (!near && u.Reckoning.Rapid is null) { u.Reckoning.Rapid = new(0.75, double.PositiveInfinity); u.SpeedBuffs.Add(u.Reckoning.Rapid); }
                        else if (near && u.Reckoning.Rapid is { } speed) { u.SpeedBuffs.RemoveAll(b => ReferenceEquals(b, speed)); u.Reckoning.Rapid = null; }
                    }); break;
                case "rec_s_volt_disk": Start(_ => u.AttackDamage -= u.BaseAttackDamage * 0.3 * drawback);
                    On(CombatHook.Attack, (c, _) => {
                        if (u.Attacks % 2 != 0) return;
                        var pool = units.Where(t => t.Alive && t.Team != u.Team).ToList(); var picked = new List<CombatUnit>();
                        while (pool.Count > 0 && picked.Count < 4) { int index = (int)(c.Random() * pool.Count); picked.Add(pool[index]); pool.RemoveAt(index); }
                        foreach (var target in picked) c.Damage(u, target, 80);
                    }); break;
                case "rec_s_titan_will": On(CombatHook.DamageTaken, (_, e) => u.Reckoning.Store += e.Amount * 0.6);
                    On(CombatHook.Attack, (c, e) => { var s = u.Reckoning; if (!(s.Store > 0) || Time < (s.RevengeAt != 0 ? s.RevengeAt : 2) || !e.Target!.Alive) return; double amount = s.Store; s.Store = 0; s.RevengeAt = Time + 2; c.Damage(u, e.Target, amount, DamageKind.True); }); break;
                case "rec_s_twin_shot": Start(_ => u.AttackDamage -= u.BaseAttackDamage * 0.1 * drawback);
                    On(CombatHook.Attack, (c, e) => {
                        var target = e.Target!;
                        var others = EnemiesNear(u, target.Cell, 2).Where(t => t != target).OrderBy(t => t.Cell.DistanceTo(target.Cell)).ThenBy(t => t.Id).Take(2).ToArray();
                        foreach (var t in others) c.Damage(u, t, e.Amount * 0.7, DamageKind.Physical, e.Critical);
                    }); break;
                case "rec_void_portal": case "rec_s_void_portal": case "rec_r_void_portal":
                    Start(_ => {
                        if (radiant) { var spawn = ReckoningVoid(u, FreeNeighbor(u, u.Cell) ?? u.Cell, false); if (spawn is not null) foreach (var e in EnemiesNear(u, spawn.Cell, 3)) e.Target = spawn; }
                        else { if (shadow) Frail(0.1); foreach (var e in EnemiesNear(u, u.Cell, 2)) e.Target = u; }
                    });
                    On(CombatHook.Death, (_, _) => ReckoningVoid(u, u.Cell, shadow)); break;
                case "rec_s_armor_breaker": Start(_ => { double k = 1 - 0.2 * drawback; u.Armor *= k; u.MagicResist *= k; });
                    On(CombatHook.Attack, (c, e) => {
                        var t = e.Target!; if (!e.Critical || !t.Alive) return;
                        t.Shred = Math.Max(t.ShredUntil > Time ? t.Shred : 0, 0.7); t.ShredUntil = Time + 5;
                        if (t.Reckoning.FractureUntil <= Time && t.MagicResist > 0) { c.Buff(t, CombatStat.MagicResist, -t.MagicResist * 0.7, 5); t.Reckoning.FractureUntil = Time + 5; }
                    }); break;
                case "rec_s_ultima_wand": Start(_ => Frail(0.15)); break;
                case "rec_seraph_staff": case "rec_r_seraph_staff": On(CombatHook.Cast, (_, _) => u.AbilityPower += u.MaxMana * (radiant ? 0.6 : 0.3)); break;
                case "rec_s_seraph_staff": On(CombatHook.Cast, (c, _) => { double add = u.MaxMana * 3.5; u.MaxHealth += add; c.Heal(u, add); }); break;
                case "rec_iron_locket": case "rec_s_iron_locket": case "rec_r_iron_locket": Start(c => {
                    foreach (var a in Row(radiant ? 3 : 2)) {
                        if (shadow) { a.Armor += 60; a.MagicResist += 60; a.Reckoning.Lunar = Math.Max(a.Reckoning.Lunar, 5 * drawback); }
                        else { if (radiant) a.MaxHealth += 200; double amount = radiant ? (u.Stage == 0 ? 450 : u.Stage == 1 ? 600 : 750) : (u.Stage == 0 ? 250 : u.Stage == 1 ? 350 : 450); c.Shield(a, amount); }
                    }
                }); break;
                case "rec_s_mana_zap": On(CombatHook.AnyCast, (c, e) => { var t = e.Unit!; if (!u.Alive || t == u || !t.Alive || u.Cell.DistanceTo(t.Cell) > 2) return; c.Damage(u, t, t.MaxMana * (t.Team != u.Team ? 2 : 0.5 * drawback)); u.AbilityPower += 10; }); break;
                case "rec_s_flame_tome":
                    On(CombatHook.DamageDealt, (_, e) => { var t = e.Target!; if (e.Kind != DamageKind.Magic || e.IsBurn || !t.Alive) return; if (t.Reckoning.CurseSource is null) t.Reckoning.CurseNext = Time + 1; t.Reckoning.CurseUntil = Time + 8; t.Reckoning.CurseSource = u; t.Reckoning.CurseRate = 0.06; t.WoundUntil = Math.Max(t.WoundUntil, Time + 8); });
                    On(CombatHook.BeforeDamage, (_, e) => { if (e.Kind == DamageKind.Magic && !e.IsBurn) e.Multiplier *= 1 - 0.2 * drawback; }); break;
                case "rec_s_jewel_gauntlet": On(CombatHook.Cast, (_, _) => { if (!u.Reckoning.Jewel) { u.Reckoning.Jewel = true; u.CriticalChance += 0.25; } ReckoningHurt(u, u.MaxMana * 0.5 * (u.Stage + 1) * drawback); }); break;
                case "rec_s_mana_overflow": case "rec_r_mana_overflow": On(CombatHook.Cast, (_, _) => u.Mana = radiant ? 30 : u.Health < u.MaxHealth * 0.6 ? 50 : 20); break;
                case "rec_frost_heart": case "rec_s_frost_heart": case "rec_r_frost_heart": On(CombatHook.UnitTick, (c, _) => {
                    if (!c.Every(u, "frost", 0.5)) return;
                    foreach (var t in EnemiesNear(u, u.Cell, 2)) if (t.Reckoning.FrostUntil <= Time + 0.1) { c.Buff(t, CombatStat.AttackSpeed, -(radiant ? 0.5 : shadow ? 0.35 : 0.25), 0.6); t.Reckoning.FrostUntil = Time + 0.6; }
                    if (shadow) foreach (var a in units.Where(a => a.Alive && a.Team == u.Team && a != u && a.Cell.DistanceTo(u.Cell) <= 1)) if (a.Reckoning.AllyFrostUntil <= Time + 0.1) { c.Buff(a, CombatStat.AttackSpeed, -0.25 * drawback, 0.6); a.Reckoning.AllyFrostUntil = Time + 0.6; }
                }); break;
                case "rec_power_chalice": case "rec_s_power_chalice": case "rec_r_power_chalice": Start(_ => {
                    foreach (var a in Row(shadow ? 2 : 1, !shadow)) { a.AbilityPower += radiant ? 50 : shadow ? 45 : 25; if (radiant) a.Omnivamp += 0.1; }
                    if (shadow) u.AbilityPower -= 20 * drawback;
                }); break;
                case "rec_s_holy_ring": On(CombatHook.UnitTick, (c, _) => { if (!c.Every(u, "sring", 5)) return; foreach (var a in units.Where(a => a.Alive && a.Team == u.Team && a != u)) c.Heal(a, (a.MaxHealth - a.Health) * 0.25); ReckoningHurt(u, u.Health * 0.05 * drawback); }); break;
                case "rec_s_justice_hand":
                    Start(_ => { u.Reckoning.Justice = true; u.AttackDamage += u.BaseAttackDamage * 0.3; u.AbilityPower += 30; u.Omnivamp += 0.2; });
                    On(CombatHook.Kill, (_, _) => { int k = u.Reckoning.Justice ? -1 : 1; u.Reckoning.Justice = !u.Reckoning.Justice; u.AttackDamage += k * u.BaseAttackDamage * 0.3; u.AbilityPower += k * 30; u.Omnivamp += k * 0.2; }); break;
                case "rec_s_stone_hide": On(CombatHook.UnitTick, (c, _) => { if (!c.Every(u, "ustone", 2)) return; int n = EnemiesNear(u, u.Cell, 2).Length; if (n > 0) c.Heal(u, u.MaxHealth * 0.02 * n, true); }); break;
                case "rec_s_flame_core": case "rec_r_flame_core": On(CombatHook.UnitTick, (c, _) => {
                    if (shadow && !u.Reckoning.Eclipse) { u.Reckoning.Eclipse = true; if (drawback == 1) c.Burn(u, u); }
                    if (!c.Every(u, radiant ? "sunlight" : "eclipse", 1.5)) return;
                    var pool = EnemiesNear(u, u.Cell, radiant ? 4 : 3); if (pool.Length > 0) c.Burn(u, pool[(int)(c.Random() * pool.Length)]);
                }); break;
                case "rec_still_shroud": case "rec_s_still_shroud": case "rec_r_still_shroud": Start(_ => {
                    foreach (var t in units) {
                        if (!t.Alive || t == u || Math.Abs(t.Cell.Column - u.Cell.Column) > (radiant ? 1 : 0)) continue;
                        double fraction = t.Team != u.Team ? (shadow ? 0.8 : radiant ? 0.5 : 0.35) : t.Cell.Column == u.Cell.Column && shadow ? 0.4 * drawback : 0;
                        if (t.Team == u.Team && radiant) t.Mana += 15;
                        if (fraction == 0 || t.Reckoning.ShroudMax != 0) continue;
                        t.Reckoning.ShroudMax = t.MaxMana; t.MaxMana = UnitValues.Round(t.MaxMana * (1 + fraction));
                    }
                }); break;
                case "rec_s_dragon_scale": On(CombatHook.BeforeDamageTaken, (_, e) => { if (e.Kind == DamageKind.Magic) e.Multiplier *= 0.8; if (e.Critical && e.Source is { CriticalDamage: > 1 } source) e.Multiplier /= source.CriticalDamage; }); break;
                case "rec_wind_ward": case "rec_s_wind_ward": case "rec_r_wind_ward": Start(c => {
                    var at = new HexCell(Columns - 1 - u.Cell.Column, 7 - u.Cell.Row);
                    var enemy = units.Where(t => t.Alive && t.Team != u.Team).OrderBy(t => t.Cell.DistanceTo(at)).FirstOrDefault();
                    if (radiant) foreach (var a in c.Mine) a.AttackSpeed *= 1.1;
                    if (enemy is null) return; double duration = radiant ? 6 : shadow ? 7 : 4; enemy.Stun = Math.Max(enemy.Stun, duration);
                    if (shadow) c.After(duration, () => { enemy.AttackDamage += enemy.BaseAttackDamage * 0.3 * drawback; enemy.AbilityPower += 30 * drawback; }, enemy);
                }); break;
                case "rec_s_quick_silver": Start(_ => Frail(0.12)); break;
                case "rec_s_giga_belt": On(CombatHook.UnitTick, (c, _) => { if (c.Every(u, "sbelt", 1)) ReckoningHurt(u, u.MaxHealth * 0.015 * drawback); }); break;
                case "rec_trap_claw": case "rec_s_trap_claw": case "rec_r_trap_claw": Start(c => { foreach (var a in radiant ? c.Mine : shadow ? Row(1) : [u]) a.Reckoning.SpellShield = true; if (!shadow) u.Reckoning.TrapStun = true; }); break;
                case "rec_s_thief_gloves": Start(_ => Frail(0.1)); break;
                default: throw new NotSupportedException($"Reckoning item hooks for {id} have not been migrated.");
            }
        }
    }
}

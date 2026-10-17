namespace DigiTactics.Rules;

internal sealed class FatesItemState
{
    internal bool Angel, Trap, Manazane, Zhonya;
    internal double Deferred, WinterUntil, CleaveUntil;
    internal int WinterStacks;
    internal double? FrozeAt;
}

internal static class FatesItems
{
    internal static void Bind(CombatSimulation combat, Ruleset rules, CombatUnit unit)
    {
        foreach (string id in unit.Items.Distinct())
        {
            var item = rules.Items.FirstOrDefault(i => i.Id == id);
            if (item is null || item.Hooks.Length == 0) continue;
            var context = combat.Hooks.Context(CombatHookSource.HeldItem, id, holder: unit, count: unit.Items.Count(x => x == id));
            void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action) => combat.Hooks.Add(context, hook, action);
            IEnumerable<CombatUnit> EnemiesNear(CombatUnit center, int radius) => combat.Units.Where(e => e.Alive && e.Team != unit.Team && e.Cell.DistanceTo(center.Cell) <= radius);
            IEnumerable<CombatUnit> Row(int radius) => combat.Units.Where(a => a.Alive && a.Team == unit.Team && a.Cell.Row == unit.Cell.Row && a.Cell.DistanceTo(unit.Cell) <= radius);
            switch (id)
            {
                case "fat_deathblade": On(CombatHook.Kill, (c, e) => { if (e.Unit!.Alive) c.Buff(e.Unit, CombatStat.AttackDamage, e.Unit.BaseAttackDamage * 0.15); }); break;
                case "fat_angel": On(CombatHook.Lethal, (c, e) => {
                    var u = e.Unit!; if (u.FatesItems.Angel || u.Health > 0) return;
                    u.FatesItems.Angel = true; u.Health = 1; u.InvulnerableUntil = c.Time + 2; c.Text(u, "Guardian Wing"); c.After(2, () => c.Heal(u, 400), u);
                }); break;
                case "fat_zeke": On(CombatHook.CombatStart, (c, _) => { foreach (var a in Row(1)) c.Buff(a, CombatStat.AttackSpeed, 0.3); }); break;
                case "fat_rfc": On(CombatHook.CombatStart, (_, _) => unit.Range++); break;
                case "fat_zzrot":
                    On(CombatHook.CombatStart, (_, _) => { foreach (var e in EnemiesNear(unit, 2)) e.Target = unit; });
                    On(CombatHook.Death, (c, e) => {
                        var u = e.Unit!; var summon = c.Summon(u.Team, "fat_construct", u.Stage, u.Cell, new(NoHooks: true));
                        if (summon is not null) { c.Text(summon, "Tankmon"); foreach (var enemy in EnemiesNear(summon, 2)) enemy.Target = summon; }
                    }); break;
                case "fat_luden": On(CombatHook.Cast, (c, e) => {
                    if (e.Target is null) return;
                    var targets = new[] { e.Target }.Concat(EnemiesNear(e.Target, 2).Where(x => x != e.Target)
                        .OrderBy(x => x.Cell.DistanceTo(e.Target.Cell)).ThenBy(x => x.Id).Take(3)).ToArray();
                    foreach (var target in targets) if (target.Alive) { c.Effect("beam", e.Unit!, target); c.Damage(e.Unit!, target, 150); }
                }); break;
                case "fat_locket": On(CombatHook.CombatStart, (c, _) => {
                    double amount = unit.Stage == 0 ? 250 : unit.Stage == 1 ? 325 : 450;
                    foreach (var ally in Row(2)) { c.Shield(ally, amount); c.After(8, () => ally.Shield -= Math.Min(ally.Shield, amount)); }
                }); break;
                case "fat_frozenheart": On(CombatHook.UnitTick, (c, e) => {
                    if (!c.Every(e.Unit!, "fheart", 1)) return;
                    foreach (var enemy in EnemiesNear(e.Unit!, 2)) c.Buff(enemy, CombatStat.AttackSpeed, -0.3, 1.05);
                }); break;
                case "fat_chalice": On(CombatHook.CombatStart, (_, _) => { foreach (var ally in Row(1)) ally.AbilityPower += 30; }); break;
                case "fat_shroud": On(CombatHook.CombatStart, (_, _) => {
                    foreach (var enemy in combat.Units.Where(e => e.Team != unit.Team && e.Cell.Column == unit.Cell.Column && e.MaxMana > 0)) enemy.Mana -= enemy.MaxMana / 3;
                }); break;
                case "fat_dclaw": On(CombatHook.BeforeDamageTaken, (_, e) => { if (e.Kind == DamageKind.Magic) e.Multiplier *= 0.6; }); break;
                case "fat_zephyr": On(CombatHook.CombatStart, (c, _) => {
                    var spot = new HexCell(combat.Columns - 1 - unit.Cell.Column, 7 - unit.Cell.Row);
                    var target = combat.Units.Where(e => e.Alive && e.Team != unit.Team).OrderBy(e => e.Cell.DistanceTo(spot)).FirstOrDefault();
                    if (target is not null) { c.Stun(target, 3); c.Text(target, "Zephyr"); }
                }); break;
                case "fat_trapclaw": On(CombatHook.BeforeDamageTaken, (c, e) => {
                    var u = e.Unit!; if (u.FatesItems.Trap || e.Kind != DamageKind.Magic || e.IsBurn || e.Source is null || e.Source.Team == u.Team) return;
                    u.FatesItems.Trap = true; e.Multiplier = 0; c.Stun(e.Source, 3); c.Text(u, "Trap Claw");
                }); break;
                case "fat_art_defiance":
                    On(CombatHook.BeforeDamageTaken, (_, e) => { e.Unit!.FatesItems.Deferred += e.Amount * e.Multiplier * 0.6; e.Multiplier *= 0.4; });
                    On(CombatHook.UnitTick, (_, e) => {
                        var u = e.Unit!; double deferred = u.FatesItems.Deferred; if (deferred <= 0) return;
                        double take = Math.Min(deferred, deferred * e.DeltaTime / 2.5 + 10 * e.DeltaTime);
                        u.FatesItems.Deferred -= take; u.Health = Math.Max(1, u.Health - take * (1 - Math.Min(0.6, u.Equipment.Reduction)));
                    }); break;
                case "fat_art_winter": On(CombatHook.DamageTaken, (c, e) => {
                    var source = e.Source; if (source is null || !source.Alive || source.Team == e.Unit!.Team || source.FatesItems.WinterUntil > c.Time) return;
                    var s = source.FatesItems; s.WinterUntil = c.Time + 2; c.Buff(source, CombatStat.AttackSpeed, -0.35, 2); s.WinterStacks++;
                    if (s.WinterStacks >= 7 && !(s.FrozeAt > c.Time - 10)) { s.WinterStacks = 0; s.FrozeAt = c.Time; c.Stun(source, 2); c.Text(source, "Frozen"); }
                }); break;
                case "fat_art_collector": On(CombatHook.Attack, (c, e) => {
                    var target = e.Target!; if (!target.Alive || target.Health >= target.MaxHealth * 0.1) return;
                    target.InvulnerableUntil = 0; c.Damage(e.Unit!, target, 1e7, DamageKind.True); if (!target.Alive) { e.Unit!.CombatGold++; c.Text(e.Unit, "+1 gold"); }
                }); break;
                case "fat_art_manazane": On(CombatHook.Cast, (c, e) => {
                    var u = e.Unit!; if (u.FatesItems.Manazane) return; u.FatesItems.Manazane = true;
                    for (int k = 1; k <= 4; k++) c.After(k, () => u.Mana = Math.Min(u.MaxMana, u.Mana + 25), u);
                }); break;
                case "fat_art_cleaver": On(CombatHook.DamageDealt, (c, e) => {
                    var target = e.Target!; if (e.Kind != DamageKind.Magic || !target.Alive || target.FatesItems.CleaveUntil > c.Time) return;
                    target.FatesItems.CleaveUntil = c.Time + 3; c.Buff(target, CombatStat.MagicResist, -20, 3);
                }); break;
                case "fat_art_randuin": On(CombatHook.CombatStart, (c, _) => { foreach (var ally in c.Adjacent(unit).Where(a => a.Team == unit.Team)) { ally.Armor += 40; ally.MagicResist += 40; } }); break;
                case "fat_art_fist": On(CombatHook.CombatStart, (c, _) => {
                    var target = combat.Units.Where(e => e.Alive && e.Team != unit.Team).OrderByDescending(e => e.Cell.DistanceTo(unit.Cell)).FirstOrDefault();
                    if (target is not null) { c.Effect("beam", unit, target); c.Stun(target, 2); c.Text(target, "Rocket Fist"); }
                }); break;
                case "fat_art_zhonya": On(CombatHook.BeforeDamageTaken, (c, e) => {
                    var u = e.Unit!; if (u.FatesItems.Zhonya || u.Health + u.Shield - e.Amount * e.Multiplier >= u.MaxHealth * 0.4) return;
                    u.FatesItems.Zhonya = true; e.Multiplier = 0; u.InvulnerableUntil = c.Time + 2.5; u.Stun = Math.Max(u.Stun, 2.5); c.Text(u, "Stasis");
                }); break;
                default: throw new NotSupportedException($"Fates item hooks for {id} have not been migrated.");
            }
        }
    }
}

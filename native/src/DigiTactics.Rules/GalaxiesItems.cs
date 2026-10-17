namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private static bool GalaxySpell(CombatUnit u) => (u.CastLock >= 0.34 && u.Mana < 1) || u.Galaxies.Casting;

    private void BindGalaxiesItems(CombatUnit unit)
    {
        foreach (string id in unit.Items.Distinct())
        {
            if (!items.TryGetValue(id, out var item) || item.Hooks.Length == 0) continue;
            var context = Hooks.Context(CombatHookSource.HeldItem, id, holder: unit, count: unit.Items.Count(x => x == id));
            void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action, int order = 0) => Hooks.Add(context, hook, action, order);
            IEnumerable<CombatUnit> Row(int radius) => units.Where(a => a.Team == unit.Team && a.Cell.Row == unit.Cell.Row && a.Cell.DistanceTo(unit.Cell) <= radius);
            switch (id)
            {
                case "gal_reaper": On(CombatHook.Kill, (c, _) => unit.AttackDamage += unit.BaseAttackDamage * 0.2 * c.Count); break;
                case "gal_rebirth": On(CombatHook.Lethal, (c, _) => {
                    if (unit.Galaxies.Rebirth) return;
                    unit.Galaxies.Rebirth = true; unit.Health = 1; unit.InvulnerableUntil = Time + 1.5;
                    c.After(1.5, () => c.Heal(unit, unit.MaxHealth * 0.4), unit);
                }); break;
                case "gal_banner": On(CombatHook.CombatStart, (_, _) => { foreach (var a in Row(1)) a.AttackSpeed *= 1.3; }); break;
                case "gal_longshot": On(CombatHook.CombatStart, (_, _) => unit.Range += 2); break;
                case "gal_portal": On(CombatHook.Death, (c, _) => {
                    if (!unit.Summoned) c.Summon(unit.Team, "gal_construct", unit.Stage, unit.Cell, new(HealthMultiplier: 1.25));
                }); break;
                case "gal_ultima": On(CombatHook.CombatReady, (_, _) => unit.AbilityPower *= 1.3, 3); break;
                case "gal_echo": On(CombatHook.Cast, (c, e) => {
                    if (e.Target is not { Alive: true } target) return;
                    var targets = new[] { target }.Concat(EnemiesNear(unit, target.Cell, 1).Where(x => x != target).OrderBy(x => x.Id).Take(3)).ToArray();
                    foreach (var x in targets) c.Damage(unit, x, 160);
                }); break;
                case "gal_locket": On(CombatHook.CombatStart, (_, _) => { foreach (var a in Row(2)) a.Shield += unit.Stage == 0 ? 260 : unit.Stage == 1 ? 320 : 400; }); break;
                case "gal_tome": On(CombatHook.CombatStart, (_, _) => unit.AbilityBurn = true); break;
                case "gal_frostheart": On(CombatHook.UnitTick, (c, _) => {
                    if (!c.Every(unit, "galFrost", 1)) return;
                    foreach (var x in EnemiesNear(unit, unit.Cell, 2)) c.Buff(x, CombatStat.AttackSpeed, -0.3, 1.05);
                }); break;
                case "gal_chalice": On(CombatHook.CombatStart, (_, _) => { foreach (var a in Row(1)) a.AbilityPower += 30; }); break;
                case "gal_spikeplate": On(CombatHook.BeforeDamageTaken, (_, e) => {
                    if (e.Critical && e.Source is { } source) e.Multiplier /= source.CriticalDamage != 0 ? source.CriticalDamage : 1.6;
                }); break;
                case "gal_swordbreaker": On(CombatHook.Attack, (c, e) => {
                    if (e.Target!.Alive && c.Random() < 0.3) e.Target.Galaxies.DisarmUntil = Time + 3;
                }); break;
                case "gal_shroud": On(CombatHook.CombatStart, (_, _) => {
                    foreach (var x in units.Where(x => x.Team != unit.Team && Math.Abs(x.Cell.Column - unit.Cell.Column) <= 1)) GalaxyTax(x, 0.4);
                }); break;
                case "gal_dragonscale": On(CombatHook.BeforeDamageTaken, (_, e) => { if (e.Kind == DamageKind.Magic) e.Multiplier *= 0.6; }); break;
                case "gal_zephyr": On(CombatHook.CombatStart, (_, _) => {
                    var spot = new HexCell(Columns - 1 - unit.Cell.Column, 7 - unit.Cell.Row);
                    var target = GalaxyEnemies(unit).OrderBy(x => x.Cell.DistanceTo(spot)).ThenBy(x => x.Id).FirstOrDefault();
                    if (target is null || target.Equipment.CrowdControlImmunity != 0) return;
                    target.Stun = Math.Max(target.Stun, 4); target.InvulnerableUntil = Math.Max(target.InvulnerableUntil, 4);
                }); break;
                case "gal_trapclaw": On(CombatHook.BeforeDamageTaken, (c, e) => {
                    if (unit.Galaxies.Trapped || e.Kind != DamageKind.Magic || e.IsBurn || e.Source is not { } source || source.Team == unit.Team || !GalaxySpell(source)) return;
                    unit.Galaxies.Trapped = true; e.Multiplier = 0; c.Stun(source, 3);
                }); break;
                default: throw new NotSupportedException($"Galaxies item hooks for {id} have not been migrated.");
            }
        }
    }
}

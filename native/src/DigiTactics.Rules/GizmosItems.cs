namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private void BindGizmosItems(CombatUnit unit)
    {
        foreach (string id in unit.Items.Distinct())
        {
            if (!items.TryGetValue(id, out var item) || item.Hooks.Length == 0) continue;
            double scale = id.StartsWith("giz_oc_", StringComparison.Ordinal) ? 1.6 : 1;
            string kind = id.Replace("giz_oc_", "giz_", StringComparison.Ordinal);
            var context = Hooks.Context(CombatHookSource.HeldItem, id, holder: unit, count: unit.Items.Count(x => x == id));
            void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action) => Hooks.Add(context, hook, action);
            IEnumerable<CombatUnit> Row(int radius) => units.Where(a => a.Alive && a.Team == unit.Team && a.Cell.Row == unit.Cell.Row && a.Cell.DistanceTo(unit.Cell) <= radius);
            switch (kind)
            {
                case "giz_gun_blade": On(CombatHook.DamageDealt, (c, e) => {
                    if (e.Kind != DamageKind.Magic || !(e.Amount > 0)) return;
                    var low = units.Where(a => a.Alive && a.Team == unit.Team).OrderBy(a => a.Health / a.MaxHealth).FirstOrDefault();
                    if (low is not null) c.Heal(low, e.Amount * 0.25 * scale, true);
                }); break;
                case "giz_backup_drive": On(CombatHook.Lethal, (c, _) => {
                    if (unit.Gizmos.Backup) return;
                    unit.Gizmos.Backup = true; unit.Health = 1; unit.InvulnerableUntil = Time + 2; unit.Stun = Math.Max(unit.Stun, 2);
                    c.After(2, () => c.Heal(unit, 400 * scale), unit);
                }); break;
                case "giz_war_banner": On(CombatHook.CombatStart, (_, _) => { foreach (var a in Row(1)) a.AttackSpeed *= 1 + 0.3 * scale; }); break;
                case "giz_rail_cannon": On(CombatHook.CombatStart, (_, _) => unit.Range++); break;
                case "giz_decoy_portal":
                    On(CombatHook.CombatReady, (_, _) => { foreach (var e in EnemiesNear(unit, unit.Cell, 4)) e.Target = unit; });
                    On(CombatHook.Death, (c, _) => {
                        var decoy = c.Summon(unit.Team, "giz_decoy", Math.Min(2, unit.Stage), unit.Cell, new(HealthMultiplier: scale, NoHooks: true));
                        if (decoy is not null) foreach (var e in EnemiesNear(unit, decoy.Cell, 4)) e.Target = decoy;
                    }); break;
                case "giz_guard_locket": On(CombatHook.CombatStart, (c, _) => {
                    double amount = (unit.Stage == 0 ? 300 : unit.Stage == 1 ? 350 : 400) * scale;
                    foreach (var a in Row(2)) c.Shield(a, amount);
                }); break;
                case "giz_frost_heart": On(CombatHook.UnitTick, (c, _) => {
                    if (!c.Every(unit, "frostheart", 1)) return;
                    foreach (var e in EnemiesNear(unit, unit.Cell, 2)) {
                        if (e.Gizmos.FrostUntil > Time) continue;
                        e.Gizmos.FrostUntil = Time + 1.05; c.Buff(e, CombatStat.AttackSpeed, -0.35 * scale, 1.05);
                    }
                }); break;
                case "giz_power_chalice": On(CombatHook.CombatStart, (_, _) => { foreach (var a in Row(1)) a.AbilityPower += UnitValues.Round(30 * scale); }); break;
                case "giz_static_shroud":
                    On(CombatHook.CombatReady, (_, _) => {
                        foreach (var e in units) {
                            if (!e.Alive || e.Team == unit.Team || Math.Abs(e.Cell.Column - unit.Cell.Column) > 1 || e.Gizmos.Shroud != 0 || !(e.MaxMana > 0)) continue;
                            e.Gizmos.Shroud = e.MaxMana; e.Mana = 0; e.MaxMana = UnitValues.Round(e.MaxMana * (1 + 0.35 * scale));
                        }
                    });
                    On(CombatHook.AnyCast, (_, e) => { var u = e.Unit!; if (u.Gizmos.Shroud != 0) { u.MaxMana = u.Gizmos.Shroud; u.Gizmos.Shroud = 0; } }); break;
                case "giz_wind_turbine": On(CombatHook.CombatReady, (_, _) => {
                    var spot = new HexCell(Columns - 1 - unit.Cell.Column, 7 - unit.Cell.Row);
                    var foe = units.Where(e => e.Alive && e.Team != unit.Team).OrderBy(e => e.Cell.DistanceTo(spot) * 100 + e.Id).FirstOrDefault();
                    if (foe is null) return;
                    double duration = 5 * (scale > 1 ? 1.4 : 1);
                    foe.InvulnerableUntil = Math.Max(foe.InvulnerableUntil, Time + duration); foe.Stun = Math.Max(foe.Stun, duration);
                }); break;
                case "giz_spell_shield": On(CombatHook.CombatStart, (_, _) => { foreach (var a in Row(1)) a.Gizmos.Ward = Math.Max(a.Gizmos.Ward, 500 * scale); }); break;
                default: throw new NotSupportedException($"Gizmos item hooks for {id} have not been migrated.");
            }
        }
    }
}

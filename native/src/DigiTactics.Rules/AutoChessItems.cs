namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private void BindAutoChessItems(CombatUnit u)
    {
        foreach (string id in u.Items.Distinct()) {
            if (!items.TryGetValue(id, out var item) || item.Hooks.Length == 0) continue;
            var context = Hooks.Context(CombatHookSource.HeldItem, id, holder: u, count: u.Items.Count(x => x == id));
            void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action) => Hooks.Add(context, hook, action);
            void Start(Action<CombatHookContext> action) => On(CombatHook.CombatStart, (c, _) => action(c));
            CombatUnit[] Allies(int radius) => units.Where(a => a.Alive && a.Team == u.Team && a.Cell.DistanceTo(u.Cell) <= radius).ToArray();
            switch (id) {
                case "ac_digizoid_armor": Start(_ => {
                    foreach (var a in Allies(2)) { a.Armor += 20; a.AttackSpeed *= 1.15; }
                    foreach (var e in EnemiesNear(u, u.Cell, 3)) e.Armor -= 20;
                }); break;
                case "ac_reflect_mail": On(CombatHook.DamageTaken, (c, e) => {
                    var source = e.Source;
                    if (e.Kind != DamageKind.Physical || e.IsBurn || source is null || !source.Alive || source == u || source.Team == u.Team) return;
                    c.Damage(u, source, e.Amount * 0.3);
                }); break;
                case "ac_refresh_disk": On(CombatHook.Cast, (_, _) => { if (u.AutoChess.Refreshed) return; u.AutoChess.Refreshed = true; u.Mana = u.MaxMana; }); break;
                case "ac_numemon_wand": On(CombatHook.UnitTick, (c, _) => {
                    if (!c.Every(u, "acWand", 7)) return;
                    var target = EnemiesOf(u).OrderByDescending(x => x.Line.Tier * 10 + x.Stage * 4).FirstOrDefault(); if (target is not null) c.Stun(target, 2.5);
                }); break;
                case "ac_true_lance": On(CombatHook.Attack, (c, e) => { if (e.Target!.Alive) c.Damage(u, e.Target, 60); }); break;
                case "ac_hellfire_rod": On(CombatHook.UnitTick, (c, _) => { if (c.Every(u, "acRod", 6) && RandomEnemyTarget(u) is { } target) c.Damage(u, target, 250); }); break;
                case "ac_stun_hammer": On(CombatHook.Attack, (c, e) => { if (e.Target!.Alive && c.Random() < 0.15) c.Stun(e.Target, 1.2); }); break;
                case "ac_frost_ring": On(CombatHook.Attack, (c, e) => { if (e.Target!.Alive) c.Buff(e.Target, CombatStat.AttackSpeed, -0.25, 2); }); break;
                case "ac_dragon_lance": Start(_ => { if (u.Range > 1) u.Range++; }); break;
                case "ac_ward_cape": Start(_ => { foreach (var a in Allies(2)) a.MagicResist += 25; }); break;
                default: throw new NotSupportedException($"Auto Chess held item {id} has not been migrated.");
            }
        }
    }
}

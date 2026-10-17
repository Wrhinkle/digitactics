namespace DigiTactics.Rules;

internal sealed class ColiseumItemState
{
    internal bool Cloak, Vow, Doom, Lich, Zhonya;
    internal double VoidUntil;
    internal List<double> Flail { get; } = [];
}

public sealed partial class CombatSimulation
{
    private void BindColiseumItems(CombatUnit u, int roundStage)
    {
        var state = u.Coliseum.Items;
        foreach (string id in u.Items.Distinct())
        {
            if (!items.TryGetValue(id, out var item) || item.Hooks.Length == 0) continue;
            var context = Hooks.Context(CombatHookSource.HeldItem, id, holder: u, count: u.Items.Count(x => x == id));
            void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action) => Hooks.Add(context, hook, action);
            void Start(Action<CombatHookContext> action) => On(CombatHook.CombatStart, (c, _) => action(c));
            switch (id)
            {
                case "col_data_gunblade": On(CombatHook.DamageDealt, (c, e) => {
                    if (e.Kind == DamageKind.True) return;
                    var ally = units.Where(a => a.Alive && a.Team == u.Team).OrderBy(a => a.Health / a.MaxHealth).FirstOrDefault();
                    if (ally is not null) c.Heal(ally, e.Amount * 0.15, true);
                }); break;
                case "col_night_cloak": On(CombatHook.DamageTaken, (_, _) => {
                    if (state.Cloak || u.Health >= u.MaxHealth * 0.6) return;
                    state.Cloak = true; u.InvulnerableUntil = Time + 1; u.Stun = 0; Emit("text", u, text: "Vanish");
                }); break;
                case "col_void_staff": On(CombatHook.DamageDealt, (c, e) => {
                    var t = e.Target!; if (!t.Alive || t.Coliseum.Items.VoidUntil > Time) return;
                    t.Coliseum.Items.VoidUntil = Time + 5; c.Buff(t, CombatStat.MagicResist, -Math.Max(0, t.MagicResist) * 0.3, 5);
                }); break;
                case "col_guardian_vow": On(CombatHook.DamageTaken, (_, _) => {
                    if (state.Vow || u.Health >= u.MaxHealth * 0.4) return;
                    state.Vow = true; u.Mana = Math.Min(u.MaxMana, u.Mana + 15);
                }); break;
                case "col_adaptive_helm": Start(_ => {
                    if (u.Entry.Row <= 5) { u.Armor += 25; u.MagicResist += 25; }
                    else { u.AttackDamage += u.BaseAttackDamage * 0.15; u.AbilityPower += 15; }
                }); break;
                case "col_steadfast_heart": On(CombatHook.BeforeDamageTaken, (_, e) => { if (u.Health > u.MaxHealth * 0.5) e.Reduction += 0.08; }); break;
                case "col_even_shroud": Start(c => { c.Buff(u, CombatStat.Armor, 25, 15); c.Buff(u, CombatStat.MagicResist, 25, 15); }); break;
                case "col_warmog_disk": Start(_ => u.MaxHealth = UnitValues.Round(u.MaxHealth * 1.1)); break;
                case "col_striker_flail": On(CombatHook.DamageDealt, (c, e) => {
                    if (!e.Critical) return;
                    state.Flail.RemoveAll(x => x <= Time); if (state.Flail.Count >= 4) return;
                    state.Flail.Add(Time + 5); c.Buff(u, CombatStat.Amplification, 0.05, 5);
                }); break;
                case "col_ar_doom": On(CombatHook.UnitTick, (c, _) => {
                    // The first tick bypasses Every; the second tick starts its 13-second timer.
                    if (state.Doom && !c.Every(u, "doom", 13)) return;
                    state.Doom = true; var t = TargetByDistance(u);
                    if (t is not null) { c.Effect("beam", u, t); c.Damage(u, t, t.MaxHealth * 0.25); }
                }); break;
                case "col_ar_collector": On(CombatHook.DamageDealt, (c, e) => {
                    var t = e.Target!;
                    if (t.Alive && e.Kind != DamageKind.True && t.Health < t.MaxHealth * 0.12) c.Damage(u, t, (t.Health + t.Shield) * 3 + 10, DamageKind.True);
                }); break;
                case "col_ar_lichbane":
                    On(CombatHook.Cast, (_, _) => state.Lich = true);
                    On(CombatHook.Attack, (c, e) => {
                        if (!state.Lich || !e.Target!.Alive) return;
                        state.Lich = false; c.Damage(u, e.Target, 100 + 60 * (roundStage == 0 ? 1 : roundStage));
                    }); break;
                case "col_ar_hull": Start(c => {
                    if (c.Adjacent(u).Any(a => a.Team == u.Team)) return;
                    u.MaxHealth += 600; u.AttackDamage += u.BaseAttackDamage * 0.2; u.AbilityPower += 20;
                }); break;
                case "col_ar_zhonya": On(CombatHook.DamageTaken, (_, _) => {
                    if (state.Zhonya || u.Health >= u.MaxHealth * 0.4) return;
                    state.Zhonya = true; u.InvulnerableUntil = Time + 2.5; Emit("text", u, text: "Stasis");
                }); break;
                default: throw new NotSupportedException($"Coliseum item {id} has not been migrated.");
            }
        }
    }
}

namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private void BindDragonlandsItems(CombatUnit u, double bank)
    {
        foreach (string id in u.Items.Distinct())
        {
            if (!items.TryGetValue(id, out var item) || item.Hooks.Length == 0) continue;
            var context = Hooks.Context(CombatHookSource.HeldItem, id, holder: u, count: u.Items.Count(x => x == id));
            void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action) => Hooks.Add(context, hook, action);
            void Start(Action<CombatHookContext> action) => On(CombatHook.CombatStart, (c, _) => action(c));
            IEnumerable<CombatUnit> Row(int width) => units.Where(a => a.Team == u.Team && !a.Summoned && a.Cell.Row == u.Cell.Row && Math.Abs(a.Cell.Column - u.Cell.Column) <= width);
            switch (id)
            {
                case "drg_night_veil": On(CombatHook.DamageTaken, (c, _) => {
                    if (u.Dragonlands.Veil || u.Health >= u.MaxHealth * 0.6) return;
                    u.Dragonlands.Veil = true; u.InvulnerableUntil = Time + 1; u.Stun = 0;
                    u.BurnSource = null; u.BurnUntil = 0; u.BurnAccumulated = 0; c.Buff(u, CombatStat.AttackSpeed, 0.4); c.Text(u, "Night Veil");
                }); break;
                case "drg_herald_banner": Start(_ => { foreach (var a in Row(1)) a.AttackSpeed *= 1.3; }); break;
                case "drg_chalice": Start(_ => { foreach (var a in Row(1)) a.AbilityPower += 30; }); break;
                case "drg_solar_locket": Start(_ => { foreach (var a in Row(2)) a.Shield += u.Stage == 0 ? 250 : u.Stage == 1 ? 350 : 450; }); break;
                case "drg_banshee": Start(_ => { foreach (var a in Row(1)) a.Shield += u.Stage == 0 ? 200 : u.Stage == 1 ? 300 : 450; }); break;
                case "drg_rapid_cannon": Start(_ => u.Range++); break;
                case "drg_justice_hand": Start(c => { if (c.Random() < 0.5) u.Equipment.Amp += 0.12; else u.Omnivamp += 0.12; }); break;
                case "drg_lure_portal":
                    Start(_ => { foreach (var e in EnemiesNear(u, u.Cell, 4)) e.Target = u; });
                    On(CombatHook.Death, (c, _) => {
                        var v = c.Summon(u.Team, "drg_voidling", u.Stage, u.Cell);
                        if (v is null) return; v.Target = null;
                        foreach (var e in EnemiesNear(v, v.Cell, 2)) e.Target = v;
                    }); break;
                case "drg_frost_heart": On(CombatHook.UnitTick, (c, _) => { if (c.Every(u, "frostHeart", 1)) foreach (var e in EnemiesNear(u, u.Cell, 2)) c.Buff(e, CombatStat.AttackSpeed, -0.25, 1); }); break;
                case "drg_shroud":
                    Start(_ => { foreach (var e in units) {
                        if (e.Team == u.Team || !e.Alive || Math.Abs(e.Cell.Column - u.Cell.Column) > 1 || e.Dragonlands.Shroud != 0) continue;
                        e.Dragonlands.Shroud = e.MaxMana; e.MaxMana = UnitValues.Round(e.MaxMana * 1.35);
                    } });
                    On(CombatHook.AnyCast, (_, e) => { var caster = e.Unit!; if (caster.Dragonlands.Shroud != 0) { caster.MaxMana = caster.Dragonlands.Shroud; caster.Dragonlands.Shroud = 0; } }); break;
                case "drg_dragon_claw": On(CombatHook.UnitTick, (c, _) => {
                    if (!c.Every(u, "dClaw", 2)) return;
                    int n = units.Count(e => e.Alive && e.Team != u.Team && e.Target == u);
                    if (n > 0) c.Heal(u, u.MaxHealth * 0.012 * n * (u.Line.Slots == 2 ? 1.2 : 1), true);
                }); break;
                case "drg_zephyr": Start(c => {
                    var spot = new HexCell(Columns - 1 - u.Cell.Column, 7 - u.Cell.Row);
                    var enemy = units.Where(e => e.Alive && e.Team != u.Team).OrderBy(e => e.Cell.DistanceTo(spot)).FirstOrDefault();
                    if (enemy is not null) { c.Stun(enemy, 4); c.Text(enemy, "Blown away"); }
                }); break;
                case "drg_g_axe": Start(_ => u.AttackDamage += u.BaseAttackDamage * 0.03 * bank); On(CombatHook.Attack, (_, _) => dragonAxe[u.Team]++); break;
                case "drg_g_blade":
                    int bladeGold = 0; Start(_ => u.AttackSpeed *= 1 + 0.03 * bank);
                    On(CombatHook.Attack, (c, _) => { if (bladeGold >= 3 || c.Random() >= 0.1) return; bladeGold++; dragonGold[u.Team]++; c.Text(u, "+1 gold"); }); break;
                case "drg_g_staff":
                    int staffGold = 0; Start(_ => u.AbilityPower += 4 * bank);
                    On(CombatHook.Kill, (c, _) => { if (staffGold >= 6 || c.Random() >= 0.8) return; staffGold += 2; dragonGold[u.Team] += 2; c.Text(u, "+2 gold"); }); break;
                case "drg_g_mail":
                    int stacks = 0;
                    On(CombatHook.DamageTaken, (_, _) => { if (stacks >= 40) return; stacks++; u.Armor++; u.MagicResist++; u.MaxHealth += 8; u.Health += 8; if (stacks == 40) { dragonGold[u.Team] += 2; Emit("text", u, text: "+2 gold"); } }); break;
                case "drg_g_gem":
                    bool done = false;
                    On(CombatHook.Tick, (_, _) => {
                        if (done || Time < 15 || !u.Alive) return; done = true;
                        var alive = units.Where(a => a.Alive && a.Team == u.Team && !a.Summoned).ToArray();
                        foreach (var a in alive) a.Equipment.Amp += 0.15 + 0.02 * bank;
                        int gold = alive.Length / 2; if (gold > 0) { dragonGold[u.Team] += gold; Emit("text", u, text: $"+{gold} gold"); }
                    }); break;
                case "drg_g_investor": On(CombatHook.Death, (_, _) => dragonInvestor[u.Team]++); break;
                case "drg_g_diamond":
                    bool high = false, low = false;
                    On(CombatHook.DamageTaken, (_, _) => {
                        if (!high && u.Health < u.MaxHealth * 0.66) { high = true; u.InvulnerableUntil = Time + 1; dragonGold[u.Team]++; Emit("text", u, text: "+1 gold"); }
                        if (!low && u.Health < u.MaxHealth * 0.33) { low = true; u.InvulnerableUntil = Time + 1; dragonGold[u.Team]++; Emit("text", u, text: "+1 gold"); }
                    }); break;
                case "drg_g_crown": double next = 0;
                    On(CombatHook.Attack, (c, e) => { if (Time < next || !e.Target!.Alive) return; next = Time + 4; if (bank > 0) c.Damage(u, e.Target, 20 * bank, DamageKind.True); }); break;
                default: throw new NotSupportedException($"Dragonlands item hooks for {id} have not been migrated.");
            }
        }
    }
}

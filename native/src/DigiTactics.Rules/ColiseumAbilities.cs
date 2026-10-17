namespace DigiTactics.Rules;

internal sealed class ColiseumUnitState
{
    internal double Academy, StarAp, StarHit, StarMana, StarHeal, Luchador, EdgeUntil, Prodigy, Protector;
    internal bool Supreme, StarHealed, LuchadorUsed, Edge, Juggernaut, ProtectorUsed;
    internal int Sorcerer;
    internal double Bulk, Adaptive, Atomic, ThornCooldown, ChillUntil;
    internal bool AllOut, AllOutUsed, Burst, Spirit, Overdrive, Ascended;
    internal int Biomerge;
    internal CombatUnit? LastTarget;
    internal ColiseumInfection? Infection;
}

internal sealed class ColiseumInfection(CombatUnit source, CombatUnit victim, double damage)
{
    internal readonly CombatUnit Source = source;
    internal CombatUnit Victim = victim;
    internal readonly double Damage = damage;
    internal int Left = 6;
}

public sealed partial class CombatSimulation
{
    private static readonly HashSet<string> ColiseumAbilities = ["col_frenzy", "col_bulk", "col_aegis", "col_infect", "col_stance", "col_monster"];
    private bool CastColiseumAbility(CombatUnit u, CombatUnit target, double value)
    {
        switch (u.Line.Ability.Kind)
        {
            case "col_frenzy":
                double f = value / 200;
                Buff(u, CombatStat.AttackDamage, u.BaseAttackDamage * f, 6); Buff(u, CombatStat.AttackSpeed, 0.4, 6);
                if (u.Line.Tier >= 5) Buff(u, CombatStat.Omnivamp, 0.15, 6);
                foreach (var a in units.Where(a => a.Alive && a.Team == u.Team && a != u).OrderBy(a => a.Cell.DistanceTo(u.Cell) * 1000 + a.Id).Take(2).ToArray())
                    Buff(a, CombatStat.AttackDamage, a.BaseAttackDamage * f / 3, 6);
                if (target.Alive) AbilityDamage(u, target, value * 1.5);
                break;
            case "col_bulk": u.MaxHealth += value; Heal(u, value); u.Coliseum.Bulk = u.MaxHealth * 0.08; break;
            case "col_aegis":
                AddShield(u, value); foreach (var e in EnemiesNear(u, u.Cell, 2)) AbilityDamage(u, e, value * 0.35); break;
            case "col_infect":
                var victim = EnemiesOf(u).Where(e => e.Coliseum.Infection is null).OrderBy(e => e.Cell.DistanceTo(u.Cell) * 10000 + e.Id).FirstOrDefault() ?? target;
                var infection = new ColiseumInfection(u, victim, value / 6); victim.Coliseum.Infection = infection;
                void TickInfection() {
                    if (infection.Left <= 0) return;
                    infection.Left--;
                    if (infection.Victim.Alive) AbilityDamage(infection.Source, infection.Victim, infection.Damage);
                    if (infection.Left > 0) Schedule(1, TickInfection, null);
                }
                Schedule(1, TickInfection, null); break;
            case "col_stance":
                string stance = u.Entry.Coliseum?.Form ?? "blitz";
                if (stance == "blitz") foreach (var e in EnemiesOf(u).OrderBy(e => e.Cell.DistanceTo(u.Cell) * 10000 + e.Id).Take(2).ToArray()) {
                    AbilityDamage(u, e, value * 0.8); Buff(e, CombatStat.Armor, -15, 5);
                }
                else if (stance == "critical") {
                    var t = EnemiesOf(u).Where(e => e.Cell.DistanceTo(u.Cell) <= 3).OrderBy(e => e.Health).ThenBy(e => e.Id).FirstOrDefault() ?? target;
                    AbilityDamage(u, t, value * 1.6); if (t.Alive) StunUnit(t, 1.5);
                    foreach (var e in EnemiesNear(u, t.Cell, 1)) if (e != t) AbilityDamage(u, e, value * 0.5);
                } else {
                    Buff(u, CombatStat.DamageReduction, 0.5, 2); AbilityDamage(u, target, value * 1.3);
                    foreach (var e in EnemiesNear(u, u.Cell, 2)) if (e != target) AbilityDamage(u, e, value * 0.5);
                }
                break;
            case "col_monster":
                string form = u.Entry.Coliseum?.Form ?? "ballista";
                if (form == "ballista") {
                    AddShield(u, value);
                    foreach (var e in EnemiesNear(u, u.Cell, 2)) { e.Target = u; StunUnit(e, 0.75); AbilityDamage(u, e, value * 0.3); }
                } else if (form == "dorulu") {
                    var t = EnemiesOf(u).OrderBy(e => e.Health).FirstOrDefault() ?? target;
                    AbilityDamage(u, t, value * 1.2 * (t.Health < t.MaxHealth * 0.5 ? 2 : 1));
                } else {
                    Buff(u, CombatStat.AttackSpeed, 0.5, 5); var pool = EnemiesOf(u).ToList();
                    for (int k = 0; k < 3 && pool.Count > 0; k++) { int index = (int)(random.Next() * pool.Count); var e = pool[index]; pool.RemoveAt(index); AbilityDamage(u, e, value * 0.5); }
                }
                break;
            default: return false;
        }
        return true;
    }

    private void BindColiseum(CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
    {
        foreach (var side in new[] { setup.SideA, setup.SideB }) {
            if ((side.Augments?.Length ?? 0) > 0 || (side.Parts?.Any(p => (p.Augments?.Length ?? 0) > 0) ?? false)) throw new NotSupportedException("Coliseum augments have not been migrated yet.");
        }
        foreach (var u in units) foreach (var id in u.Items)
            if (items.TryGetValue(id, out var item) && item.Hooks.Length > 0) throw new NotSupportedException($"Coliseum item hooks {id} have not been migrated yet.");
        BindColiseumTraits(sideA, sideB, setup);
        var context = Hooks.Context(CombatHookSource.Mechanic, "digicore");
        Hooks.Add(context, CombatHook.CombatStart, (c, _) => {
            foreach (var u in units.Where(u => !u.Summoned)) {
                if (u.Line.Id == "col_justimon") {
                    string form = u.Entry.Coliseum?.Form ?? "blitz";
                    if (form == "blitz") u.AttackSpeed *= 1.3; else if (form == "critical") u.CriticalChance += 0.2; else u.MaxHealth += 400;
                } else if (u.Line.Id == "col_labramon") {
                    string form = u.Entry.Coliseum?.Form ?? "ballista";
                    if (form == "ballista") { u.MaxHealth = UnitValues.Round(u.MaxHealth * 1.2); u.Armor += 15; u.MagicResist += 15; }
                    else if (form == "dorulu") { u.AttackDamage += u.BaseAttackDamage * 0.3; u.Omnivamp += 0.2; }
                    else { u.Range = 4; u.AttackSpeed *= 1.25; u.MaxHealth = UnitValues.Round(u.MaxHealth * 0.85); }
                } else if (u.Line.Id == "col_piedmon") u.AbilityPower += (u.Team == 0 ? setup.SideA : setup.SideB).Coliseum?.RcAP ?? 0;
                StartDigiCore(c, u);
            }
        });
        Hooks.Add(context, CombatHook.UnitTick, (c, e) => { var u = e.Unit!; TickDigiCore(c, u); if (u.Coliseum.AllOut && c.Every(u, "allout", 1)) c.Damage(u, u, u.MaxHealth * 0.05, DamageKind.True); });
        Hooks.Add(context, CombatHook.Attack, (c, e) => {
            var u = e.Unit!; var t = e.Target!;
            if (u.Coliseum.Bulk != 0 && t.Alive) { double b = u.Coliseum.Bulk; u.Coliseum.Bulk = 0; c.Damage(u, t, b, DamageKind.Physical); }
            if (u.Entry.Coliseum?.PowerUp == "slide") {
                if (u.Coliseum.LastTarget is not null && u.Coliseum.LastTarget != t && t.Alive) c.Damage(u, t, u.AttackDamage * 1.2);
                u.Coliseum.LastTarget = t;
            } else if (u.Entry.Coliseum?.PowerUp == "glacier" && t.Alive && t.Coliseum.ChillUntil <= Time) {
                t.Coliseum.ChillUntil = Time + 2; c.Buff(t, CombatStat.AttackSpeed, -0.3, 2);
            }
        });
        Hooks.Add(context, CombatHook.BeforeDamage, (_, e) => { if (e.Unit!.Entry.Coliseum?.PowerUp == "hunter" && e.Target!.Health < e.Target.MaxHealth * 0.5) e.Multiplier *= 1.3; });
        Hooks.Add(context, CombatHook.DamageTaken, TakenDigiCore);
        Hooks.Add(context, CombatHook.Kill, (c, e) => KillDigiCore(c, e.Unit!));
        Hooks.Add(context, CombatHook.Death, (c, e) => {
            var u = e.Unit!;
            if (u.Entry.Coliseum?.PowerUp == "atomic") {
                double damage = u.MaxHealth * (u.Coliseum.Atomic == 0 ? 0.1 : u.Coliseum.Atomic);
                foreach (var t in EnemiesNear(u, u.Cell, 2)) c.Damage(u, t, damage);
            }
            var inf = u.Coliseum.Infection;
            if (inf is not null && inf.Left > 0 && inf.Victim == u) {
                var next = units.Where(x => x.Alive && x.Team == u.Team && x.Coliseum.Infection is null).OrderBy(x => x.Cell.DistanceTo(u.Cell) * 10000 + x.Id).FirstOrDefault();
                if (next is not null) { inf.Victim = next; next.Coliseum.Infection = inf; }
            }
        });
        Hooks.Add(context, CombatHook.Lethal, (_, e) => {
            var u = e.Unit!; if (u.Line.Id != "col_bakemon" || u.Coliseum.AllOutUsed || u.Summoned) return;
            u.Coliseum.AllOutUsed = u.Coliseum.AllOut = true; u.Health = u.MaxHealth * 0.25;
            Buff(u, CombatStat.AttackSpeed, 0.4, double.PositiveInfinity); u.AttackDamage += (u.Armor + u.MagicResist) * 0.3; u.InvulnerableUntil = Time + 0.5;
        });
    }
}

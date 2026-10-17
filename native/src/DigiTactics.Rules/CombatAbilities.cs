namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    public static IReadOnlySet<string> SupportedAbilities { get; } = new HashSet<string>
        { "nuke", "aoe", "stun", "slam", "shield", "heal", "barrage", "drain", "weaken", "rally", "line", "execute", "chain", "dash" };

    private CombatUnit[] EnemiesNear(CombatUnit unit, HexCell center, int radius) =>
        units.Where(e => e.Alive && e.Team != unit.Team && center.DistanceTo(e.Cell) <= radius).ToArray();

    private void Cast(CombatUnit unit, CombatUnit target)
    {
        unit.Mana = 0; unit.CastLock = 0.35;
        double castPower = unit.AbilityPower;
        double value = UnitValues.AbilityValue(unit.Line, unit.Stage) * unit.AbilityPower / 100;
        int radius = unit.Line.Ability.Radius == 0 ? 1 : unit.Line.Ability.Radius;
        Emit("cast", unit);
        AfterCast(unit);
        switch (unit.Line.Ability.Kind)
        {
            case "nuke": AbilityDamage(unit, target, value); break;
            case "aoe":
                foreach (var enemy in EnemiesNear(unit, target.Cell, radius)) AbilityDamage(unit, enemy, value);
                break;
            case "stun":
                StunUnit(target, 1.5 + unit.Stage * 0.25);
                AbilityDamage(unit, target, value); break;
            case "slam":
                foreach (var enemy in EnemiesNear(unit, unit.Cell, radius))
                { StunUnit(enemy, 1); AbilityDamage(unit, enemy, value); }
                break;
            case "shield":
                AddShield(unit, value); Emit("shield", unit);
                foreach (var enemy in EnemiesNear(unit, unit.Cell, 2)) enemy.Target = unit;
                break;
            case "heal":
                foreach (var ally in units.Where(e => e.Alive && e.Team == unit.Team).OrderBy(e => e.Health / e.MaxHealth).Take(unit.Stage + 1).ToArray())
                    Heal(ally, value);
                break;
            case "barrage":
                for (int i = 0; i < unit.Line.Ability.Shots + unit.Stage; i++)
                {
                    var enemies = units.Where(e => e.Alive && e.Team != unit.Team).ToArray();
                    if (enemies.Length == 0) break;
                    AbilityDamage(unit, enemies[(int)(random.Next() * enemies.Length)], value);
                }
                break;
            case "drain":
                double total = 0;
                var hits = unit.Line.Ability.Radius > 0 ? EnemiesNear(unit, target.Cell, unit.Line.Ability.Radius) : [target];
                foreach (var enemy in hits) total += AbilityDamage(unit, enemy, value);
                Heal(unit, total * 0.6); break;
            case "weaken":
                AbilityDamage(unit, target, value);
                if (target.Alive) { target.Weakness = Math.Max(target.WeakUntil > Time ? target.Weakness : 0, 0.3); target.WeakUntil = Time + 4; }
                break;
            case "rally":
                foreach (var ally in units.Where(e => e.Alive && e.Team == unit.Team).OrderBy(e => e.Health / e.MaxHealth).ThenBy(e => e.Id)
                    .Take(unit.Stage >= 2 ? 3 : 2).ToArray())
                { AddShield(ally, value); ally.SpeedBuffs.Add(new(0.3, Time + 4)); Emit("shield", ally); }
                break;
            case "execute": AbilityDamage(unit, target, target.Health < target.MaxHealth * 0.5 ? value * 2 : value); break;
            case "dash":
                var weakest = units.Where(e => e.Alive && e.Team != unit.Team).OrderBy(e => e.Health).FirstOrDefault();
                if (weakest is null) break;
                if (unit.Cell.DistanceTo(weakest.Cell) > 1 && FreeNeighbor(weakest, unit.Cell) is { } cell)
                { Emit("leap", unit); Place(unit, cell); }
                unit.Target = weakest; AbilityDamage(unit, weakest, value); break;
            case "chain":
                var hit = new HashSet<int>();
                CombatUnit? current = target;
                for (int i = 0; i <= unit.Stage + 2 && current is not null; i++)
                {
                    hit.Add(current.Id); AbilityDamage(unit, current, value); value *= 0.8;
                    var origin = current.Cell;
                    current = units.Where(e => e.Alive && e.Team != unit.Team && !hit.Contains(e.Id) && origin.DistanceTo(e.Cell) <= 3)
                        .OrderBy(e => origin.DistanceTo(e.Cell)).FirstOrDefault();
                }
                break;
            case "line":
                foreach (var enemy in LineTargets(unit, target)) AbilityDamage(unit, enemy, value);
                break;
            default:
                if (!CastSetAbility(unit, target, value)) throw new NotSupportedException(unit.Line.Ability.Kind);
                break;
        }
        if (Hooks.Has(CombatHook.Cast, unit, CombatHook.AnyCast))
            Hooks.Fire(CombatHook.Cast, unit, new() { Target = target, Value = UnitValues.AbilityValue(unit.Line, unit.Stage) * castPower / 100 }, CombatHook.AnyCast);
    }

    private List<CombatUnit> LineTargets(CombatUnit unit, CombatUnit target)
    {
        int aq = unit.Cell.Column - (unit.Cell.Row - (unit.Cell.Row & 1)) / 2, ar = unit.Cell.Row;
        int bq = target.Cell.Column - (target.Cell.Row - (target.Cell.Row & 1)) / 2, br = target.Cell.Row;
        int distance = Math.Max(1, unit.Cell.DistanceTo(target.Cell));
        var result = new List<CombatUnit>();
        for (int k = 1; k <= 8; k++)
        {
            double f = (double)k / distance, q = aq + (bq - aq) * f + 1e-6, r = ar + (br - ar) * f + 2e-6, s = -q - r;
            int rq = (int)UnitValues.Round(q), rr = (int)UnitValues.Round(r), rs = (int)UnitValues.Round(s);
            double dq = Math.Abs(rq - q), dr = Math.Abs(rr - r), ds = Math.Abs(rs - s);
            if (dq > dr && dq > ds) rq = -rr - rs; else if (dr > ds) rr = -rq - rs;
            if (rr < 0 || rr >= grid.Rows) break;
            int column = rq + (rr - (rr & 1)) / 2;
            if (column < 0 || column >= grid.Columns) break;
            var enemy = occupancy[grid.Index(new(column, rr))];
            if (enemy is not null && enemy.Alive && enemy.Team != unit.Team && !result.Contains(enemy)) result.Add(enemy);
        }
        if (!result.Contains(target)) result.Insert(0, target);
        return result;
    }
}

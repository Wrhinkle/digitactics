namespace DigiTactics.Rules;

/// <summary>Fixed-tick combat kernel. Set hooks and equipment are integrated separately during migration.</summary>
public sealed partial class CombatSimulation
{
    public const double Tick = 1.0 / 30;
    private readonly List<CombatUnit> units = [];
    private readonly List<CombatEvent> events = [];
    private readonly HexGrid grid;
    private readonly CombatUnit?[] occupancy;
    private readonly bool[] occupied;
    private readonly DeterministicRandom random;
    private readonly double overtimeStart;
    private readonly double timeLimit;
    public IReadOnlyList<CombatUnit> Units { get; }
    public IReadOnlyList<CombatEvent> Events { get; }
    public double Time { get; private set; }
    public bool Done { get; private set; }
    public int? Winner { get; private set; }
    public int Columns => grid.Columns;

    public CombatSimulation(Ruleset rules, CombatEntry[] sideA, CombatEntry[] sideB, uint seed,
        int columns = 7, double overtimeStart = 30, double timeLimit = 45)
    {
        grid = new(columns, 8); occupancy = new CombatUnit?[grid.Count]; occupied = new bool[grid.Count];
        random = new(seed); this.overtimeStart = overtimeStart; this.timeLimit = timeLimit;
        Units = units.AsReadOnly(); Events = events.AsReadOnly();
        var lines = rules.Lines.ToDictionary(l => l.Id);
        for (int team = 0; team < 2; team++) foreach (var entry in team == 0 ? sideA : sideB)
        {
            var cell = team == 0 ? new HexCell(entry.Column, entry.Row) : new(columns - 1 - entry.Column, 7 - entry.Row);
            int index = grid.Index(cell);
            if (occupancy[index] is not null) continue;
            var line = lines[entry.Line];
            if (!SupportedAbilities.Contains(line.Ability.Kind))
                throw new NotSupportedException($"Ability {line.Ability.Kind} has not been migrated yet.");
            var unit = new CombatUnit(units.Count, entry, line, team, cell, random.Next());
            units.Add(unit); occupancy[index] = unit;
        }
        CheckEnd();
    }

    public void Step()
    {
        if (Done) return;
        Time += Tick;
        foreach (var unit in units) if (unit.Alive) Update(unit);
        CheckEnd();
    }

    private void CheckEnd()
    {
        bool a = units.Any(u => u.Alive && u.Team == 0), b = units.Any(u => u.Alive && u.Team == 1);
        if (!a || !b || Time >= timeLimit) { Done = true; Winner = a && !b ? 0 : b && !a ? 1 : -1; }
    }

    private void Update(CombatUnit unit)
    {
        unit.SpeedBuffs.RemoveAll(b => b.Until <= Time);
        unit.AttackCooldown -= Tick; unit.MoveCooldown -= Tick;
        if (unit.Stun > 0) { unit.Stun -= Tick; return; }
        if (unit.CastLock > 0) { unit.CastLock -= Tick; return; }
        if (unit.Line.Combat.Leap && !unit.Leapt) { unit.Leapt = true; Leap(unit); unit.CastLock = 0.3; return; }
        if (unit.MoveCooldown > 0) return;
        var target = unit.Target;
        if (target is null || !target.Alive || unit.Cell.DistanceTo(target.Cell) > unit.Range)
        {
            target = units.Where(e => e.Alive && e.Team != unit.Team && unit.Cell.DistanceTo(e.Cell) <= unit.Range)
                .OrderBy(e => unit.Cell.DistanceTo(e.Cell)).ThenBy(e => e.Health).FirstOrDefault();
            unit.Target = target;
        }
        if (target is not null)
        {
            if (unit.MaxMana > 0 && unit.Mana >= unit.MaxMana) { Cast(unit, target); return; }
            if (unit.AttackCooldown <= 0) Attack(unit, target);
        }
        else Move(unit);
    }

    private void Move(CombatUnit unit)
    {
        var enemies = units.Where(e => e.Alive && e.Team != unit.Team).ToArray();
        if (enemies.Length == 0) return;
        for (int i = 0; i < occupancy.Length; i++) occupied[i] = occupancy[i] is not null;
        var step = grid.StepTowardEnemy(unit.Cell, unit.Range, enemies.Select(e => e.Cell).ToArray(), occupied);
        if (step is null) { unit.MoveCooldown = 0.25; return; }
        Place(unit, step.Value.Cell); unit.MoveCooldown = 0.4; unit.Target = enemies[step.Value.TargetIndex];
    }

    private void Place(CombatUnit unit, HexCell cell)
    {
        occupancy[grid.Index(unit.Cell)] = null; unit.Cell = cell; occupancy[grid.Index(cell)] = unit;
    }

    private HexCell? FreeNeighbor(CombatUnit target, HexCell near)
    {
        return Enumerable.Range(0, grid.Count).Where(i => occupancy[i] is null && grid[i].DistanceTo(target.Cell) == 1)
            .OrderBy(i => grid[i].DistanceTo(near)).Select(i => (HexCell?)grid[i]).FirstOrDefault();
    }

    private void Leap(CombatUnit unit)
    {
        foreach (var enemy in units.Where(e => e.Alive && e.Team != unit.Team).OrderBy(e => unit.Team == 0 ? e.Cell.Row : -e.Cell.Row))
        {
            var cell = FreeNeighbor(enemy, new(enemy.Cell.Column, unit.Team == 0 ? 0 : 7));
            if (cell is null) continue;
            Emit("leap", unit); Place(unit, cell.Value); unit.Target = enemy; return;
        }
    }

    private void Attack(CombatUnit unit, CombatUnit target)
    {
        unit.AttackCooldown = 1 / (unit.AttackSpeed * Math.Min(1 + unit.SpeedBuffs.Sum(b => b.Value), 4));
        Emit("attack", unit, target);
        unit.Mana = Math.Min(unit.MaxMana, unit.Mana + 10); unit.Attacks++;
        Damage(unit, target, unit.AttackDamage, DamageKind.Physical);
    }

    private double Damage(CombatUnit source, CombatUnit target, double amount, DamageKind kind)
    {
        if (!target.Alive) return 0;
        var request = new DamageRequest(new(source.Line.Attribute, Weakened: source.WeakUntil > Time, Weakness: source.Weakness),
            new(target.Line.Attribute, target.MaxHealth, target.Armor, target.MagicResist, target.Shield, target.Mana, target.MaxMana),
            kind, amount, source.Cell.DistanceTo(target.Cell), Time > overtimeStart ? 1 + (Time - overtimeStart) * 0.2 : 1);
        var result = DamageRules.Resolve(request);
        target.Shield -= result.ShieldAbsorbed; target.Health -= result.HealthDamage; target.Mana = result.ManaAfter;
        source.Dealt += result.Amount; target.Taken += result.Amount;
        Emit("damage", target, source, UnitValues.Round(result.Amount));
        if (target.Health <= 0)
        {
            target.Alive = false; target.Health = 0; occupancy[grid.Index(target.Cell)] = null; Emit("death", target, source);
        }
        return result.Amount;
    }

    private void Heal(CombatUnit unit, double amount)
    {
        if (!unit.Alive || amount <= 0) return;
        double before = unit.Health; unit.Health = Math.Min(unit.MaxHealth, unit.Health + amount);
        unit.Healed += unit.Health - before;
        if (unit.Health > before) Emit("heal", unit, amount: UnitValues.Round(unit.Health - before));
    }

    private void Emit(string type, CombatUnit unit, CombatUnit? target = null, double amount = 0) =>
        events.Add(new(Time, type, unit.Id, target?.Id, amount));
}

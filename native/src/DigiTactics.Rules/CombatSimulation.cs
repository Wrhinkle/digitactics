namespace DigiTactics.Rules;

/// <summary>Fixed-tick combat kernel. Equipment is compiled per unit; set hooks are integrated separately during migration.</summary>
public sealed partial class CombatSimulation
{
    public const double Tick = 1.0 / 30;
    private readonly List<CombatUnit> units = [];
    private readonly Ruleset rules;
    private readonly Dictionary<string, UnitLine> lines;
    private readonly Dictionary<string, ItemDefinition> items;
    private readonly string[] thiefPool;
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
    public CombatHookRegistry Hooks { get; }
    private bool ended;
    internal Action<CombatUnit>? BindSummonedItems { get; set; }
    internal double[] HealingAmplification { get; } = new double[2];

    public CombatSimulation(Ruleset rules, CombatEntry[] sideA, CombatEntry[] sideB, uint seed,
        int columns = 7, double overtimeStart = 30, double timeLimit = 45, Action<CombatHookRegistry>? configureHooks = null, bool duos = false, CombatSetup? setup = null)
    {
        this.rules = rules;
        grid = new(columns, 8); occupancy = new CombatUnit?[grid.Count]; occupied = new bool[grid.Count];
        random = new(seed); this.overtimeStart = overtimeStart; this.timeLimit = timeLimit;
        Units = units.AsReadOnly(); Events = events.AsReadOnly();
        Hooks = new(this, duos || setup?.SideA.Parts is not null || setup?.SideB.Parts is not null);
        lines = rules.Lines.Concat(rules.ExtraLines).ToDictionary(l => l.Id);
        items = rules.Items.ToDictionary(i => i.Id);
        thiefPool = rules.CoreItems.Where(id => !items[id].Passives.TryGetValue("thief", out var flag) || !flag.GetBoolean()).ToArray();
        for (int team = 0; team < 2; team++) foreach (var entry in team == 0 ? sideA : sideB)
        {
            var cell = team == 0 ? new HexCell(entry.Column, entry.Row) : new(columns - 1 - entry.Column, 7 - entry.Row);
            int index = grid.Index(cell);
            if (occupancy[index] is not null) continue;
            var line = lines[entry.Line];
            if (!SupportsAbility(line.Ability.Kind))
                throw new NotSupportedException($"Ability {line.Ability.Kind} has not been migrated yet.");
            var unit = new CombatUnit(units.Count, entry, line, team, cell, random.Next());
            unit.Traits = TraitRules.UnitTraits(entry, rules);
            unit.Equipment.Apply(unit, items, thiefPool, random);
            units.Add(unit); occupancy[index] = unit;
        }
        if (setup is not null)
        {
            if (rules.Id.StartsWith("fates/", StringComparison.Ordinal)) FatesCombat.Bind(this, rules, sideA, sideB, setup);
            else if (rules.Id.StartsWith("galaxies/", StringComparison.Ordinal)) BindGalaxies(sideA, sideB, setup);
            else if (rules.Id.StartsWith("gizmos/", StringComparison.Ordinal)) BindGizmos(sideA, sideB, setup);
            else if (rules.Id.StartsWith("reckoning/", StringComparison.Ordinal)) BindReckoning(setup);
            else FactionWarsCombat.Bind(this, rules, sideA, sideB, setup);
        }
        configureHooks?.Invoke(Hooks);
        for (int team = 0; team < 2; team++)
        {
            Hooks.FireTeam(CombatHook.CombatStart, team, new());
            var side = team == 0 ? setup?.SideA : setup?.SideB;
            if (side?.Parts is { } parts) foreach (var part in parts) ApplyCombatBuffs(team, part.Buffs, part.Owner);
            else ApplyCombatBuffs(team, side?.Buffs, null);
        }
        Hooks.FireGlobal(CombatHook.CombatStart, new());
        for (int team = 0; team < 2; team++) Hooks.FireTeam(CombatHook.CombatReady, team, new());
        Hooks.FireGlobal(CombatHook.CombatReady, new());
        foreach (var unit in units)
        {
            if (unit.Equipment.Crown) unit.Shield += unit.MaxHealth * 0.25;
            unit.Health = unit.MaxHealth; unit.Mana = Math.Min(unit.MaxMana - 1, unit.Mana);
        }
        CheckEnd();
    }

    public void Step()
    {
        if (Done) return;
        Time += Tick;
        for (int i = 0; i < units.Count; i++) if (units[i].Alive) Update(units[i]);
        for (int team = 0; team < 2; team++) Hooks.FireTeam(CombatHook.Tick, team, new() { DeltaTime = Tick });
        Hooks.FireGlobal(CombatHook.Tick, new() { DeltaTime = Tick });
        RunTimers(timers);
        CheckEnd();
    }

    private void CheckEnd()
    {
        bool a = units.Any(u => u.Alive && u.Team == 0), b = units.Any(u => u.Alive && u.Team == 1);
        if (!a || !b || Time >= timeLimit)
        {
            Done = true; Winner = a && !b ? 0 : b && !a ? 1 : -1;
            if (ended) return;
            ended = true;
            for (int team = 0; team < 2; team++) Hooks.FireTeam(CombatHook.CombatEnd, team, new() { Winner = Winner });
            Hooks.FireGlobal(CombatHook.CombatEnd, new() { Winner = Winner });
        }
    }

    private void Update(CombatUnit unit)
    {
        unit.SpeedBuffs.RemoveAll(b => b.Until <= Time);
        ExpireBuffs(unit);
        if (unit.Equipment.Regen != 0) Heal(unit, unit.MaxHealth * unit.Equipment.Regen * Tick, true);
        Passives(unit);
        if (!unit.Alive) return;
        unit.AttackCooldown -= Tick; unit.MoveCooldown -= Tick;
        if (unit.Stun > 0) { unit.Stun -= Tick; return; }
        if (unit.CastLock > 0) { unit.CastLock -= Tick; return; }
        if (unit.Leap && !unit.Leapt) { unit.Leapt = true; Leap(unit); unit.CastLock = 0.3; return; }
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
            if (unit.MaxMana > 0 && unit.Mana >= unit.MaxMana && unit.SilenceUntil <= Time) { Cast(unit, target); return; }
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
        var p = unit.Equipment;
        // Preserve sequential accumulation: rounding can change the attack tick.
        double speed = 1 + p.Rage * unit.RageStacks + unit.AttackSpeedPerAttack * unit.AttackSpeedStacks;
        foreach (var buff in unit.SpeedBuffs) speed += buff.Value;
        unit.AttackCooldown = 1 / (unit.AttackSpeed * Math.Min(speed, 4));
        Emit("attack", unit, target);
        if (unit.Blind) { unit.Blind = false; Emit("miss", unit); return; }
        unit.Mana = Math.Min(unit.MaxMana, unit.Mana + 10 + p.Shojin); unit.Attacks++;
        if (unit.AttackSpeedPerAttack != 0 && unit.AttackSpeedStacks < unit.AttackSpeedStackLimit) unit.AttackSpeedStacks++;
        if (p.Rage != 0) unit.RageStacks++;
        unit.AttackDamage += unit.BaseAttackDamage * p.Kraken;
        if (p.Titan) TitanStack(unit);
        if (target.Equipment.Titan) TitanStack(target);
        if (target.Equipment.Thorns != 0 && !(target.ThornCooldown > Time))
        {
            target.ThornCooldown = Time + 1.5;
            foreach (var enemy in EnemiesNear(target, target.Cell, 1)) Damage(target, enemy, target.Equipment.Thorns, DamageKind.Magic);
        }
        if (target.Dodge > 0 && random.Next() < target.Dodge) { Emit("dodge", target); return; }
        double amount = unit.AttackDamage;
        bool critical = unit.CriticalChance > 0 && random.Next() < unit.CriticalChance;
        if (critical) amount *= unit.CriticalDamage;
        if (p.Shred != 0) { target.ShredUntil = Time + 3; target.Shred = p.Shred; }
        Damage(unit, target, amount, DamageKind.Physical, critical: critical);
        if (Hooks.Has(CombatHook.Attack, unit, CombatHook.AnyAttack))
            Hooks.Fire(CombatHook.Attack, unit, new() { Target = target, Amount = amount, Critical = critical }, CombatHook.AnyAttack);
        if (p.Split != 0)
        {
            var other = EnemiesNear(unit, target.Cell, 2).Where(e => e != target).OrderBy(e => target.Cell.DistanceTo(e.Cell)).FirstOrDefault();
            if (other is not null) Damage(unit, other, amount * p.Split, DamageKind.Physical, critical: critical);
        }
        if (p.Volt != 0 && unit.Attacks % 3 == 0)
        {
            var pool = units.Where(e => e.Alive && e.Team != unit.Team).ToList();
            for (int k = 0; k < 3 && pool.Count > 0; k++)
            {
                int index = (int)(random.Next() * pool.Count); var enemy = pool[index]; pool.RemoveAt(index);
                Damage(unit, enemy, p.Volt, DamageKind.Magic);
            }
        }
    }

    private double Damage(CombatUnit source, CombatUnit target, double amount, DamageKind kind, bool isBurn = false, bool critical = false)
    {
        if (!target.Alive || target.InvulnerableUntil > Time) return 0;
        var request = new DamageRequest(new(source.Line.Attribute, source.Equipment.Amp, source.WeakUntil > Time, source.Weakness, source.BonusAgainstBurning, source.PerHexAmplification, GiantSlayer: source.Equipment.Slayer, GuardBreaker: source.Equipment.GuardBreaker),
            new(target.Line.Attribute, target.MaxHealth, target.Armor, target.MagicResist, target.Shield, target.Mana, target.MaxMana, target.Equipment.Reduction, target.Gargoyle, target.VeilUntil > Time ? target.Veil : 0, target.ShredUntil > Time ? target.Shred : 0, target.BurnUntil > Time),
            kind, amount, source.Cell.DistanceTo(target.Cell), Time > overtimeStart ? 1 + (Time - overtimeStart) * 0.2 : 1, IsBurn: isBurn);
        double outgoing = DamageRules.OutgoingMultiplier(request);
        if (Hooks.Has(CombatHook.BeforeDamage, source))
        {
            var e = new CombatHookEvent { Target = target, Amount = amount, Kind = kind, Critical = critical, IsBurn = isBurn,
                Multiplier = outgoing };
            Hooks.Fire(CombatHook.BeforeDamage, source, e);
            outgoing = e.Multiplier;
        }
        request = request with { Target = request.Target with { Armor = target.Armor, MagicResist = target.MagicResist,
            Gargoyle = target.Gargoyle, Veil = target.VeilUntil > Time ? target.Veil : 0, Shred = target.ShredUntil > Time ? target.Shred : 0 } };
        double mitigated = DamageRules.Mitigate(request, outgoing);
        if (Hooks.Has(CombatHook.BeforeDamageTaken, target))
        {
            var e = new CombatHookEvent { Source = source, Target = target,
                Amount = mitigated, Kind = kind, Critical = critical, IsBurn = isBurn };
            Hooks.Fire(CombatHook.BeforeDamageTaken, target, e);
            request = request with { ExtraReduction = e.Reduction, IncomingMultiplier = e.Multiplier };
        }
        request = request with { Target = request.Target with { Reduction = target.Equipment.Reduction, Shield = target.Shield,
            Mana = target.Mana, MaxMana = target.MaxMana } };
        var result = DamageRules.ResolveMitigated(request, mitigated);
        target.Shield -= result.ShieldAbsorbed; target.Health -= result.HealthDamage; target.Mana = result.ManaAfter;
        source.Dealt += result.Amount; target.Taken += result.Amount;
        if (source.Omnivamp > 0 && source.Alive) Heal(source, result.Amount * source.Omnivamp, true);
        if (source.Equipment.Burn && !isBurn) ApplyBurn(source, target);
        Emit("damage", target, source, UnitValues.Round(result.Amount));
        if (target.Health <= 0 && Hooks.Has(CombatHook.Lethal, target))
            Hooks.Fire(CombatHook.Lethal, target, new() { Source = source, Amount = result.Amount, Kind = kind });
        if (target.Health <= 0) Kill(target, source);
        else
        {
            CheckLifelines(target);
            if (Hooks.Has(CombatHook.DamageTaken, target))
                Hooks.Fire(CombatHook.DamageTaken, target, new() { Source = source, Amount = result.Amount, Kind = kind, Critical = critical, IsBurn = isBurn });
        }
        if (Hooks.Has(CombatHook.DamageDealt, source, CombatHook.AnyDamageDealt))
            Hooks.Fire(CombatHook.DamageDealt, source, new() { Target = target, Amount = result.Amount, Kind = kind, Critical = critical, IsBurn = isBurn, Killed = !target.Alive }, CombatHook.AnyDamageDealt);
        return result.Amount;
    }

    private void Heal(CombatUnit unit, double amount, bool quiet = false)
    {
        if (!unit.Alive || amount <= 0) return;
        if (unit.WoundUntil > Time) amount *= 0.67;
        amount *= 1 + HealingAmplification[unit.Team];
        double before = unit.Health; unit.Health = Math.Min(unit.MaxHealth, unit.Health + amount);
        unit.Healed += unit.Health - before;
        if (!quiet && unit.Health > before) Emit("heal", unit, amount: UnitValues.Round(unit.Health - before));
        if (unit.Health > before && Hooks.Has(CombatHook.Heal, unit, CombatHook.AnyHeal))
            Hooks.Fire(CombatHook.Heal, unit, new() { Amount = unit.Health - before }, CombatHook.AnyHeal);
    }

    private void Emit(string type, CombatUnit unit, CombatUnit? target = null, double amount = 0) =>
        events.Add(new(Time, type, unit.Id, target?.Id, amount));
}

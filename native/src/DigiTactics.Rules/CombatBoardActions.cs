namespace DigiTactics.Rules;

public sealed record SummonOptions(string[]? Items = null, double HealthMultiplier = 1, double AttackMultiplier = 1, int? Owner = null);

public sealed partial class CombatSimulation
{
    private bool InBounds(HexCell cell) => cell.Column >= 0 && cell.Column < grid.Columns && cell.Row >= 0 && cell.Row < grid.Rows;
    internal CombatUnit? UnitAt(HexCell cell) => InBounds(cell) ? occupancy[grid.Index(cell)] : null;
    internal HexCell? FreeCellNear(CombatUnit unit, HexCell? toward) => FreeNeighbor(unit, toward ?? unit.Cell);
    internal IReadOnlyList<CombatUnit> Adjacent(CombatUnit unit) => Enumerable.Range(0, grid.Count)
        .Where(i => grid[i].DistanceTo(unit.Cell) == 1 && occupancy[i] is not null).Select(i => occupancy[i]!).ToArray();

    internal CombatUnit? Summon(int team, string lineId, int stage, HexCell cell, SummonOptions? options)
    {
        if (!lines.TryGetValue(lineId, out var line) || !InBounds(cell) || UnitAt(cell) is not null) return null;
        if (team is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(team));
        if (!SupportsAbility(line.Ability.Kind)) throw new NotSupportedException($"Ability {line.Ability.Kind} has not been migrated.");
        options ??= new();
        var entry = new CombatEntry(lineId, stage, cell.Column, team == 0 ? cell.Row : grid.Rows - 1 - cell.Row,
            -1, options.Owner, options.Items);
        var unit = new CombatUnit(units.Count, entry, line, team, cell, random.Next()) { Summoned = true };
        unit.Traits = TraitRules.UnitTraits(entry, rules);
        unit.Equipment.Apply(unit, items, thiefPool, random);
        if (options.HealthMultiplier != 0) unit.MaxHealth = UnitValues.Round(unit.MaxHealth * options.HealthMultiplier);
        if (options.AttackMultiplier != 0) unit.AttackDamage *= options.AttackMultiplier;
        units.Add(unit); occupancy[grid.Index(cell)] = unit;
        if (unit.Equipment.Crown) unit.Shield += unit.MaxHealth * 0.25;
        unit.Health = unit.MaxHealth; unit.Mana = Math.Min(unit.MaxMana - 1, unit.Mana);
        Emit("spawn", unit);
        return unit;
    }

    internal bool MoveFromHook(CombatUnit unit, HexCell cell, double duration)
    {
        if (!InBounds(cell) || (UnitAt(cell) is { } other && other != unit)) return false;
        // Movement duration is presentation-only in the legacy API; it does not change move cooldown.
        Place(unit, cell);
        return true;
    }

    internal bool PlaceFromHook(CombatUnit unit, HexCell cell)
    {
        if (!InBounds(cell) || UnitAt(cell) is not null) return false;
        unit.Cell = cell; occupancy[grid.Index(cell)] = unit;
        return true;
    }

    internal void RemoveFromHook(CombatUnit unit)
    {
        unit.Alive = false;
        if (UnitAt(unit.Cell) == unit) occupancy[grid.Index(unit.Cell)] = null;
    }

    internal void Grow(CombatUnit unit)
    {
        if (unit.Stage >= 2) return;
        unit.Stage++; unit.MaxHealth = UnitValues.Round(unit.MaxHealth * 1.5);
        unit.AttackDamage *= 1.5; unit.BaseAttackDamage *= 1.5;
    }
}

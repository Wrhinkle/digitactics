namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private CombatUnit[] EnemiesNear(CombatUnit unit, HexCell center, int radius) =>
        units.Where(e => e.Alive && e.Team != unit.Team && center.DistanceTo(e.Cell) <= radius).ToArray();

    private CombatUnit[] EnemiesOf(CombatUnit u) => units.Where(e => e.Alive && e.Team != u.Team).ToArray();
    private CombatUnit? TargetByDistance(CombatUnit u, bool farthest = false) => farthest
        ? EnemiesOf(u).OrderByDescending(e => e.Cell.DistanceTo(u.Cell)).FirstOrDefault()
        : EnemiesOf(u).OrderBy(e => e.Cell.DistanceTo(u.Cell) * 10000 + e.Health).FirstOrDefault();
    private CombatUnit? RandomEnemyTarget(CombatUnit u)
    {
        var enemies = units.Where(e => e.Alive && e.Team != u.Team).ToArray();
        return enemies.Length == 0 ? null : enemies[(int)(random.Next() * enemies.Length)];
    }
}

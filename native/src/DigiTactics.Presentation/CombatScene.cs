using DigiTactics.Rules;

namespace DigiTactics.Presentation;

public readonly record struct WorldPosition(double X, double Y, double Z);
public readonly record struct CombatView(int UnitId, WorldPosition Position, double Yaw, int Team, double Dying, bool Hover);

/// <summary>Renderer-independent arena geometry. Mirroring keeps the viewer's team at the bottom.
/// Board occupancy already points to the destination while the visual travels from MoveOrigin.</summary>
public static class CombatLayout
{
    public static WorldPosition Bench(int index, bool wide = false, bool partner = false)
    {
        double x = (index - 4) * 1.45;
        return new(wide ? x * .72 + (partner ? 6.1 : -6.1) : x, 0, 7.55);
    }

    public static WorldPosition Cell(HexCell cell, int columns, bool mirror)
    {
        int c = mirror ? columns - 1 - cell.Column : cell.Column;
        int r = mirror ? 7 - cell.Row : cell.Row;
        return new((c + 0.5 * (r & 1) - (columns - 0.5) / 2) * Math.Sqrt(3), 0, (r - 3.5) * 1.5);
    }

    public static WorldPosition Unit(CombatUnit unit, int columns, bool mirror)
    {
        double fraction = unit.MoveDuration > 0 ? Math.Min(1, unit.MoveElapsed / unit.MoveDuration) : 1;
        var from = Cell(unit.MoveOrigin, columns, mirror);
        var to = Cell(unit.Cell, columns, mirror);
        double height = fraction < 1 ? Math.Sin(Math.PI * fraction) * (unit.MoveDuration < 0.3 ? 1.6 : 0.25) : 0;
        return new(from.X + (to.X - from.X) * fraction, height, from.Z + (to.Z - from.Z) * fraction);
    }
}

/// <summary>One scene per battle. Consumes death cues once, and rebuilds the visible unit list
/// from authoritative state. No GPU resources, gameplay mutation, or wall clock dependencies.</summary>
public sealed class CombatScene
{
    private readonly CombatSimulation combat;
    private readonly Dictionary<int, double> deaths = [];
    private readonly List<CombatView> views = [];
    private int eventCursor;
    public IReadOnlyList<CombatView> Views { get; }

    public CombatScene(CombatSimulation combat)
    {
        this.combat = combat;
        Views = views.AsReadOnly();
    }

    public void Update(bool mirror = false, int? hoveredUnit = null)
    {
        while (eventCursor < combat.Events.Count)
        {
            var cue = combat.Events[eventCursor++];
            if (cue.Type == "death") deaths[cue.Unit] = cue.Time;
        }
        views.Clear();
        foreach (var unit in combat.Units)
        {
            bool dying = !unit.Alive && deaths.TryGetValue(unit.Id, out double death) && combat.Time - death < 0.5;
            if (!unit.Alive && !dying) continue;
            var position = CombatLayout.Unit(unit, combat.Columns, mirror);
            int team = mirror ? 1 - unit.Team : unit.Team;
            double yaw = team == 0 ? Math.PI : 0;
            if (unit.Target is { Alive: true } target)
            {
                var aim = CombatLayout.Unit(target, combat.Columns, mirror);
                yaw = Math.Atan2(aim.X - position.X, aim.Z - position.Z);
            }
            views.Add(new(unit.Id, position, yaw, team, dying ? (combat.Time - deaths[unit.Id]) / 0.5 : 0, hoveredUnit == unit.Id));
        }
    }
}

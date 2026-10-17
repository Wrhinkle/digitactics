namespace DigiTactics.Rules;

public sealed record OwnedUnit(int Id, string Line, int Stage);

/// <summary>Bench ownership and combination rules. Rendering has no access to mutable state.</summary>
public sealed class Roster
{
    private readonly List<OwnedUnit> units = [];
    private int nextId;
    public IReadOnlyList<OwnedUnit> Units => units.AsReadOnly();
    public HashSet<string> MaxedLines() => units.Where(u => u.Stage >= 2).Select(u => u.Line).ToHashSet();
    public bool CanAdd(UnitLine line) => units.Count < 9 || units.Count(u => u.Line == line.Id && u.Stage == 0) >= line.Copies - 1;

    public bool Add(UnitLine line)
    {
        if (!CanAdd(line)) return false;
        units.Add(new(++nextId, line.Id, 0));
        for (int stage = 0; stage < 2; stage++)
        {
            while (true)
            {
                var copies = units.Where(u => u.Line == line.Id && u.Stage == stage).Take(line.Copies).ToArray();
                if (copies.Length < line.Copies) break;
                foreach (var copy in copies) units.Remove(copy);
                units.Add(new(++nextId, line.Id, stage + 1));
            }
        }
        return true;
    }

    public OwnedUnit? Remove(int id)
    {
        var unit = units.Find(u => u.Id == id);
        if (unit is not null) units.Remove(unit);
        return unit;
    }
}

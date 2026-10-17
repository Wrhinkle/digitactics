namespace DigiTactics.Rules;

public readonly record struct HexCell(int Column, int Row)
{
    public int DistanceTo(HexCell other)
    {
        int q = Column - (Row - (Row & 1)) / 2;
        int otherQ = other.Column - (other.Row - (other.Row & 1)) / 2;
        int dq = q - otherQ, dr = Row - other.Row;
        return (Math.Abs(dq) + Math.Abs(dr) + Math.Abs(dq + dr)) / 2;
    }
}

public readonly record struct MovementStep(HexCell Cell, int TargetIndex);

/// <summary>One grid per match. Scratch storage is reused; movement performs no allocations.</summary>
public sealed class HexGrid
{
    private readonly HexCell[] cells;
    private readonly int[][] neighbors;
    private readonly int[] previous;
    private readonly int[] queue;
    public int Columns { get; }
    public int Rows { get; }
    public int Count => cells.Length;
    public HexCell this[int index] => cells[index];

    public HexGrid(int columns, int rows)
    {
        if (columns <= 0 || rows <= 0 || columns > 64 || rows > 64)
            throw new ArgumentOutOfRangeException(nameof(columns), "Grid dimensions must be 1..64.");
        Columns = columns; Rows = rows;
        cells = Enumerable.Range(0, columns * rows).Select(i => new HexCell(i % columns, i / columns)).ToArray();
        // The row-major neighbor order is part of replay compatibility.
        neighbors = cells.Select(cell => Enumerable.Range(0, cells.Length).Where(i => cell.DistanceTo(cells[i]) == 1).ToArray()).ToArray();
        previous = new int[cells.Length]; queue = new int[cells.Length];
    }

    public int Index(HexCell cell)
    {
        if ((uint)cell.Column >= (uint)Columns || (uint)cell.Row >= (uint)Rows)
            throw new ArgumentOutOfRangeException(nameof(cell));
        return cell.Row * Columns + cell.Column;
    }

    public MovementStep? StepTowardEnemy(HexCell start, double range, ReadOnlySpan<HexCell> enemies, ReadOnlySpan<bool> occupied)
    {
        if (occupied.Length != Count) throw new ArgumentException("Occupancy size differs from grid.", nameof(occupied));
        ArgumentOutOfRangeException.ThrowIfLessThan(range, 1);
        foreach (var enemy in enemies) Index(enemy);
        int origin = Index(start);
        Array.Fill(previous, -2);
        previous[origin] = -1;
        int read = 0, write = 1;
        queue[0] = origin;
        while (read < write)
        {
            int current = queue[read++];
            if (current != origin)
            {
                int target = -1, bestDistance = int.MaxValue;
                for (int i = 0; i < enemies.Length; i++)
                {
                    int distance = cells[current].DistanceTo(enemies[i]);
                    if (distance <= range && distance < bestDistance) { target = i; bestDistance = distance; }
                }
                if (target >= 0)
                {
                    int step = current;
                    while (previous[step] != origin) step = previous[step];
                    return new(cells[step], target);
                }
            }
            foreach (int neighbor in neighbors[current])
            {
                if (previous[neighbor] != -2 || occupied[neighbor]) continue;
                previous[neighbor] = current;
                queue[write++] = neighbor;
            }
        }
        return null;
    }
}

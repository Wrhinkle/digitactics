using System.Text.Json;

namespace DigiTactics.Rules;

public sealed record PlanningUnit(int Id, string Line, int Stage, string[] Items,
    Dictionary<string, int>? BonusTraits = null, JsonElement? Mods = null, bool NoSell = false, bool NoItems = false);
public readonly record struct UnitSpot(bool IsBoard, int Column, int Row, int Index)
{
    public static UnitSpot Board(int column, int row) => new(true, column, row, 0);
    public static UnitSpot Bench(int index) => new(false, 0, 0, index);
    public bool Valid => IsBoard ? Column is >= 0 and < 7 && Row is >= 4 and < 8 : Index is >= 0 and < 9;
}
public sealed record PlacedUnit(UnitSpot Spot, PlanningUnit Unit);
public enum MoveResult { Invalid, Capacity, Moved }

/// <summary>Ordered planning board and fixed bench. Unit IDs come from the owning match.</summary>
public sealed class PlanningRoster
{
    private readonly Dictionary<string, UnitLine> lines;
    private readonly Dictionary<string, ItemDefinition> items;
    // Order matters for merge anchors and cap enforcement, including delete/reinsert.
    private readonly List<PlacedUnit> board = [];
    private readonly PlanningUnit?[] bench = new PlanningUnit?[9];
    private readonly Func<string, int, PlanningUnit> create;
    public PlanningRoster(Ruleset rules, Func<string, int, PlanningUnit> create)
    {
        lines = rules.Lines.Concat(rules.ExtraLines).ToDictionary(l => l.Id);
        items = rules.Items.ToDictionary(i => i.Id);
        this.create = create;
    }

    public IReadOnlyList<PlacedUnit> Board => board.AsReadOnly();
    public IReadOnlyList<PlanningUnit?> Bench => Array.AsReadOnly(bench);
    public IEnumerable<PlacedUnit> Entries => board.Concat(bench.Select((u, i) => u is null ? null : new PlacedUnit(UnitSpot.Bench(i), u)).OfType<PlacedUnit>());
    public int FreeBenchSlot => Array.IndexOf(bench, null);
    public int BoardCount => board.Sum(e => lines[e.Unit.Line].Slots);
    public int TeamSizeBonus => board.Sum(e => TeamSizeItems(e.Unit));
    public HashSet<string> MaxedLines() => Entries.Where(e => e.Unit.Stage >= 2).Select(e => e.Unit.Line).ToHashSet();
    public PlanningUnit? At(UnitSpot spot) => !spot.Valid ? null : spot.IsBoard
        ? board.FirstOrDefault(e => e.Spot == spot)?.Unit : bench[spot.Index];

    public bool AddToBench(PlanningUnit unit)
    {
        int free = FreeBenchSlot;
        if (free < 0 || !lines.ContainsKey(unit.Line) || Entries.Any(e => e.Unit.Id == unit.Id)) return false;
        bench[free] = unit;
        return true;
    }

    public PlanningUnit? Remove(int id, bool boardLocked)
    {
        var entry = Entries.FirstOrDefault(e => e.Unit.Id == id);
        if (entry is null || boardLocked && entry.Spot.IsBoard) return null;
        Set(entry.Spot, null);
        return entry.Unit;
    }

    public bool CanAcceptPurchase(string line, bool boardLocked) => FreeBenchSlot >= 0 ||
        Entries.Count(e => (!boardLocked || !e.Spot.IsBoard) && e.Unit.Line == line && e.Unit.Stage == 0) >= lines[line].Copies - 1;

    public MoveResult Move(UnitSpot from, UnitSpot to, bool boardLocked, Func<int> capacity,
        Func<PlanningUnit, UnitSpot, bool>? canPlace = null)
    {
        if (!from.Valid || !to.Valid || boardLocked && (from.IsBoard || to.IsBoard) || At(from) is not { } a)
            return MoveResult.Invalid;
        var b = At(to);
        if (!from.IsBoard && to.IsBoard)
        {
            int aSlots = lines[a.Line].Slots, bSlots = b is null ? 0 : lines[b.Line].Slots;
            bool over = b is not null ? aSlots > bSlots && BoardCount - bSlots + aSlots > capacity()
                : BoardCount + aSlots > capacity();
            if (over) return MoveResult.Capacity;
        }
        if (to.IsBoard && canPlace is not null && !canPlace(a, to)) return MoveResult.Invalid;
        Set(from, b);
        Set(to, a);
        while (BoardCount > capacity() && FreeBenchSlot >= 0)
        {
            var entry = board.FirstOrDefault(e => TeamSizeItems(e.Unit) == 0) ?? board[0];
            Set(entry.Spot, null);
            bench[FreeBenchSlot] = entry.Unit;
        }
        return MoveResult.Moved;
    }

    public IReadOnlyList<PlanningUnit> Combine(bool boardLocked, Action<string> giveItem,
        PlanningUnit? extra = null, int maxStage = 2,
        Action<PlanningUnit, IReadOnlyList<PlanningUnit>>? onCombine = null)
    {
        List<PlanningUnit> evolved = [];
        while (true)
        {
            var entries = Entries.Where(e => !boardLocked || !e.Spot.IsBoard).ToList();
            if (extra is not null) entries.Add(new(UnitSpot.Bench(-1), extra));
            var group = entries.Where(e => e.Unit.Stage < maxStage).GroupBy(e => (e.Unit.Line, e.Unit.Stage))
                .FirstOrDefault(g => g.Count() >= lines[g.Key.Line].Copies);
            if (group is null) break;
            var copies = group.Take(lines[group.Key.Line].Copies).ToArray();
            var anchor = copies[0];
            var carried = copies.SelectMany(e => e.Unit.Items).OrderBy(id => items.TryGetValue(id, out var item) && item.Component ? 1 : 0).ToArray();
            var up = create(anchor.Unit.Line, anchor.Unit.Stage + 1) with
            {
                Items = carried.Take(3).ToArray(), BonusTraits = anchor.Unit.BonusTraits, Mods = anchor.Unit.Mods,
            };
            foreach (var id in carried.Skip(3)) giveItem(id);
            foreach (var copy in copies)
                if (copy.Spot.Valid) Set(copy.Spot, null); else extra = null;
            Set(anchor.Spot.Valid ? anchor.Spot : UnitSpot.Bench(FreeBenchSlot), up);
            evolved.Add(up);
            onCombine?.Invoke(up, copies.Select(e => e.Unit).ToArray());
        }
        if (extra is not null && !AddToBench(extra)) throw new InvalidOperationException("Unplaceable merge overflow.");
        return evolved;
    }

    private int TeamSizeItems(PlanningUnit unit) => unit.Items.Sum(id => items.TryGetValue(id, out var item) &&
        item.Passives.TryGetValue("teamSize", out var size) ? size.GetInt32() : 0);

    private void Set(UnitSpot spot, PlanningUnit? unit)
    {
        if (!spot.Valid) throw new ArgumentOutOfRangeException(nameof(spot));
        if (!spot.IsBoard) { bench[spot.Index] = unit; return; }
        int index = board.FindIndex(e => e.Spot == spot);
        if (unit is null) { if (index >= 0) board.RemoveAt(index); }
        else if (index >= 0) board[index] = new(spot, unit);
        else board.Add(new(spot, unit));
    }
}

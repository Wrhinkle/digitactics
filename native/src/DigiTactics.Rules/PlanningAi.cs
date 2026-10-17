namespace DigiTactics.Rules;

public sealed record UnitAiHints(bool Carry, int Tank, string? Damage, string? Place, double Value);
public sealed record AiItemHolder(PlanningUnit Unit, string Kind);

/// <summary>Deterministic roster preferences, placement search and item-holder ranking.</summary>
public sealed class PlanningAi(Ruleset rules)
{
    private readonly Dictionary<string, UnitLine> lines = rules.Lines.Concat(rules.ExtraLines).ToDictionary(l => l.Id);
    private readonly HashSet<string> components = rules.Items.Where(i => i.Component).Select(i => i.Id).ToHashSet();

    // These counts deliberately omit augments, matching the AI's original roster view.
    public Dictionary<string, int> Counts(IEnumerable<PlanningUnit> units, MatchPlayer? partner = null)
    {
        Dictionary<string, int> Count(IEnumerable<PlanningUnit> source) => TraitRules.Count(rules,
            source.Select(u => new CombatEntry(u.Line, u.Stage, 0, 4, u.Id, Items: u.Items, BonusTraits: u.BonusTraits)));
        var result = Count(units);
        if (partner is { Alive: true })
            foreach (var pair in Count(partner.Roster.Board.Select(e => e.Unit)))
                result[pair.Key] = result.GetValueOrDefault(pair.Key) + pair.Value;
        return result;
    }

    // The caller supplies live capacity and eligible partner copies after portal-limit checks.
    public bool Wants(MatchPlayer player, string lineId, int capacity, int partnerCopies = 0)
    {
        var owned = player.Roster.Entries.Select(e => e.Unit).ToArray();
        if (owned.Any(u => u.Line == lineId && u.Stage < 2)) return true;
        var counts = Counts(player.Roster.Board.Select(e => e.Unit));
        var line = lines[lineId];
        if (line.Traits.Any(t => counts.GetValueOrDefault(t) >= 2)) return true;
        if (line.Traits.Any(t => counts.GetValueOrDefault(t) >= 1 &&
            rules.Traits.Any(d => d.Id == t && d.Breakpoints.Contains(counts.GetValueOrDefault(t) + 1)))) return true;
        if (line.Tier == rules.Lines.Max(l => l.Tier) && player.Economy.Gold >= 8) return true;
        if ((line.Ai?.Value ?? 0) >= 6 || partnerCopies >= 2) return true;
        return owned.Length < capacity + 2;
    }

    public PlanningUnit? WorstBench(MatchPlayer player)
    {
        var counts = player.Roster.Entries.GroupBy(e => (e.Unit.Line, e.Unit.Stage)).ToDictionary(g => g.Key, g => g.Count());
        return player.Roster.Bench.OfType<PlanningUnit>().Where(u => !u.NoSell && !lines[u.Line].NoSell)
            .OrderBy(u => UnitValues.Value(lines[u.Line], u.Stage) + (counts[(u.Line, u.Stage)] >= 2 ? 100 : 0) + u.Items.Length * 50)
            .FirstOrDefault();
    }

    public UnitSpot? FreeCell(MatchPlayer player, PlanningUnit unit, Func<PlanningUnit, UnitSpot, bool> canPlace)
    {
        string? place = lines[unit.Line].Ai?.Place;
        int[] rows = place == "flank" ? [7, 6] : place == "front" ? [4, 5, 6, 7] : [7, 6, 5, 4];
        int[] columns = place == "flank" ? [0, 6, 1, 5, 2, 4, 3] : [3, 2, 4, 1, 5, 0, 6];
        foreach (int row in rows) foreach (int column in columns)
        {
            var spot = UnitSpot.Board(column, row);
            if (player.Roster.At(spot) is null && canPlace(unit, spot)) return spot;
        }
        return null;
    }

    public IReadOnlyList<AiItemHolder> Holders(MatchPlayer player)
    {
        int Value(PlanningUnit unit) => UnitValues.Value(lines[unit.Line], unit.Stage);
        UnitAiHints Hints(PlanningUnit unit) => lines[unit.Line].Ai!;
        var board = player.Roster.Board.Select(e => e.Unit).Where(u => !u.NoItems && !lines[u.Line].NoItems)
            .OrderByDescending(Value).ThenByDescending(u => u.Items.Length).ToArray();
        var carries = board.Where(u => Hints(u).Carry).OrderByDescending(u => u.Items.Count(id => !components.Contains(id)))
            .ThenByDescending(Value).Take(2).ToArray();
        var tank = board.FirstOrDefault(u => Hints(u).Tank == 1) ?? board.FirstOrDefault(u => Hints(u).Tank == 2) ??
            board.FirstOrDefault(u => Hints(u).Tank == 3 && !carries.Any(c => c.Id == u.Id));
        var result = carries.Select(u => new AiItemHolder(u, Hints(u).Damage == "ap" ? "ap" : "ad")).ToList();
        if (tank is not null) result.Add(new(tank, "tank"));
        return result;
    }
}

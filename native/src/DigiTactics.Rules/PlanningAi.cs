namespace DigiTactics.Rules;

public sealed record UnitAiHints(bool Carry, int Tank, string? Damage, string? Place, double Value);
public sealed record AiItemHolder(PlanningUnit Unit, string Kind);

/// <summary>Deterministic roster preferences, placement search and item-holder ranking.</summary>
public sealed class PlanningAi(Ruleset rules)
{
    private readonly Dictionary<string, UnitLine> lines = rules.Lines.Concat(rules.ExtraLines).ToDictionary(l => l.Id);
    private readonly Dictionary<string, TraitDefinition> definitions = rules.Traits.ToDictionary(t => t.Id);
    private readonly Dictionary<string, ItemDefinition> items = rules.Items.ToDictionary(i => i.Id);
    private readonly string mode = rules.Id.Split('/')[0];
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

    public double TraitValue(string lineId, IReadOnlyDictionary<string, int> counts, int stage)
    {
        int active = counts.Count(c => definitions.TryGetValue(c.Key, out var t) && TraitRules.Level(t, c.Value) > 0);
        double score = 0;
        foreach (string id in lines[lineId].Traits)
        {
            if (!definitions.TryGetValue(id, out var trait)) continue;
            int n = counts.GetValueOrDefault(id);
            score += n * 3;
            if (trait.Breakpoints.Contains(n)) score += 3;
            if (n > 0 && trait.Breakpoints.Contains(n + 1)) score += 2;
            score += (mode, id) switch
            {
                ("faction-wars", "RK") => active,
                ("faction-wars", "FUN") => stage <= 3 ? 2 : 0,
                ("fates", "TREASURE") or ("galaxies", "PI") or ("gizmos", "PIR") or ("arcane", "SYN") => stage <= 4 ? 1 : 0,
                ("gizmos", "PET") => 1,
                ("reckoning", "DRK") or ("coliseum", "CG") => stage <= 4 ? 2 : 0,
                ("reckoning", "RIV") => n >= 1 ? -12 : 0,
                ("underlords" or "underlords_ko", "BEAST") => stage >= 4 ? 1 : 0,
                ("underlords" or "underlords_ko", "LEGEND") => active * 0.5,
                ("autochess", "DEMON") => n >= 1 && counts.GetValueOrDefault("DH") < 2 ? -8 : 2,
                ("autochess", "OLYMPOS") => counts.Any(c => c.Key != "OLYMPOS" && definitions.TryGetValue(c.Key, out var t) && t.Kind == "race" && TraitRules.Level(t, c.Value) > 0) ? -6 : 1,
                ("autochess", "DRUID") => n >= 1 && n < 4 ? 4 : 0,
                ("autochess", "WIZARD") => counts.Any(c => c.Key != "WIZARD" && definitions.TryGetValue(c.Key, out var t) && t.Breakpoints.Any(b => b >= 4 && c.Value == b - 2)) ? 2 : 0,
                _ => 0
            };
        }
        return score + (lines[lineId].Ai?.Value ?? 0);
    }

    public double Score(PlanningUnit unit, IReadOnlyDictionary<string, int> counts, int stage) =>
        UnitValues.Value(lines[unit.Line], unit.Stage) * 10 + unit.Items.Length * 6 + TraitValue(unit.Line, counts, stage);

    // Overflow follows the original arrangement path: refunds bypass normal sale hooks.
    public void Arrange(MatchPlayer player, int baseCapacity, int stage, MatchPlayer? partner,
        Func<PlanningUnit, UnitSpot, bool> canPlace, Action<PlanningUnit> returnToPool)
    {
        var units = player.Roster.Entries.Select(e => e.Unit).ToArray();
        foreach (var unit in units) player.Roster.Remove(unit.Id, false);
        var counts = Counts(units, partner);
        int Bonus(PlanningUnit unit) => unit.Items.Sum(id => items.TryGetValue(id, out var item) &&
            item.Passives.TryGetValue("teamSize", out var size) ? size.GetInt32() : 0);
        var sorted = units.OrderByDescending(u => Score(u, counts, stage)).OrderByDescending(u => Bonus(u) > 0).ToArray();
        int capacity = baseCapacity + sorted.Take(baseCapacity).Sum(Bonus), used = 0;
        var field = new List<PlanningUnit>(); var rest = new List<PlanningUnit>();
        foreach (var unit in sorted)
        {
            int slots = lines[unit.Line].Slots;
            if (used + slots <= capacity && (slots == 1 || used < capacity)) { field.Add(unit); used += slots; }
            else rest.Add(unit);
        }
        foreach (var unit in field.OrderByDescending(u => lines[u.Line].Ai?.Place == "front"))
        {
            var spot = FreeCell(player, unit, canPlace);
            if (spot is { } cell) player.Roster.PlaceDetachedOnBoard(unit, cell); else rest.Add(unit);
        }
        foreach (var unit in rest.OrderByDescending(u => u.NoSell || lines[u.Line].NoSell))
        {
            if (player.Roster.AddToBench(unit)) continue;
            player.Economy.Credit(UnitValues.SellValue(lines[unit.Line], unit.Stage));
            foreach (string item in unit.Items) player.GiveItem(item);
            returnToPool(unit);
        }
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

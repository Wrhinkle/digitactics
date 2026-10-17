namespace DigiTactics.Rules;

public sealed record UnitAiHints(bool Carry, int Tank, string? Damage, string? Place, double Value);
public sealed record AiItemHolder(PlanningUnit Unit, string Kind);

/// <summary>Stable holder ranking preserves existing builds and the original carry/tank overlap.</summary>
public sealed class PlanningAi(Ruleset rules)
{
    private readonly Dictionary<string, UnitLine> lines = rules.Lines.Concat(rules.ExtraLines).ToDictionary(l => l.Id);
    private readonly HashSet<string> components = rules.Items.Where(i => i.Component).Select(i => i.Id).ToHashSet();

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

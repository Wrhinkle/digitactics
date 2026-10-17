namespace DigiTactics.Rules;

/// <summary>Tamer power selection creates a persistent roster unit; rank changes update it in place.</summary>
public sealed class UnderlordsPlanning(MatchState state)
{
    private readonly Dictionary<string, UnitLine> lines = state.Rules.Lines.Concat(state.Rules.ExtraLines).ToDictionary(l => l.Id);
    private readonly PlanningAi placement = new(state.Rules);
    private PlanningUnit? Tamer(MatchPlayer player) => player.Roster.Entries.Select(e => e.Unit).FirstOrDefault(u => !string.IsNullOrEmpty(lines[u.Line].Tamer));

    public void PowerPicked(MatchPlayer player, string id)
    {
        if (!state.Rules.OfferAi!.TryGetValue("power:" + id, out var power) || power.Tamer is null ||
            state.Rules.Tamers is null || !state.Rules.Tamers.TryGetValue(power.Tamer, out var tamer) || Tamer(player) is not null) return;
        var unit = state.CreateUnit(tamer.Line, 0);
        var cell = placement.FreeCell(player, unit, (_, _) => true);
        if (cell is { } spot) player.Roster.PlaceDetachedOnBoard(unit, spot);
        else player.Roster.AddToBench(unit);
    }

    public void RankUp(MatchPlayer player, int rank)
    {
        if (Tamer(player) is { } unit) player.Roster.UpdateUnit(unit with { Stage = Math.Min(2, (int)Math.Floor((rank - 1) / 2.0)) });
    }
}

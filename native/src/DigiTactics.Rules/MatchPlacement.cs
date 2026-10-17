namespace DigiTactics.Rules;

/// <summary>Live capacity and movement share persistent bonuses and current board equipment.</summary>
public sealed class MatchPlacement(MatchState state)
{
    private readonly string mode = state.Rules.Id.Split('/')[0];
    private readonly Dictionary<string, UnitLine> lines = state.Rules.Lines.Concat(state.Rules.ExtraLines).ToDictionary(l => l.Id);

    // AI arrangement uses this base before selecting equipped size-bonus holders.
    // The original arrangement does not include onTeamSize listeners in this base.
    public int BaseCapacity(MatchPlayer player) => player.Economy.Level + player.TeamBonus;

    public int Capacity(MatchPlayer player, int stage)
    {
        int size = BaseCapacity(player) + player.Roster.TeamSizeBonus;
        if (mode == "gizmos" && player.Augments.Contains("giz_a_recruit")) size++;
        if (mode is "underlords" or "underlords_ko" && player.Power is { } power &&
            state.Rules.OfferAi!.TryGetValue("power:" + power.Id, out var definition) && !string.IsNullOrEmpty(definition.Tamer) &&
            player.Roster.Board.Any(e => !string.IsNullOrEmpty(lines[e.Unit.Line].Tamer))) size++;
        foreach (var id in player.Globals)
            if (mode == "autochess" && id == "ac_r_key" || mode is "underlords" or "underlords_ko" && id == "ul_g_roster") size++;
        if (mode == "galaxies" && state.Galaxy == "bigger" && stage >= 2) size++;
        return size;
    }

    public MoveResult Move(MatchPlayer player, UnitSpot from, UnitSpot to, MatchPhase phase, int stage) =>
        player.Roster.Move(from, to, phase == MatchPhase.Combat && player.IsHuman, () => Capacity(player, stage));
}

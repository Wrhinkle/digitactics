namespace DigiTactics.Rules;

/// <summary>Mode preparation, live capacity and shop refreshes for the composed AI turn.</summary>
public sealed class MatchAiEffects : IMatchAiHooks
{
    private readonly MatchPlacement placement;
    private readonly Action<MatchPlayer, RoundPosition, MatchPhase> prepare;
    private readonly bool hasEquipRules;
    public MatchAiEffects(MatchState state, MatchUnitLifecycle units)
    {
        placement = new(state);
        Action<MatchPlayer, RoundPosition, MatchPhase> Inventory(Action<MatchPlayer, PlanningItems, MatchPhase> effect) =>
            (player, _, phase) => effect(player, units.Items, phase);
        Action<MatchPlayer, RoundPosition, MatchPhase> Simple(Action<MatchPlayer> effect) => (player, _, _) => effect(player);
        string mode = state.Rules.Id.Split('/')[0];
        hasEquipRules = mode is "underlords" or "underlords_ko";
        var faction = mode == "faction-wars" ? new FactionWarsPlanning(state) : null;
        var reckoning = mode == "reckoning" ? new ReckoningPlanning(state) : null;
        prepare = mode switch
        {
            "faction-wars" => (player, _, phase) => faction!.AiTurn(player, phase),
            "fates" => Inventory(new FatesPlanning(state).AiTurn),
            "galaxies" => Inventory(new GalaxiesPlanning(state).AiTurn),
            "arcane" => Inventory(new ArcanePlanning(state).AiTurn),
            "autochess" => Inventory(new AutoChessPlanning(state).AiTurn),
            "coliseum" => Simple(new ColiseumPlanning(state).AiTurn),
            "reckoning" => (player, round, _) => reckoning!.AiTurn(player, units.Items, round.Stage),
            "gizmos" or "dragonlands" or "underlords" or "underlords_ko" => (_, _, _) => { },
            _ => throw new NotSupportedException("Unknown AI mode: " + mode)
        };
    }
    public void TurnStarted(MatchPlayer player, RoundPosition round, MatchPhase phase) => prepare(player, round, phase);
    public int BaseCapacity(MatchPlayer player) => placement.BaseCapacity(player);
    public int Capacity(MatchPlayer player, int stage) => placement.Capacity(player, stage);
    public bool CanPlace(MatchPlayer player, PlanningUnit unit, UnitSpot spot) => true;
    public bool HasEquipRules(MatchPlayer player) => hasEquipRules;
    public void Reroll(MatchPlayer player) => player.Shop.Reroll();
}

namespace DigiTactics.Rules;

/// <summary>Routes resolved fights through mode rewards and the shared unit lifecycle.</summary>
public sealed class MatchSettlementEffects : IMatchSettlementHooks
{
    private readonly MatchLoot loot;
    private readonly RoundPosition round;
    private readonly Func<RoundEndContext, int> end;

    public MatchSettlementEffects(MatchState state, MatchUnitLifecycle units, RoundPosition round)
    {
        loot = new(state); this.round = round;
        switch (state.Rules.Id.Split('/')[0])
        {
            case "faction-wars": end = new FactionWarsPlanning(state).RoundEnd; break;
            case "fates": end = new FatesPlanning(state).RoundEnd; break;
            case "galaxies":
                var galaxies = new GalaxiesPlanning(state); end = c => galaxies.RoundEnd(c, round.Stage); break;
            case "gizmos":
                var gizmos = new GizmosPlanning(state);
                end = c => gizmos.RoundEnd(c, round.Stage, p => units.Combine(p, MatchPhase.Combat)); break;
            case "reckoning":
                var reckoning = new ReckoningPlanning(state); end = c => { reckoning.RoundEnd(c, round.Stage); return 0; }; break;
            case "dragonlands": end = new DragonlandsPlanning(state).RoundEnd; break;
            case "arcane": end = new ArcanePlanning(state).RoundEnd; break;
            case "coliseum":
                var coliseum = new ColiseumPlanning(state); end = c => { coliseum.RoundEnd(c, round.Stage); return 0; }; break;
            case "autochess":
                var autoChess = new AutoChessPlanning(state); end = c => { autoChess.RoundEnd(c, round.Stage); return 0; }; break;
            case "underlords": case "underlords_ko":
                var underlords = new UnderlordsPlanning(state); end = c => underlords.RoundEnd(c, round.Stage); break;
            default: throw new NotSupportedException("Unknown settlement mode: " + state.Rules.Id);
        }
    }

    public PveRewardSummary Reward(MatchPlayer player, bool won) => loot.Reward(player, round, won);
    public int RoundEnd(RoundEndContext context) => end(context);
    public void AfterCombat(MatchPlayer player) => player.NextBuffs = null;
    // Current modes define no elimination hooks. Core cleanup is owned by MatchState.
    public void Eliminated(MatchPlayer player) { }
}

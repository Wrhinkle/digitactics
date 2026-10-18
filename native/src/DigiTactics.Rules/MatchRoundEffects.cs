namespace DigiTactics.Rules;

/// <summary>Composes round-start mode effects, Duos gifts, income, carousel and AI services.</summary>
public sealed class MatchRoundEffects : IMatchRoundHooks
{
    private readonly MatchState state;
    private readonly MatchUnitLifecycle units;
    private readonly MatchAi ai;
    private readonly Func<MatchPhase> phase;
    private readonly DuosPlanning duos;
    private readonly Action<MatchPlayer, RoundPosition> start;
    private readonly Func<MatchPlayer, RoundPosition, IncomeBreakdown, IncomeBreakdown> income;
    private readonly UnderlordsPlanning? underlords;
    private readonly bool interleaveGifts;

    public MatchRoundEffects(MatchState state, MatchOffers offers, MatchUnitLifecycle units, MatchAi ai, Func<MatchPhase> phase)
    {
        this.state = state; this.units = units; this.ai = ai; this.phase = phase;
        duos = new(state, units.Portal);
        income = (_, _, value) => value;
        switch (state.Rules.Id.Split('/')[0])
        {
            case "faction-wars":
                var faction = new FactionWarsPlanning(state); start = faction.RoundStart; income = (p, _, value) => faction.Income(p, value); break;
            case "fates":
                var fates = new FatesPlanning(state); start = (p, _) => fates.RoundStart(p); income = (p, _, value) => fates.Income(p, value); break;
            case "galaxies":
                var galaxies = new GalaxiesPlanning(state); start = galaxies.RoundStart; income = (p, _, value) => galaxies.Income(p, value); break;
            case "gizmos":
                var gizmos = new GizmosPlanning(state); interleaveGifts = true;
                start = (p, r) => gizmos.RoundStart(p, r, Type(r), id => { if (id == "duo_care") Gift(p, r); }); break;
            case "reckoning":
                var reckoning = new ReckoningPlanning(state); start = (p, r) => reckoning.RoundStart(p, Type(r)); income = (p, _, value) => reckoning.Income(p, value); break;
            case "dragonlands":
                var dragons = new DragonlandsPlanning(state); start = (p, _) => dragons.RoundStart(p); break;
            case "arcane":
                var arcane = new ArcanePlanning(state); start = (p, r) => arcane.RoundStart(p, r, offers); income = (p, _, value) => arcane.Income(p, value); break;
            case "coliseum":
                var coliseum = new ColiseumPlanning(state);
                start = (p, r) => { if (p.Augments.Contains("col_a_rolls")) p.Shop.FreeRolls++; coliseum.RoundStart(p, r); };
                income = (p, _, value) => coliseum.Income(p, value); break;
            case "autochess":
                var autoChess = new AutoChessPlanning(state); start = (p, _) => autoChess.RoundStart(p); income = autoChess.Income; break;
            case "underlords": case "underlords_ko":
                underlords = new(state); start = underlords.RoundStart; income = underlords.Income; break;
            default: throw new NotSupportedException("Unknown round mode: " + state.Rules.Id);
        }
    }
    private string Type(RoundPosition round) => state.Rules.Schedule!.TypeOf(round);
    private void Gift(MatchPlayer player, RoundPosition round) => duos.RoundStart(player, round, phase(), Type(round));
    public void ResetTransfers(MatchPlayer player) => units.Portal.Reset(player);
    public IncomeBreakdown Income(MatchPlayer player, RoundPosition round, IncomeBreakdown value) => income(player, round, duos.Income(player, value));
    public void RankUp(MatchPlayer player, RoundPosition round) { if (player.Power is { } power) underlords?.RankUp(player, power.Rank); }
    public void RoundStart(MatchPlayer player, RoundPosition round)
    {
        if (!interleaveGifts) Gift(player, round);
        start(player, round);
    }
    public void StartCarousel(RoundPosition round) => units.Carousel.Start(round.Stage, phase());
    public void PlanAi(RoundPosition round) => ai.PlanPlayers(round, phase(), Type(round));
}

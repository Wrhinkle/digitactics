namespace DigiTactics.Rules;

/// <summary>Owns the composed planning loop. Combat resolution remains the caller's responsibility.</summary>
public sealed class MatchPlanningSession
{
    public MatchPlanningSession(Ruleset rules, uint seed, IReadOnlyList<MatchSeat> seats, bool duos = false, int[][]? teams = null)
    {
        MatchRounds? rounds = null;
        MatchPhase Phase() => rounds?.Phase ?? MatchPhase.Planning;
        State = MatchStart.Create(rules, seed, seats, () => rounds?.Position ?? RoundPosition.First, duos, teams);
        Units = State.Units!;
        CombatProjection = new(State);
        Offers = new(State, Units, Phase);
        Ai = new(State, Offers, Units);
        var effects = new MatchRoundEffects(State, Offers, Units, Ai, Phase);
        Rounds = rounds = new(State, Offers, effects);
        Rounds.Start();
    }
    public MatchState State { get; }
    public MatchUnitLifecycle Units { get; }
    public MatchCombatProjection CombatProjection { get; }
    public MatchOffers Offers { get; }
    public MatchAi Ai { get; }
    public MatchRounds Rounds { get; }
    public bool PickCarousel(MatchPlayer player, int index)
    {
        if (!Units.Carousel.Pick(player, index, Rounds.Phase)) return false;
        Units.Carousel.Step(Rounds.Phase);
        return true;
    }
    public bool Advance()
    {
        if (Units.Carousel.Active) Units.Carousel.Finish(Rounds.Phase);
        return Rounds.Advance();
    }
}

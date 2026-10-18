namespace DigiTactics.Rules;

public enum MatchPhase { Planning, Combat, Result }
public interface IMatchRoundHooks
{
    void ResetTransfers(MatchPlayer player);
    IncomeBreakdown Income(MatchPlayer player, RoundPosition round, IncomeBreakdown income);
    void RankUp(MatchPlayer player, RoundPosition round);
    void RoundStart(MatchPlayer player, RoundPosition round);
    void StartCarousel(RoundPosition round);
    void PlanAi(RoundPosition round);
}

/// <summary>Round transitions wire economy, shops, offers, powers, pairing and settlement in reference order.</summary>
public sealed class MatchRounds
{
    private readonly MatchState state;
    private readonly MatchOffers offers;
    private readonly IMatchRoundHooks hooks;
    private bool openingHandled;
    public MatchRounds(MatchState state, MatchOffers offers, IMatchRoundHooks hooks,
        RoundPosition? position = null, MatchPhase phase = MatchPhase.Planning)
    {
        this.state = state; this.offers = offers; this.hooks = hooks;
        Position = position ?? RoundPosition.First;
        _ = state.Rules.Schedule!.TypeOf(Position);
        Phase = phase;
        openingHandled = Position != RoundPosition.First || phase != MatchPhase.Planning;
    }
    public RoundPosition Position { get; private set; }
    public MatchPhase Phase { get; private set; }
    public string RoundType => state.Rules.Schedule!.TypeOf(Position);
    public IReadOnlyList<MatchPair> Pairs { get; private set; } = [];
    public IReadOnlyList<MatchRoundResult> Results { get; private set; } = [];

    public bool Start()
    {
        if (state.Over || openingHandled) return false;
        if (Phase != MatchPhase.Planning) throw new InvalidOperationException("The opening round is no longer in planning.");
        openingHandled = true;
        if (RoundType == "carousel") hooks.StartCarousel(Position);
        else
        {
            foreach (var player in state.Players)
            {
                offers.Make(player, Position);
                hooks.RoundStart(player, Position);
                player.Shop.ClearMaxed();
            }
            hooks.PlanAi(Position);
            Pairs = state.MakePairs(RoundType);
            if (state.Duos) state.SyncTeams();
        }
        return true;
    }

    public bool Advance()
    {
        if (state.Over) return false;
        if (Phase == MatchPhase.Combat) throw new InvalidOperationException("Finish combat before advancing the round.");
        openingHandled = true;
        Position = state.Rules.Schedule!.Next(Position);
        if (state.Duos) foreach (var player in state.Players) hooks.ResetTransfers(player);
        foreach (var player in state.Players.Where(p => p.Alive).ToArray())
        {
            player.Economy.BeginRound(Position.Stage, Position.Round, income => hooks.Income(player, Position, income));
            player.Shop.FreeRolls = 0;
            if (!player.Shop.Locked) player.Shop.Refresh();
            offers.Make(player, Position);
            if (player.Power is { } power && state.Rules.Offers!.PowerRanks.Contains(Position.Label))
            {
                player.Power = power with { Rank = power.Rank + 1 };
                hooks.RankUp(player, Position);
            }
            hooks.RoundStart(player, Position);
            player.Shop.ClearMaxed();
        }
        Phase = MatchPhase.Planning;
        if (RoundType == "carousel") hooks.StartCarousel(Position);
        else
        {
            hooks.PlanAi(Position);
            Pairs = state.MakePairs(RoundType);
        }
        if (state.Duos) state.SyncTeams();
        return true;
    }
    public void BeginCombat()
    {
        if (state.Over || Phase != MatchPhase.Planning)
            throw new InvalidOperationException("The round is not ready for combat.");
        Phase = MatchPhase.Combat;
    }
    public IReadOnlyList<MatchRoundResult> Resolve(IReadOnlyList<ResolvedMatch> matches, IMatchSettlementHooks settlementHooks)
    {
        if (Phase != MatchPhase.Combat) throw new InvalidOperationException("There is no combat to settle.");
        Results = new MatchSettlement(state, Position.Stage, settlementHooks).Settle(matches);
        Phase = MatchPhase.Result;
        return Results;
    }
}

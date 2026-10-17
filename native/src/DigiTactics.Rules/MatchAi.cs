namespace DigiTactics.Rules;

public interface IMatchAiHooks
{
    void TurnStarted(MatchPlayer player, RoundPosition round, MatchPhase phase);
    // Rules team size plus persistent team bonus; arrangement adds equipment itself.
    int BaseCapacity(MatchPlayer player);
    // Live cap including equipped team-size items and on-team-size hooks.
    int Capacity(MatchPlayer player, int stage);
    bool CanPlace(MatchPlayer player, PlanningUnit unit, UnitSpot spot);
    bool HasEquipRules(MatchPlayer player);
    void Reroll(MatchPlayer player);
}

/// <summary>Complete AI turn order; round orchestration retains pairing and team-health synchronization.</summary>
public sealed class MatchAi
{
    private readonly MatchState state;
    private readonly MatchOffers offers;
    private readonly MatchPortal portal;
    private readonly IMatchAiHooks hooks;
    private readonly AiOffers choices;
    private readonly AiShopping shopping;
    private readonly PlanningAi planner;
    private readonly AiInventory inventory;
    private readonly Dictionary<string, UnitLine> shopLines;

    public MatchAi(MatchState state, MatchOffers offers, MatchUnitLifecycle units)
        : this(state, offers, units.Shop, units.Items, units.Portal, new MatchAiEffects(state, units)) { }

    public MatchAi(MatchState state, MatchOffers offers, PlanningShopCommands shop, PlanningItems items, MatchPortal portal, IMatchAiHooks hooks)
    {
        this.state = state; this.offers = offers; this.portal = portal; this.hooks = hooks;
        choices = new(state); shopping = new(state.Rules, shop); planner = new(state.Rules); inventory = new(state.Rules, items);
        shopLines = state.Rules.Lines.ToDictionary(l => l.Id);
    }
    public void PlanPlayers(RoundPosition round, MatchPhase phase, string roundType)
    {
        foreach (var player in state.Players.Where(p => p.Alive && !p.IsHuman).ToArray()) Turn(player, round, phase, roundType);
    }
    public void Turn(MatchPlayer player, RoundPosition round, MatchPhase phase, string roundType)
    {
        if (player.Offer is { } offer)
            offers.Choose(player, choices.Select(player, offer, round.Stage), phase == MatchPhase.Combat && player.IsHuman);
        hooks.TurnStarted(player, round, phase);
        shopping.Shop(player, phase, round.Stage, round.Round, () => hooks.Capacity(player, round.Stage),
            line => portal.PartnerCopies(player, line, 0), () => hooks.Reroll(player));
        if (state.Duos) portal.AiTurn(player, phase, roundType);
        shopping.TrimBench(player, phase);
        Arrange(player, round.Stage);
        inventory.Assign(player, round.Stage, hooks.HasEquipRules(player));
        Arrange(player, round.Stage);
    }
    private void Arrange(MatchPlayer player, int stage) => planner.Arrange(player, hooks.BaseCapacity(player), stage,
        portal.Partner(player), (unit, spot) => hooks.CanPlace(player, unit, spot), unit =>
        {
            if (shopLines.TryGetValue(unit.Line, out var line)) state.Pool.Return(line.Id, UnitValues.Copies(line, unit.Stage));
        });
}

namespace DigiTactics.Rules;

public interface IMatchShopHooks
{
    PlanningUnit Bought(MatchPlayer player, PlanningUnit unit, int slot, MatchPhase phase);
    void Sold(MatchPlayer player, PlanningUnit unit);
    void Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources);
}

/// <summary>Match shop commands supply mechanic hooks without coupling the pool/economy layer to a mode.</summary>
public sealed class PlanningShopCommands(IMatchShopHooks hooks)
{
    public bool Buy(MatchPlayer player, int slot, MatchPhase phase) => player.Shop.Buy(slot,
        phase == MatchPhase.Combat && player.IsHuman,
        unit => hooks.Bought(player, unit, slot, phase),
        (unit, sources) => hooks.Combined(player, unit, sources));

    public bool Sell(MatchPlayer player, int uid, MatchPhase phase) => player.Shop.Sell(uid,
        phase == MatchPhase.Combat && player.IsHuman, unit => hooks.Sold(player, unit));
}

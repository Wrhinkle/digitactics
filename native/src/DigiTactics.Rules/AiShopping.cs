namespace DigiTactics.Rules;

/// <summary>AI economy and shopping order; mode purchases flow through the shared command hooks.</summary>
public sealed class AiShopping(Ruleset rules, PlanningShopCommands commands)
{
    private readonly PlanningAi planner = new(rules);

    public int LevelTarget(int stage, int round)
    {
        int wanted = rules.Pace == "normal" ? stage switch
        {
            <= 1 => 3,
            2 => round >= 5 ? 5 : 4,
            3 => round >= 2 ? 6 : 5,
            4 => round >= 5 ? 8 : 7,
            5 => 8,
            _ => 9
        } : stage + 2 + (stage >= 4 ? 1 : 0);
        return Math.Min(rules.Economy!.Xp?.MaxLevel ?? 9, wanted);
    }

    public void Level(MatchPlayer player, int stage, int round)
    {
        if (rules.Economy!.Xp is not { } xp) return;
        int target = LevelTarget(stage, round);
        for (int guard = 0; guard < 20 && player.Economy.Level < target && player.Economy.Gold >= xp.BuyCost; guard++)
            player.Economy.BuyXp();
    }

    public int Keep(int stage) => rules.Economy!.Interest is not null
        ? stage <= 3 ? Math.Min(50, 10 * (stage + 1)) : stage <= 5 ? 20 : 0
        : stage <= 2 ? 2 : 0;

    public bool SellWorst(MatchPlayer player, MatchPhase phase) => planner.WorstBench(player) is { } unit && commands.Sell(player, unit.Id, phase);

    public void BuyPass(MatchPlayer player, MatchPhase phase, Func<int> capacity, Func<string, int> partnerCopies)
    {
        bool locked = phase == MatchPhase.Combat && player.IsHuman;
        for (int slot = 0; slot < player.Shop.Slots.Count; slot++)
        {
            if (player.Shop.Slots[slot] is not { } line || !planner.Wants(player, line, capacity(), partnerCopies(line))) continue;
            if (player.Shop.CanBuy(slot, locked) == PurchaseStatus.Ready) commands.Buy(player, slot, phase);
            else if (player.Shop.CanBuy(slot, locked) == PurchaseStatus.Bench)
            {
                SellWorst(player, phase);
                if (player.Shop.CanBuy(slot, locked) == PurchaseStatus.Ready) commands.Buy(player, slot, phase);
            }
        }
    }

    // Called after offer choice and on-AI-turn hooks; portal transfers happen before TrimBench.
    public void Shop(MatchPlayer player, MatchPhase phase, int stage, int round, Func<int> capacity,
        Func<string, int> partnerCopies, Action reroll)
    {
        Level(player, stage, round);
        BuyPass(player, phase, capacity, partnerCopies);
        int keep = Keep(stage);
        for (int rolls = 0; rolls < 15 && (player.Shop.FreeRolls > 0 || player.Economy.Gold - rules.RerollCost >= keep); rolls++)
        {
            reroll();
            BuyPass(player, phase, capacity, partnerCopies);
        }
    }

    public void TrimBench(MatchPlayer player, MatchPhase phase)
    {
        while (player.Roster.Bench.Count(u => u is not null) > 6)
            if (!SellWorst(player, phase)) break;
    }
}

namespace DigiTactics.Rules;

/// <summary>Dragonlands planning snapshots, golden equipment and persistent offer rerolls.</summary>
public sealed class DragonlandsPlanning(MatchState state)
{
    private readonly HashSet<string> golden = state.Rules.Items.Where(i => i.Category == "gold").Select(i => i.Id).ToHashSet();
    private readonly MatchLoot loot = new(state);

    public int RoundEnd(RoundEndContext context)
    {
        var player = context.Player;
        var mechanic = player.Mechanics;
        var rewards = context.Match.Combat?.DragonlandsRewards(context.Team) ?? new(0, 0, 0);
        int gold = 0;
        // Trait callbacks precede player augments and the set mechanic, in count order.
        foreach (var count in context.Traits)
        {
            var trait = state.Rules.Traits.FirstOrDefault(t => t.Id == count.Key);
            if (trait is null) continue;
            int level = TraitRules.Level(trait, count.Value);
            if (level == 0) continue;
            switch (count.Key)
            {
                case "STAR":
                    mechanic.DragonStarRounds++;
                    if (mechanic.DragonStarRounds % 3 == 0)
                    {
                        gold += (int)trait.Parameters["gold"][level - 1];
                        mechanic.DragonStarShop = true;
                    }
                    break;
                case "LAGOON":
                    mechanic.DragonLagoon += rewards.Lagoon;
                    while (mechanic.DragonLagoon >= 20)
                    {
                        mechanic.DragonLagoon -= 20;
                        mechanic.DragonLagoonPaid++;
                        gold += (int)trait.Parameters["gold"][level - 1];
                        if (mechanic.DragonLagoonPaid % 3 == 0) player.GiveItem(loot.RandomComponent()!);
                    }
                    break;
                case "STARCALL":
                    if (rewards.Wish != 0) player.Health = Math.Min(state.Rules.Economy!.StartHp, player.Health + rewards.Wish);
                    break;
                case "CORE":
                    mechanic.DragonDoots = Math.Min(20, mechanic.DragonDoots + rewards.Doots);
                    break;
            }
        }
        foreach (string augment in player.Augments)
        {
            if (augment == "drg_calcloss" && !context.Won && !context.Pve)
            {
                gold += 2;
                mechanic.DragonCalculatedRoll = true;
            }
            if (augment == "drg_training") mechanic.DragonTrainingKills = Math.Min(40, mechanic.DragonTrainingKills + rewards.CtKills);
        }
        gold += rewards.Gold;
        if (rewards.Axe != 0)
        {
            mechanic.DragonAxe += rewards.Axe;
            while (mechanic.DragonAxe >= 50)
            {
                mechanic.DragonAxe -= 50;
                gold += 4;
                player.GiveItem(loot.RandomComponent()!);
            }
        }
        if (rewards.Investor != 0)
        {
            mechanic.DragonInvestor += rewards.Investor;
            if (mechanic.DragonInvestor >= 4)
            {
                mechanic.DragonInvestor = 0;
                if (player.Roster.Entries.Select(e => e.Unit).FirstOrDefault(u => u.Items.Contains("drg_g_investor")) is { } holder)
                {
                    var items = (string[])holder.Items.Clone();
                    items[Array.IndexOf(items, "drg_g_investor")] = "drg_g_diamond";
                    player.Roster.UpdateUnit(holder with { Items = items });
                }
                player.GiveItem("drg_dup");
                gold += 8;
            }
        }
        return gold;
    }

    public void RoundStart(MatchPlayer player)
    {
        var mechanic = player.Mechanics;
        if (player.Augments.Contains("drg_calcloss") && mechanic.DragonCalculatedRoll)
        {
            player.Shop.FreeRolls++;
            mechanic.DragonCalculatedRoll = false;
        }
        mechanic.DragonBank = Math.Clamp(player.Economy.Gold, 0, 15);
        mechanic.DragonHpFull = player.Health >= state.Rules.Economy!.StartHp;
        GoldenArmor(player);
        if (player.Offer is { Kind: "augment" } offer)
        {
            if (mechanic.DragonAugmentOptions is { } previous && !mechanic.DragonAugmentRolled &&
                player.Augments.LastOrDefault() is { } last && !previous.Contains(last)) mechanic.DragonAugmentRolled = true;
            player.Offer = offer with { Rerolls = mechanic.DragonAugmentRolled ? 0 : 1 };
            mechanic.DragonAugmentOptions = (string[])offer.Options.Clone();
        }
        if (player.Offer is { Kind: "drg_treasure" } treasure && player.Augments.Contains("drg_treasure_hoard"))
            player.Offer = treasure with { Rerolls = treasure.Rerolls + 1 };
    }

    private void GoldenArmor(MatchPlayer player)
    {
        player.Items.RemoveAll(golden.Contains);
        foreach (var unit in player.Roster.Bench.OfType<PlanningUnit>().ToArray())
            if (unit.Items.Any(golden.Contains)) player.Roster.UpdateUnit(unit with { Items = unit.Items.Where(id => !golden.Contains(id)).ToArray() });
        var board = player.Roster.Board.Select(e => new CombatEntry(e.Unit.Line, e.Unit.Stage, e.Spot.Column,
            e.Spot.Row, e.Unit.Id, Items: e.Unit.Items, BonusTraits: e.Unit.BonusTraits));
        var counts = TraitRules.Count(state.Rules, board, player.Augments.ToArray());
        var definition = state.Rules.Traits.Single(t => t.Id == "GOLD");
        int level = TraitRules.Level(definition, counts.GetValueOrDefault("GOLD"));
        if (level == 0) return;
        var owned = player.Roster.Board.SelectMany(e => e.Unit.Items.Where(golden.Contains)).ToArray();
        var pool = state.Rules.Items.Where(i => golden.Contains(i.Id) && i.Id != "drg_g_diamond" &&
            (i.Id != "drg_g_crown" || level >= 3) && !owned.Contains(i.Id)).Select(i => i.Id).ToList();
        for (int k = owned.Length; k < definition.Values[level - 1] && pool.Count > 0; k++)
        {
            string id = loot.Pick(pool)!;
            pool.Remove(id);
            player.GiveItem(id);
        }
    }
}


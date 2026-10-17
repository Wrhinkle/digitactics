namespace DigiTactics.Rules;

/// <summary>Dragonlands planning snapshots, golden equipment and persistent offer rerolls.</summary>
public sealed class DragonlandsPlanning(MatchState state)
{
    private readonly HashSet<string> golden = state.Rules.Items.Where(i => i.Category == "gold").Select(i => i.Id).ToHashSet();
    private readonly MatchLoot loot = new(state);

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


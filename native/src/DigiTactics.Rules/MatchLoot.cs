namespace DigiTactics.Rules;

public sealed record LootRules(int ItemCap, string[] StatComponents, string[] SpecialComponents,
    string[] Emblems, string[] Consumables, bool CustomPve, ModeLootRules? Mode = null);
public sealed record LootSpec(string Kind = "blue", int Gold = 0, string[]? Items = null, string? Unit = null);
public sealed record LootOrb(int Id, string Kind, double X, double Z, int Gold, string[] Items, string? Unit);

/// <summary>Reward generation and collection share the match RNG, IDs, inventory and reserved unit pool.</summary>
public sealed class MatchLoot(MatchState state)
{
    private LootRules Rules => state.Rules.Loot!;
    internal string? Pick(IReadOnlyList<string> ids) => ids.Count == 0 ? null : ids[(int)(state.Random.Next() * ids.Count)];
    public string? RandomComponent(bool withSpecial = false) => Pick(withSpecial && state.Random.Next() < .1 &&
        Rules.SpecialComponents.Length > 0 ? Rules.SpecialComponents : Rules.StatComponents);
    public string? RandomCompleted() => Pick(state.Rules.CoreItems);
    public string? RandomEmblem() => Pick(Rules.Emblems);

    public string? RandomEmblem(MatchPlayer player)
    {
        var emblems = state.Rules.Loot!.Emblems;
        if (emblems.Length == 0) return null;
        var board = player.Roster.Board.Select(e => new CombatEntry(e.Unit.Line, e.Unit.Stage, e.Spot.Column,
            e.Spot.Row, e.Unit.Id, Items: e.Unit.Items, BonusTraits: e.Unit.BonusTraits));
        var counts = TraitRules.Count(state.Rules, board);
        var preferred = emblems.Where(id =>
        {
            string? trait = state.Rules.Items.Single(i => i.Id == id).Emblem;
            return trait is not null && counts.GetValueOrDefault(trait) > 0 &&
                state.Rules.Traits.Single(t => t.Id == trait).Kind is not ("role" or "class");
        }).ToArray();
        return Pick(preferred.Length > 0 ? preferred : emblems);
    }

    public PveRewardSummary Reward(MatchPlayer player, RoundPosition round, bool won,
        Func<MatchPlayer, bool, IEnumerable<LootSpec>>? custom = null)
    {
        if (custom is not null) return Drop(player, custom(player, won));
        if (Rules.CustomPve) return Drop(player, new ModePveRewards(state, this).Generate(player, round, won));
        List<LootSpec> specs = [];
        static string[] Item(string? id) => id is null ? [] : [id];
        if (round.Stage == 1)
        {
            specs.Add(new(Items: Item(RandomComponent())));
            if (won)
            {
                bool last = round.Round == state.Rules.Schedule!.RoundsInStage(1);
                specs.Add(new(Gold: last ? 2 : 1));
                if (last)
                {
                    var lines = state.Rules.Lines.Where(l => l.Tier <= 2 && state.Pool.Remaining(l.Id) > 0).Select(l => l.Id).ToArray();
                    if (Pick(lines) is { } line) specs.Add(new("gold", Unit: line));
                }
            }
        }
        else
        {
            specs.Add(new(Gold: won ? 3 : 2));
            if (won && round.Stage >= 4) specs.Add(new("gold", Items: Item(RandomCompleted())));
            if (won && round.Stage >= 4 && state.Random.Next() < .2) specs.Add(new("gold", Items: Item(RandomEmblem())));
            else specs.Add(new(Items: Item(RandomComponent(round.Stage >= 3))));
            if (won && round.Stage >= 3 && state.Random.Next() < .25 && Rules.Consumables.Length > 0)
                specs.Add(new("gold", Items: Item(Pick(Rules.Consumables))));
        }
        return Drop(player, specs);
    }

    public PveRewardSummary Drop(MatchPlayer player, IEnumerable<LootSpec> specs)
    {
        // Custom drops reserve all units before positions consume random numbers.
        var drops = specs.Select(spec =>
        {
            string? line = spec.Unit;
            if (line is not null)
            {
                if (state.Pool.Remaining(line) > 0) state.Pool.Take(line);
                else line = null;
            }
            return spec with { Unit = line, Items = spec.Items?.Where(id => !string.IsNullOrEmpty(id)).ToArray() ?? [] };
        }).ToArray();
        int gold = 0;
        string? unit = null;
        List<string> items = [];
        foreach (var drop in drops)
        {
            var orb = new LootOrb(state.AllocateId(), drop.Kind, .1 + state.Random.Next() * .8,
                .15 + state.Random.Next() * .7, drop.Gold, drop.Items!, drop.Unit);
            gold += orb.Gold;
            items.AddRange(orb.Items);
            if (orb.Unit is not null) unit = orb.Unit;
            player.Orbs.Add(orb);
        }
        if (!player.IsHuman) CollectAll(player);
        return new(gold, unit, items, drops.Length);
    }

    public LootOrb? Collect(MatchPlayer player, int id, bool boardLocked = false)
    {
        int index = player.Orbs.FindIndex(o => o.Id == id);
        if (index < 0) return null;
        var orb = player.Orbs[index];
        player.Orbs.RemoveAt(index);
        player.Economy.Credit(orb.Gold);
        foreach (var item in orb.Items) player.GiveItem(item);
        if (orb.Unit is { } line)
        {
            if (player.Roster.FreeBenchSlot >= 0)
            {
                player.Roster.AddToBench(state.CreateUnit(line, 0));
                player.Roster.Combine(boardLocked, player.GiveItem);
                player.Shop.ClearMaxed();
            }
            else
            {
                state.Pool.Return(line);
                player.Economy.Credit(state.Rules.Lines.Single(l => l.Id == line).Tier);
            }
        }
        return orb;
    }
    public void CollectAll(MatchPlayer player, bool boardLocked = false)
    {
        while (player.Orbs.Count > 0) Collect(player, player.Orbs[0].Id, boardLocked);
    }
}

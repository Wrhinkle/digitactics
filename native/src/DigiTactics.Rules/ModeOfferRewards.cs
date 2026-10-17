using System.Globalization;

namespace DigiTactics.Rules;

/// <summary>Item-producing custom offers; the match retains ownership of RNG, IDs and pool copies.</summary>
public sealed class ModeOfferRewards(MatchState state)
{
    private readonly MatchLoot loot = new(state);
    private ModeLootRules Mode => state.Rules.Loot!.Mode!;
    private double Random() => state.Random.Next();
    private string? Pick(IReadOnlyList<string> ids) => loot.Pick(ids);
    private readonly string[] radiants = state.Rules.Items.Where(i => i.Category == "radiant").Select(i => i.Id).ToArray();

    public string[] RadiantOptions(MatchPlayer player, int count, IReadOnlyList<string> exclude)
    {
        var upgrades = Mode.RadiantOf!;
        var held = player.Roster.Entries.SelectMany(e => e.Unit.Items).Concat(player.Items)
            .Where(upgrades.ContainsKey).Distinct().Select(id => upgrades[id]).Where(id => !exclude.Contains(id)).ToArray();
        for (int i = held.Length - 1; i > 0; i--)
        {
            int j = (int)(Random() * (i + 1));
            (held[i], held[j]) = (held[j], held[i]);
        }
        var result = held.Take(count).ToList();
        var rest = upgrades.Values.Where(id => !result.Contains(id) && !exclude.Contains(id)).ToList();
        while (result.Count < count && rest.Count > 0)
        {
            int index = (int)(Random() * rest.Count);
            result.Add(rest[index]);
            rest.RemoveAt(index);
        }
        return result.ToArray();
    }

    public void GrantRadiant(MatchPlayer player, string id)
    {
        string? original = Mode.BaseOf!.GetValueOrDefault(id);
        foreach (var entry in player.Roster.Entries)
        {
            int index = Array.IndexOf(entry.Unit.Items, original);
            if (index < 0) continue;
            var items = entry.Unit.Items.ToArray();
            items[index] = id;
            player.Roster.UpdateUnit(entry.Unit with { Items = items });
            return;
        }
        int inventory = original is null ? -1 : player.Items.IndexOf(original);
        if (inventory >= 0) player.Items[inventory] = id;
        else player.GiveItem(id);
    }

    public string[]? Armory(MatchPlayer player, RoundPosition round)
    {
        if (Mode.Policy == "arcane")
        {
            List<string?> result = [];
            for (int guard = 0; result.Count < 3 && guard < 60; guard++)
            {
                string? id = round.Stage <= 3 ? loot.RandomComponent(true) : loot.RandomCompleted();
                if (id is not null && !result.Contains(id)) result.Add(id);
            }
            void Set(int index, string? id) { while (result.Count <= index) result.Add(null); result[index] = id; }
            if (round.Stage >= 3 && Random() < .2) Set(2, Pick(["arc_remover", "arc_reforger", "arc_copier_small"]));
            if (round.Stage >= 3 && Random() < .25 && loot.RandomEmblem() is { } emblem) Set(1, emblem);
            if (round.Stage >= 5) Set(0, Pick(Mode.Artifacts));
            return result.OfType<string>().ToArray();
        }
        if (Mode.Policy != "reckoning") return null;
        List<string> options = [];
        void Add(Func<string?> generate)
        {
            for (int attempt = 0; attempt < 20; attempt++)
                if (generate() is { } id && !options.Contains(id)) { options.Add(id); return; }
        }
        string? Normal() => Pick(state.Rules.Loot!.StatComponents);
        string? Shadow() => Pick(Mode.ShadowComponents);
        string? Component() => Random() < .4 ? Shadow() : Normal();
        string? Completed()
        {
            string? id = loot.RandomCompleted();
            return Random() < Mode.ShadowChance && id is not null ? Mode.ShadowOf.GetValueOrDefault(id, id) : id;
        }
        if (round.Stage <= 2) { Add(Shadow); Add(Normal); Add(Normal); }
        else if (round.Stage <= 3)
        {
            Add(Component); Add(Component);
            Add(() => Random() < .4 ? "rec_tome" : Random() < .5 ? loot.RandomEmblem(player) : Normal());
        }
        else { Add(Completed); Add(Completed); Add(() => Random() < .2 ? "rec_tome" : Completed()); }
        return options.ToArray();
    }

    public string[] TreasureOptions(MatchPlayer player, IReadOnlyList<string> exclude)
    {
        string version = string.IsNullOrEmpty(state.TreasureVariant) ? "classic" : state.TreasureVariant;
        bool won = player.Mechanics.DragonTreasureWon != false;
        int count = (won ? 4 : 3) + (player.Augments.Contains("drg_treasure_hoard") ? 2 : 0);
        List<string> options = [];
        bool Add(string id)
        {
            if (options.Contains(id) || exclude.Contains(id)) return false;
            options.Add(id);
            return true;
        }
        if (version == "order") for (int k = 0; k < 6; k++) if (Add(Bundle(player, "rad"))) break;
        string[] kinds = ["gold", "comp", "item", "item", "emblem", "dup", "unit", "rad"];
        for (int guard = 0; options.Count < count && guard < 60; guard++)
        {
            string kind = version == "chaos" && Random() < .5 ? "chaos"
                : kinds[(int)(Random() * (won ? kinds.Length : kinds.Length - 1))];
            Add(Bundle(player, kind == "rad" && version == "classic" && Random() < .6 ? "item" : kind));
        }
        return options.ToArray();
    }
    private string Bundle(MatchPlayer player, string kind)
    {
        switch (kind)
        {
            case "gold": return $"gold:{9 + (int)(Random() * 4)}";
            case "comp": return $"comp:{loot.RandomComponent()}.{loot.RandomComponent()}.{loot.RandomComponent()}";
            case "item": return $"item:{loot.RandomCompleted()}.{loot.RandomComponent()}";
            case "emblem": return $"emblem:{loot.RandomEmblem(player)}";
            case "dup": return "dup:5";
            case "unit":
                var lines = state.Rules.Lines.Where(l => l.Tier == 5 && l.Slots != 2 && state.Pool.Remaining(l.Id) > 0).Select(l => l.Id).ToArray();
                return lines.Length > 0 ? $"unit:{Pick(lines)}" : "gold:10";
            case "rad": return $"rad:{Pick(radiants)}";
            default: return $"chaos:{1 + (int)(Random() * 9999)}";
        }
    }

    public void ChooseTreasure(MatchPlayer player, string id, bool boardLocked)
    {
        var bundle = id.Split(':');
        var parts = bundle.Length > 1 ? bundle[1].Split('.', StringSplitOptions.RemoveEmptyEntries) : [];
        int gold = parts.Length > 0 && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : 0;
        switch (bundle[0])
        {
            case "gold": player.Economy.Credit(gold); break;
            case "comp": case "item": case "emblem": case "rad":
                foreach (var item in parts) if (state.Rules.Items.Any(i => i.Id == item)) player.GiveItem(item);
                break;
            case "dup": player.GiveItem("drg_dup"); player.Economy.Credit(gold); break;
            case "unit":
                if (parts.Length > 0 && state.Rules.Lines.FirstOrDefault(l => l.Id == parts[0]) is { } line)
                {
                    if (state.Pool.Remaining(line.Id) > 0 && player.Roster.FreeBenchSlot >= 0)
                    {
                        state.Pool.Take(line.Id);
                        player.Roster.AddToBench(state.CreateUnit(line.Id, 0));
                        player.Roster.Combine(boardLocked, player.GiveItem);
                        player.Shop.ClearMaxed();
                    }
                    else player.Economy.Credit(line.Price);
                }
                player.Economy.Credit(2);
                break;
            case "chaos":
                if (Random() < .5)
                {
                    if (Pick(radiants) is { } radiant) player.GiveItem(radiant);
                    if (loot.RandomCompleted() is { } item) player.GiveItem(item);
                }
                else player.Economy.Credit(4);
                break;
        }
    }
}

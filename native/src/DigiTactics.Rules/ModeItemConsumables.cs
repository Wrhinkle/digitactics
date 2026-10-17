namespace DigiTactics.Rules;

/// <summary>Mode-specific inventory transformations using the owning match's random stream.</summary>
public sealed class ModeItemConsumables(MatchState state)
{
    private readonly Dictionary<string, ItemDefinition> items = state.Rules.Items.ToDictionary(i => i.Id);
    private LootRules Rules => state.Rules.Loot!;
    private string Policy => Rules.Mode!.Policy;

    /// <returns>Null delegates an unimplemented mechanic to the required match hooks.</returns>
    public string? TryUse(MatchPlayer player, int index, PlanningUnit unit)
    {
        string id = player.Items[index];
        if (id is "remover" or "giz_remover" or "gal_remover" or "fat_remover" or
            "drg_remover" or "arc_remover" or "col_remover" or "rec_remover")
            return GiveBack(player, index, unit, false);
        if (id is "reforger" or "giz_reforger" or "gal_reforger" or "fat_reforger" or
            "drg_reforger" or "arc_reforger" or "col_reforger")
            return GiveBack(player, index, unit, true);
        if (id != "giz_ockit") return null;
        int slot = Array.FindIndex(unit.Items, held => items[held].Category == "completed" &&
            items.ContainsKey(held.Replace("giz_", "giz_oc_", StringComparison.Ordinal)));
        if (slot < 0) return "noItems";
        var equipped = unit.Items.ToArray();
        equipped[slot] = equipped[slot].Replace("giz_", "giz_oc_", StringComparison.Ordinal);
        player.Items.RemoveAt(index);
        player.Roster.UpdateUnit(unit with { Items = equipped });
        return id;
    }

    private string GiveBack(MatchPlayer player, int index, PlanningUnit unit, bool reforge)
    {
        if (unit.Items.Length == 0) return "noItems";
        if (player.Items.Count - 1 + unit.Items.Length > Rules.ItemCap) return "benchFull";
        string id = player.Items[index];
        player.Items.RemoveAt(index);
        var returned = reforge ? unit.Items.Select(Reforge).ToArray() : unit.Items;
        player.Roster.UpdateUnit(unit with { Items = [] });
        foreach (string? item in returned) if (item is not null) player.GiveItem(item);
        return id;
    }

    private string? Reforge(string id)
    {
        var item = items[id];
        string? Pick(IEnumerable<string> candidates)
        {
            var choices = candidates.Where(x => x != id).ToArray();
            int index = (int)(state.Random.Next() * choices.Length);
            return choices.Length == 0 ? null : choices[index];
        }
        if (item.Component)
        {
            bool preserve = Policy switch
            {
                "faction-wars" or "dragonlands" => id == "egg",
                "fates" => id == "fat_egg",
                _ => item.Special,
            };
            return preserve ? id : Pick(Rules.StatComponents);
        }
        if (item.Emblem is not null) return Pick(Rules.Emblems);
        if (Policy is "fates" or "coliseum" && item.Category == "artifact") return Pick(Rules.Mode!.Artifacts);
        if (Policy is "gizmos" or "dragonlands" && item.Category == "radiant")
            return Pick(state.Rules.Items.Where(i => i.Category == "radiant").Select(i => i.Id));
        if (Policy == "arcane" && item.Category is "artifact" or "contraband")
            return Pick(state.Rules.Items.Where(i => i.Category == item.Category &&
                (item.Category != "contraband" || i.Tier == item.Tier)).Select(i => i.Id)) ?? id;
        bool keep = Policy switch
        {
            "faction-wars" => id == "golden_egg",
            "fates" => id == "fat_golden_egg",
            "dragonlands" => item.Category != "completed" || item.NoDrop,
            _ => item.Category == "special",
        };
        return keep ? id : Pick(state.Rules.CoreItems);
    }
}

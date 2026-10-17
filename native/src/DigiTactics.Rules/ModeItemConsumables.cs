using System.Text.Json;

namespace DigiTactics.Rules;

/// <summary>Mode-specific inventory transformations using the owning match's random stream.</summary>
public sealed class ModeItemConsumables(MatchState state)
{
    private readonly Dictionary<string, ItemDefinition> items = state.Rules.Items.ToDictionary(i => i.Id);
    private readonly Dictionary<string, UnitLine> shopLines = state.Rules.Lines.ToDictionary(l => l.Id);
    private LootRules Rules => state.Rules.Loot!;
    private string Policy => Rules.Mode!.Policy;

    /// <returns>Null delegates an unimplemented mechanic to the required match hooks.</returns>
    public string? TryUse(MatchPlayer player, int index, PlanningUnit unit, MatchPhase phase, bool boardLocked,
        Action<PlanningUnit, IReadOnlyList<PlanningUnit>> onCombine)
    {
        string id = player.Items[index];
        if (id == "fat_dice")
        {
            var line = state.Rules.Lines.Concat(state.Rules.ExtraLines).Single(l => l.Id == unit.Line);
            if (!player.Shop.RefreshMatchingTraits(line.Traits)) return "none";
            player.Items.RemoveAt(index);
            return id;
        }
        if (id is "fat_holy" or "fat_dark")
        {
            if (unit.Line != "fat_lopmon" || !string.IsNullOrEmpty(player.Mechanics.FatesPath)) return "none";
            player.Items.RemoveAt(index);
            player.Items.Remove(id == "fat_holy" ? "fat_dark" : "fat_holy");
            player.Mechanics.FatesPath = id == "fat_holy" ? "holy" : "dark";
            return id;
        }
        if (id == "rec_tome")
        {
            if (phase != MatchPhase.Planning) return "phase";
            if (player.Offer is not null) return "busy";
            player.Items.RemoveAt(index);
            var options = Rules.Emblems.ToArray();
            for (int i = options.Length - 1; i > 0; i--)
            {
                int j = (int)(state.Random.Next() * (i + 1));
                (options[i], options[j]) = (options[j], options[i]);
            }
            player.Offer = new("rec_tome", options.Take(4).ToArray(), 0, null);
            return id;
        }
        if (id == "col_extractor")
        {
            if (unit.Mods is not { } mods || !mods.TryGetProperty("pu", out var power) || !Truthy(power)) return "none";
            player.Items.RemoveAt(index);
            var remaining = mods.EnumerateObject().Where(p => p.Name != "pu").ToDictionary(p => p.Name, p => p.Value);
            player.Roster.UpdateUnit(unit with { Mods = remaining.Count == 0 ? null : JsonSerializer.SerializeToElement(remaining) });
            if (player.Items.Count < Rules.ItemCap) player.GiveItem("col_core");
            else player.Mechanics.ColiseumCores++;
            return "extractor";
        }
        if (id is "giz_copy" or "gal_copier" or "fat_copy" or "drg_dup" or "drg_dup_lesser" or
            "arc_copier" or "arc_copier_small" or "ac_digiegg")
            return Copy(player, index, unit, boardLocked, onCombine);
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

    private static bool Truthy(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined or JsonValueKind.False => false,
        JsonValueKind.String => value.GetString()!.Length != 0,
        JsonValueKind.Number => value.GetDouble() != 0,
        _ => true,
    };

    private string Copy(MatchPlayer player, int index, PlanningUnit target, bool boardLocked,
        Action<PlanningUnit, IReadOnlyList<PlanningUnit>> onCombine)
    {
        string id = player.Items[index];
        int tierLimit = id switch { "giz_copy" => 4, "drg_dup_lesser" or "arc_copier_small" => 3,
            "drg_dup" or "arc_copier" => 5, _ => int.MaxValue };
        if (!shopLines.TryGetValue(target.Line, out var line) || line.Tier > tierLimit) return "none";
        bool egg = id == "ac_digiegg";
        if (egg && target.Stage >= 2) return "maxed";
        int stage = egg ? target.Stage : 0;
        int need = egg ? (int)Math.Pow(line.Copies, stage) : 1;
        int free = player.Roster.FreeBenchSlot;
        if (id == "giz_copy" && free < 0) return "benchFull";
        if (state.Pool.Remaining(line.Id) < need) return egg ? "poolEmpty" : "none";
        // Galaxies allocates an ID even when the subsequent bench check rejects the copy.
        PlanningUnit? copy = id == "gal_copier" ? state.CreateUnit(line.Id, stage) : null;
        if (free < 0)
        {
            bool canOverflow = id == "gal_copier" || egg;
            int same = player.Roster.Entries.Count(e => e.Unit.Line == line.Id && e.Unit.Stage == stage);
            if (!canOverflow || same < line.Copies - 1)
                return id is "gal_copier" or "fat_copy" ? "none" : "benchFull";
        }
        player.Items.RemoveAt(index);
        state.Pool.Take(line.Id, need);
        copy ??= state.CreateUnit(line.Id, stage);
        if (free >= 0) player.Roster.AddToBench(copy);
        // The original full-bench check includes locked board units; combine excludes them.
        // In that case an unmerged extra is discarded, with the pool reservation retained.
        player.Roster.Combine(boardLocked, player.GiveItem, free < 0 ? copy : null,
            onCombine: onCombine, discardOverflow: true);
        player.Shop.ClearMaxed();
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

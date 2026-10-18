namespace DigiTactics.Rules;

public interface IMatchItemHooks
{
    string UseConsumable(MatchPlayer player, int itemIndex, PlanningUnit unit);
    string? EquipVeto(MatchPlayer player, PlanningUnit unit, string item, string? combined);
    void Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources);
}

/// <summary>Inventory commands preserve rejection order and only combine components on a unit.</summary>
public sealed class PlanningItems(Ruleset rules, IMatchItemHooks hooks, ModeItemConsumables? consumables = null)
{
    public PlanningItems(MatchState state, IMatchItemHooks hooks) : this(state.Rules, hooks, new(state)) { }
    private readonly Dictionary<string, ItemDefinition> items = rules.Items.ToDictionary(i => i.Id);
    private readonly Dictionary<string, UnitLine> lines = rules.Lines.Concat(rules.ExtraLines).ToDictionary(l => l.Id);

    public string? Combine(string a, string b) => rules.Recipes!.GetValueOrDefault(
        string.CompareOrdinal(a, b) <= 0 ? $"{a}+{b}" : $"{b}+{a}");

    public (int First, int Second)? RecipeFrom(IReadOnlyList<string> inventory, string id)
    {
        if (!items.TryGetValue(id, out var item) || item.Ingredients is not { Length: 2 } ingredients) return null;
        int first = -1, second = -1;
        for (int i = 0; i < inventory.Count; i++) if (inventory[i] == ingredients[0]) { first = i; break; }
        if (first < 0) return null;
        for (int i = 0; i < inventory.Count; i++) if (i != first && inventory[i] == ingredients[1]) { second = i; break; }
        return second < 0 ? null : (first, second);
    }

    public bool Craft(MatchPlayer player, string id)
    {
        if (RecipeFrom(player.Items, id) is not { } recipe) return false;
        player.Items.RemoveAt(Math.Max(recipe.First, recipe.Second));
        player.Items.RemoveAt(Math.Min(recipe.First, recipe.Second));
        player.Items.Add(id);
        return true;
    }

    public bool GiveFromBench(MatchPlayer player, string id, int uid)
    {
        int index = player.Items.IndexOf(id);
        var unit = player.Roster.Entries.FirstOrDefault(e => e.Unit.Id == uid)?.Unit;
        if (index < 0 || unit is null || !CanHold(player, unit, id)) return false;
        player.Items.RemoveAt(index);
        player.Roster.UpdateUnit(unit with { Items = [..unit.Items, id] });
        return true;
    }
    public bool Swap(MatchPlayer player, int a, int b)
    {
        if (a == b || (uint)a >= player.Items.Count || (uint)b >= player.Items.Count) return false;
        (player.Items[a], player.Items[b]) = (player.Items[b], player.Items[a]);
        return true;
    }
    public bool HoldsItems(PlanningUnit unit) => !unit.NoItems && !lines[unit.Line].NoItems;
    public int SlotsUsed(PlanningUnit unit) => ItemCapacity.Used(unit.Items, items);
    public bool CanFit(PlanningUnit unit, string item, int replacedIndex = -1) =>
        ItemCapacity.Fits(unit.Items.Where((_, index) => index != replacedIndex), item, items);
    public bool CanHold(MatchPlayer player, PlanningUnit unit, string item) =>
        HoldsItems(unit) && Veto(player, unit, item, null) is null;

    // Mode AI can use a consumable directly; unlike Equip this does not reject a locked board target.
    // The consumable's own combine operation still observes combat locks.
    public string UseConsumable(MatchPlayer player, int itemIndex, int uid, MatchPhase phase)
    {
        var unit = player.Roster.Entries.FirstOrDefault(e => e.Unit.Id == uid)?.Unit;
        if ((uint)itemIndex >= player.Items.Count || unit is null || !items.TryGetValue(player.Items[itemIndex], out var item) || !item.Consumable) return "none";
        return consumables?.TryUse(player, itemIndex, unit, phase, phase == MatchPhase.Combat && player.IsHuman,
            (up, from) => hooks.Combined(player, up, from)) ?? hooks.UseConsumable(player, itemIndex, unit);
    }

    public string Equip(MatchPlayer player, int itemIndex, int uid, MatchPhase phase)
    {
        bool boardLocked = phase == MatchPhase.Combat && player.IsHuman;
        var entry = player.Roster.Entries.FirstOrDefault(e => e.Unit.Id == uid);
        if ((uint)itemIndex >= player.Items.Count || entry is null) return "none";
        if (boardLocked && entry.Spot.IsBoard) return "locked";
        string id = player.Items[itemIndex];
        if (!items.TryGetValue(id, out var item)) return "none";
        var unit = entry.Unit;
        if (item.Consumable) return UseConsumable(player, itemIndex, uid, phase);
        if (item.Category == "global") { player.Items.RemoveAt(itemIndex); player.Globals.Add(id); return id; }
        if (item.Emblem is { } trait && Traits(unit).Contains(trait)) return "hasTrait";
        if (!HoldsItems(unit)) return "cantHold";
        int component = item.Component ? Array.FindIndex(unit.Items, held => IsComponent(held) && Combine(held, id) is not null) : -1;
        string? made = component >= 0 ? Combine(unit.Items[component], id) : null;
        if (Veto(player, unit, id, made) is { } veto) return veto;
        if (made is not null)
        {
            var remaining = unit.Items.Where((_, index) => index != component).ToArray();
            if (items[made].Emblem is { } emblem && Traits(unit with { Items = remaining }).Contains(emblem)) return "hasTrait";
            var equipped = unit.Items.ToArray();
            equipped[component] = made;
            player.Roster.UpdateUnit(unit with { Items = equipped });
            player.Items.RemoveAt(itemIndex);
            return made;
        }
        if (unit.Items.Length >= 3) return "full";
        player.Roster.UpdateUnit(unit with { Items = [..unit.Items, id] });
        player.Items.RemoveAt(itemIndex);
        return id;
    }
    private bool IsComponent(string id) => items.TryGetValue(id, out var item) && item.Component;
    private string[] Traits(PlanningUnit unit) => TraitRules.UnitTraits(new(unit.Line, unit.Stage, 0, 4, Items: unit.Items), rules);
    private string? Veto(MatchPlayer player, PlanningUnit unit, string id, string? combined)
    {
        if (rules.Id.StartsWith("underlords", StringComparison.Ordinal) && unit.Items.Length >= 1 && combined is null) return "full";
        if (hooks.EquipVeto(player, unit, id, combined) is { } veto) return veto;
        int component = combined is null ? -1 : Array.FindIndex(unit.Items, held => IsComponent(held) && Combine(held, id) == combined);
        return CanFit(unit, combined ?? id, component) ? null : "full";
    }
}

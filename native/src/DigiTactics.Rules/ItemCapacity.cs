namespace DigiTactics.Rules;

/// <summary>Random-item gloves reserve the two slots their combat rolls will fill.</summary>
public static class ItemCapacity
{
    public static int Slots(ItemDefinition item) => item.Passives.TryGetValue("thief", out var thief) && thief.GetBoolean() ? 3 : 1;
    public static int Used(IEnumerable<string> held, IReadOnlyDictionary<string, ItemDefinition> definitions) =>
        held.Sum(id => definitions.TryGetValue(id, out var item) ? Slots(item) : 1);
    public static bool Fits(IEnumerable<string> held, string id, IReadOnlyDictionary<string, ItemDefinition> definitions) =>
        Used(held, definitions) + (definitions.TryGetValue(id, out var item) ? Slots(item) : 1) <= 3;
    public static string[] Fit(IEnumerable<string> carried, IReadOnlyDictionary<string, ItemDefinition> definitions, Action<string>? overflow = null)
    {
        List<string> kept = [];
        foreach (string id in carried)
            if (Fits(kept, id, definitions)) kept.Add(id); else overflow?.Invoke(id);
        return kept.ToArray();
    }
    public static IEnumerable<ItemDefinition> Equipped(IEnumerable<string> carried, IReadOnlyList<ItemDefinition> definitions)
    {
        int used = 0;
        foreach (string id in carried)
        {
            var item = definitions.FirstOrDefault(i => i.Id == id);
            int slots = item is null ? 1 : Slots(item);
            if (used + slots > 3) continue;
            used += slots;
            if (item is not null) yield return item;
        }
    }
}

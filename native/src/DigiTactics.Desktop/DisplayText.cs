using System.Text.Json;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed record DisplayEntry(string Name, string Description, string? Preview = null);
internal sealed class DisplayText
{
    private readonly Dictionary<string, Dictionary<string, DisplayEntry>> variants = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, DisplayEntry>>>(
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "content/display.json")), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    public DisplayEntry Get(string variant, string id) => variants[variant].GetValueOrDefault(id) ?? new(id, "");
    public DisplayEntry Ability(string variant, string line, int stage) => variants[variant][$"ability:{line}:{stage}"];
    public string ConsumablePreview(Ruleset rules, string id, int heldItems, int benchItems)
    {
        string? kind = Get(rules.Id, id).Preview;
        if (kind is not ("remover" or "reforger")) return Get(rules.Id, "consumable:" + id).Description;
        if (heldItems == 0) return "No items to remove";
        if (benchItems - 1 + heldItems > rules.Loot!.ItemCap) return "Not enough room on the item bench";
        string count = heldItems + (heldItems == 1 ? " item" : " items");
        return kind == "remover" ? $"Returns {count} to your bench" : $"Rerolls {count} into new items of the same kind";
    }
    public DisplayEntry Offer(string variant, string kind) => variants[variant].GetValueOrDefault("offer:" + kind) ??
        new(kind switch { "armory" => "Armory", "augment" => "Augment", "power" => "Tamer Power", "itemTier" => "Item Pick", _ => kind }, "Pick one.");

    public DisplayEntry Choice(Ruleset rules, string kind, string id)
    {
        if (variants[rules.Id].TryGetValue($"choice:{kind}:{id}", out var entry)) return entry;
        var parts = id.Split(':', 2); string value = parts.Length > 1 ? parts[1] : "";
        if (kind == "arcMarket" && parts[0] == "arc_pass")
            return new("Pass", $"Your Syndicate Digimon gain {value} max HP for the rest of the game.");
        if (kind == "col_gambit" && int.TryParse(value, out int gems))
            return parts[0] == "double"
                ? new("Double Down", $"Keep your {gems} Gem Power. For the next 3 player combats, losses earn twice as much, but a win halves it and cashes out.")
                : new("Cash Out", $"Turn {gems} Gem Power into about {UnitValues.Round(gems * .5)} gold{(gems >= 80 ? " and 2 completed items" : gems >= 45 ? " and a completed item" : gems >= 25 ? " and a component" : "")}.");
        if (kind != "drg_treasure") return Get(rules.Id, id);
        var items = value.Split('.');
        string Item(int i) => i < items.Length ? Get(rules.Id, items[i]).Name : "item";
        return parts[0] switch
        {
            "gold" => new($"{value} Gold", $"A pile of {value} gold."),
            "comp" => new("Component Hoard", string.Join(", ", items.Select(i => Get(rules.Id, i).Name))),
            "item" => new(Item(0), $"{Item(0)} and a {Item(1)}."),
            "emblem" => new(Item(0), "A Crest for your board."),
            "dup" => new("Duplicator Stash", $"A Champion Duplicator and {value} gold."),
            "unit" => new(rules.Lines.FirstOrDefault(l => l.Id == value)?.Forms[0] ?? "Digimon", "A 5-cost Digimon for your bench and 2 gold."),
            "rad" => new(Item(0), "A Shining item."),
            _ => new("Mystery Hoard", "Half the time a Shining item and a completed item. Otherwise 4 gold.")
        };
    }
}

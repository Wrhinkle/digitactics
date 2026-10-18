using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed record CosmeticItem(string Id, string Name, string? Desc, string? Rarity, int Price, bool Free, bool EggOnly, bool Secret, string[]? Linked, bool? Egg, JsonElement Data = default, string? Tag = null, string? LootPool = null);

/// <summary>A private editable snapshot. Persist Json only after an operation succeeds.</summary>
public sealed class CosmeticCollection
{
    public static readonly string[] Kinds = ["legend", "emote", "board", "killscreen", "dance", "sound", "elimsound"];
    private static readonly Dictionary<string, CosmeticItem[]> Catalog = Load();
    private readonly JsonObject save;
    private readonly JsonObject owned, equipped;
    public string Json => save.ToJsonString();
    public long Bits => save["bits"]!.GetValue<long>();
    public static IReadOnlyList<CosmeticItem> Items(string kind) => Catalog.GetValueOrDefault(kind) ?? [];
    public static bool Multi(string kind) => kind is "emote" or "sound";
    public static CosmeticItem? Item(string kind, string? id) => Items(kind).FirstOrDefault(it => it.Id == id);
    private static Dictionary<string, CosmeticItem[]> Load()
    {
        using var stream = typeof(CosmeticCollection).Assembly.GetManifestResourceStream("DigiTactics.Storage.cosmetics.json")!;
        return JsonSerializer.Deserialize<Dictionary<string, CosmeticItem[]>>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
    public CosmeticCollection(string? json)
    {
        save = new JsonObject { ["bits"] = 0L, ["owned"] = new JsonObject(), ["equipped"] = new JsonObject(), ["history"] = new JsonArray() };
        if (json is not null)
        {
            var previous = JsonNode.Parse(json) as JsonObject ?? throw new InvalidDataException("Invalid cosmetic collection.");
            foreach (var field in previous) save[field.Key] = field.Value?.DeepClone();
        }
        owned = save["owned"] as JsonObject ?? throw new InvalidDataException("Invalid cosmetic ownership.");
        equipped = save["equipped"] as JsonObject ?? throw new InvalidDataException("Invalid cosmetic equipment.");
        if (save["bits"] is null || Bits < 0 || save["history"] is not JsonArray) throw new InvalidDataException("Invalid cosmetic wallet.");
        foreach (string kind in Kinds)
        {
            if (owned[kind] is null) owned[kind] = new JsonArray();
            if (owned[kind] is not JsonArray) throw new InvalidDataException("Invalid owned " + kind);
        }
    }
    public bool Owns(string kind, string? id) => Owns(kind, id, new HashSet<string>(StringComparer.Ordinal));
    private bool Owns(string kind, string? id, HashSet<string> visited)
    {
        var item = Item(kind, id);
        if (item is null) return false;
        if (!visited.Add(kind + ":" + id)) throw new InvalidDataException("Cyclic linked cosmetic.");
        return item.Linked is { Length: 2 } link ? Owns(link[0], link[1], visited)
            : item.Free || owned[kind]!.AsArray().Any(x => x?.GetValue<string>() == id);
    }
    public JsonNode? Equipped(string kind)
    {
        if (Multi(kind))
        {
            if (equipped[kind] is JsonArray current) return new JsonArray(current.Select(x => Owns(kind, x?.GetValue<string>()) ? x?.DeepClone() : null).ToArray());
            var free = Items(kind).Where(x => x.Free).Take(6).Select(x => (JsonNode?)JsonValue.Create(x.Id)).ToList();
            while (free.Count < 6) free.Add(null);
            return new JsonArray(free.ToArray());
        }
        string? id = equipped[kind]?.GetValue<string>();
        return JsonValue.Create(Owns(kind, id) ? id : Items(kind).FirstOrDefault(x => x.Free)?.Id);
    }
    public bool Grant(string kind, string? id)
    {
        if (Item(kind, id) is null) return false;
        if (!owned[kind]!.AsArray().Any(x => x?.GetValue<string>() == id)) owned[kind]!.AsArray().Add(id);
        return true;
    }
    public bool Equip(string kind, string? id, int? slot = null)
    {
        if (!Kinds.Contains(kind) || id is not null && !Owns(kind, id)) return false;
        if (Multi(kind))
        {
            var wheel = Equipped(kind)!.AsArray();
            int empty = wheel.ToList().FindIndex(x => x is null);
            int index = slot ?? Math.Max(0, empty);
            if (index < 0 || index >= 6) return false;
            int duplicate = wheel.ToList().FindIndex(x => x?.GetValue<string>() == id);
            while (wheel.Count <= index) wheel.Add(null);
            if (duplicate >= 0 && id is not null) wheel[duplicate] = wheel[index]?.DeepClone();
            wheel[index] = id; equipped[kind] = wheel;
        }
        else equipped[kind] = id;
        return true;
    }
    public static (string Kind,CosmeticItem Item)[] Featured(DateOnly day)
    {
        static uint Hash(string s) { uint h = 2166136261; foreach (char c in s) h = unchecked((h ^ c) * 16777619); return h; }
        string date = day.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        return Kinds.SelectMany(k => Items(k).Where(x => !x.Secret && !x.Free && x.Price > 0 && !x.EggOnly).Select(x => (Kind: k, Item: x)))
            .OrderBy(x => Hash($"{date}:{x.Kind}:{x.Item.Id}")).Take(4).ToArray();
    }
    public static int Price(string kind, CosmeticItem item, DateOnly day)
    {
        if (item.Free) return 0;
        var featured=Featured(day);
        return featured.Any(x => x.Kind == kind && x.Item.Id == item.Id) ? Math.Max(5, (int)Math.Floor(item.Price * .75 / 5 + .5) * 5) : item.Price;
    }
    public bool Buy(string kind, string id, DateOnly day)
    {
        var item = Item(kind, id);
        if (item is null || item.Secret || item.EggOnly || item.Linked is not null || Owns(kind, id)) return false;
        int price = Price(kind, item, day);
        if (price < 0 || price > Bits) return false;
        save["bits"] = Bits - price;
        return Grant(kind, id);
    }
}

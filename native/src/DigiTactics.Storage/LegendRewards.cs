using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed record LegendReward(string Legend, int Data, int Level);
public sealed record LegendDuplicate(int Data, int Level, int Levels, int ExtraData);
public sealed record LegendDuplicateUpdate(string? Save, LegendDuplicate? Award);
public sealed record LegendRewardUpdate(string? Save, LegendReward? Award);

/// <summary>Original match and duplicate-legend progression, calculated without modifying the profile.</summary>
public static class LegendRewards
{
    private sealed record Legend(string Id, bool Free, string[] Reach);
    private sealed record Catalog(Legend[] Legends, int[] DataFor, int GameData, int Top4Data, Dictionary<string, int> DupData);
    private static readonly Catalog Content = Load();
    private static Catalog Load()
    {
        using var stream = typeof(LegendRewards).Assembly.GetManifestResourceStream("DigiTactics.Storage.legend-rewards.json")!;
        return JsonSerializer.Deserialize<Catalog>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
    private static JsonObject ReadSave(string? saved)
    {
        var save = saved is null ? new JsonObject { ["v"] = 1, ["legends"] = new JsonObject(), ["games"] = new JsonArray() }
            : JsonNode.Parse(saved)?.AsObject() ?? throw new InvalidDataException("Invalid legend save.");
        if (save["v"]?.GetValue<int>() != 1) throw new InvalidDataException("Unsupported legend save version.");
        return save;
    }
    private static JsonObject Record(JsonObject save, Legend legend)
    {
        var legends = save["legends"] as JsonObject ?? throw new InvalidDataException("Invalid legend records.");
        if (legends[legend.Id] is null) legends[legend.Id] = new JsonObject {
            ["xp"] = 0, ["form"] = legend.Id, ["path"] = new JsonArray(), ["nonce"] = 0,
            ["dex"] = new JsonArray(legend.Id), ["care"] = new JsonObject { ["bond"] = 0, ["battle"] = 0, ["food"] = 0 }
        };
        var record = legends[legend.Id]!.AsObject();
        if (record["care"] is null) record["care"] = new JsonObject { ["bond"] = 0, ["battle"] = 0, ["food"] = 0 };
        if (record["path"] is not JsonArray) record["path"] = new JsonArray();
        if (record["dex"] is not JsonArray) record["dex"] = new JsonArray(legend.Id);
        if (!legend.Reach.Contains(record["form"]?.GetValue<string>())) { record["form"] = legend.Id; record["path"] = new JsonArray(); }
        return record;
    }
    private static int Number(JsonObject obj, string name)
    {
        int value = obj[name]?.GetValue<int>() ?? throw new InvalidDataException("Missing legend " + name);
        return value >= 0 ? value : throw new InvalidDataException("Invalid legend " + name);
    }
    internal static (JsonObject Save, JsonObject Record) Snapshot(string? saved, string id)
    {
        var legend = Content.Legends.FirstOrDefault(x => x.Id == id) ?? throw new InvalidDataException("Unknown legend.");
        var save = ReadSave(saved);
        return (save, Record(save, legend));
    }
    private static int Level(int xp)
    {
        int level = 1;
        while (level < Content.DataFor.Length && xp >= Content.DataFor[level]) xp -= Content.DataFor[level++];
        return level;
    }
    public static LegendDuplicateUpdate Duplicate(string? saved, string id)
    {
        var legend = Content.Legends.FirstOrDefault(x => x.Id == id);
        if (legend is null) return new(saved, null);
        var save = ReadSave(saved);
        var record = Record(save, legend);
        int xp = Number(record, "xp"), maximum = Content.DataFor.Sum();
        if (xp >= maximum) return new(saved, null);
        int data = Content.DupData.GetValueOrDefault(CosmeticCollection.Item("legend", id)?.Rarity ?? "common", Content.DupData["common"]);
        int used = Math.Min(maximum - xp, data), before = Level(xp), level = Level(xp + used);
        record["xp"] = xp + used;
        return new(save.ToJsonString(), new(used, level, level - before, data - used));
    }
    public static LegendRewardUpdate Calculate(string? saved, string walletJson, string key, int place)
    {
        if (place is < 1 or > 8) throw new InvalidDataException("Invalid legend reward placement.");
        var save = ReadSave(saved);
        var games = save["games"] as JsonArray ?? throw new InvalidDataException("Invalid legend match history.");
        if (games.Any(x => x?.GetValue<string>() == key)) return new(saved, null);
        var wallet = JsonNode.Parse(walletJson)!.AsObject();
        string? equipped = wallet["equipped"]?["legend"]?.GetValue<string>();
        var legend = Content.Legends.FirstOrDefault(x => x.Id == equipped && (x.Free || wallet["owned"]?["legend"]?.AsArray().Any(id => id?.GetValue<string>() == x.Id) == true))
            ?? Content.Legends.FirstOrDefault(x => x.Free);
        if (legend is null) return new(saved, null);
        var record = Record(save, legend);
        var care = record["care"]!.AsObject();
        care["bond"] = checked(Number(care, "bond") + 1);
        if (place <= 4) care["battle"] = checked(Number(care, "battle") + (place == 1 ? 3 : 1));
        int xp = Number(record, "xp"), used = Math.Max(0, Math.Min(Content.DataFor.Sum() - xp, Content.GameData + (place <= 4 ? Content.Top4Data : 0)));
        record["xp"] = xp = checked(xp + used);
        int level = 1;
        while (level < Content.DataFor.Length && xp >= Content.DataFor[level]) xp -= Content.DataFor[level++];
        games.Insert(0, JsonValue.Create(key)); while (games.Count > 60) games.RemoveAt(games.Count - 1);
        return new(save.ToJsonString(), new(legend.Id, used, level));
    }
}

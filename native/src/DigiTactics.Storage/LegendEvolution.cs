using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed record EvolutionForm(string Id, string Name, string Level, int Tier, int Rank);
public sealed record EvolutionOdds(string Form, string Care, double Weight, int Pct);
public sealed record EvolutionView(string Form, int Level, int Data, int Need, int? Next, bool CanDigivolve, bool CanRevert,
    EvolutionOdds[] Odds, int LevelBits, string? Roll);
public enum LegendAction { Digivolve, Revert, BuyLevel, Feed }
public sealed record EvolutionChange(bool Changed, string? Legends, string? Wallet, JsonNode? Result);

/// <summary>Read-only evolution projection using original branch order, farm care and seeded rolls.</summary>
public static class LegendEvolution
{
    private sealed record Branch(string Form, string Care);
    private sealed record Catalog(Dictionary<string, EvolutionForm> Forms, Dictionary<string, Branch[]> Children, int[] DataFor, int[] TierLevel, int ItLevel);
    private static readonly Catalog Content = Load();
    private static Catalog Load()
    {
        using var stream = typeof(LegendEvolution).Assembly.GetManifestResourceStream("DigiTactics.Storage.evolution.json")!;
        return JsonSerializer.Deserialize<Catalog>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
    public static EvolutionForm Form(string id) => Content.Forms[id];
    private static double Number(JsonNode node, string key)
    {
        if (node[key] is not JsonValue value) return 0;
        if (value.TryGetValue<double>(out var number)) return number;
        if (value.TryGetValue<int>(out var integer)) return integer;
        return value.GetValue<long>();
    }
    private static Dictionary<string, double> FarmCare(string? farmJson, string id, int tier)
    {
        var result = new Dictionary<string, double> { ["bond"] = 0, ["battle"] = 0, ["food"] = 0, ["wild"] = 0 };
        if (farmJson is null) return result;
        var farm = JsonNode.Parse(farmJson)?.AsObject() ?? throw new InvalidDataException("Invalid farm save.");
        if (farm["v"]?.GetValue<int>() != 1) throw new InvalidDataException("Unsupported farm save version.");
        var pets = farm["pets"] as JsonObject ?? throw new InvalidDataException("Invalid farm pets.");
        if (pets[id] is not { } pet) return result;
        double training = Number(pet, "str") + Number(pet, "spd") + Number(pet, "wis") + Number(pet, "sta");
        double ideal = new[] { 10, 20, 30, 40 }[Math.Clamp(tier, 0, 3)];
        result["bond"] = Math.Min(8, Math.Floor(Number(pet, "goodBond")));
        result["battle"] = Math.Min(8, Math.Floor(training / 25));
        result["food"] = Math.Min(8, Math.Floor(Number(pet, "goodFood")));
        result["wild"] = Math.Min(30, Number(pet, "mistakes") * 3 + Number(pet, "poopNeglect") * 2
            + (Number(pet, "weight") >= ideal * 1.4 ? 6 : 0) + (pet["sick"]?.GetValue<bool>() == true ? 3 : 0) + (Number(pet, "overfeed") >= 5 ? 2 : 0));
        foreach (string key in result.Keys) result[key] = Math.Max(0, result[key]);
        return result;
    }
    private static double Random(string text)
    {
        unchecked
        {
            uint hash = 2166136261;
            foreach (char c in text) hash = (hash ^ c) * 16777619;
            uint t = hash + 0x6d2b79f5;
            t = (t ^ (t >> 15)) * (t | 1);
            t ^= t + (t ^ (t >> 7)) * (t | 61);
            return (t ^ (t >> 14)) / 4294967296.0;
        }
    }
    public static EvolutionView Inspect(string? legendJson, string? walletJson, string? farmJson, string id)
    {
        var (_, record) = LegendRewards.Snapshot(legendJson, id);
        var collection = new CosmeticCollection(walletJson);
        bool owned = collection.Owns("legend", id);
        string form = record["form"]!.GetValue<string>();
        int xp = record["xp"]!.GetValue<int>();
        if (xp < 0) throw new InvalidDataException("Invalid legend XP.");
        int level = 1, left = xp;
        while (level < Content.DataFor.Length && left >= Content.DataFor[level]) left -= Content.DataFor[level++];
        int need = level < Content.DataFor.Length ? Content.DataFor[level] : 0;
        if (need == 0) left = 0;
        var bonus = FarmCare(farmJson, id, Form(form).Tier);
        var care = record["care"]!.AsObject();
        var effective = new[] { "bond", "battle", "food" }.ToDictionary(k => k, k => Math.Clamp(Number(care, k) + bonus[k], 0, 20));
        var weights = Content.Children.GetValueOrDefault(form, []).Select(branch => new EvolutionOdds(branch.Form, branch.Care,
            branch.Care == "wild" ? Math.Max(4, 24 - effective.Values.Sum()) + bonus["wild"] : 20 + 4 * effective[branch.Care], 0)).ToArray();
        double sum = weights.Sum(x => x.Weight);
        double[] raw = weights.Select(x => 100 * x.Weight / (sum == 0 ? 1 : sum)).ToArray();
        var odds = weights.Select((x, i) => x with { Pct = (int)Math.Floor(raw[i]) }).ToArray();
        int remaining = 100 - odds.Sum(x => x.Pct);
        var order = raw.Select((x, i) => (Remainder: x - Math.Floor(x), Index: i)).OrderByDescending(x => x.Remainder).ToArray();
        for (int j = 0; remaining > 0 && order.Length > 0; j++, remaining--) { int i = order[j % order.Length].Index; odds[i] = odds[i] with { Pct = odds[i].Pct + 1 }; }
        int? next = odds.Length == 0 ? null : odds.Max(x => Form(x.Form).Tier > 0 ? Content.TierLevel[Form(x.Form).Tier] : Content.ItLevel);
        string? roll = null;
        if (odds.Length > 0)
        {
            double target = Random($"{id}|{level}|{form}|{record["nonce"]!.GetValue<int>()}") * sum, acc = 0;
            roll = odds[^1].Form;
            foreach (var entry in odds) { acc += entry.Weight; if (target < acc) { roll = entry.Form; break; } }
        }
        return new(form, level, left, need, next, owned && next is not null && level >= next, owned && record["path"]!.AsArray().Count > 0,
            odds, owned && need > 0 ? Math.Max(5, (int)Math.Ceiling((need - left) * .6 / 5) * 5) : 0, roll);
    }
    public static EvolutionChange Apply(string? legendJson, string? walletJson, string? farmJson, string id, LegendAction action)
    {
        EvolutionChange Unchanged(bool poor = false) => new(false, legendJson, walletJson, poor ? JsonValue.Create(false) : null);
        var collection = new CosmeticCollection(walletJson);
        if (!collection.Owns("legend", id)) return Unchanged();
        JsonObject save, record;
        if (action == LegendAction.Revert)
        {
            if (legendJson is null) return Unchanged();
            save = JsonNode.Parse(legendJson)!.AsObject();
            if (save["v"]?.GetValue<int>() != 1) throw new InvalidDataException("Unsupported legend save version.");
            if (save["legends"]?[id] is not JsonObject existing || existing["path"]!.AsArray().Count == 0) return Unchanged();
            record = existing;
        }
        else (save, record) = LegendRewards.Snapshot(legendJson, id);
        EvolutionView? view = action is LegendAction.Digivolve or LegendAction.BuyLevel ? Inspect(legendJson, walletJson, farmJson, id) : null;
        int cost;
        switch (action)
        {
            case LegendAction.Digivolve:
                if (!view!.CanDigivolve || view.Roll is null) return Unchanged();
                cost = 0; break;
            case LegendAction.Revert: cost = 50; break;
            case LegendAction.BuyLevel:
                if (view!.Need == 0) return Unchanged();
                cost = view.LevelBits; break;
            case LegendAction.Feed:
                if (Number(record["care"]!, "food") >= 20) return Unchanged();
                cost = 30; break;
            default: throw new ArgumentOutOfRangeException(nameof(action));
        }
        if (cost > collection.Bits) return Unchanged(true);
        JsonNode result;
        if (action == LegendAction.Digivolve)
        {
            string from = record["form"]!.GetValue<string>(), to = view!.Roll!;
            record["path"]!.AsArray().Add(from); record["form"] = to;
            if (!record["dex"]!.AsArray().Any(x => x?.GetValue<string>() == to)) record["dex"]!.AsArray().Add(to);
            result = new JsonObject { ["from"] = from, ["to"] = to };
        }
        else if (action == LegendAction.Revert)
        {
            string from = record["form"]!.GetValue<string>();
            var path = record["path"]!.AsArray(); string to = path[^1]!.GetValue<string>(); path.RemoveAt(path.Count - 1);
            record["form"] = to; record["nonce"] = checked((record["nonce"]?.GetValue<int>() ?? 0) + 1);
            result = new JsonObject { ["from"] = from, ["to"] = to };
        }
        else if (action == LegendAction.BuyLevel)
        {
            int used = view!.Need - view.Data;
            record["xp"] = checked(record["xp"]!.GetValue<int>() + used);
            result = new JsonObject { ["used"] = used, ["extra"] = 0, ["before"] = view.Level, ["level"] = view.Level + 1 };
        }
        else
        {
            int food = checked(record["care"]!["food"]!.GetValue<int>() + 1); record["care"]!["food"] = food; result = JsonValue.Create(food)!;
        }
        string? wallet = walletJson;
        if (action != LegendAction.Digivolve)
        {
            var paid = JsonNode.Parse(collection.Json)!.AsObject(); paid["bits"] = collection.Bits - cost; wallet = paid.ToJsonString();
        }
        return new(true, save.ToJsonString(), wallet, result);
    }
}

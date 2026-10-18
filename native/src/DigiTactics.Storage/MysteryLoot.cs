using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed record MysteryRound(string Set, bool Duos, uint Seed, int Seat, int Stage, int Round, bool Participated, bool Live = true);
public sealed record MysteryReward(string Kind, string Id);
public sealed record MysteryInventory(long Daily, long Vault, long Keys, string Day, MysteryReward? Pending, int DayProgress = 0);
public sealed record MysteryLootUpdate(string Loot, string? Wallet, MysteryReward? Reward, string[] Drops, bool Changed, string? Message = null);

/// <summary>Original local mystery rewards. Randomness is supplied by the caller and never touches combat RNG.</summary>
public static class MysteryLoot
{
    public const string StorageKey = "dt.mysteryLoot.v1";
    private const long SafeInteger = 9007199254740991;
    // Source Cosmetics.KINDS order determines the random index across each discovery pool.
    private static readonly string[] Kinds = ["legend", "emote", "board", "killscreen", "sound", "dance", "elimsound"];
    private static long Counter(JsonNode? value)
    {
        if (value is not JsonValue number) return 0;
        if (number.TryGetValue<long>(out long integer)) return integer is >= 0 and <= SafeInteger ? integer : 0;
        if (!number.TryGetValue<double>(out double n) || !double.IsFinite(n) || n < 0 || n > SafeInteger || n != Math.Truncate(n)) return 0;
        return (long)n;
    }
    private static JsonObject Read(string? json)
    {
        var value = new JsonObject { ["daily"] = 0, ["vault"] = 0, ["keys"] = 0, ["day"] = "", ["dayProgress"] = 0, ["games"] = new JsonObject(), ["boxMisses"] = 0, ["keyMisses"] = 0, ["pending"] = null };
        if (json is not null)
        {
            var saved = JsonNode.Parse(json) as JsonObject ?? throw new InvalidDataException("Invalid mystery loot save.");
            foreach (var field in saved) value[field.Key] = field.Value?.DeepClone();
        }
        foreach (string key in new[] { "daily", "vault", "keys", "boxMisses", "keyMisses" }) value[key] = Counter(value[key]);
        value["dayProgress"] = Counter(value["dayProgress"]) == 1 ? 1L : 0L;
        if (value["games"] is not JsonObject) value["games"] = new JsonObject();
        return value;
    }
    private static MysteryReward? Pending(JsonObject value) => value["pending"] is JsonObject pending && pending["kind"] is JsonValue kind && kind.TryGetValue<string>(out string? k) && pending["id"] is JsonValue id && id.TryGetValue<string>(out string? i) && k is not null && i is not null ? new(k, i) : null;
    public static MysteryInventory Inventory(string? json)
    {
        var value = Read(json); return new(Counter(value["daily"]), Counter(value["vault"]), Counter(value["keys"]), value["day"]?.GetValue<string>() ?? "", Pending(value), (int)Counter(value["dayProgress"]));
    }
    private static double Roll(Func<double> random)
    {
        double value = random(); if (!double.IsFinite(value) || value < 0 || value >= 1) throw new InvalidDataException("Invalid mystery reward random value."); return value;
    }
    private static void Increment(JsonObject value, string key) => value[key] = checked(Counter(value[key]) + 1);
    public static MysteryLootUpdate RoundEnd(string? json, MysteryRound round, DateOnly day, Func<double> random)
    {
        var value = Read(json);
        MysteryLootUpdate Unchanged() => new(value.ToJsonString(), null, null, [], false);
        if (!round.Live || !round.Participated || round.Seat < 0) return Unchanged();
        string key = $"{round.Set}:{(round.Duos ? "duos" : "solo")}:{round.Seed}:{round.Seat}";
        long sequence = checked((long)round.Stage * 100 + round.Round);
        var games = value["games"]!.AsObject();
        if (sequence <= Counter(games[key])) return Unchanged();
        games[key] = sequence;
        foreach (string old in games.Select(x => x.Key).Take(Math.Max(0, games.Count - 1000)).ToArray()) games.Remove(old);
        var drops = new List<string>(); string date = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        // Only a genuinely new active day advances progress. Gaps never reset it,
        // and imported saves retain their last day without a retroactive reward.
        if (string.CompareOrdinal(date, value["day"]?.GetValue<string>() ?? "") > 0)
        {
            value["day"] = date; Increment(value, "dayProgress");
            if (Counter(value["dayProgress"]) >= 2) { value["dayProgress"] = 0; Increment(value, "daily"); drops.Add("Daybreak Box"); }
        }
        if (round.Stage >= 2)
        {
            Increment(value, "boxMisses");
            if (Counter(value["boxMisses"]) >= 32 || Roll(random) < .03) { Increment(value, "vault"); value["boxMisses"] = 0; drops.Add("Cipher Box"); }
            Increment(value, "keyMisses");
            if (Counter(value["keyMisses"]) >= 40 || Roll(random) < .025) { Increment(value, "keys"); value["keyMisses"] = 0; drops.Add("Cipher Key"); }
        }
        return new(value.ToJsonString(), null, null, drops.ToArray(), true);
    }
    public static MysteryReward[] Available(string? wallet, string pool)
    {
        var collection = new CosmeticCollection(wallet);
        return Kinds.SelectMany(kind => CosmeticCollection.Items(kind).Where(item => item.LootPool == pool && !collection.Owns(kind, item.Id)).Select(item => new MysteryReward(kind, item.Id))).ToArray();
    }
    public static MysteryLootUpdate PrepareOpen(string? json, string? wallet, string pool, Func<double> random)
    {
        var value = Read(json);
        MysteryLootUpdate Unchanged(string? message = null) => new(value.ToJsonString(), null, null, [], false, message);
        if (pool is not ("daily" or "vault")) return Unchanged();
        if (value["pending"] is not null || Counter(value[pool]) < 1 || pool == "vault" && Counter(value["keys"]) < 1) return Unchanged();
        var available = Available(wallet, pool);
        if (available.Length == 0) return Unchanged("You have discovered every reward in this box. Your box is saved.");
        var reward = available[(int)Math.Floor(Roll(random) * available.Length)];
        value[pool] = Counter(value[pool]) - 1; if (pool == "vault") value["keys"] = Counter(value["keys"]) - 1;
        value["pending"] = new JsonObject { ["kind"] = reward.Kind, ["id"] = reward.Id };
        return new(value.ToJsonString(), null, reward, [], true);
    }
    public static MysteryLootUpdate Settle(string? json, string? wallet)
    {
        var value = Read(json); var reward = Pending(value);
        if (reward is null || CosmeticCollection.Item(reward.Kind, reward.Id) is null) return new(value.ToJsonString(), null, null, [], false);
        var collection = new CosmeticCollection(wallet);
        if (!collection.Grant(reward.Kind, reward.Id)) return new(value.ToJsonString(), null, null, [], false);
        value["pending"] = null;
        return new(value.ToJsonString(), collection.Json, reward, [], true);
    }
}

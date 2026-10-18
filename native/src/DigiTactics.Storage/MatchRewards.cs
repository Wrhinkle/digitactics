using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed record RewardStats(int PvpWins, int Kills, int MaxStars, int MaxLevel, int Place);
public sealed record RewardLine(string Label, int N);
public sealed record RewardBreakdown(RewardLine[] Lines, int Total, int Place);
public sealed record RewardUpdate(string Shop, string Wallet, RewardBreakdown? Award, bool Fresh, int RewardPlace);

/// <summary>Pure legacy Bits/mission calculation. Callers commit all resulting profile domains together.</summary>
public static class MatchRewards
{
    private sealed record Mission(string Id, string Name, int Bits, int Goal, Func<RewardStats, int> Value);
    private static readonly Mission[] Missions = [
        new("win3", "Win 3 PvP rounds in one game", 50, 3, s => s.PvpWins),
        new("win6", "Win 6 PvP rounds in one game", 80, 6, s => s.PvpWins),
        new("lvl7", "Reach level 7", 40, 7, s => s.MaxLevel),
        new("star3", "Digivolve a Digimon to 3 stars", 70, 3, s => s.MaxStars),
        new("top4", "Finish in the top half", 50, 1, s => s.Place is > 0 and <= 4 ? 1 : 0),
        new("top2", "Finish top 2", 70, 1, s => s.Place is > 0 and <= 2 ? 1 : 0),
        new("kill1", "Eliminate a Tamer", 60, 1, s => s.Kills)
    ];
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };
    private static uint Hash(string value)
    {
        uint result = 2166136261;
        foreach (char c in value) result = unchecked((result ^ c) * 16777619);
        return result;
    }
    private static JsonObject Object(string? json, string defaults)
    {
        var result = JsonNode.Parse(defaults)!.AsObject();
        if (json is not null)
        {
            var saved = JsonNode.Parse(json) as JsonObject ?? throw new InvalidDataException("Invalid reward save.");
            foreach (var field in saved) result[field.Key] = field.Value?.DeepClone();
        }
        return result;
    }
    public static RewardUpdate Calculate(string? shopJson, string? walletJson, string key, bool duos, RewardStats stats, DateOnly day, long timestamp)
    {
        if (stats.Place < 1 || stats.Place > (duos ? 4 : 8) || stats.PvpWins < 0 || stats.Kills < 0 || stats.MaxLevel < 0 || stats.MaxStars is < 1 or > 3)
            throw new InvalidDataException("Invalid reward statistics.");
        var shop = Object(shopJson, "{\"awarded\":[],\"awards\":{},\"lastDay\":\"\",\"missions\":{\"day\":\"\",\"done\":[]}}");
        var wallet = Object(walletJson, "{\"bits\":0,\"owned\":{},\"equipped\":{},\"history\":[]}");
        if (wallet["owned"] is not JsonObject || wallet["equipped"] is not JsonObject || wallet["history"] is not JsonArray || wallet["bits"] is null
            || shop["awarded"] is not JsonArray || shop["awards"] is not JsonObject || shop["lastDay"] is null
            || shop["missions"] is not JsonObject missions || missions["day"] is null || missions["done"] is not JsonArray)
            throw new InvalidDataException("Invalid reward save fields.");
        var owned = wallet["owned"]!.AsObject();
        foreach (string kind in new[] { "legend", "emote", "board", "killscreen", "sound", "dance", "elimsound" })
            if (owned[kind] is null) owned[kind] = new JsonArray();
        var awarded = shop["awarded"]!.AsArray(); var awards = shop["awards"]!.AsObject();
        int rewardPlace = duos ? stats.Place * 2 - 1 : stats.Place;
        if (awards[key] is { } existing)
            return new(shopJson ?? shop.ToJsonString(), walletJson ?? wallet.ToJsonString(), existing.Deserialize<RewardBreakdown>(Options), false, rewardPlace);
        if (awarded.Any(value => value!.GetValue<string>() == key))
            return new(shopJson ?? shop.ToJsonString(), walletJson ?? wallet.ToJsonString(), null, false, rewardPlace);
        int[] placement = [0, 120, 90, 75, 60, 45, 35, 25, 20];
        var lines = new List<RewardLine> { new($"Placement #{stats.Place}", placement[rewardPlace]) };
        if (stats.PvpWins > 0) lines.Add(new($"{stats.PvpWins} PvP round{(stats.PvpWins > 1 ? "s" : "")} won", checked(stats.PvpWins * 6)));
        if (stats.Kills > 0) lines.Add(new($"{stats.Kills} elimination{(stats.Kills > 1 ? "s" : "")}", checked(stats.Kills * 25)));
        string date = day.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        if (shop["lastDay"]!.GetValue<string>() != date) { lines.Add(new("First game of the day", 60)); shop["lastDay"] = date; }
        if (shop["missions"]!["day"]!.GetValue<string>() != date) shop["missions"] = new JsonObject { ["day"] = date, ["done"] = new JsonArray() };
        var done = shop["missions"]!["done"]!.AsArray();
        foreach (var mission in Missions.OrderBy(m => Hash(date + ":" + m.Id)).Take(2))
        {
            if (done.Any(value => value!.GetValue<string>() == mission.Id) || mission.Value(stats with { Place = rewardPlace }) < mission.Goal) continue;
            done.Add(mission.Id); lines.Add(new("Mission: " + mission.Name, mission.Bits));
        }
        var award = new RewardBreakdown(lines.ToArray(), lines.Sum(line => line.N), stats.Place);
        awarded.Insert(0, JsonValue.Create(key)); while (awarded.Count > 200) awarded.RemoveAt(awarded.Count - 1);
        awards[key] = JsonSerializer.SerializeToNode(award, Options);
        var keep = awarded.Take(10).Select(value => value!.GetValue<string>()).ToHashSet(StringComparer.Ordinal);
        foreach (string old in awards.Select(pair => pair.Key).ToArray()) if (!keep.Contains(old)) awards.Remove(old);
        long bits = wallet["bits"]!.GetValue<long>();
        if (bits < 0) throw new InvalidDataException("Invalid saved Bits balance.");
        wallet["bits"] = checked(bits + award.Total);
        var history = wallet["history"]!.AsArray();
        history.Insert(0, new JsonObject { ["n"] = award.Total, ["reason"] = $"Game: placed #{stats.Place}", ["at"] = timestamp });
        while (history.Count > 30) history.RemoveAt(history.Count - 1);
        return new(shop.ToJsonString(), wallet.ToJsonString(), award, true, rewardPlace);
    }
}

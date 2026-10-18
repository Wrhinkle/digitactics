using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed record HistoryTrait(string Id, int N, int Lv);
public sealed record HistoryUnit(string Line, int Stage, string[] Items);
public sealed record HistoryMatch(string Key, long At, string Mode, string Format, string? Set, int Place, string Round,
    int Level, HistoryTrait[] Traits, HistoryUnit[] Units, JsonNode? Stats = null, long? Bits = null);

public static class MatchHistory
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    public static string Append(string? saved, HistoryMatch match)
    {
        if (match.Format is not ("solo" or "duos") || match.Place < 1 || match.Place > (match.Format == "duos" ? 4 : 8))
            throw new InvalidDataException("Invalid match placement.");
        var profile = saved is null ? new JsonObject() : JsonNode.Parse(saved) as JsonObject ?? throw new InvalidDataException("Invalid client progress.");
        var history = profile["matches"] as JsonArray ?? (profile["matches"] is null ? new JsonArray() : throw new InvalidDataException("Invalid match history."));
        var existing = history.FirstOrDefault(entry => entry?["key"]?.GetValue<string>() == match.Key);
        if (existing is not null)
        {
            if (existing["bits"] is null && match.Bits is not null) { existing["bits"] = match.Bits; return profile.ToJsonString(); }
            return saved!;
        }
        if (profile["matches"] is null) profile["matches"] = history;
        var recorded = match with { Traits = match.Traits.Take(8).ToArray(), Units = match.Units.Take(10).ToArray() };
        history.Insert(0, JsonSerializer.SerializeToNode(recorded, Options));
        while (history.Count > 30) history.RemoveAt(history.Count - 1);
        var totals = profile["totals"] as JsonObject ?? (profile["totals"] is null ? new JsonObject() : throw new InvalidDataException("Invalid match totals."));
        if (profile["totals"] is null) profile["totals"] = totals;
        static void Increment(JsonObject values, string key, int amount)
        {
            long old = values[key]?.GetValue<long>() ?? 0;
            if (old < 0) throw new InvalidDataException("Invalid progress counter.");
            values[key] = checked(old + amount);
        }
        Increment(totals, "games", 1); Increment(totals, "wins", match.Place == 1 ? 1 : 0);
        Increment(totals, "top4", match.Place <= (match.Format == "duos" ? 2 : 4) ? 1 : 0); Increment(totals, "placeSum", match.Place);
        foreach (string field in new[] { "traits", "units" })
            if (totals[field] is null) totals[field] = new JsonObject();
        foreach (var trait in recorded.Traits) Increment(totals["traits"]!.AsObject(), trait.Id, 1);
        foreach (string line in recorded.Units.Select(unit => unit.Line).Distinct(StringComparer.Ordinal)) Increment(totals["units"]!.AsObject(), line, 1);
        return profile.ToJsonString();
    }
}

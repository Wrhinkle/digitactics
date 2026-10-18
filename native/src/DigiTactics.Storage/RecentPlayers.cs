using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed record RecentPlayer(string Name, long At);
public sealed record EncounteredPlayer(string? Name, bool Me = false);

public static class RecentPlayers
{
    public static IReadOnlyList<RecentPlayer> Read(string? saved)
    {
        var profile = Parse(saved);
        return Entries(profile).Select(entry => new RecentPlayer(entry!["name"]!.GetValue<string>(), entry["at"]!.GetValue<long>())).ToArray();
    }

    public static string? Remember(string? saved, string self, IEnumerable<EncounteredPlayer> members, long now)
    {
        var profile = Parse(saved);
        var recent = Entries(profile);
        bool changed = false;
        foreach (var member in members)
        {
            if (string.IsNullOrEmpty(member.Name) || member.Me || member.Name == self) continue;
            var old = recent.FirstOrDefault(entry => entry!["name"]!.GetValue<string>() == member.Name);
            if (old is null) { recent.Insert(0, new JsonObject { ["name"] = member.Name, ["at"] = now }); changed = true; }
            else if ((decimal)now - old["at"]!.GetValue<long>() > 60000) { old["at"] = now; changed = true; }
        }
        if (!changed) return saved;
        profile["recent"] = new JsonArray(recent.OrderByDescending(entry => entry!["at"]!.GetValue<long>()).Take(20).Select(entry => entry!.DeepClone()).ToArray());
        return profile.ToJsonString();
    }

    private static JsonObject Parse(string? saved) => saved is null ? new() : JsonNode.Parse(saved) as JsonObject ?? throw new InvalidDataException("Invalid client progress.");
    private static JsonArray Entries(JsonObject profile)
    {
        var entries = profile["recent"] as JsonArray ?? (profile["recent"] is null ? new JsonArray() : throw new InvalidDataException("Invalid recent players."));
        foreach (var entry in entries)
            if (entry is not JsonObject || entry["name"] is not JsonValue name || !name.TryGetValue<string>(out _) ||
                entry["at"] is not JsonValue at || !at.TryGetValue<long>(out _)) throw new InvalidDataException("Invalid recent player.");
        return entries;
    }
}

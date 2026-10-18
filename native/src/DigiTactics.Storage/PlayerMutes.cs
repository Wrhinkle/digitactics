using System.Text.Json;

namespace DigiTactics.Storage;

/// <summary>Device-local social mutes, compatible with the original dt.muted ordered set.</summary>
public sealed class PlayerMutes
{
    private readonly List<string> players;
    public PlayerMutes(string? saved)
    {
        if (saved is null) { players = []; return; }
        using var document = JsonDocument.Parse(saved);
        players = document.RootElement.ValueKind == JsonValueKind.Array
            ? document.RootElement.EnumerateArray().Where(v => v.ValueKind == JsonValueKind.String).Select(v => v.GetString()!).TakeLast(256).Distinct(StringComparer.Ordinal).ToList() : [];
    }
    public bool Contains(string id) => players.Contains(id, StringComparer.Ordinal);
    public string Toggle(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (!players.Remove(id)) { if (players.Count >= 256) players.RemoveAt(0); players.Add(id); }
        return JsonSerializer.Serialize(players);
    }
}

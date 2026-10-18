using System.Text.Json;

namespace DigiTactics.Assets;

public sealed record CompanionVisual(string Source, double Height, bool Unit, JsonElement Clips, Dictionary<string, string> Procedural);

/// <summary>Original Little Legend model and reachable-form selection, shared by farm, previews and matches.</summary>
public sealed record CompanionCatalog(int SchemaVersion, Dictionary<string, CompanionVisual> Bases,
    Dictionary<string, CompanionVisual> Forms, Dictionary<string, string[]> Reachable, JsonElement Fallbacks)
{
    public static CompanionCatalog Load(string path)
    {
        var catalog = JsonSerializer.Deserialize<CompanionCatalog>(File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("Missing companion catalog.");
        if (catalog.SchemaVersion != 1) throw new InvalidDataException("Unsupported companion catalog.");
        return catalog;
    }
    public CompanionVisual Resolve(string? key)
    {
        string[] parts = (key ?? "").Split('@');
        string id = Bases.ContainsKey(parts[0]) ? parts[0] : "tokomon";
        if (parts.Length > 1 && Reachable.TryGetValue(parts[0], out var reachable) && reachable.Contains(parts[1]) && Forms.TryGetValue(parts[1], out var form)) return form;
        return Bases[id];
    }
}

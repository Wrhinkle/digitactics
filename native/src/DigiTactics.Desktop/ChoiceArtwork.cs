using System.Text.Json;

namespace DigiTactics.Desktop;

internal sealed record ChoiceArt(string? Token, string? File, int? Tier);
internal sealed record ChoiceArtSample(string Variant, string Kind, string Id, ChoiceArt Art);

/// <summary>The exact image selected by the original offer card, including deliberately absent images.</summary>
internal static class ChoiceArtwork
{
    private sealed record Manifest(Dictionary<string, Dictionary<string, Dictionary<string, ChoiceArt>>> Variants, ChoiceArtSample[] Samples);
    private static readonly Manifest catalog = JsonSerializer.Deserialize<Manifest>(
        System.IO.File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "content/choice-art/manifest.json")),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

    public static ChoiceArt? Resolve(string variant, string kind, string id)
    {
        if (!catalog.Variants.TryGetValue(variant, out var kinds) || !kinds.TryGetValue(kind, out var choices)) return null;
        if (kind == "drg_treasure")
        {
            var parts = id.Split(':', 2); string tag = parts[0];
            id = tag switch
            {
                "comp" or "item" or "emblem" or "rad" => tag + ":" + (parts.Length > 1 ? parts[1].Split('.')[0] : ""),
                "dup" => "dup:0", "gold" => "gold:0", "unit" => "unit:unknown", _ => "chaos"
            };
        }
        else if (kind == "arcMarket" && id.StartsWith("arc_pass:", StringComparison.Ordinal)) id = "arc_pass:0";
        else if (kind == "col_gambit") id = id.Split(':')[0] == "double" ? "double:0" : "cash:0";
        return choices.GetValueOrDefault(id);
    }

    // One image for each kind and each generic tier; actual UI smoke can exercise every source art route.
    public static IReadOnlyList<ChoiceArtSample> Samples => catalog.Samples;
}

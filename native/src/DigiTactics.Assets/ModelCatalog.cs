using System.Security.Cryptography;
using System.Text.Json;

namespace DigiTactics.Assets;

public sealed record ModelAsset(string File, string Sha256, string SourceSha256, long Bytes, int Nodes, int Meshes, int Skins, string[] Animations, int Accessors);
public sealed record ModelSource(string Asset, JsonElement Metadata);
public sealed record ModelCatalog(int SchemaVersion, Dictionary<string, ModelAsset> Assets,
    Dictionary<string, ModelSource> Sources, Dictionary<string, Dictionary<string, string?[]>> Variants)
{
    public static ModelCatalog Load(string path)
    {
        var catalog = JsonSerializer.Deserialize<ModelCatalog>(File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("Missing model catalog.");
        if (catalog.SchemaVersion != 1) throw new InvalidDataException("Unsupported model catalog.");
        return catalog;
    }

    public ModelSource? Resolve(string variant, string line, int stage)
    {
        if (stage is < 0 or > 2) throw new ArgumentOutOfRangeException(nameof(stage));
        if (!Variants.TryGetValue(variant, out var lines) || !lines.TryGetValue(line, out var forms)) return null;
        return forms[stage] is { } key && Sources.TryGetValue(key, out var source) ? source : null;
    }

    public GltfAsset Open(string root, string asset)
    {
        var entry = Assets[asset];
        string directory = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
        string path = Path.GetFullPath(Path.Combine(root, entry.File));
        if (!path.StartsWith(directory, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Model path escapes its content root.");
        var bytes = File.ReadAllBytes(path);
        if (bytes.LongLength != entry.Bytes || !Convert.ToHexString(SHA256.HashData(bytes)).Equals(entry.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Model integrity failure: {asset}");
        return GltfAsset.LoadCompressed(bytes);
    }
}

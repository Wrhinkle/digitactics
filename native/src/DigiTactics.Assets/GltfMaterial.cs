using System.Text.Json;

namespace DigiTactics.Assets;

public readonly record struct MaterialColor(double R, double G, double B, double A = 1);
public sealed record TextureBinding(int Texture, int TexCoord, bool Srgb);
public sealed record TextureSampler(int MagFilter = 9729, int MinFilter = 9987, int WrapS = 10497, int WrapT = 10497)
{
    public bool Mipmaps => MinFilter is not (9728 or 9729);
}
public sealed record GltfTexture(int Image, TextureSampler Sampler)
{
    public static GltfTexture Load(GltfAsset asset, int index)
    {
        var texture = asset.Root.GetProperty("textures")[index];
        var sampler = texture.TryGetProperty("sampler", out var id) ? asset.Root.GetProperty("samplers")[id.GetInt32()] : default;
        int Read(string key, int fallback) => sampler.ValueKind == JsonValueKind.Object && sampler.TryGetProperty(key, out var value) ? value.GetInt32() : fallback;
        var result = new TextureSampler(Read("magFilter", 9729), Read("minFilter", 9987), Read("wrapS", 10497), Read("wrapT", 10497));
        if (result.MagFilter is not (9728 or 9729) || result.MinFilter is not (9728 or 9729 or 9984 or 9985 or 9986 or 9987) ||
            result.WrapS is not (33071 or 33648 or 10497) || result.WrapT is not (33071 or 33648 or 10497))
            throw new InvalidDataException("Unsupported texture sampler.");
        return new(texture.GetProperty("source").GetInt32(), result);
    }
}

/// <summary>Material values after the original renderer's game-rip corrections.</summary>
public sealed record GltfMaterial(MaterialColor Color, bool Unlit, bool DoubleSided, bool Transparent,
    bool DepthWrite, double AlphaCutoff, double Roughness, double SpecularIntensity,
    TextureBinding? ColorMap, TextureBinding? RoughnessMap, TextureBinding? NormalMap, TextureBinding? SpecularMap)
{
    public MaterialColor Emissive { get; init; } = new(0, 0, 0);
    public double EmissiveIntensity { get; init; } = 1;
    public TextureBinding? EmissiveMap { get; init; }
    public double NormalScale { get; init; } = 1;
    public static GltfMaterial Load(GltfAsset asset, int index, bool frontOnly = false, bool vertexAlpha = false)
    {
        var material = asset.Root.GetProperty("materials")[index];
        var pbr = Property(material, "pbrMetallicRoughness");
        var extensions = Property(material, "extensions");
        var specGloss = Property(extensions, "KHR_materials_pbrSpecularGlossiness");
        bool unlit = Property(extensions, "KHR_materials_unlit").ValueKind == JsonValueKind.Object || specGloss.ValueKind == JsonValueKind.Object;
        var color = ColorValue(Property(pbr, "baseColorFactor"));
        var map = Binding(pbr, "baseColorTexture", true);
        // The original plugin changes RGB only: opacity still comes from core PBR.
        if (specGloss.ValueKind == JsonValueKind.Object)
        {
            var diffuse = Binding(specGloss, "diffuseTexture", true);
            var rgb = diffuse is not null ? new MaterialColor(1, 1, 1) : ColorValue(Property(specGloss, "diffuseFactor"));
            color = rgb with { A = color.A };
            if (diffuse is not null) map = diffuse;
        }
        string mode = Property(material, "alphaMode").GetStringOrDefault("OPAQUE");
        bool transparent = mode == "BLEND";
        double cutoff = mode == "MASK" ? Number(material, "alphaCutoff", .5) : 0;
        // The rip cutout correction must not discard an authored per-vertex opacity gradient.
        if (transparent && color.A >= 1 && map is not null && !vertexAlpha) { transparent = false; cutoff = .5; }
        var card = mode == "OPAQUE" && map is not null ? EffectCard(asset, material, map) : EffectCardMode.None;
        if (card != EffectCardMode.None) transparent = true;
        var specular = Property(extensions, "KHR_materials_specular");
        return new(color, unlit, !frontOnly && Property(material, "doubleSided").ValueKind == JsonValueKind.True,
            transparent, !transparent, cutoff, unlit ? 1 : Number(pbr, "roughnessFactor", 1), Number(specular, "specularFactor", 1),
            map, unlit ? null : Binding(pbr, "metallicRoughnessTexture", false),
            unlit ? null : Binding(material, "normalTexture", false), unlit ? null : Binding(specular, "specularTexture", false))
        {
            Emissive = !unlit && Property(material, "emissiveFactor") is { ValueKind: JsonValueKind.Array } e ? new(e[0].GetDouble(), e[1].GetDouble(), e[2].GetDouble()) : new(0, 0, 0),
            EmissiveIntensity = Number(Property(extensions, "KHR_materials_emissive_strength"), "emissiveStrength", 1),
            EmissiveMap = unlit ? null : Binding(material, "emissiveTexture", true),
            NormalScale = Number(Property(material, "normalTexture"), "scale", 1),
            Additive = card == EffectCardMode.Additive
        };
    }

    /// <summary>Additive blending (SRC_ALPHA, ONE) for a glow card whose source uses a black background.</summary>
    public bool Additive { get; init; }

    private enum EffectCardMode { None, Blend, Additive }
    private static readonly System.Text.RegularExpressions.Regex EffectName = new(@"(^|[_\s.])(eff|ef|glw|glow|lig)(?=$|[_\s.\d])", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    // Same rule as render3d.js effectCard(). Rips export effect cards as OPAQUE even
    // though the game blends them: textures with real opacity become BLEND; opaque
    // glow textures on a mostly black background become additive. Outside effect-named
    // materials only a cut-out atlas with soft edges (Lucemon Satan Mode's circles) blends.
    private static EffectCardMode EffectCard(GltfAsset asset, JsonElement material, TextureBinding map)
    {
        // A binding without an embedded image (synthetic loader checks) has no pixels to classify.
        if (Property(asset.Root, "textures").ValueKind != JsonValueKind.Array || Property(asset.Root, "images").ValueKind != JsonValueKind.Array) return EffectCardMode.None;
        int source = GltfTexture.Load(asset, map.Texture).Image;
        var image = asset.Image(source);
        string imageName = Property(asset.Root.GetProperty("images")[source], "name").GetStringOrDefault("");
        bool effect = EffectName.IsMatch(Property(material, "name").GetStringOrDefault("")) || EffectName.IsMatch(imageName);
        int count = image.Width * image.Height, low = 0, mid = 0, dark = 0;
        for (int i = 0; i < count; i++)
        {
            byte a = image.Rgba[i * 4 + 3];
            if (a < 128) low++;
            if (a is > 16 and < 240) mid++;
            if (Math.Max(image.Rgba[i * 4], Math.Max(image.Rgba[i * 4 + 1], image.Rgba[i * 4 + 2])) < 16) dark++;
        }
        double lowShare = (double)low / count, midShare = (double)mid / count, darkShare = (double)dark / count;
        if (effect && lowShare >= .05) return EffectCardMode.Blend;
        if (effect && darkShare >= .45) return EffectCardMode.Additive;
        if (!effect && lowShare is >= .3 and <= .9 && midShare is > .06 and <= .25) return EffectCardMode.Blend;
        return EffectCardMode.None;
    }

    public IEnumerable<TextureBinding> Textures()
    {
        if (ColorMap is not null) yield return ColorMap;
        if (RoughnessMap is not null) yield return RoughnessMap;
        if (NormalMap is not null) yield return NormalMap;
        if (SpecularMap is not null) yield return SpecularMap;
        if (EmissiveMap is not null) yield return EmissiveMap;
    }
    private static JsonElement Property(JsonElement value, string name) => value.ValueKind == JsonValueKind.Object && value.TryGetProperty(name, out var result) ? result : default;
    private static double Number(JsonElement value, string name, double fallback) => Property(value, name) is var number && number.ValueKind == JsonValueKind.Number ? number.GetDouble() : fallback;
    private static MaterialColor ColorValue(JsonElement value) => value.ValueKind == JsonValueKind.Array ? new(value[0].GetDouble(), value[1].GetDouble(), value[2].GetDouble(), value[3].GetDouble()) : new(1, 1, 1);
    private static TextureBinding? Binding(JsonElement value, string name, bool srgb)
    {
        var info = Property(value, name);
        if (info.ValueKind != JsonValueKind.Object) return null;
        if (Property(info, "extensions").ValueKind == JsonValueKind.Object) throw new NotSupportedException("Texture transforms require conversion.");
        return new(info.GetProperty("index").GetInt32(), (int)Number(info, "texCoord", 0), srgb);
    }
}

internal static class JsonMaterialExtensions
{
    public static string GetStringOrDefault(this JsonElement value, string fallback) => value.ValueKind == JsonValueKind.String ? value.GetString()! : fallback;
}

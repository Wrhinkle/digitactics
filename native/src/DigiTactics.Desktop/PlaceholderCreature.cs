using System.Numerics;
using System.Text.Json;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>The original family-colored procedural creature used for units without an assigned model.</summary>
internal sealed class PlaceholderCreature : IDisposable
{
    private static readonly Lazy<JsonDocument> Content = new(() => JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "content/placeholder-creatures.json"))));
    private readonly GpuPrimitive[] geometry;
    private readonly (int Geometry, Matrix4D Matrix)[] parts;
    public PlaceholderCreature()
    {
        var root = Content.Value.RootElement;
        geometry = root.GetProperty("geometries"u8).EnumerateArray().Select(g =>
        {
            var p = g.GetProperty("positions"u8).EnumerateArray().Select(n => n.GetSingle()).ToArray();
            var normals = g.GetProperty("normals"u8).EnumerateArray().Select(n => n.GetSingle()).ToArray();
            var vertices = new float[p.Length / 3 * GltfPrimitive.Stride];
            for (int i = 0; i < p.Length / 3; i++) { p.AsSpan(i * 3, 3).CopyTo(vertices.AsSpan(i * GltfPrimitive.Stride, 3)); normals.AsSpan(i * 3, 3).CopyTo(vertices.AsSpan(i * GltfPrimitive.Stride + 3, 3)); }
            return new GpuPrimitive(new(vertices, g.GetProperty("indices"u8).EnumerateArray().Select(n => n.GetUInt32()).ToArray(), -1, true, false));
        }).ToArray();
        parts = root.GetProperty("parts"u8).EnumerateArray().Select(p => (p.GetProperty("geometry"u8).GetInt32(), Matrix4D.From(p.GetProperty("matrix"u8).EnumerateArray().Select(n => n.GetDouble()).ToArray()))).ToArray();
    }
    public int Draw(ModelRenderer renderer, Matrix4x4 camera, Matrix4D placement, string variant, string line, bool silhouette = false, bool fog = false, double emission = 0, double opacity = 1)
    {
        bool faded = opacity < 1;
        var colors = Content.Value.RootElement.GetProperty("colors"u8).GetProperty(variant).GetProperty(line);
        for (int i = 0; i < parts.Length; i++)
        {
            var c = colors[i];
            renderer.DrawRoom(geometry[parts[i].Geometry], parts[i].Matrix * placement, camera,
                silhouette ? new(.01033f,.00091f,.00091f,1) : new(c[0].GetSingle(),c[1].GetSingle(),c[2].GetSingle(),(float)opacity), null, silhouette, faded, !faded, emissive:new((float)emission), roughness:.55f, metalness:.05f, side:0, fog:fog,receiveShadow:false);
        }
        return parts.Length;
    }
    public void Dispose() { foreach (var primitive in geometry) primitive.Dispose(); }
}

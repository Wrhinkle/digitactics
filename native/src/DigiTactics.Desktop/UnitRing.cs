using System.Numerics;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>render3d.js UnitView / legends.js LegendView floor marker: a black .25 disc (radius .58, y .155) under a
/// colored .9 ring (.58-.7, y .16). Both are MeshBasicMaterial children of the unit group, so they follow its position
/// and scale; the ring alone takes the cast pulse scale. Drawn in the transparent pass and never in shadows.</summary>
internal sealed class UnitRing : IDisposable
{
    private const int Segments = 40;
    private readonly GpuPrimitive ring, disc;
    public static readonly string[] TierColors = ["#9aa4b1", "#9aa4b1", "#3fbf6a", "#3b8cff", "#b45cff", "#ffb02e"];
    public static string TierColor(int tier) => TierColors[Math.Clamp(tier, 0, TierColors.Length - 1)];

    public UnitRing()
    {
        float[] ringVertices = new float[(Segments + 1) * 2 * GltfPrimitive.Stride];
        uint[] ringIndices = new uint[Segments * 6];
        for (int row = 0; row < 2; row++) for (int i = 0; i <= Segments; i++)
            Vertex(ringVertices, row * (Segments + 1) + i, i * Math.PI * 2 / Segments, row == 0 ? .58 : .7);
        for (uint i = 0; i < Segments; i++)
        {
            int at = (int)i * 6; uint outer = i + Segments + 1;
            ringIndices[at] = i; ringIndices[at + 1] = outer; ringIndices[at + 2] = i + 1;
            ringIndices[at + 3] = outer; ringIndices[at + 4] = outer + 1; ringIndices[at + 5] = i + 1;
        }
        ring = new(new(ringVertices, ringIndices, -1, true, false));
        float[] discVertices = new float[(Segments + 2) * GltfPrimitive.Stride];
        uint[] discIndices = new uint[Segments * 3];
        Vertex(discVertices, 0, 0, 0);
        for (int i = 0; i <= Segments; i++) Vertex(discVertices, i + 1, i * Math.PI * 2 / Segments, .58);
        for (uint i = 0; i < Segments; i++) { discIndices[i * 3] = 0; discIndices[i * 3 + 1] = i + 1; discIndices[i * 3 + 2] = i + 2; }
        disc = new(new(discVertices, discIndices, -1, true, false));
    }

    private static void Vertex(float[] vertices, int index, double angle, double radius)
    {
        int at = index * GltfPrimitive.Stride;
        vertices[at] = (float)(Math.Cos(angle) * radius);
        vertices[at + 2] = (float)(-Math.Sin(angle) * radius);
        vertices[at + 4] = 1;
    }

    /// <summary>Draws the marker of a unit group at (x, y, z). <paramref name="color"/> is an sRGB hex color;
    /// <paramref name="radius"/> scales the disc and ring in place (.8 for a Little Legend).</summary>
    public int Draw(ModelRenderer renderer, Matrix4x4 camera, bool transparent, double x, double y, double z, string color,
        double alpha = 1, double scale = 1, double ringScale = 1, double radius = 1)
    {
        if (!transparent || renderer.ShadowPass) return 0;
        var linear = BoardLighting.Linear(color);
        renderer.DrawRoom(disc, GpuModel.Transform(0, .155, 0, 0, radius) * GpuModel.Transform(x, y, z, 0, scale), camera,
            new(0, 0, 0, (float)(.25 * alpha)), null, true, true, false, receiveShadow: false);
        renderer.DrawRoom(ring, GpuModel.Transform(0, .16, 0, 0, ringScale * radius) * GpuModel.Transform(x, y, z, 0, scale), camera,
            new(linear.X, linear.Y, linear.Z, (float)(.9 * alpha)), null, true, true, false, receiveShadow: false);
        return 2;
    }

    public void Dispose() { ring.Dispose(); disc.Dispose(); }
}

using System.Numerics;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>Batches the original square point sprites into camera-facing quads.</summary>
internal sealed class FarmSnowView : IDisposable
{
    private readonly FarmSnow snow;
    private readonly GpuPrimitive geometry;
    private readonly float[] vertices;
    private readonly Matrix4x4 world;
    public FarmSnowView(float[] positions, double[] matrix)
    {
        snow = new(positions); vertices = new float[positions.Length / 3 * 4 * 16];
        uint[] indices = new uint[positions.Length / 3 * 6];
        for (uint i = 0; i < positions.Length / 3; i++)
        {
            uint v = i * 4; int j = (int)i * 6;
            indices[j] = v; indices[j + 1] = v + 1; indices[j + 2] = v + 2;
            indices[j + 3] = v; indices[j + 4] = v + 2; indices[j + 5] = v + 3;
        }
        world = new((float)matrix[0],(float)matrix[1],(float)matrix[2],(float)matrix[3],(float)matrix[4],(float)matrix[5],(float)matrix[6],(float)matrix[7],(float)matrix[8],(float)matrix[9],(float)matrix[10],(float)matrix[11],(float)matrix[12],(float)matrix[13],(float)matrix[14],(float)matrix[15]);
        geometry = new(new(vertices, indices, -1, true, false));
    }
    public void Update(double delta, double time) => snow.Update(delta, time);
    public void Draw(ModelRenderer renderer, Matrix4x4 camera, Vector3 eye)
    {
        var forward = Vector3.Normalize(new Vector3(0, -.35f, .55f) - eye);
        var right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
        var up = Vector3.Cross(right, forward);
        // Three PointsMaterial uses viewport-height / 2, independent of camera focal length.
        float half = .04f * MathF.Tan(34 * MathF.PI / 360);
        for (int i = 0; i < snow.Positions.Length / 3; i++)
        {
            var p = Vector3.Transform(new(snow.Positions[i * 3], snow.Positions[i * 3 + 1], snow.Positions[i * 3 + 2]), world);
            for (int corner = 0; corner < 4; corner++)
            {
                var v = p + right * (corner is 1 or 2 ? half : -half) + up * (corner >= 2 ? half : -half);
                int offset = (i * 4 + corner) * 16;
                vertices[offset] = v.X; vertices[offset + 1] = v.Y; vertices[offset + 2] = v.Z; vertices[offset + 5] = 1;
            }
        }
        geometry.UpdateVertices(vertices);
        renderer.DrawRoom(geometry, Matrix4D.Identity, camera, new(1, 1, 1, .9f), null, true, true, false);
    }
    public void Dispose() => geometry.Dispose();
}

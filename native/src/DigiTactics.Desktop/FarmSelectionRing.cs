using System.Numerics;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

internal sealed class FarmSelectionRing : IDisposable
{
    private readonly GpuPrimitive geometry;
    public FarmSelectionRing()
    {
        const int segments = 40;
        float[] vertices = new float[(segments + 1) * 2 * GltfPrimitive.Stride];
        uint[] indices = new uint[segments * 6];
        for (int row = 0; row < 2; row++) for (int i = 0; i <= segments; i++)
        {
            double angle = i * Math.PI * 2 / segments, radius = row == 0 ? .42 : .52;
            int offset = (row * (segments + 1) + i) * GltfPrimitive.Stride;
            vertices[offset] = (float)(Math.Cos(angle) * radius);
            vertices[offset + 2] = (float)(-Math.Sin(angle) * radius);
            vertices[offset + 4] = 1;
        }
        for (uint i = 0; i < segments; i++)
        {
            int offset = (int)i * 6; uint outer = i + segments + 1;
            indices[offset] = i; indices[offset+1] = outer; indices[offset+2] = i+1;
            indices[offset+3] = outer; indices[offset+4] = outer+1; indices[offset+5] = i+1;
        }
        geometry = new(new(vertices, indices, -1, true, false));
    }
    public void Draw(ModelRenderer renderer, Matrix4x4 camera, double x, double z, double time)
    {
        float Linear(double value) => (float)(value <= .04045 ? value / 12.92 : Math.Pow((value + .055) / 1.055, 2.4));
        double scale = 1 + Math.Sin(time * 4) * .05;
        renderer.ReceiveShadow = false;
        renderer.DrawRoom(geometry, GpuModel.Transform(x,.03,z,0,scale),camera,new(1,Linear(207/255.0),Linear(74/255.0),.9f),null,true,true,false);
    }
    public void Dispose() => geometry.Dispose();
}

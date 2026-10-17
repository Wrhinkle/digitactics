using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;
using System.Numerics;

namespace DigiTactics.Desktop;

internal sealed class ArenaBoard : IDisposable
{
    private readonly GpuPrimitive ally;
    private readonly GpuPrimitive enemy;
    public ArenaBoard(int columns) { ally = Build(columns, 4, 8); enemy = Build(columns, 0, 4); }
    private static GpuPrimitive Build(int columns, int start, int end)
    {
        var vertices = new List<float>(); var indices = new List<uint>();
        for (int row = start; row < end; row++) for (int column = 0; column < columns; column++)
        {
            var center = CombatLayout.Cell(new HexCell(column, row), columns, false);
            uint first = (uint)(vertices.Count / GltfPrimitive.Stride);
            void Vertex(double x, double z) { vertices.AddRange([(float)x, .14f, (float)z, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0]); }
            Vertex(center.X, center.Z);
            for (int i = 0; i < 6; i++) { double angle = i * Math.PI / 3; Vertex(center.X + .93 * Math.Sin(angle), center.Z + .93 * Math.Cos(angle)); }
            for (uint i = 0; i < 6; i++) indices.AddRange([first, first + 1 + i, first + 1 + (i + 1) % 6]);
        }
        return new(new(vertices.ToArray(), indices.ToArray(), -1, true, false));
    }
    public void Draw(ModelRenderer renderer, Matrix4x4 camera)
    {
        static float Linear(int value) { double c = value / 255.0; return (float)(c <= .04045 ? c / 12.92 : Math.Pow((c + .055) / 1.055, 2.4)); }
        renderer.DrawStatic(ally, Matrix4D.Identity, camera, new(Linear(48), Linear(89), Linear(140), 1));
        renderer.DrawStatic(enemy, Matrix4D.Identity, camera, new(Linear(122), Linear(52), Linear(66), 1));
    }
    public void Dispose() { ally.Dispose(); enemy.Dispose(); }
}

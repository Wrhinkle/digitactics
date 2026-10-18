using System.Numerics;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>Owns board triangle geometry; weather, lines and scene animation are separate layers.</summary>
internal sealed class BoardMeshLayer : IDisposable
{
    private sealed record Part(GpuPrimitive Geometry, BoardMaterial Material, bool HasNormals);
    private sealed record ObjectParts(BoardObject Object, Part[] Parts);
    private readonly List<GpuPrimitive> geometry = [];
    private readonly List<GpuTexture> textures = [];
    private readonly List<ObjectParts> objects = [];
    public int DrawCalls { get; private set; }
    public BoardMeshLayer(BoardScene scene)
    {
        try
        {
            foreach (var texture in scene.Textures)
                textures.Add(new(BoardMesh.Image(texture), new(WrapS: BoardMesh.Wrap(texture.WrapS), WrapT: BoardMesh.Wrap(texture.WrapT)), texture.ColorSpace == "srgb"));
            var cache = new Dictionary<string, Part[]>();
            foreach (var obj in scene.Objects.Where(o => o.Kind == "mesh"))
            {
                string key = obj.Geometry + ":" + string.Join(',', obj.Materials);
                if (!cache.TryGetValue(key, out var parts))
                {
                    parts = BoardMesh.Create(scene, obj).Select(part =>
                    {
                        var gpu = new GpuPrimitive(part.Primitive); geometry.Add(gpu);
                        return new Part(gpu, scene.Materials[part.Material], part.Primitive.HasNormals);
                    }).ToArray();
                    cache.Add(key, parts);
                }
                objects.Add(new(obj, parts));
            }
        }
        catch { Dispose(); throw; }
    }

    public void Draw(ModelRenderer renderer, Matrix4x4 camera, bool transparent)
    {
        DrawCalls = 0;
        foreach (var item in objects)
        {
            if (!item.Object.Visible) continue;
            var world = Matrix4D.From(item.Object.Matrix);
            foreach (var part in item.Parts)
            {
                var material = part.Material;
                if (material.Transparent != transparent) continue;
                void Draw(Matrix4D matrix, float r, float g, float b)
                {
                    renderer.ReceiveShadow = item.Object.ReceiveShadow;
                    renderer.DrawRoom(part.Geometry, matrix, camera,
                        new(material.Color[0] * r, material.Color[1] * g, material.Color[2] * b, material.Color[3]),
                        material.Map is { } map ? textures[map] : null, material.Unlit, transparent, material.DepthWrite,
                        new Vector3(material.Emissive[0], material.Emissive[1], material.Emissive[2]) * material.EmissiveIntensity,
                        material.Roughness, material.Metalness, material.Side, additive: material.Blending == 2,
                        vertexColors: material.VertexColors, flatShading: material.FlatShading || !part.HasNormals, alphaTest: material.AlphaTest, wireframe: material.Wireframe);
                    DrawCalls++;
                }
                if (item.Object.Instances is { } instances)
                    foreach (var instance in instances) Draw(Matrix4D.From(instance.Matrix) * world, instance.Color[0], instance.Color[1], instance.Color[2]);
                else Draw(world, 1, 1, 1);
            }
        }
    }

    public void Dispose()
    {
        foreach (var primitive in geometry) primitive.Dispose(); geometry.Clear();
        foreach (var texture in textures) texture.Dispose(); textures.Clear();
        objects.Clear();
    }
}

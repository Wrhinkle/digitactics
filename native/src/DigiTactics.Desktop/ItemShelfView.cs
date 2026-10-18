using System.Numerics;
using System.Text.Json;
using DigiTactics.Assets;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed class ItemShelfView : IDisposable
{
    private static readonly string folder = Path.Combine(AppContext.BaseDirectory, "content/item-tokens");
    private static readonly Dictionary<string, Dictionary<string, string>> art = JsonSerializer.Deserialize<Manifest>(File.ReadAllText(Path.Combine(folder, "manifest.json")), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!.Sets;
    private sealed record Manifest(Dictionary<string, Dictionary<string, string>> Sets);
    private readonly Dictionary<string, GpuTexture> textures = [];
    private readonly GpuPrimitive quad;
    public string Ruleset { get; set; } = "faction-wars";
    public IReadOnlyList<string> Items { get; set; } = [];
    public int DragIndex { get; set; } = -1;
    public WorldPosition? DragPosition { get; set; }
    public int DrawCalls { get; private set; }
    public static WorldPosition Position(int index) => new(-7.5 - index % 2 * .95, .62, 1.2 + index / 2 * 1.5);
    public static Brutal.Numerics.float2 Screen(int index, int width, int height, ArenaCamera? camera = null)
    {
        var p = Position(index);
        var clip = Vector4.Transform(new Vector4((float)p.X, (float)p.Y, (float)p.Z, 1), camera?.Matrix(width, height, 7) ?? BattleView.Camera(width, height, 7));
        return new((clip.X / clip.W + 1) * width / 2, (1 - clip.Y / clip.W) * height / 2);
    }

    public ItemShelfView()
    {
        float[] vertices = new float[4 * GltfPrimitive.Stride];
        for (int i = 0; i < 4; i++)
        {
            int at = i * GltfPrimitive.Stride; float x = i is 1 or 2 ? 1 : 0, y = i >= 2 ? 1 : 0;
            vertices[at] = x - .5f; vertices[at + 1] = y - .5f; vertices[at + 5] = 1;
            vertices[at + 6] = x; vertices[at + 7] = 1 - y;
        }
        quad = new(new(vertices, [0, 1, 2, 0, 2, 3], -1, true, false));
    }

    public void Draw(ModelRenderer renderer, Matrix4x4 camera, Vector3 eye, int columns, double time, BoardSurfaceFeedback feedback, Vector3 target = default)
    {
        DrawCalls = 0;
        var used = new HashSet<string>();
        if (columns == 7 && art.TryGetValue(Ruleset.Split('/')[0], out var icons))
        {
            var rotation = Quaternion.CreateFromRotationMatrix(Matrix4x4.CreateRotationY(MathF.PI) * Matrix4x4.CreateBillboard(Vector3.Zero, eye - target, Vector3.UnitY, Vector3.UnitZ));
            for (int i = 0; i < Math.Min(10, Items.Count); i++)
            {
                if (!icons.TryGetValue(Items[i], out var file)) throw new InvalidDataException($"Missing item artwork: {Ruleset}/{Items[i]}");
                used.Add(file);
                if (!textures.TryGetValue(file, out var texture)) textures[file] = texture = new(PngImage.Decode(File.ReadAllBytes(Path.Combine(folder, file))), new(WrapS: 33071, WrapT: 33071), true);
                bool dragging = DragIndex == i && DragPosition is not null;
                var position = dragging ? DragPosition!.Value with { Y = 1.1 } : Position(i) with { Y = .62 + Math.Sin(time * 2 + i) * .05 };
                double scale = dragging ? .95 : feedback.HoverItem == i || feedback.ItemDragOver == i ? .92 : .78;
                var world = Matrix4D.Compose(new(position.X, position.Y, position.Z), new(rotation.X, rotation.Y, rotation.Z, rotation.W), new(scale, scale, 1));
                renderer.DrawRoom(quad, world, camera, new(1, 1, 1, 1), texture, true, true, false);
                DrawCalls++;
            }
        }
        foreach (var file in textures.Keys.Where(file => !used.Contains(file)).ToArray()) { textures[file].Dispose(); textures.Remove(file); }
    }

    public void Dispose() { quad.Dispose(); foreach (var texture in textures.Values) texture.Dispose(); textures.Clear(); }
}

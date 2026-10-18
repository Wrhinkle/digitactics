using DigiTactics.Assets;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

/// <summary>One model's immutable geometry/textures and prepared rig, shared by every unit instance.</summary>
internal sealed class GpuModel : IDisposable
{
    public ModelSource Source { get; }
    public GltfRig Rig { get; }
    public GltfMesh Mesh { get; }
    public GltfMaterial[] Materials { get; }
    public List<GpuPrimitive> Primitives { get; } = [];
    public Dictionary<int, GpuTexture> Textures { get; } = [];
    public Dictionary<int, GpuTexture> SurfaceTextures { get; } = [];
    public Matrix4D Normalization { get; }
    public double Footprint { get; }
    public Matrix4D PreviewPlacement => Normalization * Transform(0, 0, 0, Rotation, 2 / Math.Max(1, Footprint));
    private double Rotation => Number("rotY"u8, 0);
    public CompanionAnimator CreateCompanionAnimator(CompanionVisual visual, System.Text.Json.JsonElement fallbacks, double height = 1.2, double yaw = 0)
        => new(Rig, visual, fallbacks, Math.Min(height * visual.Height, 1.5 / Math.Max(Footprint, .01)), Rotation, yaw);
    public Matrix4D PlaceCompanion(CompanionAnimator animator, double x = 0, double y = 0, double z = 0)
        => Normalization * animator.Transform.Matrix * Transform(x, y, z, animator.BodyYaw, 1);

    public GpuModel(ModelCatalog catalog, ModelSource source)
    {
        Source = source;
        try
        {
            string root = Path.Combine(AppContext.BaseDirectory, "assets");
            using var asset = catalog.Open(root, source.Asset);
            Rig = GltfRig.Load(asset).Prepare(source, root); Mesh = GltfMesh.Load(asset);
            var pose = new RigPose(Rig); pose.Sample(ModelAnimator.FindIdle(Rig), 0);
            var (min, max) = Mesh.Bounds(Rig, pose, precise: false);
            double height = Math.Max(max.Y - min.Y, .001);
            Footprint = Math.Max(max.X - min.X, max.Z - min.Z) / height;
            Normalization = Transform(-(min.X + max.X) / (2 * height), -min.Y / height, -(min.Z + max.Z) / (2 * height), 0, 1 / height);
            bool front = source.Metadata.TryGetProperty("frontOnly"u8, out var frontOnly) && frontOnly.GetBoolean();
            Materials = asset.Root.GetProperty("materials"u8).EnumerateArray().Select((_, i) => GltfMaterial.Load(asset, i, front)).ToArray();
            foreach (var binding in Materials.SelectMany(m => new[] { m.ColorMap, m.EmissiveMap }).OfType<TextureBinding>())
                if (!Textures.ContainsKey(binding.Texture))
                {
                    var texture = GltfTexture.Load(asset, binding.Texture);
                    Textures.Add(binding.Texture, new(asset.Image(texture.Image), texture.Sampler, binding.Srgb));
                }
            foreach (var group in Mesh.Meshes) foreach (var primitive in group) Primitives.Add(new(primitive));
            foreach (var binding in Materials.SelectMany(m => new[] { m.RoughnessMap, m.SpecularMap }).OfType<TextureBinding>())
                if (!SurfaceTextures.ContainsKey(binding.Texture))
                {
                    var texture = GltfTexture.Load(asset, binding.Texture);
                    SurfaceTextures.Add(binding.Texture, new(asset.Image(texture.Image), texture.Sampler, false));
                }
        }
        catch { Dispose(); throw; }
    }

    public Matrix4D Place(CombatVisual view, int stage, int tier)
    {
        double height = new[] { 1.3, 1.65, 2.0 }[stage] + (tier == 5 ? .15 : 0);
        double scale = Math.Min(height, Number("maxFootprint"u8, 1.9) / Math.Max(Footprint, .01));
        return Normalization * Transform(0, Number("yOffset"u8, 0) * scale, 0, Rotation, scale)
            * Transform(view.BodyOffset.X, view.BodyOffset.Y, view.BodyOffset.Z, view.Yaw, 1)
            * Transform(view.Position.X, view.Position.Y, view.Position.Z, 0, view.Scale);
    }
    private double Number(ReadOnlySpan<byte> name, double fallback) => Source.Metadata.TryGetProperty(name, out var value) ? value.GetDouble() : fallback;
    internal static Matrix4D Transform(double x, double y, double z, double yaw, double scale) => Matrix4D.Compose(new(x, y, z), new(0, Math.Sin(yaw / 2), 0, Math.Cos(yaw / 2)), new(scale, scale, scale));
    public void Dispose()
    {
        foreach (var primitive in Primitives) primitive.Dispose(); Primitives.Clear();
        foreach (var texture in Textures.Values) texture.Dispose(); Textures.Clear();
        foreach (var texture in SurfaceTextures.Values) texture.Dispose(); SurfaceTextures.Clear();
    }
}

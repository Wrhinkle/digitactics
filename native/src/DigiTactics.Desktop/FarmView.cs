using System.IO.Compression;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Assets;
using DigiTactics.Storage;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed class FarmView : IDisposable
{
    private sealed record Material(float[] Color, int? Texture, bool Unlit, bool Transparent, bool DepthWrite);
    private sealed record Texture(int Width, int Height, string Rgba, bool Clamp);
    private sealed record Mesh(float[] Vertices, uint[] Indices, double[] Matrix, int Material, bool Sprite, int Meat);
    private sealed record Room(int SchemaVersion, string Theme, string[] Sky, Material[] Materials, Texture[] Textures, Mesh[] Meshes);
    private sealed record Actor(GpuModel Model, CompanionAnimator Animator, FarmResident Resident, CompanionVisual Visual);
    private sealed class MotionView(CompanionAnimator animator) : IFarmCompanionView
    {
        public bool HasRun => animator.HasRun;
        public double Yaw { get => animator.Yaw; set => animator.Yaw = value; }
        public void SetMoving(bool moving) => animator.SetMoving(moving);
        public void Play(string role) => animator.Play(role);
        public void Update(double delta) => animator.Update(delta);
    }
    private readonly FarmChoreography choreography = new();
    private readonly ModelCatalog models = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
    private readonly CompanionCatalog companions = CompanionCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/companion-models.json"));
    private readonly ModelRenderer renderer = new();
    private readonly Dictionary<string, Actor> actors = [];
    private readonly Dictionary<string, GpuModel> loaded = [];
    private readonly List<GpuPrimitive> geometry = [];
    private readonly List<GpuTexture> textures = [];
    private Room? room;
    private string? lastFarm, lastLegends;
    private int meat;
    public int DrawCalls { get; private set; }
    public IReadOnlyCollection<string> Residents => actors.Keys;
    public string? Theme => room?.Theme;
    public int SleepingResidents => choreography.Residents.Values.Count(x => x.Mode == "sleep");
    public void Sync(string farmJson, string? legendJson, string? walletJson)
    {
        if (farmJson == lastFarm && legendJson == lastLegends) return;
        var farm = JsonNode.Parse(farmJson)!;
        string theme = farm["theme"]!.GetValue<string>();
        if (room?.Theme != theme) LoadRoom(theme);
        meat = farm["meat"]!["n"]!.GetValue<int>();
        var residents = farm["residents"]!.AsArray().Select(x => x!.GetValue<string>()).ToArray();
        foreach (string id in actors.Keys.Except(residents).ToArray()) { actors.Remove(id); choreography.Remove(id); }
        foreach (string id in residents)
        {
            string form = LegendEvolution.Inspect(legendJson, walletJson, farmJson, id).Form;
            var visual = companions.Resolve(form == id ? id : id + "@" + form);
            var pet = farm["pets"]![id]!;
            var needs = new FarmResidentNeeds(pet["flags"]?["asleep"]?.GetValue<bool>() == true, pet["sick"]!.GetValue<bool>(), pet["energy"]!.GetValue<double>(), pet["happy"]!.GetValue<double>(), pet["tantrum"]!.GetValue<double>() != 0);
            if (actors.TryGetValue(id, out var existing) && existing.Visual == visual) { existing.Resident.Needs = needs; continue; }
            if (!loaded.TryGetValue(visual.Source, out var model)) loaded[visual.Source] = model = new(models, models.Sources[visual.Source]);
            var animator = model.CreateCompanionAnimator(visual, companions.Fallbacks);
            if (existing is not null) { choreography.Remove(id); actors.Remove(id); }
            var resident = choreography.Add(id, new MotionView(animator), existing?.Resident.X, existing?.Resident.Z); resident.Needs = needs;
            actors[id] = new(model, animator, resident, visual);
        }
        foreach (string key in loaded.Keys.Where(key => !actors.Values.Any(a => a.Model == loaded[key])).ToArray()) { loaded[key].Dispose(); loaded.Remove(key); }
        lastFarm = farmJson; lastLegends = legendJson;
    }
    private void LoadRoom(string theme)
    {
        if (!FarmCare.Themes.Any(x => x.Id == theme)) throw new InvalidDataException("Unknown farm room theme.");
        using var file = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "content/farm-rooms", theme + ".json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        var next = JsonSerializer.Deserialize<Room>(gzip, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new InvalidDataException("Missing farm room.");
        if (next.SchemaVersion != 1 || next.Theme != theme) throw new InvalidDataException("Unsupported farm room.");
        foreach (var primitive in geometry) primitive.Dispose(); geometry.Clear();
        foreach (var texture in textures) texture.Dispose(); textures.Clear();
        room = null;
        try
        {
            foreach (var texture in next.Textures) textures.Add(new(new(texture.Width, texture.Height, Convert.FromBase64String(texture.Rgba)), new(WrapS: texture.Clamp ? 33071 : 10497, WrapT: texture.Clamp ? 33071 : 10497), true));
            foreach (var mesh in next.Meshes) geometry.Add(new(new(mesh.Vertices, mesh.Indices, -1, true, false)));
            room = next;
        }
        catch { foreach (var primitive in geometry) primitive.Dispose(); geometry.Clear(); foreach (var texture in textures) texture.Dispose(); textures.Clear(); throw; }
    }
    public void Update(double delta) => choreography.Update(delta);
    public void React(string? id, string role) { if (id is not null && actors.TryGetValue(id, out var actor)) actor.Animator.Play(role); }
    private static (Matrix4x4 Camera, Vector3 Eye) Camera(int width, int height)
    {
        float aspect = (float)width / height;
        float distance = Math.Clamp(Math.Max(15, 5.6f / (MathF.Tan(MathF.PI * 34 / 360) * aspect)), 15, 22);
        var eye = new Vector3(0, distance * .74f, distance * .84f);
        return (Matrix4x4.CreateLookAt(eye, new(0, -.35f, .55f), Vector3.UnitY) * Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI * 34 / 180, aspect, .1f, 100), eye);
    }
    public IEnumerable<(string Id, float X, float Y)> Anchors(int width, int height)
    {
        var camera = Camera(width, height).Camera;
        foreach (var (id, actor) in actors)
        {
            var point = Vector4.Transform(new Vector4((float)actor.Resident.X, 1.6f + (float)actor.Resident.Height, (float)actor.Resident.Z, 1), camera);
            yield return (id, (point.X / point.W * .5f + .5f) * width, (1 - (point.Y / point.W * .5f + .5f)) * height);
        }
    }
    public void Draw(int left, int bottom, int width, int height)
    {
        DrawCalls = 0; if (room is null || width <= 0 || height <= 0) return;
        var (camera, eye) = Camera(width, height); ModelRenderer.Begin(width, height);
        Brutal.OpenGlApi.OpenGl.Viewport(left, bottom, width, height);
        foreach (bool transparent in new[] { false, true })
        {
            for (int i = 0; i < room.Meshes.Length; i++)
            {
                var mesh = room.Meshes[i]; var material = room.Materials[mesh.Material];
                if (material.Transparent != transparent || mesh.Meat >= meat && mesh.Meat >= 0) continue;
                var matrix = Matrix4D.From(mesh.Matrix);
                if (mesh.Sprite)
                {
                    var billboard = Matrix4x4.CreateBillboard(new((float)matrix.M41, (float)matrix.M42, (float)matrix.M43), eye, Vector3.UnitY, Vector3.UnitZ);
                    var rotation = Quaternion.CreateFromRotationMatrix(billboard);
                    matrix = Matrix4D.Compose(new(matrix.M41, matrix.M42, matrix.M43), new(rotation.X, rotation.Y, rotation.Z, rotation.W), new(.42, .42, .42));
                }
                renderer.DrawRoom(geometry[i], matrix, camera, new(material.Color[0], material.Color[1], material.Color[2], material.Color[3]), material.Texture is { } texture ? textures[texture] : null, material.Unlit, transparent, material.DepthWrite); DrawCalls++;
            }
            foreach (var actor in actors.Values)
            {
                var resident = actor.Resident;
                var root = new CompanionTransform(new(resident.X, resident.Height, resident.Z), new(0, 0, resident.Tilt * 1.15), new(1, resident.Breathe, 1));
                renderer.Draw(actor.Model, actor.Animator.Pose, actor.Model.PlaceCompanion(actor.Animator) * root.Matrix, camera, transparent); DrawCalls += renderer.DrawCalls;
            }
        }
        ModelRenderer.End();
    }
    public void Dispose()
    {
        foreach (var primitive in geometry) primitive.Dispose(); foreach (var texture in textures) texture.Dispose(); foreach (var model in loaded.Values) model.Dispose(); renderer.Dispose();
    }
}

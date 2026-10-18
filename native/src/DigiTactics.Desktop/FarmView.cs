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
    private sealed record Material(float[] Color, int? Texture, bool Unlit, bool Transparent, bool DepthWrite, float[] Emissive, float EmissiveIntensity, bool Bulb, float Roughness, float Metalness, int Side, int? ShadowSide);
    private sealed record Texture(int Width, int Height, string Rgba, bool Clamp);
    private sealed record Mesh(float[] Vertices, uint[] Indices, double[] Matrix, int Material, bool Sprite, int Meat, FarmRoomMotion? Motion, bool CastShadow, bool ReceiveShadow);
    private sealed record Points(float[] Positions, double[] Matrix);
    private sealed record Room(int SchemaVersion, string Theme, string[] Sky, Material[] Materials, Texture[] Textures, Mesh[] Meshes, Points[] Points);
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
    private readonly FarmLighting lighting = new();
    public bool Night => lighting.Night;
    private readonly ModelCatalog models = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
    private readonly CompanionCatalog companions = CompanionCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/companion-models.json"));
    private readonly ModelRenderer renderer = new();
    private readonly FarmShadowMap shadows = new();
    private readonly FarmAtmosphere atmosphere = new();
    public double NightOpacity => atmosphere.Opacity;
    public int ShadowDrawCalls { get; private set; }
    internal bool EnableSunShadows { get; set; } = true;
    private readonly Dictionary<string, Actor> actors = [];
    private readonly Dictionary<string, GpuModel> loaded = [];
    private readonly List<GpuPrimitive> geometry = [];
    private readonly List<GpuTexture> textures = [];
    private Room? room;
    private readonly List<FarmSnowView> snow = [];
    public int SnowParticles => room?.Points.Sum(x => x.Positions.Length / 3) ?? 0;
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
        lighting.Night = farm["lights"]?.GetValue<bool>() == false;
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
        if (next.SchemaVersion != 2 || next.Theme != theme) throw new InvalidDataException("Unsupported farm room.");
        foreach (var primitive in geometry) primitive.Dispose(); geometry.Clear();
        foreach (var texture in textures) texture.Dispose(); textures.Clear();
        foreach (var particles in snow) particles.Dispose(); snow.Clear();
        room = null;
        try
        {
            foreach (var texture in next.Textures) textures.Add(new(new(texture.Width, texture.Height, Convert.FromBase64String(texture.Rgba)), new(WrapS: texture.Clamp ? 33071 : 10497, WrapT: texture.Clamp ? 33071 : 10497), true));
            foreach (var mesh in next.Meshes) geometry.Add(new(new(mesh.Vertices, mesh.Indices, -1, true, false)));
            foreach (var points in next.Points) snow.Add(new(points.Positions, points.Matrix));
            room = next;
        }
        catch { foreach (var particles in snow) particles.Dispose(); snow.Clear(); foreach (var primitive in geometry) primitive.Dispose(); geometry.Clear(); foreach (var texture in textures) texture.Dispose(); textures.Clear(); throw; }
    }
    public void Update(double delta)
    {
        choreography.Update(delta);
        lighting.Update(delta, Theme ?? "meadow");
        atmosphere.Update(delta, lighting.Night);
        foreach (var particles in snow) particles.Update(delta, choreography.Time);
    }
    public void React(string? id, string role) { if (id is not null && actors.TryGetValue(id, out var actor)) actor.Animator.Play(role); }
    public void StartTraining(string id) => choreography.StartTraining(id);
    public void FinishTraining(string id, double score) => choreography.FinishTraining(id, score);
    public void Care(string? id, FarmCareAction action, string? option, JsonObject receipt, string? before)
    {
        bool Flag(string key) => receipt[key]?.GetValue<bool>() == true;
        if (!Flag("ok")) { if (action == FarmCareAction.Feed && Flag("refused")) React(id, "taunt"); return; }
        if (action == FarmCareAction.Clean)
        {
            var pets = before is null ? null : JsonNode.Parse(before)?["pets"];
            foreach (string resident in actors.Keys) if (pets?[resident]?["poop"]?.AsArray().Count > 0) React(resident, "happy");
        }
        if (id is null) return;
        switch (action)
        {
            case FarmCareAction.Feed: choreography.Feed(id); break;
            case FarmCareAction.Pet: choreography.Pet(id); break;
            case FarmCareAction.Play: choreography.Play(id, option); break;
            case FarmCareAction.Cure: React(id, "happy"); break;
            case FarmCareAction.Praise: React(id, Flag("spoiled") ? "taunt" : "happy"); break;
            case FarmCareAction.Scold: React(id, Flag("unfair") ? "sad" : "hit"); break;
        }
    }
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
        DrawShadows();
        var (camera, eye) = Camera(width, height); ModelRenderer.Begin(width, height);
        renderer.ShadowTexture = EnableSunShadows ? shadows.Texture : 0;
        renderer.FarmLight = lighting;
        renderer.FarmEye = eye;
        Brutal.OpenGlApi.OpenGl.Viewport(left, bottom, width, height);
        atmosphere.Draw(left,bottom,width,height,room.Sky,false);
        foreach (bool transparent in new[] { false, true })
        {
            for (int i = 0; i < room.Meshes.Length; i++)
            {
                var mesh = room.Meshes[i]; var material = room.Materials[mesh.Material];
                if (material.Transparent != transparent || mesh.Meat >= meat && mesh.Meat >= 0) continue;
                var matrix = mesh.Motion?.Evaluate(choreography.Time) ?? Matrix4D.From(mesh.Matrix);
                if (mesh.Sprite)
                {
                    var billboard = Matrix4x4.CreateBillboard(new((float)matrix.M41, (float)matrix.M42, (float)matrix.M43), eye, Vector3.UnitY, Vector3.UnitZ);
                    // System.Numerics billboards face local -Z; Three sprites face +Z.
                    var rotation = Quaternion.CreateFromRotationMatrix(Matrix4x4.CreateRotationY(MathF.PI) * billboard);
                    matrix = Matrix4D.Compose(new(matrix.M41, matrix.M42, matrix.M43), new(rotation.X, rotation.Y, rotation.Z, rotation.W), new(.42, .42, .42));
                }
                float intensity = material.Bulb ? (float)lighting.BulbEmission : material.EmissiveIntensity;
                renderer.ReceiveShadow = mesh.ReceiveShadow;
                renderer.DrawRoom(geometry[i], matrix, camera, new(material.Color[0], material.Color[1], material.Color[2], material.Color[3]), material.Texture is { } texture ? textures[texture] : null, material.Unlit, transparent, material.DepthWrite, new(material.Emissive[0] * intensity, material.Emissive[1] * intensity, material.Emissive[2] * intensity), material.Roughness, material.Metalness, material.Side, material.ShadowSide); DrawCalls++;
            }
            foreach (var actor in actors.Values)
            {
                renderer.ReceiveShadow = false;
                var resident = actor.Resident;
                var root = new CompanionTransform(new(resident.X, resident.Height, resident.Z), new(0, 0, resident.Tilt * 1.15), new(1, resident.Breathe, 1));
                renderer.Draw(actor.Model, actor.Animator.Pose, actor.Model.PlaceCompanion(actor.Animator) * root.Matrix, camera, transparent); DrawCalls += renderer.DrawCalls;
            }
        }
        renderer.ReceiveShadow = false;
        foreach (var particles in snow) { particles.Draw(renderer, camera, eye); DrawCalls++; }
        atmosphere.Draw(left,bottom,width,height,room.Sky,true);
        ModelRenderer.End();
    }
    private void DrawShadows()
    {
        ShadowDrawCalls = 0; shadows.Begin(); renderer.ShadowPass = true;
        try
        {
            for (int i = 0; i < room!.Meshes.Length; i++)
            {
                var mesh = room.Meshes[i];
                if (!mesh.CastShadow || mesh.Meat >= meat && mesh.Meat >= 0) continue;
                var material = room.Materials[mesh.Material];
                renderer.DrawRoom(geometry[i], mesh.Motion?.Evaluate(choreography.Time) ?? Matrix4D.From(mesh.Matrix), FarmShadowMap.Camera, new(1,1,1,1), null, true, false, true, side: material.Side, shadowSide: material.ShadowSide);
                ShadowDrawCalls++;
            }
            foreach (var actor in actors.Values)
            {
                var a = actor.Resident;
                var root = new CompanionTransform(new(a.X,a.Height,a.Z),new(0,0,a.Tilt*1.15),new(1,a.Breathe,1));
                foreach (bool transparent in new[] { false, true })
                {
                    renderer.Draw(actor.Model, actor.Animator.Pose, actor.Model.PlaceCompanion(actor.Animator)*root.Matrix, FarmShadowMap.Camera, transparent);
                    ShadowDrawCalls += renderer.DrawCalls;
                }
            }
        }
        finally { renderer.ShadowPass = false; shadows.End(); }
    }
    public void Dispose()
    {
        foreach (var particles in snow) particles.Dispose(); foreach (var primitive in geometry) primitive.Dispose(); foreach (var texture in textures) texture.Dispose(); foreach (var model in loaded.Values) model.Dispose(); atmosphere.Dispose(); shadows.Dispose(); renderer.Dispose();
    }
}

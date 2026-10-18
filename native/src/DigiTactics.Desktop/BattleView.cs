using System.Numerics;
using Brutal.ImGuiApi;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Combat workbench using the real fixed-tick simulation; round/match flow is separate.</summary>
internal sealed class BattleView : ICombatModelDriver, IDisposable
{
    private readonly Ruleset rules;
    private readonly ModelCatalog catalog = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
    private readonly ModelRenderer renderer = new();
    private readonly Dictionary<ModelSource, GpuModel> models = [];
    private readonly Dictionary<int, (GpuModel Model, ModelAnimator Animator)> actors = [];
    private readonly ArenaBoard board;
    private readonly CombatChoreography choreography;
    private double accumulator;
    private bool paused;
    private bool mirror;
    public CombatSimulation Combat { get; }
    public int DrawCalls { get; private set; }
    public bool BackRequested { get; private set; }
    public int ModelCount => models.Count;
    public bool Mirrored => mirror;

    public BattleView(Ruleset rules, CombatEntry[]? roster = null, int columns = 7)
    {
        this.rules = rules; board = new(columns);
        var side = roster is { Length: > 0 } ? roster : rules.Lines.Take(6).Select((line, i) => new CombatEntry(line.Id, i % 3, i % columns, 4 + i / columns, Items: [])).ToArray();
        var enemy = rules.Lines.Skip(3).Take(side.Length).Select((line, i) => new CombatEntry(line.Id, i % 3, i % columns, 4 + i / columns, Items: [])).ToArray();
        Combat = new(rules, side, enemy, 84219, columns, setup: new(new(), new()));
        choreography = new(Combat, this); choreography.Update(0);
    }
    public BattleView(Ruleset rules, CombatSimulation combat, bool mirror)
    {
        this.rules = rules; Combat = combat; this.mirror = mirror; board = new(combat.Columns);
        choreography = new(combat, this); choreography.Update(0, mirror);
    }
    public void Create(CombatUnit unit)
    {
        var source = catalog.Resolve(rules.Id, unit.Line.Id, unit.Stage); if (source is null) return;
        if (!models.TryGetValue(source, out var model)) { model = new(catalog, source); models.Add(source, model); }
        actors[unit.Id] = (model, new ModelAnimator(model.Rig, (ModelAnimator.FindIdle(model.Rig)?.Duration ?? 0) * .17));
    }
    public void Remove(int unit) => actors.Remove(unit);
    public bool Play(int unit, string role) => actors.TryGetValue(unit, out var actor) && actor.Animator.Play(role);
    public void Update(int unit, double delta) { if (actors.TryGetValue(unit, out var actor)) actor.Animator.Update(delta); }
    public void Advance(double delta)
    {
        if (paused) return;
        accumulator += Math.Min(delta, .25);
        while (accumulator >= CombatSimulation.Tick && !Combat.Done) { Combat.Step(); accumulator -= CombatSimulation.Tick; }
        choreography.Update(delta, mirror);
    }
    public void Controls(Brutal.Numerics.int2 size)
    {
        ImGui.SetNextWindowPos(new(20, 20)); ImGui.SetNextWindowSize(new(size.X - 40, 125));
        ImGui.Begin("Native combat workbench"u8, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
        ImGui.Text($"{rules.Name} / {rules.Pace}    {Combat.Time:F1}s    {(Combat.Done ? $"Winner: {Combat.Winner}" : "Fighting")}");
        ImGui.Text("Combat rendering test. Round progression and multiplayer are not connected yet."u8);
        if (ImGui.Button(paused ? "Resume"u8 : "Pause"u8)) paused = !paused;
        ImGui.SameLine(); if (ImGui.Button("Back to shop"u8)) BackRequested = true;
        ImGui.End();
    }
    internal static Vector3 CameraEye(int width, int height, int columns)
    {
        float distance = (float)Math.Max(24, (columns * Math.Sqrt(3) + 4) / (2 * Math.Tan(.36) * width / height));
        return new(0, distance * .6644f, distance * .7474f);
    }
    internal static Matrix4x4 Camera(int width, int height, int columns)
    {
        return Matrix4x4.CreateLookAt(CameraEye(width,height,columns), new(0, 0, 0), Vector3.UnitY)
            * Matrix4x4.CreatePerspectiveFieldOfView(.72f, (float)width / height, .1f, 100);
    }
    public void Labels(int width, int height, int windowHeight, int left = 0, int bottom = 0)
    {
        if (width <= 0 || height <= 0) return;
        var camera = Camera(width, height, Combat.Columns); var draw = ImGui.GetForegroundDrawList();
        foreach (var view in choreography.Views)
        {
            var unit = Combat.Units[view.UnitId]; if (!unit.Alive) continue;
            var point = Vector4.Transform(new Vector4((float)view.Position.X, (float)view.Position.Y + 2, (float)view.Position.Z, 1), camera);
            if (point.W <= 0) continue;
            float x = left + (point.X / point.W * .5f + .5f) * width, y = windowHeight - bottom - (point.Y / point.W * .5f + .5f) * height;
            float health = (float)Math.Clamp(unit.Health / unit.MaxHealth, 0, 1);
            draw.AddRectFilled(new(x - 25, y), new(x + 25, y + 5), new ImColor8(15, 20, 25));
            draw.AddRectFilled(new(x - 25, y), new(x - 25 + 50 * health, y + 5), unit.Team == (mirror ? 1 : 0) ? new ImColor8(80, 220, 150) : new ImColor8(235, 80, 90));
            if (ImGui.IsMouseHoveringRect(new(x - 30, y - 35), new(x + 30, y + 25)))
                draw.AddText(ImGui.GetFont(), 14, new(x - 25, y - 20), new ImColor8(230, 235, 245), unit.Line.Forms[unit.Stage]);
            if (view.CastVisible && view.CastName is { } name) draw.AddText(ImGui.GetFont(), 12, new(x - 25, y + 8), new ImColor8(255, 220, 100), name);
        }
    }
    public void Draw(int width, int height, int left = 0, int bottom = 0)
    {
        if (width <= 0 || height <= 0) return;
        var camera = Camera(width, height, Combat.Columns); ModelRenderer.Begin(width, height);
        Brutal.OpenGlApi.OpenGl.Viewport(left, bottom, width, height); board.Draw(renderer, camera); DrawCalls = 0;
        foreach (bool transparent in new[] { false, true }) foreach (var view in choreography.Views)
        {
            if (!actors.TryGetValue(view.UnitId, out var actor)) continue;
            var unit = Combat.Units[view.UnitId];
            renderer.Draw(actor.Model, actor.Animator.Pose, actor.Model.Place(view, unit.Stage, unit.Line.Tier), camera, transparent, view.Flash > 0 ? view.Flash : view.Glow);
            DrawCalls += renderer.DrawCalls;
        }
        ModelRenderer.End();
    }
    public void Dispose() { foreach (var model in models.Values) model.Dispose(); models.Clear(); board.Dispose(); renderer.Dispose(); }
}

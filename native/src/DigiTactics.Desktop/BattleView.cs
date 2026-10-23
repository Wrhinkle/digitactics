using System.Numerics;
using Brutal.ImGuiApi;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Combat workbench using the real fixed-tick simulation; round/match flow is separate.</summary>
internal sealed partial class BattleView : ICombatModelDriver, IDisposable
{
    private readonly Ruleset rules;
    private readonly ModelCatalog catalog;
    private readonly ModelRenderer renderer = new();
    private readonly Dictionary<ModelSource, GpuModel> models = [];
    private readonly Dictionary<int, (GpuModel Model, CombatModelAnimator Animator,CombatUnit Unit)> actors = [];
    private PlaceholderCreature? placeholder;
    private readonly RosterModelView bench = new();
    internal IReadOnlySet<int> RenderedBenchUnits => bench.RenderedUnits;
    public int? DraggedUnit { get => bench.DraggedUnit; set => bench.DraggedUnit = value; }
    public WorldPosition? DragPosition { get => bench.DragPosition; set => bench.DragPosition = value; }
    public int? HoveredBenchUnit { get => bench.HoveredUnit; set => bench.HoveredUnit = value; }
    /// <summary>ui.hoverCombatId: the hovered fighter grows to 1.08 and its ring turns white.</summary>
    public int? HoveredUnit { get; set; }
    /// <summary>Element color of a caster's ability; its ring takes it while the cast glow lasts.</summary>
    public Func<CombatUnit, string>? CastColor { get; set; }
    private readonly UnitRing ring = new();
    private readonly ArenaEnvironment board;
    private readonly bool ownsBoard;
    public string BoardTheme { get; set; } = "file-island";
    public int BoardDrawCalls => board.DrawCalls;
    public double SceneTime { get=>board.Time; set=>board.Time=value; }
    private readonly CombatChoreography choreography;
    private readonly CombatBoardBursts boardBursts;
    private static readonly Vector3 allyBurst=BoardLighting.Linear("#6fd3ff"),enemyBurst=BoardLighting.Linear("#ff7a6a");
    internal int BoardFlashCount=>boardBursts.FlashCount;
    private double accumulator;
    private bool paused;
    private bool mirror;
    public CombatSimulation Combat { get; }
    public int DrawCalls { get; private set; }
    internal int FallbackDrawCalls { get; private set; }
    public bool BackRequested { get; private set; }
    public int ModelCount => models.Count;
    public bool Mirrored => mirror;

    public BattleView(Ruleset rules, CombatEntry[]? roster = null, int columns = 7, ModelCatalog? modelCatalog = null)
    {
        catalog=modelCatalog??ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory,"content/models.json"));
        board=new(); ownsBoard=true;
        this.rules = rules;
        var side = roster is { Length: > 0 } ? roster : rules.Lines.Take(6).Select((line, i) => new CombatEntry(line.Id, i % 3, i % columns, 4 + i / columns, Items: [])).ToArray();
        var enemy = rules.Lines.Skip(3).Take(side.Length).Select((line, i) => new CombatEntry(line.Id, i % 3, i % columns, 4 + i / columns, Items: [])).ToArray();
        Combat = new(rules, side, enemy, 84219, columns, setup: new(new(), new()));
        boardBursts=new(columns,false);
        choreography = new(Combat, this); choreography.Update(0); InitializeMoveEffects();
    }
    public BattleView(Ruleset rules, CombatSimulation combat, bool mirror,ArenaEnvironment? environment=null,ModelCatalog? modelCatalog=null)
    {
        catalog=modelCatalog??ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory,"content/models.json"));
        board=environment??new(); ownsBoard=environment is null; board.Feedback.ClearFlashes();
        this.rules = rules; Combat = combat; this.mirror = mirror;
        boardBursts=new(combat.Columns,mirror);
        choreography = new(combat, this); choreography.Update(0, mirror); InitializeMoveEffects();
    }
    public void Create(CombatUnit unit)
    {
        string? overrideKey = CaptureModelSource is { } forced && catalog.Sources.ContainsKey(forced) ? forced : null;
        var source = overrideKey is not null ? catalog.Sources[overrideKey] : catalog.Resolve(rules.Id, unit.Line.Id, unit.Stage); if (source is null) return;
        if (!models.TryGetValue(source, out var model)) { model = new(catalog, source); models.Add(source, model); }
        string key=overrideKey ?? catalog.Variants[rules.Id][unit.Line.Id][unit.Stage]!;
        actors[unit.Id] = (model, new CombatModelAnimator(model.Rig,key,(ModelAnimator.FindIdle(model.Rig)?.Duration ?? 0) * .17),unit);
    }
    public void Remove(int unit) => actors.Remove(unit);
    public void UpdateBench(MatchPlayer player, MatchPlayer? partner, double delta)
    {
        bool wide = Combat.Columns > 7;
        var entries = player.Roster.Entries.Where(e => !e.Spot.IsBoard)
            .Select(e => (e.Unit, CombatLayout.Bench(e.Spot.Index, wide)));
        if (wide && partner is not null)
            entries = entries.Concat(partner.Roster.Entries.Where(e => !e.Spot.IsBoard)
                .Select(e => (e.Unit, CombatLayout.Bench(e.Spot.Index, true, true))));
        bench.Update(rules, entries, delta);
    }
    public bool Play(int unit, string role)
    {
        if (role == "special") OfferCutIn(unit);
        return actors.TryGetValue(unit, out var actor) && actor.Animator.Play(role);
    }
    public void Update(int unit, double delta)
    {
        if(actors.TryGetValue(unit,out var actor))actor.Animator.Update(delta,actor.Unit.Alive&&actor.Unit.MoveElapsed<actor.Unit.MoveDuration&&actor.Unit.MoveDuration>=.3&&actor.Unit.MoveOrigin!=actor.Unit.Cell);
    }
    public double MovementHeight(int unit,double sourceHeight)=>actors.TryGetValue(unit,out var actor)&&actor.Unit.MoveDuration>=.3&&!actor.Animator.UsesTravelHop?actor.Animator.GlideOffset:sourceHeight;
    public void Advance(double delta, int speed = 1)
    {
        SceneTime+=Math.Clamp(delta,0,.1);
        if (paused) return;
        double playbackDelta = Math.Clamp(delta, 0, .1) * speed;
        accumulator += playbackDelta;
        while (accumulator >= CombatSimulation.Tick && !Combat.Done) { Combat.Step(); accumulator -= CombatSimulation.Tick; }
        choreography.Update(delta, mirror, HoveredUnit); AdvanceMoveEffects(playbackDelta); UpdateCinematic(delta, speed);
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
    internal IEnumerable<(CombatUnit Unit, Brutal.Numerics.float2 Point)> InspectionTargets(int width, int height)
    {
        if(width<=0 || height<=0) yield break;
        var camera = board.Camera.Matrix(width, height, Combat.Columns);
        foreach (var unit in Combat.Units)
        {
            if (!unit.Alive) continue;
            var world=CombatLayout.Unit(unit,Combat.Columns,mirror);
            // The source pickCombat uses a fixed .7 body anchor, independent of jump height.
            var point = Vector4.Transform(new Vector4((float)world.X, .7f, (float)world.Z, 1), camera);
            float x = (point.X / point.W * .5f + .5f) * width;
            float y = (1 - (point.Y / point.W * .5f + .5f)) * height;
            yield return (unit, new(x,y));
        }
    }
    public void Labels(int width, int height, int windowHeight, int left = 0, int bottom = 0,ImDrawListPtr? layer=null)
    {
        if (width <= 0 || height <= 0) return;
        var camera = board.Camera.Matrix(width, height, Combat.Columns); var draw = layer??ImGui.GetForegroundDrawList();
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
        FrameCinematic(width,height); var camera=MoveCamera(board.Camera.Matrix(width,height,Combat.Columns)); board.SetTheme(BoardTheme); DrawCalls=FallbackDrawCalls=0;
        bench.RenderedUnits.Clear();
        board.Feedback.Time=Combat.Time;
        boardBursts.Update(Combat.Events,flash=>board.Feedback.Flash(flash.Column,flash.Row,flash.Until,flash.ViewTeam==0?allyBurst:enemyBurst));
        void Units(Matrix4x4 viewCamera,bool transparent)
        {
            DrawCalls += bench.Draw(renderer, viewCamera, transparent);
            foreach(var view in choreography.Views)
            {
                var unit=Combat.Units[view.UnitId];
                string ringColor=view.Glow>0&&CastColor is {} cast?cast(unit):view.UnitId==HoveredUnit?"#ffffff":UnitRing.TierColor(unit.Line.Tier);
                ring.Draw(renderer,viewCamera,transparent,view.Position.X,view.Position.Y,view.Position.Z,ringColor,1,view.Scale,view.RingScale);
                if(!actors.TryGetValue(view.UnitId,out var actor))
                {
                    if(!transparent)
                    {
                        var placement=GpuModel.Transform(view.BodyOffset.X,view.BodyOffset.Y,view.BodyOffset.Z,view.Yaw,1)*GpuModel.Transform(view.Position.X,view.Position.Y,view.Position.Z,0,view.Scale);
                        int drawn=(placeholder??=new()).Draw(renderer,viewCamera,placement,rules.Id,unit.Line.Id,fog:true,emission:view.Flash>0?view.Flash:view.Glow);
                        if(!renderer.ShadowPass){DrawCalls+=drawn;FallbackDrawCalls+=drawn;}
                    }
                    continue;
                }
                // A hit flash is white; the cast glow takes the special's element colour (render3d.js UnitView.update).
                Vector3? tint=view.Flash<=0&&view.Glow>0&&CastColor is {} glowColor?BoardLighting.Linear(glowColor(unit)):null;
                renderer.Draw(actor.Model,actor.Animator.Pose,actor.Model.Place(view,unit.Stage,unit.Line.Tier),viewCamera,transparent,view.Flash>0?view.Flash:view.Glow,tint:tint);
                if(!renderer.ShadowPass) DrawCalls+=renderer.DrawCalls;
            }
            if(!renderer.ShadowPass)DrawCalls+=moveEffects!.DrawLit(renderer,viewCamera,transparent);
        }
        board.Draw(renderer,camera,board.Camera.Eye(width,height,Combat.Columns),Combat.Columns,false,left,bottom,width,height,Units);
        DrawMoveEffects(camera,width,height);
    }
    public void Dispose() { if (board.Camera.Cinematic is not null) board.Camera.Cinematic = null; cutInView?.Dispose(); combatMoves?.Clear();moveEffects?.Dispose(); placeholder?.Dispose();ring.Dispose();bench.Dispose(); foreach (var model in models.Values) model.Dispose(); models.Clear(); if(ownsBoard) board.Dispose(); renderer.Dispose(); }
}

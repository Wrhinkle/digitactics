using System.Numerics;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed class PlanningView : IDisposable
{
    private readonly RosterModelView roster = new();
    // ui.previewSide: the next opponent's side (the wild wave in PvE) mirrored onto the enemy rows at .45 opacity.
    private readonly RosterModelView preview = new() { Opacity = .45, Yaw = 0 };
    private readonly ModelRenderer renderer = new();
    private readonly ArenaEnvironment board;
    private readonly bool ownsBoard;
    public PlanningView(ArenaEnvironment? environment=null) { board=environment??new(); ownsBoard=environment is null; }
    public string BoardTheme { get; set; } = "file-island";
    public int BoardDrawCalls => board.DrawCalls;
    public double SceneTime { get=>board.Time; set=>board.Time=value; }
    public int DrawCalls { get; private set; }
    public IReadOnlySet<int> RenderedUnits => roster.RenderedUnits;
    public int? DraggedUnit { get => roster.DraggedUnit; set => roster.DraggedUnit = value; }
    public WorldPosition? DragPosition { get => roster.DragPosition; set => roster.DragPosition = value; }
    public int? HoveredUnit { get => roster.HoveredUnit; set => roster.HoveredUnit = value; }
    /// <summary>Preview units by their side's own cell (rows 4-7), with stage and items, for labels.</summary>
    public IReadOnlyList<CombatEntry> PreviewSide { get; private set; } = [];
    public void Update(Ruleset rules, MatchPlayer player, double delta, IReadOnlyList<CombatEntry>? previewSide = null)
    {
        SceneTime+=Math.Clamp(delta,0,.1);
        roster.Update(rules, player.Roster.Entries.Select(e => (e.Unit, e.Spot.IsBoard
            ? CombatLayout.Cell(new(e.Spot.Column, e.Spot.Row), 7, false)
            : CombatLayout.Bench(e.Spot.Index))), delta);
        PreviewSide = previewSide ?? [];
        preview.Update(rules, PreviewSide.Select((u, i) => (new PlanningUnit(i, u.Line, u.Stage, u.Items ?? []), PreviewCell(u))), delta);
    }
    /// <summary>render3d.js: cellWorld(COLS - 1 - c, ROWS - 1 - r).</summary>
    internal static WorldPosition PreviewCell(CombatEntry unit) => CombatLayout.Cell(new(6 - unit.Column, 7 - unit.Row), 7, false);
    public Brutal.Numerics.float2 Cell(int column, int row, int width, int height)
        => Project(CombatLayout.Cell(new(column, row), 7, false), width, height);
    public Brutal.Numerics.float2 Bench(int index, int width, int height)
        => Project(CombatLayout.Bench(index), width, height);
    /// <summary>Screen point of a world position at height <paramref name="y"/>, for unit labels.</summary>
    internal Brutal.Numerics.float2 Point(WorldPosition position, double y, int width, int height)
    {
        var point = Vector4.Transform(new Vector4((float)position.X, (float)y, (float)position.Z, 1), board.Camera.Matrix(width, height, 7));
        return new((point.X / point.W * .5f + .5f) * width, (1 - (point.Y / point.W * .5f + .5f)) * height);
    }
    private Brutal.Numerics.float2 Project(WorldPosition position, int width, int height)
    {
        var point = Vector4.Transform(new Vector4((float)position.X, .14f, (float)position.Z, 1), board.Camera.Matrix(width, height, 7));
        return new((point.X / point.W * .5f + .5f) * width, (1 - (point.Y / point.W * .5f + .5f)) * height);
    }
    public void Draw(Ruleset rules, MatchPlayer player, int left, int bottom, int width, int height)
    {
        var camera = board.Camera.Matrix(width,height,7); board.SetTheme(BoardTheme); DrawCalls=0; roster.RenderedUnits.Clear();
        void Units(Matrix4x4 viewCamera,bool transparent)
        {
            DrawCalls += roster.Draw(renderer, viewCamera, transparent);
            DrawCalls += preview.Draw(renderer, viewCamera, transparent);
        }
        board.Draw(renderer,camera,board.Camera.Eye(width,height,7),7,true,left,bottom,width,height,Units);
    }
    public void Dispose() { roster.Dispose(); preview.Dispose(); if(ownsBoard) board.Dispose(); renderer.Dispose(); }
}

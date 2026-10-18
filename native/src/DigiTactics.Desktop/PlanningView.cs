using System.Numerics;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed class PlanningView : IDisposable
{
    private readonly RosterModelView roster = new();
    private readonly ModelRenderer renderer = new();
    private readonly ArenaEnvironment board;
    private readonly bool ownsBoard;
    public PlanningView(ArenaEnvironment? environment=null) { board=environment??new(); ownsBoard=environment is null; }
    public string BoardTheme { get; set; } = "file-island";
    public int BoardDrawCalls => board.DrawCalls;
    public double SceneTime { get=>board.Time; set=>board.Time=value; }
    public int DrawCalls { get; private set; }
    public IReadOnlySet<int> RenderedUnits => roster.RenderedUnits;
    public void Update(Ruleset rules, MatchPlayer player, double delta)
    {
        SceneTime+=Math.Clamp(delta,0,.1);
        roster.Update(rules, player.Roster.Entries.Select(e => (e.Unit, e.Spot.IsBoard
            ? CombatLayout.Cell(new(e.Spot.Column, e.Spot.Row), 7, false)
            : CombatLayout.Bench(e.Spot.Index))), delta);
    }
    public static Brutal.Numerics.float2 Cell(int column, int row, int width, int height)
        => Project(CombatLayout.Cell(new(column, row), 7, false), width, height);
    public static Brutal.Numerics.float2 Bench(int index, int width, int height)
        => Project(CombatLayout.Bench(index), width, height);
    private static Brutal.Numerics.float2 Project(WorldPosition position, int width, int height)
    {
        var point = Vector4.Transform(new Vector4((float)position.X, .14f, (float)position.Z, 1), BattleView.Camera(width, height, 7));
        return new((point.X / point.W * .5f + .5f) * width, (1 - (point.Y / point.W * .5f + .5f)) * height);
    }
    public void Draw(Ruleset rules, MatchPlayer player, int left, int bottom, int width, int height)
    {
        var camera = BattleView.Camera(width,height,7); board.SetTheme(BoardTheme); DrawCalls=0; roster.RenderedUnits.Clear();
        void Units(Matrix4x4 viewCamera,bool transparent)
        {
            DrawCalls += roster.Draw(renderer, viewCamera, transparent);
        }
        board.Draw(renderer,camera,BattleView.CameraEye(width,height,7),7,true,left,bottom,width,height,Units);
    }
    public void Dispose() { roster.Dispose(); if(ownsBoard) board.Dispose(); renderer.Dispose(); }
}

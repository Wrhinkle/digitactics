using System.Numerics;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed class PlanningView : IDisposable
{
    private readonly ModelCatalog catalog = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
    private readonly ModelRenderer renderer = new();
    private readonly ArenaEnvironment board = new();
    public string BoardTheme { get; set; } = "file-island";
    public int BoardDrawCalls => board.DrawCalls;
    public double SceneTime { get=>board.Time; set=>board.Time=value; }
    private readonly Dictionary<ModelSource, GpuModel> models = [];
    private readonly Dictionary<int, (GpuModel Model, ModelAnimator Animator)> actors = [];
    public int DrawCalls { get; private set; }
    public void Update(Ruleset rules, MatchPlayer player, double delta)
    {
        SceneTime+=Math.Clamp(delta,0,.1);
        var live = player.Roster.Board.Select(e => e.Unit.Id).ToHashSet();
        foreach (int id in actors.Keys.Where(id => !live.Contains(id)).ToArray()) actors.Remove(id);
        foreach (var entry in player.Roster.Board)
        {
            var source = catalog.Resolve(rules.Id, entry.Unit.Line, entry.Unit.Stage);
            if (source is null) continue;
            if (!actors.TryGetValue(entry.Unit.Id, out var existing) || existing.Model.Source != source)
            {
                if (!models.TryGetValue(source, out var model)) models[source] = model = new(catalog, source);
                actors[entry.Unit.Id] = (model, new ModelAnimator(model.Rig, (entry.Unit.Id % 7) * .13));
            }
            actors[entry.Unit.Id].Animator.Update(delta);
        }
        var used = actors.Values.Select(a => a.Model.Source).ToHashSet();
        foreach (var source in models.Keys.Where(s => !used.Contains(s)).ToArray()) { models[source].Dispose(); models.Remove(source); }
    }
    public static Brutal.Numerics.float2 Cell(int column, int row, int width, int height)
    {
        var position = CombatLayout.Cell(new(column, row), 7, false);
        var point = Vector4.Transform(new Vector4((float)position.X, .14f, (float)position.Z, 1), BattleView.Camera(width, height, 7));
        return new((point.X / point.W * .5f + .5f) * width, (1 - (point.Y / point.W * .5f + .5f)) * height);
    }
    public void Draw(Ruleset rules, MatchPlayer player, int left, int bottom, int width, int height)
    {
        var camera = BattleView.Camera(width,height,7); board.SetTheme(BoardTheme); DrawCalls=0;
        void Units(Matrix4x4 viewCamera,bool transparent)
        {
            foreach(var entry in player.Roster.Board)
            {
                if(!actors.TryGetValue(entry.Unit.Id,out var actor)) continue;
                var position=CombatLayout.Cell(new(entry.Spot.Column,entry.Spot.Row),7,false);
                var view=new CombatVisual(entry.Unit.Id,position,Math.PI,default,1,0,0,1,null,false);
                var line=rules.Lines.Concat(rules.ExtraLines).Single(l=>l.Id==entry.Unit.Line);
                renderer.Draw(actor.Model,actor.Animator.Pose,actor.Model.Place(view,entry.Unit.Stage,line.Tier),viewCamera,transparent);
                if(!renderer.ShadowPass) DrawCalls+=renderer.DrawCalls;
            }
        }
        board.Draw(renderer,camera,BattleView.CameraEye(width,height,7),7,true,left,bottom,width,height,Units);
    }
    public void Dispose() { foreach (var model in models.Values) model.Dispose(); models.Clear(); board.Dispose(); renderer.Dispose(); }
}

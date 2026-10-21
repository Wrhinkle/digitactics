using System.Numerics;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Idle roster actors shared by planning and combat benches. Gameplay IDs stay
/// separate from combat IDs, and the caller supplies the arena's lit/shadow renderer.</summary>
internal sealed class RosterModelView(ModelCatalog? modelCatalog = null) : IDisposable
{
    private readonly ModelCatalog catalog = modelCatalog ?? ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
    private readonly Dictionary<ModelSource, GpuModel> models = [];
    private readonly Dictionary<int, (GpuModel Model, ModelAnimator Animator)> actors = [];
    private readonly List<(PlanningUnit Unit, WorldPosition Position, int Tier)> entries = [];
    // Displayed positions: a dropped or moved unit glides into its new cell and a picked-up unit rises to the drag
    // height instead of teleporting. Horizontal drag follows the cursor exactly.
    private readonly Dictionary<int, WorldPosition> shown = [];
    internal const double SettleRate = 18, LiftRate = 25;
    private PlaceholderCreature? placeholder;
    private string variant="";
    public HashSet<int> RenderedUnits { get; } = [];
    public int? DraggedUnit { get; set; }
    public WorldPosition? DragPosition { get; set; }

    public void Update(Ruleset rules, IEnumerable<(PlanningUnit Unit, WorldPosition Position)> roster, double delta)
    {
        entries.Clear();variant=rules.Id;
        foreach (var entry in roster)
        {
            var line = rules.Lines.Concat(rules.ExtraLines).Single(l => l.Id == entry.Unit.Line);
            entries.Add((entry.Unit, entry.Position, line.Tier));
            var source = catalog.Resolve(rules.Id, entry.Unit.Line, entry.Unit.Stage);
            if (source is null) { actors.Remove(entry.Unit.Id); continue; }
            if (!actors.TryGetValue(entry.Unit.Id, out var actor) || actor.Model.Source != source)
            {
                if (!models.TryGetValue(source, out var model)) models[source] = model = new(catalog, source);
                actors[entry.Unit.Id] = actor = (model, new ModelAnimator(model.Rig, (entry.Unit.Id % 7) * .13));
            }
            actor.Animator.Update(delta);
        }
        foreach (var entry in entries)
        {
            var target = Target(entry.Unit.Id, entry.Position);
            if (!shown.TryGetValue(entry.Unit.Id, out var current)) { shown[entry.Unit.Id] = target; continue; }
            double lift = 1 - Math.Exp(-LiftRate * delta), settle = 1 - Math.Exp(-SettleRate * delta);
            bool dragged = entry.Unit.Id == DraggedUnit && DragPosition is not null;
            var next = new WorldPosition(dragged ? target.X : current.X + (target.X - current.X) * settle, current.Y + (target.Y - current.Y) * lift,
                dragged ? target.Z : current.Z + (target.Z - current.Z) * settle);
            if (Math.Abs(next.X - target.X) + Math.Abs(next.Y - target.Y) + Math.Abs(next.Z - target.Z) < 1e-3) next = target;
            shown[entry.Unit.Id] = next;
        }
        var live = entries.Select(e => e.Unit.Id).ToHashSet();
        foreach (int id in actors.Keys.Where(id => !live.Contains(id)).ToArray()) actors.Remove(id);
        foreach (int id in shown.Keys.Where(id => !live.Contains(id)).ToArray()) shown.Remove(id);
        var used = actors.Values.Select(a => a.Model.Source).ToHashSet();
        foreach (var source in models.Keys.Where(s => !used.Contains(s)).ToArray()) { models[source].Dispose(); models.Remove(source); }
    }

    public int Draw(ModelRenderer renderer, Matrix4x4 camera, bool transparent)
    {
        int calls = 0;
        foreach (var entry in entries)
        {
            var position = shown.TryGetValue(entry.Unit.Id, out var displayed) ? displayed : Target(entry.Unit.Id, entry.Position);
            var view = new CombatVisual(entry.Unit.Id, position, Math.PI, default, 1, 0, 0, 1, null, false);
            if(!actors.TryGetValue(entry.Unit.Id,out var actor))
            {
                if(!transparent)
                {
                    int drawn=(placeholder??=new()).Draw(renderer,camera,GpuModel.Transform(position.X,position.Y,position.Z,Math.PI,1),variant,entry.Unit.Line,fog:true);
                    if(!renderer.ShadowPass){calls+=drawn;RenderedUnits.Add(entry.Unit.Id);}
                }
                continue;
            }
            renderer.Draw(actor.Model, actor.Animator.Pose, actor.Model.Place(view, entry.Unit.Stage, entry.Tier), camera, transparent);
            if (!renderer.ShadowPass)
            {
                calls += renderer.DrawCalls;
                if (renderer.DrawCalls > 0) RenderedUnits.Add(entry.Unit.Id);
            }
        }
        return calls;
    }

    private WorldPosition Target(int unit, WorldPosition cell) => unit == DraggedUnit && DragPosition is { } drag ? drag : cell;

    public void Dispose()
    {
        placeholder?.Dispose();placeholder=null;
        foreach (var model in models.Values) model.Dispose();
        models.Clear(); actors.Clear(); entries.Clear(); shown.Clear(); RenderedUnits.Clear();
    }
}

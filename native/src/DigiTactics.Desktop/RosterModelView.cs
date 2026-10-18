using System.Numerics;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Idle roster actors shared by planning and combat benches. Gameplay IDs stay
/// separate from combat IDs, and the caller supplies the arena's lit/shadow renderer.</summary>
internal sealed class RosterModelView : IDisposable
{
    private readonly ModelCatalog catalog = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
    private readonly Dictionary<ModelSource, GpuModel> models = [];
    private readonly Dictionary<int, (GpuModel Model, ModelAnimator Animator)> actors = [];
    private readonly List<(PlanningUnit Unit, WorldPosition Position, int Tier)> entries = [];
    public HashSet<int> RenderedUnits { get; } = [];

    public void Update(Ruleset rules, IEnumerable<(PlanningUnit Unit, WorldPosition Position)> roster, double delta)
    {
        entries.Clear();
        foreach (var entry in roster)
        {
            var source = catalog.Resolve(rules.Id, entry.Unit.Line, entry.Unit.Stage);
            if (source is null) { actors.Remove(entry.Unit.Id); continue; }
            if (!actors.TryGetValue(entry.Unit.Id, out var actor) || actor.Model.Source != source)
            {
                if (!models.TryGetValue(source, out var model)) models[source] = model = new(catalog, source);
                actors[entry.Unit.Id] = actor = (model, new ModelAnimator(model.Rig, (entry.Unit.Id % 7) * .13));
            }
            actor.Animator.Update(delta);
            var line = rules.Lines.Concat(rules.ExtraLines).Single(l => l.Id == entry.Unit.Line);
            entries.Add((entry.Unit, entry.Position, line.Tier));
        }
        var live = entries.Select(e => e.Unit.Id).ToHashSet();
        foreach (int id in actors.Keys.Where(id => !live.Contains(id)).ToArray()) actors.Remove(id);
        var used = actors.Values.Select(a => a.Model.Source).ToHashSet();
        foreach (var source in models.Keys.Where(s => !used.Contains(s)).ToArray()) { models[source].Dispose(); models.Remove(source); }
    }

    public int Draw(ModelRenderer renderer, Matrix4x4 camera, bool transparent)
    {
        int calls = 0;
        foreach (var entry in entries)
        {
            var actor = actors[entry.Unit.Id];
            var view = new CombatVisual(entry.Unit.Id, entry.Position, Math.PI, default, 1, 0, 0, 1, null, false);
            renderer.Draw(actor.Model, actor.Animator.Pose, actor.Model.Place(view, entry.Unit.Stage, entry.Tier), camera, transparent);
            if (!renderer.ShadowPass)
            {
                calls += renderer.DrawCalls;
                if (renderer.DrawCalls > 0) RenderedUnits.Add(entry.Unit.Id);
            }
        }
        return calls;
    }

    public void Dispose()
    {
        foreach (var model in models.Values) model.Dispose();
        models.Clear(); actors.Clear(); entries.Clear(); RenderedUnits.Clear();
    }
}

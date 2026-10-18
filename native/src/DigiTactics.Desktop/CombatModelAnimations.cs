using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Shares prepared rigs across a battle while each unit owns its playback state.</summary>
internal sealed class CombatModelAnimations(ModelCatalog catalog, string assetRoot, string variant, Func<double>? idlePhase = null) : ICombatModelDriver
{
    private readonly Dictionary<ModelSource, GltfRig> rigs = [];
    private readonly Dictionary<int, (CombatModelAnimator Animator,CombatUnit Unit)> units = [];
    public RigPose? Pose(int unit) => units.TryGetValue(unit, out var actor) ? actor.Animator.Pose : null;
    public void Create(CombatUnit unit)
    {
        var source = catalog.Resolve(variant, unit.Line.Id, unit.Stage);
        if (source is null) return;
        if (!rigs.TryGetValue(source, out var rig))
        {
            using var asset = catalog.Open(assetRoot, source.Asset);
            rig = GltfRig.Load(asset).Prepare(source, assetRoot); rigs.Add(source, rig);
        }
        double duration = ModelAnimator.FindIdle(rig)?.Duration ?? 0;
        string key=catalog.Variants[variant][unit.Line.Id][unit.Stage]!;
        units[unit.Id] = (new(rig,key,(idlePhase?.Invoke() ?? Random.Shared.NextDouble()) * duration),unit);
    }
    public void Remove(int unit) => units.Remove(unit);
    public bool Play(int unit, string role) => units.TryGetValue(unit, out var actor) && actor.Animator.Play(role);
    public void Update(int unit, double delta)
    {
        if(units.TryGetValue(unit,out var actor))actor.Animator.Update(delta,actor.Unit.Alive&&actor.Unit.MoveElapsed<actor.Unit.MoveDuration&&actor.Unit.MoveDuration>=.3&&actor.Unit.MoveOrigin!=actor.Unit.Cell);
    }
    public double MovementHeight(int unit,double sourceHeight)=>units.TryGetValue(unit,out var actor)&&actor.Unit.MoveDuration>=.3&&!actor.Animator.UsesTravelHop?actor.Animator.GlideOffset:sourceHeight;
}

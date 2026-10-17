using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Shares prepared rigs across a battle while each unit owns its playback state.</summary>
internal sealed class CombatModelAnimations(ModelCatalog catalog, string assetRoot, string variant, Func<double>? idlePhase = null) : ICombatModelDriver
{
    private readonly Dictionary<ModelSource, GltfRig> rigs = [];
    private readonly Dictionary<int, ModelAnimator> units = [];
    public RigPose? Pose(int unit) => units.TryGetValue(unit, out var animator) ? animator.Pose : null;
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
        units[unit.Id] = new(rig, (idlePhase?.Invoke() ?? Random.Shared.NextDouble()) * duration);
    }
    public void Remove(int unit) => units.Remove(unit);
    public bool Play(int unit, string role) => units.TryGetValue(unit, out var animator) && animator.Play(role);
    public void Update(int unit, double delta) { if (units.TryGetValue(unit, out var animator)) animator.Update(delta); }
}

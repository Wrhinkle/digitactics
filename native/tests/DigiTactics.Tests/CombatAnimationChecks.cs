using DigiTactics.Assets;

/// <summary>Board playback continuity and scale stability through the production combat animator
/// (see CombatAnimationProbe; the whole catalog runs with DigiTactics.Desktop --animation-stability).</summary>
internal static class CombatAnimationChecks
{
    // Renamon's three forms in every variant, plus sources whose special or attack used to drop out on its last frame.
    private static readonly string[] Sources =
    [
        "catalog:CS_Renamon:chr391", "catalog:TS_Renamon:chr391", "catalog:L_Kyubimon:chr395", "renamon:0", "renamon:1", "renamon:2",
        "ac:Airdramon", "ac:Whamon", "catalog:CS_Palmon:chr348", "catalog:CS_Ginryumon:chr191", "drg:AncientGreymon", "agumon:2", "palmon:0",
    ];

    public static int Run(string root)
    {
        var catalog = ModelCatalog.Load(Path.Combine(root, "content/models.json"));
        string assets = Path.Combine(root, "artifacts/model-content"); int count = 0;
        void Check(bool condition, string message) { count++; if (!condition) throw new InvalidOperationException("Combat animation: " + message); }
        foreach (string key in Sources)
        {
            var source = catalog.Sources[key]; using var asset = catalog.Open(assets, source.Asset);
            var rig = GltfRig.Load(asset).Prepare(source, assets); var mesh = GltfMesh.Load(asset);
            var report = CombatAnimationProbe.Measure(key, source.Asset, rig, mesh);
            Check(report.Error is null, key + " sampled without error");
            foreach (var (role, scale) in report.MaxBodyScaleByRole) Check(scale <= CombatAnimationProbe.MaxBodyScale, $"{key} {role} body scale {scale:0.000} stays near idle size");
            foreach (var (role, scale) in report.MinBodyScaleByRole) Check(scale >= CombatAnimationProbe.MinBodyScale, $"{key} {role} body scale {scale:0.000} stays near idle size");
            foreach (var change in report.Switches)
                Check(!change.Pop, $"{key} {change.Kind} at {change.Time:0.00}s moves a joint {change.Jump:0.000} idle heights in one frame (in-clip peak {change.Reference:0.000})");
            Check(report.Switches.Any(s => s.Kind.StartsWith("end-", StringComparison.Ordinal)), key + " exercised an action ending");
        }
        // Zero delta must not advance a crossfade, and a finished crossfade hands the pose back exactly.
        {
            const string key = "catalog:CS_Renamon:chr391";
            var source = catalog.Sources[key]; using var asset = catalog.Open(assets, source.Asset);
            var rig = GltfRig.Load(asset).Prepare(source, assets);
            var combat = new CombatModelAnimator(rig, key); var mixer = new ModelAnimator(rig);
            for (int i = 0; i < 20; i++) { combat.Update(1d / 60, false); mixer.Update(1d / 60); }
            Check(combat.Play("attack") && mixer.Play("attack"), key + " attack starts");
            combat.Update(1d / 60, false); mixer.Update(1d / 60);
            var held = combat.Pose.Local.ToArray(); combat.Update(0, false); mixer.Update(0);
            Check(combat.Blending && held.SequenceEqual(combat.Pose.Local), key + " zero delta holds the crossfade");
            for (int i = 0; i < 10; i++) { combat.Update(1d / 60, false); mixer.Update(1d / 60); }
            Check(!combat.Blending && combat.Pose.Local.SequenceEqual(mixer.Pose.Local), key + " attack owns the exact mixer pose after the start blend");
            for (int i = 0; i < 60; i++) { combat.Update(1d / 60, false); mixer.Update(1d / 60); }
            Check(combat.Role is null && !combat.Blending && combat.Pose.Local.SequenceEqual(mixer.Pose.Local), key + " idle owns the exact mixer pose after the end blend");
        }
        Console.WriteLine($"Combat animation: {count} scale, continuity and crossfade checks passed.");
        return count;
    }
}

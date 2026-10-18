using DigiTactics.Assets;
using System.Text.Json;

internal static class CompanionLocomotionChecks
{
    public static int Run(string root)
    {
        var models = ModelCatalog.Load(Path.Combine(root, "content/models.json"));
        var companions = CompanionCatalog.Load(Path.Combine(root, "content/companion-models.json"));
        int checks = 0;
        void Check(bool value, string message) { checks++; if (!value) throw new InvalidOperationException("Companion locomotion: " + message); }
        foreach (var (id, leftName, rightName) in new[] {
            ("agumon:2", "J_kakato_L", "J_kakato_R"), ("palmon:0", "J_kakato_L", "J_kakato_R"),
            ("agumon:0", "J_foot_l_012", "J_foot_r_016"), ("coronamon:0", "Bip001 L Foot", "Bip001 R Foot"),
            ("renamon:0", "J_unkleL", "J_unkleR"), ("hagurumon:1", "J_asi_L_07", "J_asi_R_011") })
        {
            var visual = companions.Forms.Values.Concat(companions.Bases.Values).First(v => v.Source == id); var source = models.Sources[visual.Source];
            using var asset = models.Open(Path.Combine(root, "artifacts/model-content"), source.Asset);
            var rig = GltfRig.Load(asset).Prepare(source, Path.Combine(root, "artifacts/model-content"));
            var still = new CompanionAnimator(rig, visual, companions.Fallbacks);
            var moving = new CompanionAnimator(rig, visual, companions.Fallbacks);
            moving.SetMoving(true);
            for (int i = 0; i < 17; i++) { still.Update(1d / 60); moving.Update(1d / 60); }
            int left = Array.FindIndex(rig.Nodes, n => n.Name == leftName);
            int right = Array.FindIndex(rig.Nodes, n => n.Name == rightName);
            Check(left >= 0 && right >= 0, id + " fixture has independently identified left and right feet");
            static double Distance(Matrix4D a, Matrix4D b) => Math.Sqrt(Math.Pow(a.M41 - b.M41, 2) + Math.Pow(a.M42 - b.M42, 2) + Math.Pow(a.M43 - b.M43, 2));
            double separation = Distance(still.Pose.World[left], still.Pose.World[right]);
            Check(Distance(moving.Pose.World[left], still.Pose.World[left]) > separation * .02 && Distance(moving.Pose.World[right], still.Pose.World[right]) > separation * .02,
                id + " moving must articulate both feet instead of keeping the idle skeleton");
            Check(moving.Pose.World.All(m => m.IsFinite), id + " moving pose remains finite");
            moving.SetMoving(false);
            for (int i = 0; i < 30; i++) { still.Update(1d / 60); moving.Update(1d / 60); }
            Check(Distance(moving.Pose.World[left], still.Pose.World[left]) < separation * 1e-5 && Distance(moving.Pose.World[right], still.Pose.World[right]) < separation * 1e-5,
                id + " stopping restores the current idle pose without a permanent stride offset");
        }
        foreach (var (id, foreleg) in new[] { ("ac:Dobermon", "Bip001 L Forearm"), ("ac:Grizzlymon", "J_elbow_l") })
        {
            var source = models.Sources[id];
            using var asset = models.Open(Path.Combine(root, "artifacts/model-content"), source.Asset);
            var rig = GltfRig.Load(asset).Prepare(source, Path.Combine(root, "artifacts/model-content"));
            var pose = new RigPose(rig); pose.Sample(ModelAnimator.FindIdle(rig), .3, loop: true);
            var locomotion = new CompanionLocomotion(rig, id, pose, false);
            Check(locomotion.LegCount == 4, id + " uses all four grounded limbs rather than only rear feet");
            int front = Array.FindIndex(rig.Nodes, n => n.Name == foreleg);
            Check(front >= 0, id + " independently identified foreleg exists");
            bool bends = false;
            foreach (double time in new[] { .3, .6 })
            {
                pose.Sample(ModelAnimator.FindIdle(rig), time, loop: true);
                var idleRotation = pose.Local[front].Rotation;
                locomotion.Apply(pose, true, time, .2);
                bends |= pose.Local[front].Rotation != idleRotation;
            }
            Check(bends, id + " front limb bends during a stride");
            locomotion.Reset(); pose.Sample(ModelAnimator.FindIdle(rig), .3, loop: true);
            var idle = pose.Local.ToArray(); locomotion.Apply(pose, false, .3, 0);
            Check(pose.Local.SequenceEqual(idle), id + " action reset immediately removes procedural gait");
        }
        foreach (string id in new[] { "hisyaryumon", "hagurumon" })
        {
            var visual = companions.Forms[id]; var source = models.Sources[visual.Source];
            using var asset = models.Open(Path.Combine(root, "artifacts/model-content"), source.Asset);
            var rig = GltfRig.Load(asset).Prepare(source, Path.Combine(root, "artifacts/model-content"));
            var still = new CompanionAnimator(rig, visual, companions.Fallbacks);
            var moving = new CompanionAnimator(rig, visual, companions.Fallbacks); moving.SetMoving(true);
            for (int i = 0; i < 17; i++) { still.Update(1d / 60); moving.Update(1d / 60); }
            Check(moving.LocomotionKind == CompanionLocomotionKind.Glide && moving.HasRun, id + " glides without the ground hopping fallback");
            Check(moving.Pose.Local.SequenceEqual(still.Pose.Local), id + " airborne anatomy keeps its authored idle articulation");
            Check(Math.Abs(moving.Transform.Position.Y - still.Transform.Position.Y) is > .005 and < .03, id + " bounded visible glide motion");
            moving.SetMoving(false); for (int i = 0; i < 30; i++) moving.Update(1d / 60);
            Check(moving.Transform.Position.Y == 0, id + " arrival clears glide offset");
        }
        var inventory = new List<object>();
        var reachable = companions.Bases.Values.Concat(companions.Forms.Values).Select(v => v.Source).ToHashSet(StringComparer.Ordinal);
        foreach (var (key, source) in models.Sources.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            using var asset = models.Open(Path.Combine(root, "artifacts/model-content"), source.Asset);
            var rig = GltfRig.Load(asset).Prepare(source, Path.Combine(root, "artifacts/model-content"));
            var pose = new RigPose(rig); pose.Sample(ModelAnimator.FindIdle(rig), 0);
            var authored = rig.Animations.FirstOrDefault(c => c.Name is "run" or "walk" or "move");
            var locomotion = new CompanionLocomotion(rig, key, pose, authored is not null);
            for (int frame = 0; frame < 12; frame++)
            {
                double time = (frame + 1) / 12d;
                pose.Sample(authored ?? ModelAnimator.FindIdle(rig), time, loop: true);
                double offset = locomotion.Apply(pose, true, time, 1d / 12);
                Check(pose.World.All(matrix => matrix.IsFinite) && double.IsFinite(offset), key + " sampled movement remains finite");
                Check(Math.Abs(offset) <= .0250001, key + " procedural glide cannot become an unbounded displacement");
            }
            inventory.Add(new { sourceKey = key, asset = source.Asset, motionSha256 = source.Motion!.Sha256,
                companionReachable = reachable.Contains(key), authoredClip = authored?.Name,
                companionMovement = locomotion.Kind.ToString(), completeLegChains = locomotion.LegCount,
                visualReviewStatus = "UNVERIFIED" });
        }
        string output = Path.Combine(root, "artifacts/companion-locomotion-inventory.json");
        File.WriteAllText(output, JsonSerializer.Serialize(new { schemaVersion = 1, sources = inventory }, new JsonSerializerOptions { WriteIndented = true }));
        Check(inventory.Count == models.Sources.Count, "every catalog source appears in the locomotion inventory");
        Console.WriteLine($"Companion locomotion: {checks} articulated movement and arrival checks passed.");
        return checks;
    }
}

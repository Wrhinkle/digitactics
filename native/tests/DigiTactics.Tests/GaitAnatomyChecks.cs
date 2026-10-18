using System.Security.Cryptography;
using System.Text.Json;
using DigiTactics.Assets;

/// <summary>Actual imported limb and skin behavior; changing joint quaternions alone is not a pass.</summary>
internal static class GaitAnatomyChecks
{
    public static int Run(string root)
    {
        string catalogPath = Path.Combine(root, "content/models.json");
        var catalog = ModelCatalog.Load(catalogPath);
        string payload = Path.Combine(root, "artifacts/model-content");
        int checks = 0;
        var failures = new List<string>();
        var evidence = new List<object>();
        string[] importedDirectCandidates = ["commandramon:0", "extra:Monzaemon", "monzaemon:0", "monzaemon:1", "monzaemon:2",
            "drg:Varodurumon", "drg:ToyAgumon", "drg:Penguinmon", "unique:TS_Muchomon", "unique:TS_Terriermon_Assistant",
            "catalog:CS_Lopmon:chr750", "catalog:L_ToyAgumon:chr708", "catalog:TS_Terriermon_Lopmon_and_Assistant:chr701",
            "catalog:TS_Terriermon_Lopmon_and_Assistant:chr750", "catalog:TS_ToyAgumon:chr708"];
        void Check(bool condition, string message) { checks++; if (!condition) failures.Add(message); }
        Fixture[] foreHindFixtures = [
            new("myotismon:2", ["J_foot_f_l","J_foot_f_r","J_foot_b_l","J_foot_b_r"], ["J_leg_f_l","J_leg_f_r","J_leg_b_l","J_leg_b_r"]),
            new("drg:GranDracmon", ["J_foot_f_l","J_foot_f_r","J_foot_b_l","J_foot_b_r"], ["J_leg_f_l","J_leg_f_r","J_leg_b_l","J_leg_b_r"]),
            new("kudamon:1", ["foot_f_l","foot_f_r","foot_b_l","foot_b_r"], ["leg_f_l","leg_f_r","leg1_b_l","leg1_b_r"]),
            new("catalog:CS_Reppamon:chr077", ["foot_f_l","foot_f_r","foot_b_l","foot_b_r"], ["leg_f_l","leg_f_r","leg1_b_l","leg1_b_r"]),
            new("woodmon:0", ["J_unkleFL","J_unkleFR","J_unkleRL","J_unkleRR"], ["J_legFL","J_legFR","J_legRL","J_legRR"]),
            new("catalog:CS_Woodmon:chr393", ["J_unkleFL","J_unkleFR","J_unkleRL","J_unkleRR"], ["J_legFL","J_legFR","J_legRL","J_legRR"]),
            new("extra:Okuwamon", ["J_unkleFL","J_unkleFR","J_unkleRL","J_unkleRR"], ["J_legFL","J_legFR","J_legRL","J_legRR"]),
            new("okuwamon:1", ["J_unkleFL","J_unkleFR","J_unkleRL","J_unkleRR"], ["J_legFL","J_legFR","J_legRL","J_legRR"])
        ];
        var fixtures = importedDirectCandidates.Where(key => key != "drg:Varodurumon")
            .Select(key => new Fixture(key, ["J_foot_l","J_foot_r"], RequireDirect: true))
            .Concat(new[] {
                new Fixture("drg:Ankylomon", ["J_foot_l","J_foot_r","J_hand_l","J_hand_r"], ["J_leg_l","J_leg_r","J_arm_l","J_arm_r"]),
                new Fixture("armadillomon:1", ["J_foot_l","J_foot_r","J_hand_l","J_hand_r"], ["J_leg_l","J_leg_r","J_arm_l","J_arm_r"])
            }).Concat(foreHindFixtures);
        foreach (var fixture in fixtures)
        {
            string key = fixture.Source;
            var source = catalog.Sources[key];
            using var asset = catalog.Open(payload, source.Asset);
            var rig = GltfRig.Load(asset).Prepare(source, payload);
            var mesh = GltfMesh.Load(asset);
            var moving = new CombatModelAnimator(rig, key);
            var idle = new ModelAnimator(rig);
            var helper = new CompanionLocomotion(rig, key, idle.Pose, authored: false);
            string[] footNames = fixture.Feet;
            int[] feet = footNames.Select(name => Array.FindIndex(rig.Nodes, node => node.Name == name)).ToArray();
            // Short imported limbs use their actual immediate foot parents (including Monzaemon J_leg1).
            // Explicit quadruped hips preserve intermediate joints such as Reppamon leg2_b_l/r.
            int[] hips = fixture.Hips is { } hipNames
                ? hipNames.Select(name => Array.FindIndex(rig.Nodes, node => node.Name == name)).ToArray()
                : feet.Select(foot => foot >= 0 ? rig.Nodes[foot].Parent : -1).ToArray();
            Check(hips.All(i => i >= 0) && feet.All(i => i >= 0), key + " independently identified actual limbs exist");
            if (hips.Any(i => i < 0) || feet.Any(i => i < 0)) continue;
            Check(moving.LocomotionKind == CompanionLocomotionKind.Grounded && helper.LegCount == feet.Length,
                key + $" includes all {feet.Length} weight-bearing limbs (actual {helper.Kind}/{helper.LegCount})");
            Check(feet.Select((foot, i) => Below(foot, hips[i], rig)).All(x => x), key + " actual feet descend from independently identified hips");
            if (fixture.RequireDirect) Check(feet.Select((foot, i) => rig.Nodes[foot].Parent == hips[i]).All(x => x),
                key + " uses immediate hip-to-foot joints without inventing knees");
            int rootNode = hips[0]; while (rig.Nodes[rootNode].Parent >= 0) rootNode = rig.Nodes[rootNode].Parent;
            var vertices = Vertices(mesh, rig);
            var edges = Edges(mesh);
            var initial = vertices.Select(v => Skin(v, idle.Pose)).ToArray();
            double span = Math.Max(initial.Max(p => p.X) - initial.Min(p => p.X), Math.Max(initial.Max(p => p.Y) - initial.Min(p => p.Y), initial.Max(p => p.Z) - initial.Min(p => p.Z)));
            var length = feet.Select((foot, i) => (Point(idle.Pose.World[foot]) - Point(idle.Pose.World[hips[i]])).Length).ToArray();
            var weights = hips.Select(hip => vertices.Select(v => v.Influences.Where(influence => Below(influence.Node, hip, rig)).Sum(influence => influence.Weight)).ToArray()).ToArray();
            var totalWeight = weights.Select(w => w.Sum()).ToArray();
            Check(totalWeight.All(w => w > 0), key + " all actual limb subtrees influence visible skin vertices");
            var maxFoot = new double[feet.Length]; var maxSkin = new double[feet.Length];
            bool finite = true, translations = true, scales = true;
            double maxEdgeRatio = 1, peakEdgeTime = 0, worstEdgeBaseline = 0, worstEdgeMoved = 0, maxAbsoluteEdgeGrowth = 0, maxOverstretchedEdgeGrowth = 0;
            int[] peakEdge = [];
            for (int frame = 0; frame < 48; frame++)
            {
                moving.Update(1d / 60, true); idle.Update(1d / 60);
                var rootDelta = Point(moving.Pose.World[rootNode]) - Point(idle.Pose.World[rootNode]);
                for (int i = 0; i < rig.Nodes.Length; i++)
                {
                    translations &= i == rootNode || moving.Pose.Local[i].Translation == idle.Pose.Local[i].Translation;
                    scales &= moving.Pose.Local[i].Scale == idle.Pose.Local[i].Scale;
                }
                var moved = vertices.Select(v => Skin(v, moving.Pose)).ToArray();
                var still = vertices.Select(v => Skin(v, idle.Pose)).ToArray();
                finite &= moved.All(p => p.IsFinite) && moving.Pose.World.All(m => m.IsFinite);
                var squareDisplacement = moved.Select((p, i) => (p - still[i] - rootDelta).LengthSquared).ToArray();
                for (int limb = 0; limb < feet.Length; limb++)
                {
                    maxFoot[limb] = Math.Max(maxFoot[limb], (Point(moving.Pose.World[feet[limb]]) - Point(idle.Pose.World[feet[limb]]) - rootDelta).Length);
                    double sum = 0; for (int vertex = 0; vertex < vertices.Length; vertex++) sum += squareDisplacement[vertex] * weights[limb][vertex];
                    if (totalWeight[limb] > 0) maxSkin[limb] = Math.Max(maxSkin[limb], Math.Sqrt(sum / totalWeight[limb]));
                }
                foreach (var (a, b) in edges)
                {
                    double baseline = (still[a] - still[b]).Length;
                    if (baseline < span * .001) continue; // Ignore numerical/duplicate-vertex micro-edges, not ordinary mesh edges.
                    double changed = (moved[a] - moved[b]).Length, ratio = changed / baseline;
                    maxAbsoluteEdgeGrowth = Math.Max(maxAbsoluteEdgeGrowth, changed - baseline);
                    if (ratio > 2) maxOverstretchedEdgeGrowth = Math.Max(maxOverstretchedEdgeGrowth, changed - baseline);
                    if (ratio > maxEdgeRatio) { maxEdgeRatio = ratio; peakEdgeTime = (frame + 1) / 60d; peakEdge = [a, b]; worstEdgeBaseline = baseline; worstEdgeMoved = changed; }
                }
            }
            for (int limb = 0; limb < feet.Length; limb++)
            {
                double minimum = length[limb] * .02;
                Check(maxFoot[limb] > minimum, key + " " + footNames[limb] + $" moves relative to the root ({maxFoot[limb]:G6} > {minimum:G6})");
                Check(maxSkin[limb] > minimum, key + " " + footNames[limb] + $" deforms effective limb skin ({maxSkin[limb]:G6} > {minimum:G6})");
            }
            Check(finite, key + " skinned vertices and world matrices remain finite");
            Check(translations && scales, key + " gait preserves limb translations and scales");
            // Existing supported lopmon:0/terriermon:0 reach 3.21x/4.52x ratios on blended seams,
            // but only 13.60%/13.51% of actual limb length in absolute edge growth. A 15% limb
            // envelope preserves those controls without letting long ears/tails hide larger tears.
            // This is a regression envelope, not visual approval; raw ratios remain in evidence.
            Check(maxAbsoluteEdgeGrowth <= length.Min() * .15, key + $" ordinary edge growth stays within 15% of the shortest real limb (actual {maxAbsoluteEdgeGrowth / length.Min():P3})");
            for (int frame = 0; frame < 30; frame++) { moving.Update(1d / 60, false); idle.Update(1d / 60); }
            Check(moving.Pose.Local.SequenceEqual(idle.Pose.Local), key + " arrival restores the exact current idle pose");
            for (int frame = 0; frame < 17; frame++) { moving.Update(1d / 60, true); idle.Update(1d / 60); }
            Check(moving.Play("attack") && idle.Play("attack"), key + " accepts an attack during travel");
            bool actionPriority = true;
            for (int frame = 0; frame < 24; frame++) { moving.Update(1d / 60, true); idle.Update(1d / 60); actionPriority &= moving.Pose.Local.SequenceEqual(idle.Pose.Local); }
            Check(actionPriority, key + " action pose takes priority over gait");
            evidence.Add(new { source = key, assetSha256 = catalog.Assets[source.Asset].Sha256, motionSha256 = source.Motion!.Sha256,
                kind = helper.Kind.ToString(), legs = helper.LegCount, bodySpan = span, footNames, limbLengths = length, effectiveLimbWeight = totalWeight,
                maxRootRelativeFootTravel = maxFoot, maxRootRelativeSkinRms = maxSkin, maxOrdinaryConnectedEdgeRatio = maxEdgeRatio, peakEdgeTime, peakEdge,
                worstEdgeBaseline, worstEdgeMoved, worstEdgeGrowthOverSpan = (worstEdgeMoved - worstEdgeBaseline) / span,
                maxAbsoluteEdgeGrowth, maxAbsoluteEdgeGrowthOverSpan = maxAbsoluteEdgeGrowth / span,
                maxAbsoluteEdgeGrowthOverShortestLimb = maxAbsoluteEdgeGrowth / length.Min(), edgeGrowthLimitOverShortestLimb = .15,
                maxOverstretchedEdgeGrowth, maxOverstretchedEdgeGrowthOverSpan = maxOverstretchedEdgeGrowth / span,
                worstEdgeWeights = peakEdge.Select(index => new { vertex = index, influences = vertices[index].Influences.Select(i => new { bone = rig.Nodes[i.Node].Name, i.Weight }).ToArray() }).ToArray() });
        }
        {
            const string key = "drg:Varodurumon";
            var source = catalog.Sources[key]; using var asset = catalog.Open(payload, source.Asset);
            var rig = GltfRig.Load(asset).Prepare(source, payload);
            var moving = new CombatModelAnimator(rig, key); var idle = new ModelAnimator(rig);
            bool samePose = true; double peak = 0;
            for (int frame = 0; frame < 48; frame++) { moving.Update(1d / 60, true); idle.Update(1d / 60); samePose &= moving.Pose.Local.SequenceEqual(idle.Pose.Local); peak = Math.Max(peak, Math.Abs(moving.GlideOffset)); }
            Check(moving.LocomotionKind == CompanionLocomotionKind.Glide && !moving.UsesTravelHop, key + " reviewed airborne presentation keeps glide priority over short-leg discovery");
            Check(samePose && peak is > .005 and <= .025, key + " flight preserves exact idle limb pose with bounded glide");
            evidence.Add(new { source = key, assetSha256 = catalog.Assets[source.Asset].Sha256, motionSha256 = source.Motion!.Sha256,
                kind = moving.LocomotionKind.ToString(), peakGlideOffset = peak, skinPoseUnchanged = samePose });
        }
        {
            const string key = "drg:Saberdramon";
            var source = catalog.Sources[key]; using var asset = catalog.Open(payload, source.Asset);
            var rig = GltfRig.Load(asset).Prepare(source, payload); var idle = new ModelAnimator(rig);
            var moving = new CombatModelAnimator(rig, key);
            var helper = new CompanionLocomotion(rig, key, idle.Pose, authored: false);
            Check(helper.Kind == CompanionLocomotionKind.Hop && helper.LegCount == 0,
                key + " lettered J_legB_L/R segments are not interpreted as back-leg position tokens");
            bool unchanged = true;
            for (int frame = 0; frame < 24; frame++) { moving.Update(1d / 60, true); idle.Update(1d / 60); unchanged &= moving.Pose.Local.SequenceEqual(idle.Pose.Local); }
            Check(unchanged, key + " retains its current idle skeleton under the bounded name-discovery change");
        }
        // The full authored-run inventory already has its own regression; independently pin priority here too.
        bool authoredExercised = false;
        foreach (var (key, source) in catalog.Sources.Where(pair => pair.Key.StartsWith("legend:", StringComparison.Ordinal)))
        {
            using var asset = catalog.Open(payload, source.Asset); var rig = GltfRig.Load(asset).Prepare(source, payload);
            var run = rig.Animations.FirstOrDefault(clip => clip.Name is "run" or "walk" or "move"); if (run is null) continue;
            var moving = new CombatModelAnimator(rig, key); var raw = new RigPose(rig); double time = 0;
            for (int frame = 0; frame < 24; frame++) { time += 1d / 60; moving.Update(1d / 60, true); }
            raw.Sample(run, time, loop: true);
            Check(moving.LocomotionKind == CompanionLocomotionKind.Authored && moving.Pose.Local.SequenceEqual(raw.Local), key + " authored run keeps exact priority over anatomy fallback");
            authoredExercised = true; break;
        }
        Check(authoredExercised, "an authored-run priority control was exercised");
        Directory.CreateDirectory(Path.Combine(root, "artifacts"));
        File.WriteAllText(Path.Combine(root, "artifacts/gait-anatomy-checks.json"), JsonSerializer.Serialize(new {
            catalogSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(catalogPath))).ToLowerInvariant(),
            runtimeAssetsSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(CompanionLocomotion).Assembly.Location))).ToLowerInvariant(), checks, failures,
            method = "48 native travel frames at 60 Hz versus independent same-time idle; root translation removed from limb and skin displacement. All 14 short-leg source keys, both Ankylomon aliases, and all eight fore/hind-tagged quadruped sources sample all frames and ordinary edges >=0.1% body span. The reviewed airborne candidate keeps exact idle articulation. Edge growth limit 15% real limb length derives from existing lopmon/terriermon controls (13.60%/13.51%), not body span or a blanket ratio. Numeric checks are not visual approval.",
            importedDirectCandidates, foreHindSources = foreHindFixtures.Select(f => f.Source).ToArray(), models = evidence }, new JsonSerializerOptions { WriteIndented = true }));
        if (failures.Count > 0) throw new InvalidOperationException($"Gait anatomy: {failures.Count}/{checks} failed. " + string.Join("; ", failures));
        Console.WriteLine($"Gait anatomy: {checks} skin, foot, edge, arrival and priority checks passed.");
        return checks;
    }
    private static bool Below(int node, int ancestor, GltfRig rig) { for (int i = node; i >= 0; i = rig.Nodes[i].Parent) if (i == ancestor) return true; return false; }
    private static V Point(Matrix4D m) => new(m.M41, m.M42, m.M43);
    private static V Transform(V v, Matrix4D m) => new(v.X*m.M11+v.Y*m.M21+v.Z*m.M31+m.M41, v.X*m.M12+v.Y*m.M22+v.Z*m.M32+m.M42, v.X*m.M13+v.Y*m.M23+v.Z*m.M33+m.M43);
    private static V Skin(Vertex vertex, RigPose pose)
    {
        if (vertex.Influences.Length == 0) return Transform(vertex.Position, pose.World[vertex.MeshNode]);
        V result = new(0,0,0);
        // mesh-local palette * mesh world simplifies to inverseBind * joint world.
        foreach (var influence in vertex.Influences) result += Transform(vertex.Position, influence.InverseBind * pose.World[influence.Node]) * influence.Weight;
        return result;
    }
    private static Vertex[] Vertices(GltfMesh mesh, GltfRig rig)
    {
        var result = new List<Vertex>();
        foreach (var instance in mesh.Instances) foreach (var primitive in mesh.Meshes[instance.Mesh]) for (int vertex = 0; vertex < primitive.VertexCount; vertex++)
        {
            int at = vertex * GltfPrimitive.Stride; var influences = new List<Influence>();
            if (primitive.HasSkin && instance.Skin >= 0) for (int j = 0; j < 4; j++)
            {
                double weight = primitive.Vertices[at + 12 + j]; if (weight <= 0) continue;
                int palette = (int)primitive.Vertices[at + 8 + j]; var skin = rig.Skins[instance.Skin];
                influences.Add(new(skin.Joints[palette], weight, skin.InverseBindMatrices[palette]));
            }
            result.Add(new(new(primitive.Vertices[at], primitive.Vertices[at+1], primitive.Vertices[at+2]), instance.Node, influences.ToArray()));
        }
        return result.ToArray();
    }
    private static (int A, int B)[] Edges(GltfMesh mesh)
    {
        var edges = new HashSet<(int,int)>(); int offset = 0;
        foreach (var instance in mesh.Instances) foreach (var primitive in mesh.Meshes[instance.Mesh])
        {
            for (int i = 0; i < primitive.Indices.Length; i += 3) for (int k = 0; k < 3; k++)
            { int a = offset + (int)primitive.Indices[i+k], b = offset + (int)primitive.Indices[i+(k+1)%3]; edges.Add((Math.Min(a,b), Math.Max(a,b))); }
            offset += primitive.VertexCount;
        }
        return edges.ToArray();
    }
    private sealed record Fixture(string Source, string[] Feet, string[]? Hips = null, bool RequireDirect = false);
    private sealed record Influence(int Node, double Weight, Matrix4D InverseBind);
    private sealed record Vertex(V Position, int MeshNode, Influence[] Influences);
    private readonly record struct V(double X, double Y, double Z)
    {
        public double LengthSquared => X*X+Y*Y+Z*Z;
        public double Length => Math.Sqrt(LengthSquared);
        public bool IsFinite => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z);
        public static V operator +(V a, V b) => new(a.X+b.X,a.Y+b.Y,a.Z+b.Z);
        public static V operator -(V a, V b) => new(a.X-b.X,a.Y-b.Y,a.Z-b.Z);
        public static V operator *(V a, double scale) => new(a.X*scale,a.Y*scale,a.Z*scale);
    }
}

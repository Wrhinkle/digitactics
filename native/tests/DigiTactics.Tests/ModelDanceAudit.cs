using System.Collections.Concurrent;
using System.IO.Compression;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using DigiTactics.Assets;
using DigiTactics.Presentation;

/// <summary>Retargeted dance evidence through the production mixer, bone driver,
/// normalized placement and lineup camera. Finite poses do not certify visual quality.</summary>
internal static class ModelDanceAudit
{
    private sealed record BoundsSample(string Context, double Time, double[] Min, double[] Max,
        double Width, double Height, double Depth, double CenterOffset, double WideFrameExtent, double NarrowFrameExtent);
    private sealed record Phrase(string Dance, int PoseSamples, double MaxBoneTurn, double MaxRootMotion, BoundsSample[] Bounds, string[] Review);
    private sealed record SourceEvidence(string Source, string Asset, int DrivenBones, bool Limbless, int Vertices,
        Phrase[] Dances, string[] Review, string? Error);
    private static readonly string[] Dances = ["floss", "robot", "running", "disco", "macarena", "windmill", "gangnam", "jig", "shuffle", "butterfly", "headbang",
        .. Enumerable.Range(1, 20).Select(i => $"mystery_daily_dance_{i}"), .. Enumerable.Range(1, 20).Select(i => $"mystery_vault_dance_{i}")];

    public static void Run(string root, string? filter = null)
    {
        var models = ModelCatalog.Load(Path.Combine(root, "content/models.json"));
        var companions = CompanionCatalog.Load(Path.Combine(root, "content/companion-models.json"));
        var bindings = DanceCatalog.Load(Path.Combine(root, "content/dance-bindings.json"));
        var contexts = companions.Bases.Values.Concat(companions.Forms.Values).GroupBy(v => v.Source).ToDictionary(g => g.Key, g => g.First());
        var reachable = contexts.Keys.Concat(models.Variants.Values.SelectMany(v => v.Values).SelectMany(v => v).OfType<string>()).ToHashSet();
        var excluded = models.Sources.Keys.Where(key => !reachable.Contains(key)).Order(StringComparer.Ordinal)
            .Select(key => new { source = key, status = "OUTSIDE_DANCE_CONTEXT", reason = "Not referenced by any native unit mapping or companion base/form; remains in the overall model audit." }).ToArray();
        var sources = reachable
            .Where(key => filter is null || key.Contains(filter, StringComparison.OrdinalIgnoreCase)).Order(StringComparer.Ordinal).ToArray();
        var result = new ConcurrentDictionary<string, SourceEvidence>(); int completed = 0;
        Parallel.ForEach(sources, new ParallelOptions { MaxDegreeOfParallelism = Math.Min(4, Environment.ProcessorCount) }, key =>
        {
            var source = models.Sources[key];
            try { result[key] = Inspect(key, source, models, bindings.Binding(key), contexts.GetValueOrDefault(key), companions.Fallbacks, Path.Combine(root, "artifacts/model-content")); }
            catch (Exception error) { result[key] = new(key, source.Asset, 0, false, 0, [], ["sampling-failure"], error.ToString()); }
            int count = Interlocked.Increment(ref completed);
            if (count % 25 == 0 || count == sources.Length) Console.WriteLine($"Dance audit: {count}/{sources.Length} reachable sources sampled.");
        });
        string suffix = filter is null ? "" : "-filtered", output = Path.Combine(root, "artifacts/model-dance-audit" + suffix + ".json.gz");
        var report = new { schemaVersion = 1, generatedUtc = DateTimeOffset.UtcNow,
            scope = "All reachable dance bindings, all 51 phrases; native finite bone/placement checks and exact deformed mesh AABBs. Rendered T-pose and viewport review remains required.",
            sampling = "Quarter-beat skeleton samples over 32 beats (64 for Macarena's complete four-turn cycle) plus .06/.13/.25 second entry and 3.99/4/4.01 second match-overlay expiry; exact mesh bounds at .25/3.99/4/4.01 seconds and beats .5/2/4/8/12/16/24/32, plus48/64 for Macarena. Full unit and available companion contexts. Camera extent uses conservative transformed AABB corners in a settled two-unit lineup; greater than 1 is review evidence, not a clipping verdict.",
            catalogSourceCount = models.Sources.Count, reachableSourceCount = reachable.Count, excluded, sourceCount = sources.Length, danceCount = Dances.Length, errors = result.Values.Count(v => v.Error is not null),
            sources = result.Values.OrderBy(v => v.Source, StringComparer.Ordinal).ToArray() };
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        using (var file = File.Create(output)) using (var zip = new GZipStream(file, CompressionLevel.Optimal)) JsonSerializer.Serialize(zip, report);
        string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(root, path)))) .ToLowerInvariant();
        var code = new[] { "content/models.json", "content/companion-models.json", "content/dance-bindings.json", "src/DigiTactics.Assets/DanceSkeleton.cs", "src/DigiTactics.Assets/DanceChoreography.cs", "src/DigiTactics.Assets/DanceChoreography.Phrases.cs", "src/DigiTactics.Assets/DancePlacement.cs", "src/DigiTactics.Assets/ModelAnimator.cs", "src/DigiTactics.Assets/CompanionAnimator.cs", "src/DigiTactics.Assets/CompanionLocomotion.cs", "src/DigiTactics.Desktop/EliminationStage.cs", "src/DigiTactics.Presentation/EliminationLayout.cs", "tests/DigiTactics.Tests/ModelDanceAudit.cs" };
        var summary = new { report.scope, report.sampling, report.catalogSourceCount, report.reachableSourceCount, report.excluded, report.sourceCount, report.danceCount, report.errors, hashes = code.ToDictionary(p => p, Hash), evidenceSha256 = Hash("artifacts/" + Path.GetFileName(output)),
            sources = report.sources.Select(s => new { source = s.Source, s.Asset, s.DrivenBones, s.Limbless, s.Vertices, status = s.Error is null ? "UNVERIFIED" : "FAIL", s.Review, s.Error,
                dances = s.Dances.Select(d => new { d.Dance, d.PoseSamples, d.MaxBoneTurn, d.MaxRootMotion, d.Review, minHeight = d.Bounds.MinBy(b => b.Height), maxHeight = d.Bounds.MaxBy(b => b.Height), maxOffset = d.Bounds.MaxBy(b => b.CenterOffset), maxFrame = d.Bounds.MaxBy(b => Math.Max(b.WideFrameExtent, b.NarrowFrameExtent)) }) }) };
        File.WriteAllText(Path.Combine(root, "artifacts/model-dance-summary" + suffix + ".json"), JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Dance audit: {report.sourceCount} sources × {Dances.Length} dances; {report.errors} errors. Evidence: {output}. Visual status remains UNVERIFIED.");
        if (report.errors != 0) throw new InvalidDataException("Retargeted dance audit contains sampling errors.");
    }

    private static SourceEvidence Inspect(string key, ModelSource source, ModelCatalog models, DanceBinding binding, CompanionVisual? visual, JsonElement fallbacks, string assets)
    {
        using var asset = models.Open(assets, source.Asset);
        var rig = GltfRig.Load(asset).Prepare(source, assets); var mesh = GltfMesh.Load(asset);
        var baseline = new RigPose(rig); baseline.Sample(ModelAnimator.FindIdle(rig), 0);
        var (lo, hi) = mesh.Bounds(rig, baseline, precise: false);
        double height = Math.Max(hi.Y - lo.Y, .001), footprint = Math.Max(hi.X - lo.X, hi.Z - lo.Z) / height;
        var normalization = Transform(-(lo.X + hi.X) / (2 * height), -lo.Y / height, -(lo.Z + hi.Z) / (2 * height), 0, 1 / height);
        double yaw = source.Metadata.TryGetProperty("rotY", out var rot) ? rot.GetDouble() : 0, scale = Math.Min(1, 1.5 / Math.Max(footprint, .01));
        var basis = new CompanionTransform(new(), new(0, yaw, 0), new(scale, scale, scale));
        var phrases = new List<Phrase>(); var sourceReview = new List<string>();
        if (rig.Skins.Length > 0 && binding.Bones.Length == 0) sourceReview.Add("skinned-source-with-no-driven-bones");
        foreach (string dance in Dances)
        {
            double beat = 60d / DanceChoreography.Bpm(dance);
            var times = new SortedSet<double>(Enumerable.Range(0, dance=="macarena"?257:129).Select(i => i * .25 * beat)) { .06, .13, .25, 3.99, 4, 4.01 };
            var measured = new HashSet<double>(new[] { .5, 2, 4, 8, 12, 16, 24, 32 }.Select(b => b * beat)) { .25, 3.99, 4, 4.01 };
            if(dance=="macarena"){measured.Add(48*beat);measured.Add(64*beat);}
            var animator = new ModelAnimator(rig); var dancer = new DanceSkeleton(binding);
            var companion = visual is null ? null : new CompanionAnimator(rig, visual, fallbacks, Math.Min(visual.Height, 1.5 / Math.Max(footprint, .01)), yaw);
            var companionDancer = new DanceSkeleton(binding);
            var samples = new List<BoundsSample>(); double previous = 0, maxTurn = 0, maxRootMotion = 0;
            foreach (double time in times)
            {
                double delta = time - previous; animator.Update(delta); companion?.Update(delta); previous = time;
                var pose = DanceChoreography.Evaluate(dance, time);
                var root = dancer.Apply(pose, animator.Pose, Math.Min(1, (float)(time / .25)));
                maxRootMotion = Math.Max(maxRootMotion, root.Position.Length() + root.Rotation.Length() + Math.Abs(root.Squash - 1));
                var placement = normalization * DancePlacement.Apply(basis, root, scale);
                Validate(animator.Pose, placement, dance, time);
                foreach (var bone in binding.Bones)
                {
                    var q = animator.Pose.Local[bone.Node].Rotation; var b = baseline.Local[bone.Node].Rotation;
                    double dot = Math.Abs(q.X * b.X + q.Y * b.Y + q.Z * b.Z + q.W * b.W);
                    maxTurn = Math.Max(maxTurn, 2 * Math.Acos(Math.Min(1, dot)));
                }
                if (measured.Contains(time)) samples.Add(Measure("unit", time, animator.Pose, placement));
                if (companion is not null)
                {
                    var companionRoot = companionDancer.Apply(pose, companion.Pose);
                    var companionPlacement = normalization * DancePlacement.Apply(companion.Transform, companionRoot, companion.Scale);
                    Validate(companion.Pose, companionPlacement, dance, time);
                    if (measured.Contains(time)) samples.Add(Measure("companion", time, companion.Pose, companionPlacement));
                }
            }
            var review = new List<string>();
            if (samples.Any(s => s.Height < .15 * scale)) review.Add("height-below-15-percent-normalized-model");
            if (samples.Any(s => s.Height > 2.5 || s.Width > 4 || s.Depth > 4)) review.Add("expanded-body-bounds");
            if (samples.Any(s => s.CenterOffset > 1.5)) review.Add("body-leaves-slot");
            if (samples.Any(s => s.WideFrameExtent > 1.03 || s.NarrowFrameExtent > 1.03)) review.Add("camera-edge-review");
            if (binding.Bones.Length > 0 && maxTurn < .001 && maxRootMotion < .001) review.Add("constant-driven-and-root-pose");
            phrases.Add(new(dance, times.Count, maxTurn, maxRootMotion, samples.ToArray(), review.ToArray()));
        }
        return new(key, source.Asset, binding.Bones.Length, binding.Limbless, mesh.Meshes.Sum(g => g.Sum(p => p.VertexCount)), phrases.ToArray(), sourceReview.ToArray(), null);

        void Validate(RigPose pose, Matrix4D placement, string dance, double time)
        {
            if (!placement.IsFinite || pose.World.Any(m => !m.IsFinite)) throw new InvalidDataException($"Nonfinite dance pose {key}/{dance}/{time}");
        }
        BoundsSample Measure(string context, double time, RigPose pose, Matrix4D placement)
        {
            var bounds = mesh.Bounds(rig, pose);
            var points = (from x in new[] { bounds.Min.X, bounds.Max.X } from y in new[] { bounds.Min.Y, bounds.Max.Y } from z in new[] { bounds.Min.Z, bounds.Max.Z } select Point(new(x, y, z), placement)).ToArray();
            double[] min = [points.Min(p => p.X), points.Min(p => p.Y), points.Min(p => p.Z)], max = [points.Max(p => p.X), points.Max(p => p.Y), points.Max(p => p.Z)];
            if (min.Concat(max).Any(v => !double.IsFinite(v))) throw new InvalidDataException($"Nonfinite dance bounds {key}/{context}/{time}");
            double Frame(double aspect)
            {
                var layout = EliminationLayout.Create(2, aspect); var slot = context == "companion" ? EliminationLayout.Legend : layout.Slots[0];
                var model = Transform(slot.X, slot.Y, slot.Z, 0, slot.Scale) * Transform(0, -.05, 0, Math.Sin(time * .7) * .08, 1);
                var world = points.Select(p => Point(p, model)).ToArray();
                var camera = new EliminationCamera();camera.Lineup(layout, aspect, context == "companion" ? Math.Max(1, world.Max(p => p.Y)) : 1.05, true, 1);
                var corners=world.Select(p=>new Vector3((float)p.X,(float)p.Y,(float)p.Z)).ToArray();
                var fitted=camera.Contain(corners,aspect,1);
                return corners.Select(p => Vector4.Transform(new Vector4(p,1), fitted)).Max(p => p.W <= 0 ? 100 : Math.Max(Math.Abs(p.X / p.W), Math.Abs(p.Y / p.W)));
            }
            return new(context, time, min, max, max[0] - min[0], max[1] - min[1], max[2] - min[2], Math.Sqrt(Math.Pow((min[0] + max[0]) / 2, 2) + Math.Pow((min[2] + max[2]) / 2, 2)), Frame(16d / 9), Frame(.8));
        }
    }
    private static Vector3D Point(Vector3D p, Matrix4D m) => new(p.X*m.M11+p.Y*m.M21+p.Z*m.M31+m.M41, p.X*m.M12+p.Y*m.M22+p.Z*m.M32+m.M42, p.X*m.M13+p.Y*m.M23+p.Z*m.M33+m.M43);
    private static Matrix4D Transform(double x, double y, double z, double yaw, double scale) => Matrix4D.Compose(new(x,y,z), new(0,Math.Sin(yaw/2),0,Math.Cos(yaw/2)),new(scale,scale,scale));
}

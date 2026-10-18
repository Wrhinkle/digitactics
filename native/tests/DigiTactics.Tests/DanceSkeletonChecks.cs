using System.Numerics;
using System.Text.Json;
using DigiTactics.Assets;

internal static class DanceSkeletonChecks
{
    public static int Run(string root)
    {
        using var asset = GltfAsset.LoadCompressed(File.ReadAllBytes(Path.Combine(root, "tests/fixtures/dance-skeleton.glb.gz")));
        var rig = GltfRig.Load(asset); var target = new RigPose(rig);
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/dance-skeleton.json.gz"));
        using var gzip = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        static Vector3 V(JsonElement e) => new(e[0].GetSingle(), e[1].GetSingle(), e[2].GetSingle());
        static Quaternion Q(JsonElement e) => new(e[0].GetSingle(), e[1].GetSingle(), e[2].GetSingle(), e[3].GetSingle());
        int checks = 0; DanceSkeleton? dancer = null;
        foreach (var row in fixture.RootElement.EnumerateArray())
        {
            if (row.TryGetProperty("binding", out var b))
                dancer = new(new(b.GetProperty("bones").EnumerateArray().Select(e => new DanceBone(e.GetProperty("node").GetInt32(), e.GetProperty("kind").GetString()!, e.GetProperty("parentEntry").GetInt32(), V(e.GetProperty("direction")), Q(e.GetProperty("parentRotation")), Q(e.GetProperty("restRotation")), e.GetProperty("side").GetInt32(), e.GetProperty("share").GetSingle(), e.GetProperty("index").GetInt32(), e.GetProperty("limit").GetSingle())).ToArray(), b.GetProperty("limbless").GetBoolean(), b.GetProperty("stubby").GetBoolean(), b.GetProperty("yaw").GetSingle(),b.TryGetProperty("quadruped",out var quad)&&quad.GetBoolean()));
            var choreography = DanceChoreography.Evaluate(row.GetProperty("dance").GetString(), row.GetProperty("time").GetDouble());
            var initial = row.GetProperty("initial");
            for (int i = 0; i < target.Local.Length; i++) { var q = Q(initial[i]); target.Local[i] = target.Local[i] with { Rotation = new(q.X, q.Y, q.Z, q.W) }; }
            var actual = dancer!.Apply(choreography, target, row.GetProperty("weight").GetSingle());
            void Near(double a, double e) { checks++; if (Math.Abs(a - e) > .00003) throw new InvalidOperationException($"Dance skeleton {row.GetProperty("dance")}/{row.GetProperty("shape")}/{row.GetProperty("time")}: {a} vs {e}"); }
            var expected = row.GetProperty("expected");
            for (int i = 0; i < target.Local.Length; i++)
            {
                var q = target.Local[i].Rotation; var e = Q(expected[i]); double sign = q.X * e.X + q.Y * e.Y + q.Z * e.Z + q.W * e.W < 0 ? -1 : 1;
                Near(q.X * sign, e.X); Near(q.Y * sign, e.Y); Near(q.Z * sign, e.Z); Near(q.W * sign, e.W);
            }
            var rt = row.GetProperty("root");
            foreach (var (key, value) in new[] { ("x", actual.Position.X), ("y", actual.Position.Y), ("z", actual.Position.Z), ("rx", actual.Rotation.X), ("ry", actual.Rotation.Y), ("rz", actual.Rotation.Z), ("piv", actual.Pivot), ("sq", actual.Squash) })
                Near(value, rt.TryGetProperty(key, out var e) ? e.GetDouble() : key == "sq" ? 1 : 0);
        }
        Console.WriteLine($"Dance skeleton: {checks:N0} original retargeting, limits, blending and root checks passed."); return checks;
    }
}

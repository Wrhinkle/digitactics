using System.Text.Json;
using DigiTactics.Assets;

internal static class DanceBindingChecks
{
    public static int Run(string root)
    {
        var catalog = DanceCatalog.Load(Path.Combine(root, "content/dance-bindings.json"));
        var models = ModelCatalog.Load(Path.Combine(root, "content/models.json"));
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/dance-bindings.json.gz"));
        using var gzip = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip); int checks = 0;
        foreach (var row in fixture.RootElement.EnumerateArray())
        {
            string key = row.GetProperty("source").GetString()!; var source = models.Sources[key];
            using var asset = models.Open(Path.Combine(root, "artifacts/model-content"), source.Asset);
            var rig = GltfRig.Load(asset).Prepare(source, Path.Combine(root, "artifacts/model-content"));
            var pose = new RigPose(rig); var binding = catalog.Binding(key); var dancer = new DanceSkeleton(binding);
            foreach (var sample in row.GetProperty("samples").EnumerateArray())
            {
                string id = sample.GetProperty("id").GetString()!; double time = sample.GetProperty("time").GetDouble();
                int boneIndex = -1;
                // Single-precision rendering math can cross the original blend's dot=1
                // cutoff on imperfect source quaternions. Bound component error to 2e-4.
                void Near(double a, double e) { checks++; if (Math.Abs(a - e) > .0002) throw new InvalidOperationException($"Dance binding {key}/{id}/{time}/{boneIndex}: {a:R} vs {e:R}"); }
                pose.Sample(ModelAnimator.FindIdle(rig), 0);
                var actual = dancer.Apply(DanceChoreography.Evaluate(id, time), pose, sample.GetProperty("weight").GetSingle());
                var rotations = sample.GetProperty("rotations");
                for (int i = 0; i < binding.Bones.Length; i++)
                {
                    boneIndex = i;
                    var q = pose.Local[binding.Bones[i].Node].Rotation; var e = rotations[i];
                    double[] values = [q.X, q.Y, q.Z, q.W]; double sign = values.Select((v, n) => v * e[n].GetDouble()).Sum() < 0 ? -1 : 1;
                    for (int n = 0; n < 4; n++) Near(values[n] * sign, e[n].GetDouble());
                }
                var expected = sample.GetProperty("root");
                foreach (var (name, value) in new[] { ("x", actual.Position.X), ("y", actual.Position.Y), ("z", actual.Position.Z), ("rx", actual.Rotation.X), ("ry", actual.Rotation.Y), ("rz", actual.Rotation.Z), ("piv", actual.Pivot), ("sq", actual.Squash) })
                    Near(value, expected.TryGetProperty(name, out var e) ? e.GetDouble() : name == "sq" ? 1 : 0);
                var matrix = DancePlacement.Apply(new(new(.1,.2,-.3),new(.17,.37,-.12),new(1.2,1.2,1.2)),actual,1.2);
                var valuesMatrix = new double[16]; matrix.CopyTo(valuesMatrix);
                for (int i = 0; i < 16; i++) Near(valuesMatrix[i], sample.GetProperty("matrix")[i].GetDouble());
            }
        }
        Console.WriteLine($"Dance bindings: {checks:N0} actual-model bone and root checks passed."); return checks;
    }
}

using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class ActionParity
{
    public static int Run(string root)
    {
        var catalog = ModelCatalog.Load(Path.Combine(root, "content/models.json"));
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/actions.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0, sources = 0;
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string key = trace.GetProperty("key").GetString()!;
            var source = catalog.Sources[key];
            using var asset = catalog.Open(Path.Combine(root, "artifacts/model-content"), source.Asset);
            var rig = GltfRig.Load(asset).Prepare(source, Path.Combine(root, "artifacts/model-content"));
            var animator = new ModelAnimator(rig, trace.GetProperty("idleOffset").GetDouble());
            int step = 0; var values = new double[16];
            void Check(bool value, string field) { checks++; if (!value) throw new InvalidOperationException($"Action mismatch: {key}, step {step}, {field}."); }
            void Near(double actual, double expected, string field, double tolerance = 1e-6) => Check(double.IsFinite(actual) && Math.Abs(actual - expected) <= tolerance * (1 + Math.Abs(expected)), $"{field}: {actual} vs {expected}");
            foreach (var expected in trace.GetProperty("steps").EnumerateArray())
            {
                if (expected.GetProperty("request").ValueKind != JsonValueKind.Null)
                    Check(animator.Play(expected.GetProperty("request").GetString()!) == expected.GetProperty("accepted").GetBoolean(), "accepted");
                else if (expected.TryGetProperty("end", out _)) animator.EndAction();
                else animator.Update(expected.GetProperty("delta").GetDouble());
                Check(animator.Role == expected.GetProperty("role").GetString(), "role");
                Near(animator.Time, expected.GetProperty("time").GetDouble(), "mixer time");
                var actions = expected.GetProperty("actions"); Check(animator.Actions.Count == actions.GetArrayLength(), "action count");
                for (int i = 0; i < animator.Actions.Count; i++)
                {
                    var actual = animator.Actions[i]; var action = actions[i];
                    Check(actual.Clip.Name == action.GetProperty("name").GetString(), "clip");
                    Check(actual.Enabled == action.GetProperty("enabled").GetBoolean(), "enabled");
                    Near(actual.Time, action.GetProperty("time").GetDouble(), "action time");
                    Near(actual.Weight, action.GetProperty("weight").GetDouble(), "weight", 1e-12);
                    Near(actual.Speed, action.GetProperty("speed").GetDouble(), "speed");
                }
                if (expected.TryGetProperty("locals", out var locals)) for (int n = 0; n < locals.GetArrayLength(); n++)
                {
                    var p = animator.Pose.Local[n]; double[] local = [p.Translation.X,p.Translation.Y,p.Translation.Z,p.Rotation.X,p.Rotation.Y,p.Rotation.Z,p.Rotation.W,p.Scale.X,p.Scale.Y,p.Scale.Z];
                    for (int c = 0; c < 10; c++) Near(local[c], locals[n][c].GetDouble(), $"local {n}/{c}", 1e-7);
                }
                foreach (var node in expected.GetProperty("world").EnumerateArray())
                {
                    animator.Pose.World[node.GetProperty("node").GetInt32()].CopyTo(values);
                    for (int i = 0; i < 16; i++) Near(values[i], node.GetProperty("matrix")[i].GetDouble(), $"node {node.GetProperty("node").GetInt32()}/{i}", 1e-4);
                }
                step++;
            }
            sources++;
        }
        Console.WriteLine($"Passed {checks} action/mixer reference checks across {sources} model sources.");
        return checks;
    }
}

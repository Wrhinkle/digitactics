using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class CompanionPlaybackChecks
{
    public static int Run(string root)
    {
        var companions = CompanionCatalog.Load(Path.Combine(root, "content/companion-models.json"));
        var models = ModelCatalog.Load(Path.Combine(root, "content/models.json"));
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/companion-playback.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        foreach (var test in fixture.RootElement.EnumerateArray())
        {
            string id = test.GetProperty("id").GetString()!;
            var visual = (test.GetProperty("kind").GetString() == "bases" ? companions.Bases : companions.Forms)[id];
            var source = models.Sources[visual.Source];
            using var asset = models.Open(Path.Combine(root, "artifacts/model-content"), source.Asset);
            var rig = GltfRig.Load(asset).Prepare(source, Path.Combine(root, "artifacts/model-content"));
            var animator = new CompanionAnimator(rig, visual, companions.Fallbacks, 1.2, .37, .3);
            int frame = 0;
            void Check(bool ok, string field) { checks++; if (!ok) throw new InvalidOperationException($"Companion playback {id}/{frame}: {field}"); }
            void Near(double actual, double expected, string field) => Check(Math.Abs(actual - expected) < 1e-4 + Math.Abs(expected) * 2e-5, $"{field} {actual} vs {expected}");
            foreach (var step in test.GetProperty("steps").EnumerateArray())
            {
                frame++;
                if (step.TryGetProperty("play", out var play)) animator.Play(play.GetString()!);
                else if (step.TryGetProperty("moving", out var moving) && !step.TryGetProperty("delta", out _) && !step.TryGetProperty("yaw", out _)) animator.SetMoving(moving.GetBoolean());
                if (step.TryGetProperty("yaw", out var yaw)) animator.Yaw = yaw.GetDouble();
                if (step.TryGetProperty("delta", out var delta)) animator.Update(delta.GetDouble());
                Check(animator.Role == step.GetProperty("role").GetString(), "role"); Check(animator.StepIndex == step.GetProperty("index").GetInt32(), "step index");
                Check(animator.Moving == step.GetProperty("moving").GetBoolean(), "moving"); Near(animator.Time, step.GetProperty("time").GetDouble(), "time"); Near(animator.BodyYaw, step.GetProperty("bodyYaw").GetDouble(), "yaw");
                var transform = animator.Transform;
                double[] values = [transform.Position.X, transform.Position.Y, transform.Position.Z, transform.Rotation.X, transform.Rotation.Y, transform.Rotation.Z, transform.Scale.X, transform.Scale.Y, transform.Scale.Z];
                for (int i = 0; i < values.Length; i++) Near(values[i], step.GetProperty("transform")[i].GetDouble(), "transform");
                Check(animator.Actions.Count == step.GetProperty("actions").GetArrayLength(), "action count");
                foreach (var expected in step.GetProperty("actions").EnumerateArray())
                {
                    var action = animator.Actions.Single(x => x.Clip.Name == expected.GetProperty("name").GetString());
                    Near(action.Time, expected.GetProperty("time").GetDouble(), action.Clip.Name + " time"); Near(action.Weight, expected.GetProperty("weight").GetDouble(), action.Clip.Name + " weight"); Near(action.Speed, expected.GetProperty("speed").GetDouble(), "speed");
                    Check(action.Enabled == expected.GetProperty("enabled").GetBoolean(), "enabled"); Check(action.Paused == expected.GetProperty("paused").GetBoolean(), "paused");
                }
                foreach (var world in step.GetProperty("world").EnumerateArray())
                {
                    var matrix = new double[16]; animator.Pose.World[world.GetProperty("node").GetInt32()].CopyTo(matrix);
                    for (int i = 0; i < 16; i++) Near(matrix[i], world.GetProperty("matrix")[i].GetDouble(), "bone " + world.GetProperty("node") + "/" + i);
                }
            }
        }
        Console.WriteLine($"Companion playback: {checks} original sequence, action, transform and skeletal checks passed.");
        return checks;
    }
}

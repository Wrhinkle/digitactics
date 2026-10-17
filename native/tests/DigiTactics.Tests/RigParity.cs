using System.IO.Compression;
using System.Numerics;
using System.Text.Json;
using DigiTactics.Assets;

internal static class RigParity
{
    public static int Run(string root)
    {
        var catalog = ModelCatalog.Load(Path.Combine(root, "content/models.json"));
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/rigs.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var data = JsonDocument.Parse(gzip);
        int checks = 0, repairedKeys = 0, repairedBinds = 0;
        foreach (var trace in data.RootElement.EnumerateArray())
        {
            string id = trace.GetProperty("asset").GetString()!;
            using var asset = catalog.Open(Path.Combine(root, "artifacts/model-content"), id);
            var rig = GltfRig.Load(asset); var pose = new RigPose(rig);
            repairedKeys += rig.RepairedKeys; repairedBinds += rig.RepairedBindMatrices;
            void Compare(Matrix4D actual, JsonElement expected, string context)
            {
                var values = Values(actual);
                for (int i = 0; i < 16; i++) {
                    checks++; double target = expected[i].GetDouble();
                    if (!double.IsFinite(values[i]) || Math.Abs(values[i] - target) > 1e-4 + Math.Abs(target) * 2e-5)
                        throw new InvalidDataException($"Rig mismatch {id}/{context}/{i}: {values[i]} != {target}");
                }
            }
            foreach (var sample in trace.GetProperty("samples").EnumerateArray()) {
                int animation = sample.GetProperty("animation").GetInt32(); double time = sample.GetProperty("time").GetDouble();
                pose.Sample(animation < 0 ? null : rig.Animations[animation], time);
                foreach (var node in sample.GetProperty("world").EnumerateArray()) {
                    int index = node.GetProperty("node").GetInt32(); Compare(pose.World[index], node.GetProperty("matrix"), $"world/{animation}/{time}/{index}");
                }
                foreach (var skin in sample.GetProperty("skins").EnumerateArray()) {
                    int index = skin.GetProperty("skin").GetInt32(); var matrices = new Matrix4D[rig.Skins[index].Joints.Length];
                    pose.SkinMatrices(index, skin.GetProperty("mesh").GetInt32(), matrices);
                    Compare(matrices[skin.GetProperty("joint").GetInt32()], skin.GetProperty("matrix"), $"skin/{animation}/{time}/{index}");
                }
            }
            if (rig.RepairedKeys > 0 || rig.RepairedBindMatrices > 0) foreach (var animation in rig.Animations) {
                for (int step = 0; step <= 20; step++) {
                    pose.Sample(animation, animation.Duration * step / 20);
                    foreach (var world in pose.World) { checks++; if (!Values(world).All(double.IsFinite)) throw new InvalidDataException($"Nonfinite repaired pose: {id}"); }
                    int node = 0;
                    foreach (var definition in asset.Root.GetProperty("nodes").EnumerateArray()) {
                        if (definition.TryGetProperty("skin", out var binding)) {
                            int skin = binding.GetInt32(); var palette = new Matrix4D[rig.Skins[skin].Joints.Length];
                            pose.SkinMatrices(skin, node, palette);
                            foreach (var matrix in palette) { checks++; if (!matrix.IsFinite) throw new InvalidDataException($"Nonfinite repaired palette: {id}"); }
                        }
                        node++;
                    }
                }
            }
        }
        Console.WriteLine($"Passed {checks} skeletal reference checks; repaired {repairedKeys} keys and {repairedBinds} inverse binds.");
        return checks;
    }
    private static double[] Values(Matrix4D m) => [m.M11,m.M12,m.M13,m.M14,m.M21,m.M22,m.M23,m.M24,m.M31,m.M32,m.M33,m.M34,m.M41,m.M42,m.M43,m.M44];
}

using System.Text.Json;
using DigiTactics.Assets;

internal static class MeshParity
{
    public static int Run(string root)
    {
        var catalog = ModelCatalog.Load(Path.Combine(root, "content/models.json"));
        using var fixture = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root, "tests/fixtures/meshes.json")));
        int checks = 0, vertices = 0;
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string id = trace.GetProperty("asset").GetString()!;
            using var asset = catalog.Open(Path.Combine(root, "artifacts/model-content"), id);
            var mesh = GltfMesh.Load(asset); var rig = GltfRig.Load(asset); var pose = new RigPose(rig);
            void Check(bool condition, string field)
            {
                checks++; if (!condition) throw new InvalidOperationException($"Native mesh differs: {id}/{field}.");
            }
            void Near(double actual, double expected, string field) => Check(Math.Abs(actual - expected) <= 1e-4 + Math.Abs(expected) * 1e-5, $"{field}: {actual} vs {expected}");
            var cursors = new int[mesh.Meshes.Length];
            foreach (var expected in trace.GetProperty("primitives").EnumerateArray())
            {
                int group = expected.GetProperty("mesh").GetInt32();
                var primitive = mesh.Meshes[group][cursors[group]++];
                Check(primitive.VertexCount == expected.GetProperty("count").GetInt32(), "vertex count");
                Check(primitive.Indices.Length == expected.GetProperty("indices").GetInt32(), "index count");
                vertices += primitive.VertexCount;
                foreach (var vertex in expected.GetProperty("vertices").EnumerateArray())
                {
                    int start = vertex.GetProperty("index").GetInt32() * GltfPrimitive.Stride;
                    for (int c = 0; c < 3; c++) Near(primitive.Vertices[start + c], vertex.GetProperty("position")[c].GetDouble(), "position");
                    if (vertex.GetProperty("weights").ValueKind == JsonValueKind.Array)
                        for (int c = 0; c < 4; c++) Near(primitive.Vertices[start + 12 + c], vertex.GetProperty("weights")[c].GetDouble(), "weight");
                }
            }
            string? name = trace.GetProperty("animation").GetString();
            var clip = rig.Animations.FirstOrDefault(a => a.Name == name);
            foreach (var sample in trace.GetProperty("samples").EnumerateArray())
            {
                pose.Sample(clip, sample.GetProperty("time").GetDouble(), loop: true);
                var (min, max) = mesh.Bounds(rig, pose);
                double[] values = [min.X,min.Y,min.Z,max.X,max.Y,max.Z];
                for (int i = 0; i < 6; i++) Near(values[i], sample.GetProperty(i < 3 ? "min" : "max")[i % 3].GetDouble(), "posed bounds");
            }
        }
        Console.WriteLine($"Passed {checks} original-loader mesh checks across {vertices} vertices.");
        return checks;
    }
}

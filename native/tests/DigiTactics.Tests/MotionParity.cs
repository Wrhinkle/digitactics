using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class MotionParity
{
    public static int Run(string root)
    {
        var catalog = ModelCatalog.Load(Path.Combine(root, "content/models.json"));
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/motions.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0, sources = 0;
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string key = trace.GetProperty("key").GetString()!;
            var source = catalog.Sources[key];
            using var asset = catalog.Open(Path.Combine(root, "artifacts/model-content"), source.Asset);
            var rig = GltfRig.Load(asset).Prepare(source, Path.Combine(root, "artifacts/model-content"));
            var pose = new RigPose(rig);
            var actual = new double[16];
            foreach (var sample in trace.GetProperty("samples").EnumerateArray())
            {
                int clip = sample.GetProperty("clip").GetInt32();
                pose.Sample(clip < 0 ? null : rig.Animations[clip], sample.GetProperty("time").GetDouble());
                foreach (var node in sample.GetProperty("world").EnumerateArray())
                {
                    int index = node.GetProperty("node").GetInt32();
                    pose.World[index].CopyTo(actual);
                    for (int i = 0; i < 16; i++)
                    {
                        checks++; double expected = node.GetProperty("matrix")[i].GetDouble();
                        if (!double.IsFinite(actual[i]) || Math.Abs(actual[i] - expected) > 1e-4 + Math.Abs(expected) * 2e-5)
                            throw new InvalidOperationException($"Prepared motion differs: {key}, clip {clip}, node {index}, component {i}: {actual[i]} vs {expected}.");
                    }
                }
            }
            sources++;
        }
        Console.WriteLine($"Passed {checks} prepared-motion reference checks across {sources} model sources.");
        return checks;
    }
}

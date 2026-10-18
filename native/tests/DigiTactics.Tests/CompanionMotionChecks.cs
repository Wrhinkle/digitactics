using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class CompanionMotionChecks
{
    public static int Run(string root)
    {
        var catalog = CompanionCatalog.Load(Path.Combine(root, "content/companion-models.json"));
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/companion-motion.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        foreach (var test in fixture.RootElement.GetProperty("plans").EnumerateArray())
        {
            string id = test.GetProperty("id").GetString()!, mode = test.GetProperty("mode").GetString()!;
            var visual = (test.GetProperty("kind").GetString() == "bases" ? catalog.Bases : catalog.Forms)[id];
            var actual = CompanionMotion.Plan(visual, catalog.Fallbacks, test.GetProperty("clips").EnumerateArray().Select(x => x.GetString()!), test.GetProperty("role").GetString()!, mode == "moving", mode != "noMixer");
            var expected = JsonSerializer.Deserialize<CompanionStep[]>(test.GetProperty("expected"), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            checks++;
            if (!actual.SequenceEqual(expected)) throw new InvalidOperationException($"Companion plan differs: {id}/{test.GetProperty("role")}/{mode}");
        }
        foreach (var test in fixture.RootElement.GetProperty("effects").EnumerateArray())
        {
            double N(string key) => test.GetProperty(key).GetDouble();
            var transform = CompanionMotion.Evaluate(test.GetProperty("effect").GetString(), N("k"), N("loops"), N("duration"), N("scale"), N("yaw"));
            void Compare(IEnumerable<double> actual, string field)
            {
                var expected = test.GetProperty(field).EnumerateArray().Select(x => x.GetDouble()).ToArray();
                int i = 0;
                foreach (double value in actual) { checks++; if (Math.Abs(value - expected[i++]) > 1e-12) throw new InvalidOperationException($"Companion transform differs: {test.GetProperty("effect")}/{N("k")}/{field}"); }
            }
            Compare([transform.Position.X, transform.Position.Y, transform.Position.Z], "position");
            Compare([transform.Rotation.X, transform.Rotation.Y, transform.Rotation.Z], "rotation");
            Compare([transform.Scale.X, transform.Scale.Y, transform.Scale.Z], "size");
            var matrix = new double[16]; transform.Matrix.CopyTo(matrix); Compare(matrix, "matrix");
        }
        Console.WriteLine($"Companion motion: {checks} original sequence and procedural transform checks passed.");
        return checks;
    }
}

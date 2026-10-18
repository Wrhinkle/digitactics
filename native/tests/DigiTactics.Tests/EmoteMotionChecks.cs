using System.Text.Json;
using DigiTactics.Assets;

internal static class EmoteMotionChecks
{
    public static int Run(string root)
    {
        int checks = 0;
        void Check(bool ok, string why) { checks++; if (!ok) throw new InvalidOperationException("Emote motion: " + why); }
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/emote-motion.json.gz"));
        using var gzip = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        foreach (var row in fixture.RootElement.EnumerateArray())
        {
            var actual = CompanionEmoteMotion.Evaluate(row.GetProperty("move").GetString()!, row.GetProperty("elapsed").GetDouble(), row.GetProperty("duration").GetDouble());
            var expected = row.GetProperty("pose");
            Check((actual is null) == (expected.ValueKind == JsonValueKind.Null), "active time window");
            if (actual is null) continue;
            double Value(string name, double fallback = 0) => expected.TryGetProperty(name, out var value) ? value.GetDouble() : fallback;
            foreach (var (a, b) in new[] { (actual.Position.X, Value("x")), (actual.Position.Y, Value("y")), (actual.Position.Z, Value("z")),
                (actual.Rotation.X, Value("rx")), (actual.Rotation.Y, Value("ry")), (actual.Rotation.Z, Value("rz")), (actual.Scale.Y, Value("sy", 1)) })
                Check(Math.Abs(a - b) < 1e-10, "original pose offset");
            Check(actual.Matrix.IsFinite, "finite composed body transform");
        }
        Check(CompanionEmoteMotion.Evaluate("missing", 100, 1600) is null, "unknown move excluded");
        Console.WriteLine($"Emote motion: {checks:N0} original pose, easing, pivot and lifetime checks passed."); return checks;
    }
}

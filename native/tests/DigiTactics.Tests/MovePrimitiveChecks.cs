using System.Text.Json;
using DigiTactics.Presentation;

internal static class MovePrimitiveChecks
{
    public static int Run(string root)
    {
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/move-primitives.json.gz"));
        using var gzip = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0, cases = 0;
        foreach (var row in fixture.RootElement.EnumerateArray())
        {
            string name = row.GetProperty("name").GetString()!;
            string primitive = row.GetProperty("primitive").GetString()!;
            string methodName = char.ToUpperInvariant(primitive[0]) + primitive[1..];
            var method = typeof(MovePrimitives).GetMethod(methodName, [typeof(MoveEffects), typeof(MoveContext), typeof(MoveOptions), typeof(double)])
                ?? throw new InvalidOperationException($"Missing native primitive {primitive}");
            var play = method.CreateDelegate<Action<MoveEffects, MoveContext, MoveOptions, double>>();
            var fx = new MoveEffects(row.GetProperty("seed").GetUInt32());
            List<(double Amplitude, double Duration)> shakes = [];
            fx.Shake = (amplitude, duration) => shakes.Add((amplitude, duration));
            var context = new MoveContext
            {
                A = new(-1.6, 1.05, -.7), P = new(-1.8, 0, -1), C = new(2.1, 0, 1.7),
                T = [new(2.1, .8, 1.7), new(1.2, 1.1, 2.3)], H = 1.65, R = 1.8, S = 1.3, Stage = 1, Yaw = .84,
                Ca = MoveColor.Hex("#e57630"), Cb = MoveColor.Hex("#ffe9a4"),
                Spec = new("fire", "none", .3, "hit", .2, [], [], [])
            };
            var overrides = row.GetProperty("context");
            if (overrides.TryGetProperty("from", out var from)) context.From = new(from.GetProperty("x").GetDouble(), 0, from.GetProperty("z").GetDouble());
            if (overrides.TryGetProperty("T", out var targets)) context.T = targets.EnumerateArray().Select(t => new MoveVector(t.GetProperty("x").GetDouble(), t.GetProperty("y").GetDouble(), t.GetProperty("z").GetDouble())).ToArray();
            if (overrides.TryGetProperty("type", out var type)) context.Type = type.GetString();
            if (overrides.TryGetProperty("stunTime", out var stunTime)) context.StunTime = stunTime.GetDouble();
            var verify = new MoveEffectSnapshotChecks(fx, name, shakes);
            play(fx, context, new MoveOptions(row.GetProperty("options")), .07);
            verify.Snapshot(row.GetProperty("initial"));
            int frameIndex = 0;
            foreach (var frame in row.GetProperty("frames").EnumerateArray())
            {
                verify.At = $"frame {frameIndex++}";
                fx.Update(frame.GetProperty("dt").GetDouble());
                if (frame.TryGetProperty("snapshot", out var expected)) verify.Snapshot(expected);
            }
            verify.Near(fx.Random(), row.GetProperty("nextRandom").GetDouble(), "random draw ordering", 1e-12);
            checks += verify.Checks;
            cases++;
        }
        Console.WriteLine($"Move primitives: {checks:N0} source checks across {cases} cases passed (timelines, particles, pieces, meshes, ribbons and random ordering).");
        return checks;
    }
}

using System.Text.Json;
using DigiTactics.Presentation;

internal static class CompanionTravelChecks
{
    public static int Run(string root)
    {
        int checks = 0;
        void Check(bool ok, string why) { checks++; if (!ok) throw new InvalidOperationException("Companion travel: " + why); }
        var travel = new CompanionTravel();
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/companion-travel.json.gz"));
        using var gzip = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        foreach (var row in fixture.RootElement.EnumerateArray())
        {
            var target = row.GetProperty("target");
            if (target.ValueKind != JsonValueKind.Null) Check(travel.Move(target.GetProperty("x").GetDouble(), target.GetProperty("z").GetDouble()), "destination accepted");
            travel.Update(row.GetProperty("delta").GetDouble()); var expected = row.GetProperty("expected");
            foreach (var (actual, name) in new[] { (travel.X, "x"), (travel.Z, "z"), (travel.Yaw, "yaw"), (travel.Hop(row.GetProperty("hasRun").GetBoolean()), "hop") })
                Check(Math.Abs(actual - expected.GetProperty(name).GetDouble()) < 1e-10, "original " + name);
            Check(travel.Moving == expected.GetProperty("moving").GetBoolean(), "walk/idle state " + row.GetRawText() + $" actual x={travel.X:R} z={travel.Z:R} target={travel.TargetX:R},{travel.TargetZ:R}");
            Check((travel.TargetX is null) == (expected.GetProperty("tx").ValueKind == JsonValueKind.Null), "destination completion");
        }
        double x = travel.X, z = travel.Z;
        Check(!travel.Move(double.NaN, 0) && !travel.Move(0, double.PositiveInfinity) && travel.X == x && travel.Z == z, "invalid destinations cannot corrupt position");
        Console.WriteLine($"Companion travel: {checks:N0} original bounds, movement, turn, arrival and hop checks passed."); return checks;
    }
}

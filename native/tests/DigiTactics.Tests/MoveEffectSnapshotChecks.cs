using System.Text.Json;
using DigiTactics.Presentation;

internal sealed class MoveEffectSnapshotChecks(MoveEffects fx, string name, List<(double Amplitude, double Duration)> shakes)
{
    public int Checks { get; private set; }
    public string At { get; set; } = "initial";
    void Check(bool valid, string path)
    {
        Checks++;
        if (!valid) throw new InvalidOperationException($"Move primitive {name}/{At}: {path}");
    }
    public void Near(double actual, double expected, string path, double tolerance = 1e-8)
        => Check(double.IsFinite(actual) && Math.Abs(actual - expected) <= tolerance * Math.Max(1, Math.Abs(expected)), $"{path}: {actual:R} vs {expected:R}");
    void Vector(MoveVector actual, JsonElement expected, string path)
    {
        Near(actual.X, expected[0].GetDouble(), path + ".x");
        Near(actual.Y, expected[1].GetDouble(), path + ".y");
        Near(actual.Z, expected[2].GetDouble(), path + ".z");
    }
    void Color(MoveColor actual, JsonElement expected, string path)
    {
        Near(actual.R, expected[0].GetDouble(), path + ".r");
        Near(actual.G, expected[1].GetDouble(), path + ".g");
        Near(actual.B, expected[2].GetDouble(), path + ".b");
    }
    void Particles(MoveParticles pool, JsonElement expected, string path)
    {
        bool sampled = expected.ValueKind == JsonValueKind.Object;
        int count = sampled ? expected.GetProperty("count").GetInt32() : expected.GetArrayLength();
        Check(pool.Count == count, $"{path} count: {pool.Count} vs {count}");
        var particles = pool.Items;
        var samples = sampled ? expected.GetProperty("samples") : expected;
        for (int sample = 0; sample < samples.GetArrayLength(); sample++)
        {
            int i = sampled ? samples[sample].GetProperty("index").GetInt32() : sample;
            var p = particles[i];
            var e = sampled ? samples[sample].GetProperty("value") : samples[sample];
            double[] values = [p.X, p.Y, p.Z, p.Vx, p.Vy, p.Vz, p.Life, p.MaxLife, p.Alpha0, p.Size0, p.Size, p.Alpha,
                p.Rotation, p.Spin, p.Gravity, p.Drag, p.Grow];
            for (int j = 0; j < values.Length; j++)
            {
                // Aligned sprite rotation is projected by the renderer; the source
                // computes it in its GPU upload path, outside this command model.
                if (j == 12 && p.Aligned) continue;
                Near(values[j], e[j].GetDouble(), $"{path}[{i}][{j}]", 2e-6);
            }
            Check(p.Cell == e[17].GetString(), $"{path}[{i}] cell");
            Near(p.Color.R, e[18].GetDouble(), $"{path}[{i}] red", 1e-7);
            Near(p.Color.G, e[19].GetDouble(), $"{path}[{i}] green", 1e-7);
            Near(p.Color.B, e[20].GetDouble(), $"{path}[{i}] blue", 1e-7);
            Check(p.Aligned == e[21].GetBoolean(), $"{path}[{i}] alignment");
        }
    }
    public void Snapshot(JsonElement expected)
    {
        Check(fx.TaskCount == expected.GetProperty("tasks").GetInt32(), $"task count: {fx.TaskCount} vs {expected.GetProperty("tasks").GetInt32()}");
        Particles(fx.Add, expected.GetProperty("add"), "add");
        Particles(fx.Alpha, expected.GetProperty("alpha"), "alpha");
        var expectedPieces = expected.GetProperty("pieces");
        Check(fx.Pieces.Count == expectedPieces.GetArrayLength(), $"piece count: {fx.Pieces.Count} vs {expectedPieces.GetArrayLength()}");
        for (int i = 0; i < fx.Pieces.Count; i++)
        {
            var p = fx.Pieces[i];
            var e = expectedPieces[i];
            Check(p.Kind == e.GetProperty("kind").GetString(), $"piece[{i}] kind");
            var values = e.GetProperty("values");
            double[] actual = [p.X, p.Y, p.Z, p.Sx, p.Sy, p.Sz, p.Color.R, p.Color.G, p.Color.B, p.Alpha, p.Rotation];
            for (int j = 0; j < actual.Length; j++) Near(actual[j], values[j].GetDouble(), $"piece[{i}][{j}]");
            Check(p.Face.HasValue == (values[11].ValueKind != JsonValueKind.Null), $"piece[{i}] facing mode");
            if (p.Face is { } facing) Near(facing, values[11].GetDouble(), $"piece[{i}] facing");
            Near(p.Rx, values[12].GetDouble(), $"piece[{i}] rx");
            Near(p.Ry, values[13].GetDouble(), $"piece[{i}] ry");
            Near(p.Rz, values[14].GetDouble(), $"piece[{i}] rz");
        }
        var meshes = fx.Meshes.Where(m => m.Visible).ToArray();
        var expectedMeshes = expected.GetProperty("meshes");
        Check(meshes.Length == expectedMeshes.GetArrayLength(), $"mesh count: {meshes.Length} vs {expectedMeshes.GetArrayLength()}");
        for (int i = 0; i < meshes.Length; i++)
        {
            var mesh = meshes[i];
            var e = expectedMeshes[i];
            string prefix = $"mesh[{i}]";
            Check(mesh.Kind == e.GetProperty("kind").GetString(), prefix + " kind");
            Vector(mesh.Position, e.GetProperty("position"), prefix + ".position");
            Vector(mesh.Scale, e.GetProperty("scale"), prefix + ".scale");
            Vector(mesh.Rotation, e.GetProperty("rotation"), prefix + ".rotation");
            Color(mesh.Color, e.GetProperty("color"), prefix + ".color");
            Color(mesh.Emissive, e.GetProperty("emissive"), prefix + ".emissive");
            Near(mesh.Opacity, e.GetProperty("opacity").GetDouble(), prefix + ".opacity");
            Check(mesh.RotationOrder == e.GetProperty("order").GetString(), prefix + " rotation order");
            Check(mesh.Billboard == e.GetProperty("billboard").GetBoolean(), prefix + " billboard");
            var look = e.GetProperty("lookTarget");
            Check((mesh.LookTarget is null) == (look.ValueKind == JsonValueKind.Null), prefix + " look target");
            if (mesh.LookTarget is { } target) Vector(target, look, prefix + ".lookTarget");
        }
        var ribbons = fx.Ribbons.Where(r => r.Visible).ToArray();
        var expectedRibbons = expected.GetProperty("ribbons");
        Check(ribbons.Length == expectedRibbons.GetArrayLength(), $"ribbon count: {ribbons.Length} vs {expectedRibbons.GetArrayLength()}");
        for (int i = 0; i < ribbons.Length; i++)
        {
            var ribbon = ribbons[i];
            var e = expectedRibbons[i];
            var points = e.GetProperty("points");
            string prefix = $"ribbon[{i}]";
            Check(ribbon.Count == points.GetArrayLength(), prefix + " point count");
            for (int j = 0; j < ribbon.Count; j++) Vector(ribbon.Points[j], points[j], prefix + $".point[{j}]");
            Color(ribbon.Color, e.GetProperty("color"), prefix + ".color");
            Near(ribbon.Width, e.GetProperty("width").GetDouble(), prefix + ".width");
            Near(ribbon.Taper, e.GetProperty("taper").GetDouble(), prefix + ".taper");
            Near(ribbon.Brightness, e.GetProperty("brightness").GetDouble(), prefix + ".brightness");
        }
        var expectedShakes = expected.GetProperty("shakes");
        Check(shakes.Count == expectedShakes.GetArrayLength(), "shake count");
        for (int i = 0; i < shakes.Count; i++)
        {
            Near(shakes[i].Amplitude, expectedShakes[i][0].GetDouble(), $"shake[{i}] amplitude");
            Near(shakes[i].Duration, expectedShakes[i][1].GetDouble(), $"shake[{i}] duration");
        }
    }
}

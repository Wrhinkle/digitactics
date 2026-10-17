using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Presentation;
using DigiTactics.Rules;

internal static class CombatSceneParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/scenes.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var data = JsonDocument.Parse(gzip);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        int checks = 0;
        foreach (var trace in data.RootElement.EnumerateArray())
        {
            string variant = trace.GetProperty("variant").GetString()!;
            var rules = catalog.Variants.Single(v => v.Id == variant);
            var combat = new CombatSimulation(rules, trace.GetProperty("sideA").Deserialize<CombatEntry[]>(options)!,
                trace.GetProperty("sideB").Deserialize<CombatEntry[]>(options)!, trace.GetProperty("seed").GetUInt32(),
                trace.GetProperty("cols").GetInt32(), setup: new(new(), new()));
            var scene = new CombatScene(combat);
            int tick = 0;
            foreach (var sample in trace.GetProperty("samples").EnumerateArray())
            {
                int until = sample.GetProperty("ticks").GetInt32();
                while (tick < until) { combat.Step(); tick++; }
                scene.Update(sample.GetProperty("mirror").GetBoolean(), sample.GetProperty("hover").GetInt32());
                var expected = sample.GetProperty("views").Deserialize<CombatView[]>(options)!;
                checks++;
                if (scene.Views.Count != expected.Length) throw new InvalidOperationException($"Scene visibility mismatch {variant}/{tick}");
                for (int i = 0; i < expected.Length; i++)
                {
                    var actual = scene.Views[i]; var e = expected[i];
                    void Near(double a, double b) {
                        checks++;
                        if (Math.Abs(a - b) > 1e-10) throw new InvalidOperationException($"Scene transform mismatch {variant}/{tick}/{i}: {actual} != {e}");
                    }
                    Near(actual.Position.X, e.Position.X); Near(actual.Position.Y, e.Position.Y); Near(actual.Position.Z, e.Position.Z);
                    Near(actual.Yaw, e.Yaw); Near(actual.Dying, e.Dying);
                    checks++;
                    if (actual.UnitId != e.UnitId || actual.Team != e.Team || actual.Hover != e.Hover)
                        throw new InvalidOperationException($"Scene identity mismatch {variant}/{tick}/{i}");
                }
            }
        }
        Console.WriteLine($"Passed {checks} original-renderer scene checks.");
        return checks;
    }
}

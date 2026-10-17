using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Presentation;
using DigiTactics.Rules;

internal static class ChoreographyParity
{
    private sealed class Driver : ICombatModelDriver
    {
        public List<object[]> Calls { get; } = [];
        public void Create(CombatUnit unit) => Calls.Add(["create", unit.Id]);
        public void Remove(int unit) => Calls.Add(["remove", unit]);
        public bool Play(int unit, string role) { Calls.Add(["play", unit, role]); return role != "attack" || unit % 3 != 0; }
        public void Update(int unit, double delta) => Calls.Add(["update", unit, delta]);
    }
    public static int Run(string root, ContentCatalog catalog)
    {
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/choreography.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var data = JsonDocument.Parse(gzip);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        int checks = 0;
        foreach (var trace in data.RootElement.EnumerateArray())
        {
            string variant = trace.GetProperty("variant").GetString()!;
            var combat = new CombatSimulation(catalog.Variants.Single(v => v.Id == variant), trace.GetProperty("sideA").Deserialize<CombatEntry[]>(options)!,
                trace.GetProperty("sideB").Deserialize<CombatEntry[]>(options)!, trace.GetProperty("seed").GetUInt32(), trace.GetProperty("cols").GetInt32(), setup: new(new(), new()));
            var driver = new Driver(); var presentation = new CombatChoreography(combat, driver); int tick = 0;
            foreach (var sample in trace.GetProperty("samples").EnumerateArray())
            {
                while (tick < sample.GetProperty("ticks").GetInt32()) { combat.Step(); tick++; }
                driver.Calls.Clear(); presentation.Update(sample.GetProperty("delta").GetDouble(), trace.GetProperty("mirror").GetBoolean(), sample.GetProperty("hover").GetInt32());
                void Check(bool value, string field) { checks++; if (!value) throw new InvalidOperationException($"Combat choreography mismatch {variant}/{tick}: {field}"); }
                void Near(double actual, double expected, string field) => Check(Math.Abs(actual - expected) < 1e-9, $"{field}: {actual} vs {expected}");
                Check(JsonSerializer.Serialize(driver.Calls) == sample.GetProperty("calls").GetRawText(), $"model calls: {JsonSerializer.Serialize(driver.Calls)} != {sample.GetProperty("calls").GetRawText()}");
                var frames = sample.GetProperty("frames"); Check(frames.GetArrayLength() == presentation.Views.Count, "visible count");
                for (int i = 0; i < presentation.Views.Count; i++)
                {
                    var actual = presentation.Views[i]; var expected = frames[i];
                    Check(actual.UnitId == expected.GetProperty("unitId").GetInt32(), "unit id");
                    double[] position = [actual.Position.X,actual.Position.Y,actual.Position.Z], offset = [actual.BodyOffset.X,actual.BodyOffset.Y,actual.BodyOffset.Z];
                    for (int c = 0; c < 3; c++) { Near(position[c], expected.GetProperty("position")[c].GetDouble(), "position"); Near(offset[c], expected.GetProperty("offset")[c].GetDouble(), "lunge"); }
                    Near(actual.Yaw, expected.GetProperty("yaw").GetDouble(), "yaw"); Near(actual.Scale, expected.GetProperty("scale").GetDouble(), "scale");
                    Near(actual.Flash > 0 ? actual.Flash : actual.Glow, expected.GetProperty("emission").GetDouble(), "emission"); Near(actual.RingScale, expected.GetProperty("ring").GetDouble(), "ring");
                    Check(actual.CastName == expected.GetProperty("castName").GetString(), "cast name"); Check(actual.CastVisible == expected.GetProperty("castVisible").GetBoolean(), "cast label");
                }
            }
        }
        Console.WriteLine($"Passed {checks} combat choreography reference checks.");
        return checks;
    }
}

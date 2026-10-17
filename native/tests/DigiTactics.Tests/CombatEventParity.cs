using System.Text.Json;
using System.Text.Json.Serialization;
using DigiTactics.Rules;

internal static class CombatEventParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/events.json.gz"));
        using var gzip = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Decompress);
        using var data = JsonDocument.Parse(gzip);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        options.Converters.Add(new JsonStringEnumConverter());
        int checks = 0;
        foreach (var trace in data.RootElement.EnumerateArray())
        {
            string name = trace.GetProperty("name").GetString()!;
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var combat = new CombatSimulation(rules, trace.GetProperty("sideA").Deserialize<CombatEntry[]>(options)!,
                trace.GetProperty("sideB").Deserialize<CombatEntry[]>(options)!, trace.GetProperty("seed").GetUInt32(), trace.GetProperty("cols").GetInt32(),
                setup: trace.TryGetProperty("setup", out var setup) ? setup.Deserialize<CombatSetup>(options) : null);
            for (int i = 0; i < trace.GetProperty("ticks").GetInt32(); i++) combat.Step();
            var expected = trace.GetProperty("events").Deserialize<CombatEvent[]>(options)!;
            checks++;
            for (int i = 0; i < Math.Min(expected.Length, combat.Events.Count); i++)
            {
                var actual = combat.Events[i];
                // Native damage cues additionally retain source identity for consumers.
                if (actual.Type == "damage") actual = actual with { Target = null };
                checks++;
                if (Math.Abs(actual.Time - expected[i].Time) > 1e-10 || actual with { Time = expected[i].Time } != expected[i])
                    throw new InvalidOperationException($"Event mismatch {name}/{i}: {actual} != {expected[i]}");
            }
            if (combat.Events.Count != expected.Length) throw new InvalidOperationException($"Event count mismatch {name}: {combat.Events.Count} != {expected.Length}");
        }
        Console.WriteLine($"Passed {checks} ordered presentation event checks.");
        return checks;
    }
}

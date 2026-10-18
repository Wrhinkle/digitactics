using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Storage;

internal static class FarmSimulationChecks
{
    public static int Run(string root)
    {
        int checks = 0;
        void Compare(JsonNode? actual, JsonNode? expected, string path)
        {
            checks++;
            if (actual is JsonObject a && expected is JsonObject b)
            {
                if (!a.Select(x => x.Key).Order().SequenceEqual(b.Select(x => x.Key).Order())) throw new InvalidOperationException("Farm fields: " + path);
                foreach (var field in b) Compare(a[field.Key], field.Value, path + "/" + field.Key);
            }
            else if (actual is JsonArray aa && expected is JsonArray bb)
            {
                if (aa.Count != bb.Count) throw new InvalidOperationException("Farm array: " + path + " " + aa.Count + " != " + bb.Count);
                for (int i = 0; i < aa.Count; i++) Compare(aa[i], bb[i], path + "/" + i);
            }
            else if (actual is JsonValue av && expected is JsonValue bv && av.TryGetValue<double>(out double an) && bv.TryGetValue<double>(out double bn))
            { if (Math.Abs(an - bn) > 1e-9) throw new InvalidOperationException($"Farm number {path}: {an:R} != {bn:R}"); }
            else if (!JsonNode.DeepEquals(actual, expected)) throw new InvalidOperationException("Farm value: " + path + " " + actual + " != " + expected);
        }
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/farm-simulation.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int index = 0;
        foreach (var test in fixture.RootElement.EnumerateArray())
        {
            var input = test.GetProperty("input");
            var result = FarmSimulation.Advance(input.ValueKind == JsonValueKind.Null ? null : input.GetRawText(), test.GetProperty("wallet").GetRawText(), test.GetProperty("legend").GetRawText(), test.GetProperty("time").GetInt64());
            Compare(JsonNode.Parse(result.Save), JsonNode.Parse(test.GetProperty("expected").GetRawText()), "case " + index);
            Compare(JsonNode.Parse(result.Events.ToJsonString()), JsonNode.Parse(test.GetProperty("events").GetRawText()), "events " + index++);
        }
        Console.WriteLine($"Farm simulation: {checks} original save and event comparisons passed.");
        return checks;
    }
}

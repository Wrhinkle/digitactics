using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Storage;

internal static class EvolutionChecks
{
    public static int Run(string root)
    {
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/evolution.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        int checks = 0;
        foreach (var test in fixture.RootElement.EnumerateArray())
        {
            string? Read(string name) => test.GetProperty(name).ValueKind == JsonValueKind.Null ? null : test.GetProperty(name).GetRawText();
            var view = LegendEvolution.Inspect(Read("legend"), Read("wallet"), Read("farm"), test.GetProperty("id").GetString()!);
            var actual = JsonSerializer.SerializeToNode(view, options);
            if (!JsonNode.DeepEquals(actual, JsonNode.Parse(test.GetProperty("view").GetRawText()))) throw new InvalidOperationException("Evolution mismatch: " + test + " got " + actual);
            checks++;
        }
        using var actionsFile = File.OpenRead(Path.Combine(root, "tests/fixtures/evolution-actions.json.gz"));
        using var actionsGzip = new GZipStream(actionsFile, CompressionMode.Decompress);
        using var actions = JsonDocument.Parse(actionsGzip);
        foreach (var test in actions.RootElement.EnumerateArray())
        {
            string? Read(string name) => test.GetProperty(name).ValueKind == JsonValueKind.Null ? null : test.GetProperty(name).GetRawText();
            var action = Enum.Parse<LegendAction>(test.GetProperty("action").GetString()!, true);
            var result = LegendEvolution.Apply(Read("legend"), Read("wallet"), Read("farm"), test.GetProperty("id").GetString()!, action);
            void Equal(JsonNode? actual, string name)
            {
                checks++;
                if (!JsonNode.DeepEquals(actual, JsonNode.Parse(test.GetProperty(name).GetRawText()))) throw new InvalidOperationException("Evolution action " + name + ": " + test + " got " + actual);
            }
            Equal(result.Result, "result"); Equal(JsonNode.Parse(result.Legends ?? "null"), "legendAfter"); Equal(JsonNode.Parse(result.Wallet!), "walletAfter");
        }
        Console.WriteLine($"Evolution: {checks} original views, farm bonuses, rolls and action/save comparisons passed.");
        return checks;
    }
}

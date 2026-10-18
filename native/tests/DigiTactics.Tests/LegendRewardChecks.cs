using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Storage;

internal static class LegendRewardChecks
{
    public static int Run(string root)
    {
        int checks = 0;
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/legend-rewards.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        using var catalogStream = typeof(LegendRewards).Assembly.GetManifestResourceStream("DigiTactics.Storage.legend-rewards.json")!;
        using var catalog = JsonDocument.Parse(catalogStream);
        if (!catalog.RootElement.GetProperty("legends").EnumerateArray().Select(x => x.GetProperty("id").GetString())
            .SequenceEqual(CosmeticCollection.Items("legend").Select(x => x.Id)))
            throw new InvalidOperationException("Legend progression must cover the complete cosmetic catalog in registry order.");
        checks++;
        foreach (var test in fixture.RootElement.EnumerateArray())
        {
            var previous = test.GetProperty("previous");
            var result = LegendRewards.Calculate(previous.ValueKind == JsonValueKind.Null ? null : previous.GetRawText(), test.GetProperty("wallet").GetRawText(), test.GetProperty("key").GetString()!, test.GetProperty("place").GetInt32());
            if (!JsonNode.DeepEquals(JsonNode.Parse(result.Save!), JsonNode.Parse(test.GetProperty("expected").GetRawText()))) throw new InvalidOperationException("Legend reward save mismatch: " + test);
            checks++;
            if (!JsonNode.DeepEquals(JsonSerializer.SerializeToNode(result.Award, options), JsonNode.Parse(test.GetProperty("award").GetRawText()))) throw new InvalidOperationException("Legend award mismatch: " + test);
            checks++;
        }
        Console.WriteLine($"Legend rewards: {checks} original save and award comparisons passed.");
        return checks;
    }
}

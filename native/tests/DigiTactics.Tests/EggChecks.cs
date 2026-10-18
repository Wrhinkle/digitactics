using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Storage;

internal static class EggChecks
{
    public static int Run(string root)
    {
        int checks = 0;
        void Equal(JsonNode? actual, JsonElement expected, string label)
        {
            checks++;
            if (!JsonNode.DeepEquals(actual, JsonNode.Parse(expected.GetRawText()))) throw new InvalidOperationException("Egg mismatch: " + label + " expected " + expected + " got " + actual);
        }
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/eggs.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        foreach (var test in fixture.RootElement.GetProperty("odds").EnumerateArray())
            Equal(JsonSerializer.SerializeToNode(DigiEggs.Odds(DigiEggs.Definitions.Single(x => x.Id == test.GetProperty("id").GetString())), options), test.GetProperty("odds"), "odds");
        foreach (var test in fixture.RootElement.GetProperty("cases").EnumerateArray())
        {
            string? legend = test.GetProperty("legend").ValueKind == JsonValueKind.Null ? null : test.GetProperty("legend").GetRawText();
            string wallet = test.GetProperty("wallet").GetRawText();
            var result = DigiEggs.Purchase(wallet, legend, test.GetProperty("egg").GetString()!, test.GetProperty("rolls")[0].GetDouble(), test.GetProperty("rolls")[1].GetDouble(), 1234567);
            Equal(JsonSerializer.SerializeToNode(result?.Award, options), test.GetProperty("award"), "award " + test.GetProperty("egg"));
            Equal(JsonNode.Parse(result?.Wallet ?? wallet), test.GetProperty("walletAfter"), "wallet");
            Equal(result?.Legends is { } next ? JsonNode.Parse(next) : legend is not null ? JsonNode.Parse(legend) : null, test.GetProperty("legendAfter"), "legend");
        }
        var duplicate = fixture.RootElement.GetProperty("cases").EnumerateArray().First(x => x.GetProperty("award").ValueKind != JsonValueKind.Null && x.GetProperty("award").GetProperty("evolution").ValueKind != JsonValueKind.Null);
        string directory = Path.Combine(Path.GetTempPath(), "digitactics-eggs-" + Guid.NewGuid().ToString("N"));
        void Check(bool ok, string label) { checks++; if (!ok) throw new InvalidOperationException("Egg transaction: " + label); }
        try
        {
            var profile = new NativeProfileStore(directory);
            Check(profile.UpdateStorage(new Dictionary<string, string> { ["dt.cosmetics.v1"] = duplicate.GetProperty("wallet").GetRawText(), ["dt.future"] = "keep raw" }), "initial wallet saved");
            string path = Path.Combine(directory, "profile.json"), before = File.ReadAllText(path);
            var result = DigiEggs.Purchase(profile.Storage["dt.cosmetics.v1"], null, duplicate.GetProperty("egg").GetString()!, duplicate.GetProperty("rolls")[0].GetDouble(), duplicate.GetProperty("rolls")[1].GetDouble(), 1234567)!;
            var changes = new Dictionary<string, string> { ["dt.cosmetics.v1"] = result.Wallet, ["dt.legendevo.v1"] = result.Legends! };
            using (var guard = new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                Check(!profile.UpdateStorage(changes), "locked purchase rejected");
                Check(File.ReadAllText(path) == before && !profile.Storage.ContainsKey("dt.legendevo.v1"), "no partial debit or legend award");
            }
            Check(profile.UpdateStorage(changes), "same purchase retried");
            var reopened = new NativeProfileStore(directory);
            Check(reopened.Storage["dt.cosmetics.v1"] == result.Wallet && reopened.Storage["dt.legendevo.v1"] == result.Legends, "wallet and legend restart together");
            Check(reopened.Storage["dt.future"] == "keep raw" && File.ReadAllText(path + ".bak") == before, "unknown data and prior profile preserved");
        }
        finally
        {
            string resolved = Path.GetFullPath(directory);
            if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("digitactics-eggs-", StringComparison.Ordinal))
                throw new InvalidOperationException("Refusing cleanup outside generated egg-test directory.");
            if (Directory.Exists(resolved)) Directory.Delete(resolved, true);
        }
        Console.WriteLine($"Digi-Eggs: {checks} original odds, award, save and transaction checks passed.");
        return checks + IncubatorChecks.Run(root);
    }
}

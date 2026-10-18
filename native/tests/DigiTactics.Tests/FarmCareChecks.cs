using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Storage;
using DigiTactics.Presentation;

internal static class FarmCareChecks
{
    public static int Run(string root)
    {
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/farm-care.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        foreach (var test in fixture.RootElement.EnumerateArray())
        {
            var result = FarmCare.Apply(test.GetProperty("farm").GetRawText(), test.GetProperty("wallet").GetRawText(), test.GetProperty("legend").GetRawText(), test.GetProperty("time").GetInt64(),
                Enum.Parse<FarmCareAction>(test.GetProperty("action").GetString()!), test.GetProperty("id").GetString(), test.TryGetProperty("option", out var option) ? option.GetString() : null,
                test.TryGetProperty("pile", out var pile) ? pile.GetInt32() : 0, test.TryGetProperty("lights", out var lights) && lights.ValueKind != JsonValueKind.Null ? lights.GetBoolean() : null);
            void Equal(JsonNode? actual, string field)
            {
                checks++;
                if (!JsonNode.DeepEquals(actual, JsonNode.Parse(test.GetProperty(field).GetRawText())))
                    throw new InvalidOperationException("Farm care " + test.GetProperty("id") + "/" + test.GetProperty("action") + "/" + field + " expected " + test.GetProperty(field) + " actual " + actual);
            }
            Equal(result.Result, "result"); Equal(JsonNode.Parse(result.Farm), "farmAfter"); Equal(JsonNode.Parse(result.Wallet!), "walletAfter"); Equal(JsonNode.Parse(result.Legends!), "legendAfter"); Equal(result.Events, "events");
        }
        void Check(bool ok, string message) { checks++; if (!ok) throw new InvalidOperationException("Farm persistence: " + message); }
        string directory = Path.Combine(Path.GetTempPath(), "digitactics-farm-care-" + Guid.NewGuid().ToString("N"));
        try
        {
            var profile = new NativeProfileStore(directory); var controller = new FarmProgress(profile);
            var sample = fixture.RootElement[0]; long time = sample.GetProperty("time").GetInt64();
            controller.Update(30, time); Check(!profile.Exists, "automatic tick does not prevent first profile import");
            Check(profile.UpdateStorage(new Dictionary<string, string> { ["dt.farm.v1"] = sample.GetProperty("farm").GetRawText(), ["dt.cosmetics.v1"] = sample.GetProperty("wallet").GetRawText(), ["dt.legendevo.v1"] = sample.GetProperty("legend").GetRawText(), ["dt.future"] = "raw value" }), "seed imported state");
            string path = Path.Combine(directory, "profile.json"), before = File.ReadAllText(path);
            using (var guard = new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                Check(!controller.Act(time, FarmCareAction.Feed, "tokomon", "meat") && controller.Error is not null, "blocked care write reports failure");
                Check(File.ReadAllText(path) == before && profile.Storage["dt.farm.v1"] == sample.GetProperty("farm").GetRawText(), "no partial charge, care or reward");
            }
            Check(controller.Act(time, FarmCareAction.Feed, "tokomon", "meat") && controller.Error is null, "retry commits once");
            var reopened = new NativeProfileStore(directory);
            foreach (var (key, expected) in new[] { ("dt.farm.v1", "farmAfter"), ("dt.cosmetics.v1", "walletAfter"), ("dt.legendevo.v1", "legendAfter") })
                Check(JsonNode.DeepEquals(JsonNode.Parse(reopened.Storage[key]), JsonNode.Parse(sample.GetProperty(expected).GetRawText())), "restart " + key);
            Check(reopened.Storage["dt.future"] == "raw value" && File.ReadAllText(path + ".bak") == before, "unrelated save and backup intact");
            Check(reopened.UpdateStorage(new Dictionary<string, string> { ["dt.farm.v1"] = "{\"v\":2,\"future\":true}" }), "future farm seeded");
            before = File.ReadAllText(path); var future = new FarmProgress(reopened);
            Check(!future.Refresh(time + 60000) && future.Error is not null && File.ReadAllText(path) == before, "future farm preserved");
        }
        finally
        {
            string resolved = Path.GetFullPath(directory);
            if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("digitactics-farm-care-", StringComparison.Ordinal)) throw new InvalidOperationException("Refusing cleanup outside farm test directory.");
            if (Directory.Exists(resolved)) Directory.Delete(resolved, true);
        }
        Console.WriteLine($"Farm care: {checks} original results, save domains, events and persistence checks passed.");
        return checks;
    }
}

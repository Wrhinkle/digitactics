using System.Text.Json.Nodes;
using DigiTactics.Storage;

internal static class LoadoutChecks
{
    public static int Run(string root)
    {
        int checks = 0;
        void Check(bool ok, string why) { checks++; if (!ok) throw new InvalidOperationException("Loadouts: " + why); }
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/loadouts.json.gz"));
        using var gzip = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Decompress);
        var fixture = JsonNode.Parse(gzip)!;
        foreach (var row in fixture["ai"]!.AsArray())
        {
            var actual = CosmeticLoadouts.Ai(row!["seat"]!.GetValue<int>(), row["name"]!.GetValue<string>());
            actual["legend"] = CosmeticLoadouts.CompanionKey(actual);
            Check(JsonNode.DeepEquals(actual, row["expected"]), "original AI selection " + row.ToJsonString());
        }
        foreach (var row in fixture["local"]!.AsArray())
            Check(JsonNode.DeepEquals(CosmeticLoadouts.Local(row!["wallet"]!.ToJsonString(), row["legends"]!.ToJsonString()), row["expected"]), "original equipped selection " + row.ToJsonString());
        foreach (var row in fixture["keys"]!.AsArray())
            Check(CosmeticLoadouts.CompanionKey(row!.AsObject()) == row["expected"]!.GetValue<string>(), "reachable form validation");
        Console.WriteLine($"Loadouts: {checks:N0} original AI, equipped and appearance checks passed."); return checks;
    }
}

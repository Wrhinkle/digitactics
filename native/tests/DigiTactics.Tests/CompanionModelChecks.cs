using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;
using DigiTactics.Storage;

internal static class CompanionModelChecks
{
    public static int Run(string root)
    {
        var companions = CompanionCatalog.Load(Path.Combine(root, "content/companion-models.json"));
        var models = ModelCatalog.Load(Path.Combine(root, "content/models.json"));
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/companion-models.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        foreach (var item in CosmeticCollection.Items("legend"))
        {
            checks++;
            if (!companions.Bases.ContainsKey(item.Id)) throw new InvalidDataException("Missing companion " + item.Id);
        }
        foreach (var test in fixture.RootElement.EnumerateArray())
        {
            string key = test.GetProperty("key").GetString()!; var visual = companions.Resolve(key);
            checks += 4;
            if (visual.Source != test.GetProperty("source").GetString() || visual.Height != test.GetProperty("height").GetDouble() || visual.Unit != test.GetProperty("unit").GetBoolean()) throw new InvalidDataException("Companion visual selection differs: " + key);
            if (!models.Sources.TryGetValue(visual.Source, out var source) || !models.Assets.ContainsKey(source.Asset) || source.Motion is null) throw new InvalidDataException("Companion payload missing: " + key);
        }
        foreach (var visual in companions.Bases.Values.Where(x => !x.Unit))
        {
            var source = models.Sources[visual.Source];
            using var asset = models.Open(Path.Combine(root, "artifacts/model-content"), source.Asset);
            var original = GltfRig.Load(asset); var prepared = original.Prepare(source, Path.Combine(root, "artifacts/model-content"));
            checks++;
            if (!original.Animations.Select(x => (x.Name, x.Duration)).SequenceEqual(prepared.Animations.Select(x => (x.Name, x.Duration)))) throw new InvalidDataException("Companion authored animations were replaced: " + visual.Source);
        }
        Console.WriteLine($"Companion models: {checks} original selection, coverage and authored-animation checks passed.");
        return checks;
    }
}

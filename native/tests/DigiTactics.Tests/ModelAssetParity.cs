using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class ModelAssetParity
{
    public static int Run(string root)
    {
        var catalog = ModelCatalog.Load(Path.Combine(root, "content/models.json"));
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/models.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var document = JsonDocument.Parse(gzip);
        int checks = 0, files = 0;
        foreach (var probe in document.RootElement.EnumerateArray())
        {
            string id = probe.GetProperty("asset").GetString()!;
            using var model = catalog.Open(Path.Combine(root, "artifacts/model-content"), id);
            var entry = catalog.Assets[id]; files++;
            foreach (var (property, expected) in new[] { ("nodes", entry.Nodes), ("meshes", entry.Meshes), ("skins", entry.Skins), ("accessors", entry.Accessors) })
            {
                int count = model.Root.TryGetProperty(property, out var list) ? list.GetArrayLength() : 0;
                checks++; if (count != expected) throw new InvalidDataException($"Model structure mismatch {id}/{property}");
            }
            foreach (var sample in probe.GetProperty("samples").EnumerateArray())
            {
                var actual = model.AccessorElement(sample[0].GetInt32(), sample[1].GetInt32());
                checks++; if (actual.Length != sample.GetArrayLength() - 2) throw new InvalidDataException("Accessor shape mismatch.");
                for (int i = 0; i < actual.Length; i++)
                {
                    checks++;
                    var expected = sample[i + 2];
                    double value = expected.ValueKind == JsonValueKind.String ? expected.GetString() switch {
                        "NaN" => double.NaN, "Infinity" => double.PositiveInfinity, "-Infinity" => double.NegativeInfinity,
                        _ => throw new InvalidDataException("Invalid reference component.")
                    } : expected.GetDouble();
                    if (!actual[i].Equals(value)) throw new InvalidDataException($"Accessor mismatch {id}/{sample[0]}/{sample[1]}/{i}");
                }
            }
        }
        checks++;
        if (files != catalog.Assets.Count) throw new InvalidDataException("Incomplete model corpus.");
        foreach (var (variant, lines) in catalog.Variants)
            foreach (var (line, forms) in lines)
                for (int stage = 0; stage < forms.Length; stage++)
                {
                    checks++;
                    if (catalog.Resolve(variant, line, stage) is not { } source || !catalog.Assets.ContainsKey(source.Asset))
                        throw new InvalidDataException($"Unresolved model {variant}/{line}/{stage}");
                }
        Console.WriteLine($"Passed {checks} native model checks across {files} assets.");
        return checks;
    }
}

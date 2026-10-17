using System.Security.Cryptography;
using System.Text.Json;
using DigiTactics.Assets;

internal static class TextureParity
{
    public static int Run(string root)
    {
        var catalog = ModelCatalog.Load(Path.Combine(root, "content/models.json"));
        using var fixture = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root, "tests/fixtures/textures.json")));
        int count = 0;
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string id = trace.GetProperty("asset").GetString()!;
            using var model = catalog.Open(Path.Combine(root, "artifacts/model-content"), id);
            int index = 0;
            foreach (var expected in trace.GetProperty("images").EnumerateArray())
            {
                var actual = model.Image(index++);
                if (actual.Width != expected.GetProperty("width").GetInt32() ||
                    actual.Height != expected.GetProperty("height").GetInt32() ||
                    Convert.ToHexString(SHA256.HashData(actual.Rgba)).ToLowerInvariant() != expected.GetProperty("sha256").GetString())
                    throw new InvalidOperationException($"Native texture pixels differ: {id}, image {index - 1}.");
                count++;
            }
        }
        Console.WriteLine($"Passed {count} native texture pixel checks.");
        return count;
    }
}

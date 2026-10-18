using System.Text.Json;
using DigiTactics.Presentation;

internal static class EmoteLayoutChecks
{
    public static int Run(string root)
    {
        int checks = 0;
        void Check(bool ok, string why) { checks++; if (!ok) throw new InvalidOperationException("Emote layout: " + why); }
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/emote-layout.json.gz"));
        using var gzip = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        foreach (var row in fixture.RootElement.EnumerateArray())
        {
            var actual = EmoteLayout.Transform(row.GetProperty("age").GetDouble(), row.GetProperty("row").GetBoolean());
            foreach (var (key, value) in new[] { ("scale", actual.Scale), ("x", actual.X), ("y", actual.Y), ("opacity", actual.Opacity) })
                Check(Math.Abs(value - row.GetProperty(key).GetDouble()) < .002, key + " " + row.GetRawText() + " actual " + value);
        }
        Check(!EmoteLayout.Anchor(0, 0, true, 800, 600).Visible, "behind camera hidden");
        Check(!EmoteLayout.Anchor(-20, 0, false, 800, 600).Visible, "left offscreen boundary");
        Check(EmoteLayout.Anchor(0, 0, false, 800, 600) == new EmoteAnchor(true, 58, 118), "top left clamped");
        Check(EmoteLayout.Anchor(800, 600, false, 800, 600) == new EmoteAnchor(true, 742, 596), "bottom right clamped");
        Check(EmoteLayout.Transform(2600, false).Opacity == 0, "expired hidden");
        Console.WriteLine($"Emote layout: {checks} browser motion and visibility checks passed."); return checks;
    }
}

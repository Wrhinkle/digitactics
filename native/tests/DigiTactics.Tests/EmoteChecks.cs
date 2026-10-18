using System.Text.Json;
using System.Security.Cryptography;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Storage;

internal static class EmoteChecks
{
    public static int Run(string root)
    {
        int checks = 0;
        void Check(bool ok, string why) { checks++; if (!ok) throw new InvalidOperationException("Emotes: " + why); }
        var emotes = new MatchEmotes();
        using (var file = File.OpenRead(Path.Combine(root, "tests/fixtures/emote-rate.json.gz")))
        using (var gzip = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Decompress))
        using (var fixture = JsonDocument.Parse(gzip))
            foreach (var row in fixture.RootElement.EnumerateArray())
                Check(emotes.Receive(row.GetProperty("seat").GetInt32(), "gg", -1, false, row.GetProperty("now").GetInt64()) == row.GetProperty("allowed").GetBoolean(), "original per-player rate decision");
        emotes = new(); var collection = new CosmeticCollection(null);
        Check(!emotes.Send(collection, "gg", -1, 0), "spectator cannot send");
        Check(!emotes.Send(collection, "missing", 0, 0), "unknown item cannot send");
        Check(!emotes.Send(collection, CosmeticCollection.Items("emote").First(i => !i.Free).Id, 0, 0), "unowned item cannot send");
        Check(!emotes.Receive(0, "gg", 0, false, 0) && !emotes.Receive(1, "gg", 0, true, 0), "echo and muted remote excluded");
        Check(emotes.Send(collection, "gg", 0, 0), "owned emote sends");
        Check(emotes.At(0, 2599)?.Id == "gg" && emotes.At(0, 2600) is null, "2600ms lifetime");
        emotes.Send(collection, "gg", 0, 3000); emotes.Receive(1, "gg", 0, false, 3000); emotes.ClearOthers(0);
        Check(emotes.At(0, 3001) is not null && emotes.At(1, 3001) is null, "mute clears current remote sticker while preserving local sticker");
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "content/emotes/manifest.json")));
        var atlases = EmoteAtlasCatalog.Load(Path.Combine(root, "content/emotes/manifest.json"));
        var clips = manifest.RootElement.GetProperty("clips");
        Check(clips.EnumerateObject().Select(p => p.Name).Order().SequenceEqual(CosmeticCollection.Items("emote").Select(i => i.Id).Order()), "every original catalog item has artwork");
        int animated = 0;
        foreach (var clip in clips.EnumerateObject())
        {
            byte[] bytes = File.ReadAllBytes(Path.Combine(root, "content/emotes", clip.Value.GetProperty("file").GetString()!));
            Check(Convert.ToHexStringLower(SHA256.HashData(bytes)) == clip.Value.GetProperty("sha256").GetString(), "atlas integrity " + clip.Name);
            var png = PngImage.Decode(bytes); Check(png.Width == 1280 && png.Height == 1120, "atlas dimensions");
            var hashes = new HashSet<string>();
            for (int frame = 0; frame < 52; frame++)
            {
                var pixels = new byte[160 * 160 * 4];
                for (int y = 0; y < 160; y++) png.Rgba.AsSpan(((frame / 8 * 160 + y) * png.Width + frame % 8 * 160) * 4, 640).CopyTo(pixels.AsSpan(y * 640));
                Check(Enumerable.Range(0, 160 * 160).Any(p => pixels[p * 4 + 3] != 0), "nonempty frame " + clip.Name);
                hashes.Add(Convert.ToHexStringLower(SHA256.HashData(pixels)));
            }
            if (hashes.Count > 1) animated++;
            if (clip.Name is "gg" or "nice" or "cry" or "hop") Check(hashes.Count > 10, "original animated frames vary " + clip.Name);
            var descriptor = atlases.Clips[clip.Name];
            Check(descriptor.Match.FrameAt(2599, false) == 51 && descriptor.Match.FrameAt(2600, false) == 51, "match frame timing remains unchanged " + clip.Name);
            foreach (var layer in descriptor.Preview)
            {
                byte[] preview = File.ReadAllBytes(Path.Combine(root, "content/emotes", layer.File));
                Check(Convert.ToHexStringLower(SHA256.HashData(preview)) == layer.Sha256, "preview atlas integrity " + layer.File);
                var image = PngImage.Decode(preview);
                Check(image.Width == layer.Columns * 160 && image.Height == layer.Rows * 160 && image.Height <= 8192, "preview texture dimensions and conservative GPU limit " + layer.File);
                Check(layer.FrameAt(-1, true) == 0 && layer.FrameAt(layer.LoopMs - 1, true) == layer.Frames - 1 && layer.FrameAt(layer.LoopMs, true) == 0,
                    "preview wraps exactly at its source period " + layer.File);
                Check(layer.FrameAt(layer.LoopMs * 1000L + 50, true) == (layer.Frames == 1 ? 0 : 1), "long-running preview retains source timing " + layer.File);
            }
        }
        Check(atlases.Clips["mystery_daily_emote_19"].Preview.Single().LoopMs == 6000, "Windmill nested 6-second spin and 2-second breath retain full cycle");
        Check(atlases.Clips["mystery_vault_emote_16"].Preview.Single().FrameAt(2600, true) == 52, "Vortex preview continues through old 2.6-second reset");
        Check(atlases.Clips["nice"].Preview.Select(layer => layer.LoopMs).SequenceEqual(new[] { 6000, 1100, 900 }), "NICE keeps source paint order and independent periods without a 198-second atlas");
        Check(atlases.Clips["wave"].Preview.Any(layer => layer.LoopMs == 1000), "alternating wave includes both forward and backward directions");
        Check(animated > 40, "animation retained across catalog");
        Console.WriteLine($"Emotes: {checks} original rate, ownership, lifetime and frame checks passed; {animated} animated sheets."); return checks;
    }
}

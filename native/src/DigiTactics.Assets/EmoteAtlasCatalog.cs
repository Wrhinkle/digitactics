using System.Text.Json;

namespace DigiTactics.Assets;

/// <summary>Source-timed preview layers and the unchanged 2.6-second match sheets.</summary>
public sealed record EmoteAtlas(string File, string Sha256, int Columns, int Rows, int Frames, int FrameMs, int LoopMs)
{
    public int FrameAt(long age, bool loop) => (int)((loop ? Math.Max(0, age) % LoopMs : Math.Min(Math.Max(0, age), (long)(Frames - 1) * FrameMs)) / FrameMs);
}

public sealed record EmoteAtlasClip(EmoteAtlas Match, EmoteAtlas[] Preview);

public sealed class EmoteAtlasCatalog
{
    public const int Tile = 160;
    public IReadOnlyDictionary<string, EmoteAtlasClip> Clips { get; }
    private EmoteAtlasCatalog(Dictionary<string, EmoteAtlasClip> clips) => Clips = clips;

    public static EmoteAtlasCatalog Load(string file)
    {
        using var document = JsonDocument.Parse(System.IO.File.ReadAllText(file));
        var root = document.RootElement;
        if (root.GetProperty("tile"u8).GetInt32() != Tile || root.GetProperty("duration"u8).GetInt32() != 2600)
            throw new InvalidDataException("Unexpected emote match timing or cell size.");
        var clips = new Dictionary<string, EmoteAtlasClip>();
        foreach (var entry in root.GetProperty("clips"u8).EnumerateObject())
        {
            var item = entry.Value;
            var match = new EmoteAtlas(item.GetProperty("file"u8).GetString()!, item.GetProperty("sha256"u8).GetString()!, 8, 7, 52, 50, 2600);
            var preview = item.GetProperty("preview"u8).EnumerateArray().Select(layer => new EmoteAtlas(
                layer.GetProperty("file"u8).GetString()!, layer.GetProperty("sha256"u8).GetString()!,
                layer.GetProperty("columns"u8).GetInt32(), layer.GetProperty("rows"u8).GetInt32(),
                layer.GetProperty("frames"u8).GetInt32(), layer.GetProperty("frameMs"u8).GetInt32(), layer.GetProperty("loopMs"u8).GetInt32())).ToArray();
            if (preview.Length is < 1 or > 32) throw new InvalidDataException("Invalid emote preview layer count.");
            foreach (var atlas in preview.Append(match))
                if (Path.GetFileName(atlas.File) != atlas.File || atlas.Columns != 8 || atlas.Rows < 1 || atlas.Rows > 8192 / Tile
                    || atlas.Frames < 1 || atlas.Rows != (atlas.Frames + atlas.Columns - 1) / atlas.Columns
                    || atlas.FrameMs != 50 || atlas.LoopMs != checked(atlas.Frames * atlas.FrameMs))
                    throw new InvalidDataException("Invalid emote atlas geometry or loop period.");
            clips.Add(entry.Name, new(match, preview));
        }
        return new(clips);
    }
}

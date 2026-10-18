using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Assets;
using DigiTactics.Storage;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

/// <summary>Original animation frames; textures unused in the preceding frame are released before new draw commands.</summary>
internal sealed class EmoteArtwork : IDisposable
{
    private readonly Dictionary<string, (GpuTexture Texture, long Used)> cache = [];
    private EmoteAtlasCatalog? catalog;
    internal int Uploads { get; private set; }
    internal long CachedBytes => cache.Values.Sum(entry => (long)entry.Texture.Width * entry.Texture.Height * 4);
    private long frame;
    public void BeginFrame()
    {
        frame++;
        foreach (string id in cache.Where(p => p.Value.Used < frame - 1).Select(p => p.Key).ToArray()) { cache[id].Texture.Dispose(); cache.Remove(id); }
    }
    public void Draw(string id, long age, float2 position, float size, bool fading = false, bool arena = false)
    {
        if (CosmeticCollection.Item("emote", id) is null) return;
        catalog ??= EmoteAtlasCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content", "emotes", "manifest.json"));
        if (!catalog.Clips.TryGetValue(id, out var clip)) return;
        var transform = fading ? EmoteLayout.Transform(age, !arena) : new EmoteTransform(1, 0, 0, 1);
        byte alpha = (byte)(255 * transform.Opacity);
        // Atlases include 20px padding around the original 120px SVG. Scale the entire
        // cell so the artwork itself occupies the requested CSS dimensions.
        float cell = size * 160 / 120, drawn = cell * (float)transform.Scale;
        position = new(position.X + size / 2 - drawn / 2 + (float)transform.X, position.Y + size / 2 - drawn / 2 + (float)transform.Y);
        if (fading) DrawAtlas(clip.Match, age, false, position, drawn, alpha);
        else foreach (var layer in clip.Preview) DrawAtlas(layer, age, true, position, drawn, alpha);
    }
    private void DrawAtlas(EmoteAtlas atlas, long age, bool loop, float2 position, float drawn, byte alpha)
    {
        if (!cache.TryGetValue(atlas.File, out var entry))
        {
            var pixels = PngImage.Decode(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "content", "emotes", atlas.File)));
            if (pixels.Width != atlas.Columns * EmoteAtlasCatalog.Tile || pixels.Height != atlas.Rows * EmoteAtlasCatalog.Tile)
                throw new InvalidDataException("Emote texture dimensions do not match its atlas metadata.");
            entry = (new(pixels, new(MinFilter: 9729, WrapS: 33071, WrapT: 33071), false), frame);
            Uploads++;
        }
        cache[atlas.File] = (entry.Texture, frame);
        int index = atlas.FrameAt(age, loop), column = index % atlas.Columns, row = index / atlas.Columns;
        ImGui.GetWindowDrawList().AddImage(new ImTextureRef { _TexID = new((nint)entry.Texture.Id) }, position, new(position.X + drawn, position.Y + drawn),
            new(column / (float)atlas.Columns, row / (float)atlas.Rows), new((column + 1) / (float)atlas.Columns, (row + 1) / (float)atlas.Rows), new ImColor8(255, 255, 255, alpha));
    }
    public void Dispose() { foreach (var entry in cache.Values) entry.Texture.Dispose(); cache.Clear(); }
}

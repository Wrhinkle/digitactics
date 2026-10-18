using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Assets;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Original animation frames; textures unused in the preceding frame are released before new draw commands.</summary>
internal sealed class EmoteArtwork : IDisposable
{
    private readonly Dictionary<string, (GpuTexture Texture, long Used)> cache = [];
    private long frame;
    public void BeginFrame()
    {
        frame++;
        foreach (string id in cache.Where(p => p.Value.Used < frame - 1).Select(p => p.Key).ToArray()) { cache[id].Texture.Dispose(); cache.Remove(id); }
    }
    public void Draw(string id, long age, float2 position, float size, bool fading = false)
    {
        if (CosmeticCollection.Item("emote", id) is null) return;
        if (!cache.TryGetValue(id, out var entry))
            entry = (new(PngImage.Decode(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "content", "emotes", id + ".png"))), new(MinFilter: 9729, WrapS: 33071, WrapT: 33071), false), frame);
        cache[id] = (entry.Texture, frame);
        int index = (int)Math.Clamp(age / 50, 0, 51), column = index % 8, row = index / 8;
        byte alpha = fading ? (byte)(255 * Math.Clamp((2600 - age) / 300d, 0, 1)) : (byte)255;
        ImGui.GetWindowDrawList().AddImage(new ImTextureRef { _TexID = new((nint)entry.Texture.Id) }, position, new(position.X + size, position.Y + size),
            new(column / 8f, row / 7f), new((column + 1) / 8f, (row + 1) / 7f), new ImColor8(255, 255, 255, alpha));
    }
    public void Dispose() { foreach (var entry in cache.Values) entry.Texture.Dispose(); cache.Clear(); }
}

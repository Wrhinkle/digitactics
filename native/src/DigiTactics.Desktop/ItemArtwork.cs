using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>Original item tokens rendered in screen space, with only visible textures retained.</summary>
internal sealed class ItemArtwork : IDisposable
{
    private static readonly string folder = Path.Combine(AppContext.BaseDirectory, "content/item-tokens");
    private sealed record Manifest(Dictionary<string, Dictionary<string, string>> Sets);
    private static readonly Manifest manifest = JsonSerializer.Deserialize<Manifest>(File.ReadAllText(Path.Combine(folder, "manifest.json")), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    private readonly Dictionary<string, (GpuTexture Texture, long Used)> textures = [];
    private long frame;
    public int DrawCalls { get; private set; }

    public void BeginFrame()
    {
        frame++; DrawCalls = 0;
        foreach (var file in textures.Where(p => p.Value.Used < frame - 1).Select(p => p.Key).ToArray())
        { textures[file].Texture.Dispose(); textures.Remove(file); }
    }

    public void Draw(string variant, string item, float2 position, float size)
    {
        if (!manifest.Sets.TryGetValue(variant.Split('/')[0], out var icons) || !icons.TryGetValue(item, out var file))
            throw new InvalidDataException($"Missing item artwork: {variant}/{item}");
        if (!textures.TryGetValue(file, out var entry))
            entry = (new(PngImage.Decode(File.ReadAllBytes(Path.Combine(folder, file))), new(MinFilter: 9729, WrapS: 33071, WrapT: 33071), false), frame);
        textures[file] = (entry.Texture, frame);
        ImGui.GetWindowDrawList().AddImage(new ImTextureRef { _TexID = new((nint)entry.Texture.Id) }, position, new(position.X + size, position.Y + size), new(0, 0), new(1, 1), new ImColor8(255, 255, 255));
        DrawCalls++;
    }

    public void Dispose() { foreach (var entry in textures.Values) entry.Texture.Dispose(); textures.Clear(); }
}

using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

internal sealed class PostgameArtwork(string folder="postgame") : IDisposable
{
    private readonly Dictionary<string, (GpuTexture Texture, int Width, int Height)> textures = [];
    public void Draw(string name, float2 position, float2 size, byte opacity = 255, bool cover = false, float verticalAlignment = .5f,ImDrawListPtr? drawList=null)
    {
        if (!textures.TryGetValue(name, out var item))
        {
            var image = PngImage.Decode(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "content", folder, name + ".png")));
            item = (new(image, new(MinFilter:9729,WrapS:33071,WrapT:33071), false), image.Width, image.Height);
            textures.Add(name, item);
        }
        float2 uv0 = new(0,0), uv1 = new(1,1);
        if (cover)
        {
            float scale = Math.Max(size.X / item.Width, size.Y / item.Height);
            var fraction = new float2(size.X / (item.Width * scale), size.Y / (item.Height * scale));
            uv0 = new((1-fraction.X)*.5f,(1-fraction.Y)*verticalAlignment); uv1 = uv0 + fraction;
        }
        (drawList??ImGui.GetWindowDrawList()).AddImage(new ImTextureRef { _TexID = new((nint)item.Texture.Id) }, position, position + size, uv0, uv1, new ImColor8(255,255,255,opacity));
    }
    public void Dispose() { foreach (var item in textures.Values) item.Texture.Dispose(); textures.Clear(); }
}

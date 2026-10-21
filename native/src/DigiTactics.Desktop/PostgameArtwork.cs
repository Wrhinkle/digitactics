using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

internal sealed class PostgameArtwork(string folder="postgame") : IDisposable
{
    private readonly Dictionary<(string Name,bool Grayscale), (GpuTexture Texture, int Width, int Height)> textures = [];
    public void Draw(string name, float2 position, float2 size, byte opacity = 255, bool cover = false, float verticalAlignment = .5f,ImDrawListPtr? drawList=null,bool grayscale=false,byte brightness=255,float rounding=0)
    {
        var key=(name,grayscale);
        if (!textures.TryGetValue(key, out var item))
        {
            var image = PngImage.Decode(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "content", folder, name + ".png")));
            if(grayscale)
            {
                // Match CSS grayscale(1), retaining the original transparent edges.
                for(int pixel=0;pixel<image.Rgba.Length;pixel+=4)
                {
                    byte gray=(byte)Math.Round(image.Rgba[pixel]*.2126+image.Rgba[pixel+1]*.7152+image.Rgba[pixel+2]*.0722);
                    image.Rgba[pixel]=image.Rgba[pixel+1]=image.Rgba[pixel+2]=gray;
                }
            }
            item = (new(image, new(MinFilter:9729,WrapS:33071,WrapT:33071), false), image.Width, image.Height);
            textures.Add(key, item);
        }
        float2 uv0 = new(0,0), uv1 = new(1,1);
        if (cover)
        {
            float scale = Math.Max(size.X / item.Width, size.Y / item.Height);
            var fraction = new float2(size.X / (item.Width * scale), size.Y / (item.Height * scale));
            uv0 = new((1-fraction.X)*.5f,(1-fraction.Y)*verticalAlignment); uv1 = uv0 + fraction;
        }
        var target = drawList ?? ImGui.GetWindowDrawList(); var texture = new ImTextureRef { _TexID = new((nint)item.Texture.Id) }; var tint = new ImColor8(brightness,brightness,brightness,opacity);
        if (rounding > 0) target.AddImageRounded(texture, position, position + size, uv0, uv1, tint, rounding);
        else target.AddImage(texture, position, position + size, uv0, uv1, tint);
    }
    public void Dispose() { foreach (var item in textures.Values) item.Texture.Dispose(); textures.Clear(); }
}

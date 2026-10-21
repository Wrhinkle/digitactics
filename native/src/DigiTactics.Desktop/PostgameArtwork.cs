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
            // Mipmapped like a browser's downscale: emblems and mode art are drawn far below their source size.
            item = (new(image, new(MinFilter:9987,WrapS:33071,WrapT:33071), false), image.Width, image.Height);
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
    /// <summary>Pixel size of a painting, for object-fit: contain layouts.</summary>
    public float2 Size(string name)
    {
        if (!textures.TryGetValue((name, false), out var item))
        {
            var image = PngImage.Decode(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "content", folder, name + ".png")));
            item = (new(image, new(MinFilter:9729,WrapS:33071,WrapT:33071), false), image.Width, image.Height);
            textures.Add((name, false), item);
        }
        return new(item.Width, item.Height);
    }
    /// <summary>CSS background repeat at <paramref name="tile"/> px with an opacity mask sampled per 32 px cell (mask-image).</summary>
    public void DrawTiled(string name, float2 min, float2 max, float tile, float opacity, Func<float2, float> mask, ImDrawListPtr drawList)
    {
        var key = (name, false);
        if (!textures.TryGetValue(key, out var item))
        {
            var image = PngImage.Decode(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "content", folder, name + ".png")));
            item = (new(image, new(MinFilter:9729,WrapS:10497,WrapT:10497), false), image.Width, image.Height);
            textures.Add(key, item);
        }
        const float cell = 32;
        float alpha = opacity * ImGui.GetStyle().Alpha;
        ImColor8 At(float2 point) => new(255, 255, 255, (byte)Math.Clamp(Math.Round(255 * alpha * mask(point)), 0, 255));
        drawList.PushTexture(new ImTextureRef { _TexID = new((nint)item.Texture.Id) });
        for (float y = min.Y; y < max.Y; y += cell)
        for (float x = min.X; x < max.X; x += cell)
        {
            var a = new float2(x, y); var b = new float2(Math.Min(x + cell, max.X), Math.Min(y + cell, max.Y));
            var ca = At(a); var cb = At(new(b.X, a.Y)); var cc = At(b); var cd = At(new(a.X, b.Y));
            if (ca.AsByte4().W == 0 && cb.AsByte4().W == 0 && cc.AsByte4().W == 0 && cd.AsByte4().W == 0) continue;
            float2 Uv(float2 p) => (p - min) / tile;
            drawList.PrimReserve(6, 6);
            drawList.PrimVtx(a, Uv(a), ca); drawList.PrimVtx(new(b.X, a.Y), Uv(new(b.X, a.Y)), cb); drawList.PrimVtx(b, Uv(b), cc);
            drawList.PrimVtx(a, Uv(a), ca); drawList.PrimVtx(b, Uv(b), cc); drawList.PrimVtx(new(a.X, b.Y), Uv(new(a.X, b.Y)), cd);
        }
        drawList.PopTexture();
    }
    public void Dispose() { foreach (var item in textures.Values) item.Texture.Dispose(); textures.Clear(); }
}

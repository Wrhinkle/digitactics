using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

internal sealed class PortraitArtwork : IDisposable
{
    private static readonly string folder = Path.Combine(AppContext.BaseDirectory,"content/portraits");
    private static readonly ModelCatalog models = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory,"content/models.json"));
    private static readonly Dictionary<string,string> files = JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(Path.Combine(folder,"manifest.json")))!;
    private static readonly string evolutionFolder=Path.Combine(AppContext.BaseDirectory,"content/evolution-portraits");
    private static readonly Dictionary<string,string> evolutionFiles=JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(Path.Combine(evolutionFolder,"manifest.json")))!;
    private readonly Dictionary<string,(GpuTexture Texture,long Used)> cache = [];
    private long frame;
    public int DrawCalls { get; private set; }
    public void BeginFrame()
    {
        frame++; DrawCalls=0;
        foreach(var file in cache.Where(p=>p.Value.Used<frame-1).Select(p=>p.Key).ToArray()) {cache[file].Texture.Dispose();cache.Remove(file);}
    }
    public void Draw(string variant,string line,int stage,float2 position,float size,byte alpha=255,ImColor8? tint=null)
    {
        if (!models.Variants.TryGetValue(variant,out var lines) || !lines.TryGetValue(line,out var forms) || forms[stage] is not { } key) return;
        if (!files.TryGetValue(key,out var file)) throw new InvalidDataException($"Missing portrait for {key}.");
        DrawFile(Path.Combine(folder,file),position,size,tint??new ImColor8(255,255,255,alpha));
    }
    public void DrawEvolution(string form,float2 position,float size,bool discovered)
    {
        if(!evolutionFiles.TryGetValue(form,out var file)) throw new InvalidDataException($"Missing evolution portrait for {form}.");
        DrawFile(Path.Combine(evolutionFolder,file),position,size,discovered?new ImColor8(255,255,255):new ImColor8(0,0,0));
    }
    /// <summary>Draws a legend portrait; <paramref name="grayscale"/> applies CSS grayscale(amount) to the texture (locked cards).</summary>
    public bool DrawCompanion(string? key,float2 position,float size,byte alpha=255,float grayscale=0)
    {
        if(key is null || !evolutionFiles.TryGetValue(key.Split('@')[^1],out var file))return false;
        DrawFile(Path.Combine(evolutionFolder,file),position,size,new ImColor8(255,255,255,alpha),grayscale); return true;
    }
    private void DrawFile(string path,float2 position,float size,ImColor8 tint,float grayscale=0)
    {
        string key=grayscale>0?path+"|gray"+grayscale.ToString(System.Globalization.CultureInfo.InvariantCulture):path;
        if (!cache.TryGetValue(key,out var entry))
        {
            var image=PngImage.Decode(File.ReadAllBytes(path));
            if(grayscale>0)
                for(int pixel=0;pixel<image.Rgba.Length;pixel+=4)
                {
                    double gray=image.Rgba[pixel]*.2126+image.Rgba[pixel+1]*.7152+image.Rgba[pixel+2]*.0722;
                    for(int c=0;c<3;c++)image.Rgba[pixel+c]=(byte)Math.Round(image.Rgba[pixel+c]+(gray-image.Rgba[pixel+c])*grayscale);
                }
            // Trilinear mipmaps: portraits are drawn well below their source size.
            entry=(PostgameArtwork.Sharpen(new(image,new(MinFilter:9987,WrapS:33071,WrapT:33071),false)),frame);
        }
        cache[key]=(entry.Texture,frame);
        ImGui.GetWindowDrawList().AddImage(new ImTextureRef {_TexID=new((nint)entry.Texture.Id)},position,new(position.X+size,position.Y+size),new(0,0),new(1,1),tint);
        DrawCalls++;
    }
    public void Dispose(){foreach(var entry in cache.Values)entry.Texture.Dispose();cache.Clear();}
}

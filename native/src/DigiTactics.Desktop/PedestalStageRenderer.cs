using System.Numerics;
using System.Text.Json;
using Brutal.Numerics;
using DigiTactics.Assets;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

/// <summary>The original client Stage pedestal, separate from the companion's pointer rotation.</summary>
internal sealed class PedestalStageRenderer : IDisposable
{
    private sealed record Part(string Name,GpuPrimitive Geometry,Matrix4D World,float4 Color,
        bool Unlit,bool Transparent,bool DepthWrite,float Roughness,float Metalness,int Side,bool Textured);
    private readonly List<Part> parts=[];
    private readonly GpuTexture texture;
    public PedestalStageRenderer()
    {
        string folder=Path.Combine(AppContext.BaseDirectory,"content/pedestal-stage");
        texture=new(PngImage.Decode(File.ReadAllBytes(Path.Combine(folder,"hex.png"))),new(WrapS:33071,WrapT:33071),true);
        try
        {
            using var data=JsonDocument.Parse(File.ReadAllBytes(Path.Combine(folder,"geometry.json")));
            foreach(var entry in data.RootElement.EnumerateObject())
            {
                var value=entry.Value;var material=value.GetProperty("material"u8);
                var position=Vector(value.GetProperty("position"u8));var rotation=Vector(value.GetProperty("rotation"u8));var scale=Vector(value.GetProperty("scale"u8));
                var color=Vector(material.GetProperty("color"u8));
                var geometry=new GpuPrimitive(new(value.GetProperty("vertices"u8).EnumerateArray().Select(x=>x.GetSingle()).ToArray(),
                    value.GetProperty("indices"u8).EnumerateArray().Select(x=>x.GetUInt32()).ToArray(),-1,true,false));
                parts.Add(new(entry.Name,geometry,new CompanionTransform(position,rotation,scale).Matrix,
                    new((float)color.X,(float)color.Y,(float)color.Z,material.GetProperty("opacity"u8).GetSingle()),
                    material.GetProperty("unlit"u8).GetBoolean(),material.GetProperty("transparent"u8).GetBoolean(),material.GetProperty("depthWrite"u8).GetBoolean(),
                    material.GetProperty("roughness"u8).GetSingle(),material.GetProperty("metalness"u8).GetSingle(),material.GetProperty("side"u8).GetInt32(),
                    material.GetProperty("map"u8).ValueKind!=JsonValueKind.Null));
            }
        }
        catch {Dispose();throw;}
    }
    private static Vector3D Vector(JsonElement value)=>new(value[0].GetDouble(),value[1].GetDouble(),value[2].GetDouble());
    public int Draw(ModelRenderer renderer,Matrix4x4 camera,double seconds,bool transparent)
    {
        int calls=0;
        foreach(var part in parts)
        {
            if(part.Transparent!=transparent)continue;
            var color=part.Color;
            if(part.Name=="ring")
            {var ring=PedestalStage.RingColor(seconds);color=new((float)ring.R,(float)ring.G,(float)ring.B,color.W);}
            renderer.DrawRoom(part.Geometry,part.World,camera,color,part.Textured?texture:null,part.Unlit,part.Transparent,part.DepthWrite,
                roughness:part.Roughness,metalness:part.Metalness,side:part.Side,fog:false,receiveShadow:false);
            calls++;
        }
        return calls;
    }
    public void Dispose(){foreach(var part in parts)part.Geometry.Dispose();parts.Clear();texture.Dispose();}
}

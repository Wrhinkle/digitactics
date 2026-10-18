using System.Numerics;
using DigiTactics.Assets;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed class EvolutionBackdrop : IDisposable
{
    private GpuPrimitive? quad;
    private GpuTexture? background,rays;
    public EvolutionBackdrop()
    {
        try
        {
            float[] vertices=new float[4*GltfPrimitive.Stride];
            float[,] points={{-1,1,0,0},{-1,-1,0,1},{1,-1,1,1},{1,1,1,0}};
            for(int i=0;i<4;i++) {int at=i*GltfPrimitive.Stride;vertices[at]=points[i,0];vertices[at+1]=points[i,1];vertices[at+5]=1;vertices[at+6]=points[i,2];vertices[at+7]=points[i,3];}
            quad=new(new(vertices,[0,1,2,0,2,3],0,true,false));
            GpuTexture Load(string name)=>new(PngImage.Decode(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,"content/evolution-stage",name+".png"))),new(MinFilter:9729,WrapS:33071,WrapT:33071),true);
            background=Load("background");rays=Load("rays");
        }
        catch {Dispose();throw;}
    }
    public void Draw(ModelRenderer renderer,EvolutionTransition? effect)
    {
        renderer.WhiteOverride=false;
        renderer.DrawRoom(quad!,Matrix4D.Identity,Matrix4x4.Identity,new(1,1,1,1),background,true,false,false,fog:false);
        if(effect is null || effect.RayOpacity<=0)return;
        double angle=-effect.RayAngle,scale=520d/180;
        var transform=Matrix4D.Compose(new(0,-.1,0),new(0,0,Math.Sin(angle/2),Math.Cos(angle/2)),new(scale,scale,1));
        renderer.DrawRoom(quad!,transform,Matrix4x4.Identity,new(1,1,1,(float)effect.RayOpacity),rays,true,true,false,fog:false);
    }
    public void Dispose() {quad?.Dispose();background?.Dispose();rays?.Dispose();}
}

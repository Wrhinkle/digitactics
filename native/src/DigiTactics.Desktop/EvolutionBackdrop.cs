using System.Numerics;
using DigiTactics.Assets;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed class EvolutionBackdrop : IDisposable
{
    private GpuPrimitive? quad,backgroundQuad;
    private float previousWidth,previousHeight;
    private GpuTexture? background,rays;
    public EvolutionBackdrop()
    {
        try
        {
            quad=Quad(1,1);
            GpuTexture Load(string name)=>new(PngImage.Decode(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory,"content/evolution-stage",name+".png"))),new(MinFilter:9729,WrapS:33071,WrapT:33071),true);
            background=Load("background");rays=Load("rays");
        }
        catch {Dispose();throw;}
    }
    private static GpuPrimitive Quad(float scaleX,float scaleY)
    {
        float[] vertices=new float[4*GltfPrimitive.Stride];
        float[,] points={{-1,1,0,0},{-1,-1,0,1},{1,-1,1,1},{1,1,1,0}};
        for(int i=0;i<4;i++) {int at=i*GltfPrimitive.Stride;vertices[at]=points[i,0];vertices[at+1]=points[i,1];vertices[at+5]=1;vertices[at+6]=.5f+(points[i,2]-.5f)*scaleX;vertices[at+7]=.62f+(points[i,3]-.62f)*scaleY;}
        return new(new(vertices,[0,1,2,0,2,3],0,true,false));
    }
    public void Draw(ModelRenderer renderer,EvolutionTransition? effect,float width,float height)
    {
        if(backgroundQuad is null || previousWidth!=width || previousHeight!=height)
        {
            // CSS radial-gradient(circle at 50% 62%) keeps a circular radius in pixel space.
            float radius=MathF.Sqrt(width*width*.25f+height*height*.62f*.62f),reference=MathF.Sqrt(.25f+.62f*.62f);
            var next=Quad(width/radius*reference,height/radius*reference);
            backgroundQuad?.Dispose();backgroundQuad=next;previousWidth=width;previousHeight=height;
        }
        renderer.WhiteOverride=false;
        renderer.DrawRoom(backgroundQuad,Matrix4D.Identity,Matrix4x4.Identity,new(1,1,1,1),background,true,false,false,fog:false);
        if(effect is null || effect.RayOpacity<=0)return;
        double angle=-effect.RayAngle;
        var transform=Matrix4D.Compose(new(),new(0,0,Math.Sin(angle/2),Math.Cos(angle/2)),new(1,1,1))
            *Matrix4D.Compose(new(0,-.1,0),new(0,0,0,1),new(520d/width,520d/height,1));
        renderer.DrawRoom(quad!,transform,Matrix4x4.Identity,new(1,1,1,(float)effect.RayOpacity),rays,true,true,false,fog:false);
    }
    public void Dispose() {quad?.Dispose();backgroundQuad?.Dispose();background?.Dispose();rays?.Dispose();}
}

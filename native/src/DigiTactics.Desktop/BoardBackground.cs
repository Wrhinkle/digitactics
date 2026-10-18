using System.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>Original canvas gradient or solid sky, drawn behind the arena without writing depth.</summary>
internal sealed class BoardBackground : IDisposable
{
    private readonly GpuPrimitive geometry;
    private readonly GpuTexture? texture;
    private readonly Vector3 color;

    public BoardBackground(BoardScene scene)
    {
        float[] vertices=new float[4*GltfPrimitive.Stride];
        for(int i=0;i<4;i++)
        {
            int at=i*GltfPrimitive.Stride; float x=i is 1 or 2 ? 1 : 0, y=i>=2 ? 1 : 0;
            vertices[at]=x*2-1; vertices[at+1]=y*2-1; vertices[at+2]=.5f;
            vertices[at+5]=1; vertices[at+6]=x; vertices[at+7]=y;
        }
        geometry=new(new(vertices,[0,1,2,0,2,3],-1,true,false));
        try
        {
            if(scene.Background is { } index)
            {
                var source=scene.Textures[index];
                texture=new(BoardMesh.Image(source),new(WrapS:BoardMesh.Wrap(source.WrapS),WrapT:BoardMesh.Wrap(source.WrapT)),source.ColorSpace=="srgb");
                color=Vector3.One;
            }
            else color=BoardLighting.Linear(scene.Sky[0]);
        }
        catch { geometry.Dispose(); throw; }
    }

    public unsafe void Draw(ModelRenderer renderer)
    {
        OpenGl.Native.glDisable(GlCapability.DepthTest);
        try { renderer.DrawRoom(geometry,Matrix4D.Identity,Matrix4x4.Identity,new(color.X,color.Y,color.Z,1),texture,true,false,false,fog:false); }
        finally { OpenGl.Native.glDepthMask(true); OpenGl.Native.glEnable(GlCapability.DepthTest); }
    }

    public void Dispose() { texture?.Dispose(); geometry.Dispose(); }
}

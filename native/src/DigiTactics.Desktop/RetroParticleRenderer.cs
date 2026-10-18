using System.Numerics;
using System.Text.Json;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

internal sealed class RetroParticleRenderer : IDisposable
{
    private readonly GpuPrimitive puff, beam, glyph;
    private readonly GpuTexture? texture;
    public RetroParticleRenderer(string action, bool hasGlyph)
    {
        string folder=Path.Combine(AppContext.BaseDirectory,"content/retro");
        using var geometry=JsonDocument.Parse(File.ReadAllText(Path.Combine(folder,"geometry.json")));
        GpuPrimitive Read(string key)
        {
            var data=geometry.RootElement.GetProperty(key);
            var vertices=data.GetProperty("vertices"u8).EnumerateArray().Select(x=>x.GetSingle()).ToArray();
            // CanvasTexture flips vertically; native PNG upload preserves row order.
            if(key=="glyph")for(int i=7;i<vertices.Length;i+=GltfPrimitive.Stride)vertices[i]=1-vertices[i];
            return new(new(vertices,data.GetProperty("indices"u8).EnumerateArray().Select(x=>x.GetUInt32()).ToArray(),-1,true,false));
        }
        puff=Read("puff");beam=Read("beam");glyph=Read("glyph");
        if(hasGlyph)texture=new(PngImage.Decode(File.ReadAllBytes(Path.Combine(folder,"retro_"+action+".png"))),new(MinFilter:9729,WrapS:33071,WrapT:33071),true);
    }
    public int Draw(ModelRenderer renderer,Matrix4x4 camera,RetroParticles particles,RetroEmotePose motion,bool ufo)
    {
        int count=0;
        var right=Vector3.Normalize(new(camera.M11,camera.M21,camera.M31));
        var up=Vector3.Normalize(new(camera.M12,camera.M22,camera.M32));
        var back=Vector3.Cross(right,up);
        var facing=Quaternion.CreateFromRotationMatrix(new(right.X,right.Y,right.Z,0,up.X,up.Y,up.Z,0,back.X,back.Y,back.Z,0,0,0,0,1));
        foreach(var particle in particles.Active)
        {
            var q=particle.Puff?Quaternion.Identity:facing;
            var world=Matrix4D.Compose(particle.Position,new(q.X,q.Y,q.Z,q.W),new(particle.Scale,particle.Scale,particle.Scale));
            renderer.DrawRoom(particle.Puff?puff:glyph,world,camera,new(1,1,1,(float)particle.Opacity),particle.Puff?null:texture,true,true,false);count++;
        }
        if(ufo && motion.BeamOpacity>0)
        {
            var at=motion.Holder.Position;
            var world=Matrix4D.Compose(new(at.X,at.Y-motion.BeamHeight/2,at.Z),new(0,0,0,1),new(1,Math.Max(.01,motion.BeamHeight),1));
            static float Linear(double c)=>(float)(c<=.04045?c/12.92:Math.Pow((c+.055)/1.055,2.4));
            renderer.DrawRoom(beam,world,camera,new(Linear(159/255d),Linear(246/255d),1,(float)motion.BeamOpacity),null,true,true,false,additive:true);count++;
        }
        return count;
    }
    public void Dispose(){puff.Dispose();beam.Dispose();glyph.Dispose();texture?.Dispose();}
}

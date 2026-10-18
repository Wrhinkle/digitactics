using Brutal.OpenGlApi;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>Converts a transparent GL source-over surface to straight alpha for ImGui's image blending.</summary>
internal sealed class StraightAlphaTexture : IDisposable
{
    private readonly GlProgram program;
    private readonly GpuPrimitive quad;
    private GlFramebuffer frame;
    private GlTexture texture;
    private int width,height;
    public uint Texture => texture.Id;
    public StraightAlphaTexture()
    {
        using var vertex=OpenGl.CreateShader(GlShaderType.Vertex).SetSource("""
            #version 450 core
            layout(location=0) in vec3 position;
            out vec2 uv;
            void main(){gl_Position=vec4(position,1);uv=position.xy*.5+.5;}
            """u8).Compile();
        using var fragment=OpenGl.CreateShader(GlShaderType.Fragment).SetSource("""
            #version 450 core
            layout(binding=0) uniform sampler2D source;
            in vec2 uv;out vec4 color;
            void main(){vec4 p=texture(source,uv);color=p.a>0?vec4(p.rgb/p.a,p.a):vec4(0);}
            """u8).Compile();
        program=OpenGl.CreateProgram();program.Attach(vertex).Attach(fragment).Link();
        var vertices=new float[4*GltfPrimitive.Stride];
        for(int i=0;i<4;i++){vertices[i*GltfPrimitive.Stride]=(i==0||i==3)?-1:1;vertices[i*GltfPrimitive.Stride+1]=i<2?-1:1;}
        quad=new(new(vertices,[0,1,2,0,2,3],-1,false,false));
    }
    public unsafe void Resolve(uint source,int w,int h)
    {
        if(width!=w||height!=h)
        {
            OpenGl.DeleteTextures(new[]{texture});OpenGl.DeleteFramebuffers(new[]{frame});width=w;height=h;
            Span<GlTexture> textures=stackalloc GlTexture[1];OpenGl.CreateTextures(OpenGl.GL_TEXTURE_2D,textures);texture=textures[0];
            Span<GlFramebuffer> frames=stackalloc GlFramebuffer[1];OpenGl.CreateFramebuffers(frames);frame=frames[0];
            OpenGl.TextureStorage2D(texture.Id,1,OpenGl.GL_RGBA8,w,h);OpenGl.TextureParameterFloat(texture.Id,OpenGl.GL_TEXTURE_MIN_FILTER,OpenGl.GL_LINEAR);OpenGl.TextureParameterFloat(texture.Id,OpenGl.GL_TEXTURE_MAG_FILTER,OpenGl.GL_LINEAR);
            OpenGl.TextureParameterFloat(texture.Id,OpenGl.GL_TEXTURE_WRAP_S,OpenGl.GL_CLAMP_TO_EDGE);OpenGl.TextureParameterFloat(texture.Id,OpenGl.GL_TEXTURE_WRAP_T,OpenGl.GL_CLAMP_TO_EDGE);
            OpenGl.NamedFramebufferTexture(frame,OpenGl.GL_COLOR_ATTACHMENT0,texture.Id,0);OpenGl.NamedFramebufferDrawBuffer(frame,OpenGl.GL_COLOR_ATTACHMENT0);
            if(OpenGl.CheckNamedFramebufferStatus(frame,OpenGl.GL_FRAMEBUFFER)!=OpenGl.GL_FRAMEBUFFER_COMPLETE)throw new InvalidOperationException("Straight-alpha framebuffer is incomplete.");
        }
        OpenGl.BindFramebuffer(OpenGl.GL_FRAMEBUFFER,frame);OpenGl.Viewport(0,0,w,h);OpenGl.Native.glDisable(GlCapability.DepthTest);OpenGl.Native.glDisable(GlCapability.ScissorTest);OpenGl.Native.glDisable(GlCapability.Blend);
        OpenGl.Native.glDisable(GlCapability.CullFace);program.Use();OpenGl.BindTextureUnit(0,source);quad.Draw();ModelRenderer.End();
    }
    public void Dispose(){quad.Dispose();program.Dispose();OpenGl.DeleteTextures(new[]{texture});OpenGl.DeleteFramebuffers(new[]{frame});}
}

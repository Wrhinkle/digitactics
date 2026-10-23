using System.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>Offscreen view of one caster for the special cut-in panel: the live animated model only, on a
/// transparent background, with a rim light in the panel colour. The texture is straight alpha for ImGui.</summary>
internal sealed class SpecialCutInView : IDisposable
{
    private readonly ModelRenderer renderer=new(){EliminationStage=true};
    private readonly StraightAlphaTexture straightAlpha=new();
    private GlFramebuffer frame,resolveFrame;
    private GlTexture color,depth,multisampleColor;
    private int samples=-1;
    public int Width {get;private set;}
    public int Height {get;private set;}
    public uint Texture=>straightAlpha.Texture;
    public int DrawCalls {get;private set;}

    /// <summary>Sizes the target (rounded to 16 px so a resizing panel does not reallocate every frame).</summary>
    public void Ensure(int width,int height)
    {
        width=Math.Clamp((width+15)/16*16,64,1600);height=Math.Clamp((height+15)/16*16,64,900);
        if(width==Width&&height==Height)return;
        Release();
        if(samples<0)samples=CosmeticScenePreview.SupportedSamples();
        Width=width;Height=height;
        Span<GlFramebuffer> frames=stackalloc GlFramebuffer[2];OpenGl.CreateFramebuffers(frames);frame=frames[0];resolveFrame=frames[1];
        Span<GlTexture> textures=stackalloc GlTexture[1];OpenGl.CreateTextures(OpenGl.GL_TEXTURE_2D,textures);color=textures[0];
        OpenGl.TextureStorage2D(color.Id,1,OpenGl.GL_RGBA8,width,height);
        OpenGl.TextureParameterFloat(color.Id,OpenGl.GL_TEXTURE_MIN_FILTER,OpenGl.GL_LINEAR);OpenGl.TextureParameterFloat(color.Id,OpenGl.GL_TEXTURE_MAG_FILTER,OpenGl.GL_LINEAR);
        if(samples>1)
        {
            Span<GlTexture> multisample=stackalloc GlTexture[2];OpenGl.CreateTextures(OpenGl.GL_TEXTURE_2D_MULTISAMPLE,multisample);multisampleColor=multisample[0];depth=multisample[1];
            OpenGl.TextureStorage2DMultisample(multisampleColor.Id,samples,OpenGl.GL_RGBA8,width,height,true);
            OpenGl.TextureStorage2DMultisample(depth.Id,samples,OpenGl.GL_DEPTH_COMPONENT24,width,height,true);
            OpenGl.NamedFramebufferTexture(resolveFrame,OpenGl.GL_COLOR_ATTACHMENT0,color.Id,0);OpenGl.NamedFramebufferDrawBuffer(resolveFrame,OpenGl.GL_COLOR_ATTACHMENT0);
        }
        else {OpenGl.CreateTextures(OpenGl.GL_TEXTURE_2D,textures);depth=textures[0];OpenGl.TextureStorage2D(depth.Id,1,OpenGl.GL_DEPTH_COMPONENT24,width,height);}
        OpenGl.NamedFramebufferTexture(frame,OpenGl.GL_COLOR_ATTACHMENT0,samples>1?multisampleColor.Id:color.Id,0);OpenGl.NamedFramebufferTexture(frame,OpenGl.GL_DEPTH_ATTACHMENT,depth.Id,0);
        OpenGl.NamedFramebufferDrawBuffer(frame,OpenGl.GL_COLOR_ATTACHMENT0);
        if(OpenGl.CheckNamedFramebufferStatus(frame,OpenGl.GL_FRAMEBUFFER)!=OpenGl.GL_FRAMEBUFFER_COMPLETE){Release();throw new InvalidOperationException("Special cut-in framebuffer is incomplete.");}
        // Resolve once so the panel never samples an undefined texture before the first draw.
        Clear();
    }

    private void Clear()
    {
        Span<int> value=stackalloc int[1];OpenGl.GetIntegerv(OpenGl.GL_DRAW_FRAMEBUFFER_BINDING,value);int previousDraw=value[0];
        OpenGl.GetIntegerv(OpenGl.GL_READ_FRAMEBUFFER_BINDING,value);int previousRead=value[0];
        OpenGl.BindFramebuffer(OpenGl.GL_FRAMEBUFFER,frame);OpenGl.Viewport(0,0,Width,Height);OpenGl.ClearColor(new(0,0,0,0));OpenGl.Clear(GlClearBit.Color|GlClearBit.Depth);Resolve();
        OpenGl.BindFramebuffer(OpenGl.GL_DRAW_FRAMEBUFFER,new((uint)previousDraw));OpenGl.BindFramebuffer(OpenGl.GL_READ_FRAMEBUFFER,new((uint)previousRead));
    }
    private void Resolve()
    {
        if(samples>1)OpenGl.BlitNamedFramebuffer(frame.Id,resolveFrame.Id,0,0,Width,Height,0,0,Width,Height,OpenGl.GL_COLOR_BUFFER_BIT,OpenGl.GL_NEAREST);
        straightAlpha.Resolve(color.Id,Width,Height);
    }

    /// <summary>Draws <paramref name="model"/> in its current pose and board placement from <paramref name="eye"/>.</summary>
    /// <param name="near">Near clip distance: geometry between the camera and the head (a swinging arm, a collar or
    /// cape) is cut away so the face stays visible.</param>
    public void Draw(GpuModel model,RigPose pose,Matrix4D placement,Vector3 eye,Vector3 target,Vector3 rim,float near=.02f)
    {
        DrawCalls=0;if(Width==0)return;
        Span<int> value=stackalloc int[1],viewport=stackalloc int[4];
        OpenGl.GetIntegerv(OpenGl.GL_DRAW_FRAMEBUFFER_BINDING,value);int previousDraw=value[0];
        OpenGl.GetIntegerv(OpenGl.GL_READ_FRAMEBUFFER_BINDING,value);int previousRead=value[0];OpenGl.GetIntegerv(OpenGl.GL_VIEWPORT,viewport);
        try
        {
            OpenGl.BindFramebuffer(OpenGl.GL_FRAMEBUFFER,frame);OpenGl.Viewport(0,0,Width,Height);
            OpenGl.ClearColor(new(0,0,0,0));OpenGl.Clear(GlClearBit.Color|GlClearBit.Depth);
            var camera=Matrix4x4.CreateLookAt(eye,target,Vector3.UnitY)*Matrix4x4.CreatePerspectiveFieldOfView((float)Presentation.SpecialCinematic.HeadFov,(float)Width/Height,Math.Max(.02f,near),80);
            renderer.FarmEye=eye;renderer.ShadowTexture=0;renderer.BoardLight=null;renderer.EliminationRim=rim;renderer.EliminationRimIntensity=3.4f;
            ModelRenderer.Begin(Width,Height);
            try
            {
                renderer.Draw(model,pose,placement,camera,false);DrawCalls+=renderer.DrawCalls;
                renderer.BeginTransparency();renderer.Draw(model,pose,placement,camera,true);DrawCalls+=renderer.DrawCalls;renderer.FlushTransparency();
            }
            finally {renderer.ClearTransparency();ModelRenderer.End();}
            Resolve();
        }
        finally
        {
            OpenGl.BindFramebuffer(OpenGl.GL_DRAW_FRAMEBUFFER,new((uint)previousDraw));OpenGl.BindFramebuffer(OpenGl.GL_READ_FRAMEBUFFER,new((uint)previousRead));
            OpenGl.Viewport(viewport[0],viewport[1],viewport[2],viewport[3]);
        }
    }

    private void Release()
    {
        if(Width==0)return;
        OpenGl.DeleteTextures(new[]{color,depth,multisampleColor});OpenGl.DeleteFramebuffers(new[]{frame,resolveFrame});
        color=depth=multisampleColor=default;Width=Height=0;
    }
    public void Dispose(){Release();straightAlpha.Dispose();renderer.Dispose();}
}

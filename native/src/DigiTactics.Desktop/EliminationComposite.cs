using Brutal.ImGuiApi;
using Brutal.ImGuiApi.Backends;
using Brutal.Numerics;
using Brutal.OpenGlApi;

namespace DigiTactics.Desktop;

/// <summary>Composes the entire kill-screen surface before applying its CSS-style group opacity.</summary>
internal sealed unsafe class EliminationComposite : IDisposable
{
    private GlFramebuffer frame;
    private GlTexture texture;
    private int width,height;
    private ImDrawListPtr captured;
    private float2 origin;
    public uint Texture => texture.Id;
    public void Capture(ImDrawListPtr draw,float2 origin,float2 extent,float opacity,ImDrawListPtr destination=default,float rounding=0)
    {
        ReleaseCapture();Resize((int)extent.X,(int)extent.Y);this.origin=origin;
        captured=draw.CloneOutput();
        // Reset the window's output while preserving the active ImGui clip/texture stacks.
        // End() still owns and pops its window clip after this method returns.
        var clips=draw._ClipRectStack.Span.ToArray();var textures=draw._TextureStack.Span.ToArray();var flags=draw.Flags;
        draw.ResetForNewFrame();draw.Flags=flags;
        foreach(var clip in clips)draw.PushClipRect(new(clip.X,clip.Y),new(clip.Z,clip.W),false);
        foreach(var item in textures)draw.PushTexture(item);
        if(destination.Ptr==null)destination=draw;
        var tint=new ImColor8(255,255,255,(byte)Math.Clamp(Math.Round(opacity*255),0,255));
        if(rounding>0)destination.AddImageRounded(new ImTextureRef{_TexID=new((nint)texture.Id)},origin,origin+extent,new(0,1),new(1,0),tint,rounding);
        else destination.AddImage(new ImTextureRef{_TexID=new((nint)texture.Id)},origin,origin+extent,new(0,1),new(1,0),tint);
    }
    public void Render(OpenGlBackend backend)
    {
        if(captured.Ptr==null)return;
        Span<int> value=stackalloc int[1],viewport=stackalloc int[4];
        OpenGl.GetIntegerv(OpenGl.GL_DRAW_FRAMEBUFFER_BINDING,value);int previousDraw=value[0];
        OpenGl.GetIntegerv(OpenGl.GL_READ_FRAMEBUFFER_BINDING,value);int previousRead=value[0];
        OpenGl.GetIntegerv(OpenGl.GL_VIEWPORT,viewport);
        try
        {
            OpenGl.BindFramebuffer(OpenGl.GL_FRAMEBUFFER,frame);OpenGl.Viewport(0,0,width,height);
            OpenGl.ClearColor(new(0,0,0,0));OpenGl.Native.glDisable(GlCapability.ScissorTest);OpenGl.Clear(GlClearBit.Color);
            var current=ImGui.GetDrawData();
            var data=new ImDrawData{Valid=true,DisplayPos=origin,DisplaySize=new(width,height),FramebufferScale=new(1,1),OwnerViewport=current.OwnerViewport,Textures=current.Textures};
            ImGui.Internal.AddDrawListToDrawDataEx(new(&data),out data.CmdLists,captured);
            data.CmdListsCount=data.CmdLists.Count;
            try { backend.RenderDrawData(new(&data)); }
            finally { ImGui.PInvoke.MemFree(data.CmdLists.DataRaw); }
        }
        finally
        {
            OpenGl.BindFramebuffer(OpenGl.GL_DRAW_FRAMEBUFFER,new((uint)previousDraw));OpenGl.BindFramebuffer(OpenGl.GL_READ_FRAMEBUFFER,new((uint)previousRead));OpenGl.Viewport(viewport[0],viewport[1],viewport[2],viewport[3]);ReleaseCapture();
        }
    }
    private void Resize(int w,int h)
    {
        if(width==w&&height==h)return;
        OpenGl.DeleteTextures(new[]{texture});OpenGl.DeleteFramebuffers(new[]{frame});width=w;height=h;
        Span<GlFramebuffer> frames=stackalloc GlFramebuffer[1];OpenGl.CreateFramebuffers(frames);frame=frames[0];
        Span<GlTexture> textures=stackalloc GlTexture[1];OpenGl.CreateTextures(OpenGl.GL_TEXTURE_2D,textures);texture=textures[0];
        OpenGl.TextureStorage2D(texture.Id,1,OpenGl.GL_RGBA8,w,h);OpenGl.TextureParameterFloat(texture.Id,OpenGl.GL_TEXTURE_MIN_FILTER,OpenGl.GL_LINEAR);OpenGl.TextureParameterFloat(texture.Id,OpenGl.GL_TEXTURE_MAG_FILTER,OpenGl.GL_LINEAR);
        OpenGl.TextureParameterFloat(texture.Id,OpenGl.GL_TEXTURE_WRAP_S,OpenGl.GL_CLAMP_TO_EDGE);OpenGl.TextureParameterFloat(texture.Id,OpenGl.GL_TEXTURE_WRAP_T,OpenGl.GL_CLAMP_TO_EDGE);
        OpenGl.NamedFramebufferTexture(frame,OpenGl.GL_COLOR_ATTACHMENT0,texture.Id,0);OpenGl.NamedFramebufferDrawBuffer(frame,OpenGl.GL_COLOR_ATTACHMENT0);
        if(OpenGl.CheckNamedFramebufferStatus(frame,OpenGl.GL_FRAMEBUFFER)!=OpenGl.GL_FRAMEBUFFER_COMPLETE)throw new InvalidOperationException("Elimination composite framebuffer is incomplete.");
    }
    private void ReleaseCapture()
    {
        if(captured.Ptr==null)return;
        captured.SetDrawListSharedData(default);captured.ClearFreeMemory();ImGui.PInvoke.MemFree(captured.Ptr);captured=default;
    }
    public void Dispose(){ReleaseCapture();OpenGl.DeleteTextures(new[]{texture});OpenGl.DeleteFramebuffers(new[]{frame});}
}

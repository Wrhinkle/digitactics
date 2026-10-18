using System.Numerics;
using Brutal.OpenGlApi;

namespace DigiTactics.Desktop;

internal sealed class FarmShadowMap : IDisposable
{
    public const int Size = 1024;
    public static readonly Matrix4x4 Camera = Matrix4x4.CreateLookAt(new(4, 9, 5), Vector3.Zero, Vector3.UnitY) * Matrix4x4.CreateOrthographic(16, 16, 1, 30);
    private GlFramebuffer framebuffer;
    private GlTexture depth;
    private int previousDraw, previousRead;
    public uint Texture => depth.Id;
    public FarmShadowMap()
    {
        Span<GlFramebuffer> frames = stackalloc GlFramebuffer[1]; OpenGl.CreateFramebuffers(frames); framebuffer = frames[0];
        Span<GlTexture> textures = stackalloc GlTexture[1]; OpenGl.CreateTextures(OpenGl.GL_TEXTURE_2D, textures); depth = textures[0];
        try
        {
            OpenGl.TextureStorage2D(depth.Id, 1, OpenGl.GL_DEPTH_COMPONENT24, Size, Size);
            OpenGl.TextureParameterFloat(depth.Id, OpenGl.GL_TEXTURE_MIN_FILTER, OpenGl.GL_NEAREST);
            OpenGl.TextureParameterFloat(depth.Id, OpenGl.GL_TEXTURE_MAG_FILTER, OpenGl.GL_NEAREST);
            OpenGl.TextureParameterFloat(depth.Id, OpenGl.GL_TEXTURE_WRAP_S, OpenGl.GL_CLAMP_TO_EDGE);
            OpenGl.TextureParameterFloat(depth.Id, OpenGl.GL_TEXTURE_WRAP_T, OpenGl.GL_CLAMP_TO_EDGE);
            OpenGl.NamedFramebufferTexture(framebuffer, OpenGl.GL_DEPTH_ATTACHMENT, depth.Id, 0);
            OpenGl.NamedFramebufferDrawBuffer(framebuffer, OpenGl.GL_NONE);
            OpenGl.NamedFramebufferReadBuffer(framebuffer, OpenGl.GL_NONE);
            if (OpenGl.CheckNamedFramebufferStatus(framebuffer, OpenGl.GL_FRAMEBUFFER) != OpenGl.GL_FRAMEBUFFER_COMPLETE) throw new InvalidOperationException("Farm shadow framebuffer is incomplete.");
        }
        catch { Dispose(); throw; }
    }
    public void Begin()
    {
        Span<int> value = stackalloc int[1];
        OpenGl.GetIntegerv(OpenGl.GL_DRAW_FRAMEBUFFER_BINDING, value); previousDraw = value[0];
        OpenGl.GetIntegerv(OpenGl.GL_READ_FRAMEBUFFER_BINDING, value); previousRead = value[0];
        OpenGl.BindFramebuffer(OpenGl.GL_FRAMEBUFFER, framebuffer); ModelRenderer.Begin(Size, Size);
    }
    public void End()
    {
        OpenGl.BindFramebuffer(OpenGl.GL_DRAW_FRAMEBUFFER, new((uint)previousDraw));
        OpenGl.BindFramebuffer(OpenGl.GL_READ_FRAMEBUFFER, new((uint)previousRead));
    }
    public void Dispose() { OpenGl.DeleteTextures(new[] { depth }); OpenGl.DeleteFramebuffers(new[] { framebuffer }); }
}


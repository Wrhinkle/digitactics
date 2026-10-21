using Brutal.OpenGlApi;

namespace DigiTactics.Desktop;

/// <summary>render3d.js creates its WebGLRenderer with antialias:true, a multisampled canvas (normally four
/// samples). The arena scene draws into a matching multisampled target at the arena size, which is then
/// resolved into the window framebuffer at the arena's position. Without multisampling (constrained hardware)
/// or an incomplete target, the scene draws straight into the window as before.</summary>
internal sealed class ArenaFrameTarget : IDisposable
{
    private GlFramebuffer frame;
    private GlTexture color, depth;
    private int width, height, samples = -1, previousDraw, previousRead;
    public int Samples => Math.Max(1, samples);

    /// <summary>Binds the target for a <paramref name="w"/> x <paramref name="h"/> scene. Returns false when the
    /// scene must draw into the window directly.</summary>
    public bool Begin(int w, int h)
    {
        if (samples < 0) samples = CosmeticScenePreview.SupportedSamples();
        if (samples <= 1 || w <= 0 || h <= 0) return false;
        if (w != width || h != height) Allocate(w, h);
        if (samples <= 1) return false;
        Span<int> value = stackalloc int[1];
        OpenGl.GetIntegerv(OpenGl.GL_DRAW_FRAMEBUFFER_BINDING, value); previousDraw = value[0];
        OpenGl.GetIntegerv(OpenGl.GL_READ_FRAMEBUFFER_BINDING, value); previousRead = value[0];
        OpenGl.BindFramebuffer(OpenGl.GL_FRAMEBUFFER, frame); OpenGl.Viewport(0, 0, w, h);
        OpenGl.ClearColor(new(0, 0, 0, 1)); OpenGl.Clear(GlClearBit.Color | GlClearBit.Depth);
        return true;
    }

    /// <summary>Resolves the samples into the previous framebuffer with the scene's lower-left corner at (x, y).</summary>
    public void End(int x, int y)
    {
        OpenGl.BlitNamedFramebuffer(frame.Id, (uint)previousDraw, 0, 0, width, height, x, y, x + width, y + height, OpenGl.GL_COLOR_BUFFER_BIT, OpenGl.GL_NEAREST);
        OpenGl.BindFramebuffer(OpenGl.GL_DRAW_FRAMEBUFFER, new((uint)previousDraw));
        OpenGl.BindFramebuffer(OpenGl.GL_READ_FRAMEBUFFER, new((uint)previousRead));
    }

    private void Allocate(int w, int h)
    {
        Release();
        width = w; height = h;
        Span<GlFramebuffer> frames = stackalloc GlFramebuffer[1]; OpenGl.CreateFramebuffers(frames); frame = frames[0];
        Span<GlTexture> textures = stackalloc GlTexture[2]; OpenGl.CreateTextures(OpenGl.GL_TEXTURE_2D_MULTISAMPLE, textures); color = textures[0]; depth = textures[1];
        OpenGl.TextureStorage2DMultisample(color.Id, samples, OpenGl.GL_RGBA8, w, h, true);
        OpenGl.TextureStorage2DMultisample(depth.Id, samples, OpenGl.GL_DEPTH_COMPONENT24, w, h, true);
        OpenGl.NamedFramebufferTexture(frame, OpenGl.GL_COLOR_ATTACHMENT0, color.Id, 0);
        OpenGl.NamedFramebufferTexture(frame, OpenGl.GL_DEPTH_ATTACHMENT, depth.Id, 0);
        OpenGl.NamedFramebufferDrawBuffer(frame, OpenGl.GL_COLOR_ATTACHMENT0);
        if (OpenGl.CheckNamedFramebufferStatus(frame, OpenGl.GL_FRAMEBUFFER) != OpenGl.GL_FRAMEBUFFER_COMPLETE)
        { Release(); samples = 1; }
    }

    private void Release()
    {
        if (width == 0) return;
        OpenGl.DeleteTextures(new[] { color, depth }); OpenGl.DeleteFramebuffers(new[] { frame });
        width = height = 0;
    }

    public void Dispose() => Release();
}

using System.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>Native offscreen companion stage, owned and rendered on the GL context thread.</summary>
internal sealed class CompanionPreview : IDisposable
{
    private const int Size=384;
    private readonly ModelCatalog models=ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory,"content/models.json"));
    private readonly CompanionCatalog companions=CompanionCatalog.Load(Path.Combine(AppContext.BaseDirectory,"content/companion-models.json"));
    private readonly ModelRenderer renderer=new();
    private GlFramebuffer frame;
    private GlTexture color,depth;
    private GpuModel? model;
    private CompanionAnimator? animator;
    private string? key;
    private double? lastTime;
    public uint Texture=>color.Id;
    public int DrawCalls { get; private set; }
    public double AnimationTime=>animator?.Time??0;
    public string? Key=>key;
    public CompanionPreview()
    {
        try
        {
            Span<GlFramebuffer> frames=stackalloc GlFramebuffer[1]; OpenGl.CreateFramebuffers(frames); frame=frames[0];
            Span<GlTexture> textures=stackalloc GlTexture[2]; OpenGl.CreateTextures(OpenGl.GL_TEXTURE_2D,textures); color=textures[0];depth=textures[1];
            OpenGl.TextureStorage2D(color.Id,1,OpenGl.GL_RGBA8,Size,Size);
            OpenGl.TextureStorage2D(depth.Id,1,OpenGl.GL_DEPTH_COMPONENT24,Size,Size);
            OpenGl.TextureParameterFloat(color.Id,OpenGl.GL_TEXTURE_MIN_FILTER,OpenGl.GL_LINEAR);
            OpenGl.TextureParameterFloat(color.Id,OpenGl.GL_TEXTURE_MAG_FILTER,OpenGl.GL_LINEAR);
            OpenGl.TextureParameterFloat(color.Id,OpenGl.GL_TEXTURE_WRAP_S,OpenGl.GL_CLAMP_TO_EDGE);
            OpenGl.TextureParameterFloat(color.Id,OpenGl.GL_TEXTURE_WRAP_T,OpenGl.GL_CLAMP_TO_EDGE);
            OpenGl.NamedFramebufferTexture(frame,OpenGl.GL_COLOR_ATTACHMENT0,color.Id,0);
            OpenGl.NamedFramebufferTexture(frame,OpenGl.GL_DEPTH_ATTACHMENT,depth.Id,0);
            OpenGl.NamedFramebufferDrawBuffer(frame,OpenGl.GL_COLOR_ATTACHMENT0);
            if(OpenGl.CheckNamedFramebufferStatus(frame,OpenGl.GL_FRAMEBUFFER)!=OpenGl.GL_FRAMEBUFFER_COMPLETE) throw new InvalidOperationException("Companion preview framebuffer is incomplete.");
        }
        catch { Dispose();throw; }
    }
    public void Select(string next)
    {
        if(key==next)return;
        var visual=companions.Resolve(next);
        var replacement=new GpuModel(models,models.Sources[visual.Source]);
        CompanionAnimator nextAnimator;
        try { nextAnimator=replacement.CreateCompanionAnimator(visual,companions.Fallbacks,1,.35); }
        catch { replacement.Dispose();throw; }
        model?.Dispose();model=replacement;animator=nextAnimator;key=next;lastTime=null;
    }
    public void Play(string role)=>animator?.Play(role);
    public void Draw(double time)
    {
        DrawCalls=0;if(model is null || animator is null)return;
        animator.Update(lastTime is { } previous?Math.Clamp(time-previous,0,.05):0);lastTime=time;
        Span<int> value=stackalloc int[1],viewport=stackalloc int[4];
        OpenGl.GetIntegerv(OpenGl.GL_DRAW_FRAMEBUFFER_BINDING,value);int previousDraw=value[0];
        OpenGl.GetIntegerv(OpenGl.GL_READ_FRAMEBUFFER_BINDING,value);int previousRead=value[0];
        OpenGl.GetIntegerv(OpenGl.GL_VIEWPORT,viewport);
        try
        {
            OpenGl.BindFramebuffer(OpenGl.GL_FRAMEBUFFER,frame);ModelRenderer.Begin(Size,Size);
            OpenGl.ClearColor(new(.035f,.05f,.075f,1));OpenGl.Clear(GlClearBit.Color|GlClearBit.Depth);
            var camera=Matrix4x4.CreateLookAt(new(0,1,3.6f),new(0,.6f,0),Vector3.UnitY)*Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI/6,1,.01f,50);
            foreach(bool transparent in new[]{false,true})
            {
                renderer.Draw(model,animator.Pose,model.PlaceCompanion(animator),camera,transparent);DrawCalls+=renderer.DrawCalls;
            }
            ModelRenderer.End();
        }
        finally
        {
            OpenGl.BindFramebuffer(OpenGl.GL_DRAW_FRAMEBUFFER,new((uint)previousDraw));
            OpenGl.BindFramebuffer(OpenGl.GL_READ_FRAMEBUFFER,new((uint)previousRead));
            OpenGl.Viewport(viewport[0],viewport[1],viewport[2],viewport[3]);
        }
    }
    public void Dispose() { model?.Dispose();renderer.Dispose();OpenGl.DeleteTextures(new[]{color,depth});OpenGl.DeleteFramebuffers(new[]{frame}); }
}

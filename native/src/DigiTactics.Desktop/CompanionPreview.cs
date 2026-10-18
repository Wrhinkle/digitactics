using System.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

/// <summary>Native offscreen companion stage, owned and rendered on the GL context thread.</summary>
internal sealed class CompanionPreview : IDisposable
{
    private const int Size=384;
    private readonly ModelCatalog models=ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory,"content/models.json"));
    private readonly CompanionCatalog companions=CompanionCatalog.Load(Path.Combine(AppContext.BaseDirectory,"content/companion-models.json"));
    private readonly ModelRenderer renderer=new() {EvolutionStage=true,FarmEye=new(0,1,3.6f)};
    private GpuPrimitive? floor;
    private EvolutionBackdrop? backdrop;
    public double RaysOpacity=>transition?.RayOpacity??0;
    private GlFramebuffer frame;
    private GlTexture color,depth;
    private GpuModel? model;
    private CompanionAnimator? animator;
    private string? key;
    private double? lastTime;
    private GpuModel? outgoing;
    private CompanionAnimator? outgoingAnimator;
    private EvolutionTransition? transition;
    public double TransitionTime=>transition?.Time??0;
    public bool Evolving=>transition is {Complete:false};
    public double FlashOpacity=>transition?.FlashOpacity??0;
    public Action? OnSettled { get; set; }
    public Action? OnFlash { get; set; }
    public void TransformTo(string next)
    {
        if(key==next || Evolving)return;
        var previous=model; var previousAnimator=animator;
        model=null;
        try { Select(next); }
        catch { model=previous;animator=previousAnimator;throw; }
        outgoing?.Dispose();outgoing=previous;outgoingAnimator=previousAnimator;
        transition=previous is null?null:new();
    }
    public uint Texture=>color.Id;
    public bool FloorDrawn { get; private set; }
    public int DrawCalls { get; private set; }
    public double AnimationTime=>animator?.Time??0;
    public string? Key=>key;
    public CompanionPreview()
    {
        try
        {
            backdrop=new();
            var vertices=new float[42*GltfPrimitive.Stride];var indices=new uint[40*3];
            for(int i=0;i<=40;i++)
            {
                double angle=(double)i/40*Math.PI*2;int at=(i+1)*GltfPrimitive.Stride;
                vertices[at]=(float)(Math.Cos(angle)*.75);vertices[at+2]=(float)(-Math.Sin(angle)*.75);vertices[at+4]=1;
                if(i<40){indices[i*3]=0;indices[i*3+1]=(uint)i+1;indices[i*3+2]=(uint)i+2;}
            }
            vertices[4]=1;floor=new(new(vertices,indices,0,true,false));
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
        FloorDrawn=false;DrawCalls=0;if(model is null || animator is null)return;
        double delta=lastTime is { } previous?Math.Clamp(time-previous,0,.05):0;lastTime=time;
        animator.Update(delta);outgoingAnimator?.Update(delta);
        if(transition is { } effect)
        {
            if(effect.Advance(delta))OnFlash?.Invoke();
            if(effect.Complete && outgoing is not null) { outgoing.Dispose();outgoing=null;outgoingAnimator=null;animator.Play("happy");OnSettled?.Invoke(); }
            if(effect.Time>=(effect.FlashStarted??double.PositiveInfinity)+.9)transition=null;
        }
        Span<int> value=stackalloc int[1],viewport=stackalloc int[4];
        OpenGl.GetIntegerv(OpenGl.GL_DRAW_FRAMEBUFFER_BINDING,value);int previousDraw=value[0];
        OpenGl.GetIntegerv(OpenGl.GL_READ_FRAMEBUFFER_BINDING,value);int previousRead=value[0];
        OpenGl.GetIntegerv(OpenGl.GL_VIEWPORT,viewport);
        try
        {
            OpenGl.BindFramebuffer(OpenGl.GL_FRAMEBUFFER,frame);ModelRenderer.Begin(Size,Size);
            OpenGl.ClearColor(new(.035f,.05f,.075f,1));OpenGl.Clear(GlClearBit.Color|GlClearBit.Depth);
            backdrop!.Draw(renderer,transition);
            var camera=Matrix4x4.CreateLookAt(new(0,1,3.6f),new(0,.6f,0),Vector3.UnitY)*Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI/6,1,.01f,50);
            renderer.WhiteOverride=transition?.White??false;
            var active=transition is {ShowNew:false} && outgoing is not null?outgoing:model;
            var pose=active==outgoing?outgoingAnimator!:animator;
            var placement=active.PlaceCompanion(pose);
            if(transition is { } motion) placement*=Matrix4D.Compose(new(),new(0,Math.Sin(motion.Yaw/2),0,Math.Cos(motion.Yaw/2)),new(motion.Scale,motion.Scale,motion.Scale));
            foreach(bool transparent in new[]{false,true})
            {
                renderer.Draw(active,pose.Pose,placement,camera,transparent);DrawCalls+=renderer.DrawCalls;
                if(transparent && !renderer.WhiteOverride)
                {
                    renderer.DrawRoom(floor!,Matrix4D.Identity,camera,new(0,0,0,.3f),null,true,true,true,side:0,fog:false);FloorDrawn=true;
                }
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
    public void Dispose() { backdrop?.Dispose();floor?.Dispose();outgoing?.Dispose();model?.Dispose();renderer.Dispose();OpenGl.DeleteTextures(new[]{color,depth});OpenGl.DeleteFramebuffers(new[]{frame}); }
}

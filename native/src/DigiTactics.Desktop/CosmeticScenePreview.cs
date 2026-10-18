using System.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

/// <summary>One reusable native render target for the selected store item. No game state is mutated.</summary>
internal sealed partial class CosmeticScenePreview : IDisposable
{
    public const int Width=384, Height=240;
    private readonly ModelCatalog models=ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory,"content/models.json"));
    private readonly CompanionCatalog companions=CompanionCatalog.Load(Path.Combine(AppContext.BaseDirectory,"content/companion-models.json"));
    private static readonly Lazy<DanceCatalog> dances=new(()=>DanceCatalog.Load(Path.Combine(AppContext.BaseDirectory,"content/dance-bindings.json")));
    private readonly ModelRenderer renderer=new();
    private readonly StraightAlphaTexture straightAlpha=new();
    private PedestalStageRenderer? pedestal;
    private ArenaEnvironment? board;
    private GpuModel? model;
    private CompanionAnimator? animator;
    private DanceSkeleton? dancer;
    private string? modelKey;
    private double modelHeight;
    private GlFramebuffer frame,resolveFrame;
    private GlTexture color,depth,multisampleColor;
    private double? previousTime;
    private double pedestalTime;
    public uint Texture=>straightAlpha.Texture;
    public int DrawCalls {get;private set;}
    public int PedestalDrawCalls {get;private set;}
    public int Samples {get;}
    public double AnimationTime=>animator?.Time??0;
    public CosmeticScenePreview(PedestalInteraction? interaction=null)
    {
        pointer=interaction??new();
        try
        {
            Samples=SupportedSamples();
            Span<GlFramebuffer> frames=stackalloc GlFramebuffer[2];OpenGl.CreateFramebuffers(frames);frame=frames[0];resolveFrame=frames[1];
            Span<GlTexture> textures=stackalloc GlTexture[1];OpenGl.CreateTextures(OpenGl.GL_TEXTURE_2D,textures);color=textures[0];
            OpenGl.TextureStorage2D(color.Id,1,OpenGl.GL_RGBA8,Width,Height);
            OpenGl.TextureParameterFloat(color.Id,OpenGl.GL_TEXTURE_MIN_FILTER,OpenGl.GL_LINEAR);OpenGl.TextureParameterFloat(color.Id,OpenGl.GL_TEXTURE_MAG_FILTER,OpenGl.GL_LINEAR);
            OpenGl.TextureParameterFloat(color.Id,OpenGl.GL_TEXTURE_WRAP_S,OpenGl.GL_CLAMP_TO_EDGE);OpenGl.TextureParameterFloat(color.Id,OpenGl.GL_TEXTURE_WRAP_T,OpenGl.GL_CLAMP_TO_EDGE);
            if(Samples>1)
            {
                Span<GlTexture> multisample=stackalloc GlTexture[2];OpenGl.CreateTextures(OpenGl.GL_TEXTURE_2D_MULTISAMPLE,multisample);multisampleColor=multisample[0];depth=multisample[1];
                OpenGl.TextureStorage2DMultisample(multisampleColor.Id,Samples,OpenGl.GL_RGBA8,Width,Height,true);
                OpenGl.TextureStorage2DMultisample(depth.Id,Samples,OpenGl.GL_DEPTH_COMPONENT24,Width,Height,true);
                OpenGl.NamedFramebufferTexture(resolveFrame,OpenGl.GL_COLOR_ATTACHMENT0,color.Id,0);OpenGl.NamedFramebufferDrawBuffer(resolveFrame,OpenGl.GL_COLOR_ATTACHMENT0);
                if(OpenGl.CheckNamedFramebufferStatus(resolveFrame,OpenGl.GL_FRAMEBUFFER)!=OpenGl.GL_FRAMEBUFFER_COMPLETE)throw new InvalidOperationException("Cosmetic preview resolve framebuffer is incomplete.");
            }
            else
            {
                OpenGl.CreateTextures(OpenGl.GL_TEXTURE_2D,textures);depth=textures[0];OpenGl.TextureStorage2D(depth.Id,1,OpenGl.GL_DEPTH_COMPONENT24,Width,Height);
            }
            OpenGl.NamedFramebufferTexture(frame,OpenGl.GL_COLOR_ATTACHMENT0,Samples>1?multisampleColor.Id:color.Id,0);OpenGl.NamedFramebufferTexture(frame,OpenGl.GL_DEPTH_ATTACHMENT,depth.Id,0);
            OpenGl.NamedFramebufferDrawBuffer(frame,OpenGl.GL_COLOR_ATTACHMENT0);
            if(OpenGl.CheckNamedFramebufferStatus(frame,OpenGl.GL_FRAMEBUFFER)!=OpenGl.GL_FRAMEBUFFER_COMPLETE)throw new InvalidOperationException("Cosmetic preview framebuffer is incomplete.");
        }
        catch {Dispose();throw;}
    }
    private static int SupportedSamples()
    {
        static int[] Counts(uint format)
        {
            Span<int> count=stackalloc int[1];OpenGl.GetInternalformativ(OpenGl.GL_TEXTURE_2D_MULTISAMPLE,format,OpenGl.GL_NUM_SAMPLE_COUNTS,count);
            var values=new int[Math.Max(0,count[0])];if(values.Length>0)OpenGl.GetInternalformativ(OpenGl.GL_TEXTURE_2D_MULTISAMPLE,format,OpenGl.GL_SAMPLES,values);return values;
        }
        // Browser antialias:true normally uses four samples. Both attachments
        // must support the same count; constrained hardware keeps a valid target.
        return SelectSamples(Counts(OpenGl.GL_RGBA8),Counts(OpenGl.GL_DEPTH_COMPONENT24));
    }
    internal static int SelectSamples(IEnumerable<int> colorSamples,IEnumerable<int> depthSamples)
        =>colorSamples.Intersect(depthSamples).Where(count=>count>1&&count<=4).DefaultIfEmpty(1).Max();
    private void Resolve()
    {
        if(Samples>1)OpenGl.BlitNamedFramebuffer(frame.Id,resolveFrame.Id,0,0,Width,Height,0,0,Width,Height,OpenGl.GL_COLOR_BUFFER_BIT,OpenGl.GL_NEAREST);
        straightAlpha.Resolve(color.Id,Width,Height);
    }
    public void Draw(string kind,string id,string legend,double time,bool motion=true)
    {
        if(kind!="legend")CancelPointer();
        DrawCalls=PedestalDrawCalls=0;
        double delta=previousTime is {} before?Math.Clamp(time-before,0,.1):0;previousTime=time;
        pedestalTime+=delta;
        Span<int> value=stackalloc int[1],viewport=stackalloc int[4];
        OpenGl.GetIntegerv(OpenGl.GL_DRAW_FRAMEBUFFER_BINDING,value);int previousDraw=value[0];
        OpenGl.GetIntegerv(OpenGl.GL_READ_FRAMEBUFFER_BINDING,value);int previousRead=value[0];OpenGl.GetIntegerv(OpenGl.GL_VIEWPORT,viewport);
        try
        {
            OpenGl.BindFramebuffer(OpenGl.GL_FRAMEBUFFER,frame);OpenGl.Viewport(0,0,Width,Height);
            OpenGl.ClearColor(new(0,0,0,0));OpenGl.Clear(GlClearBit.Color|GlClearBit.Depth);
            if(kind=="board")
            {
                renderer.CosmeticStage=renderer.PedestalStage=false;
                board??=new(){Stage=4,Evolution=0,Won=false,CombatActive=false};board.Time=2;board.SetTheme(id);
                var eye=new Vector3(0,16.5f*1.02f,1.8f+15.5f*1.02f);
                var camera=Matrix4x4.CreateLookAt(eye,new(0,0,1.8f),Vector3.UnitY)*Matrix4x4.CreatePerspectiveFieldOfView(38*MathF.PI/180,(float)Width/Height,.1f,200);
                board.Draw(renderer,camera,eye,7,true,0,0,Width,Height,(_,_)=>{});DrawCalls=board.DrawCalls;
                Resolve();return;
            }
            double height=kind=="legend"?PedestalStage.LegendHeight:1;
            if(modelKey!=legend||modelHeight!=height)
            {
                var visual=companions.Resolve(legend);var replacement=new GpuModel(models,models.Sources[visual.Source]);
                CompanionAnimator next;DanceSkeleton nextDancer;
                try {next=replacement.CreateCompanionAnimator(visual,companions.Fallbacks,height);nextDancer=new(dances.Value.Binding(visual.Source));}
                catch {replacement.Dispose();throw;}
                model?.Dispose();model=replacement;animator=next;dancer=nextDancer;modelKey=legend;modelHeight=height;
            }
            animator!.Update(delta);
            DanceRoot? root=kind=="dance"?dancer!.Apply(DanceChoreography.Evaluate(id,time),animator.Pose,1):null;
            renderer.BoardLight=null;renderer.EvolutionStage=false;renderer.CosmeticStage=true;renderer.ShadowTexture=0;
            renderer.PedestalStage=kind=="legend";
            var companionEye=new Vector3(MathF.Sin(.35f)*3.3f,.75f,MathF.Cos(.35f)*3.3f);renderer.FarmEye=companionEye;
            var companionCamera=Matrix4x4.CreateLookAt(companionEye,new(0,.55f,0),Vector3.UnitY)*Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI/6,(float)Width/Height,.05f,50);
            if(kind=="legend")
            {
                pedestal??=new();var source=PedestalStage.Camera(Width,Height);
                companionEye=new((float)source.Eye.X,(float)source.Eye.Y,(float)source.Eye.Z);renderer.FarmEye=companionEye;
                var target=new Vector3((float)source.Target.X,(float)source.Target.Y,(float)source.Target.Z);
                companionCamera=Matrix4x4.CreateLookAt(companionEye,target,Vector3.UnitY)*Matrix4x4.CreatePerspectiveFieldOfView((float)source.FovRadians,(float)source.Aspect,(float)source.Near,(float)source.Far);
            }
            ModelRenderer.Begin(Width,Height);
            var placement=model!.PlaceCompanion(animator,dance:root);
            if(kind=="legend")placement*=GpuModel.Transform(0,0,0,pointer.Yaw(pedestalTime,motion),1);
            try
            {
                if(kind=="legend")PedestalDrawCalls+=pedestal!.Draw(renderer,companionCamera,pedestalTime,false);
                renderer.Draw(model!,animator.Pose,placement,companionCamera,false);DrawCalls+=renderer.DrawCalls;
                renderer.BeginTransparency();
                if(kind=="legend")PedestalDrawCalls+=pedestal!.Draw(renderer,companionCamera,pedestalTime,true);
                renderer.Draw(model!,animator.Pose,placement,companionCamera,true);DrawCalls+=renderer.DrawCalls;
                renderer.FlushTransparency();DrawCalls+=PedestalDrawCalls;
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
    public void Dispose(){CancelPointer();board?.Dispose();model?.Dispose();pedestal?.Dispose();straightAlpha.Dispose();renderer.Dispose();OpenGl.DeleteTextures(new[]{color,depth,multisampleColor});OpenGl.DeleteFramebuffers(new[]{frame,resolveFrame});}
}

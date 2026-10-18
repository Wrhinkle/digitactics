using System.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Owns the final-hit actor and the complete, synchronised dancing lineup on one offscreen GL stage.</summary>
internal sealed class EliminationStage : IDisposable
{
    private sealed class Member(EliminationMember unit, EliminationSlot slot, int order)
    {
        public readonly EliminationMember Unit = unit;
        public readonly EliminationSlot Slot = slot;
        public readonly int Order = order;
        public GpuModel? Model;
        public ModelAnimator? Animator;
        public CompanionAnimator? Companion;
        public DanceSkeleton Dancer = new(new([], true, false));
        public Matrix4D Placement;
        public double? Born;
        public RigPose? Pose => Companion?.Pose ?? Animator?.Pose;
    }
    private static readonly Lazy<ModelCatalog> Models = new(() => ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json")));
    private static readonly Lazy<CompanionCatalog> Companions = new(() => CompanionCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/companion-models.json")));
    private static readonly Lazy<DanceCatalog> Dances = new(() => DanceCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/dance-bindings.json")));
    private readonly Dictionary<ModelSource, GpuModel> models = [];
    private readonly List<Member> members = [];
    private readonly ModelRenderer renderer = new() { EliminationStage = true };
    private readonly EliminationCamera camera = new();
    private readonly StraightAlphaTexture straightAlpha = new();
    private readonly EliminationPlayback playback;
    private readonly Ruleset rules;
    private readonly string dance;
    private readonly bool silhouette;
    private readonly PlaceholderCreature placeholder = new();
    private readonly GpuPrimitive shadowPlane, honorRing;
    private readonly GpuTexture shadow;
    private EliminationLayout layout;
    private GlFramebuffer frame;
    private GlTexture color, depth;
    private int width, height;
    private double lastTime, lastLegendMeasure = -1, legendTop = 1.05;
    private Member? hero;
    private NativeMoveEffects? moves;
    public int EffectDrawCalls { get; private set; }
    public bool Special { get; private set; }
    public bool Fired { get; private set; }
    public int DrawCalls { get; private set; }
    public int Members => members.Count;
    public uint Texture => straightAlpha.Texture;
    public double PoseSignature => members.Sum(m => m.Placement.M41 * 11 + m.Placement.M42 * 13 + m.Placement.M22 * 7 + (m.Pose?.Local.Sum(n => n.Rotation.X * 3 + n.Rotation.Y * 5 + n.Rotation.Z * 7) ?? 0));
    public EliminationStage(Ruleset rules, EliminationPlayback playback, string? companion, string dance, string? theme)
    {
        this.rules = rules; this.playback = playback; this.dance = dance; silhouette = EliminationThemeRenderer.Silhouette(theme);
        var rim = EliminationThemes.Rim(theme, false);
        static float Linear(double c) => (float)(c <= .04045 ? c / 12.92 : Math.Pow((c + .055) / 1.055, 2.4));
        renderer.EliminationRim = new(Linear(rim.R/255), Linear(rim.G/255), Linear(rim.B/255));
        layout = EliminationLayout.Create(playback.Event.Team.Length, 16d/9);
        shadowPlane = Plane(); honorRing = Ring(); shadow = Shadow();
        try
        {
            if (companion is not null)
            {
                var visual = Companions.Value.Resolve(companion);
                var model = Model(Models.Value.Sources[visual.Source]);
                var m = new Member(new("",0), EliminationLayout.Legend, 0) { Model = model,
                    Companion = model.CreateCompanionAnimator(visual, Companions.Value.Fallbacks, 1, 0), Dancer = new(Dances.Value.Binding(visual.Source)) };
                members.Add(m);
            }
            for (int i = 0; i < playback.Event.Team.Length; i++) members.Add(Unit(playback.Event.Team[i],layout.Slots[i],i+1));
            if (members.Count == 0 && playback.Event.Killer is not null)
            {
                var line = rules.Lines.Concat(rules.ExtraLines).FirstOrDefault(l => l.Id == "patamon") ?? rules.Lines[0];
                layout = EliminationLayout.Create(1, 16d/9);members.Add(Unit(new(line.Id,0),layout.Slots[0],1));
            }
            if (playback.Cinematic && playback.Event.FinalHit is { } hit) { hero = Unit(hit, default, -1); moves = new(rules.Id); }
        }
        catch { Dispose();throw; }
    }
    private GpuModel Model(ModelSource source)
    { if (!models.TryGetValue(source,out var model)) models[source] = model = new(Models.Value,source); return model; }
    private Member Unit(EliminationMember unit, EliminationSlot slot, int order)
    {
        var m = new Member(unit,slot,order);
        var source = Models.Value.Resolve(rules.Id,unit.Line,unit.Stage);
        if (source is not null)
        {
            m.Model = Model(source);m.Animator = new(m.Model.Rig);
            string key = Models.Value.Variants[rules.Id][unit.Line][unit.Stage]!;
            m.Dancer = new(Dances.Value.Binding(key));
        }
        return m;
    }
    private static double Metadata(GpuModel model,string key,double fallback=0) => model.Source.Metadata.TryGetProperty(key,out var value) ? value.GetDouble() : fallback;
    private void Pose(Member m, double delta, double time)
    {
        m.Companion?.Update(delta);m.Animator?.Update(delta);
        var root = m.Dancer.Apply(DanceChoreography.Evaluate(dance,time),m.Pose);
        if (m.Companion is { } companion) m.Placement = m.Model!.PlaceCompanion(companion,dance:root);
        else
        {
            double scale = m.Model is null ? 1 : Math.Min(1,1.5/Math.Max(m.Model.Footprint,.01));
            var basis = new CompanionTransform(new(),new(0,m.Model is null ? 0 : Metadata(m.Model,"rotY"),0),new(scale,scale,scale));
            m.Placement = (m.Model?.Normalization ?? Matrix4D.Identity) * DancePlacement.Apply(basis,root,scale);
        }
        m.Born ??= Math.Max(time,.06+m.Order*.07);
        m.Placement *= GpuModel.Transform(m.Slot.X,m.Slot.Y,m.Slot.Z,0,EliminationLayout.PopScale(time,m.Born.Value,m.Slot));
    }
    public void Draw(int requestedWidth,int requestedHeight)
    {
        if(requestedWidth<=0||requestedHeight<=0)return;
        Resize(requestedWidth,requestedHeight);
        double time = playback.Time, delta = Math.Min(.1,Math.Max(0,time-lastTime));lastTime=time;DrawCalls=EffectDrawCalls=0;
        double aspect=(double)width/height;
        Span<int> value=stackalloc int[1],viewport=stackalloc int[4];
        OpenGl.GetIntegerv(OpenGl.GL_DRAW_FRAMEBUFFER_BINDING,value);int previousDraw=value[0];
        OpenGl.GetIntegerv(OpenGl.GL_READ_FRAMEBUFFER_BINDING,value);int previousRead=value[0];
        OpenGl.GetIntegerv(OpenGl.GL_VIEWPORT,viewport);
        try
        {
            OpenGl.BindFramebuffer(OpenGl.GL_FRAMEBUFFER,frame);ModelRenderer.Begin(width,height);
            OpenGl.ClearColor(new(0,0,0,0));OpenGl.Clear(GlClearBit.Color|GlClearBit.Depth);
            Matrix4x4 view;
            if(playback.Attack && hero is { } actor)
            {
                actor.Animator?.Update(delta);view=camera.Attack(time,aspect);renderer.FarmEye=camera.Eye;
                moves!.Effects.CameraPosition.Set(camera.Eye.X,camera.Eye.Y,camera.Eye.Z);
                if(time>.55 && !Fired)
                {
                    Fired=true;Special=actor.Animator?.Play("special")??false;
                    var line=rules.Lines.Concat(rules.ExtraLines).Single(l=>l.Id==actor.Unit.Line);
                    moves.Play(line,actor.Unit.Stage,new(0,0,0),[new(0,0,3.1)],Special?actor.Model!.Rig.Animations.First(a=>a.Name=="special").Duration:null,2.5);
                }
                moves.Advance(delta);
                double scale=actor.Model is null?1:Math.Min(2.5,Metadata(actor.Model,"maxFootprint",1.9)/Math.Max(actor.Model.Footprint,.01));
                var basis = new CompanionTransform(new(0,actor.Model is null?0:Metadata(actor.Model,"yOffset")*scale,0),new(0,actor.Model is null?0:Metadata(actor.Model,"rotY"),0),new(scale,scale,scale));
                var body = new CompanionTransform(new(0,0,!Special && time>.65?Math.Sin(Math.Min(1,(time-.65)/.7)*Math.PI)*.45:0),new(!Special?-.18*Math.Sin(Math.Min(1,time/.8)*Math.PI):0,0,0),new(1,1,1));
                actor.Placement=(actor.Model?.Normalization??Matrix4D.Identity)*basis.Matrix*body.Matrix;
                Render(actor,view,false);Render(actor,view,true);EffectDrawCalls=moves.Draw(renderer,view,width,height);DrawCalls+=EffectDrawCalls;
            }
            else
            {
                moves?.Clear();
                foreach(var member in members)Pose(member,delta,playback.DanceTime);
                var legend=members.FirstOrDefault(m=>m.Companion is not null);
                if(legend is not null && time-lastLegendMeasure>.15)
                {
                    lastLegendMeasure=time;var (min,max)=legend.Model!.Mesh.Bounds(legend.Model.Rig,legend.Pose!);legendTop=1;
                    foreach(double x in new[]{min.X,max.X})foreach(double y in new[]{min.Y,max.Y})foreach(double z in new[]{min.Z,max.Z})
                        legendTop=Math.Max(legendTop,x*legend.Placement.M12+y*legend.Placement.M22+z*legend.Placement.M32+legend.Placement.M42-.05);
                }
                view=camera.Lineup(layout,aspect,legendTop,playback.Big,delta);renderer.FarmEye=camera.Eye;
                renderer.EliminationRimIntensity=(float)(3+2.5*Math.Exp(-((time*DanceChoreography.Bpm(dance)/60)%1)*5));
                var group=GpuModel.Transform(0,-.05,0,Math.Sin(time*.7)*.08,1);
                if(silhouette && !playback.Cinematic && members.FirstOrDefault() is { } giant)
                {
                    renderer.SilhouetteOverride=true;
                    // The giant shares the dance but has its own placement outside the fitted lineup.
                    var old=giant.Placement;Pose(giant,0,playback.DanceTime);
                    double pop=EliminationLayout.PopScale(playback.DanceTime,giant.Born!.Value,giant.Slot);
                    Matrix4D.Invert(GpuModel.Transform(giant.Slot.X,giant.Slot.Y,giant.Slot.Z,0,pop),out var unplace);
                    giant.Placement=giant.Placement*unplace*GpuModel.Transform(0,-1.4,-7.5,Math.Sin(time*.3)*.15,4.6);
                    Render(giant,view,false);Render(giant,view,true);giant.Placement=old;renderer.SilhouetteOverride=false;
                }
                foreach(var member in members){member.Placement*=group;Render(member,view,false);}
                renderer.BeginTransparency();
                foreach(var member in members)
                {
                    Render(member,view,true);
                    var slot=member.Slot;
                    var shadowAt=Matrix4D.Compose(new(slot.X,.005,slot.Z),new(0,0,0,1),new(slot.Scale*1.35,1,slot.Scale*.9))*group;
                    renderer.DrawRoom(shadowPlane,shadowAt,view,new(1,1,1,1),shadow,true,true,false,side:2,fog:false);DrawCalls++;
                    if(member.Unit.FinalHit){renderer.DrawRoom(honorRing,GpuModel.Transform(slot.X,.015,slot.Z,0,1)*group,view,new(1,.7913f,.3515f,.75f),null,true,true,false,side:2,fog:false);DrawCalls++;}
                }
                renderer.FlushTransparency();
            }
            ModelRenderer.End();
            straightAlpha.Resolve(color.Id,width,height);
        }
        finally
        {
            renderer.ClearTransparency();renderer.SilhouetteOverride=false;
            OpenGl.BindFramebuffer(OpenGl.GL_DRAW_FRAMEBUFFER,new((uint)previousDraw));OpenGl.BindFramebuffer(OpenGl.GL_READ_FRAMEBUFFER,new((uint)previousRead));
            OpenGl.Viewport(viewport[0],viewport[1],viewport[2],viewport[3]);
        }
    }
    private void Render(Member member,Matrix4x4 view,bool transparent)
    {
        if(member.Model is not null){renderer.Draw(member.Model,member.Pose!,member.Placement,view,transparent);DrawCalls+=renderer.DrawCalls;}
        else if(!transparent)DrawCalls+=placeholder.Draw(renderer,view,member.Placement,rules.Id,member.Unit.Line,renderer.SilhouetteOverride);
    }
    private void Resize(int w,int h)
    {
        if(width==w&&height==h)return;
        OpenGl.DeleteTextures(new[]{color,depth});OpenGl.DeleteFramebuffers(new[]{frame});
        width=w;height=h;layout=EliminationLayout.Create(playback.Event.Team.Length>0?playback.Event.Team.Length:members.Count(m=>m.Companion is null),(double)w/h);
        Span<GlFramebuffer> frames=stackalloc GlFramebuffer[1];OpenGl.CreateFramebuffers(frames);frame=frames[0];
        Span<GlTexture> textures=stackalloc GlTexture[2];OpenGl.CreateTextures(OpenGl.GL_TEXTURE_2D,textures);color=textures[0];depth=textures[1];
        OpenGl.TextureStorage2D(color.Id,1,OpenGl.GL_RGBA8,w,h);OpenGl.TextureStorage2D(depth.Id,1,OpenGl.GL_DEPTH_COMPONENT24,w,h);
        OpenGl.TextureParameterFloat(color.Id,OpenGl.GL_TEXTURE_MIN_FILTER,OpenGl.GL_LINEAR);OpenGl.TextureParameterFloat(color.Id,OpenGl.GL_TEXTURE_MAG_FILTER,OpenGl.GL_LINEAR);
        OpenGl.TextureParameterFloat(color.Id,OpenGl.GL_TEXTURE_WRAP_S,OpenGl.GL_CLAMP_TO_EDGE);OpenGl.TextureParameterFloat(color.Id,OpenGl.GL_TEXTURE_WRAP_T,OpenGl.GL_CLAMP_TO_EDGE);
        OpenGl.NamedFramebufferTexture(frame,OpenGl.GL_COLOR_ATTACHMENT0,color.Id,0);OpenGl.NamedFramebufferTexture(frame,OpenGl.GL_DEPTH_ATTACHMENT,depth.Id,0);OpenGl.NamedFramebufferDrawBuffer(frame,OpenGl.GL_COLOR_ATTACHMENT0);
        if(OpenGl.CheckNamedFramebufferStatus(frame,OpenGl.GL_FRAMEBUFFER)!=OpenGl.GL_FRAMEBUFFER_COMPLETE)throw new InvalidOperationException("Elimination framebuffer is incomplete.");
    }
    private static GpuPrimitive Plane()
    {
        var vertices=new float[4*GltfPrimitive.Stride];
        var positions=new[]{(-.5f,-.5f,0f,0f),(.5f,-.5f,1f,0f),(.5f,.5f,1f,1f),(-.5f,.5f,0f,1f)};
        for(int i=0;i<4;i++){int at=i*GltfPrimitive.Stride;vertices[at]=positions[i].Item1;vertices[at+2]=positions[i].Item2;vertices[at+4]=1;vertices[at+6]=positions[i].Item3;vertices[at+7]=positions[i].Item4;}
        return new(new(vertices,[0,2,1,0,3,2],-1,true,false));
    }
    private static GpuPrimitive Ring()
    {
        var vertices=new float[98*GltfPrimitive.Stride];var indices=new uint[48*6];
        for(int i=0;i<=48;i++)for(int j=0;j<2;j++){int at=(i*2+j)*GltfPrimitive.Stride;double a=i*Math.PI/24,r=j==0?.32:.48;vertices[at]=(float)(Math.Cos(a)*r);vertices[at+2]=(float)(Math.Sin(a)*r);vertices[at+4]=1;if(i<48&&j==0){int k=i*6;uint v=(uint)(i*2);indices[k]=v;indices[k+1]=v+3;indices[k+2]=v+1;indices[k+3]=v;indices[k+4]=v+2;indices[k+5]=v+3;}}
        return new(new(vertices,indices,-1,true,false));
    }
    private static GpuTexture Shadow()
    {
        var pixels=new byte[64*64*4];
        for(int y=0;y<64;y++)for(int x=0;x<64;x++){int at=(y*64+x)*4;double r=Math.Sqrt(Math.Pow(x+.5-32,2)+Math.Pow(y+.5-32,2));pixels[at+3]=(byte)Math.Round(127.5*Math.Clamp((32-r)/29,0,1));}
        return new(new(64,64,pixels),new(MinFilter:9729,WrapS:33071,WrapT:33071),true);
    }
    public void Dispose()
    { moves?.Dispose();foreach(var model in models.Values)model.Dispose();models.Clear();straightAlpha.Dispose();placeholder.Dispose();shadowPlane.Dispose();honorRing.Dispose();shadow.Dispose();renderer.Dispose();OpenGl.DeleteTextures(new[]{color,depth});OpenGl.DeleteFramebuffers(new[]{frame}); }
}

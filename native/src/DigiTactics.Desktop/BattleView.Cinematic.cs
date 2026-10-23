using System.Numerics;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Special cut-in and tracking shot. Reads the shown fight and its model poses only; the
/// simulation, its RNG and lockstep never see it.</summary>
internal sealed partial class BattleView
{
    internal readonly record struct CasterFrame(WorldPosition Min,WorldPosition Max,WorldPosition Head,double HeadSize,double Facing,string HeadSource);
    private readonly SpecialCinematicDirector cinematic=new();
    private readonly Dictionary<GpuModel,(HeadChoice? Head,int Stride)> headRigs=[];
    private SpecialCutInView? cutInView;
    private int cinematicSpeed=1;
    private CasterFrame? framedCaster;
    /// <summary>Settings → Special cut-ins. Off by default so diagnostic battles keep the fixed board camera.</summary>
    public CutInMode CutIns {get;set;}=CutInMode.Off;
    /// <summary>False while the window is minimised, during reduced motion, or when no cinematic may cover the board.</summary>
    public bool CutInsAllowed {get;set;}
    /// <summary>The local player's units ("Mine only").</summary>
    public Func<CombatUnit,bool>? IsMine {get;set;}
    internal SpecialCinematicDirector Cinematic=>cinematic;
    /// <summary>Diagnostic captures only (combat-sequence ...:cutin:&lt;source&gt;): every fighter uses this model source.</summary>
    internal static string? CaptureModelSource {get;set;}
    internal CasterFrame? FramedCaster=>framedCaster;
    internal int CutInDrawCalls=>cutInView?.DrawCalls??0;

    private void OfferCutIn(int id)
    {
        if(!actors.TryGetValue(id,out var actor)||!actor.Unit.Alive)return;
        double clip=actor.Model.Rig.Animations.FirstOrDefault(c=>c.Name=="special")?.Duration??0;
        cinematic.Offer(id,IsMine?.Invoke(actor.Unit)??false,CutIns,CutInsAllowed,clip,cinematicSpeed);
    }
    private void UpdateCinematic(double delta,int speed)
    {
        cinematicSpeed=speed;
        cinematic.Update(Math.Clamp(delta,0,.1),speed);
        if(cinematic.Unit is {} id&&(!Combat.Units[id].Alive||!actors.ContainsKey(id)))cinematic.Abort();
        if(cinematic.Unit is null)board.Camera.Cinematic=null;
    }
    /// <summary>The caster's animated world bounds this frame (root motion, flight offsets and scale included),
    /// its head point and facing.</summary>
    internal CasterFrame? Caster(int id)
    {
        if(!actors.TryGetValue(id,out var actor))return null;
        CombatVisual? found=null;foreach(var view in choreography.Views)if(view.UnitId==id){found=view;break;}
        if(found is not {} visual)return null;
        var model=actor.Model;var pose=actor.Animator.Pose;
        if(!headRigs.TryGetValue(model,out var rig))
        {
            var nodes=model.Rig.Nodes.Select(n=>(n.Name,Depth(model.Rig,n))).ToArray();
            int vertices=model.Mesh.Meshes.Sum(group=>group.Sum(p=>p.VertexCount));
            headRigs[model]=rig=(SpecialCinematic.PickHead(nodes),Math.Max(1,vertices/6000));
        }
        var placement=model.Place(visual,actor.Unit.Stage,actor.Unit.Line.Tier);
        var (lo,hi)=model.Mesh.Bounds(model.Rig,pose,precise:true,stride:rig.Stride);
        if(!double.IsFinite(lo.X)||!double.IsFinite(hi.X))return null;
        var min=new WorldPosition(double.PositiveInfinity,double.PositiveInfinity,double.PositiveInfinity);var max=new WorldPosition(double.NegativeInfinity,double.NegativeInfinity,double.NegativeInfinity);
        for(int corner=0;corner<8;corner++)
        {
            var p=Apply(placement,new((corner&1)==0?lo.X:hi.X,(corner&2)==0?lo.Y:hi.Y,(corner&4)==0?lo.Z:hi.Z));
            min=new(Math.Min(min.X,p.X),Math.Min(min.Y,p.Y),Math.Min(min.Z,p.Z));max=new(Math.Max(max.X,p.X),Math.Max(max.Y,p.Y),Math.Max(max.Z,p.Z));
        }
        var estimate=SpecialCinematic.HeadEstimate(min,max,visual.Yaw);
        WorldPosition? head=null;
        if(rig.Head is {} choice)
        {
            WorldPosition Node(int index)=>Apply(placement,new(pose.World[index].M41,pose.World[index].M42,pose.World[index].M43));
            var point=choice.Indices.Select(Node).Aggregate((a,b)=>new(a.X+b.X,a.Y+b.Y,a.Z+b.Z));
            point=new(point.X/choice.Indices.Length,point.Y/choice.Indices.Length,point.Z/choice.Indices.Length);
            int parent=model.Rig.Nodes[choice.Indices[0]].Parent;
            head=SpecialCinematic.HeadFromBone(choice.Kind,point,parent>=0?Node(parent):null,min,max,estimate.Size);
        }
        string source=head is null?"bounds":rig.Head!.Value.Kind;
        // A face on a round or rigid-part body sits lower than the top quarter: aim at the upper third.
        var aim=head??estimate.Point with{Y=max.Y-.3*(max.Y-min.Y)};
        return new(min,max,aim,estimate.Size*SpecialCinematic.HeadScale(source),visual.Yaw,source);
    }
    private static int Depth(GltfRig rig,RigNode node){int depth=0;for(int p=node.Parent;p>=0&&depth<512;p=rig.Nodes[p].Parent)depth++;return depth;}
    private static WorldPosition Apply(Matrix4D m,Vector3D v)=>new(v.X*m.M11+v.Y*m.M21+v.Z*m.M31+m.M41,v.X*m.M12+v.Y*m.M22+v.Z*m.M32+m.M42,v.X*m.M13+v.Y*m.M23+v.Z*m.M33+m.M43);
    private static Vector3 V(WorldPosition p)=>new((float)p.X,(float)p.Y,(float)p.Z);

    /// <summary>Applies this frame's tracking shot to the shared arena camera, before anything projects with it.</summary>
    private void FrameCinematic(int width,int height)
    {
        framedCaster=null;
        if(cinematic.Unit is not {} id){board.Camera.Cinematic=null;return;}
        if(Caster(id) is not {} caster){cinematic.Abort();return;}
        framedCaster=caster;
        // Fit to the visible part of the stretched canvas: its own vertical field of view and aspect.
        double stretch=Math.Max(1,board.Camera.CanvasStretch),aspect=(double)width/height,fov=2*Math.Atan(Math.Tan(19*Math.PI/180)/stretch);
        if(cinematic.Frame(caster.Min,caster.Max,caster.Facing,board.Camera.Yaw,aspect,fov,Combat.Columns>7?18:13,choreography.Views.Where(v=>v.UnitId!=id).Select(v=>v.Position).ToArray()) is {} shot)
        {
            // The arena crops a taller canvas from the top (ArenaCamera.CanvasStretch), so the projection centre sits
            // below the visible centre; lower the shot by that much so the caster is centred in what is shown.
            Vector3 eye=V(shot.Eye),target=V(shot.Target);
            float drop=Vector3.Distance(eye,target)*MathF.Tan(19*MathF.PI/180)*(1-1/(float)Math.Max(1,board.Camera.CanvasStretch));
            board.Camera.Cinematic=(eye-Vector3.UnitY*drop,target-Vector3.UnitY*drop,(float)Math.Clamp(shot.Weight,0,1));
        }
    }

    /// <summary>The cut-in texture for a <paramref name="width"/> x <paramref name="height"/> panel window.</summary>
    internal uint CutInTexture(int width,int height){(cutInView??=new()).Ensure(width,height);return cutInView.Texture;}
    /// <summary>Renders the caster's head view into the cut-in texture (after the arena, before ImGui).</summary>
    internal void DrawCutIn(bool faceLeft,Vector3 rim)
    {
        if(cutInView is null||cinematic.CutIn is null||cinematic.Unit is not {} id||!actors.TryGetValue(id,out var actor)||framedCaster is not {} caster)return;
        if(cinematic.HeadFrame(caster.Head,caster.HeadSize,caster.Facing,faceLeft,caster.HeadSource=="bounds") is not {} pose)return;
        CombatVisual? found=null;foreach(var view in choreography.Views)if(view.UnitId==id){found=view;break;}
        if(found is not {} visual)return;
        cutInView.Draw(actor.Model,actor.Animator.Pose,actor.Model.Place(visual,actor.Unit.Stage,actor.Unit.Line.Tier),V(pose.Eye),V(pose.Target),rim);
    }
}

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
    private readonly Dictionary<GpuModel,(bool[] InHead,bool Named,int Stride)> headRigs=[];
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
            // The head is the chosen joint and everything under it (jaw, ears, horns, eyes).
            int joint=SpecialCinematic.HeadJoint(nodes);var inHead=new bool[nodes.Length];
            if(joint>=0)for(int i=0;i<nodes.Length;i++)for(int p=i;p>=0;p=model.Rig.Nodes[p].Parent)if(p==joint){inHead[i]=true;break;}
            headRigs[model]=rig=(inHead,joint>=0,Math.Max(1,vertices/6000));
        }
        var placement=model.Place(visual,actor.Unit.Stage,actor.Unit.Line.Tier);
        SampleVertices(model,pose,placement,rig.InHead,rig.Stride);
        if(samples.Count==0)return null;
        var min=new WorldPosition(double.PositiveInfinity,double.PositiveInfinity,double.PositiveInfinity);var max=new WorldPosition(double.NegativeInfinity,double.NegativeInfinity,double.NegativeInfinity);
        foreach(var p in samples){min=new(Math.Min(min.X,p.X),Math.Min(min.Y,p.Y),Math.Min(min.Z,p.Z));max=new(Math.Max(max.X,p.X),Math.Max(max.Y,p.Y),Math.Max(max.Z,p.Z));}
        if(!double.IsFinite(min.X)||!double.IsFinite(max.X))return null;
        // Face from the head joint's own vertices; without one, from the top-front of the body; else a bounds guess.
        string source="head";
        var face=rig.Named?SpecialCinematic.FaceFrom(headSamples,visual.Yaw):null;
        if(face is null){source="top-front";face=SpecialCinematic.FaceFrom(SpecialCinematic.TopFront(samples,min,max,visual.Yaw),visual.Yaw);}
        if(face is null){source="bounds";var guess=SpecialCinematic.HeadEstimate(min,max,visual.Yaw);face=guess with{Point=guess.Point with{Y=max.Y-.3*(max.Y-min.Y)}};}
        return new(min,max,face.Value.Point,face.Value.Size*SpecialCinematic.HeadScale(source),visual.Yaw,source);
    }
    private readonly List<WorldPosition> samples=[],headSamples=[];
    private Matrix4D[] skinPalette=[];
    /// <summary>World positions of every stride-th vertex in its current skinned pose, split into the head joint's
    /// vertices (by the joint with the largest weight, or the node of a rigid part) and the rest.</summary>
    private void SampleVertices(GpuModel model,RigPose pose,Matrix4D placement,bool[] inHead,int stride)
    {
        samples.Clear();headSamples.Clear();
        var rig=model.Rig;var mesh=model.Mesh;
        foreach(var instance in mesh.Instances)
        {
            int[]? joints=null;
            if(instance.Skin>=0)
            {
                joints=rig.Skins[instance.Skin].Joints;
                if(skinPalette.Length<joints.Length)skinPalette=new Matrix4D[joints.Length];
                pose.SkinMatrices(instance.Skin,instance.Node,skinPalette.AsSpan(0,joints.Length));
            }
            var world=pose.World[instance.Node]*placement;
            foreach(var primitive in mesh.Meshes[instance.Mesh])
            {
                bool skinned=joints is not null&&primitive.HasSkin;
                for(int vertex=0;vertex<primitive.VertexCount;vertex+=stride)
                {
                    var values=primitive.Vertices.AsSpan(vertex*GltfPrimitive.Stride,GltfPrimitive.Stride);
                    var point=new Vector3D(values[0],values[1],values[2]);int owner=instance.Node;
                    if(skinned)
                    {
                        double x=0,y=0,z=0,heaviest=-1;
                        for(int c=0;c<4;c++)
                        {
                            int slot=checked((int)values[8+c]);double weight=values[12+c];if(weight<=0)continue;
                            var p=Apply(skinPalette[slot],point);x+=p.X*weight;y+=p.Y*weight;z+=p.Z*weight;
                            if(weight>heaviest){heaviest=weight;owner=joints![slot];}
                        }
                        point=new(x,y,z);
                    }
                    var at=Apply(world,point);samples.Add(at);
                    if(inHead[owner])headSamples.Add(at);
                }
            }
        }
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
        if(cinematic.HeadFrame(caster.Head,caster.HeadSize,caster.Facing,faceLeft,caster.HeadSource!="head") is not {} pose)return;
        CombatVisual? found=null;foreach(var view in choreography.Views)if(view.UnitId==id){found=view;break;}
        if(found is not {} visual)return;
        cutInView.Draw(actor.Model,actor.Animator.Pose,actor.Model.Place(visual,actor.Unit.Stage,actor.Unit.Line.Tier),V(pose.Eye),V(pose.Target),rim,(float)(Math.Sqrt(Math.Pow(pose.Eye.X-caster.Head.X,2)+Math.Pow(pose.Eye.Y-caster.Head.Y,2)+Math.Pow(pose.Eye.Z-caster.Head.Z,2))-1.25*caster.HeadSize));
    }
}

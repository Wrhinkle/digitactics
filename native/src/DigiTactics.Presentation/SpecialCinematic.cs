namespace DigiTactics.Presentation;

/// <summary>Special cut-in setting: every special, only the local player's units, or none.</summary>
public enum CutInMode { All, Mine, Off }
public readonly record struct CutInCurve(double Offset, double Push, double Dim, double Time);
public readonly record struct CameraPose(WorldPosition Eye, WorldPosition Target);
public readonly record struct CinematicCamera(WorldPosition Eye, WorldPosition Target, double Weight);
public readonly record struct HeadChoice(string Kind, int[] Indices);
public readonly record struct HeadPoint(WorldPosition Point, double Size);

/// <summary>Who gets a special cut-in, its timeline and the camera framing.
/// Presentation only; it reads combat state and never changes the fight, RNG or lockstep.</summary>
/// <remarks>Phase A (cut-in) slides a slanted panel with a live side view of the caster's head; phase B eases the
/// arena camera onto a three-quarter view that frames the caster's animated bounds every frame, then back.
/// 2x playback runs the timeline 1.35x faster and 4x skips cinematics.</remarks>
public static class SpecialCinematic
{
    public const double CutIn=.62,SlideIn=.12,SlideOut=.12,Dim=.35,CameraLead=.22,Follow=.9,MinTrack=1.2,MaxTrack=2.5,ReturnLimit=1.6,
        GlobalCooldown=6,UnitCooldown=18,WeightOmega=6.5,FrameOmega=9,SpecialMax=1.1,TrackSide=1.25,TrackPitch=.34,
        HeadSide=Math.PI/2-.5,FrontSide=.55,HeadPitch=.1,HeadFov=30*Math.PI/180;
    /// <summary>Head-shot size factor by anchor: face and eye anchors sit on the face itself, and a bounds estimate
    /// is only roughly placed, so both get a looser frame than a skull joint.</summary>
    public static double HeadScale(string source)=>source switch{"face" or "eye"=>1.3,"bounds"=>1.6,_=>1};
    public static CutInMode Mode(string? value)=>value switch{"all"=>CutInMode.All,"off"=>CutInMode.Off,_=>CutInMode.Mine};
    public static string Name(CutInMode mode)=>mode switch{CutInMode.All=>"all",CutInMode.Off=>"off",_=>"mine"};
    /// <summary>Playback speed to timeline rate; 0 skips the cinematic.</summary>
    public static double Pace(int speed)=>speed>=4?0:speed>=2?1.35:1;
    private static double Clamp(double v,double lo,double hi)=>Math.Min(hi,Math.Max(lo,v));

    /// <summary>Exact critically damped step toward a fixed target over dt.</summary>
    public static (double Value,double Velocity) Spring(double x,double v,double target,double omega,double dt)
    {
        double y=x-target,j=v+omega*y,e=Math.Exp(-omega*dt);
        return (target+(y+j*dt)*e,(v-omega*j*dt)*e);
    }
    public static double TrackLength(double clipSeconds)=>Clamp(Math.Min(clipSeconds,SpecialMax)-CutIn+Follow,MinTrack,MaxTrack);
    public static CutInCurve Curve(double t)
    {
        double p=1-Math.Pow(1-Clamp(t/SlideIn,0,1),3);
        double q=t>CutIn-SlideOut?Math.Pow(Clamp((t-(CutIn-SlideOut))/SlideOut,0,1),3):0;
        double push=Clamp((t-SlideIn)/(CutIn-SlideIn-SlideOut),0,1);
        return new(p-1+q,push,Dim*Clamp(t/.1,0,1)*(1-q),t);
    }

    public static (string Kind,int Score)? HeadScore(string? name)
    {
        string n=(name??"").ToLowerInvariant();
        bool Match(string pattern)=>System.Text.RegularExpressions.Regex.IsMatch(n,pattern);
        if(Match("mesh|geo|material|vram|tmd "))return null;
        if(Match("face.?center"))return ("face",100);
        bool skip=Match("end|top|nub|tip|hair|target|hud|look|null");
        if(!skip&&Match("(^|[^a-z])(head|haed)([^a-z]|$)"))return ("head",90);
        if(!skip&&Match("head|haed"))return ("head",80);
        if(!skip&&Match("(^|[^a-z])(kao|atama)([^a-z]|$)"))return ("head",70);
        if(Match("(^|[^a-z])eye")&&!Match("brow|lid|lash|light|target"))return ("eye",60);
        if(!skip&&Match("neck|kubi"))return ("neck",50);
        return null;
    }
    public static HeadChoice? PickHead(IReadOnlyList<(string Name,int Depth)> nodes)
    {
        (string Kind,int Score,int Depth,int Index)? best=null;
        for(int i=0;i<nodes.Count;i++)
        {
            if(HeadScore(nodes[i].Name) is not {} s)continue;
            if(best is not {} b||s.Score>b.Score||s.Kind=="neck"&&s.Score==b.Score&&nodes[i].Depth>b.Depth)best=(s.Kind,s.Score,nodes[i].Depth,i);
        }
        if(best is not {} chosen)return null;
        if(chosen.Kind=="eye")return new("eye",Enumerable.Range(0,nodes.Count).Where(i=>HeadScore(nodes[i].Name)?.Kind=="eye").ToArray());
        return new(chosen.Kind,[chosen.Index]);
    }
    public static HeadPoint HeadEstimate(WorldPosition min,WorldPosition max,double facing)
    {
        double ex=max.X-min.X,ey=max.Y-min.Y,ez=max.Z-min.Z,fx=Math.Sin(facing),fz=Math.Cos(facing);
        double along=.5*(Math.Abs(fx)*ex+Math.Abs(fz)*ez);
        double size=Clamp(.2*Math.Max(ey,Math.Max(ex,ez)*.5),.18,2);
        return new(new((min.X+max.X)/2+fx*along*.55,max.Y-.6*size,(min.Z+max.Z)/2+fz*along*.55),size);
    }
    public static WorldPosition? HeadFromBone(string kind,WorldPosition point,WorldPosition? parent,WorldPosition min,WorldPosition max,double size)
    {
        var p=point;
        if(kind=="head")p=point with{Y=point.Y+.35*size};
        else if(kind=="neck")
        {
            var d=parent is {} q?new WorldPosition(point.X-q.X,point.Y-q.Y,point.Z-q.Z):new(0,1,0);
            double l=Math.Sqrt(d.X*d.X+d.Y*d.Y+d.Z*d.Z);if(l==0)l=1;
            p=new(point.X+d.X/l*.8*size,point.Y+d.Y/l*.8*size,point.Z+d.Z/l*.8*size);
        }
        static bool Inside(double v,double a,double b)=>v>=a-.25*(b-a)&&v<=b+.25*(b-a);
        if(!Inside(p.X,min.X,max.X)||!Inside(p.Y,min.Y,max.Y)||!Inside(p.Z,min.Z,max.Z)||p.Y<min.Y+.2*(max.Y-min.Y))return null;
        return p;
    }
    /// <summary>Units standing between the caster (center) and a camera on this azimuth, within the shot's near range.</summary>
    public static int BlockedBy(WorldPosition center,double azimuth,IEnumerable<WorldPosition>? blockers)
    {
        double dx=Math.Sin(azimuth),dz=Math.Cos(azimuth);int n=0;
        foreach(var b in blockers??[])
        {
            double x=b.X-center.X,z=b.Z-center.Z,t=x*dx+z*dz;
            if(t>.4&&t<7&&Math.Abs(x*dz-z*dx)<1.1)n++;
        }
        return n;
    }
    /// <summary>Mostly a side view, on the side nearer the player's camera, unless other units would stand in front of
    /// the caster on that side.</summary>
    public static double TrackAzimuth(double facing,double cameraYaw,WorldPosition? center=null,IEnumerable<WorldPosition>? blockers=null)
    {
        double a=facing+TrackSide,b=facing-TrackSide;
        double Score(double az)=>Math.Cos(az-cameraYaw)-2*(center is {} c?BlockedBy(c,az,blockers):0);
        return Score(a)>=Score(b)?a:b;
    }
    public static CameraPose TrackPose(WorldPosition min,WorldPosition max,double azimuth,double aspect,double fovY,double reach)
    {
        var c=new WorldPosition((min.X+max.X)/2,(min.Y+max.Y)/2,(min.Z+max.Z)/2);
        double ex=max.X-min.X,ey=max.Y-min.Y,ez=max.Z-min.Z;
        double r=Math.Max(.5*Math.Sqrt(ex*ex+ey*ey+ez*ez),.55);
        double half=Math.Min(fovY/2,Math.Atan(Math.Tan(fovY/2)*aspect));
        double d=Clamp(r*1.3/Math.Sin(half),3.2,16),pitch=TrackPitch;
        WorldPosition eye;
        for(int i=0;;i++)
        {
            eye=new(c.X+d*Math.Cos(pitch)*Math.Sin(azimuth),c.Y+d*Math.Sin(pitch),c.Z+d*Math.Cos(pitch)*Math.Cos(azimuth));
            if(Math.Sqrt(eye.X*eye.X+eye.Z*eye.Z)<=reach||pitch>=1.1||i>=8)break;
            pitch+=.1;
        }
        return new(eye with{Y=Math.Max(eye.Y,1.2)},c);
    }
    /// <param name="front">A front three-quarter view instead of the side view: used when the head was estimated from
    /// the bounds, so a round or rigid-part body still shows its face.</param>
    public static CameraPose HeadPose(WorldPosition head,double size,double facing,bool faceLeft,double push,bool front=false)
    {
        double turn=front?FrontSide:HeadSide,az=facing+(faceLeft?turn:-turn),fx=Math.Sin(facing),fz=Math.Cos(facing);
        var target=new WorldPosition(head.X+fx*size*.12,head.Y-.1*size,head.Z+fz*size*.12);
        double d=1.2*size/Math.Tan(HeadFov/2)*(1-.12*push);
        return new(new(target.X+d*Math.Cos(HeadPitch)*Math.Sin(az),target.Y+d*Math.Sin(HeadPitch),target.Z+d*Math.Cos(HeadPitch)*Math.Cos(az)),target);
    }
}

/// <summary>One director per shown fight. Wall-clock seconds in, camera blend
/// and cut-in curve out.</summary>
public sealed class SpecialCinematicDirector
{
    private sealed class Spring3(WorldPosition p)
    {
        public WorldPosition P=p,V;
        public void Step(WorldPosition goal,double omega,double dt)
        {
            var x=SpecialCinematic.Spring(P.X,V.X,goal.X,omega,dt);var y=SpecialCinematic.Spring(P.Y,V.Y,goal.Y,omega,dt);var z=SpecialCinematic.Spring(P.Z,V.Z,goal.Z,omega,dt);
            P=new(x.Value,y.Value,z.Value);V=new(x.Velocity,y.Velocity,z.Velocity);
        }
    }
    private sealed class Active(int unit,bool mine,double pace,double track)
    {
        public readonly int Unit=unit;public readonly bool Mine=mine;public readonly double Pace=pace,Track=track;
        public double T,Step,Weight,Velocity,ReturnT;public bool Returning,Aborted;public double? Azimuth;
        public Spring3? Eye,Target;
    }
    private readonly Dictionary<int,double> lastStart=[];
    private Active? active;
    private double clock,lastEnd=-1e9;
    public int? Unit=>active?.Unit;
    public double Weight=>active?.Weight??0;
    public CutInCurve? CutIn=>active is {Aborted:false} a&&a.T<SpecialCinematic.CutIn?SpecialCinematic.Curve(a.T):null;

    public bool Offer(int unit,bool mine,CutInMode mode,bool allowed,double clipSeconds,int speed)
    {
        if(!allowed||mode==CutInMode.Off||mode==CutInMode.Mine&&!mine)return false;
        double pace=SpecialCinematic.Pace(speed);
        if(pace==0||clock-lastStart.GetValueOrDefault(unit,-1e9)<SpecialCinematic.UnitCooldown)return false;
        if(active is {} a&&!(a.T==0&&!a.Mine&&mine))return false;
        if(active is null&&clock-lastEnd<SpecialCinematic.GlobalCooldown)return false;
        active=new(unit,mine,pace,SpecialCinematic.TrackLength(clipSeconds));lastStart[unit]=clock;
        return true;
    }
    public void Update(double delta,int speed)
    {
        clock+=delta;
        if(active is not {} a)return;
        if(SpecialCinematic.Pace(speed)==0)Abort();
        a.Step=delta*a.Pace;a.T+=a.Step;
        if(!a.Returning&&a.T>=SpecialCinematic.CutIn+a.Track)a.Returning=true;
        if(a.Returning)a.ReturnT+=a.Step;
        double goal=!a.Returning&&a.T>=SpecialCinematic.CameraLead?1:0;
        (a.Weight,a.Velocity)=SpecialCinematic.Spring(a.Weight,a.Velocity,goal,SpecialCinematic.WeightOmega,a.Step);
        if(a.Returning&&(a.Weight<.004&&a.ReturnT>.2||a.ReturnT>=SpecialCinematic.ReturnLimit)){active=null;lastEnd=clock;}
    }
    public void Abort(){if(active is {} a){a.Returning=true;a.Aborted=true;}}
    public CinematicCamera? Frame(WorldPosition min,WorldPosition max,double facing,double cameraYaw,double aspect,double fovY,double reach,IReadOnlyList<WorldPosition>? blockers=null)
    {
        if(active is not {} a)return null;
        a.Azimuth??=SpecialCinematic.TrackAzimuth(facing,cameraYaw,new((min.X+max.X)/2,0,(min.Z+max.Z)/2),blockers);
        var pose=SpecialCinematic.TrackPose(min,max,a.Azimuth.Value,aspect,fovY,reach);
        if(a.Eye is null||a.Target is null){a.Eye=new(pose.Eye);a.Target=new(pose.Target);}
        else{a.Eye.Step(pose.Eye,SpecialCinematic.FrameOmega,a.Step);a.Target.Step(pose.Target,SpecialCinematic.FrameOmega,a.Step);}
        return new(a.Eye.P,a.Target.P,a.Weight);
    }
    /// <summary>Head camera for the cut-in panel; it follows the head bone exactly (a smoothed close-up would fall
    /// behind a leap or lunge in the special).</summary>
    public CameraPose? HeadFrame(WorldPosition head,double size,double facing,bool faceLeft,bool front=false)
        =>CutIn is {} curve?SpecialCinematic.HeadPose(head,size,facing,faceLeft,curve.Push,front):null;
}

using DigiTactics.Presentation;

/// <summary>Special cut-in policy, timeline and framing (SpecialCinematic, SpecialCinematicDirector).</summary>
internal static class SpecialCinematicChecks
{
    public static int Run(string root)
    {
        int checks=0;
        void Check(bool ok,string message){checks++;if(!ok)throw new InvalidOperationException("Special cinematic: "+message);}
        // Settings and pace.
        Check(SpecialCinematic.Mode("all")==CutInMode.All&&SpecialCinematic.Mode("off")==CutInMode.Off&&SpecialCinematic.Mode("mine")==CutInMode.Mine&&SpecialCinematic.Mode("bogus")==CutInMode.Mine&&SpecialCinematic.Mode(null)==CutInMode.Mine,"setting values");
        foreach(var mode in Enum.GetValues<CutInMode>())Check(SpecialCinematic.Mode(SpecialCinematic.Name(mode))==mode,"setting round trip");
        Check(SpecialCinematic.Pace(1)==1&&SpecialCinematic.Pace(2)==1.35&&SpecialCinematic.Pace(4)==0,"playback pace (4x skips)");
        // Head bones by name.
        (string Name,string? Kind)[] names=[("J_head","head"),("J_head_027","head"),("J_haed","head"),("Bip01 Head","head"),("J_kao","head"),("CharacterFaceCenterTarget_013","face"),
            ("eye_L","eye"),("J_kubiB","neck"),("Neck1","neck"),("Bip01 HeadNub",null),("head_end",null),("HeadTop_End",null),("HUDTarget_014",null),("head_mesh",null),
            ("TMD object #3, VRAM page #5",null),("J_eyelid_L",null),("RootNode",null),("J_tail",null),("",null)];
        foreach(var (name,kind) in names)Check(SpecialCinematic.HeadScore(name)?.Kind==kind,"head name "+name);
        Check(SpecialCinematic.PickHead([("J_root",0),("J_neck_026",1),("J_head_027",2)]) is {Kind:"head"} h&&h.Indices.SequenceEqual([2]),"head beats neck");
        Check(SpecialCinematic.PickHead([("Object_2",0),("J_head",2),("CharacterFaceCenterTarget_013",1)]) is {Kind:"face"} f&&f.Indices.SequenceEqual([2]),"face anchor beats head joint");
        Check(SpecialCinematic.PickHead([("J_root",0),("eye_L",2),("eye_R",2),("J_kuchi",2)]) is {Kind:"eye"} e&&e.Indices.SequenceEqual([1,2]),"eyes are averaged");
        Check(SpecialCinematic.PickHead([("neck",1),("neck_2",3),("neck_1",2)]) is {Kind:"neck"} n&&n.Indices.SequenceEqual([1]),"deepest neck joint");
        Check(SpecialCinematic.PickHead([("TMD object #0, VRAM page #5",0)]) is null,"rigid parts fall back to the bounds");
        // Cut-in curve.
        var start=SpecialCinematic.Curve(0);var held=SpecialCinematic.Curve(.3);var end=SpecialCinematic.Curve(SpecialCinematic.CutIn);
        Check(start.Offset==-1&&start.Dim==0&&held.Offset==0&&held.Dim==SpecialCinematic.Dim&&Math.Abs(end.Offset-1)<1e-12&&end.Dim<1e-12,"panel slides in, holds, slides out");
        for(double t=0;t<SpecialCinematic.CutIn;t+=.01)Check(SpecialCinematic.Curve(t+.01).Offset>=SpecialCinematic.Curve(t).Offset-1e-12&&SpecialCinematic.Curve(t).Push is >=0 and <=1,"panel moves one way");
        // Critically damped spring: no overshoot, converges.
        double x=0,v=0;for(int i=0;i<200;i++){(x,v)=SpecialCinematic.Spring(x,v,1,6.5,1/60.0);Check(x<=1+1e-12,"spring overshoot");}
        Check(Math.Abs(x-1)<1e-4,"spring converges");
        Check(SpecialCinematic.TrackLength(0)==SpecialCinematic.MinTrack&&SpecialCinematic.TrackLength(99)>=SpecialCinematic.MinTrack&&SpecialCinematic.TrackLength(99)<=SpecialCinematic.MaxTrack,"tracking length bounds");
        // Tracking framing keeps the bounds inside the view, above ground and over the island.
        var rng=new Random(1234);
        for(int i=0;i<300;i++)
        {
            double cx=rng.NextDouble()*12-6,cz=rng.NextDouble()*10-5,w=.3+rng.NextDouble()*3,hgt=.3+rng.NextDouble()*3,lift=rng.NextDouble()*1.5;
            var min=new WorldPosition(cx-w/2,lift,cz-w/3);var max=new WorldPosition(cx+w/2,lift+hgt,cz+w/3);
            double facing=rng.NextDouble()*Math.Tau,aspect=.8+rng.NextDouble()*1.2,fov=38*Math.PI/180;
            var pose=SpecialCinematic.TrackPose(min,max,SpecialCinematic.TrackAzimuth(facing,0),aspect,fov,13);
            Check(pose.Eye.Y>=1.2,"eye above ground");
            double dx=pose.Eye.X-pose.Target.X,dy=pose.Eye.Y-pose.Target.Y,dz=pose.Eye.Z-pose.Target.Z,d=Math.Sqrt(dx*dx+dy*dy+dz*dz);
            double r=.5*Math.Sqrt(w*w+hgt*hgt+(2*w/3)*(2*w/3)),half=Math.Min(fov/2,Math.Atan(Math.Tan(fov/2)*aspect));
            Check(d>=Math.Min(16,r/Math.Sin(half))-1e-9,"bounds sphere inside the view");
        }
        var center=new WorldPosition(0,0,0);double side=SpecialCinematic.TrackAzimuth(0,0);
        var blocker=new WorldPosition(2*Math.Sin(side),0,2*Math.Cos(side));
        Check(SpecialCinematic.BlockedBy(center,side,[blocker])==1&&SpecialCinematic.TrackAzimuth(0,0,center,[blocker])!=side,"tracking side avoids a unit in front of the caster");
        // Head point from bones.
        var lo=new WorldPosition(-.5,0,-.4);var hi=new WorldPosition(.5,1.6,.4);var guess=SpecialCinematic.HeadEstimate(lo,hi,0);
        Check(guess.Point.Y>.9&&guess.Point.Z>0&&guess.Size is >.2 and <.5,"bounds head estimate toward the face");
        Check(SpecialCinematic.HeadFromBone("head",new(0,1.3,0),null,lo,hi,guess.Size) is {} up&&up.Y>1.3,"head joint lifted to the skull");
        Check(SpecialCinematic.HeadFromBone("face",new(9,1.3,0),null,lo,hi,guess.Size) is null&&SpecialCinematic.HeadFromBone("face",new(0,.1,0),null,lo,hi,guess.Size) is null,"stray helper nodes rejected");
        var leftPose=SpecialCinematic.HeadPose(guess.Point,guess.Size,0,true,0);var pushed=SpecialCinematic.HeadPose(guess.Point,guess.Size,0,true,1);
        double Dist(CameraPose p)=>Math.Sqrt(Math.Pow(p.Eye.X-p.Target.X,2)+Math.Pow(p.Eye.Y-p.Target.Y,2)+Math.Pow(p.Eye.Z-p.Target.Z,2));
        // faceLeft: facing +Z must project to the camera's left (sin(facing - azimuth) < 0).
        double az=Math.Atan2(leftPose.Eye.X-leftPose.Target.X,leftPose.Eye.Z-leftPose.Target.Z);
        Check(Math.Sin(0-az)<0&&Dist(pushed)<Dist(leftPose),"head shot faces into the panel and pushes in");
        var frontPose=SpecialCinematic.HeadPose(guess.Point,guess.Size,0,true,0,front:true);
        Check(Math.Cos(Math.Atan2(frontPose.Eye.X-frontPose.Target.X,frontPose.Eye.Z-frontPose.Target.Z))>Math.Cos(az)&&SpecialCinematic.HeadScale("bounds")>SpecialCinematic.HeadScale("head"),"estimated heads get a looser front three-quarter shot");
        // Director policy.
        var director=new SpecialCinematicDirector();
        Check(!director.Offer(1,false,CutInMode.Mine,true,1,1)&&!director.Offer(1,true,CutInMode.Off,true,1,1)&&!director.Offer(1,true,CutInMode.All,false,1,1)&&!director.Offer(1,true,CutInMode.All,true,1,4),"mine-only, off, disallowed and 4x skip");
        Check(director.Offer(1,false,CutInMode.All,true,1,1)&&director.Offer(2,true,CutInMode.All,true,1,1)&&director.Unit==2,"the local player's cast takes priority in the same frame");
        Check(!director.Offer(3,true,CutInMode.All,true,1,1),"one cinematic at a time");
        double previous=0,peak=0;bool panelShown=false,panelGone=false;int steps=0;
        while(director.Unit is not null&&steps++<1000)
        {
            director.Update(1/60.0,1);panelShown|=director.CutIn is not null;if(panelShown&&director.CutIn is null)panelGone=true;
            Check(director.CutIn is null||!panelGone,"panel does not return");
            peak=Math.Max(peak,director.Weight);previous=director.Weight;
            director.Frame(new(-.5,0,-.4),new(.5,1.6,.4),0,0,1.6,.66,13);
        }
        Check(panelShown&&peak>.99&&previous<.01&&steps<300,"cut-in, tracking shot and return complete");
        Check(!director.Offer(4,true,CutInMode.All,true,1,1),"global cooldown");
        for(int i=0;i<(int)(SpecialCinematic.GlobalCooldown*60)+2;i++)director.Update(1/60.0,1);
        Check(!director.Offer(2,true,CutInMode.All,true,1,1)&&director.Offer(4,true,CutInMode.All,true,1,1),"per-unit cooldown");
        director.Update(1/60.0,4);Check(director.CutIn is null,"switching to 4x hides the panel and eases back");
        for(int i=0;i<200;i++)director.Update(1/60.0,4);Check(director.Unit is null,"aborted shot ends");
        Console.WriteLine($"Special cinematic: {checks} policy, timeline and framing checks passed.");
        return checks;
    }
}

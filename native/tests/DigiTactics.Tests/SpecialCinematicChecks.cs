using System.Text.Json;
using DigiTactics.Presentation;

/// <summary>SpecialCinematic against js/cutin-core.js (native/tools/export-special-cinematic.cjs).</summary>
internal static class SpecialCinematicChecks
{
    public static int Run(string root)
    {
        int checks=0;
        void Check(bool ok,string message){checks++;if(!ok)throw new InvalidOperationException("Special cinematic: "+message);}
        static bool Near(double a,double b)=>Math.Abs(a-b)<=1e-9*Math.Max(1,Math.Abs(b));
        static WorldPosition P(JsonElement e)=>new(e.GetProperty("x").GetDouble(),e.GetProperty("y").GetDouble(),e.GetProperty("z").GetDouble());
        bool Same(WorldPosition a,JsonElement e)=>Near(a.X,e.GetProperty("x").GetDouble())&&Near(a.Y,e.GetProperty("y").GetDouble())&&Near(a.Z,e.GetProperty("z").GetDouble());
        bool SamePose(CameraPose? a,JsonElement e)=>e.ValueKind==JsonValueKind.Null?a is null:a is {} p&&Same(p.Eye,e.GetProperty("eye"))&&Same(p.Target,e.GetProperty("target"));
        using var fixture=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"tests/fixtures/special-cinematic.json")));
        var r=fixture.RootElement;var cut=r.GetProperty("cut");
        foreach(var (name,value) in new (string,double)[]{("cutIn",SpecialCinematic.CutIn),("slideIn",SpecialCinematic.SlideIn),("slideOut",SpecialCinematic.SlideOut),("dim",SpecialCinematic.Dim),
            ("cameraLead",SpecialCinematic.CameraLead),("follow",SpecialCinematic.Follow),("minTrack",SpecialCinematic.MinTrack),("maxTrack",SpecialCinematic.MaxTrack),("returnLimit",SpecialCinematic.ReturnLimit),
            ("globalCooldown",SpecialCinematic.GlobalCooldown),("unitCooldown",SpecialCinematic.UnitCooldown),("weightOmega",SpecialCinematic.WeightOmega),("frameOmega",SpecialCinematic.FrameOmega),
            ("specialMax",SpecialCinematic.SpecialMax),("trackSide",SpecialCinematic.TrackSide),("trackPitch",SpecialCinematic.TrackPitch),("headSide",SpecialCinematic.HeadSide),("headPitch",SpecialCinematic.HeadPitch),("headFov",SpecialCinematic.HeadFov)})
            Check(Near(value,cut.GetProperty(name).GetDouble()),"constant "+name);
        foreach(var m in r.GetProperty("modes").EnumerateArray())
            Check(SpecialCinematic.Name(SpecialCinematic.Mode(m.GetProperty("value").ValueKind==JsonValueKind.String?m.GetProperty("value").GetString():null))==m.GetProperty("mode").GetString(),"setting value");
        foreach(var p in r.GetProperty("paces").EnumerateArray())Check(SpecialCinematic.Pace(p.GetProperty("speed").GetInt32())==p.GetProperty("pace").GetDouble(),"playback pace");
        foreach(var s in r.GetProperty("scores").EnumerateArray())
        {
            var actual=SpecialCinematic.HeadScore(s.GetProperty("name").GetString());var expected=s.GetProperty("result");
            Check(expected.ValueKind==JsonValueKind.Null?actual is null:actual is {} a&&a.Kind==expected.GetProperty("kind").GetString()&&a.Score==expected.GetProperty("score").GetInt32(),"head name "+s.GetProperty("name").GetString());
        }
        foreach(var p in r.GetProperty("picks").EnumerateArray())
        {
            var nodes=p.GetProperty("nodes").EnumerateArray().Select(n=>(n.GetProperty("name").GetString()!,n.GetProperty("depth").GetInt32())).ToArray();
            var actual=SpecialCinematic.PickHead(nodes);var expected=p.GetProperty("result");
            Check(expected.ValueKind==JsonValueKind.Null?actual is null:actual is {} a&&a.Kind==expected.GetProperty("kind").GetString()&&a.Indices.SequenceEqual(expected.GetProperty("indices").EnumerateArray().Select(i=>i.GetInt32())),"head bone choice");
        }
        foreach(var c in r.GetProperty("curves").EnumerateArray())
        {
            var curve=SpecialCinematic.Curve(c.GetProperty("t").GetDouble());
            Check(Near(curve.Offset,c.GetProperty("offset").GetDouble())&&Near(curve.Push,c.GetProperty("push").GetDouble())&&Near(curve.Dim,c.GetProperty("dim").GetDouble())&&Near(curve.Time,c.GetProperty("time").GetDouble()),"cut-in curve");
        }
        foreach(var t in r.GetProperty("tracks").EnumerateArray())Check(Near(SpecialCinematic.TrackLength(t.GetProperty("clip").GetDouble()),t.GetProperty("length").GetDouble()),"tracking length");
        foreach(var c in r.GetProperty("poses").EnumerateArray())
        {
            var min=P(c.GetProperty("min"));var max=P(c.GetProperty("max"));double facing=c.GetProperty("facing").GetDouble();
            var center=new WorldPosition((min.X+max.X)/2,0,(min.Z+max.Z)/2);
            var blockers=c.GetProperty("blockers").EnumerateArray().Select(b=>new WorldPosition(b.GetProperty("x").GetDouble(),0,b.GetProperty("z").GetDouble())).ToArray();
            double az=SpecialCinematic.TrackAzimuth(facing,c.GetProperty("yaw").GetDouble(),center,blockers);Check(Near(az,c.GetProperty("azimuth").GetDouble()),"tracking side");
            Check(Near(SpecialCinematic.TrackAzimuth(facing,c.GetProperty("yaw").GetDouble()),c.GetProperty("plainAzimuth").GetDouble())&&SpecialCinematic.BlockedBy(center,az,blockers)==c.GetProperty("blocked").GetInt32(),"tracking side around other units");
            Check(SamePose(SpecialCinematic.TrackPose(min,max,az,c.GetProperty("aspect").GetDouble(),38*Math.PI/180,c.GetProperty("reach").GetDouble()),c.GetProperty("track")),"tracking framing");
            var estimate=SpecialCinematic.HeadEstimate(min,max,facing);var e=c.GetProperty("estimate");
            Check(Same(estimate.Point,e)&&Near(estimate.Size,e.GetProperty("size").GetDouble()),"head estimate");
            Check(SamePose(SpecialCinematic.HeadPose(estimate.Point,estimate.Size,facing,true,.3),c.GetProperty("left"))&&SamePose(SpecialCinematic.HeadPose(estimate.Point,estimate.Size,facing,false,1),c.GetProperty("right")),"head framing");
            void Bone(string kind,WorldPosition point,WorldPosition? parent,string key)
            {
                var actual=SpecialCinematic.HeadFromBone(kind,point,parent,min,max,estimate.Size);var expected=c.GetProperty(key);
                Check(expected.ValueKind==JsonValueKind.Null?actual is null:actual is {} a&&Same(a,expected),"head bone point "+key);
            }
            Bone("head",new((min.X+max.X)/2,max.Y*.8,(min.Z+max.Z)/2),null,"fromHead");
            Bone("neck",new((min.X+max.X)/2,max.Y*.7,min.Z),new((min.X+max.X)/2,max.Y*.5,min.Z+.1),"fromNeck");
            Bone("face",new(max.X+5,max.Y,max.Z),null,"outside");
        }
        var director=new SpecialCinematicDirector();int index=0,claimed=0,active=0;
        foreach(var step in r.GetProperty("steps").EnumerateArray())
        {
            int i=index++;double dt=step.GetProperty("dt").GetDouble();int speed=step.GetProperty("speed").GetInt32();
            foreach(var o in step.GetProperty("offers").EnumerateArray())
            {
                bool got=director.Offer(o.GetProperty("unit").GetInt32(),o.GetProperty("mine").GetBoolean(),SpecialCinematic.Mode(o.GetProperty("mode").GetString()),o.GetProperty("allowed").GetBoolean(),o.GetProperty("clip").GetDouble(),o.GetProperty("speed").GetInt32());
                Check(got==o.GetProperty("claimed").GetBoolean(),$"offer at step {i}");if(got)claimed++;
            }
            if(i==333&&director.Unit is not null)director.Abort();
            director.Update(dt,speed);
            double t=i*dt;var min=new WorldPosition(Math.Sin(t)-.6,.2*Math.Abs(Math.Sin(t*3)),-1);var max=new WorldPosition(Math.Sin(t)+.6,1.5+.3*Math.Sin(t*2),.2);
            var shot=director.Frame(min,max,.4+i*.001,.2,1.7,38*Math.PI/180,13,[new(1.5,0,.3),new(-.4,0,-2)]);
            var head=director.HeadFrame(new(0,1.2,0),.4,.4,i%2==0);
            var unit=step.GetProperty("unit");
            Check(unit.ValueKind==JsonValueKind.Null?director.Unit is null:director.Unit==unit.GetInt32(),$"active caster at step {i}");if(director.Unit is not null)active++;
            Check(Near(director.Weight,step.GetProperty("weight").GetDouble()),$"camera weight at step {i}");
            var curve=step.GetProperty("cutIn");
            Check(curve.ValueKind==JsonValueKind.Null?director.CutIn is null:director.CutIn is {} c&&Near(c.Offset,curve.GetProperty("offset").GetDouble())&&Near(c.Push,curve.GetProperty("push").GetDouble())&&Near(c.Dim,curve.GetProperty("dim").GetDouble()),$"cut-in panel at step {i}");
            var s=step.GetProperty("shot");
            Check(s.ValueKind==JsonValueKind.Null?shot is null:shot is {} sh&&Same(sh.Eye,s.GetProperty("eye"))&&Same(sh.Target,s.GetProperty("target"))&&Near(sh.Weight,s.GetProperty("weight").GetDouble()),$"tracking camera at step {i}");
            Check(SamePose(head,step.GetProperty("head")),$"head camera at step {i}");
        }
        Check(claimed>=3&&active>100,"scenario covers several cinematics");
        Console.WriteLine($"Special cinematic: {checks} source policy, timeline and framing checks passed ({claimed} cinematics over {index} steps).");
        return checks;
    }
}

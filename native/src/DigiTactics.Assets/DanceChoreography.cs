using System.Numerics;
using static System.Math;

namespace DigiTactics.Assets;

/// <summary>Continuous original dance phrases, evaluated in beats without sampled animation tables.</summary>
public static partial class DanceChoreography
{
    private static Vector3 V(double x, double y, double z) => new((float)x, (float)y, (float)z);
    private static Vector3 Mirror(Vector3 v) => new(-v.X, v.Y, v.Z);
    private static Vector3 Mix(Vector3 a, Vector3 b, double k) => a + (b - a) * (float)k;
    private static double Fr(double b, double n = 1) => ((b / n % 1) + 1) % 1;
    private static double Beat(double b, double sharp = 6) => Exp(-Fr(b) * sharp);
    private static double Hop(double b) => Abs(Sin(PI * b));
    private static double Smooth(double k) => k * k * (3 - 2 * k);
    private static double Snap(double k) => k < .22 ? Smooth(k / .22) : 1;
    private static readonly Dictionary<string, Vector3> Directions = new()
    {
        ["up"] = V(.12,1,.02), ["out"] = V(1,.05,.05), ["upOut"] = V(.7,.7,.08), ["downOut"] = V(.55,-.83,.05), ["down"] = V(.15,-1,.06),
        ["fwd"] = V(.12,0,1), ["fwdUp"] = V(.15,.7,.7), ["fwdDown"] = V(.18,-.6,.78), ["cross"] = V(-.5,-.15,.85),
        ["back"] = V(.3,-.6,-.75), ["hip"] = V(.75,-.55,-.15), ["point"] = V(.55,.8,.25), ["toHip"] = V(-.55,-.55,.3), ["toHead"] = V(-.75,.35,-.45),
        ["lDown"] = V(.06,-1,.02), ["kneeUp"] = V(.08,-.15,1), ["kick"] = V(.08,-.45,.9), ["lBack"] = V(.06,-.8,-.6),
        ["lOut"] = V(.55,-.85,.05), ["lIn"] = V(-.15,-.98,.1), ["shinBack"] = V(.03,-.75,-.66)
    };
    private static Vector3 L(string name) => Directions[name];
    private static Vector3 R(string name) => Mirror(L(name));
    private static readonly Dictionary<string, int> Tempos = new() { ["floss"]=120,["robot"]=100,["running"]=128,["disco"]=110,["macarena"]=103,["windmill"]=126,["gangnam"]=132,["jig"]=140,["shuffle"]=124,["butterfly"]=96,["headbang"]=150 };
    private static bool Mystery(string? id, out int index, out int pool)
    {
        index = pool = 0; if (id is null) return false;
        string[] parts = id.Split('_');
        if (parts.Length != 4 || parts[0] != "mystery" || parts[2] != "dance" || parts[1] is not ("daily" or "vault") || !int.TryParse(parts[3], out int n) || n is < 1 or > 20) return false;
        index = n - 1; pool = parts[1] == "vault" ? 1 : 0; return true;
    }
    public static int Bpm(string? id) => Mystery(id, out int i, out int pool) ? 84 + i * 3 + pool * 7 : id is not null && Tempos.TryGetValue(id, out int bpm) ? bpm : 120;
    public static double Duration(string? id) => Max(3.6, 480d / Bpm(id));
    public static DancePose Evaluate(string? id, double seconds)
    {
        if (!double.IsFinite(seconds) || seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds));
        double b = seconds * Bpm(id) / 60;
        return Mystery(id, out int index, out int pool) ? Unlock(b, index, pool) : id switch
        {
            "robot" => Robot(b), "running" => Running(b), "disco" => Disco(b), "macarena" => Macarena(b), "windmill" => Windmill(b),
            "gangnam" => Gangnam(b), "jig" => Jig(b), "shuffle" => Shuffle(b), "butterfly" => Butterfly(b), "headbang" => Headbang(b), _ => Floss(b)
        };
    }
    private sealed class PoseBuilder
    {
        public readonly DancePose Pose = new([], [], []);
        public void Set(string key, Vector3 value) => Pose.Vectors[key] = value;
        public void Set(string key, double x, double y, double z) => Set(key, V(x,y,z));
        public void Scalar(string key, double value) => Pose.Scalars[key] = (float)value;
        public void Root(string key, double value) => Pose.Root[key] = (float)value;
    }
    private static DancePose Floss(double b)
    {
        double s=Tanh(3*Sin(PI*b))/Tanh(3), f=Tanh(3*Sin(PI*b/2+.6))/Tanh(3); var p=new PoseBuilder();
        p.Set("aL",-.55*s,-.83,.32*f); p.Set("aR",-.55*s,-.83,-.32*f); p.Set("hp",0,0,12*s); p.Set("sp",4,0,-10*s); p.Set("hd",6*Beat(b),0,6*s);
        p.Scalar("tl",25*s);p.Scalar("w",.3*s);p.Root("x",.05*s);p.Root("y",.025*Hop(b));return p.Pose;
    }
    private static DancePose Running(double b)
    {
        double k=Fr(b,2),l=k<.5?Smooth(k*2):1-Smooth((k-.5)*2),r=1-l;var p=new PoseBuilder();
        foreach(var (side,amount) in new[]{("L",l),("R",r)})
        {
            Vector3 D(string name)=>side=="L"?L(name):R(name);
            p.Set("l"+side,Mix(D("lBack"),D("kneeUp"),amount));p.Set("k"+side,Mix(D("shinBack"),D("lDown"),amount));
            p.Set("a"+side,Mix(D("fwdDown"),D("back"),amount));p.Set("f"+side,Mix(D("fwdUp"),D("fwd"),amount));
        }
        p.Set("sp",10,8*(l-r),0);p.Set("hd",5*Beat(b),0,0);p.Scalar("tl",20*(l-r));p.Root("y",.05*Hop(b));p.Root("z",.03*Sin(PI*b));return p.Pose;
    }
    private static DancePose Disco(double b)
    {
        int side=Floor(b/8)%2!=0?-1:1;double k=Fr(b,4),up=k<.5?Snap(k*2):1-Snap((k-.5)*2),s=Sin(PI*b);var p=new PoseBuilder();
        var point=Mix(V(.45,-.75,.45),L("point"),up);
        p.Set(side>0?"aR":"aL",side>0?Mirror(point):point);p.Set(side>0?"aL":"aR",side>0?L("hip"):R("hip"));p.Set(side>0?"fL":"fR",side>0?L("toHip"):R("toHip"));
        p.Set("hp",0,0,10*s);p.Set("sp",0,-12*side*up,-6*side*up);p.Set("hd",18-32*up,-20*side*up+10*side*(1-up),0);
        p.Scalar("tl",20*s);p.Root("x",.04*s);p.Root("y",.02*Hop(b));return p.Pose;
    }
    private static DancePose Gangnam(double b)
    {
        double k=Fr(b,2),left=k<.5?Sin(PI*Fr(b*2)):0,right=k>=.5?Sin(PI*Fr(b*2)):0;var p=new PoseBuilder();
        p.Set("lL",Mix(V(.3,-.9,.2),V(.3,-.45,.85),left));p.Set("kL",Mix(V(.1,-1,0),V(.05,-1,.2),left));
        p.Set("lR",Mix(V(-.3,-.9,.2),V(-.3,-.45,.85),right));p.Set("kR",Mix(V(-.1,-1,0),V(-.05,-1,.2),right));
        p.Set("aL",-.15,-.35,.92);p.Set("fL",-.5,0,.85);p.Set("aR",.15,-.35,.92);p.Set("fR",.5,.02,.85);
        p.Set("sp",12,0,0);p.Set("hd",-6+10*Beat(b*2),0,0);p.Scalar("tl",25*Sin(Tau*b));p.Scalar("w",.4*Sin(Tau*b));p.Root("y",.05*Hop(b*2));p.Root("x",.02*Sin(PI*b));
        if(Floor(b/8)%2==1){double a=Fr(b)*Tau;p.Set("aR",-.2,1,.1);p.Set("fR",.75*Cos(a),.45,.75*Sin(a));p.Set("hd",-12,-15,0);}return p.Pose;
    }
    private static DancePose Jig(double b)
    {
        double c=Fr(b,8)*8,kick=Sin(PI*Fr(b));bool left=Floor(b)%2==0,pump=c>=6;var p=new PoseBuilder();
        p.Set("lL",left?Mix(L("lDown"),L("kick"),kick):L("lDown"));p.Set("kL",p.Pose.Vectors["lL"]);
        p.Set("lR",!left?Mix(R("lDown"),R("kick"),kick):R("lDown"));p.Set("kR",p.Pose.Vectors["lR"]);
        p.Set("aL",pump?L("up"):Mix(L("hip"),L("upOut"),left?0:kick*.6));p.Set("fL",pump?L("up"):left?L("toHip"):L("upOut"));
        p.Set("aR",pump?R("up"):Mix(R("hip"),R("upOut"),left?kick*.6:0));p.Set("fR",pump?R("up"):left?R("upOut"):R("toHip"));
        p.Set("sp",-4,0,left?6:-6);p.Set("hd",-8*kick,0,left?8:-8);p.Scalar("tl",left?30:-30);p.Scalar("w",pump?Sin(Tau*b*2):.3);
        p.Root("y",.12*Hop(b));p.Root("ry",c>=6?Smooth((c-6)/2)*Tau:0);return p.Pose;
    }
    private static DancePose Shuffle(double b)
    {
        double s=Sin(PI*b),o=(s+1)/2,lift=Floor(b)%2!=0?0:Sin(PI*Fr(b));var p=new PoseBuilder();
        p.Set("lL",Mix(L("lIn"),L("lOut"),o));p.Set("lR",Mix(R("lIn"),R("lOut"),1-o));p.Set("kL",lift!=0?Mix(L("lDown"),L("shinBack"),lift):L("lDown"));p.Set("kR",R("lDown"));
        p.Set("aR",-.25,.6,.75);p.Set("fR",-.05,.95,.3);p.Set("aL",Mix(L("downOut"),L("out"),Beat(b,4)));p.Set("fL",L("fwdUp"));
        p.Set("sp",4,-10,0);p.Set("hd",-12+6*Beat(b),-15,0);p.Scalar("tl",25*s);p.Scalar("w",.4*s);p.Root("x",.06*s);p.Root("y",.025*Hop(b));p.Root("ry",.25*Sin(PI*b/4));return p.Pose;
    }
    private static DancePose Butterfly(double b)
    {
        double flap=.5+.5*Sin(PI*b),sway=Sin(PI*b/2);var p=new PoseBuilder();
        p.Set("aL",Mix(L("downOut"),L("upOut"),flap));p.Set("fL",Mix(L("out"),V(.6,.8,.05),flap));p.Set("aR",Mirror(p.Pose.Vectors["aL"]));p.Set("fR",Mirror(p.Pose.Vectors["fL"]));
        p.Set("hp",0,0,12*sway);p.Set("sp",0,10*sway,-8*sway);p.Set("hd",-6*flap,0,-14*sway);p.Scalar("tl",25*sway);p.Scalar("w",flap*2-1);
        p.Root("x",.07*sway);p.Root("y",.05*flap);p.Root("ry",.35*sway);return p.Pose;
    }
    private static DancePose Headbang(double b)
    {
        double hit=Beat(b,5),strum=.5+.5*Cos(Tau*b*2);bool horns=Floor(b/8)%2==1&&Fr(b,8)>.5;var p=new PoseBuilder();
        p.Set("hd",-10+42*hit,0,0);p.Set("sp",4+16*hit,0,0);p.Set("aL",.75,-.2,.6);p.Set("fL",.65,.05,.75);
        p.Set("aR",horns?R("up"):V(-.25,-.6,.75));p.Set("fR",horns?R("up"):Mix(V(.65,-.45,.55),V(.6,.15,.7),strum));p.Set("lL",L("lOut"));p.Set("lR",R("lOut"));
        p.Scalar("tl",30*(hit-.5));p.Scalar("w",hit);p.Root("y",-.03*hit+.01);p.Root("z",.02*hit);return p.Pose;
    }
    private static DancePose Unlock(double b,int i,int pool)
    {
        double phase=b*PI*(i%3==0?2:1)+pool*PI/2,wave=Sin(phase),beat=Max(0,Sin(b*PI)),reach=.35+i%5*.1,swing=Sin(phase+i*.31);bool left=Floor(b)%8%2==0;var p=new PoseBuilder();
        p.Set("aL",reach,.25+swing*.5,.45);p.Set("aR",-reach,.25-swing*.5,.45);p.Set("fL",.2,left?.9:.1,.55);p.Set("fR",-.2,left?.1:.9,.55);
        p.Set("lL",.13+wave*.14,-1,.12*beat);p.Set("lR",-.13+wave*.14,-1,-.12*beat);p.Set("hp",0,wave*(4+i%4),wave*7);p.Set("sp",beat*5,0,-wave*5);p.Set("hd",-beat*8,wave*9,0);
        p.Scalar("tl",wave*24);p.Scalar("w",wave*.45);p.Root("x",wave*(.025+i%4*.015));p.Root("y",beat*(.025+i%5*.008));p.Root("ry",i%3==0?Sin(b*PI/4)*.6:wave*.12);p.Root("z",Sin(b*PI/2)*.025);return p.Pose;
    }
}

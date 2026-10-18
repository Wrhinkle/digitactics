using System.Numerics;
using static System.Math;

namespace DigiTactics.Assets;

public static partial class DanceChoreography
{
    private sealed record RobotKey(string ArmLeft, string ForeLeft, string ArmRight, string ForeRight, Vector3 Head, Vector3 Spine, double Turn);
    private static readonly RobotKey[] RobotKeys =
    [
        new("out","up","down","down",V(0,30,0),V(0,10,0),.2),
        new("out","up","out","down",V(0,0,0),V(0,0,0),0),
        new("fwd","fwd","fwd","fwd",V(10,-30,0),V(0,-12,0),-.25),
        new("down","fwd","down","fwd",V(-8,0,0),V(6,0,0),0),
        new("down","down","out","up",V(0,-30,0),V(0,-10,0),-.2),
        new("out","down","out","up",V(0,0,0),V(0,0,0),0),
        new("fwdUp","fwdUp","fwdDown","fwd",V(0,25,12),V(0,12,0),.25),
        new("down","fwd","down","fwd",V(12,0,0),V(0,0,0),0)
    ];
    private static DancePose Robot(double b)
    {
        double pos=Fr(b,8)*8;int index=(int)Floor(pos);double k=Snap(pos-index);var a=RobotKeys[index];var c=RobotKeys[(index+1)%8];var p=new PoseBuilder();
        p.Set("aL",Mix(L(a.ArmLeft),L(c.ArmLeft),k));p.Set("fL",Mix(L(a.ForeLeft),L(c.ForeLeft),k));p.Set("aR",Mix(R(a.ArmRight),R(c.ArmRight),k));p.Set("fR",Mix(R(a.ForeRight),R(c.ForeRight),k));
        p.Set("hd",Mix(a.Head,c.Head,k));p.Set("sp",Mix(a.Spine,c.Spine,k));p.Root("ry",a.Turn+(c.Turn-a.Turn)*k);p.Root("y",-.02*Beat(b,14));p.Scalar("tl",20*Sign(Sin(Fr(b,2)*Tau)));return p.Pose;
    }
    private static readonly (string Right,string Left)[] MacarenaSteps =
    [
        ("fwd","rest"),("fwd","fwd"),("fwd","fwd"),("fwd","fwd"),("shoulder","fwd"),("shoulder","shoulder"),("head","shoulder"),("head","head"),
        ("hipF","head"),("hipF","hipF"),("butt","hipF"),("butt","butt"),("butt","butt"),("butt","butt"),("butt","butt"),("butt","butt")
    ];
    private static (Vector3 Arm,Vector3 Fore) MacarenaArm(string key) => key switch
    {
        "fwd" => (L("fwd"),L("fwd")), "shoulder" => (V(-.45,.05,.9),V(-.7,.55,-.4)), "head" => (L("upOut"),L("toHead")),
        "hipF" => (V(-.25,-.85,.45),V(-.6,-.4,.4)), "butt" => (V(.35,-.75,-.55),V(-.3,-.5,-.6)), _ => (L("down"),L("down"))
    };
    private static DancePose Macarena(double b)
    {
        int n=(int)Floor(Fr(b,16)*16);double k=Snap(Fr(b));var current=MacarenaSteps[n];var previous=MacarenaSteps[(n+15)%16];var p=new PoseBuilder();
        var left=MacarenaArm(current.Left);var oldLeft=MacarenaArm(previous.Left);var right=MacarenaArm(current.Right);var oldRight=MacarenaArm(previous.Right);
        p.Set("aL",Mix(oldLeft.Arm,left.Arm,k));p.Set("fL",Mix(oldLeft.Fore,left.Fore,k));p.Set("aR",Mirror(Mix(oldRight.Arm,right.Arm,k)));p.Set("fR",Mirror(Mix(oldRight.Fore,right.Fore,k)));
        double circle=n>=12&&n<15?Fr(b)*Tau:0,jump=n==15?Sin(PI*Fr(b)):0,turn=Floor(b/16)*(PI/2)+(n==15?Smooth(Fr(b))*(PI/2):0);
        p.Set("hp",circle!=0?V(12*Sin(circle),0,12*Cos(circle)):V(0,0,6*Sin(PI*b)));p.Set("hd",4*Beat(b),0,0);p.Scalar("tl",circle!=0?30*Sin(circle):10*Sin(PI*b));
        p.Root("y",.15*jump+.015*Hop(b));p.Root("ry",turn);p.Root("x",circle!=0?.04*Sin(circle):0);return p.Pose;
    }
    private static DancePose Windmill(double b)
    {
        double c=Fr(b,16)*16;var p=new PoseBuilder();
        if(c<4)
        {
            double s=Sin(PI*c);p.Set("aL",Mix(L("downOut"),L("cross"),(s+1)/2));p.Set("aR",Mix(R("downOut"),R("cross"),(1-s)/2));p.Set("fL",L("fwd"));p.Set("fR",R("fwd"));
            p.Set("lL",L(s>0?"kick":"lDown"));p.Set("lR",R(s<0?"kick":"lDown"));p.Set("kL",L("lDown"));p.Set("kR",R("lDown"));p.Set("sp",10,15*s,0);p.Set("hd",8*Beat(b),0,0);p.Scalar("tl",20*s);p.Root("y",.03*Hop(c));return p.Pose;
        }
        double drop=Smooth(Min(1,(c-4)/1.5)),rise=c>14?Smooth(Min(1,(c-14)/.8)):0,spin=c<5.5?0:(c<14?c-5.5:8.5+Smooth(Min(1,(c-14)/2))*.5)*(Tau/2),tilt=drop*(1-rise*.7),sw=Sin(spin);bool legsUp=c>5.5&&c<14;
        p.Set("aL",L(rise!=0?"up":"out"));p.Set("aR",R("out"));p.Set("lL",legsUp?Mix(L("lOut"),V(.7,.2,.7),.5+.5*sw):L("kneeUp"));p.Set("lR",legsUp?Mix(R("lOut"),V(-.7,.2,.7),.5-.5*sw):R("lDown"));
        if(!legsUp){p.Set("kL",L("lDown"));p.Set("kR",R("lDown"));}
        p.Set("sp",-10*tilt,0,10*sw*tilt);p.Set("hd",-15*tilt,0,0);p.Scalar("tl",30*sw);p.Scalar("w",.6*sw);
        p.Root("rx",-1.15*tilt);p.Root("ry",spin);p.Root("rz",.25*sw*tilt);p.Root("piv",.32);p.Root("y",.03*tilt);return p.Pose;
    }
}

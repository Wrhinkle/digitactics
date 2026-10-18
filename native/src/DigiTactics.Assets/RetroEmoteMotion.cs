using static System.Math;

namespace DigiTactics.Assets;

public sealed record RetroEmotePose(CompanionTransform Model, CompanionTransform Holder, double Phase, double Exit, double BeamHeight, double BeamOpacity);

/// <summary>Original nine retro appearances, in model/holder space beside a moving companion.</summary>
public static class RetroEmoteMotion
{
    public static RetroEmotePose? Evaluate(string action, double seconds, double scale, double modelYaw,
        Vector3D companion, Vector3D camera, Vector3D cameraRight)
    {
        if (!double.IsFinite(seconds) || seconds < 0 || seconds >= 3) return null;
        double off=.95,y=0,yaw=0,rz=0,rx=0,sx=1,sy=1,jitter=0,phase=0,k=Max(0,seconds-.25);
        switch(action)
        {
            case "hug": off=.95-.5*Min(1,k/.6);sy=1+.08*Sin(k*10);sx=1-.05*Sin(k*10);break;
            case "cane": rz=Sin(k*9)*.35;y=Abs(Sin(k*9))*.04;break;
            case "ufo": y=1.05+Sin(k*4)*.08;rz=Sin(k*3)*.12;off=.35;break;
            case "shiver": jitter=Sin(seconds*70)*.035;sy=1+.03*Sin(seconds*40);break;
            case "flex": double b=Max(0,Sin(k*5));sx=1+.18*b;sy=1-.06*b;y=b*.05;break;
            case "float": y=.35+Sin(k*3)*.12;rz=Sin(k*2.2)*.15;break;
            case "swoop": double q=Min(1,k/1.8);off=1.7-3.4*q;y=.45+.75*Abs(2*q-1);yaw=PI/2;break;
            case "salute": rx=-.25*Min(1,Min(k*4,Max(0,(1.8-k)*4)));rz=.08*Min(1,k*4);break;
            case "stomp": phase=k*1.6%1;y=Sin(phase*PI)*.35;sy=phase<.08?.82:1;break;
        }
        double lengthSquared=cameraRight.X*cameraRight.X+cameraRight.Z*cameraRight.Z;
        var side=lengthSquared<1e-6?new Vector3D(1,0,0):new(cameraRight.X/Sqrt(lengthSquared),0,cameraRight.Z/Sqrt(lengthSquared));
        var at=new Vector3D(companion.X+side.X*(off+jitter),companion.Y+y,companion.Z+side.Z*(off+jitter));
        double face=Atan2(camera.X-at.X,camera.Z-at.Z),pin=Min(1,seconds/.25),exit=Max(0,Min(1,(seconds-2.5+.3)/.3));
        double size=Max(.001,(pin<1?Sin(pin*PI*.5)*(1+.25*Sin(pin*PI)):1)*(1-exit));
        return new(new(new(0,0,0),new(rx,modelYaw+yaw,rz),new(scale*sx,scale*sy,scale*sx)),
            new(at,new(0,face-(yaw!=0?0:.5*Sign(off!=0?off:1)),0),new(size,size,size)),phase,exit,y,.45*Min(1,k*3)*(1-exit));
    }
}

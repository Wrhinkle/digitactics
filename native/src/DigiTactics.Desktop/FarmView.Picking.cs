using System.Numerics;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

internal sealed partial class FarmView
{
    public (string Id,int? Pile)? Pick(float x,float y,int width,int height)
    {
        if(x<0||y<0||x>=width||y>=height)return null;
        var (camera,eye)=Camera(width,height);
        if(!Matrix4x4.Invert(camera,out var inverse))return null;
        var far=Vector4.Transform(new Vector4(x/width*2-1,1-y/height*2,1,1),inverse);
        var direction=Vector3.Normalize(new Vector3(far.X,far.Y,far.Z)/far.W-eye);
        double nearest=double.PositiveInfinity; (string Id,int? Pile)? hit=null;
        foreach(var actor in actors.Values)
        {
            var a=actor.Resident;
            var world=Matrix4x4.CreateScale(1,(float)a.Breathe,1)*Matrix4x4.CreateRotationZ((float)(a.Tilt*1.15))*Matrix4x4.CreateTranslation((float)a.X,(float)a.Height,(float)a.Z);
            if(FarmPicking.Cylinder(eye,direction,world,.5,1.3,10) is { } distance && distance<nearest) {nearest=distance;hit=(a.Id,null);}
        }
        foreach(var pile in piles.Values)
        {
            var world=Matrix4x4.CreateScale((float)pile.Scale)*Matrix4x4.CreateRotationY((float)pile.Yaw)*Matrix4x4.CreateTranslation((float)pile.X,(float)pile.Y,(float)pile.Z);
            if(FarmPicking.Cylinder(eye,direction,world,.3,.5,8) is { } distance && distance<nearest) {nearest=distance;hit=(pile.Id,pile.Key);}
        }
        return hit;
    }
}

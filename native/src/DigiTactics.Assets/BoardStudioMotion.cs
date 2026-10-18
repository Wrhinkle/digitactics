using System.Numerics;

namespace DigiTactics.Assets;

public sealed class BoardStudioMotion
{
    private double combat;
    private static readonly Vector3 calm=BoardLighting.Linear("#fff6e0"),red=BoardLighting.Linear("#ff3a4a"),blue=BoardLighting.Linear("#3a7aff"),stick=BoardLighting.Linear("#7affd0");
    public void Advance(double delta,bool active)=>combat+=((active?1:0)-combat)*Math.Min(1,2*delta);
    public Vector3 LightColor(int index)=>Vector3.Lerp(calm,index%2!=0?red:blue,(float)combat);
    public Vector3 StickColor=>Vector3.Lerp(stick,red,(float)combat);
    public Matrix4D Light(int index,bool beam,double time)
    {
        double speed=.35+combat*.6,ox=(index-2.5)*3.4,tx=Math.Sin(time*speed+index*1.7)*6.5,tz=Math.Cos(time*speed*.8+index*2.3)*4.5-.5;
        if(!beam)return new CompanionTransform(new(tx,.16,tz),new(0,0,0),new(1,1,1)).Matrix;
        double dx=ox-tx,dy=13.84,dz=-12-tz,len=Math.Sqrt(dx*dx+dy*dy+dz*dz);
        double qx=dz/len,qz=-dx/len,qw=1+dy/len,n=Math.Sqrt(qx*qx+qz*qz+qw*qw);
        return Matrix4D.Compose(new(ox,14,-12),new(qx/n,0,qz/n,qw/n),new(1,len,1));
    }
    public static Matrix4D Audience(BoardCrowdItem p,double time)=>new CompanionTransform(new(p.X,Height(p,time),p.Z),new(0,p.Ry,0),new(1,1,1)).Matrix;
    private static double Height(BoardCrowdItem p,double time)=>p.By+Math.Max(0,Math.Sin(time*5+p.Ph))*.08;
    public static void Sticks(float[] vertices,BoardCrowdItem[] people,double time)
    {
        for(int i=0;i<people.Length;i++){var p=people[i];int at=i*GltfPrimitive.Stride;vertices[at]=(float)(p.X-Math.Sign(p.X)*.3+Math.Sin(time*4+p.Ph)*.2);vertices[at+1]=(float)(Height(p,time)+.85);vertices[at+2]=(float)p.Z;}
    }
    public static Matrix4D Crane(BoardMotion motion,double time)
    {
        var m=motion.Matrix!;
        return new CompanionTransform(new(m[12],m[13],m[14]),new(0,0,motion.Speed==1?-.2+Math.Sin(time*.45)*.15:0),new(1,1,1)).Matrix
            *new CompanionTransform(new(11.6,0,3.2),new(0,Math.PI*.75+Math.Sin(time*.3)*.6,0),new(1,1,1)).Matrix;
    }
    public static Vector3 Strip(int index,double time)
    {
        double h=(time*.1+(index==0?0:.5))%1,l=index==0?.6:.55,s=.9,p=l<=.5?l*(1+s):l+s-l*s,q=2*l-p;
        double Hue(double t){if(t<0)t++;if(t>1)t--;return t<1d/6?q+(p-q)*6*t:t<.5?p:t<2d/3?q+(p-q)*6*(2d/3-t):q;}
        return new((float)Hue(h+1d/3),(float)Hue(h),(float)Hue(h-1d/3));
    }
}

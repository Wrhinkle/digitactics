namespace DigiTactics.Assets;

public sealed class BoardFreezeMotion
{
    public double Weather {get;private set;}
    public float FogNear=>(float)(26-16*Weather);
    public float FogFar=>(float)(62-22*Weather);
    public float SnowSize=>(float)(.13+.05*Weather);
    public void Advance(double delta,bool combat)=>Weather+=((combat?1:0)-Weather)*Math.Min(1,delta);
    public float Opacity(int index)=>(float)((.9-.75*Weather)*(.85+.15*Math.Sin(index+Weather)));
    public void Snow(float[] vertices,double delta)
    {
        for(int i=0;i<vertices.Length;i+=GltfPrimitive.Stride)
        {
            vertices[i]=(float)(vertices[i]+Weather*9*delta);
            if(vertices[i]>17)vertices[i]-=34;
        }
    }
    public static Matrix4D Snowman(BoardProp p,double time)=>new CompanionTransform(new(p.X,Math.Abs(Math.Sin(time*1.3+p.Phase))*.08,p.Z),new(0,p.Ry,Math.Sin(time*1.3+p.Phase)*.06),new(p.S,p.S,p.S)).Matrix;
    public static void Aurora(float[] vertices,BoardAurora p,double time)
    {
        for(int i=0;i<p.Base.Length;i++)
        {
            double u=p.Base[i][0],v=p.Base[i][1],a=p.A0+(p.A1-p.A0)*u,r=p.R+Math.Sin(u*9+time*.7+p.Ph)*.9;
            int at=i*GltfPrimitive.Stride;vertices[at]=(float)(Math.Cos(a)*r);vertices[at+1]=(float)(p.Y+v*p.H+Math.Sin(u*5+time*.4+p.Ph)*.5);vertices[at+2]=(float)(Math.Sin(a)*r);
        }
    }
}

public sealed record BoardAurora(double R,double A0,double A1,double Y,double H,double Ph,double[][] Base);

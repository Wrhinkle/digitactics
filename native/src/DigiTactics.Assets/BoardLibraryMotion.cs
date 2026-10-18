using System.Numerics;

namespace DigiTactics.Assets;

public sealed class BoardLibraryMotion(int initialEvolution=0)
{
    private int previousEvolution=initialEvolution;
    private double flare,spin;
    private static readonly Vector3 purple=BoardLighting.Linear("#b07aff"),gold=BoardLighting.Linear("#ffd27a"),spark=BoardLighting.Linear("#d8b0ff");
    public double LightFlash {get;private set;}
    public float TintWeight=>(float)(flare*.8);
    public Vector3 CircleColor=>Vector3.Lerp(purple,gold,(float)flare);
    public Vector3 SparkColor=>Vector3.Lerp(spark,gold,(float)flare);
    public float Opacity(double time)=>(float)(.5+.1*Math.Sin(time*2)+flare*.4);
    public void Advance(double delta,int evolution)
    {
        if(evolution>previousEvolution){flare=1;LightFlash=1.2;}
        previousEvolution=evolution;flare=Math.Max(0,flare-delta*.45);spin+=delta*(.05+flare*1.5);LightFlash=Math.Max(0,LightFlash-delta*5);
    }
    public Matrix4D Circle(string kind,double[] matrix)=>new CompanionTransform(new(matrix[12],matrix[13],matrix[14]),new(0,kind=="library-circle"?spin:-spin*.6,0),new(1,1,1)).Matrix;
    public static Matrix4D Instance(string kind,BoardLibraryItem p,double time)
    {
        double x=p.X,y=0,z=p.Z,ry=0,rz=0,sy=1;
        if(kind=="library-pages")
        {
            double a=p.A0+time*p.Sp;x=Math.Cos(a)*p.Rad;z=Math.Sin(a)*p.Rad*.85-1.5;y=p.H+Math.Sin(time*1.3+p.Ph)*.3;
            ry=-a+(p.Side>0?0:Math.PI);rz=p.Side*(.3+.6*Math.Abs(Math.Sin(time*4+p.Ph)));
        }
        else
        {
            y=p.By+Math.Sin(time*1.1+p.Ph)*.2;
            if(kind=="library-flames"){y+=.36;sy=1+.25*Math.Sin(time*15+p.Ph);}
            else if(kind=="library-halos")y+=.38;
            else if(kind!="library-candles")throw new ArgumentException("Unknown library motion.",nameof(kind));
        }
        return new CompanionTransform(new(x,y,z),new(0,ry,rz),new(1,sy,1)).Matrix;
    }
}

public sealed record BoardLibraryItem(double X=0,double Z=0,double By=0,double Ph=0,double A0=0,double Sp=0,double Rad=0,double H=0,int Side=0);

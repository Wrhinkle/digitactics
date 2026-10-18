namespace DigiTactics.Assets;

public sealed class BoardTempleMotion
{
    private readonly BoardTempleItem[] geysers;
    private readonly double[] starts;
    private readonly Func<double> random;
    private double flare,fire,previousTime;
    private int previousEvolution;
    public Matrix4D[] Geysers { get; }
    public double LightFlash { get; private set; }
    public float SunMultiplier=>(float)(1+flare*1.4);
    public float TintWeight=>(float)flare;
    public float EmberSize=>(float)(.16+flare*.2);
    public float LavaColor { get; private set; }=.65f;
    public BoardTempleMotion(BoardTempleItem[] geysers,int initialEvolution=0,Func<double>? random=null)
    {
        this.geysers=geysers;this.random=random??Random.Shared.NextDouble;previousEvolution=initialEvolution;
        starts=geysers.Select(g=>g.T0).ToArray();Geysers=new Matrix4D[geysers.Length];Advance(0,0,false,1,initialEvolution);
    }
    public void Advance(double delta,double time,bool combat,int stage,int evolution)
    {
        fire=flare;
        if(evolution>previousEvolution)
        {
            flare=1;LightFlash=2.2;
            for(int i=0;i<starts.Length;i++)starts[i]=previousTime+random()*.3;
        }
        previousEvolution=evolution;previousTime=time;
        for(int i=0;i<geysers.Length;i++)
        {
            var g=geysers[i];if(time-starts[i]>3.4 && Math.Sin(time*.9+g.Ph*3)>.995)starts[i]=time;
            double k=time-starts[i],h=k<.3?k/.3:k<1.6?1:Math.Max(0,1-(k-1.6)/.6);
            Geysers[i]=new CompanionTransform(new(g.X,g.Y,g.Z),new(0,time*3,0),new(.6+h*.6,.01+h*7,.6+h*.6)).Matrix;
        }
        flare=Math.Max(0,flare-delta*.6);LightFlash=Math.Max(0,LightFlash-delta*5);
        double heat=Math.Min(1,(stage-1)/6d)*.35+(combat?.2:0)+.65+.06*Math.Sin(time*2);
        LavaColor=(float)Math.Min(1.3,heat+flare*.5);
    }
    public Matrix4D Flame(BoardTempleItem f,bool inner,double time)
    {
        double k=(1+.25*Math.Sin(time*11+f.Ph)+.15*Math.Sin(time*17+f.Ph*2))*(1+fire*.8);
        double sy=inner?(1+.3*Math.Sin(time*13+f.Ph))*(1+fire):k*f.H;
        return new CompanionTransform(new(f.Bx,1.6,f.Bz),new(0,time*2+f.Ph,0),new(1/Math.Sqrt(k),sy,1/Math.Sqrt(k))).Matrix;
    }
    public static void Scroll(float[] vertices,float[] initial,double time)
    {
        for(int at=0;at<vertices.Length;at+=GltfPrimitive.Stride)
        {vertices[at+6]=(float)(initial[at+6]+time*.01%1);vertices[at+7]=(float)(initial[at+7]+time*.006%1);}
    }
}

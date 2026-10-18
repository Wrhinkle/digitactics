namespace DigiTactics.Assets;

public sealed class BoardTrainMotion(bool initialCombat=false)
{
    private double start=-.6,previousTime;
    private bool previousCombat=initialCombat;
    public double X {get;private set;}=-35.2;
    public float SteamOpacity=>Math.Abs(X)<40?.3f:0;
    public void Advance(double time,bool combat)
    {
        if(combat&&!previousCombat && previousTime-start>7)start=previousTime;
        previousCombat=combat;previousTime=time;
        if(time-start>16)start=time;
        X=-46+(time-start)*18;
    }
    public Matrix4D Instance(string kind,int index,double[] initial)=>Matrix4D.From(initial) with {M41=kind=="train-cars"?X-4.2-index*3.9:X};
    public Matrix4D Steam=>new CompanionTransform(new(X+.9,1.6,-14.2),new(0,0,0),new(1,1,1)).Matrix;
    public static Matrix4D Clock(double[] initial,double time)=>new CompanionTransform(new(initial[12],initial[13],initial[14]),new(0,0,-time*.5),new(1,1,1)).Matrix;
}

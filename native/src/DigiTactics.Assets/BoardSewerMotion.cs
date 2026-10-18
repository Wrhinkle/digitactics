namespace DigiTactics.Assets;

public sealed class BoardSewerMotion(bool initialWon=false)
{
    private bool previousWon=initialWon;
    private double previousTime,until=-1;
    public void Advance(double time,bool won)
    {
        if(won && !previousWon)until=previousTime+4;
        previousWon=won;previousTime=time;
    }
    public Matrix4D Instance(string kind,BoardProp p,double time)
    {
        double x=p.X,y=p.Y,z=p.Z,ry=p.Ry,rz=0,sx=p.S,sy=p.S,sz=p.S;
        if(kind=="sewer-numemon")
        {
            x=p.Cx+Math.Sin(time*.4+p.Phase)*.4;
            sy=1+Math.Sin(time*3+p.Phase)*.08;sx=sz=1/Math.Sqrt(sy);
            y=time<until?Math.Abs(Math.Sin(time*6+p.Phase))*1.1:0;
        }
        else if(kind=="sewer-duck")
        {
            double a=time*.06+3.6;x=Math.Cos(a)*14;z=Math.Sin(a)*9-2;
            y=-.85+Math.Sin(time*1.8)*.06;ry=-a;rz=Math.Sin(time*1.3)*.1;
        }
        else throw new ArgumentException("Unknown sewer motion.",nameof(kind));
        return new CompanionTransform(new(x,y,z),new(0,ry,rz),new(sx,sy,sz)).Matrix;
    }
}

namespace DigiTactics.Assets;

public sealed class BoardGraveyardMotion
{
    private double haste,ghostHaste;
    public float FogNear=>(float)(22-13*haste);
    public float FogFar=>(float)(56-18*haste);
    public void Advance(double delta,bool combat)
    {
        // Source ghost callbacks run before the weather callback updates haste.
        ghostHaste=haste;haste+=((combat?1:0)-haste)*Math.Min(1,delta);
    }
    public Matrix4D Ghost(BoardGhostItem p,double time)
    {
        double a=p.A0+time*p.Sp*(1+ghostHaste*2);
        return new CompanionTransform(new(Math.Cos(a)*p.Rad,p.H+Math.Sin(time*1.6+p.Ph)*.35,Math.Sin(a)*p.Rad*.9-1),new(0,-a+Math.PI,Math.Sin(time*2+p.Ph)*.1),new(1,1,1)).Matrix;
    }
}

public sealed record BoardGhostItem(double A0,double Rad,double H,double Ph,double Sp);

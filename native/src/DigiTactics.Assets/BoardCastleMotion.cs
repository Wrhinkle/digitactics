using System.Numerics;

namespace DigiTactics.Assets;

public sealed class BoardCastleMotion
{
    private double red,swarm,until=-1,previousTime;
    private bool previousCombat;
    public BoardCastleMotion(bool initialCombat=false) { previousCombat=initialCombat; }
    public Vector3 MoonColor=>new(1,(float)(.42+(.08-.42)*red),(float)(.3+(.06-.3)*red));
    public float HaloOpacity(double time)=>(float)(.18+.12*red+.03*Math.Sin(time*1.3));
    public void Advance(double delta,double time,bool combat,int stage)
    {
        if(!previousCombat && combat)until=previousTime+5;
        previousCombat=combat;previousTime=time;
        red+=(Math.Min(1,(stage-1)/6d)-red)*Math.Min(1,delta);
        swarm+=((time<until?1:0)-swarm)*Math.Min(1,1.5*delta);
    }
    public Matrix4D Instance(string kind,BoardCastleItem p,int index,double time)
    {
        if(kind=="castle-flames")
        {
            double k=1+.25*Math.Sin(time*14+p.Ph);
            return new CompanionTransform(new(p.X,p.Y,p.Z),new(0,0,0),new(1/k,k,1/k)).Matrix;
        }
        if(kind!="castle-bats")throw new ArgumentException("Unknown castle animation.",nameof(kind));
        double angle=p.A0+time*p.Sp*(1+swarm*1.8),radius=p.Rad+(7.5+index%5*.4-p.Rad)*swarm;
        double x=Math.Cos(angle)*radius,z=Math.Sin(angle)*radius*.85-1.5;
        double y=p.H+(3+Math.Sin(index)*.8-p.H)*swarm+Math.Sin(time*2+p.Ph)*.3;
        double yaw=-angle+(p.Sp>0?0:Math.PI),sx=p.S*(.35+.65*Math.Abs(Math.Sin(time*13+p.Ph)));
        return new CompanionTransform(new(x,y,z),new(0,yaw,0),new(sx,p.S,p.S)).Matrix;
    }
}

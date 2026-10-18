using DigiTactics.Rules;

namespace DigiTactics.Presentation;

/// <summary>Spiral Mountain's original strike scheduler and shared lightning flicker.</summary>
public sealed class BoardLightning
{
    private readonly DeterministicRandom bolts=new(91),delay;
    private readonly double[][] spots;
    private readonly double intensity;
    private bool? previousCombat;
    private double previousTime,strikeTime,next=1.5;
    public int Active { get; private set; }=-1;
    public bool Visible { get; private set; }
    public double X { get; private set; }
    public double Y { get; private set; }
    public double Z { get; private set; }
    public double Yaw { get; private set; }
    public double Flash { get; private set; }
    public BoardLightning(double[][] spots,double intensity,uint delaySeed,int delayCalls,int boltCalls)
    {
        this.spots=spots;this.intensity=intensity;delay=new(delaySeed);
        for(int i=0;i<delayCalls;i++)delay.Next();
        for(int i=0;i<boltCalls;i++)bolts.Next();
    }
    private void Strike(double time)
    {
        Active=(int)(bolts.Next()*3);var spot=spots[(int)(bolts.Next()*spots.Length)];
        X=spot[0];Y=spot[1];Z=spot[2];Yaw=bolts.Next()*Math.PI*2;strikeTime=time;Flash=intensity;
        Visible=false; // A scheduled strike becomes visible on the next visibility tick.
    }
    public void Advance(double delta,double time,bool combat)
    {
        if(previousCombat is false && combat) Strike(previousTime);
        previousCombat=combat;
        if(Active>=0)
        {
            double age=time-strikeTime;
            Visible=age<.32 && (age<.08 || age>.14);
            if(age>.35) {Visible=false;Active=-1;}
        }
        if(combat && time>next) {Strike(time);next=time+1.2+delay.Next()*2.5;}
        Flash=Math.Max(0,Flash-delta*5); previousTime=time;
    }
}

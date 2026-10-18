using DigiTactics.Rules;

namespace DigiTactics.Presentation;

public sealed class BoardFireworks
{
    public sealed class Burst(int count)
    {
        public float[] Positions {get;}=new float[count*3];
        internal float[] Velocity {get;}=new float[count*3];
        internal double Start=-9,X,Y,Z;
        public int ColorIndex {get;internal set;}=-1;
        public double Opacity {get;internal set;}=1;
        public bool Visible {get;internal set;}
    }
    private readonly DeterministicRandom particles=new(55),schedule;
    private readonly int colors;
    private int nextBurst,showLeft;
    private double next=3,showTime;
    private bool previousWon;
    public Burst[] Bursts {get;}
    public BoardFireworks(int count,int max,int colors,uint seed,int calls,bool initialWon=false)
    {
        this.colors=colors;previousWon=initialWon;schedule=new(seed);for(int i=0;i<calls;i++)schedule.Next();
        Bursts=Enumerable.Range(0,max).Select(_=>new Burst(count)).ToArray();
    }
    private void Launch(double time,double x,double y,double z)
    {
        var b=Bursts[nextBurst++%Bursts.Length];b.Start=time;b.X=x;b.Y=y;b.Z=z;
        b.ColorIndex=(int)(particles.Next()*colors);double speed=2.2+particles.Next()*1.2;
        for(int i=0;i<b.Velocity.Length;i+=3)
        {
            double u=particles.Next()*2-1,a=particles.Next()*Math.PI*2,s=Math.Sqrt(1-u*u);
            b.Velocity[i]=(float)(Math.Cos(a)*s*speed);b.Velocity[i+1]=(float)(u*speed);b.Velocity[i+2]=(float)(Math.Sin(a)*s*speed);
        }
    }
    public void Advance(double delta,double time,bool won)
    {
        if(won&&!previousWon)showLeft=7;previousWon=won;
        // Original particle tick precedes the scheduler; a launch becomes visible on the next tick.
        foreach(var b in Bursts)
        {
            double k=time-b.Start;if(k>2.2){b.Visible=false;continue;}b.Visible=true;
            for(int i=0;i<b.Positions.Length;i+=3){b.Positions[i]=(float)(b.X+b.Velocity[i]*k);b.Positions[i+1]=(float)(b.Y+b.Velocity[i+1]*k-1.6*k*k);b.Positions[i+2]=(float)(b.Z+b.Velocity[i+2]*k);}
            b.Opacity=Math.Max(0,1-k/2.2);
        }
        if(time>next){Launch(time,(schedule.Next()-.5)*22,3.5+schedule.Next()*2.5,-9-schedule.Next()*2);next=time+4+schedule.Next()*4;}
        if(showLeft>0){showTime-=delta;if(showTime<=0){showLeft--;showTime=.35;Launch(time,(schedule.Next()-.5)*20,3+schedule.Next()*3,-6-schedule.Next()*5);}}
    }
}

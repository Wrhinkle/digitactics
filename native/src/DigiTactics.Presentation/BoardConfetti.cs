using DigiTactics.Rules;

namespace DigiTactics.Presentation;

public sealed class BoardConfetti
{
    private sealed class Piece { public double X,Y=-50,Z,Vx,Vy,Vz,Rx,Rz,Spin; }
    private readonly Piece[] pieces;
    private readonly DeterministicRandom random=new(77);
    private bool previousWon;
    private double left;
    public bool Visible=>left>0;
    public BoardConfettiPose[] Poses {get;}
    public BoardConfetti(int count,bool initialWon=false)
    {
        previousWon=initialWon;pieces=Enumerable.Range(0,count).Select(_=>new Piece()).ToArray();Poses=new BoardConfettiPose[count];
    }
    public void Advance(double delta,bool won)
    {
        if(won&&!previousWon)
        {
            foreach(var p in pieces)
            {
                double x=(random.Next()-.5)*16,z=-6+random.Next()*12;
                p.X=x+(random.Next()-.5)*1.5;p.Y=8+random.Next()*2;p.Z=z+(random.Next()-.5)*1.5;
                p.Vx=(random.Next()-.5)*6;p.Vy=2+random.Next()*5;p.Vz=(random.Next()-.5)*6;
                p.Rx=random.Next()*6;p.Rz=random.Next()*6;p.Spin=4+random.Next()*8;
            }
            left=5;
        }
        previousWon=won;if(left<=0)return;left-=delta;
        for(int i=0;i<pieces.Length;i++)
        {
            var p=pieces[i];p.Vy=Math.Max(-2.2,p.Vy-6*delta);p.X+=p.Vx*delta;p.Y+=p.Vy*delta;p.Z+=p.Vz*delta;
            p.Vx*=.985;p.Vz*=.985;p.Rx+=p.Spin*delta;p.Rz+=p.Spin*.7*delta;
            Poses[i]=new(p.X,p.Y,p.Z,p.Rx,p.Rz);
        }
    }
}

public readonly record struct BoardConfettiPose(double X,double Y,double Z,double Rx,double Rz);

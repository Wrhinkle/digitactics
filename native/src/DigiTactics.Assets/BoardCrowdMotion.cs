namespace DigiTactics.Assets;

public sealed class BoardCrowdMotion(bool initialCombat=false,bool initialWon=false)
{
    private bool previousCombat=initialCombat,previousWon=initialWon;
    private double previousTime,jump=-1,cheer=-1;
    public void Advance(double time,bool combat,bool won)
    {
        if(combat&&!previousCombat)jump=previousTime+3;
        if(won&&!previousWon)cheer=previousTime+6;
        previousCombat=combat;previousWon=won;previousTime=time;
    }
    public double Height(BoardCrowdItem p,double time)
    {
        if(time<cheer)return p.By+Math.Abs(Math.Sin(time*9+p.Ph))*.45;
        if(time<jump)return p.By+Math.Max(0,Math.Sin(time*7+p.Ph))*.3;
        double w=Math.Sin(p.A*2-time*2.2);return p.By+(w>.85?(w-.85)*3:0);
    }
    public Matrix4D Instance(string kind,BoardCrowdItem p,double time)=>new CompanionTransform(new(p.X,kind=="knights-crowd"?Height(p,time):p.Y,p.Z),new(kind=="knights-banners"?-.05+Math.Sin(time*1.4+p.Ph)*.06:0,p.Ry,0),new(p.S,p.S,p.S)).Matrix;
    public void Flashes(float[] vertices,BoardCrowdItem[] people,double time)
    {
        for(int i=0;i<people.Length;i++)
        {
            var p=people[i];bool on=Math.Sin(time*3.7+i*12.9)>.97||(time<cheer&&Math.Sin(time*9+i*4.1)>.8);
            int at=i*GltfPrimitive.Stride;vertices[at]=(float)p.X;vertices[at+1]=on?(float)(Height(p,time)+.6):-50;vertices[at+2]=(float)p.Z;
        }
    }
}

public sealed record BoardCrowdItem(double X=0,double Y=0,double Z=0,double By=0,double A=0,double Ry=0,double Ph=0,double S=1);

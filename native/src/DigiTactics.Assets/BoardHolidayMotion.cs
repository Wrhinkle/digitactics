using System.Numerics;

namespace DigiTactics.Assets;

public sealed class BoardHolidayMotion(bool initialWon=false)
{
    private bool previousWon=initialWon;
    private double previousTime,until=-1;
    private static readonly Vector3[] colors=[BoardLighting.Linear("#ff3a3a"),BoardLighting.Linear("#ffd83a"),BoardLighting.Linear("#3aff6a"),BoardLighting.Linear("#3a8aff"),BoardLighting.Linear("#ff6ae0")];
    private static readonly Vector3 gold=BoardLighting.Linear("#ffe680");
    public bool Party {get;private set;}
    public float SnowSize=>Party?.18f:.13f;
    public float SparkleOpacity=>Party?.9f:0;
    public void Advance(double time,bool won)
    {
        if(won&&!previousWon)until=previousTime+5;
        previousWon=won;previousTime=time;Party=time<until;
    }
    public Vector3 Color(string kind,int index,double phase,double time)
    {
        if(kind=="holiday-tree")return (Party?gold:colors[index%colors.Length])*(Math.Sin(time*(Party?9:2.2)+phase)>-.2?1:.15f);
        if(kind=="holiday-string")return Party?gold:colors[(index+(int)(time*2))%colors.Length];
        throw new ArgumentException("Unknown holiday light.",nameof(kind));
    }
}

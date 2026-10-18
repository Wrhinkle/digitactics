namespace DigiTactics.Presentation;

public readonly record struct CombatPickTarget(int Id, double X, double Y, bool Alive = true);

/// <summary>Original arena selection: nearest living body anchor within 40 pixels, with stable roster-order ties.</summary>
public static class CombatPicking
{
    public static int? Pick(IEnumerable<CombatPickTarget> targets,double x,double y)
    {
        int? best=null; double distance=40*40;
        foreach(var target in targets)
        {
            if(!target.Alive) continue;
            double dx=target.X-x,dy=target.Y-y,d=dx*dx+dy*dy;
            if(d<distance) { distance=d; best=target.Id; }
        }
        return best;
    }
}

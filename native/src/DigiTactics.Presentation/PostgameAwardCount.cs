namespace DigiTactics.Presentation;

/// <summary>Original shop.js result sequence: 350 ms lead-in, 380 ms per line, then 700 ms for the total.</summary>
public static class PostgameAwardCount
{
    public static int Value(int amount,double time,double delay,double duration)
    {
        double k=Math.Clamp((time-delay)/duration,0,1);
        return (int)Math.Floor(amount*(1-Math.Pow(1-k,3))+.5);
    }
    public static int Line(int amount,int index,double time)=>Value(amount,time,.35+index*.38,.38);
    public static int Total(int amount,int lines,double time)=>Value(amount,time,.35+lines*.38,.7);
    public static int CompletedRows(int lines,double time)
    {
        if(time<.35)return 0;
        int done=Math.Min(lines,(int)((time-.35)/.38));
        return done+(time>=.35+lines*.38+.7?1:0);
    }
}

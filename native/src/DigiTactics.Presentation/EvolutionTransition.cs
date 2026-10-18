namespace DigiTactics.Presentation;

/// <summary>Original digivolution spin, pulse and increasingly rapid old/new alternation.</summary>
public sealed class EvolutionTransition
{
    public double Time { get; private set; }
    public double? FlashStarted { get; private set; }
    public bool Complete=>FlashStarted is { } start && Time>=start+.26;
    public bool White=>Time>.5 && !Complete;
    public bool ShowNew { get; private set; }
    public double Scale { get; private set; }=1;
    public double Yaw { get; private set; }
    public bool Advance(double delta)
    {
        Time+=Math.Max(0,delta);
        if(Time<2.7)
        {
            double k=Math.Max(0,(Time-.9)/1.8),period=.32-.28*k;
            ShowNew=Time>.9 && Math.Floor((Time-.9)/period)%2==1;
            Scale=1+.08*Math.Sin(Time*18)*Math.Min(1,Time/.9);Yaw=Time*Time*3;
        }
        else if(FlashStarted is null) { FlashStarted=Time;return true; }
        if(Complete) { ShowNew=true;Scale=1;Yaw=0; }
        return false;
    }
    public double FlashOpacity
    {
        get
        {
            if(FlashStarted is not { } start)return 0;
            double t=(Time-start)/.9;
            if(t<.18)return Ease(t/.18);
            if(t<.45)return 1;
            return t<1?1-Ease((t-.45)/.55):0;
        }
    }
    private static double Ease(double x)
    {
        // CSS ease-out: cubic-bezier(0,0,.58,1), applied between each keyframe.
        if(x<=0)return 0;if(x>=1)return 1;
        double low=0,high=1;
        for(int i=0;i<24;i++) { double t=(low+high)/2;double curve=3*(1-t)*t*t*.58+t*t*t;if(curve<x)low=t;else high=t; }
        double u=(low+high)/2;return 3*(1-u)*u*u+u*u*u;
    }
}

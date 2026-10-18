namespace DigiTactics.Presentation;

public sealed record HomeHeroSlide(string Id,string Art,string Eyebrow,string Title,string Desc,string Cta,string Act,string Thumb,string? Meta=null,bool Feat=false);
public sealed record HomeHeroCatalog(HomeHeroSlide[] Sets,HomeHeroSlide Store,HomeHeroSlide Multiplayer,HomeHeroSlide Farm,HomeHeroSlide Evolution)
{
    public HomeHeroSlide[] Slides(IEnumerable<HomeHeroSlide>? sets=null,bool featured=true)
    {
        var slides=(sets??Sets).ToList();
        if(featured)slides.Insert(Math.Min(1,slides.Count),Store);
        slides.Add(Multiplayer);
        if(slides.Count<7)slides.Add(Farm);
        if(slides.Count<7)slides.Add(Evolution);
        return slides.ToArray();
    }
}

/// <summary>The original renderHero / renderHeroLater timeout pair, independent of frame rate.</summary>
public sealed class HomeCarousel(int count)
{
    public const double Interval=8;
    public int Count {get;}=Math.Max(1,count);
    public int Index {get;private set;}
    public int Revision {get;private set;}
    public double RenderedAt {get;private set;}
    public double? Due {get;private set;}
    public bool Deferred {get;private set;}
    public double ProgressAge {get;private set;}
    private double last;
    public void Render(double now,int? selected=null)
    {
        int next=((selected??Index)%Count+Count)%Count;
        if(Revision==0||next!=Index)ProgressAge=0;
        Index=next;RenderedAt=last=now;Due=now+Interval;Deferred=false;Revision++;
    }
    public void Tick(double now,bool home,bool client,bool module,bool result,bool overview,bool hovered)
    {
        if(Revision==0)return;
        if(!hovered)ProgressAge+=Math.Max(0,now-last);last=now;
        if(Due is not {} due||now<due)return;
        if(Deferred)
        {
            Due=null;
            if(home&&client)Render(now);
        }
        else if(home&&client&&!module&&!result&&overview&&!hovered)Render(now,Index+1);
        else {Deferred=true;Due=now+Interval;}
    }
    public double Progress(bool motion)=>Math.Clamp(ProgressAge/ClientMotion.CssDuration(Interval,motion),0,1);
    public static double Opacity(double age,bool motion)=>EliminationPlayback.CssEase(age/ClientMotion.CssDuration(.9,motion));
    public static double Scale(double age,bool motion)=>1.06-.06*Math.Clamp(age/ClientMotion.CssDuration(9,motion),0,1);
    public static double TextProgress(double age,int child,bool motion)
    {
        // The preference shortens duration, retaining the source stagger delays.
        double x=Math.Clamp((age-child*.05)/ClientMotion.CssDuration(.55,motion),0,1);
        return Bezier(x,.2,.8,.2,1);
    }
    private static double Bezier(double x,double x1,double y1,double x2,double y2)
    {
        double lo=0,hi=1,t=0;
        static double Value(double t,double a,double b)=>3*(1-t)*(1-t)*t*a+3*(1-t)*t*t*b+t*t*t;
        for(int i=0;i<32;i++){t=(lo+hi)*.5;if(Value(t,x1,x2)<x)lo=t;else hi=t;}
        return Value((lo+hi)*.5,y1,y2);
    }
}

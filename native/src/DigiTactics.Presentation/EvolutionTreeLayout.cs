namespace DigiTactics.Presentation;

public readonly record struct EvolutionTreeLayout(int NodeWidth,int PortraitSize,int Columns,bool ShowNames)
{
    public static EvolutionTreeLayout Fit(double width,double height,IReadOnlyList<int> rows)
    {
        double w=width-70,h=height-14;int nw=66;
        for(;nw>30;nw-=2)
        {
            int np=(int)Math.Floor(nw*.82+.5),label=nw>=46?15:0,per=Math.Max(1,(int)Math.Floor((w+4)/(nw+4)));
            int lines=rows.Sum(count=>(int)Math.Ceiling((double)count/per));
            if(lines*(np+6+label)+rows.Count*4<=h)break;
        }
        return new(nw,(int)Math.Floor(nw*.82+.5),Math.Max(1,(int)Math.Floor((w+4)/(nw+4))),nw>=46);
    }
}

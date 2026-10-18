namespace DigiTactics.Assets;

public static class BoardCarnivalMotion
{
    public static Matrix4D Wheel(double time)=>new CompanionTransform(new(0,0,0),new(0,0,time*.18),new(1,1,1)).Matrix
        *new CompanionTransform(new(11.2,5.6,-10.6),new(0,-.6,0),new(1,1,1)).Matrix;
    public static Matrix4D Instance(string kind,int index,BoardCarnivalItem p,double time)
    {
        double x=0,y=0,z=0,ry=0,s=1,sy=1;
        switch(kind)
        {
            case "carnival-gondolas":case "carnival-bulbs":
                double a=time*.18+index/(kind=="carnival-gondolas"?12d:36d)*Math.PI*2,lx=Math.Cos(a)*5;
                x=11.2+lx*Math.Cos(-.6);y=5.6+Math.Sin(a)*5;z=-10.6-lx*Math.Sin(-.6);
                if(kind=="carnival-gondolas")ry=-.6;else s=Math.Sin(time*4+index)>0?1:.4;
                sy=s;break;
            case "carnival-horses":
                double angle=time*.6+index/6d*Math.PI*2;x=-11.2+Math.Cos(angle)*1.9;z=-4.2+Math.Sin(angle)*1.9;y=1.1+Math.Sin(time*2.4+index*1.3)*.25;ry=-angle;break;
            case "carnival-balloons":x=p.Bx+Math.Sin(time*.8+p.Ph)*.12;z=p.Bz;y=p.By+Math.Sin(time*1.2+p.Ph)*.12;sy=1.2;break;
            default:throw new ArgumentException("Unknown carnival motion.",nameof(kind));
        }
        return new CompanionTransform(new(x,y,z),new(0,ry,0),new(s,sy,s)).Matrix;
    }
}

public sealed record BoardCarnivalItem(double Bx=0,double By=0,double Bz=0,double Ph=0);

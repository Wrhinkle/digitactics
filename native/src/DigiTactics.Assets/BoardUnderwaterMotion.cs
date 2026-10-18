namespace DigiTactics.Assets;

public sealed class BoardUnderwaterMotion(bool initialCombat=false)
{
    private double scatter,until=-1,previousTime;
    private bool previousCombat=initialCombat;
    public void Advance(double delta,double time,bool combat)
    {
        if(combat && !previousCombat)until=previousTime+2.5;
        previousCombat=combat;previousTime=time;
        scatter+=((time<until?1:0)-scatter)*Math.Min(1,3*delta);
    }
    public Matrix4D Instance(string kind,BoardUnderwaterItem p,double time)
    {
        double x=p.X,y=p.Y,z=p.Z,rx=0,ry=p.Ry,rz=p.Rz,sx=p.Sx??p.S,sy=p.Sy??p.S,sz=p.Sz??p.S;
        switch(kind)
        {
            case "underwater-shafts":sx=1.4+.5*Math.Sin(time*.5+p.Ph);break;
            case "underwater-kelp":rz=Math.Sin(time*.9+p.Ph)*.12;rx=Math.Cos(time*.7+p.Ph)*.08;break;
            case "underwater-clams":rx=-Math.Max(0,Math.Sin(time*.6+p.Ph))*.7;break;
            case "underwater-fish":
                double a=p.Dir*time*(.25+scatter*.6)+p.Off+p.S*1.6,r=p.Rad+scatter*(3+p.Ph%3);
                x=Math.Cos(a)*r;z=Math.Sin(a)*r*.9-1.5;y=p.H+Math.Sin(time*2+p.Ph)*.12+scatter*(p.Ph%2);
                ry=-a+(p.Dir>0?-Math.PI/2:Math.PI/2)+Math.Sin(time*9+p.Ph)*.15;break;
            default:throw new ArgumentException("Unknown underwater motion.",nameof(kind));
        }
        return new CompanionTransform(new(x,y,z),new(rx,ry,rz),new(sx,sy,sz)).Matrix;
    }
    public static void Scroll(float[] vertices,float[] initial,string kind,double time)
    {
        double u=kind=="underwater-caustics"?time*.012:-time*.01,v=kind=="underwater-caustics"?time*.008:time*.013;
        for(int at=0;at<vertices.Length;at+=GltfPrimitive.Stride){vertices[at+6]=(float)(initial[at+6]+u);vertices[at+7]=(float)(initial[at+7]+v);}
    }
}

namespace DigiTactics.Assets;

public sealed class BoardPyramidMotion(bool initialCombat=false)
{
    private bool previousCombat=initialCombat;
    private double previousTime,until=-1,blaze;
    private readonly double[] bands=new double[6];
    public void Advance(double delta,double time,bool combat,int stage)
    {
        if(combat&&!previousCombat)until=previousTime+2.5;
        previousCombat=combat;previousTime=time;
        blaze+=((time<until?1:0)-blaze)*Math.Min(1,3*delta);
        for(int k=0;k<bands.Length;k++)bands[k]+=((k<Math.Min(6,stage)?.75+.2*Math.Sin(time*2-k):.04)-bands[k])*Math.Min(1,2*delta);
    }
    public float Opacity(string kind,int index,double time)=>kind=="pyramid-beams"?(float)(.18+.05*Math.Sin(time*2+index)+blaze*.45):(float)bands[index];
    public Matrix4D Beam(double[] matrix)=>new CompanionTransform(new(matrix[12],matrix[13],matrix[14]),new(0,0,0),new(1+blaze*1.5,1,1+blaze*1.5)).Matrix;
    public static Matrix4D Flame(BoardProp p,double time)=>new CompanionTransform(new(p.X,p.Y,p.Z),new(0,time,0),new(1,1+.25*Math.Sin(time*12+p.Phase),1)).Matrix;
}

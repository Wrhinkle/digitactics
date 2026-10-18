namespace DigiTactics.Assets;

public sealed class BoardMysteryMotion(BoardProp[] props)
{
    private readonly double[] yaw=props.Select(p=>p.Ry).ToArray();
    public void Advance(double delta){for(int i=0;i<yaw.Length;i++)yaw[i]+=delta*.2;}
    public Matrix4D Satellite(int index,int family,double time)
    {
        var p=props[index];return new CompanionTransform(new(p.X,.8+Math.Sin(time*(.6+family*.05)+index)*.22,p.Z),new(0,yaw[index],0),new(p.S,p.S,p.S)).Matrix;
    }
    public static Matrix4D World(BoardMotion m,double time)
    {
        var local=Matrix4D.From(m.Matrix!);var parent=m.Parent is {} p?Matrix4D.From(p):Matrix4D.Identity;
        Matrix4D Rotation(double x,double y,double z)=>new CompanionTransform(new(0,0,0),new(x,y,z),new(1,1,1)).Matrix;
        switch(m.Kind)
        {
            case "mystery-kinetic":
                var k=m.Kinetic??throw new ArgumentException("Missing kinetic parameters.",nameof(m));
                double angle=time+Math.Sin(time*k.SwingSpeed+k.Phase)*k.Swing;
                parent=parent with {M42=parent.M42+Math.Sin(time*k.BobSpeed+k.Phase)*k.Bob};
                return local*Rotation(k.Rotation[0]*angle,k.Rotation[1]*angle,k.Rotation[2]*angle)*parent;
            case "mystery-monument":
                local=Rotation(0,time*(.15+m.Speed*.025),0)*local;
                goto case "mystery-center";
            case "mystery-center":
                return local*(m.Speed is 3 or 6 or 8?Rotation(0,0,time*.25):Matrix4D.Identity)*parent;
            case "mystery-blades":return local*Rotation(0,0,time*.6)*parent;
            case "mystery-rings":return local*Rotation(0,time*.2,Math.Sin(time*.3)*.3)*parent;
            case "mystery-meteor":return new CompanionTransform(new(local.M41,1.7+Math.Sin(time+m.Speed)*.3,local.M43),new(0,time*.3,0),new(1,1,1)).Matrix;
            default:throw new ArgumentException("Unknown mystery world motion.",nameof(m));
        }
    }
}

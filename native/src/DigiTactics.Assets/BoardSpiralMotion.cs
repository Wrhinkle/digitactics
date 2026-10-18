namespace DigiTactics.Assets;

/// <summary>Preserves the original frame-driven helix and islet update order.</summary>
public sealed class BoardSpiralMotion
{
    private readonly BoardMotion motion;
    private readonly double[] phases,heights;
    private double speed=1;
    public Matrix4D[] Matrices { get; }
    public BoardSpiralMotion(BoardMotion motion)
    {
        if(motion.Kind is not ("spiral-rocks" or "spiral-islets") || motion.Spirals is null)
            throw new ArgumentException("Expected spiral board motion.",nameof(motion));
        this.motion=motion; phases=motion.Spirals.Select(p=>p.Phase).ToArray(); heights=motion.Spirals.Select(p=>p.Y).ToArray();
        Matrices=new Matrix4D[motion.Spirals.Length];
        Advance(0,0,false); // The exported source scene runs its initial zero-delta tick too.
    }
    public void Advance(double delta,double time,bool combat)
    {
        for(int i=0;i<Matrices.Length;i++)
        {
            var p=motion.Spirals![i]; double x=p.X,y=p.Y,z=p.Z,rx=0,ry;
            if(motion.Kind=="spiral-rocks")
            {
                double angle=p.A0+phases[i]; x=Math.Cos(angle)*p.Rad; z=Math.Sin(angle)*p.Rad; y=-3+p.K*8;
                rx=time*.4+p.Sp; ry=time*.3+p.Sp;
            }
            else
            {
                heights[i]+=Math.Sin(time*.5+p.Ph)*.004; y=heights[i]; ry=time*.05+p.Ph;
            }
            Matrices[i]=new CompanionTransform(new(x,y,z),new(rx,ry,0),new(p.S,p.S,p.S)).Matrix;
        }
        // Source advances phase after writing instance matrices, so it affects the next frame.
        if(motion.Kind=="spiral-rocks")
        {
            speed+=((combat?3.2:1)-speed)*Math.Min(1,2*delta);
            for(int i=0;i<phases.Length;i++) phases[i]+=delta*.07*speed;
        }
    }
}

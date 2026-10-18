namespace DigiTactics.Assets;

public sealed class RetroParticle(bool puff, Vector3D position, double started, Vector3D velocity)
{
    public bool Puff { get; } = puff;
    public Vector3D Position { get; internal set; } = position;
    public double Started { get; } = started;
    public Vector3D Velocity { get; } = velocity;
    public double Opacity { get; internal set; }
    public double Scale { get; internal set; } = puff ? 1 : .32;
}

/// <summary>Original per-display-frame particles, independent of simulation time and lockstep state.</summary>
public sealed class RetroParticles(string action, bool glyph, Func<double>? random = null)
{
    private readonly List<RetroParticle> particles = [];
    private readonly Func<double> nextRandom = random ?? Random.Shared.NextDouble;
    private double lastPhase = double.NaN, nextGlyph;
    private bool poofed;
    public IReadOnlyList<RetroParticle> Active => particles;
    private void Add(bool puff, Vector3D at, double time)
    {
        double angle = nextRandom() * Math.PI * 2, speed = puff ? .9 : .15;
        particles.Add(new(puff,at,time,new(Math.Cos(angle)*speed,puff?.4:.7,Math.Sin(angle)*speed)));
    }
    public void Update(double seconds, RetroEmotePose? motion)
    {
        if (motion is null) { particles.Clear(); return; }
        var at = motion.Holder.Position;
        if (action == "stomp" && motion.Phase < lastPhase)
            for(int i=0;i<4;i++) Add(true,new(at.X,.12,at.Z),seconds);
        lastPhase = motion.Phase;
        if (motion.Exit > 0 && !poofed)
        {
            poofed = true;
            for(int i=0;i<9;i++) Add(true,new(at.X,at.Y+.35,at.Z),seconds);
        }
        if(glyph && seconds>.3 && seconds<2.1 && seconds*1000>=nextGlyph)
        {
            nextGlyph=seconds*1000+280;
            Add(false,new(at.X,at.Y+.8,at.Z),seconds);
        }
        for(int i=particles.Count-1;i>=0;i--)
        {
            var p=particles[i];double age=(seconds-p.Started)/(p.Puff?.5:1.1);
            if(age>=1){particles.RemoveAt(i);continue;}
            // The original renderer advances particles by 1/60 on each displayed frame.
            p.Position=new(p.Position.X+p.Velocity.X/60,p.Position.Y+p.Velocity.Y/60,p.Position.Z+p.Velocity.Z/60);
            p.Opacity=(p.Puff?.9:1)*(1-age);
            if(p.Puff)p.Scale=1+age*1.6;
        }
    }
}

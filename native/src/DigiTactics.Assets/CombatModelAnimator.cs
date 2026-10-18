namespace DigiTactics.Assets;

/// <summary>Combat action playback with a separate movement overlay.</summary>
public sealed class CombatModelAnimator
{
    private readonly ModelAnimator actions;
    private readonly CompanionLocomotion locomotion;
    private readonly RigAnimation? run;
    private readonly RigPose runPose;
    private double runWeight;
    public RigPose Pose => actions.Pose;
    public string? Role => actions.Role;
    public CompanionLocomotionKind LocomotionKind => locomotion.Kind;
    public double GlideOffset { get; private set; }
    public bool UsesTravelHop => locomotion.UsesTravelHop;
    public CombatModelAnimator(GltfRig rig, string source, double idleOffset = 0)
    {
        actions=new(rig,idleOffset);runPose=new(rig);
        run=new[]{"run","walk","move"}.Select(name=>rig.Animations.FirstOrDefault(clip=>clip.Name==name)).FirstOrDefault(clip=>clip is not null);
        locomotion=new(rig,source,Pose,run is not null);
    }
    public bool Play(string role)
    {
        bool played=actions.Play(role);
        if(played){runWeight=0;locomotion.Reset();GlideOffset=0;}
        return played;
    }
    public void Update(double delta, bool moving)
    {
        actions.Update(delta);
        if(Role is not null){runWeight=0;locomotion.Reset();GlideOffset=0;return;}
        runWeight=Math.Clamp(runWeight+(moving?1:-1)*delta*5,0,1);
        if(run is not null&&runWeight>0)
        {
            runPose.Sample(run,actions.Time,loop:true);
            for(int i=0;i<Pose.Local.Length;i++)
            {
                var idle=Pose.Local[i];var travel=runPose.Local[i];
                if(runWeight==1){Pose.Local[i]=travel;continue;}
                Pose.Local[i]=idle with{Translation=Lerp(idle.Translation,travel.Translation,runWeight),
                    Rotation=RigPose.Blend(idle.Rotation,travel.Rotation,runWeight),Scale=Lerp(idle.Scale,travel.Scale,runWeight)};
            }
            Pose.UpdateWorld();
        }
        GlideOffset=locomotion.Apply(Pose,moving,actions.Time,delta);
    }
    private static Vector3D Lerp(Vector3D a,Vector3D b,double weight)=>new(a.X+(b.X-a.X)*weight,a.Y+(b.Y-a.Y)*weight,a.Z+(b.Z-a.Z)*weight);
}

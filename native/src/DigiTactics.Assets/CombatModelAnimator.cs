namespace DigiTactics.Assets;

/// <summary>Combat action playback with a separate movement overlay.</summary>
/// <remarks>The action mixer keeps the original renderer's policy (a finished one-shot drops out on its last frame,
/// a repeated attack restarts at frame 0, an action replaces the gait at once). Those switches are discontinuous,
/// so this layer crossfades from the last displayed pose into the new one, and eases the gait weight.</remarks>
public sealed class CombatModelAnimator
{
    /// <summary>Crossfade from the displayed pose when an action starts, ends, or a looping clip wraps onto a different pose.</summary>
    public const double StartBlend = .12, EndBlend = .22, SeamBlend = .15;
    // A loop whose last and first joint positions differ by more than this fraction of the skeleton extent is blended at its wrap.
    private const double SeamTolerance = .02;
    private readonly ModelAnimator actions;
    private readonly CompanionLocomotion locomotion;
    private readonly RigAnimation? run;
    private readonly RigPose runPose;
    private readonly NodeTransform[] shown, from;
    private readonly bool idleSeam, runSeam;
    private double runWeight, blendElapsed, blendDuration, lastIdleTime, lastRunTime;
    public RigPose Pose => actions.Pose;
    public string? Role => actions.Role;
    public CompanionLocomotionKind LocomotionKind => locomotion.Kind;
    public double GlideOffset { get; private set; }
    public bool UsesTravelHop => locomotion.UsesTravelHop;
    /// <summary>True while the displayed pose is still easing out of a discontinuous switch.</summary>
    public bool Blending => blendElapsed < blendDuration;
    /// <summary>False reproduces the unsmoothed original switches (diagnostic A/B captures only).</summary>
    public bool Smoothing { get; init; } = true;
    public CombatModelAnimator(GltfRig rig, string source, double idleOffset = 0)
    {
        actions=new(rig,idleOffset);runPose=new(rig);
        run=new[]{"run","walk","move"}.Select(name=>rig.Animations.FirstOrDefault(clip=>clip.Name==name)).FirstOrDefault(clip=>clip is not null);
        locomotion=new(rig,source,Pose,run is not null);
        shown=Pose.Local.ToArray();from=new NodeTransform[shown.Length];
        idleSeam=HasSeam(rig,ModelAnimator.FindIdle(rig));runSeam=HasSeam(rig,run);
        lastIdleTime=IdleTime;
    }
    public bool Play(string role)
    {
        bool played=actions.Play(role);
        if(played){runWeight=0;locomotion.Reset();GlideOffset=0;Crossfade(StartBlend);}
        return played;
    }
    public void Update(double delta, bool moving)
    {
        string? before=Role;
        actions.Update(delta);
        double idleTime=IdleTime;
        if(before is not null&&Role is null)Crossfade(EndBlend);
        else if(delta>0&&idleSeam&&Role is null&&idleTime<lastIdleTime)Crossfade(SeamBlend);
        lastIdleTime=idleTime;
        if(Role is not null){runWeight=0;locomotion.Reset();GlideOffset=0;Finish(delta);return;}
        runWeight=Math.Clamp(runWeight+(moving?1:-1)*delta*5,0,1);
        if(run is not null&&runWeight>0)
        {
            double runTime=run.Duration>0?actions.Time%run.Duration:0;
            if(delta>0&&runSeam&&runTime<lastRunTime)Crossfade(SeamBlend);
            lastRunTime=runTime;
            runPose.Sample(run,actions.Time,loop:true);
            // Ease the linear gait ramp so the body does not start or stop moving with a velocity step.
            double weight=Smoothing?runWeight*runWeight*(3-2*runWeight):runWeight;
            for(int i=0;i<Pose.Local.Length;i++)
            {
                var idle=Pose.Local[i];var travel=runPose.Local[i];
                if(weight==1){Pose.Local[i]=travel;continue;}
                Pose.Local[i]=idle with{Translation=Lerp(idle.Translation,travel.Translation,weight),
                    Rotation=RigPose.Blend(idle.Rotation,travel.Rotation,weight),Scale=Lerp(idle.Scale,travel.Scale,weight)};
            }
            Pose.UpdateWorld();
        }
        GlideOffset=locomotion.Apply(Pose,moving,actions.Time,delta);
        Finish(delta);
    }
    private double IdleTime=>actions.Actions.Count>0?actions.Actions[0].Time:0;
    private void Crossfade(double duration)
    {
        // Blend from what is on screen now, including a crossfade that is still running.
        if(!Smoothing)return;
        shown.CopyTo(from,0);blendElapsed=0;blendDuration=duration;
    }
    private void Finish(double delta)
    {
        if(blendElapsed<blendDuration)
        {
            blendElapsed=Math.Min(blendDuration,blendElapsed+delta);
            if(blendElapsed<blendDuration)
            {
                double u=blendElapsed/blendDuration,weight=u*u*(3-2*u);
                for(int i=0;i<Pose.Local.Length;i++)
                {
                    var a=from[i];var b=Pose.Local[i];
                    Pose.Local[i]=b with{Translation=Lerp(a.Translation,b.Translation,weight),Rotation=RigPose.Blend(a.Rotation,b.Rotation,weight),Scale=Lerp(a.Scale,b.Scale,weight)};
                }
                Pose.UpdateWorld();
            }
        }
        Pose.Local.CopyTo(shown,0);
    }
    private static bool HasSeam(GltfRig rig,RigAnimation? clip)
    {
        if(clip is null||clip.Duration<=0)return false;
        var first=new RigPose(rig);var last=new RigPose(rig);first.Sample(clip,0);last.Sample(clip,clip.Duration);
        double minX=double.MaxValue,minY=double.MaxValue,minZ=double.MaxValue,maxX=double.MinValue,maxY=double.MinValue,maxZ=double.MinValue,gap=0,scale=0;
        for(int i=0;i<first.World.Length;i++)
        {
            var a=first.World[i];var b=last.World[i];
            minX=Math.Min(minX,a.M41);minY=Math.Min(minY,a.M42);minZ=Math.Min(minZ,a.M43);maxX=Math.Max(maxX,a.M41);maxY=Math.Max(maxY,a.M42);maxZ=Math.Max(maxZ,a.M43);
            double d=Math.Sqrt((a.M41-b.M41)*(a.M41-b.M41)+(a.M42-b.M42)*(a.M42-b.M42)+(a.M43-b.M43)*(a.M43-b.M43));
            if(double.IsFinite(d))gap=Math.Max(gap,d);
            var p=first.Local[i].Scale;var q=last.Local[i].Scale;
            scale=Math.Max(scale,Math.Max(Math.Abs(p.X-q.X),Math.Max(Math.Abs(p.Y-q.Y),Math.Abs(p.Z-q.Z))));
        }
        double extent=Math.Max(1e-9,Math.Max(maxX-minX,Math.Max(maxY-minY,maxZ-minZ)));
        return gap/extent>SeamTolerance||scale>SeamTolerance;
    }
    private static Vector3D Lerp(Vector3D a,Vector3D b,double weight)=>new(a.X+(b.X-a.X)*weight,a.Y+(b.Y-a.Y)*weight,a.Z+(b.Z-a.Z)*weight);
}

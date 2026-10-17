namespace DigiTactics.Assets;

public sealed class ModelAction
{
    public RigAnimation Clip { get; }
    public double Time { get; internal set; }
    public double Weight { get; internal set; } = 1;
    public double Speed { get; internal set; } = 1;
    public bool Enabled { get; internal set; } = true;
    internal bool Repeat = true;
    internal float? FadeStart, FadeEnd;
    internal bool FadeIn;
    internal ModelAction(RigAnimation clip) { Clip = clip; }
}

/// <summary>Per-model action policy and normal-weight track blending, matching UnitView's mixer.</summary>
public sealed class ModelAnimator
{
    private readonly GltfRig rig;
    private readonly RigPose scratch;
    private readonly NodeTransform[] original;
    private readonly double[,] weights;
    private readonly List<ModelAction> actions = [];
    private readonly ModelAction? idle;
    private ModelAction? oneShot;
    public RigPose Pose { get; }
    public double Time { get; private set; }
    public string? Role { get; private set; }
    public IReadOnlyList<ModelAction> Actions => actions;
    public static RigAnimation? FindIdle(GltfRig rig) => rig.Animations.FirstOrDefault(a =>
        System.Text.RegularExpressions.Regex.IsMatch(a.Name, "idle|wait|stand", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        ?? rig.Animations.FirstOrDefault(a => a.Name.Contains("_bn01", StringComparison.OrdinalIgnoreCase)) ?? rig.Animations.FirstOrDefault();

    public ModelAnimator(GltfRig rig, double idleOffset = 0)
    {
        if (!double.IsFinite(idleOffset) || idleOffset < 0) throw new ArgumentOutOfRangeException(nameof(idleOffset));
        this.rig = rig; Pose = new(rig); scratch = new(rig);
        var clip = FindIdle(rig);
        Pose.Sample(clip, 0); original = Pose.Local.ToArray(); weights = new double[rig.Nodes.Length, 3];
        if (clip is not null) { idle = new(clip) { Time = idleOffset }; actions.Add(idle); }
    }

    public bool Play(string role)
    {
        if (idle is null || role is not ("attack" or "special" or "win")) return false;
        var clip = rig.Animations.FirstOrDefault(c => c.Name == role);
        if (clip is null || Role is not null && role == "attack" && Role != "attack") return false;
        if (Role == role && role == "attack" && oneShot!.Time < clip.Duration * .6) return false;
        if (oneShot is not null && oneShot.Clip != clip) Fade(oneShot, .08, false);
        var action = actions.FirstOrDefault(a => a.Clip == clip);
        if (action is null) { action = new(clip); actions.Add(action); }
        Reset(action); action.Repeat = role == "win";
        action.Speed = role == "attack" ? Math.Max(1, clip.Duration / .55) : role == "special" ? Math.Max(1, clip.Duration / 1.1) : 1;
        action.Weight = 1; Fade(action, .08, true); Fade(idle, .08, false);
        oneShot = action; Role = role; return true;
    }

    public void EndAction()
    {
        if (oneShot is null) return;
        Fade(oneShot, .15, false); Reset(idle!); Fade(idle!, .15, true);
        oneShot = null; Role = null;
    }

    public void Update(double delta)
    {
        if (!double.IsFinite(delta) || delta < 0) throw new ArgumentOutOfRangeException(nameof(delta));
        Time += delta; Array.Clear(weights); original.CopyTo(Pose.Local, 0);
        foreach (var action in actions)
        {
            if (action.Enabled)
            {
                if (delta != 0)
                {
                    action.Time += delta * action.Speed;
                    if (action.Repeat && action.Clip.Duration > 0) action.Time %= action.Clip.Duration;
                    else if (!action.Repeat && action.Time >= action.Clip.Duration)
                    {
                        action.Time = action.Clip.Duration; action.Enabled = false;
                        if (action == oneShot) EndAction();
                    }
                }
                action.Weight = action.Enabled ? 1 : 0;
                if (action.Enabled && action.FadeStart is { } start && action.FadeEnd is { } end)
                {
                    // Three's fade control interpolants store both times and results as float32.
                    double fraction = end > start ? Math.Clamp((Time - start) / ((double)end - start), 0, 1) : 1;
                    action.Weight = (float)(action.FadeIn ? fraction : 1 - fraction);
                    if (Time > end) { action.FadeStart = null; action.FadeEnd = null; if (action.Weight == 0) action.Enabled = false; }
                }
            }
            else action.Weight = 0;
            if (action.Weight <= 0) continue;
            scratch.Sample(action.Clip, action.Time);
            foreach (var channel in action.Clip.Channels)
            {
                int path = (int)channel.Path, node = channel.Node;
                double previous = weights[node, path]; double sum = previous + action.Weight;
                Pose.Local[node] = Mix(Pose.Local[node], scratch.Local[node], channel.Path, previous == 0 ? 1 : action.Weight / sum);
                weights[node, path] = sum;
            }
        }
        for (int node = 0; node < original.Length; node++)
            for (int path = 0; path < 3; path++)
                if (weights[node, path] < 1) Pose.Local[node] = Mix(Pose.Local[node], original[node], (AnimationPath)path, 1 - weights[node, path]);
        Pose.UpdateWorld();
    }

    private void Fade(ModelAction action, double duration, bool fadeIn)
    { action.FadeStart = (float)Time; action.FadeEnd = (float)(Time + duration); action.FadeIn = fadeIn; }
    private static void Reset(ModelAction action) { action.Enabled = true; action.Time = 0; action.FadeStart = null; action.FadeEnd = null; }
    private static Vector3D Lerp(Vector3D a, Vector3D b, double t) => new(a.X*(1-t)+b.X*t,a.Y*(1-t)+b.Y*t,a.Z*(1-t)+b.Z*t);
    private static NodeTransform Mix(NodeTransform a, NodeTransform b, AnimationPath path, double t) => path switch
    {
        AnimationPath.Translation => a with { Translation = Lerp(a.Translation,b.Translation,t) },
        AnimationPath.Scale => a with { Scale = Lerp(a.Scale,b.Scale,t) },
        _ => a with { Rotation = RigPose.Blend(a.Rotation,b.Rotation,t) }
    };
}

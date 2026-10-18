using System.Text.Json;

namespace DigiTactics.Assets;

public sealed class CompanionAction(RigAnimation clip)
{
    public RigAnimation Clip { get; } = clip;
    public double Time { get; internal set; }
    public double Weight { get; internal set; } = 1;
    public double Speed { get; internal set; } = 1;
    public bool Enabled { get; internal set; } = true;
    public bool Paused { get; internal set; }
    internal bool Active, FadeIn;
    internal double Repetitions = double.PositiveInfinity;
    internal int CompletedLoops;
    internal float? FadeStart, FadeEnd;
}

/// <summary>Per-companion reaction sequencing, idle/run transitions and normal-weight skeletal blending.</summary>
public sealed class CompanionAnimator
{
    private readonly GltfRig rig;
    private readonly CompanionVisual visual;
    private readonly JsonElement fallbacks;
    private readonly RigPose scratch;
    private readonly NodeTransform[] original;
    private readonly double[,] weights;
    private readonly List<CompanionAction> actions = [];
    private readonly CompanionAction? idle, run;
    private CompanionAction? current;
    private CompanionStep[]? sequence;
    private bool sequenceClips;
    private double started, duration, mixerTime;
    public double Time { get; private set; }
    public double Yaw { get; set; }
    public double BodyYaw { get; private set; }
    public double Scale { get; }
    public double ModelYaw { get; }
    public bool Moving { get; private set; }
    public bool HasRun => run is not null;
    public string? Role { get; private set; }
    public int StepIndex { get; private set; } = -1;
    public RigPose Pose { get; }
    public CompanionTransform Transform { get; private set; }
    public IReadOnlyList<CompanionAction> Actions => actions;
    public CompanionAnimator(GltfRig rig, CompanionVisual visual, JsonElement fallbacks, double scale = 1, double modelYaw = 0, double yaw = 0)
    {
        this.rig = rig; this.visual = visual; this.fallbacks = fallbacks; Scale = scale; ModelYaw = modelYaw; Yaw = BodyYaw = yaw;
        Pose = new(rig); scratch = new(rig); weights = new double[rig.Nodes.Length, 3];
        var clip = rig.Animations.FirstOrDefault(x => x.Name == "idle") ?? ModelAnimator.FindIdle(rig);
        Pose.Sample(clip, 0); original = Pose.Local.ToArray();
        if (clip is not null)
        {
            idle = Action(clip); idle.Active = true; current = idle;
            var runClip = new[] { "run", "walk", "move" }.Select(name => rig.Animations.FirstOrDefault(x => x.Name == name)).FirstOrDefault(x => x is not null);
            if (runClip is not null) run = Action(runClip);
        }
        Transform = CompanionMotion.Evaluate(null, 0, scale: scale, yaw: modelYaw);
    }
    private CompanionAction Action(RigAnimation clip)
    {
        var action = actions.FirstOrDefault(x => x.Clip == clip);
        if (action is null) { action = new(clip); actions.Add(action); }
        return action;
    }
    private CompanionAction? Base => Moving && run is not null ? run : idle;
    private void Fade(CompanionAction action, double seconds, bool fadeIn)
    { action.FadeStart = (float)mixerTime; action.FadeEnd = (float)(mixerTime + seconds); action.FadeIn = fadeIn; }
    private static void Reset(CompanionAction action)
    { action.Enabled = true; action.Paused = false; action.Time = 0; action.CompletedLoops = 0; action.FadeStart = action.FadeEnd = null; }
    private void FadeTo(CompanionAction? action, double seconds = .12)
    {
        if (action is null || current == action) return;
        if (current is not null) Fade(current, seconds, false);
        Reset(action); action.Weight = 1; Fade(action, seconds, true); action.Active = true; current = action;
    }
    public void SetMoving(bool moving)
    {
        if (moving == Moving) return;
        Moving = moving;
        if (moving && sequenceClips) ClearSequence();
        if (idle is not null && !sequenceClips) FadeTo(Base);
    }
    public void Play(string role)
    {
        sequence = CompanionMotion.Plan(visual, fallbacks, rig.Animations.Select(x => x.Name), role, Moving, idle is not null);
        sequenceClips = sequence.Any(x => x.Clip is not null); Role = role; StepIndex = -1; NextStep();
    }
    private void ClearSequence() { sequence = null; sequenceClips = false; Role = null; StepIndex = -1; }
    private void NextStep()
    {
        StepIndex++;
        if (StepIndex >= sequence!.Length) { ClearSequence(); FadeTo(Base, .2); return; }
        var step = sequence[StepIndex]; started = Time;
        var clip = rig.Animations.FirstOrDefault(x => x.Name == step.Clip);
        if (clip is not null && idle is not null)
        {
            var action = Action(clip);
            if (current == action) Reset(action); else FadeTo(action, StepIndex == 0 ? .12 : .08);
            action.Repetitions = step.Loops; action.Speed = step.Speed;
            duration = clip.Duration * step.Loops / step.Speed;
        }
        else { FadeTo(Base); duration = step.Length; }
    }
    public void Update(double delta)
    {
        if (!double.IsFinite(delta) || delta < 0) throw new ArgumentOutOfRangeException(nameof(delta));
        Time += delta;
        if (sequence is not null && Time - started >= duration) NextStep();
        mixerTime += delta; Blend(delta);
        double angle = Yaw - BodyYaw; BodyYaw += Math.Atan2(Math.Sin(angle), Math.Cos(angle)) * Math.Min(1, delta * 10);
        var step = sequence?[StepIndex];
        string? effect = step?.Effect ?? (sequence is null && idle is null && !Moving ? "idle" : null);
        double progress = effect == "idle" && sequence is null ? Time : duration > 0 ? Math.Min(1, (Time - started) / duration) : 1;
        Transform = CompanionMotion.Evaluate(effect, progress, step?.Loops ?? 1, duration, Scale, ModelYaw);
    }
    private void Blend(double delta)
    {
        Array.Clear(weights); original.CopyTo(Pose.Local, 0);
        foreach (var action in actions.Where(x => x.Active))
        {
            if (!action.Enabled) { action.Weight = 0; continue; }
            if (!action.Paused && delta != 0)
            {
                action.Time += delta * action.Speed;
                if (action.Clip.Duration > 0 && action.Time >= action.Clip.Duration)
                {
                    int loops = (int)Math.Floor(action.Time / action.Clip.Duration);
                    action.CompletedLoops += loops;
                    if (action.CompletedLoops >= action.Repetitions) { action.Time = action.Clip.Duration; action.Paused = true; }
                    else action.Time %= action.Clip.Duration;
                }
            }
            action.Weight = 1;
            if (action.FadeStart is { } start && action.FadeEnd is { } end)
            {
                double fraction = end > start ? Math.Clamp((mixerTime - start) / ((double)end - start), 0, 1) : 1;
                action.Weight = (float)(action.FadeIn ? fraction : 1 - fraction);
                if (mixerTime > end) { action.FadeStart = action.FadeEnd = null; if (action.Weight == 0) action.Enabled = false; }
            }
            if (action.Weight <= 0) continue;
            scratch.Sample(action.Clip, action.Time);
            foreach (var channel in action.Clip.Channels)
            {
                int node = channel.Node, path = (int)channel.Path;
                double previous = weights[node, path], sum = previous + action.Weight;
                Pose.Local[node] = Mix(Pose.Local[node], scratch.Local[node], channel.Path, previous == 0 ? 1 : action.Weight / sum); weights[node, path] = sum;
            }
        }
        for (int node = 0; node < original.Length; node++) for (int path = 0; path < 3; path++)
            if (weights[node, path] < 1) Pose.Local[node] = Mix(Pose.Local[node], original[node], (AnimationPath)path, 1 - weights[node, path]);
        Pose.UpdateWorld();
    }
    private static Vector3D Lerp(Vector3D a, Vector3D b, double t) => new(a.X*(1-t)+b.X*t,a.Y*(1-t)+b.Y*t,a.Z*(1-t)+b.Z*t);
    private static NodeTransform Mix(NodeTransform a, NodeTransform b, AnimationPath path, double t) => path switch
    {
        AnimationPath.Translation => a with { Translation = Lerp(a.Translation, b.Translation, t) },
        AnimationPath.Scale => a with { Scale = Lerp(a.Scale, b.Scale, t) },
        _ => a with { Rotation = RigPose.Blend(a.Rotation, b.Rotation, t) }
    };
}

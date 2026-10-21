namespace DigiTactics.Assets;

/// <summary>
/// Scripted combat timeline through the production <see cref="CombatModelAnimator"/> at 60 fps: idle, move, stop,
/// repeated attacks, attack interrupted by special, win. Per frame it records the skinned mesh height against idle,
/// the median joint scale against idle, and the largest joint jump (in idle heights), so size changes and transition
/// pops are reported per role and per switch. Height ratios are kept for reference (leaps and raised limbs change them
/// legitimately); the scale flags use the median joint scale. Used by the Desktop --animation-stability sweep and the
/// CPU regression suite.
/// </summary>
public static class CombatAnimationProbe
{
    public const double Step = 1d / 60;
    // A switch frame whose largest joint jump exceeds both this absolute limit and 2.5x the clip's own fastest
    // in-clip motion is a pop rather than authored motion.
    public const double PopLimit = .12, PopFactor = 2.5;
    // Median joint scale (cube root of the world matrix determinant) relative to the same joint at idle. A raised arm or
    // a leap changes height but not this; a scale track that inflates the whole body does.
    public const double MaxBodyScale = 1.25, MinBodyScale = .8;

    public sealed record Event(double Time, string Kind);
    public static readonly Event[] Script =
    [
        new(1.0, "move"), new(2.0, "stop"), new(2.5, "attack"), new(3.2, "attack"), new(3.9, "attack"),
        new(4.15, "special"), new(6.0, "attack"), new(7.0, "win"),
    ];
    public const double ScriptEnd = 9.5;

    public sealed record Switch(string Kind, double Time, double Jump, double Reference, bool Pop);
    public sealed record Report(string Source, string Asset, double IdleHeight, Dictionary<string, double> MaxRatio, Dictionary<string, double> MinRatio,
        Dictionary<string, double> MaxBodyScaleByRole, Dictionary<string, double> MinBodyScaleByRole, double MaxInClipJump, Switch[] Switches, double IdleLoopSeam, double RunLoopSeam, string[] Flags, string? Error);

    /// <summary>Runs the script through the production animator.</summary>
    public static Report Measure(string key, string assetName, GltfRig rig, GltfMesh mesh, bool smoothing = true)
    {
        var animator = new CombatModelAnimator(rig, key) { Smoothing = smoothing };
        var joints = rig.Skins.SelectMany(skin => skin.Joints).Distinct().ToArray();
        if (joints.Length == 0) joints = Enumerable.Range(0, rig.Nodes.Length).ToArray();
        var previous = new Vector3D[joints.Length]; var current = new Vector3D[joints.Length];
        double Height() { var (min, max) = mesh.Bounds(rig, animator.Pose, precise: false); return double.IsFinite(max.Y - min.Y) ? max.Y - min.Y : 0; }
        var idleScale = new double[joints.Length]; var scales = new double[joints.Length];
        double JointScale(int node)
        {
            var m = animator.Pose.World[node];
            double det = m.M11 * (m.M22 * m.M33 - m.M23 * m.M32) - m.M12 * (m.M21 * m.M33 - m.M23 * m.M31) + m.M13 * (m.M21 * m.M32 - m.M22 * m.M31);
            return Math.Cbrt(Math.Abs(det));
        }
        double BodyScale()
        {
            int n = 0;
            for (int j = 0; j < joints.Length; j++) { double s = JointScale(joints[j]); if (idleScale[j] > 1e-12 && double.IsFinite(s)) scales[n++] = s / idleScale[j]; }
            if (n == 0) return 1;
            Array.Sort(scales, 0, n); return scales[n / 2];
        }
        void Joints(Vector3D[] into) { for (int j = 0; j < joints.Length; j++) { var m = animator.Pose.World[joints[j]]; into[j] = new(m.M41, m.M42, m.M43); } }

        animator.Update(0, false);
        double idleHeight = 0; int idleSamples = 0;
        var maxRatio = new Dictionary<string, double>(); var minRatio = new Dictionary<string, double>();
        var maxScale = new Dictionary<string, double>(); var minScale = new Dictionary<string, double>();
        for (int j = 0; j < joints.Length; j++) idleScale[j] = JointScale(joints[j]);
        var jumps = new List<(double Time, double Jump, string Role)>();
        int next = 0; bool moving = false; double time = 0;
        var switches = new List<(Event Event, int Frame)>();
        Joints(previous);
        for (int frame = 1; time < ScriptEnd - 1e-9; frame++)
        {
            while (next < Script.Length && Script[next].Time <= time + 1e-9)
            {
                var e = Script[next++];
                if (e.Kind == "move") moving = true; else if (e.Kind == "stop") moving = false;
                else if (!animator.Play(e.Kind)) { continue; }
                switches.Add((e, frame));
            }
            string? before = animator.Role;
            animator.Update(Step, moving); time += Step;
            if (before is not null && animator.Role is null) switches.Add((new(time, "end-" + before), frame));
            Joints(current);
            double jump = 0;
            for (int j = 0; j < joints.Length; j++)
            {
                double dx = current[j].X - previous[j].X, dy = current[j].Y - previous[j].Y, dz = current[j].Z - previous[j].Z;
                double d = Math.Sqrt(dx * dx + dy * dy + dz * dz); if (double.IsFinite(d)) jump = Math.Max(jump, d);
            }
            (previous, current) = (current, previous);
            string role = animator.Role ?? (moving ? "move" : "idle");
            jumps.Add((time, jump, role));
            double height = Height();
            if (time < 1) { if (time >= .25) { idleHeight += height; idleSamples++; } continue; }
            double ratio = idleSamples > 0 && idleHeight > 0 ? height / (idleHeight / idleSamples) : 1;
            maxRatio[role] = Math.Max(maxRatio.GetValueOrDefault(role, 0), ratio);
            double body = BodyScale();
            maxScale[role] = Math.Max(maxScale.GetValueOrDefault(role, 0), body);
            minScale[role] = Math.Min(minScale.GetValueOrDefault(role, double.MaxValue), body);
            minRatio[role] = Math.Min(minRatio.GetValueOrDefault(role, double.MaxValue), ratio);
        }
        double idle = idleSamples > 0 ? idleHeight / idleSamples : 1; if (idle <= 1e-9) idle = 1;
        // Switch windows: the 3 frames after a switch. Reference: the fastest in-clip frame outside every window.
        var window = new HashSet<int>(switches.SelectMany(s => Enumerable.Range(s.Frame - 1, 4)));
        double inClip = jumps.Where((_, i) => !window.Contains(i)).Select(j => j.Jump).DefaultIfEmpty(0).Max() / idle;
        var results = switches.Select(s =>
        {
            double jump = Enumerable.Range(s.Frame - 1, 4).Where(i => i >= 0 && i < jumps.Count).Select(i => jumps[i].Jump).DefaultIfEmpty(0).Max() / idle;
            return new Switch(s.Event.Kind, Math.Round(s.Event.Time, 3), Math.Round(jump, 4), Math.Round(inClip, 4), jump > PopLimit && jump > inClip * PopFactor);
        }).ToArray();
        var flags = new List<string>();
        foreach (var (role, scale) in maxScale) if (scale > MaxBodyScale) flags.Add($"{role}-body-scale-x{scale:0.00}");
        foreach (var (role, scale) in minScale) if (scale < MinBodyScale) flags.Add($"{role}-body-scale-x{scale:0.00}");
        foreach (var pop in results.Where(s => s.Pop)) flags.Add($"pop-{pop.Kind}@{pop.Time:0.00}");
        double Seam(RigAnimation? clip)
        {
            if (clip is null || clip.Duration <= 0) return 0;
            var a = new RigPose(rig); var b = new RigPose(rig); a.Sample(clip, 0); b.Sample(clip, clip.Duration);
            double worst = 0;
            foreach (int j in joints)
            {
                var p = a.World[j]; var q = b.World[j];
                double d = Math.Sqrt((p.M41 - q.M41) * (p.M41 - q.M41) + (p.M42 - q.M42) * (p.M42 - q.M42) + (p.M43 - q.M43) * (p.M43 - q.M43));
                if (double.IsFinite(d)) worst = Math.Max(worst, d);
            }
            return Math.Round(worst / idle, 4);
        }
        var run = new[] { "run", "walk", "move" }.Select(name => rig.Animations.FirstOrDefault(c => c.Name == name)).FirstOrDefault(c => c is not null);
        return new(key, assetName, Math.Round(idle, 5), maxRatio.ToDictionary(p => p.Key, p => Math.Round(p.Value, 3)), minRatio.ToDictionary(p => p.Key, p => Math.Round(p.Value, 3)),
            maxScale.ToDictionary(p => p.Key, p => Math.Round(p.Value, 3)), minScale.ToDictionary(p => p.Key, p => Math.Round(p.Value, 3)), Math.Round(inClip, 4), results, Seam(ModelAnimator.FindIdle(rig)), Seam(run), flags.ToArray(), null);
    }
}

using System.Numerics;

namespace DigiTactics.Presentation;

public readonly record struct EliminationSlot(double X, double Y, double Z, double Scale);
public sealed record EliminationLayout(EliminationSlot[] Slots, double HalfWidth, double Top, bool Narrow)
{
    public static readonly EliminationSlot Legend = new(0, 0, .3, .86);
    public static EliminationLayout Create(int count, double aspect)
    {
        if (count < 0 || !double.IsFinite(aspect) || aspect <= 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (count == 0) return new([], .6, 1, false);
        double scale = count <= 2 ? .86 : .8, gap = scale * 1.22, inner = Legend.Scale * .6 + gap * .5;
        var slots = Enumerable.Range(0, count).Select(i => new EliminationSlot((i % 2 == 0 ? 1 : -1) * (inner + i / 2 * gap), 0, -.12 - Math.Min(.35, i / 2 * .04), scale)).ToArray();
        return new(slots, Math.Max(.6, slots.Max(s => Math.Abs(s.X) + s.Scale * .55)), Math.Max(1, slots.Max(s => s.Y + s.Scale * 1.05)), aspect < 1.3);
    }
    public static double PopScale(double time, double born, EliminationSlot slot)
    {
        double elapsed = time - born;
        double k = elapsed <= 0 ? .001 : elapsed < .4 ? (1 - Math.Pow(1 - Math.Clamp(elapsed / .4, 0, 1), 3)) * (1 + .14 * Math.Sin(Math.PI * Math.Clamp(elapsed / .4, 0, 1))) : 1;
        return Math.Max(.001, k * slot.Scale);
    }
}

/// <summary>The source lineup camera fits the shallow arc and sampled animated legend bounds.</summary>
public sealed class EliminationCamera
{
    private double? distance, y;
    public Vector3 Eye { get; private set; }
    public Vector3 Target { get; private set; }
    public Matrix4x4 Lineup(EliminationLayout layout, double aspect, double legendTop, bool big, double delta)
    {
        double top = Math.Max(layout.Top, legendTop) + .08, bottom = -.05, tangent = Math.Tan(Math.PI / 12);
        double wantDistance = Math.Max(big ? 3.2 : 3, Math.Max((layout.HalfWidth + .25) / (tangent * aspect), (top - bottom) * .66 / tangent)) + EliminationLayout.Legend.Z;
        double wantY = layout.Narrow ? bottom + (wantDistance - EliminationLayout.Legend.Z) * tangent * .6 : bottom + (top - bottom) * .4;
        double k = distance is null ? 1 : Math.Min(1, delta * 4);
        distance = distance is { } d ? d + (wantDistance - d) * k : wantDistance;
        y = y is { } old ? old + (wantY - old) * k : wantY;
        Eye = new(0, (float)(y.Value + .2 + distance.Value * .025), (float)distance.Value);
        Target = new(0, (float)y.Value, -.35f);
        return Matrix4x4.CreateLookAt(Eye, Target, Vector3.UnitY) * Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 6, (float)aspect, .1f, 60);
    }
    public Matrix4x4 Attack(double time, double aspect)
    {
        double x = Math.Sin(time * 1.2) * .08, y = 1.1;
        if (time > 1.15 && time < 1.6) { x += Math.Sin(time * 90) * .05; y += Math.Cos(time * 71) * .04; }
        Eye = new((float)x, (float)y, (float)(4 - Math.Min(time, 2) * .15)); Target = new(0, .85f, 0);
        return Matrix4x4.CreateLookAt(Eye, Target, Vector3.UnitY) * Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 6, (float)aspect, .1f, 60);
    }
}

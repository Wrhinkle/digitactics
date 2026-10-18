namespace DigiTactics.Presentation;

/// <summary>Local-only arena travel, matching the original right-click companion controller.</summary>
public sealed class CompanionTravel
{
    public double X { get; private set; } = -7.6;
    public double Z { get; private set; } = 8.4;
    public double Yaw { get; private set; } = Math.PI;
    public bool Moving { get; private set; }
    public double? TargetX { get; private set; }
    public double? TargetZ { get; private set; }
    public double Time { get; private set; }
    public double Hop(bool hasRun) => Moving && !hasRun ? Math.Abs(Math.Sin(Time * 12)) * .18 : 0;
    public bool Move(double x, double z)
    {
        if (!double.IsFinite(x) || !double.IsFinite(z)) return false;
        TargetX = Math.Clamp(x, -9.6, 9.6); TargetZ = Math.Clamp(z, -.4, 9.2); return true;
    }
    private static double Distance(double x, double z)
    {
        // Match the source Math.hypot scaling; direct sqrt can delay arrival by one frame at an exact step boundary.
        double maximum = Math.Max(Math.Abs(x), Math.Abs(z));
        if (maximum == 0) return 0;
        x /= maximum; z /= maximum;
        return Math.Sqrt(x * x + z * z) * maximum;
    }
    public void Update(double delta)
    {
        if (!double.IsFinite(delta) || delta < 0) throw new ArgumentOutOfRangeException(nameof(delta));
        Time += delta;
        if (TargetX is not { } x || TargetZ is not { } z) return;
        double dx = x - X, dz = z - Z, distance = Distance(dx, dz), step = 6 * delta;
        if (distance <= step) { X = x; Z = z; TargetX = null; Moving = false; }
        else { X += dx / distance * step; Z += dz / distance * step; Moving = true; Yaw = Math.Atan2(dx, dz); }
    }
}

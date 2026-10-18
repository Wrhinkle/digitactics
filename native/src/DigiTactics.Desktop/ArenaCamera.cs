using System.Numerics;

namespace DigiTactics.Desktop;

/// <summary>Original arena orbit camera. Projection and picking share this session-owned state.</summary>
internal sealed class ArenaCamera
{
    public Vector3 Target { get; private set; } = new(0, 0, 1.8f);
    public double Zoom { get; private set; } = 1;
    public double Yaw { get; private set; }
    public double Pitch { get; private set; } = Math.Atan2(16.5, 15.5);
    public void Reset() { Target = new(0, 0, 1.8f); Zoom = 1; Yaw = 0; Pitch = Math.Atan2(16.5, 15.5); }
    public Vector3 Eye(int width, int height, int columns)
    {
        double distance = Math.Sqrt(16.5 * 16.5 + 15.5 * 15.5) * Math.Max(1, 1.35 * height / width) * Zoom * (columns > 7 ? 1.65 : 1);
        return Target + new Vector3((float)(Math.Sin(Yaw) * Math.Cos(Pitch) * distance), (float)(Math.Sin(Pitch) * distance), (float)(Math.Cos(Yaw) * Math.Cos(Pitch) * distance));
    }
    public Matrix4x4 Matrix(int width, int height, int columns) => Matrix4x4.CreateLookAt(Eye(width, height, columns), Target, Vector3.UnitY)
        * Matrix4x4.CreatePerspectiveFieldOfView(38 * MathF.PI / 180, (float)width / height, .1f, 200);
    private void Translate(double x, double z) => Target = new((float)Math.Clamp(Target.X + x, -8, 8), 0, (float)Math.Clamp(Target.Z + z, -8, 9));
    public void Orbit(float dx, float dy) { Yaw -= dx * .008; Pitch = Math.Clamp(Pitch + dy * .006, .35, 1.45); }
    public void Pan(float x0, float y0, float x1, float y1, int width, int height, int columns)
    {
        var a = ArenaPicking.Ground(x0, y0, width, height, columns, this);
        var b = ArenaPicking.Ground(x1, y1, width, height, columns, this);
        if (a is { } from && b is { } to) Translate(from.X - to.X, from.Z - to.Z);
    }
    public void ZoomAt(float x, float y, double factor, int width, int height, int columns)
    {
        var before = ArenaPicking.Ground(x, y, width, height, columns, this);
        Zoom = Math.Clamp(Zoom * factor, .22, 1.5);
        var after = ArenaPicking.Ground(x, y, width, height, columns, this);
        if (before is { } a && after is { } b) Translate(a.X - b.X, a.Z - b.Z);
    }
}

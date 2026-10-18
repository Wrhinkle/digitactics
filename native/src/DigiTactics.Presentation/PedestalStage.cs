namespace DigiTactics.Presentation;

public sealed record PedestalCamera(WorldPosition Eye, WorldPosition Target, double FovRadians, double Aspect, double Near, double Far);

/// <summary>Original client Stage camera framing and animated Digivice ring color.</summary>
public static class PedestalStage
{
    public const double LegendHeight = 1.05;
    public const double FovRadians = Math.PI / 6;
    public const double Near = 0.05, Far = 50;

    public static PedestalCamera Camera(double width, double height) => Camera(width / height);

    public static PedestalCamera Camera(double aspect)
    {
        if (!double.IsFinite(aspect) || aspect <= 0) throw new ArgumentOutOfRangeException(nameof(aspect));
        double tangent = Math.Tan(FovRadians / 2);
        double distance = Math.Max(1.05 / tangent, 1.32 / (tangent * aspect)) + 0.6;
        return new(new(0, 0.55 + distance * 0.2, distance), new(0, 0.38, 0), FovRadians, aspect, Near, Far);
    }

    // THREE.Color.setHSL defaults to its linear working color space. The source
    // pulse deliberately remains active when the decorative sway is disabled.
    public static MoveColor RingColor(double seconds)
    {
        const double hue = 0.53, saturation = 1;
        double lightness = 0.55 + Math.Sin(seconds * 2) * 0.08;
        double p = lightness <= 0.5 ? lightness * (1 + saturation) : lightness + saturation - lightness * saturation;
        double q = 2 * lightness - p;
        double Channel(double h)
        {
            if (h < 0) h++;
            if (h > 1) h--;
            return h < 1d / 6 ? q + (p - q) * 6 * h : h < 0.5 ? p : h < 2d / 3 ? q + (p - q) * 6 * (2d / 3 - h) : q;
        }
        return new(Channel(hue + 1d / 3), Channel(hue), Channel(hue - 1d / 3));
    }
}

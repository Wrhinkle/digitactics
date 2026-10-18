namespace DigiTactics.Presentation;

public readonly record struct EmoteTransform(double Scale, double X, double Y, double Opacity);
public readonly record struct EmoteAnchor(bool Visible, double X, double Y);

/// <summary>Original sticker entry/exit CSS transforms and arena edge constraints.</summary>
public static class EmoteLayout
{
    private static double Bezier(double progress, double x1, double y1, double x2, double y2)
    {
        static double Curve(double t, double a, double b) => 3 * (1 - t) * (1 - t) * t * a + 3 * (1 - t) * t * t * b + t * t * t;
        double lo = 0, hi = 1;
        for (int i = 0; i < 40; i++) { double t = (lo + hi) / 2; if (Curve(t, x1, x2) < progress) lo = t; else hi = t; }
        return Curve((lo + hi) / 2, y1, y2);
    }
    public static EmoteTransform Transform(double age, bool row)
    {
        if (age < 0 || age >= 2600) return new(1, 0, 0, 0);
        if (age > 2300)
        {
            double p = Bezier((age - 2300) / 300, .42, 0, 1, 1), scale = 1 - .4 * p;
            return new(scale, 0, -16 * p * scale, 1 - p);
        }
        if (age >= 350) return new(1, 0, 0, 1);
        double entry = Bezier(age / 350, .2, 1.6, .4, 1), s = (row ? .3 : .2) + (row ? .7 : .8) * entry;
        return new(s, row ? 30 * (1 - entry) : 0, row ? 0 : 20 * (1 - entry) * s, Math.Clamp(entry, 0, 1));
    }
    public static EmoteAnchor Anchor(double x, double y, bool behind, double width, double height) =>
        new(!behind && x > -20 && x < width + 20 && y > -40 && y < height + 60,
            Math.Max(58, Math.Min(width - 58, x)), Math.Max(118, Math.Min(height - 4, y)));
}

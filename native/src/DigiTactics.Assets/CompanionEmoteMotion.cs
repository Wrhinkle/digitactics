namespace DigiTactics.Assets;

/// <summary>Original emote body offsets, including entry/exit easing and the flip pivot.</summary>
public static class CompanionEmoteMotion
{
    public static CompanionTransform? Evaluate(string move, double elapsedMs, double durationMs)
    {
        double k = elapsedMs / durationMs, t = elapsedMs / 1000;
        if (!double.IsFinite(k) || k < 0 || k >= 1) return null;
        double x = 0, y = 0, z = 0, rx = 0, ry = 0, rz = 0, sy = 1;
        switch (move)
        {
            case "hop": y = Math.Abs(Math.Sin(t * 9)) * .5; sy = 1 + .1 * Math.Cos(t * 18); break;
            case "spin": ry = k * Math.PI * 4; y = Math.Abs(Math.Sin(t * 6)) * .15; break;
            case "wiggle": rz = Math.Sin(t * 14) * .35; x = Math.Sin(t * 7) * .15; break;
            case "slide": x = Math.Sin(t * 3.6) * .8; ry = Math.Cos(t * 3.6) * 1.1; y = Math.Abs(Math.Sin(t * 7.2)) * .06; break;
            case "flip": double phase = k * 2 % 1; y = Math.Sin(phase * Math.PI) * 1.1; rx = -phase * Math.PI * 2; y += .7 - .7 * Math.Cos(rx); z = -.7 * Math.Sin(rx); break;
            case "happy": y = Math.Abs(Math.Sin(t * 8)) * .3; break;
            case "sad": rx = .4 * Math.Min(1, Math.Min(k * 5, (1 - k) * 5)); rz = Math.Sin(t * 3) * .06; break;
            case "taunt": ry = Math.Sin(t * 16) * .5; y = Math.Abs(Math.Sin(t * 8)) * .08; break;
            case "eat": rx = (Math.Sin(t * 14) * .5 + .5) * .4; break;
            case "wave": rz = Math.Sin(t * 6) * .22; break;
            default: return null;
        }
        if (move is "hop" or "wiggle" or "slide" or "happy" or "taunt" or "eat" or "wave")
        {
            double ease = Math.Min(1, Math.Min(k * 8, (1 - k) * 8));
            x *= ease; y *= ease; rx *= ease; ry *= ease; rz *= ease; sy = 1 + (sy - 1) * ease;
        }
        return new(new(x, y, z), new(rx, ry, rz), new(1, sy, 1));
    }
}

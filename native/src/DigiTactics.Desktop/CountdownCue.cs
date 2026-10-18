namespace DigiTactics.Desktop;

internal static class CountdownCue
{
    public static string? Select(int? previous, double seconds)
    {
        int remaining = (int)Math.Ceiling(Math.Max(0,seconds));
        return remaining != previous && remaining is > 0 and <= 5 ? remaining == 1 ? "tick_last" : "tick_normal" : null;
    }
}

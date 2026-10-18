namespace DigiTactics.Presentation;

/// <summary>The client.js sfx gate uses wall time and a shared 70 ms quiet-event limit.
/// Nonquiet equip/start/reward sounds bypass the preference, but advance the same clock.</summary>
public sealed class ClientMenuSoundGate
{
    private double last;
    public bool TryPlay(double milliseconds, bool uiSfx, bool quiet)
    {
        if (quiet && (!uiSfx || milliseconds - last < 70)) return false;
        last = milliseconds; return true;
    }
}

public static class ClientMotion
{
    public static double PedestalYaw(double seconds, bool enabled) => enabled ? Math.Sin(seconds * .45) * .35 : 0;
    // The source reduce-motion CSS changes decorative animations/transitions to one 1 ms iteration.
    public static double CssDuration(double seconds, bool enabled) => enabled ? seconds : .001;
}

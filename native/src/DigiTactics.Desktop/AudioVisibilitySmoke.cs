using Brutal.GlfwApi;

namespace DigiTactics.Desktop;

/// <summary>Exercise the production window visibility binding using real minimize/restore events.</summary>
internal sealed class AudioVisibilitySmoke
{
    private readonly System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
    private int step, cycles;
    private double since;
    private (bool Muted, bool MusicOff, float Music, float Effects, float Board) settings;
    public bool Verify(GlfwWindow window, GameAudio audio)
    {
        if (clock.Elapsed.TotalSeconds > 10) throw new InvalidOperationException($"Window audio visibility timed out at step {step}.");
        double now = clock.Elapsed.TotalSeconds;
        switch (step)
        {
            case 0:
                if (!audio.Available) throw new InvalidOperationException(audio.Error);
                settings = (audio.Muted,audio.MusicOff,audio.MusicVolume,audio.EffectsVolume,audio.BoardVolume);
                audio.Music("planning"); since = now; step = 1; break;
            case 1 when now - since > .15:
                if (window.IsIconified || audio.Suspended) throw new InvalidOperationException("Visible window unexpectedly suspended audio.");
                window.Iconify(); step = 2; break;
            case 2 when window.IsIconified:
                if (!audio.Suspended) throw new InvalidOperationException("Minimize event did not suspend audio through the match loop.");
                since = now; step = 3; break;
            case 3 when now - since > .15:
                if (!audio.Suspended) throw new InvalidOperationException("Minimized window resumed audio early.");
                window.Restore(); step = 4; break;
            case 4 when !window.IsIconified:
                if (audio.Suspended || settings != (audio.Muted,audio.MusicOff,audio.MusicVolume,audio.EffectsVolume,audio.BoardVolume))
                    throw new InvalidOperationException("Restore failed to resume audio or changed preferences.");
                if (++cycles == 2)
                {
                    Console.WriteLine("Window audio visibility passed: two real GLFW minimize/restore cycles preserve preferences and suspend/resume SDL.");
                    return true;
                }
                since = now; step = 1; break;
        }
        Thread.Sleep(2);
        return false;
    }
}

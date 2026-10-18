using System.Diagnostics;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed unsafe partial class GameAudio
{
    private readonly ClientMenuSoundGate clientSoundGate = new();
    private readonly Stopwatch clientSoundClock = Stopwatch.StartNew();
    public bool MenuSounds { get; set; } = true;
    public void PlayClient(string id, bool quiet = true)
    {
        if (clientSoundGate.TryPlay(clientSoundClock.Elapsed.TotalMilliseconds, MenuSounds, quiet))
            Play(id == "tick" ? "tick_normal" : id);
    }
}

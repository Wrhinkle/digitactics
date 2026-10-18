using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    // client.js mounts the same Stage on Home and Collection, retaining its yaw.
    private readonly PedestalInteraction legendPedestal=new();
}

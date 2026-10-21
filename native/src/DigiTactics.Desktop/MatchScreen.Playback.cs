namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    // The top bar's "1x" control cycles this (MatchScreen.TopBar.cs).
    internal int ReplaySpeed { get; private set; } = 1;
}

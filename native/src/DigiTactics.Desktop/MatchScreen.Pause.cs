using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool localPaused;
    internal bool IsPaused => localPaused && networkRoom is null && Session is not null;
    private void PauseLocal()
    {
        if (networkRoom is not null || Session is null || Session.State.Over) return;
        CancelArenaDrag(); cameraDrag = null; localPaused = true;
    }
    private void PausePanel()
    {
        Window("Game paused", size.X / 2 - 190, size.Y / 2 - 85, 380, 170);
        UiText.Wrapped("Your local match is paused. Resume whenever you are ready."u8);
        if (Button("resume", "Resume game", 220, 40) || ImGui.IsKeyPressed(ImGuiKey.Escape, false)) localPaused = false;
        ImGui.End();
    }
}

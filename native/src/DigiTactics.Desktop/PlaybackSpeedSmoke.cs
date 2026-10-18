using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal sealed class PlaybackSpeedSmoke
{
    private int step;
    private bool down;
    private bool released;
    internal bool Done => step == 3;
    internal void Input(MatchScreen screen)
    {
        if (down)
        {
            ImGui.GetIO().AddMouseButtonEvent(0, false);
            down = false;
            released = true;
            return;
        }
        if (released)
        {
            released = false;
            int expected = step == 0 ? 2 : step == 1 ? 4 : 1;
            if (screen.ReplaySpeed != expected) throw new InvalidOperationException("Battle speed button did not cycle 1x/2x/4x.");
            step++;
            return;
        }
        if (!screen.Buttons.TryGetValue("replaySpeed", out var target)) throw new InvalidOperationException("Battle speed control is missing.");
        ImGui.GetIO().AddMousePosEvent(target.X, target.Y);
        ImGui.GetIO().AddMouseButtonEvent(0, true);
        down = true;
    }
}

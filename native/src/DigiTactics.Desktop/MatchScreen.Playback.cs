using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal int ReplaySpeed { get; private set; } = 1;
    private void PlaybackSpeedControl()
    {
        ImGui.SameLine();
        if (Button("replaySpeed", $"{ReplaySpeed}x",50,36)) ReplaySpeed = ReplaySpeed == 1 ? 2 : ReplaySpeed == 2 ? 4 : 1;
        if (ImGui.IsItemHovered()) { ImGui.BeginTooltip(); ImGui.Text("Battle playback speed"u8); ImGui.EndTooltip(); }
    }
}

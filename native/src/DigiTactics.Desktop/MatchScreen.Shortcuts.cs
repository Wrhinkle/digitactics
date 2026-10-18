using Brutal.ImGuiApi;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private int? hoveredOwnedUnit;
    private void MatchShortcuts()
    {
        if (Session is null || Session.State.Over || IsSpectating || !Human.Alive || ImGui.GetIO().WantTextInput || showAudio || showPlayers) return;
        if (ImGui.IsKeyPressed(ImGuiKey.D, false)) Act("reroll");
        if (ImGui.IsKeyPressed(ImGuiKey.F, false)) Act("buyXp");
        if (ImGui.IsKeyPressed(ImGuiKey.E, false) && hoveredOwnedUnit is { } uid && !arenaDragMoved && !itemDragMoved)
        {
            Act("sell", new { uid });
            if (selectedUnit == uid) selectedUnit = null;
        }
        if (ImGui.IsKeyPressed(ImGuiKey.Space, false) && Session.Rounds.Phase == MatchPhase.Planning) Fight();
    }
}

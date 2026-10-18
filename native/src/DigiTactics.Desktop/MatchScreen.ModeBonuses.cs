using Brutal.ImGuiApi;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal CombatPower? DisplayedPower { get; private set; }
    internal IReadOnlyList<string> DisplayedGlobals { get; private set; } = [];

    private void ModeBonusSidebar(MatchPlayer player)
    {
        DisplayedPower = player.Power;
        DisplayedGlobals = player.Globals.ToArray();
        if (player.Power is { } power)
        {
            ImGui.SeparatorText("Tamer power"u8);
            var description = text.Get(Rules.Id, power.Id);
            UiText.Wrapped($"{description.Name} - rank {power.Rank}");
            UiText.Wrapped(description.Description);
        }
        if (DisplayedGlobals.Count == 0) return;
        ImGui.SeparatorText("Global bonuses"u8);
        foreach (string id in DisplayedGlobals)
        {
            var description = text.Get(Rules.Id, id);
            if (ItemArtwork.Has(Rules.Id, id))
            {
                var position = ImGui.GetCursorScreenPos(); ImGui.Dummy(new(30,30));
                itemArtwork.Draw(Rules.Id, id, position, 30);
                ImGui.SameLine();
            }
            UiText.Wrapped(description.Name);
            UiText.Wrapped(description.Description);
            ImGui.Separator();
        }
    }
}

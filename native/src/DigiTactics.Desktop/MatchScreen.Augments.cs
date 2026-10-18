using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal IReadOnlyList<string> DisplayedAugments { get; private set; } = [];
    internal int AugmentPortraits { get; private set; }

    private void AugmentSidebar(MatchPlayer player)
    {
        DisplayedAugments = player.Augments.ToArray();
        AugmentPortraits = 0;
        if (DisplayedAugments.Count == 0) return;
        ImGui.SeparatorText($"Augments - {DisplayedAugments.Count}/3");
        foreach (string id in DisplayedAugments)
        {
            var description = text.Get(Rules.Id, id);
            var definition = Rules.Offers!.Augments.Concat(Rules.Offers.DuoAugments).Single(augment => augment.Id == id);
            var color = definition.Tier switch
            {
                1 => new ImColor8(201,209,220),
                2 => new ImColor8(255,207,74),
                _ => new ImColor8(229,139,255)
            };
            if (ItemArtwork.Has(Rules.Id, "aug:" + id))
            {
                var position = ImGui.GetCursorScreenPos(); ImGui.Dummy(new(30,30));
                itemArtwork.Draw(Rules.Id, "aug:" + id, position, 30);
                AugmentPortraits++;
                ImGui.SameLine();
            }
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            UiText.Wrapped(description.Name);
            ImGui.PopStyleColor();
            UiText.Wrapped(description.Description);
            ImGui.Separator();
        }
    }
}

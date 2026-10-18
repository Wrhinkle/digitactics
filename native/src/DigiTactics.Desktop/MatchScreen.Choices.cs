using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private PlayerOffer? displayedOffer;
    private bool peekChoices;
    internal bool PeekingChoices => peekChoices;

    private void Choices()
    {
        if (IsSpectating) return;
        if (Session!.Units.Carousel.Active) { CarouselChoices(); return; }
        var offer = Human.Offer;
        if (!ReferenceEquals(offer, displayedOffer)) { displayedOffer = offer; peekChoices = false; }
        if (offer is null || Session.Rounds.Phase != MatchPhase.Planning) return;
        var heading = text.Offer(Rules.Id, offer.Kind);
        Window(heading.Name + "##reward", Left + 12, Top + 12, Width - 24, peekChoices ? 65 : Math.Min(Height - 24, 440));
        if (Button("peekChoices", peekChoices ? "Show choices" : "View board")) peekChoices = !peekChoices;
        if (peekChoices) { ImGui.End(); return; }
        ImGui.TextWrapped(heading.Description);
        ImGui.Separator();
        for (int i = 0; i < offer.Options.Length; i++)
        {
            string id = offer.Options[i];
            var choice = text.Choice(Rules, offer.Kind, id);
            var item = offer.Kind is "armory" or "itemTier" ? Rules.Items.FirstOrDefault(item => item.Id == id) : null;
            bool pick;
            if (item is not null)
            {
                pick = Button("offer/" + i, "", Width - 65, 52);
                var min = ImGui.GetItemRectMin(); var max = ImGui.GetItemRectMax(); var draw = ImGui.GetWindowDrawList();
                draw.PushClipRect(min, max, true);
                itemArtwork.Draw(Rules.Id, id, new(min.X + 4, min.Y + 4), 44);
                draw.AddText(ImGui.GetFont(), 15, new(min.X + 58, min.Y + 17), new ImColor8(235, 240, 248), choice.Name);
                draw.PopClipRect();
            }
            else pick = Button("offer/" + i, choice.Name, Width - 65, 34);
            if (pick) { Act("offer", new { i }); break; }
            ImGui.TextWrapped(choice.Description.Replace("%", "%%"));
            if (item is { Component: false, Ingredients: { Length: > 0 } ingredients })
                ImGui.TextWrapped("From " + string.Join(" + ", ingredients.Select(Name)));
            ImGui.Separator();
        }
        if (offer.Rerolls > 0 && Button("offerReroll", $"Reroll choices ({offer.Rerolls})")) Act("offerReroll");
        ImGui.End();
    }
}

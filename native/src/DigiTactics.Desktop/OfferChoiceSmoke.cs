using Brutal.ImGuiApi;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Seed an armory fixture, then exercise its actual UI and reward action.</summary>
internal sealed class OfferChoiceSmoke
{
    private int step, count;
    private PlayerOffer? offer;
    public bool Capture => step == 7;
    public bool Input(MatchScreen screen)
    {
        var session = screen.Session!; var player = session.State.Players[0]; var io = ImGui.GetIO();
        void Click(string key) { var point = screen.Buttons[key]; io.AddMousePosEvent(point.X, point.Y); io.AddMouseButtonEvent(0, true); }
        switch (step++)
        {
            case 0:
                // Faction Wars has no scheduled armory; seed only the test reward, then use real UI/actions.
                offer = new("armory", session.State.Rules.Items.Where(item => item.Component).Take(3).Select(item => item.Id).ToArray(), 0, null);
                typeof(MatchPlayer).GetProperty(nameof(MatchPlayer.Offer))!.SetValue(player, offer);
                count = player.Items.Count; break;
            case 1: Click("peekChoices"); break;
            case 2: io.AddMouseButtonEvent(0, false); break;
            case 3:
                if (!screen.PeekingChoices || !ReferenceEquals(offer, player.Offer) || player.Items.Count != count || screen.Buttons.ContainsKey("offer/0"))
                    throw new InvalidOperationException("Peeking changed the reward or retained hidden choice controls.");
                Click("peekChoices"); break;
            case 4: io.AddMouseButtonEvent(0, false); break;
            case 5:
                if (screen.PeekingChoices || !ReferenceEquals(offer, player.Offer)) throw new InvalidOperationException("Showing choices lost the pending reward.");
                break;
            case 6: break;
            case 7: Click("offer/0"); break;
            case 8: io.AddMouseButtonEvent(0, false); break;
            case 9:
                if (player.Offer is not null || player.Items.Count != count + 1 || player.Items[^1] != offer!.Options[0])
                    throw new InvalidOperationException("Reward selection did not grant exactly the selected item.");
                return true;
        }
        return false;
    }
}

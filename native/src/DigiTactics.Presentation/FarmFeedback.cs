using System.Text.Json.Nodes;
using DigiTactics.Storage;

namespace DigiTactics.Presentation;

public static class FarmFeedback
{
    /// <summary>Original care sounds, called only after the action receipt is saved.</summary>
    public static string? Sound(FarmCareAction action, JsonObject receipt)
    {
        bool Flag(string name) => receipt[name]?.GetValue<bool>() == true;
        if (action == FarmCareAction.Pet) return "emote";
        if (!Flag("ok")) return action == FarmCareAction.Feed && Flag("refused") ? "error" : null;
        return action switch
        {
            FarmCareAction.Feed or FarmCareAction.Praise => "pick",
            FarmCareAction.Clean or FarmCareAction.CleanPile => "sell",
            FarmCareAction.Cure => "heal",
            FarmCareAction.Lights => Flag("lights") ? "equip" : "place",
            FarmCareAction.Scold => Flag("unfair") ? "error" : "place",
            FarmCareAction.Play => "emote",
            _ => null
        };
    }
}

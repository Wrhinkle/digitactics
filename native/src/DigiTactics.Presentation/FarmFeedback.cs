using System.Text.Json.Nodes;
using DigiTactics.Storage;

namespace DigiTactics.Presentation;

public enum FarmSymbol { Meat, Carrot, BigMeat, Sirloin, Cake, Refused, Medicine, Bandage, Spoiled, Heart, Tear, Exclaim, Sleep, Poop, Mistake, Tantrum, Toilet, Music }

public static class FarmFeedback
{
    public static FarmSymbol? Symbol(FarmCareAction action, string? option, JsonObject receipt, bool asleep)
    {
        bool Flag(string name) => receipt[name]?.GetValue<bool>() == true;
        if (action == FarmCareAction.Pet) return asleep ? FarmSymbol.Sleep : Flag("small") ? null : FarmSymbol.Heart;
        if (!Flag("ok")) return action == FarmCareAction.Feed && Flag("refused") ? FarmSymbol.Refused : null;
        return action switch
        {
            FarmCareAction.Feed => option switch { "carrot" => FarmSymbol.Carrot, "bigmeat" => FarmSymbol.BigMeat, "sirloin" => FarmSymbol.Sirloin, "cake" => FarmSymbol.Cake, _ => FarmSymbol.Meat },
            FarmCareAction.Cure => option == "medicine" ? FarmSymbol.Medicine : FarmSymbol.Bandage,
            FarmCareAction.Praise => Flag("spoiled") ? FarmSymbol.Spoiled : FarmSymbol.Heart,
            FarmCareAction.Scold => Flag("unfair") ? FarmSymbol.Tear : FarmSymbol.Exclaim,
            _ => null
        };
    }
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

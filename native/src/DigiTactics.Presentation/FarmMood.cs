using System.Text.Json.Nodes;

namespace DigiTactics.Presentation;

public sealed record FarmMood(int Icon, string Text)
{
    public static FarmMood Read(JsonNode pet, bool lights, double ideal)
    {
        double Number(string key) => double.Parse(pet[key]!.ToString(), System.Globalization.CultureInfo.InvariantCulture);
        if (pet["flags"]?["asleep"]?.GetValue<bool>() == true) return new(0, "Sleeping");
        if (pet["sick"]?.GetValue<bool>() == true) return new(1, "Sick");
        if (Number("tantrum") != 0) return new(2, "Throwing a tantrum");
        if (Number("hunger") < 20) return new(3, "Hungry");
        if (lights && Number("energy") < 20) return new(4, "Sleepy");
        if (pet["poop"]!.AsArray().Count >= 2) return new(5, "Wants a clean room");
        if (Number("happy") < 25) return new(6, "Sad");
        if (Number("weight") >= ideal * 1.4) return new(7, "Chubby");
        if (Number("happy") >= 80) return new(8, "Happy");
        return new(9, "Content");
    }
}

using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed record FarmCaretakerPlan(string Id, string Name, int Price, int Needs);
public sealed record FarmCaretakerQuote(int Price, long Until, bool Upgrade, bool Blocked);

/// <summary>Explicit prepaid upkeep, with a monotonic clock and no automatic charges or rewards.</summary>
public static class FarmCaretaker
{
    public const long Day = 86400000;
    public static readonly FarmCaretakerPlan[] Plans = [new("basic", "Basic caretaker", 100, 40), new("full", "Full care", 750, 100)];
    public static long Time(JsonNode farm, long timestamp) => Math.Max(timestamp, farm["caretaker"]?["clock"]?.GetValue<long>() ?? 0);
    public static long Until(JsonNode farm) => farm["caretaker"]?["until"]?.GetValue<long>() ?? 0;
    public static FarmCaretakerPlan? At(JsonNode farm, long timestamp)
        => farm["caretaker"] is JsonObject service && timestamp >= service["since"]!.GetValue<long>() && timestamp < service["until"]!.GetValue<long>()
            ? Plans.FirstOrDefault(plan => plan.Id == service["tier"]!.GetValue<string>()) : null;
    public static FarmCaretakerPlan? Current(JsonNode farm, long timestamp = 0) => At(farm, Time(farm, timestamp));
    public static FarmCaretakerQuote Quote(JsonNode farm, FarmCaretakerPlan plan, long timestamp)
    {
        long time = Time(farm, timestamp), until = Until(farm); var current = Current(farm, time);
        if (current is not null && current.Price > plan.Price) return new(0, until, false, true);
        if (current is not null && current.Price < plan.Price)
            return new(checked((int)Math.Ceiling((plan.Price - current.Price) * ((until - time) / (double)Day))), until, true, false);
        return new(plan.Price, checked(Math.Max(time, until) + Day), false, false);
    }
    private static double Number(JsonNode pet, string field)
    {
        var value = pet[field]!.AsValue();
        if (value.TryGetValue<double>(out double number)) return number;
        if (value.TryGetValue<int>(out int integer)) return integer;
        return value.GetValue<long>();
    }
    internal static void Maintain(JsonNode pet, FarmCaretakerPlan? plan, double ideal, bool lights)
    {
        if (plan is null) return;
        foreach (string field in new[] { "hunger", "energy", "happy" })
            pet[field] = Math.Max(Number(pet, field), plan.Needs);
        if (plan.Id == "full") { pet["disc"] = 100; pet["weight"] = ideal; pet["nap"] = false; }
        pet["poop"]!.AsArray().Clear();
        foreach (string field in new[] { "dirt", "sickMin", "starveMin", "sadMin", "sleepyMin", "tantrum" }) pet[field] = 0;
        pet["sick"] = false; pet["sickWhy"] = "";
        pet["flags"]!["asleep"] = !lights && Number(pet, "energy") < 100 || pet["nap"]!.GetValue<bool>();
    }
}

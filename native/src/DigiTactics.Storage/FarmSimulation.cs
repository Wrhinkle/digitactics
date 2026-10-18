using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed record FarmUpdate(string Save, JsonArray Events);

/// <summary>Pure minute-by-minute farm catch-up. Callers choose when to persist the resulting snapshot.</summary>
public static class FarmSimulation
{
    private const long Minute = 60000;
    private static double N(JsonNode node, string key)
    {
        if (node[key] is not JsonValue value) throw new InvalidDataException("Missing farm value: " + key);
        if (value.TryGetValue<double>(out double number)) return number;
        if (value.TryGetValue<int>(out int integer)) return integer;
        return value.GetValue<long>();
    }
    private static bool B(JsonNode node, string key) => node[key]?.GetValue<bool>() ?? throw new InvalidDataException("Missing farm flag: " + key);
    private static void Add(JsonNode node, string key, double amount) => node[key] = N(node, key) + amount;
    private static double Ideal(string? legends, string id)
    {
        var (_, record) = LegendRewards.Snapshot(legends, id);
        return new[] { 10, 20, 30, 40 }[Math.Clamp(LegendEvolution.Form(record["form"]!.GetValue<string>()).Tier, 0, 3)];
    }
    private static JsonObject Pet(string id, long time, double ideal)
    {
        var result = new JsonObject { ["born"] = time, ["t"] = time, ["hunger"] = 80, ["energy"] = 90, ["happy"] = 70, ["disc"] = 30,
            ["weight"] = ideal, ["poop"] = new JsonArray(), ["poopClock"] = Math.Floor(LegendEvolution.SeededRandom(id) * 90),
            ["sick"] = false, ["sickWhy"] = "", ["nap"] = false, ["flags"] = new JsonObject() };
        foreach (string key in new[] { "dirt", "sickMin", "mistakes", "poopNeglect", "overfeed", "refusals", "goodBond", "goodFood", "str", "spd", "wis", "sta", "trained", "tantrum", "starveMin", "sadMin", "sleepyMin", "lastPraise", "lastPet", "toilet", "pileN" }) result[key] = 0;
        return result;
    }
    internal static JsonObject NewPet(string id, long time, string? legends) => Pet(id, time, Ideal(legends, id));
    public static FarmUpdate Advance(string? farmJson, string? walletJson, string? legendJson, long timestamp)
    {
        var save = new JsonObject { ["v"] = 1, ["cap"] = 6, ["theme"] = "meadow", ["themes"] = new JsonArray("meadow"),
            ["residents"] = new JsonArray(), ["seen"] = new JsonArray(), ["lights"] = true, ["pets"] = new JsonObject(),
            ["meat"] = new JsonObject { ["n"] = 3, ["t"] = 0 }, ["bandage"] = new JsonObject { ["n"] = 2, ["t"] = 0 },
            ["daily"] = new JsonObject { ["day"] = "", ["bits"] = 0, ["data"] = 0 } };
        if (farmJson is not null)
        {
            var previous = JsonNode.Parse(farmJson) as JsonObject ?? throw new InvalidDataException("Invalid farm save.");
            foreach (var field in previous) save[field.Key] = field.Value?.DeepClone();
        }
        if (save["v"]?.GetValue<int>() != 1) throw new InvalidDataException("Unsupported farm save version.");
        timestamp = FarmCaretaker.Time(save, timestamp);
        if (save["caretaker"] is JsonObject service) service["clock"] = timestamp;
        if (save["themes"] is not JsonArray || save["residents"] is not JsonArray || save["seen"] is not JsonArray || save["pets"] is not JsonObject || save["meat"] is not JsonObject || save["bandage"] is not JsonObject || save["daily"] is not JsonObject daily || daily["day"] is null)
            throw new InvalidDataException("Invalid farm save fields.");
        var collection = new CosmeticCollection(walletJson);
        var owned = CosmeticCollection.Items("legend").Where(x => collection.Owns("legend", x.Id)).Select(x => x.Id).ToArray();
        int cap = (int)N(save, "cap"); cap = Math.Clamp(cap == 0 ? 6 : cap, 6, 12); save["cap"] = cap;
        var residents = save["residents"]!.AsArray().Select(x => x!.GetValue<string>()).Where(owned.Contains).Distinct().Take(cap).ToList();
        var seen = save["seen"]!.AsArray(); string? equipped = collection.Equipped("legend")?.GetValue<string>();
        foreach (string id in owned.OrderByDescending(id => id == equipped))
        {
            if (seen.Any(x => x?.GetValue<string>() == id)) continue;
            seen.Add(id); if (residents.Count < cap && !residents.Contains(id)) residents.Add(id);
        }
        save["residents"] = new JsonArray(residents.Select(id => (JsonNode?)JsonValue.Create(id)).ToArray());
        var pets = save["pets"]!.AsObject();
        foreach (string id in residents) if (pets[id] is null) pets[id] = Pet(id, timestamp, Ideal(legendJson, id));
        void Stock(string key, int max, int every)
        {
            var stock = save[key]!;
            if (N(stock, "t") == 0) stock["t"] = timestamp;
            if (N(stock, "n") >= max) { stock["t"] = timestamp; return; }
            double add = Math.Floor((timestamp - N(stock, "t")) / (every * Minute));
            if (add <= 0) return;
            stock["n"] = Math.Min(max, N(stock, "n") + add);
            stock["t"] = N(stock, "n") >= max ? timestamp : N(stock, "t") + add * every * Minute;
        }
        Stock("meat", 3, 120); Stock("bandage", 2, 360);
        string day = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime.ToString("yyyy-M-d", System.Globalization.CultureInfo.InvariantCulture);
        if (save["daily"]!["day"]!.GetValue<string>() != day) save["daily"] = new JsonObject { ["day"] = day, ["bits"] = 0, ["data"] = 0 };
        var events = new JsonArray();
        void Emit(string id, string type, string? why = null)
        {
            var value = new JsonObject { ["id"] = id, ["type"] = type }; if (why is not null) value["why"] = why;
            events.Add(value); if (events.Count > 200) events.RemoveAt(0);
        }
        bool lights = B(save, "lights");
        foreach (string id in residents)
        {
            var p = pets[id]!;
            if (p["poop"] is not JsonArray || p["flags"] is not JsonObject) throw new InvalidDataException("Invalid farm pet fields.");
            if (N(p, "t") == 0) p["t"] = timestamp;
            double ideal = Ideal(legendJson, id);
            // Supplies and age use wall time; unattended biological decline is bounded to two hours.
            double from = Math.Max(N(p, "t"), timestamp - 120 * Minute);
            // If paid time ended before the retained window, still apply its final upkeep before unattended decay.
            if (save["caretaker"] is JsonObject coverage && N(p,"t") < FarmCaretaker.Until(save) && from >= FarmCaretaker.Until(save))
                FarmCaretaker.Maintain(p, FarmCaretaker.At(save, FarmCaretaker.Until(save) - 1), ideal, lights);
            int minutes = (int)Math.Floor((timestamp - from) / Minute);
            if (minutes <= 0) { FarmCaretaker.Maintain(p, FarmCaretaker.Current(save, timestamp), ideal, lights); continue; }
            int budget = minutes > 30 ? 2 : 99;
            void Mistake(string why) { if (budget <= 0) return; budget--; Add(p, "mistakes", 1); Emit(id, "mistake", why); }
            var poop = p["poop"]!.AsArray();
            for (int m = 0; m < minutes; m++)
            {
                double time = from + m * Minute; long minute = (long)Math.Floor(time / Minute);
                var caretaker = FarmCaretaker.At(save, (long)time);
                FarmCaretaker.Maintain(p, caretaker, ideal, lights);
                if (!lights) p["nap"] = false;
                else if (N(p, "energy") <= 5) p["nap"] = true;
                else if (B(p, "nap") && N(p, "energy") >= 50) p["nap"] = false;
                bool asleep = !lights && N(p, "energy") < 100 || B(p, "nap"); p["flags"]!["asleep"] = asleep;
                Add(p, "energy", asleep ? lights ? 100d / 240 : 100d / 120 : -100d / 720);
                Add(p, "hunger", asleep ? -(100d / 600) / 2 : -100d / 600);
                double dh = asleep ? 100d / 2400 : 100d / 900;
                if (N(p, "hunger") < 20) dh += 1d / 6;
                if (B(p, "sick")) dh += 1d / 6;
                dh += poop.Count * (1d / 20);
                if (N(p, "weight") >= ideal * 1.4) dh += 1d / 30;
                if (!lights && !asleep) dh += 1d / 10;
                Add(p, "happy", -dh); if (caretaker is null) Add(p, "disc", -100d / 3000);
                foreach (string field in new[] { "energy", "hunger", "happy", "disc" }) p[field] = Math.Clamp(N(p, field), 0, 100);
                FarmCaretaker.Maintain(p, caretaker, ideal, lights);
                if (N(p, "hunger") <= 0)
                {
                    Add(p, "starveMin", 1);
                    if (N(p, "starveMin") == 60) Mistake("hungry");
                    if (N(p, "starveMin") == 180 && !B(p, "sick")) { p["sick"] = true; p["sickWhy"] = "hungry"; p["sickMin"] = 0; Emit(id, "sick", "hungry"); }
                    if (N(p, "starveMin") % 120 == 0) p["weight"] = Math.Max(Math.Floor(ideal * .5 + .5), N(p, "weight") - 1);
                }
                else p["starveMin"] = 0;
                if (N(p, "happy") <= 0) { Add(p, "sadMin", 1); if (N(p, "sadMin") == 120) Mistake("sad"); } else p["sadMin"] = 0;
                if (lights && !B(p, "nap") && N(p, "energy") < 20) { Add(p, "sleepyMin", 1); if (N(p, "sleepyMin") == 60) Mistake("lights"); }
                else if (!lights || N(p, "energy") >= 20) p["sleepyMin"] = 0;
                Add(p, "poopClock", asleep ? .5 : 1);
                if (N(p, "poopClock") >= 180)
                {
                    p["poopClock"] = 0;
                    if (caretaker is not null) { /* The caretaker handles it without granting care progress. */ }
                    else if (N(p, "disc") >= 60 && !asleep) { Add(p, "toilet", 1); p["disc"] = Math.Clamp(N(p, "disc") + 1, 0, 100); Emit(id, "toilet"); }
                    else if (poop.Count < 4)
                    {
                        double a = LegendEvolution.SeededRandom($"{id}|pa|{minute}") * Math.PI * 2, r = 1.2 + LegendEvolution.SeededRandom($"{id}|pr|{minute}") * 2.6;
                        Add(p, "pileN", 1);
                        poop.Add(new JsonObject { ["t"] = time, ["x"] = Math.Round(Math.Cos(a) * r, 2, MidpointRounding.AwayFromZero), ["z"] = Math.Round(Math.Sin(a) * r * .8 + .4, 2, MidpointRounding.AwayFromZero), ["n"] = false, ["k"] = N(p, "pileN") });
                        Emit(id, "poop");
                    }
                }
                Add(p, "dirt", poop.Count);
                foreach (var pile in poop) if (!B(pile!, "n") && time - N(pile!, "t") >= 180 * Minute) { pile!["n"] = true; Add(p, "poopNeglect", 1); Emit(id, "neglect"); }
                if (!B(p, "sick") && N(p, "dirt") >= 240) { p["sick"] = true; p["sickWhy"] = "dirty"; p["sickMin"] = 0; Emit(id, "sick", "dirty"); }
                if (B(p, "sick")) { Add(p, "sickMin", 1); if (N(p, "sickMin") == 360) Mistake("sick"); }
                if (N(p, "tantrum") != 0 && time >= N(p, "tantrum")) { p["tantrum"] = 0; Mistake("tantrum"); }
                else if (caretaker is null && N(p, "tantrum") == 0 && !asleep && !B(p, "sick") && LegendEvolution.SeededRandom($"{id}|tt|{minute}") < .004 * Math.Pow(1 - N(p, "disc") / 100, 2))
                { p["tantrum"] = time + 30 * Minute; Emit(id, "tantrum"); }
            }
            p["t"] = from + minutes * Minute;
            FarmCaretaker.Maintain(p, FarmCaretaker.Current(save, timestamp), ideal, lights);
        }
        return new(save.ToJsonString(), events);
    }
}

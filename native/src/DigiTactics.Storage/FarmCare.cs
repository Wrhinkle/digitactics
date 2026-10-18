using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed record FarmCareUpdate(string Farm, string? Wallet, string? Legends, JsonObject Result, JsonArray Events);
public enum FarmCareAction { Feed, Clean, CleanPile, Cure, Lights, Praise, Scold, Play, Pet, MoveIn, MoveOut, Swap, Expand, BuyTheme, SetTheme, Train }
public sealed record FarmTheme(string Id, string Name, int Price, string? Board = null, bool Free = false);

public static class FarmCare
{
    private static readonly Dictionary<string, (string Name, string Stat)> Training = new() { ["strength"] = ("Strength", "str"), ["speed"] = ("Speed", "spd"), ["wisdom"] = ("Wisdom", "wis"), ["stamina"] = ("Stamina", "sta") };
    public static readonly FarmTheme[] Themes = [new("meadow", "File Island Meadow", 0, Free: true), new("tropical", "Tropical Isle", 300, "file-beach"),
        new("snowy", "Freezeland", 300, "infinity-mountain"), new("sewer", "Numemon Sewer", 200, "dark-ocean"), new("toy", "Toy Town Yard", 400, "toy-town"), new("grid", "Digital Grid", 600, "digital-grid")];
    public static bool OwnsTheme(JsonNode farm, CosmeticCollection collection, FarmTheme theme)
    {
        var themes = farm["themes"] as JsonArray ?? throw new InvalidDataException("Invalid farm themes.");
        return theme.Free || themes.Any(x => x?.GetValue<string>() == theme.Id) || theme.Board is not null && collection.Owns("board", theme.Board);
    }
    public static int ExpansionPrice(int cap) => cap >= 12 ? 0 : cap == 6 ? 250 : cap == 8 ? 400 : 600;
    private static double N(JsonNode node, string key)
    {
        var value = node[key]?.AsValue() ?? throw new InvalidDataException("Missing farm value: " + key);
        if (value.TryGetValue<double>(out double d)) return d;
        if (value.TryGetValue<int>(out int i)) return i;
        return value.GetValue<long>();
    }
    private static bool B(JsonNode node, string key) => node[key]?.GetValue<bool>() == true;
    public static string? TrainingRefusal(JsonNode farm, string? legendJson, string? id)
    {
        if (id is null || !farm["residents"]!.AsArray().Any(x => x?.GetValue<string>() == id)) return "Not in the room.";
        var pet = farm["pets"]![id]!;
        var (_, record) = LegendRewards.Snapshot(legendJson, id);
        string name = LegendEvolution.Form(record["form"]!.GetValue<string>()).Name;
        if (B(pet["flags"]!, "asleep")) return name + " is asleep.";
        if (B(pet, "sick")) return name + " is sick. Cure it first.";
        if (N(pet, "energy") < 15) return name + " is too tired. Turn the lights off for a nap.";
        if (N(pet, "hunger") < 10) return name + " is too hungry to train.";
        if (N(pet, "tantrum") != 0 && N(pet, "disc") < 40) return name + " won't listen. Scold it first.";
        return null;
    }
    private sealed record Food(string Name, int Bits, int Hunger, int Weight, int Happy = 0, int Energy = 0, int Disc = 0, bool Healthy = false, bool Sweet = false);
    private static readonly Dictionary<string, Food> Foods = new() {
        ["meat"] = new("Meat", 0, 30, 1), ["carrot"] = new("Supercarrot", 8, 25, 0, Energy: 6, Healthy: true),
        ["bigmeat"] = new("Giant Meat", 15, 70, 3, Happy: 4), ["sirloin"] = new("Sirloin", 25, 50, 2, Happy: 15, Energy: 10),
        ["cake"] = new("Digi-Cake", 10, 10, 3, Happy: 20, Disc: -5, Sweet: true)
    };
    public static FarmCareUpdate Apply(string? farmJson, string? walletJson, string? legendJson, long timestamp, FarmCareAction action, string? id = null, string? option = null, int pile = 0, bool? lights = null, double score = 0)
    {
        var advanced = (action is FarmCareAction.BuyTheme or FarmCareAction.SetTheme) && farmJson is not null
            ? new FarmUpdate(farmJson, new JsonArray()) : FarmSimulation.Advance(farmJson, walletJson, legendJson, timestamp);
        var farm = JsonNode.Parse(advanced.Save)!.AsObject();
        if (farm["v"]?.GetValue<int>() != 1) throw new InvalidDataException("Unsupported farm save version.");
        var collection = new CosmeticCollection(walletJson);
        var wallet = JsonNode.Parse(collection.Json)!.AsObject(); bool walletChanged = false;
        var residents = farm["residents"]!.AsArray().Select(x => x!.GetValue<string>()).ToArray();
        var pets = farm["pets"]!.AsObject();
        JsonObject? p = id is not null && residents.Contains(id) ? pets[id]!.AsObject() : null;
        static void Add(JsonNode node, string key, double value) => node[key] = N(node, key) + value;
        static void Stat(JsonNode node, string key, double value) => node[key] = Math.Clamp(N(node, key) + value, 0, 100);
        string Name(string petId) { var (_, record) = LegendRewards.Snapshot(legendJson, petId); return LegendEvolution.Form(record["form"]!.GetValue<string>()).Name; }
        double Ideal(string petId) { var (_, record) = LegendRewards.Snapshot(legendJson, petId); return new[] { 10, 20, 30, 40 }[Math.Clamp(LegendEvolution.Form(record["form"]!.GetValue<string>()).Tier, 0, 3)]; }
        bool Spend(int bits)
        {
            long balance = wallet["bits"]!.GetValue<long>(); if (balance < bits) return false;
            wallet["bits"] = balance - bits; walletChanged = true; return true;
        }
        static JsonObject Zero() => new() { ["bits"] = 0, ["data"] = 0 };
        JsonObject Reward(string petId, int bits, int data)
        {
            var daily = farm["daily"]!;
            int b = (int)Math.Clamp(bits, 0, 60 - N(daily, "bits")), x = (int)Math.Clamp(data, 0, 100 - N(daily, "data"));
            if (b > 0)
            {
                Add(daily, "bits", b); wallet["bits"] = checked(wallet["bits"]!.GetValue<long>() + b); walletChanged = true;
                var history = wallet["history"]!.AsArray(); history.Insert(0, new JsonObject { ["n"] = b, ["reason"] = "Digi-Farm", ["at"] = timestamp });
                while (history.Count > 30) history.RemoveAt(history.Count - 1);
            }
            if (x > 0) { var grant = LegendRewards.GrantData(legendJson, petId, x); legendJson = grant.Save; x = grant.Used; Add(daily, "data", x); }
            return new JsonObject { ["bits"] = b, ["data"] = x };
        }
        FarmCareUpdate Done(bool ok, string msg, params (string Key, JsonNode? Value)[] fields)
        {
            var result = new JsonObject { ["ok"] = ok, ["msg"] = msg }; foreach (var field in fields) result[field.Key] = field.Value;
            return new(farm.ToJsonString(), walletChanged ? wallet.ToJsonString() : walletJson, legendJson, result, advanced.Events);
        }
        bool Asleep(JsonNode pet) => B(pet["flags"]!, "asleep");
        void Admit(string petId)
        {
            if (pets[petId] is null) pets[petId] = FarmSimulation.NewPet(petId, timestamp, legendJson);
            else pets[petId]!["t"] = timestamp;
        }
        var room = farm["residents"]!.AsArray();
        if (action == FarmCareAction.MoveIn)
        {
            if (!collection.Owns("legend", id) || residents.Contains(id)) return Done(false, "");
            if (room.Count >= N(farm, "cap")) return Done(false, "The room is full. Send someone to the Digi-Bank first.");
            room.Add(id); Admit(id!); return Done(true, Name(id!) + " moved in.");
        }
        if (action == FarmCareAction.MoveOut)
        {
            if (!residents.Contains(id)) return Done(false, "");
            if (room.Count <= 1) return Done(false, "Keep at least one legend in the room.");
            room.RemoveAt(Array.IndexOf(residents, id)); return Done(true, Name(id!) + " went to the Digi-Bank.");
        }
        if (action == FarmCareAction.Swap)
        {
            if (!residents.Contains(option) || residents.Contains(id) || !collection.Owns("legend", id)) return Done(false, "");
            room[Array.IndexOf(residents, option)] = id; Admit(id!);
            return Done(true, $"{Name(id!)} moved in. {Name(option!)} went to the Digi-Bank.");
        }
        if (action == FarmCareAction.Expand)
        {
            int cap = (int)N(farm, "cap");
            if (cap >= 12) return Done(false, "The room is as big as it gets.");
            int cost = ExpansionPrice(cap); if (!Spend(cost)) return Done(false, $"Needs {cost} Bits.");
            farm["cap"] = cap + 2;
            foreach (var legend in CosmeticCollection.Items("legend"))
                if (room.Count < cap + 2 && collection.Owns("legend", legend.Id) && !room.Any(x => x?.GetValue<string>() == legend.Id)) { room.Add(legend.Id); Admit(legend.Id); }
            return Done(true, $"The room now holds {cap + 2}.");
        }
        if (action is FarmCareAction.BuyTheme or FarmCareAction.SetTheme)
        {
            var theme = Themes.FirstOrDefault(x => x.Id == id); if (theme is null) return Done(false, "");
            if (!OwnsTheme(farm, collection, theme))
            {
                if (action == FarmCareAction.SetTheme) return Done(false, "");
                if (!Spend(theme.Price)) return Done(false, $"Needs {theme.Price} Bits.");
                farm["themes"]!.AsArray().Add(theme.Id);
            }
            farm["theme"] = theme.Id; return Done(true, theme.Name + ".");
        }
        if (action == FarmCareAction.Lights)
        {
            bool on = lights ?? !B(farm, "lights"); farm["lights"] = on;
            foreach (string resident in residents) { var pet = pets[resident]!; if (on && N(pet, "energy") >= 100) pet["nap"] = false; pet["flags"]!["asleep"] = !on && N(pet, "energy") < 100 || B(pet, "nap"); }
            return Done(true, on ? "Lights on." : "Lights off. Good night!", ("lights", JsonValue.Create(on)));
        }
        if (action == FarmCareAction.Clean)
        {
            int count = 0;
            foreach (string resident in residents) { var pet = pets[resident]!; count += pet["poop"]!.AsArray().Count; pet["poop"] = new JsonArray(); pet["dirt"] = 0; }
            return count == 0 ? Done(false, "Nothing to clean.") : Done(true, $"Cleaned {count} pile{(count == 1 ? "" : "s")}.", ("piles", JsonValue.Create(count)), ("reward", p is null ? Zero() : Reward(id!, 3, 5)));
        }
        if (p is null) return Done(false, action is FarmCareAction.Pet or FarmCareAction.CleanPile ? "" : "Not in the room.");
        string name = Name(id!);
        switch (action)
        {
            case FarmCareAction.Feed:
                string food = option ?? "meat";
                if (!Foods.TryGetValue(food, out var f)) return Done(false, "Not in the room.");
                if (Asleep(p)) return Done(false, name + " is asleep.");
                if (N(p, "tantrum") != 0 && N(p, "disc") < 50) { Add(p, "refusals", 1); return Done(false, name + " refuses to eat! Scold it.", ("refused", JsonValue.Create(true))); }
                if (N(p, "hunger") >= 100 && !f.Sweet) { Add(p, "refusals", 1); return Done(false, name + " is full.", ("refused", JsonValue.Create(true))); }
                int paid = 0;
                if (food == "meat" && N(farm["meat"]!, "n") > 0) { if (N(farm["meat"]!, "n") >= 3) farm["meat"]!["t"] = timestamp; Add(farm["meat"]!, "n", -1); }
                else { paid = food == "meat" ? 5 : f.Bits; if (!Spend(paid)) return Done(false, $"Needs {paid} Bits."); }
                double before = N(p, "hunger"), over = Math.Max(0, before + f.Hunger - 100), ideal = Ideal(id!);
                Stat(p, "hunger", f.Hunger); p["weight"] = Math.Clamp(N(p, "weight") + f.Weight + (over >= 20 ? 1 : 0), Math.Floor(ideal * .5 + .5), Math.Floor(ideal * 2.5 + .5));
                if (over >= 20 || f.Sweet && before >= 90) Add(p, "overfeed", 1);
                if (f.Happy != 0) Stat(p, "happy", f.Happy); if (f.Energy != 0) Stat(p, "energy", f.Energy); if (f.Disc != 0) Stat(p, "disc", f.Disc);
                Add(p, "poopClock", 15); if (before < 70) Add(p, "goodFood", f.Healthy ? .75 : .5); p["starveMin"] = 0;
                return Done(true, $"{name} ate the {f.Name}.", ("paid", JsonValue.Create(paid)), ("reward", before < 80 ? Reward(id!, 2, 4) : Zero()), ("hunger", p["hunger"]!.DeepClone()), ("weight", p["weight"]!.DeepClone()));
            case FarmCareAction.CleanPile:
                var piles = p["poop"]!.AsArray(); int index = piles.ToList().FindIndex(x => N(x!, "k") == pile);
                if (index < 0) return Done(false, ""); piles.RemoveAt(index); if (piles.Count == 0) p["dirt"] = 0;
                return Done(true, "Cleaned.", ("reward", Reward(id!, 1, 2)));
            case FarmCareAction.Cure:
                if (!B(p, "sick")) return Done(false, name + " is not sick.");
                if (option is null or "bandage")
                {
                    if (N(farm["bandage"]!, "n") <= 0) return Done(false, "No bandages left. They refill over time.");
                    if (N(farm["bandage"]!, "n") >= 2) farm["bandage"]!["t"] = timestamp; Add(farm["bandage"]!, "n", -1);
                }
                else { if (!Spend(20)) return Done(false, "Medicine needs 20 Bits."); Stat(p, "happy", -5); }
                p["sick"] = false; p["sickWhy"] = ""; p["sickMin"] = 0; if (N(p, "dirt") >= 240) p["dirt"] = 0; p["starveMin"] = 0;
                return Done(true, name + " feels better.", ("reward", Reward(id!, 3, 5)));
            case FarmCareAction.Praise:
                if (N(p, "tantrum") != 0) { p["tantrum"] = 0; Stat(p, "disc", -8); Stat(p, "happy", 5); return Done(true, name + " got away with it. Discipline went down.", ("spoiled", JsonValue.Create(true))); }
                bool fresh = timestamp - N(p, "lastPraise") >= 600000; p["lastPraise"] = timestamp; Stat(p, "happy", fresh ? 10 : 2);
                if (fresh) { Add(p, "goodBond", .34); Stat(p, "disc", 1); }
                return Done(true, name + " is proud.", ("reward", fresh ? Reward(id!, 2, 3) : Zero()));
            case FarmCareAction.Scold:
                if (N(p, "tantrum") != 0) { p["tantrum"] = 0; Stat(p, "disc", 15); Stat(p, "happy", -3); return Done(true, name + " calmed down. Discipline went up.", ("reward", Reward(id!, 3, 4))); }
                Stat(p, "happy", B(p, "sick") ? -15 : -12); Stat(p, "disc", 3); return Done(true, name + " did nothing wrong and is upset.", ("unfair", JsonValue.Create(true)));
            case FarmCareAction.Play:
                if (Asleep(p)) return Done(false, name + " is asleep."); if (B(p, "sick")) return Done(false, name + " is sick. Cure it first."); if (N(p, "energy") < 10) return Done(false, name + " is too tired.");
                var other = option is not null && option != id && residents.Contains(option) ? pets[option] : null; double was = N(p, "happy");
                foreach (var pet in new[] { p, other }) { if (pet is null || Asleep(pet) || B(pet, "sick")) continue; Stat(pet, "happy", 15); Stat(pet, "energy", -8); Stat(pet, "hunger", -5); Add(pet, "goodBond", .34); }
                return Done(true, other is not null ? $"{name} and {Name(option!)} played together." : name + " played with the ball.", ("reward", was < 95 ? Reward(id!, 2, 3) : Zero()), ("with", other is null ? null : JsonValue.Create(option)));
            case FarmCareAction.Pet:
                if (timestamp - N(p, "lastPet") < 15000) return Done(true, "", ("small", JsonValue.Create(true)));
                p["lastPet"] = timestamp; Stat(p, "happy", 3); Add(p, "goodBond", .05); return Done(true, "");
            case FarmCareAction.Train:
                if (TrainingRefusal(farm, legendJson, id) is { } refusal) return Done(false, refusal);
                if (option is null || !Training.TryGetValue(option, out var training)) return Done(false, "Unknown training.");
                score = double.IsNaN(score) ? 0 : Math.Clamp(score, 0, 1);
                int gain = 2 + (int)Math.Floor(8 * score + .5);
                var gains = new JsonObject { [training.Stat] = gain };
                if (training.Stat != "sta") gains["sta"] = 1 + (score >= .6 ? 1 : 0);
                foreach (var entry in gains) p[entry.Key] = Math.Clamp((p[entry.Key] is null ? 0 : N(p, entry.Key)) + entry.Value!.GetValue<int>(), 0, 300);
                Stat(p, "energy", -15); Stat(p, "hunger", -8);
                p["weight"] = Math.Max(Math.Floor(Ideal(id!) * .5 + .5), N(p, "weight") - (score >= .7 ? 2 : 1));
                Add(p, "trained", 1); if (score >= .8) Stat(p, "happy", 5);
                return Done(true, $"{training.Name} +{gain}", ("gains", gains), ("reward", Reward(id!, 3 + (int)Math.Floor(3 * score + .5), 6 + (int)Math.Floor(8 * score + .5))), ("score", JsonValue.Create(score)));
            default: throw new ArgumentOutOfRangeException(nameof(action));
        }
    }
}

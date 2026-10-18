using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public sealed record EggDefinition(string Id, string Name, int Price, string[] Kinds);
public sealed record EggOdds(string Rarity, double Pct, int Count);
public sealed record EggItem(string Kind, CosmeticItem Item);
public sealed record EggAward(string Kind, string Id, bool Duplicate, int Refund, LegendDuplicate? Evolution);
public sealed record EggPurchase(string Wallet, string? Legends, EggAward Award);

public static class DigiEggs
{
    private static readonly string[] Rarities = ["common", "uncommon", "rare", "epic", "legendary"];
    private static readonly int[] Weights = [45, 25, 18, 9, 3];
    public static readonly EggDefinition[] Definitions = [
        new("mixed", "Mixed Digi-Egg", 350, CosmeticCollection.Kinds),
        new("legend", "Little Legend Egg", 250, ["legend"]), new("emote", "Emote Egg", 250, ["emote"]),
        new("board", "Board Egg", 250, ["board"]), new("killscreen", "Kill Screen Egg", 250, ["killscreen"]),
        new("dance", "Dance Egg", 250, ["dance"]), new("sound", "Sound Egg", 250, ["sound"]), new("elimsound", "Elim Sound Egg", 250, ["elimsound"])
    ];
    public static EggItem[] Pool(EggDefinition egg) => egg.Kinds.SelectMany(kind => CosmeticCollection.Items(kind)
        .Where(item => !item.Secret && item.Egg != false && !item.Free).Select(item => new EggItem(kind, item))).ToArray();
    public static EggOdds[] Odds(EggDefinition egg)
    {
        var pool = Pool(egg);
        var present = Rarities.Select((rarity, i) => (Rarity: rarity, Weight: Weights[i], Count: pool.Count(x => (x.Item.Rarity ?? "common") == rarity))).Where(x => x.Count > 0).ToArray();
        int sum = present.Sum(x => x.Weight);
        return present.Select(x => new EggOdds(x.Rarity, 100.0 * x.Weight / sum, x.Count)).ToArray();
    }
    public static EggItem? Roll(EggDefinition egg, double rarityRoll, double itemRoll)
    {
        if (!double.IsFinite(rarityRoll) || !double.IsFinite(itemRoll) || rarityRoll is < 0 or >= 1 || itemRoll is < 0 or >= 1)
            throw new ArgumentOutOfRangeException(nameof(rarityRoll), "Egg rolls must be in [0,1).");
        var odds = Odds(egg);
        if (odds.Length == 0) return null;
        double value = rarityRoll * 100; string rarity = odds[^1].Rarity;
        foreach (var entry in odds) { if (value < entry.Pct) { rarity = entry.Rarity; break; } value -= entry.Pct; }
        var choices = Pool(egg).Where(x => (x.Item.Rarity ?? "common") == rarity).ToArray();
        return choices[(int)Math.Floor(itemRoll * choices.Length)];
    }
    public static EggPurchase? Purchase(string? walletJson, string? legendJson, string eggId, double rarityRoll, double itemRoll, long timestamp)
    {
        var egg = Definitions.FirstOrDefault(x => x.Id == eggId);
        if (egg is null) return null;
        var collection = new CosmeticCollection(walletJson);
        if (collection.Bits < egg.Price) return null;
        var roll = Roll(egg, rarityRoll, itemRoll);
        if (roll is null) return null;
        bool duplicate = collection.Owns(roll.Kind, roll.Item.Id);
        LegendDuplicateUpdate? evolution = duplicate && roll.Kind == "legend" ? LegendRewards.Duplicate(legendJson, roll.Item.Id) : null;
        int refund = duplicate && evolution?.Award is null ? (int)Math.Floor(roll.Item.Price * .4 + .5) : 0;
        if (!duplicate) collection.Grant(roll.Kind, roll.Item.Id);
        var wallet = JsonNode.Parse(collection.Json)!.AsObject();
        wallet["bits"] = checked(collection.Bits - egg.Price + refund);
        if (refund > 0)
        {
            var history = wallet["history"]!.AsArray();
            history.Insert(0, new JsonObject { ["n"] = refund, ["reason"] = "Duplicate refund: " + roll.Item.Name, ["at"] = timestamp });
            while (history.Count > 30) history.RemoveAt(history.Count - 1);
        }
        return new(wallet.ToJsonString(), evolution?.Save ?? legendJson, new(roll.Kind, roll.Item.Id, duplicate, refund, evolution?.Award));
    }
}

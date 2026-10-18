using System.Text.Json.Nodes;

namespace DigiTactics.Storage;

public static class CosmeticLoadouts
{
    internal static uint Hash(string text) { uint h = 2166136261; foreach (char c in text) h = unchecked((h ^ c) * 16777619); return h; }
    public static JsonObject Local(string? wallet, string? legends)
    {
        var collection = new CosmeticCollection(wallet); var look = new JsonObject();
        foreach (string kind in CosmeticCollection.Kinds) look[kind] = collection.Equipped(kind);
        if (look["legend"]?.GetValue<string>() is { } id)
        {
            string form = LegendRewards.Snapshot(legends, id).Record["form"]!.GetValue<string>();
            look["legendForm"] = form == id ? null : form;
        }
        else look["legendForm"] = null;
        return look;
    }
    public static JsonObject Ai(int seat, string name)
    {
        string key = $"{seat}:{name}"; var look = new JsonObject();
        foreach (string kind in CosmeticCollection.Kinds)
        {
            var items = CosmeticCollection.Items(kind).Where(i => !i.Secret).ToArray();
            string? Pick(int n) => items.Length == 0 ? null : items[Hash($"{key}:{kind}:{n}") % items.Length].Id;
            look[kind] = CosmeticCollection.Multi(kind) ? new JsonArray(Enumerable.Range(0, 6).Select(n => (JsonNode?)JsonValue.Create(Pick(n))).ToArray()) : JsonValue.Create(Pick(0));
        }
        look["legendForm"] = LegendEvolution.AiForm(look["legend"]?.GetValue<string>(), key); return look;
    }
    public static string? CompanionKey(JsonObject look)
    {
        string? id = look["legend"]?.GetValue<string>()?.Split('@')[0];
        return id is null ? null : LegendEvolution.VisualKey(id, look["legendForm"]?.GetValue<string>());
    }
}

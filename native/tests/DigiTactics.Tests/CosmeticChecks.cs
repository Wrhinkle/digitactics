using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Storage;

internal static class CosmeticChecks
{
    public static int Run(string root)
    {
        int checks = 0;
        void Check(bool ok, string what) { checks++; if (!ok) throw new InvalidOperationException("Cosmetics: " + what); }
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/cosmetics.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        string saved = fixture.RootElement.GetProperty("initial").GetRawText();
        foreach (var test in fixture.RootElement.GetProperty("cases").EnumerateArray())
        {
            var collection = new CosmeticCollection(saved);
            string kind = test.GetProperty("kind").GetString()!, op = test.GetProperty("op").GetString()!;
            string? id = test.GetProperty("id").GetString();
            bool result = op == "grant" ? collection.Grant(kind, id) : collection.Equip(kind, id, test.GetProperty("slot").GetInt32());
            Check(result == test.GetProperty("result").GetBoolean(), op + " " + kind + "/" + id);
            if (result) saved = collection.Json;
            Check(JsonNode.DeepEquals(JsonNode.Parse(saved), JsonNode.Parse(test.GetProperty("save").GetRawText())), "save " + kind + "/" + id);
            foreach (string k in CosmeticCollection.Kinds)
            {
                Check(JsonNode.DeepEquals(collection.Equipped(k), JsonNode.Parse(test.GetProperty("equipped").GetProperty(k).GetRawText())), "equipment " + k);
                var owned = CosmeticCollection.Items(k).Where(x => collection.Owns(k, x.Id)).Select(x => x.Id).ToArray();
                Check(owned.SequenceEqual(test.GetProperty("owned").GetProperty(k).EnumerateArray().Select(x => x.GetString())), "ownership " + k);
            }
        }
        foreach (var test in fixture.RootElement.GetProperty("prices").EnumerateArray())
        {
            string kind = test.GetProperty("kind").GetString()!, id = test.GetProperty("id").GetString()!;
            var day = DateOnly.ParseExact(test.GetProperty("day").GetString()!, "yyyy-MM-dd");
            int price = test.GetProperty("price").GetInt32();
            var item = CosmeticCollection.Item(kind, id)!;
            Check(CosmeticCollection.Price(kind, item, day) == price, "featured price " + kind + "/" + id);
            var buyer = new CosmeticCollection("{\"bits\":100000,\"future\":true}");
            bool allowed = !item.Free && !item.Secret && !item.EggOnly && item.Linked is null;
            Check(buyer.Buy(kind, id, day) == allowed, "purchase eligibility");
            if (allowed)
            {
                Check(buyer.Bits == 100000 - price && buyer.Owns(kind, id), "purchase debit and ownership");
                Check(!buyer.Buy(kind, id, day) && buyer.Bits == 100000 - price, "duplicate purchase rejected");
                var poor = new CosmeticCollection("{\"bits\":0}");
                Check(!poor.Buy(kind, id, day) && poor.Bits == 0 && !poor.Owns(kind, id), "insufficient balance untouched");
            }
        }
        Console.WriteLine($"Cosmetics: {checks} original collection, price and purchase checks passed.");
        return checks;
    }
}

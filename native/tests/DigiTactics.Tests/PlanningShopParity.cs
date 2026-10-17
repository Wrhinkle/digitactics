using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class PlanningShopParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/planning-shops.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool condition, string context) { checks++; if (!condition) throw new InvalidOperationException("Shared shop parity: " + context); }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string variant = trace.GetProperty("variant").GetString()!;
            var rules = catalog.Variants.Single(v => v.Id == variant);
            var pool = new ShopPool(rules, new(143));
            int uid = 0;
            PlanningUnit Create(string line, int stage) => new(++uid, line, stage, []);
            var economies = Enumerable.Range(0, 2).Select(_ => new PlayerEconomy(rules.Economy!)).ToArray();
            var rosters = Enumerable.Range(0, 2).Select(_ => new PlanningRoster(rules, Create)).ToArray();
            var shops = Enumerable.Range(0, 2).Select(i => new PlayerShop(rules, pool, economies[i], rosters[i], Create, _ => { })).ToArray();
            for (int i = 0; i < 2; i++) { economies[i].Credit(500 - economies[i].Gold); shops[i].Refresh(); }
            void State(JsonElement state)
            {
                foreach (var pair in state.GetProperty("pool").EnumerateObject()) Check(pool.Remaining(pair.Name) == pair.Value.GetInt32(), variant + " pool " + pair.Name);
                for (int i = 0; i < 2; i++)
                {
                    var expected = state.GetProperty("players")[i];
                    Check(economies[i].Gold == expected.GetProperty("gold").GetInt32(), variant + " gold");
                    Check(shops[i].FreeRolls == expected.GetProperty("freeRolls").GetInt32(), variant + " free rolls");
                    Check(shops[i].Slots.SequenceEqual(expected.GetProperty("shop").EnumerateArray().Select(e => e.GetString())), variant + " offers");
                    for (int j = 0; j < 9; j++)
                    {
                        var unit = expected.GetProperty("bench")[j]; var actual = rosters[i].Bench[j];
                        Check(unit.ValueKind == JsonValueKind.Null ? actual is null : actual is not null &&
                            actual.Id == unit.GetProperty("uid").GetInt32() && actual.Line == unit.GetProperty("line").GetString() &&
                            actual.Stage == unit.GetProperty("stage").GetInt32(), variant + " bench");
                    }
                }
            }
            State(trace.GetProperty("initial"));
            foreach (var step in trace.GetProperty("steps").EnumerateArray())
            {
                int index = step.GetProperty("index").GetInt32(); var shop = shops[index]; var command = step.GetProperty("command");
                bool result;
                switch (command.GetProperty("kind").GetString())
                {
                    case "freeRoll": shop.FreeRolls = 1; result = shop.Reroll(); break;
                    case "reroll": result = shop.Reroll(); break;
                    case "buy": result = shop.Buy(command.GetProperty("slot").GetInt32(), false); break;
                    case "sell": result = shop.Sell(command.GetProperty("id").GetInt32(), false); break;
                    default: throw new InvalidDataException("Unknown shop command.");
                }
                Check(result == step.GetProperty("result").GetBoolean(), variant + " command");
                State(step.GetProperty("state"));
            }
            // A maxed line clears even locked offers, without changing the partner's offers.
            var maxedLine = shops[0].Slots.FirstOrDefault(id => id is not null);
            if (maxedLine is not null)
            {
                var isolatedPool = new ShopPool(rules, new(143));
                var isolatedRoster = new PlanningRoster(rules, Create);
                var isolatedShop = new PlayerShop(rules, isolatedPool, economies[0], isolatedRoster, Create, _ => { });
                isolatedShop.Refresh();
                string line = isolatedShop.Slots.First(id => id is not null)!;
                int reserved = isolatedShop.Slots.Count(id => id == line), before = isolatedPool.Remaining(line);
                isolatedRoster.AddToBench(Create(line, 2));
                isolatedShop.Locked = true;
                isolatedShop.BeginRound();
                Check(!isolatedShop.Slots.Contains(line) && isolatedPool.Remaining(line) == before + reserved, variant + " locked max-star release");
                Check(isolatedShop.CanBuy(-1, false) == PurchaseStatus.Empty, variant + " invalid slot");
            }
            var held = shops[0].Slots.ToArray(); shops[0].Locked = true; shops[0].BeginRound();
            Check(held.SequenceEqual(shops[0].Slots), variant + " locked offers");
            foreach (var shop in shops) shop.ReleaseOffers();
            foreach (var roster in rosters) foreach (var entry in roster.Entries)
                pool.Return(entry.Unit.Line, UnitValues.Copies(rules.Lines.Single(l => l.Id == entry.Unit.Line), entry.Unit.Stage));
            foreach (var line in rules.Lines) Check(pool.Remaining(line.Id) == rules.PoolSizes[line.Tier], variant + " total copy conservation");
        }
        Console.WriteLine($"Shared shop parity: {checks:N0} checks across 22 variants.");
        return checks;
    }
}

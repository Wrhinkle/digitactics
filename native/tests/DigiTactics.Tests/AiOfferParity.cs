using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class AiOfferParity
{
    public static int Run(string root, ContentCatalog catalog, bool custom = false)
    {
        using var input = File.OpenRead(Path.Combine(root, custom ? "tests/fixtures/ai-custom-offers.json.gz" : "tests/fixtures/ai-offers.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool condition, string context) { checks++; if (!condition) throw new InvalidOperationException("AI offer parity: " + context); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var state = new MatchState(rules, 143, [new("AI", custom && trace.GetProperty("human").GetBoolean())]); var p = state.Players[0]; var ai = new AiOffers(state);
            int step = 0;
            foreach (var sample in trace.GetProperty("cases").EnumerateArray())
            {
                foreach (var entry in p.Roster.Entries.ToArray()) p.Roster.Remove(entry.Unit.Id, false);
                foreach (var spec in sample.GetProperty("setup").EnumerateArray())
                {
                    var u = spec.GetProperty("unit");
                    var unit = new PlanningUnit(u.GetProperty("uid").GetInt32(), u.GetProperty("line").GetString()!, u.GetProperty("stage").GetInt32(),
                        Strings(u.GetProperty("items")), Mods: u.GetProperty("mods").Clone(), NoItems: u.GetProperty("noItems").GetBoolean());
                    int slot = p.Roster.FreeBenchSlot; p.Roster.AddToBench(unit);
                    if (spec.GetProperty("board").GetBoolean()) { int i = spec.GetProperty("index").GetInt32(); p.Roster.Move(UnitSpot.Bench(slot), UnitSpot.Board(i % 7, 4 + i / 7), false, () => 100); }
                }
                string context = $"{rules.Id}/{step++}";
                p.Health = sample.GetProperty("hp").GetInt32();
                bool won = sample.GetProperty("won").GetBoolean(); p.Economy.RecordOutcome(!won, false); p.Economy.RecordOutcome(won, false);
                if (custom) p.Economy.Credit(sample.GetProperty("gold").GetInt32() - p.Economy.Gold);
                p.Items.Clear(); p.Items.AddRange(Strings(sample.GetProperty("items")));
                p.Augments.Clear(); p.Augments.UnionWith(Strings(sample.GetProperty("augs")));
                foreach (var query in sample.GetProperty("offers").EnumerateArray())
                {
                    var offer = new PlayerOffer(query.GetProperty("kind").GetString()!, Strings(query.GetProperty("options")), 0, null);
                    int selected = custom ? ai.Select(p, offer, sample.GetProperty("stage").GetInt32()) : ai.Select(p, offer, sample.GetProperty("stage").GetInt32(), (_, _) => throw new InvalidOperationException("Unexpected custom offer"));
                    Check(selected == query.GetProperty("selected").GetInt32(), context + " " + offer.Kind + " selection");
                    Check(state.Random.Next() == query.GetProperty("nextRandom").GetDouble(), context + " RNG");
                }
            }
        }
        Console.WriteLine($"AI offer parity: {checks:N0} checks across {fixture.RootElement.GetArrayLength()} traces.");
        return checks;
    }
}

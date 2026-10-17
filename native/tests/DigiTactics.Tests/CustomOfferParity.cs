using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class CustomOfferParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/custom-offers.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool condition, string message) { checks++; if (!condition) throw new InvalidOperationException("Custom offer parity: " + message); }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string id = trace.GetProperty("variant").GetString()!;
            var rules = catalog.Variants.Single(v => v.Id == id);
            var state = new MatchState(rules, trace.GetProperty("seed").GetUInt32(), [new("Human", true)]);
            var player = state.Players[0];
            var lines = trace.GetProperty("boardLines").EnumerateArray().Select(e => e.GetString()!).ToArray();
            for (int i = 0; i < lines.Length; i++)
            {
                player.Roster.AddToBench(state.CreateUnit(lines[i], 0));
                player.Roster.Move(UnitSpot.Bench(0), UnitSpot.Board(i, 4), false, () => 100);
                state.Pool.Take(lines[i]);
            }
            if (trace.GetProperty("full").GetBoolean()) for (int i = 0; i < 9; i++)
            {
                player.Roster.AddToBench(state.CreateUnit(lines[0], 2));
                state.Pool.Take(lines[0], UnitValues.Copies(rules.Lines.Single(l => l.Id == lines[0]), 2));
            }
            var rewards = new ModeOfferRewards(state);
            var offers = new MatchOffers(state, new UnusedHooks());
            int iteration = 0;
            foreach (var step in trace.GetProperty("steps").EnumerateArray())
            {
                var round = new RoundPosition(step.GetProperty("stage").GetInt32(), 1);
                state.TreasureVariant = step.GetProperty("version").GetString();
                player.Mechanics.DragonTreasureWon = step.GetProperty("won").Deserialize<bool?>();
                player.Augments.Clear();
                if (step.GetProperty("hoard").GetBoolean()) player.Augments.Add("drg_treasure_hoard");
                var excluded = step.GetProperty("exclude").EnumerateArray().Select(e => e.GetString()!).ToArray();
                bool treasure = id.StartsWith("dragonlands/", StringComparison.Ordinal);
                var options = offers.Options(player, round, new(treasure ? "drg_treasure" : "armory"), excluded);
                Check(options.SequenceEqual(step.GetProperty("options").EnumerateArray().Select(e => e.GetString()!)), id + $" options/{iteration}");
                string? chosen = options.Length > 0 ? options[iteration % options.Length] : null;
                Check(chosen == step.GetProperty("chosen").GetString(), id + " chosen");
                if (chosen is not null)
                {
                    if (treasure) rewards.ChooseTreasure(player, chosen, step.GetProperty("locked").GetBoolean());
                    else player.GiveItem(chosen);
                }
                var expected = step.GetProperty("state");
                Check(player.Economy.Gold == expected.GetProperty("gold").GetInt32(), id + " gold");
                Check(player.Items.SequenceEqual(expected.GetProperty("items").EnumerateArray().Select(e => e.GetString()!)), id + " items");
                Check(player.Shop.Slots.SequenceEqual(expected.GetProperty("shop").EnumerateArray().Select(e => e.GetString())), id + " shop cleanup");
                foreach (var pair in expected.GetProperty("pool").EnumerateObject()) Check(state.Pool.Remaining(pair.Name) == pair.Value.GetInt32(), id + " pool");
                Check(player.Roster.Entries.Select(e => (e.Unit.Id, e.Unit.Line, e.Unit.Stage)).SequenceEqual(expected.GetProperty("units").EnumerateArray()
                    .Select(u => (u.GetProperty("uid").GetInt32(), u.GetProperty("line").GetString()!, u.GetProperty("stage").GetInt32()))), id + " roster");
                iteration++;
            }
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), id + " RNG consumption");
        }
        Console.WriteLine($"Custom offer parity: {checks:N0} checks across 36 armory/treasure traces.");
        return checks;
    }
    private sealed class UnusedHooks : IMatchOfferHooks
    {
        public string[] Armory(MatchPlayer player, RoundPosition round) => throw new InvalidOperationException("Native armory missing.");
        public string[] CustomOptions(MatchPlayer player, RoundPosition round, OfferDefinition definition, IReadOnlyList<string> exclude) => throw new InvalidOperationException("Native custom offer missing.");
        public void AugmentPicked(MatchPlayer player, string id) => throw new InvalidOperationException("Unexpected augment.");
        public void PowerPicked(MatchPlayer player, string id) => throw new InvalidOperationException("Unexpected power.");
        public void CustomChosen(MatchPlayer player, PlayerOffer offer, string id) => throw new InvalidOperationException("Unexpected fallback choice.");
    }
}

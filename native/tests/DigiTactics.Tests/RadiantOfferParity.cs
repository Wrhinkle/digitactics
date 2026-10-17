using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class RadiantOfferParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/radiant-offers.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool condition, string message) { checks++; if (!condition) throw new InvalidOperationException("Radiant offer parity: " + message); }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var state = new MatchState(rules, trace.GetProperty("seed").GetUInt32(), [new("Human", true)]);
            var player = state.Players[0];
            string line = trace.GetProperty("line").GetString()!;
            for (int i = 0; i < 3; i++)
            {
                player.Roster.AddToBench(state.CreateUnit(line, 0));
                if (i < 2) player.Roster.Move(UnitSpot.Bench(0), UnitSpot.Board(i, 4), false, () => 100);
            }
            var rewards = new ModeOfferRewards(state);
            var offers = new MatchOffers(state, new UnusedHooks());
            int iteration = 0;
            foreach (var step in trace.GetProperty("steps").EnumerateArray())
            {
                var before = step.GetProperty("before");
                player.Items.Clear(); player.Items.AddRange(before.GetProperty("items").EnumerateArray().Select(e => e.GetString()!));
                for (int i = 0; i < 3; i++)
                {
                    var unit = player.Roster.Entries.ElementAt(i).Unit;
                    player.Roster.UpdateUnit(unit with { Items = before.GetProperty("held")[i].EnumerateArray().Select(e => e.GetString()!).ToArray() });
                }
                string kind = step.GetProperty("kind").GetString()!;
                var options = offers.Options(player, new(4, 1), new(kind), step.GetProperty("exclude").EnumerateArray().Select(e => e.GetString()!).ToArray());
                Check(options.SequenceEqual(step.GetProperty("options").EnumerateArray().Select(e => e.GetString()!)), rules.Id + " ordered options");
                string selected = options[iteration++ % options.Length];
                Check(selected == step.GetProperty("selected").GetString(), rules.Id + " selection");
                if (kind == "rec_tome") player.GiveItem(selected); else rewards.GrantRadiant(player, selected);
                var after = step.GetProperty("after");
                Check(player.Items.SequenceEqual(after.GetProperty("items").EnumerateArray().Select(e => e.GetString()!)), rules.Id + " item bench");
                for (int i = 0; i < 3; i++) Check(player.Roster.Entries.ElementAt(i).Unit.Items.SequenceEqual(after.GetProperty("held")[i].EnumerateArray().Select(e => e.GetString()!)), rules.Id + " equipped upgrade order");
            }
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), rules.Id + " RNG");
        }
        Console.WriteLine($"Radiant/Tome parity: {checks:N0} checks across 6 traces.");
        return checks;
    }
    private sealed class UnusedHooks : IMatchOfferHooks
    {
        public string[] Armory(MatchPlayer player, RoundPosition round) => throw new InvalidOperationException("Unexpected armory.");
        public string[] CustomOptions(MatchPlayer player, RoundPosition round, OfferDefinition definition, IReadOnlyList<string> exclude) => throw new InvalidOperationException("Missing native offer.");
        public void AugmentPicked(MatchPlayer player, string id) => throw new InvalidOperationException("Unexpected augment.");
        public void PowerPicked(MatchPlayer player, string id) => throw new InvalidOperationException("Unexpected power.");
        public void CustomChosen(MatchPlayer player, PlayerOffer offer, string id) => throw new InvalidOperationException("Unexpected fallback choice.");
    }
}

using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class OfferParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/offers.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool condition, string message) { checks++; if (!condition) throw new InvalidOperationException("Offer parity: " + message); }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string id = trace.GetProperty("variant").GetString()!;
            var source = catalog.Variants.Single(v => v.Id == id);
            // This trace isolates engine scheduling/selection; mode handlers have separate tests.
            var rules = source with { Offers = source.Offers! with { CustomArmory = false } };
            var state = new MatchState(rules, 143, Enumerable.Range(0, 8).Select(i => new MatchSeat($"Player {i}", i == 0)).ToArray(), trace.GetProperty("duo").GetBoolean());
            List<string> picks = [];
            var offers = new MatchOffers(state, new Hooks(picks));
            void Snapshot(MatchPlayer player, JsonElement expected)
            {
                var offer = expected.GetProperty("offer");
                Check(player.Offer is null ? offer.ValueKind == JsonValueKind.Null : offer.ValueKind != JsonValueKind.Null, id + " pending offer");
                if (player.Offer is { } actual)
                {
                    Check(actual.Kind == offer.GetProperty("kind").GetString(), id + " kind");
                    Check(actual.Options.SequenceEqual(offer.GetProperty("options").EnumerateArray().Select(e => e.GetString()!)), id + " options");
                    Check(actual.Rerolls == offer.GetProperty("rerolls").GetInt32(), id + " rerolls");
                    Check(actual.Tier == (offer.TryGetProperty("tier", out var tier) ? tier.GetInt32() : null), id + " tier");
                }
                Check(player.Items.SequenceEqual(expected.GetProperty("items").EnumerateArray().Select(e => e.GetString()!)), id + " items");
                Check(player.Globals.SequenceEqual(expected.GetProperty("globals").EnumerateArray().Select(e => e.GetString()!)), id + " globals");
                Check(player.Augments.SequenceEqual(expected.GetProperty("augs").EnumerateArray().Select(e => e.GetString()!)), id + " augments");
                var power = expected.GetProperty("power");
                Check(player.Power is null ? power.ValueKind == JsonValueKind.Null : power.ValueKind != JsonValueKind.Null &&
                    player.Power.Id == power.GetProperty("id").GetString() && player.Power.Rank == power.GetProperty("rank").GetInt32(), id + " power");
                var loot = expected.GetProperty("loot").Deserialize<string[]>();
                Check(player.Mechanics.AutoChessLoot is null ? loot is null : loot is not null && player.Mechanics.AutoChessLoot.SequenceEqual(loot), id + " loot choices");
                Check(picks.SequenceEqual(expected.GetProperty("picks").EnumerateArray().Select(e => e.GetString()!)), id + " pick hook order");
            }
            foreach (var step in trace.GetProperty("steps").EnumerateArray())
            {
                var round = new RoundPosition(step.GetProperty("stage").GetInt32(), step.GetProperty("round").GetInt32());
                var player = state.Players[step.GetProperty("index").GetInt32()];
                player.Mechanics.AutoChessLoot = rules.Loot!.Mode!.Components.Take(3).ToArray();
                var at = offers.At(round); var expectedAt = step.GetProperty("at");
                Check(at is null ? expectedAt.ValueKind == JsonValueKind.Null : expectedAt.ValueKind != JsonValueKind.Null && at.Kind == expectedAt.GetProperty("kind").GetString(), id + " schedule");
                if (expectedAt.ValueKind != JsonValueKind.Null && expectedAt.TryGetProperty("gate", out var gate))
                    Check(at!.Gate == gate.GetInt32(), id + " upgrade gate metadata");
                offers.Make(player, round); Snapshot(player, step.GetProperty("made"));
                if (step.GetProperty("reroll").ValueKind != JsonValueKind.Null)
                    Check(offers.Reroll(player, round) == step.GetProperty("reroll").GetBoolean(), id + " reroll result");
                Snapshot(player, step.GetProperty("rolled"));
                Check(offers.Choose(player, step.GetProperty("choice").GetInt32(), false) == step.GetProperty("selected").GetString(), id + " chosen ID");
                Check(offers.Choose(player, -1, false) is null, id + " invalid choice");
                Snapshot(player, step.GetProperty("after"));
            }
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), id + " RNG");
        }
        Console.WriteLine($"Offer parity: {checks:N0} checks across 44 Solo/Duos traces.");
        return checks;
    }
    private sealed class Hooks(List<string> picks) : IMatchOfferHooks
    {
        public string[] Armory(MatchPlayer player, RoundPosition round) => throw new InvalidOperationException("Unexpected custom armory.");
        public string[] CustomOptions(MatchPlayer player, RoundPosition round, OfferDefinition definition, IReadOnlyList<string> exclude) =>
            new[] { "custom_a", "custom_b", "custom_c" }.Where(id => !exclude.Contains(id)).ToArray();
        public void AugmentPicked(MatchPlayer player, string id) => picks.Add($"augment:{player.Id}:{id}");
        public void PowerPicked(MatchPlayer player, string id) => picks.Add($"power:{player.Id}:{id}");
        public void CustomChosen(MatchPlayer player, PlayerOffer offer, string id) => picks.Add($"{offer.Kind}:{player.Id}:{id}");
    }
}

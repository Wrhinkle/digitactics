using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class DragonPlanningParity
{
    private sealed class Hooks : IMatchOfferHooks
    {
        public string[] Armory(MatchPlayer p, RoundPosition r) => throw new NotSupportedException();
        public string[] CustomOptions(MatchPlayer p, RoundPosition r, OfferDefinition d, IReadOnlyList<string> e) => throw new NotSupportedException();
        public void AugmentPicked(MatchPlayer p, string id) => throw new NotSupportedException();
        public void PowerPicked(MatchPlayer p, string id) => throw new NotSupportedException();
        public void CustomChosen(MatchPlayer p, PlayerOffer o, string id) => throw new NotSupportedException();
    }
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/dragon-planning.json.gz"));
        using var zip = new GZipStream(input, CompressionMode.Decompress);
        using var doc = JsonDocument.Parse(zip);
        int checks = 0;
        void Check(bool ok, string c) { checks++; if (!ok) throw new InvalidOperationException("Dragonlands planning parity: " + c); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in doc.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            uint seed = trace.GetProperty("seed").GetUInt32(); string c = $"{rules.Id}/{seed}";
            var state = new MatchState(rules, seed, [new("Player", true)]) { TreasureVariant = "classic" };
            var p = state.Players[0]; var mode = new DragonlandsPlanning(state); var offers = new MatchOffers(state, new Hooks());
            var before = trace.GetProperty("before"); var initial = before.GetProperty("mech");
            p.Economy.Credit(before.GetProperty("gold").GetInt32() - p.Economy.Gold);
            p.Health = before.GetProperty("hp").GetInt32(); p.Shop.FreeRolls = before.GetProperty("rolls").GetInt32();
            p.Augments.UnionWith(Strings(before.GetProperty("augs"))); p.Items.AddRange(Strings(before.GetProperty("items")));
            p.Mechanics.DragonCalculatedRoll = initial.GetProperty("calcRoll").GetInt32() != 0;
            p.Mechanics.DragonAugmentRolled = initial.GetProperty("augRolled").GetInt32() != 0;
            p.Mechanics.DragonAugmentOptions = initial.GetProperty("augOpts").ValueKind == JsonValueKind.Null ? null : Strings(initial.GetProperty("augOpts"));
            foreach (var entry in trace.GetProperty("added").EnumerateArray())
            {
                var e = entry.GetProperty("unit"); var u = state.CreateUnit(e.GetProperty("line").GetString()!, 0) with { Items = Strings(e.GetProperty("items")), BonusTraits = e.TryGetProperty("bonusTraits", out var traits) ? traits.Deserialize<Dictionary<string, int>>() : null };
                int slot = p.Roster.FreeBenchSlot; p.Roster.AddToBench(u);
                if (entry.GetProperty("board").GetBoolean()) p.Roster.Move(UnitSpot.Bench(slot), UnitSpot.Board(entry.GetProperty("col").GetInt32(), 4), false, () => 100);
            }
            offers.Make(p, new(trace.GetProperty("stage").GetInt32(), trace.GetProperty("round").GetInt32()));
            foreach (var snapshot in trace.GetProperty("snapshots").EnumerateArray())
            {
                mode.RoundStart(p); var m = snapshot.GetProperty("mech");
                Check(p.Mechanics.DragonBank == m.GetProperty("bank").GetInt32() && p.Mechanics.DragonHpFull == m.GetProperty("hpFull").GetBoolean(), c + " combat snapshot");
                Check(p.Mechanics.DragonCalculatedRoll == (m.GetProperty("calcRoll").GetInt32() != 0) && p.Shop.FreeRolls == snapshot.GetProperty("rolls").GetInt32(), c + " free roll");
                Check(p.Mechanics.DragonAugmentRolled == (m.GetProperty("augRolled").GetInt32() != 0), c + " augment reroll spent");
                Check(m.GetProperty("augOpts").ValueKind == JsonValueKind.Null ? p.Mechanics.DragonAugmentOptions is null : p.Mechanics.DragonAugmentOptions!.SequenceEqual(Strings(m.GetProperty("augOpts"))), c + " augment options");
                Check(p.Items.SequenceEqual(Strings(snapshot.GetProperty("items"))), c + " inventory");
                var units = p.Roster.Entries.ToArray();
                for (int i = 0; i < units.Length; i++) Check(units[i].Unit.Items.SequenceEqual(Strings(snapshot.GetProperty("units")[i].GetProperty("items"))), c + " equipment");
                var o = snapshot.GetProperty("offer");
                Check(o.ValueKind == JsonValueKind.Null ? p.Offer is null : p.Offer is not null && p.Offer.Kind == o.GetProperty("kind").GetString() && p.Offer.Rerolls == o.GetProperty("rerolls").GetInt32() && p.Offer.Options.SequenceEqual(Strings(o.GetProperty("options"))), c + " offer");
            }
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), c + " RNG");
        }
        Console.WriteLine($"Dragonlands planning parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} traces."); return checks;
    }
}

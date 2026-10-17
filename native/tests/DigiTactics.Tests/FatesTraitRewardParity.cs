using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class FatesTraitRewardParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/fates-trait-rewards.json.gz"));
        using var zip = new GZipStream(input, CompressionMode.Decompress);
        using var doc = JsonDocument.Parse(zip);
        int checks = 0;
        void Check(bool ok, string c) { checks++; if (!ok) throw new InvalidOperationException("Fates trait reward parity: " + c); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        static int Int(JsonElement e, string key) => e.GetProperty(key).GetInt32();
        foreach (var trace in doc.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            uint seed = trace.GetProperty("seed").GetUInt32(); string c = $"{rules.Id}/{seed}";
            var state = new MatchState(rules, seed, [new("Player", true)]); var p = state.Players[0];
            var mode = new FatesPlanning(state); var m = p.Mechanics;
            var before = trace.GetProperty("before"); var initial = before.GetProperty("mech");
            p.Items.AddRange(Strings(before.GetProperty("items")));
            m.FatesLuck = Int(initial, "luck"); m.FatesDeva = Int(initial, "deva");
            m.FatesFallenFights = Int(initial, "fallenFights"); m.FatesFallenGiven = initial.GetProperty("fallenGiven").GetBoolean();
            m.FatesPath = initial.GetProperty("fallen").GetString(); m.FatesForge = Int(initial, "forge"); m.FatesForged = Int(initial, "forged");
            foreach (var entry in trace.GetProperty("added").EnumerateArray())
            {
                var e = entry.GetProperty("unit"); var unit = state.CreateUnit(e.GetProperty("line").GetString()!, Int(e, "stage"));
                int slot = p.Roster.FreeBenchSlot; p.Roster.AddToBench(unit);
                if (entry.GetProperty("board").GetBoolean()) p.Roster.Move(UnitSpot.Bench(slot), UnitSpot.Board(Int(entry, "col"), 4), false, () => 100);
            }
            var counts = trace.GetProperty("counts").Deserialize<Dictionary<string, int>>()!;
            foreach (var snapshot in trace.GetProperty("snapshots").EnumerateArray())
            {
                bool won = snapshot.GetProperty("won").GetBoolean(), pve = snapshot.GetProperty("pve").GetBoolean();
                var match = new ResolvedMatch(0, null, pve, false, won ? 0 : 1, [], counts, counts);
                int gold = mode.RoundEnd(new(match, p, 0, null, [], counts)); var em = snapshot.GetProperty("mech");
                Check(gold == Int(snapshot, "gold"), c + " gold");
                Check(p.Items.SequenceEqual(Strings(snapshot.GetProperty("items"))), c + " item order/cap");
                Check(m.FatesLuck == Int(em, "luck") && m.FatesDeva == Int(em, "deva"), c + " luck/deva");
                Check(m.FatesFallenFights == Int(em, "fallenFights") && m.FatesFallenGiven == em.GetProperty("fallenGiven").GetBoolean() && m.FatesPath == em.GetProperty("fallen").GetString(), c + " Fallen");
                Check(m.FatesForge == Int(em, "forge") && m.FatesForged == Int(em, "forged"), c + " Forge");
            }
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), c + " RNG");
        }
        Console.WriteLine($"Fates trait reward parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} traces."); return checks;
    }
}

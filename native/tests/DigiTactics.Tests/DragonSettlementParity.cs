using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class DragonSettlementParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/dragon-settlement.json.gz"));
        using var zip = new GZipStream(input, CompressionMode.Decompress);
        using var doc = JsonDocument.Parse(zip);
        var json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        int checks = 0;
        void Check(bool ok, string c) { checks++; if (!ok) throw new InvalidOperationException("Dragonlands settlement parity: " + c); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        static int Int(JsonElement e, string key) => e.GetProperty(key).GetInt32();
        static bool Truth(JsonElement e, string key) => e.GetProperty(key).ValueKind == JsonValueKind.Number ? Int(e, key) != 0 : e.GetProperty(key).GetBoolean();
        foreach (var trace in doc.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var a = trace.GetProperty("sideA").Deserialize<CombatEntry[]>(json)!;
            var b = trace.GetProperty("sideB").Deserialize<CombatEntry[]>(json)!;
            var combat = new CombatSimulation(rules, a, b, trace.GetProperty("seed").GetUInt32(), setup: new(new(Augments: ["drg_training"]), new(Augments: ["drg_training"]), RoundType: "pvp"));
            int ticks = 0; while (!combat.Done && ticks < 2000) { combat.Step(); ticks++; }
            string battle = $"{rules.Id}/{trace.GetProperty("seed")}";
            Check(combat.Done && ticks == Int(trace, "ticks") && combat.Winner == Int(trace, "winner"), battle + " result");
            for (int team = 0; team < 2; team++) Check(combat.DragonlandsRewards(team) == trace.GetProperty("rewards")[team].Deserialize<DragonlandsCombatRewards>(json), battle + $" combat counters team {team}: {combat.DragonlandsRewards(team)} / {trace.GetProperty("rewards")[team]}");
            var state = new MatchState(rules, 143, [new("A", true), new("B", true)]); var mode = new DragonlandsPlanning(state); int index = 0;
            foreach (var sample in trace.GetProperty("cases").EnumerateArray())
            {
                int team = Int(sample, "team"); string c = $"{battle}/{index++}";
                bool Flag(string key) => sample.GetProperty(key).GetBoolean();
                var p = state.Players[team]; var m = p.Mechanics;
                foreach (var entry in p.Roster.Entries.ToArray()) p.Roster.Remove(entry.Unit.Id, false);
                foreach (var entry in sample.GetProperty("added").EnumerateArray())
                {
                    var e = entry.GetProperty("unit"); var u = state.CreateUnit(e.GetProperty("line").GetString()!, 0) with { Items = Strings(e.GetProperty("items")) };
                    int slot = p.Roster.FreeBenchSlot; p.Roster.AddToBench(u);
                    if (entry.GetProperty("board").GetBoolean()) p.Roster.Move(UnitSpot.Bench(slot), UnitSpot.Board(0, 4), false, () => 100);
                }
                var before = sample.GetProperty("before"); var initial = before.GetProperty("mech");
                p.Health = Int(before, "hp"); p.Items.Clear(); p.Items.AddRange(Strings(before.GetProperty("items")));
                p.Augments.Clear(); p.Augments.UnionWith(Strings(before.GetProperty("augs")));
                m.DragonStarRounds = Int(initial, "starN"); m.DragonStarShop = Truth(initial, "starShop");
                m.DragonLagoon = Int(initial, "lagoon"); m.DragonLagoonPaid = Int(initial, "lagoonPaid");
                m.DragonDoots = Int(initial, "doots"); m.DragonTrainingKills = Int(initial, "ctKills");
                m.DragonCalculatedRoll = Truth(initial, "calcRoll"); m.DragonAxe = Int(initial, "axe"); m.DragonInvestor = Int(initial, "investor");
                var counts = sample.GetProperty("counts").Deserialize<Dictionary<string, int>>()!;
                var match = ResolvedMatch.FromCombat(0, 1, Flag("pve"), false, combat, counts, counts) with { Combat = Flag("missing") ? null : combat, Winner = Flag("won") ? team : 1 - team };
                bool view = Flag("view"), first = Flag("first");
                var scoped = match.Units.Where(u => !view || u.Survivor.Team != team || u.Owner == p.Id || u.Owner is null && first).ToArray();
                int gold = mode.RoundEnd(new(match, p, team, view ? p.Id : null, scoped, counts));
                var expected = sample.GetProperty("after"); var em = expected.GetProperty("mech");
                Check(gold == Int(sample, "gold") && p.Health == Int(expected, "hp"), c + " gold/health");
                Check(m.DragonStarRounds == Int(em, "starN") && m.DragonStarShop == Truth(em, "starShop"), c + " Star progress");
                Check(m.DragonLagoon == Int(em, "lagoon") && m.DragonLagoonPaid == Int(em, "lagoonPaid"), c + " Lagoon progress");
                Check(m.DragonDoots == Int(em, "doots") && m.DragonTrainingKills == Int(em, "ctKills") && m.DragonCalculatedRoll == Truth(em, "calcRoll"), c + " Core/training/loss");
                Check(m.DragonAxe == Int(em, "axe") && m.DragonInvestor == Int(em, "investor"), c + " item progress");
                Check(p.Items.SequenceEqual(Strings(expected.GetProperty("items"))), c + " rewards");
                var units = p.Roster.Entries.ToArray();
                for (int i = 0; i < units.Length; i++) Check(units[i].Unit.Items.SequenceEqual(Strings(expected.GetProperty("units")[i].GetProperty("items"))), c + " Investor replacement");
                Check(state.Random.Next() == sample.GetProperty("nextRandom").GetDouble(), c + " RNG");
            }
        }
        Console.WriteLine($"Dragonlands settlement parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} battles."); return checks;
    }
}

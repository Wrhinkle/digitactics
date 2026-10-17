using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class ArcaneSettlementParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/arcane-settlement.json.gz"));
        using var zip = new GZipStream(input, CompressionMode.Decompress); using var doc = JsonDocument.Parse(zip);
        var json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true }; int checks = 0;
        void Check(bool ok, string c) { checks++; if (!ok) throw new InvalidOperationException("Arcane settlement parity: " + c); }
        static int Int(JsonElement e, string k) => e.GetProperty(k).GetInt32();
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in doc.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var a = trace.GetProperty("sideA").Deserialize<CombatEntry[]>(json)!; var b = trace.GetProperty("sideB").Deserialize<CombatEntry[]>(json)!;
            var combat = new CombatSimulation(rules, a, b, trace.GetProperty("seed").GetUInt32(), setup: new(new(), new(), RoundType: "pvp"));
            int ticks = 0; while (!combat.Done && ticks < 2000) { combat.Step(); ticks++; }
            string battle = $"{rules.Id}/{trace.GetProperty("seed")}";
            Check(combat.Done && ticks == Int(trace, "ticks") && combat.Winner == Int(trace, "winner"), battle + " result");
            for (int team = 0; team < 2; team++) Check(combat.ArcaneRewards(team) == trace.GetProperty("rewards")[team].Deserialize<ArcaneCombatRewards>(json), battle + " counters");
            var state = new MatchState(rules, 143, [new("A", true), new("B", true)]); var mode = new ArcanePlanning(state); int index = 0;
            foreach (var sample in trace.GetProperty("cases").EnumerateArray())
            {
                int team = Int(sample, "team"); string c = $"{battle}/{index++}"; bool Flag(string key) => sample.GetProperty(key).GetBoolean();
                var p = state.Players[team]; var m = p.Mechanics.Arcane; var before = sample.GetProperty("before"); var initial = before.GetProperty("mech");
                p.Economy.ResetStreak(); int streak = Int(before, "streak"); for (int i = 0; i < Math.Abs(streak); i++) p.Economy.RecordOutcome(streak > 0, false); p.Economy.Credit(Int(before, "gold") - p.Economy.Gold);
                p.Augments.Clear(); p.Augments.UnionWith(Strings(before.GetProperty("augs"))); p.Items.Clear(); p.Items.AddRange(Strings(before.GetProperty("items")));
                m.AcademyLevel = Int(initial, "arcAcaLv"); m.SyndicateLevel = Int(initial, "arcSynLv"); m.Shimmer = Int(initial, "arcShimmer"); m.MarketsQueued = Int(initial, "arcMarkets"); m.Conquest = Int(initial, "arcConquest"); m.Chests = Int(initial, "arcChests"); m.FindKills = Int(initial, "arcFindK");
                state.ArcaneSponsor = Strings(sample.GetProperty("sponsor"));
                foreach (var entry in p.Roster.Entries.ToArray()) p.Roster.Remove(entry.Unit.Id, false);
                foreach (var entry in sample.GetProperty("added").EnumerateArray())
                {
                    var e = entry.GetProperty("unit"); var u = new PlanningUnit(Int(e, "uid"), e.GetProperty("line").GetString()!, Int(e, "stage"), Strings(e.GetProperty("items")));
                    int slot = p.Roster.FreeBenchSlot; p.Roster.AddToBench(u); if (entry.GetProperty("board").GetBoolean()) p.Roster.Move(UnitSpot.Bench(slot), UnitSpot.Board(Int(entry, "col"), 4), false, () => 100);
                }
                var counts = sample.GetProperty("counts").Deserialize<Dictionary<string, int>>()!;
                var match = ResolvedMatch.FromCombat(0, 1, Flag("pve"), false, combat, counts, counts) with { Combat = Flag("missing") ? null : combat, Winner = Flag("won") ? team : 1 - team };
                bool view = Flag("view"), first = Flag("first"); var scoped = match.Units.Where(u => !view || u.Survivor.Team != team || u.Owner == p.Id || u.Owner is null && first).ToArray();
                int gold = mode.RoundEnd(new(match, p, team, view ? p.Id : null, scoped, counts)); p.Economy.Credit(gold);
                var expected = sample.GetProperty("after"); var em = expected.GetProperty("mech");
                Check(gold == Int(sample, "gold") && p.Economy.Gold == Int(expected, "gold"), c + " bonus/direct gold");
                Check(p.Items.SequenceEqual(Strings(expected.GetProperty("items"))), c + " reward order/cap");
                Check(m.AcademyLevel == Int(em, "arcAcaLv") && m.SyndicateLevel == Int(em, "arcSynLv"), c + " levels");
                Check(m.Shimmer == Int(em, "arcShimmer") && m.MarketsQueued == Int(em, "arcMarkets"), c + " Shimmer");
                Check(m.Conquest == em.GetProperty("arcConquest").GetDouble() && m.Chests == Int(em, "arcChests") && m.FindKills == Int(em, "arcFindK"), c + " combat progress");
                Check(state.Random.Next() == sample.GetProperty("nextRandom").GetDouble(), c + " RNG");
            }
        }
        Console.WriteLine($"Arcane settlement parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} battles."); return checks;
    }
}

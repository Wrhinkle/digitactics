using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class UnderlordsSettlementParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/underlords-settlement.json.gz"));
        using var zip = new GZipStream(input, CompressionMode.Decompress);
        using var doc = JsonDocument.Parse(zip);
        var json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        int checks = 0;
        void Check(bool ok, string c) { checks++; if (!ok) throw new InvalidOperationException("Underlords settlement parity: " + c); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in doc.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var a = trace.GetProperty("sideA").Deserialize<CombatEntry[]>(json)!;
            var b = trace.GetProperty("sideB").Deserialize<CombatEntry[]>(json)!;
            var combat = new CombatSimulation(rules, a, b, trace.GetProperty("seed").GetUInt32(), setup: new(new(), new()));
            int ticks = 0; while (!combat.Done && ticks < 2000) { combat.Step(); ticks++; }
            string battle = $"{rules.Id}/{trace.GetProperty("seed")}";
            Check(combat.Done && ticks == trace.GetProperty("ticks").GetInt32() && combat.Winner == trace.GetProperty("winner").GetInt32(), battle + " result");
            foreach (var sample in trace.GetProperty("cases").EnumerateArray())
            {
                int n = sample.GetProperty("n").GetInt32(), team = sample.GetProperty("team").GetInt32();
                string c = $"{battle}/{n}";
                bool Flag(string name) => sample.GetProperty(name).GetBoolean();
                var state = new MatchState(rules, 143, [new("A", true), new("B", true)]);
                var mode = new UnderlordsPlanning(state); var p = state.Players[team]; var opponent = state.Players[1 - team];
                if (Flag("dead")) { opponent.Health = 0; state.Eliminate(); }
                opponent.Health = 30;
                var before = sample.GetProperty("before"); var initial = before.GetProperty("mech");
                p.Mechanics.UnderlordsBeastKills = initial.GetProperty("ulBeastKills").GetDouble();
                p.Mechanics.UnderlordsDuel = initial.GetProperty("ulDuel").GetDouble();
                p.Mechanics.UnderlordsSilver = initial.GetProperty("ulSilver").GetInt32();
                p.Mechanics.UnderlordsFreeRoll = initial.GetProperty("ulFree").GetBoolean();
                p.Health = before.GetProperty("hp").GetInt32(); p.Globals.AddRange(Strings(before.GetProperty("globals")));
                opponent.Items.AddRange(Strings(before.GetProperty("inventory")));
                for (int i = 0; i < 2; i++)
                {
                    var u = state.CreateUnit(rules.Lines[i].Id, 0) with { Items = ["ul_rapier", "ul_rapier"] };
                    int slot = p.Roster.FreeBenchSlot; p.Roster.AddToBench(u);
                    if (n % 6 != 0) p.Roster.Move(UnitSpot.Bench(slot), UnitSpot.Board(i, 4), false, () => 100);
                }
                var counts = new Dictionary<string, int>();
                var match = ResolvedMatch.FromCombat(0, Flag("absent") ? null : 1, Flag("pve"), Flag("ghost"), combat, counts, counts) with
                { Combat = Flag("missing") ? null : combat, Winner = Flag("won") ? team : 1 - team };
                bool view = Flag("view"), first = Flag("first");
                var scoped = match.Units.Where(u => !view || u.Survivor.Team != team || u.Owner == p.Id || u.Owner is null && first).ToArray();
                int gold = new MatchSettlementEffects(state, new MatchUnitLifecycle(state, () => RoundPosition.First), new(sample.GetProperty("stage").GetInt32(), 1)).RoundEnd(new(match, p, team, view ? p.Id : null, scoped, counts));
                var expected = sample.GetProperty("after"); var mech = expected.GetProperty("mech");
                Check(gold == sample.GetProperty("gold").GetInt32(), c + " gold");
                Check(p.Mechanics.UnderlordsBeastKills == mech.GetProperty("ulBeastKills").GetDouble() && p.Mechanics.UnderlordsDuel == mech.GetProperty("ulDuel").GetDouble(), c + " persistent kills/duel");
                Check(p.Mechanics.UnderlordsSilver == mech.GetProperty("ulSilver").GetInt32() && p.Mechanics.UnderlordsFreeRoll == mech.GetProperty("ulFree").GetBoolean(), c + " silver/free roll");
                Check(p.Health == expected.GetProperty("hp").GetInt32() && opponent.Health == expected.GetProperty("opponentHp").GetInt32(), c + " health");
                Check(p.Globals.SequenceEqual(Strings(expected.GetProperty("globals"))) && opponent.Items.SequenceEqual(Strings(expected.GetProperty("inventory"))), c + " globals/inventory");
                var units = p.Roster.Entries.ToArray();
                for (int i = 0; i < units.Length; i++) Check(units[i].Unit.Items.SequenceEqual(Strings(expected.GetProperty("items")[i])), c + " Rapier holder");
                Check(state.Random.Next() == sample.GetProperty("nextRandom").GetDouble(), c + " RNG");
            }
        }
        Console.WriteLine($"Underlords settlement parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} battles."); return checks;
    }
}

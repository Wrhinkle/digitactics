using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class ColiseumSettlementParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/coliseum-settlement.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        var json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        int checks = 0;
        void Check(bool condition, string context) { checks++; if (!condition) throw new InvalidOperationException("Coliseum settlement parity: " + context); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var a = trace.GetProperty("sideA").Deserialize<CombatEntry[]>(json)!; var b = trace.GetProperty("sideB").Deserialize<CombatEntry[]>(json)!;
            var combat = new CombatSimulation(rules, a, b, trace.GetProperty("seed").GetUInt32(), setup: new(new(), new()));
            int ticks = 0; while (!combat.Done && ticks < 2000) { combat.Step(); ticks++; }
            string battle = $"{rules.Id}/{trace.GetProperty("seed")}";
            Check(combat.Done && ticks == trace.GetProperty("ticks").GetInt32() && combat.Winner == trace.GetProperty("winner").GetInt32(), battle + " battle result");
            var units = trace.GetProperty("units"); Check(combat.Units.Count == units.GetArrayLength(), battle + " unit count");
            for (int i = 0; i < combat.Units.Count; i++)
            {
                var u = combat.Units[i]; var e = units[i];
                Check(u.Line.Id == e.GetProperty("line").GetString() && u.Stage == e.GetProperty("stage").GetInt32() && u.Team == e.GetProperty("team").GetInt32() && u.Alive == e.GetProperty("alive").GetBoolean() && u.Summoned == e.GetProperty("summoned").GetBoolean(), battle + " terminal unit");
                Check(u.Traits.SequenceEqual(Strings(e.GetProperty("traits"))), battle + " terminal traits");
                double dealt = e.GetProperty("dealt").GetDouble(); Check(Math.Abs(u.Dealt - dealt) <= 1e-6 * Math.Max(1, Math.Abs(dealt)), battle + " damage dealt");
            }
            var state = new MatchState(rules, 143, [new("A", true), new("B", true)]);
            var planning = new ColiseumPlanning(state);
            int step = 0;
            foreach (var sample in trace.GetProperty("cases").EnumerateArray())
            {
                int team = sample.GetProperty("team").GetInt32(); var p = state.Players[team]; var g = p.Mechanics.ColiseumGambit;
                var initial = sample.GetProperty("initial");
                g.Gems = initial.GetProperty("gem").GetDouble(); g.Level = initial.GetProperty("gLv").GetInt32(); g.Count = initial.GetProperty("gCount").GetInt32();
                g.DoubleDown = initial.GetProperty("dd").GetBoolean(); g.Ready = initial.GetProperty("gReady").GetBoolean(); g.Kills = initial.GetProperty("gKills").GetInt32(); g.KillTarget = initial.GetProperty("gTarget").GetInt32();
                p.Mechanics.ColiseumFans = initial.GetProperty("fans").GetInt32(); p.Mechanics.ColiseumSupreme = initial.GetProperty("supreme").GetString();
                p.Health = 30; p.Economy.Credit(40 - p.Economy.Gold); p.Items.Clear(); p.Items.AddRange(Strings(sample.GetProperty("inventory")));
                var counts = sample.GetProperty("counts").EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetInt32());
                var match = ResolvedMatch.FromCombat(0, 1, sample.GetProperty("pve").GetBoolean(), false, combat, counts, counts) with
                    { Winner = sample.GetProperty("winner").GetInt32(), Combat = sample.GetProperty("missing").GetBoolean() ? null : combat };
                bool view = sample.GetProperty("view").GetBoolean(), first = sample.GetProperty("first").GetBoolean();
                var scoped = match.Units.Where(u => !view || u.Survivor.Team != team || u.Owner == p.Id || u.Owner is null && first).ToArray();
                planning.RoundEnd(new(match, p, team, view ? p.Id : null, scoped, counts), sample.GetProperty("stage").GetInt32());
                string context = $"{battle}/{step++}"; var after = sample.GetProperty("after"); var m = after.GetProperty("mech");
                Check(p.Economy.Gold == after.GetProperty("gold").GetInt32() && p.Health == after.GetProperty("hp").GetInt32(), context + " gold/health");
                Check(p.Items.SequenceEqual(Strings(after.GetProperty("items"))), context + " inventory");
                Check(p.Mechanics.ColiseumSupreme == m.GetProperty("supreme").GetString() && p.Mechanics.ColiseumFans == m.GetProperty("fans").GetInt32(), context + " supreme/fans");
                Check(g.Gems == m.GetProperty("gem").GetDouble() && g.Level == m.GetProperty("gLv").GetInt32() && g.Count == m.GetProperty("gCount").GetInt32() && g.DoubleDown == m.GetProperty("dd").GetBoolean() && g.Ready == m.GetProperty("gReady").GetBoolean(), context + " gambit");
                Check(g.Kills == m.GetProperty("gKills").GetInt32() && g.KillTarget == m.GetProperty("gTarget").GetInt32(), context + " kill rewards");
                Check(state.Random.Next() == sample.GetProperty("nextRandom").GetDouble(), context + " RNG");
            }
        }
        Console.WriteLine($"Coliseum post-combat parity: {checks:N0} checks.");
        return checks;
    }
}

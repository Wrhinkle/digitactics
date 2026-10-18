using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class AutoChessPriestParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/autochess-priest.json.gz"));
        using var zip = new GZipStream(input, CompressionMode.Decompress); using var doc = JsonDocument.Parse(zip);
        int checks = 0;
        foreach (var trace in doc.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var state = new MatchState(rules, 1, [new("Player", true)]); var player = state.Players[0];
            player.Health = trace.GetProperty("before").GetInt32(); int team = trace.GetProperty("team").GetInt32();
            var units = trace.GetProperty("units").EnumerateArray().Select((u, i) => new ResolvedUnit(new(rules.Lines.Single(l => l.Id == u.GetProperty("line").GetString()), u.GetProperty("stage").GetInt32(), u.GetProperty("team").GetInt32(), u.GetProperty("alive").GetBoolean(), u.GetProperty("summoned").GetBoolean()), null, i)).ToArray();
            var counts = trace.GetProperty("counts").Deserialize<Dictionary<string, int>>()!;
            var combat = trace.GetProperty("missing").GetBoolean() ? null : new CombatSimulation(rules, [], [], 1, setup: new(new(), new()));
            var match = new ResolvedMatch(0, null, trace.GetProperty("pve").GetBoolean(), false, trace.GetProperty("won").GetBoolean() ? team : 1 - team, units, counts, counts, Combat: combat);
            new MatchSettlementEffects(state, new MatchUnitLifecycle(state, () => RoundPosition.First), new(trace.GetProperty("stage").GetInt32(), 1)).RoundEnd(new(match, player, team, null, units, counts));
            checks++; if (player.Health != trace.GetProperty("after").GetInt32()) throw new InvalidOperationException($"Auto Chess Priest parity: {rules.Id}, case {checks}.");
        }
        Console.WriteLine($"Auto Chess Priest parity: {checks:N0} checks."); return checks;
    }
}

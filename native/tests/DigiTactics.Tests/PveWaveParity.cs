using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class PveWaveParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/pve-waves.json.gz"));
        using var zip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(zip);
        int checks = 0;
        void Check(bool ok, string context) { checks++; if (!ok) throw new InvalidOperationException("PvE waves: " + context); }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var round = new RoundPosition(trace.GetProperty("stage").GetInt32(), trace.GetProperty("round").GetInt32());
            string? treasure = trace.GetProperty("treasure").GetString();
            string context = $"{rules.Id}/{round.Label}/{treasure}";
            var wave = rules.MatchCombat!.Wave(round, treasure);
            var expected = trace.GetProperty("units");
            Check(wave.Length == expected.GetArrayLength(), context + " size");
            for (int i = 0; i < wave.Length; i++)
            {
                var e = expected[i];
                Check(wave[i] == new CombatEntry(e.GetProperty("line").GetString()!, e.GetProperty("stage").GetInt32(),
                    e.GetProperty("c").GetInt32(), e.GetProperty("r").GetInt32(), e.GetProperty("uid").GetInt32()), context + " formation");
            }
            var time = trace.GetProperty("time");
            Check(rules.MatchCombat.Overtime == time.GetProperty("overtime").GetDouble() &&
                rules.MatchCombat.Limit == time.GetProperty("timeLimit").GetDouble(), context + " timers");
            var original = wave[0]; wave[0] = original with { Stage = 99 };
            Check(rules.MatchCombat.Wave(round, treasure)[0] == original, context + " isolated array");
        }
        Console.WriteLine($"PvE wave parity: {checks:N0} checks across {fixture.RootElement.GetArrayLength()} cases.");
        return checks;
    }
}

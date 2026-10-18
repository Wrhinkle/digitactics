using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Rules;

internal static class LockstepParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root,"tests/fixtures/lockstep.json.gz"));
        using var gzip = new GZipStream(input,CompressionMode.Decompress); using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool ok,string context) { checks++; if (!ok) throw new InvalidOperationException("Lockstep: " + context); }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string variant = trace.GetProperty("variant").GetString()!; var init = trace.GetProperty("init");
            var match = new LockstepMatch(catalog.Variants.Single(v => v.Id == variant), init.GetProperty("seed").GetUInt32(),
                init.GetProperty("slots").EnumerateArray().Select(s => new MatchSeat(s.GetProperty("name").GetString()!, s.GetProperty("human").GetBoolean())).ToArray(),
                init.GetProperty("planTime").GetDouble(), init.GetProperty("at").GetInt64(), init.GetProperty("format").GetString() == "duos");
            void Snapshot(JsonElement expected)
            {
                string context = variant + " " + init.GetProperty("format").GetString() + " seq " + match.Sequence;
                Check(match.Digest() == expected.GetProperty("digest").GetString(), context + " digest\n" + match.Digest() + "\n" + expected.GetProperty("digest").GetString());
                Check(match.Check() == expected.GetProperty("check").GetString(),context + " hash");
                Check(match.Sequence == expected.GetProperty("seq").GetInt32() && match.PhaseAt == expected.GetProperty("phaseAt").GetInt64() && match.TurnAt == expected.GetProperty("turnAt").GetInt64(), context + " log timing");
                Check(match.Votes.SetEquals(expected.GetProperty("votes").EnumerateArray().Select(v => v.GetInt32())),context + " votes");
                if (expected.GetProperty("maxCombat").ValueKind == JsonValueKind.Number)
                {
                    double before = match.Session.Battles.Sum(b => b.Combat.Time);
                    Check(Math.Abs(match.MaximumCombatMs() - expected.GetProperty("maxCombat").GetDouble()) < .001,context + " combat duration");
                    Check(match.Session.Battles.Sum(b => b.Combat.Time) == before,context + " duration query leaves replay untouched");
                }
                foreach (var decision in expected.GetProperty("decisions").EnumerateArray())
                {
                    var actual = match.Decide(decision.GetProperty("now").GetInt64(), decision.GetProperty("online").EnumerateArray().Select(v => v.GetInt32()).ToHashSet(),decision.GetProperty("done").EnumerateArray().Select(v => v.GetInt32()).ToHashSet());
                    Check(JsonNode.DeepEquals(actual,JsonNode.Parse(decision.GetProperty("result").GetRawText())),context + " phase decision");
                }
            }
            Snapshot(trace.GetProperty("opening"));
            foreach (var step in trace.GetProperty("steps").EnumerateArray())
            {
                Check(match.Apply(step.GetProperty("entry")).Applied == step.GetProperty("applied").GetBoolean(),variant + " action result");
                Snapshot(step.GetProperty("after"));
            }
        }
        Console.WriteLine($"Lockstep: {checks} original log, timing, decision and digest checks passed."); return checks;
    }
}

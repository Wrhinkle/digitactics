using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class AiDecisionParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/ai-decisions.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool condition, string context) { checks++; if (!condition) throw new InvalidOperationException("AI decision parity: " + context); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var state = new MatchState(rules, 143, [new("AI", false)]); var p = state.Players[0]; var ai = new PlanningAi(rules);
            int step = 0;
            foreach (var sample in trace.GetProperty("cases").EnumerateArray())
            {
                foreach (var entry in p.Roster.Entries.ToArray()) p.Roster.Remove(entry.Unit.Id, false);
                foreach (var spec in sample.GetProperty("setup").EnumerateArray())
                {
                    var u = spec.GetProperty("unit");
                    var unit = new PlanningUnit(u.GetProperty("uid").GetInt32(), u.GetProperty("line").GetString()!, u.GetProperty("stage").GetInt32(),
                        Strings(u.GetProperty("items")), BonusTraits: u.GetProperty("bonusTraits").EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetInt32()), NoSell: u.GetProperty("noSell").GetBoolean());
                    int slot = p.Roster.FreeBenchSlot; p.Roster.AddToBench(unit);
                    if (spec.GetProperty("board").GetBoolean()) { int i = spec.GetProperty("index").GetInt32(); p.Roster.Move(UnitSpot.Bench(slot), UnitSpot.Board(i % 7, 4 + i / 7), false, () => 100); }
                }
                int n = step++; string context = $"{rules.Id}/{n}";
                p.Economy.Credit(sample.GetProperty("gold").GetInt32() - p.Economy.Gold);
                var expectedCounts = sample.GetProperty("counts").EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetInt32());
                var partnerState = new MatchState(rules, 1, [new("Partner", false)]); var partner = partnerState.Players[0];
                foreach (var entry in p.Roster.Board.Take(sample.GetProperty("partnerCount").GetInt32()))
                {
                    int slot = partner.Roster.FreeBenchSlot; partner.Roster.AddToBench(entry.Unit);
                    partner.Roster.Move(UnitSpot.Bench(slot), entry.Spot, false, () => 100);
                }
                var counts = ai.Counts(p.Roster.Entries.Select(e => e.Unit), sample.GetProperty("partnerAlive").GetBoolean() ? partner : null);
                Check(counts.Count == expectedCounts.Count && counts.All(x => expectedCounts.GetValueOrDefault(x.Key) == x.Value), context + " counts");
                foreach (var score in sample.GetProperty("scores").EnumerateArray())
                    Check(ai.TraitValue(score.GetProperty("id").GetString()!, counts, n % 8) == score.GetProperty("value").GetDouble(), context + " trait score " + score.GetProperty("id"));
                foreach (var score in sample.GetProperty("unitScores").EnumerateArray())
                {
                    var unit = p.Roster.Entries.Single(e => e.Unit.Id == score.GetProperty("uid").GetInt32()).Unit;
                    Check(ai.Score(unit, counts, n % 8) == score.GetProperty("value").GetDouble(), context + " unit score");
                }
                foreach (var want in sample.GetProperty("wants").EnumerateArray())
                    Check(ai.Wants(p, want.GetProperty("id").GetString()!, sample.GetProperty("capacity").GetInt32(), sample.GetProperty("partnerCopies").GetInt32()) == want.GetProperty("value").GetBoolean(), context + " wants " + want.GetProperty("id"));
                var worst = sample.GetProperty("worst");
                Check(ai.WorstBench(p)?.Id == (worst.ValueKind == JsonValueKind.Null ? (int?)null : worst.GetInt32()), context + " worst");
                foreach (var query in sample.GetProperty("cells").EnumerateArray())
                {
                    var cell = ai.FreeCell(p, new(0, query.GetProperty("id").GetString()!, 0, []), (_, spot) => (spot.Column + spot.Row + n) % 4 != 0);
                    var expected = query.GetProperty("cell");
                    Check(expected.ValueKind == JsonValueKind.Null ? cell is null : cell == UnitSpot.Board(expected.GetProperty("c").GetInt32(), expected.GetProperty("r").GetInt32()), context + " cell");
                }
                p.Items.Clear(); var returned = new List<int>();
                ai.Arrange(p, sample.GetProperty("capacity").GetInt32(), n % 8, sample.GetProperty("partnerAlive").GetBoolean() ? partner : null,
                    (_, spot) => (spot.Column + spot.Row + n) % 4 != 0, u => returned.Add(u.Id));
                var arranged = sample.GetProperty("arranged");
                Check(p.Roster.Board.Select(e => $"{e.Spot.Column},{e.Spot.Row}:{e.Unit.Id}").SequenceEqual(arranged.GetProperty("board").EnumerateArray().Select(e => e.GetProperty("cell").GetString() + ":" + e.GetProperty("uid").GetInt32())), context + " arranged board");
                Check(p.Roster.Bench.Select(u => u?.Id).SequenceEqual(arranged.GetProperty("bench").EnumerateArray().Select(e => e.ValueKind == JsonValueKind.Null ? (int?)null : e.GetInt32())), context + " arranged bench");
                Check(p.Economy.Gold == arranged.GetProperty("gold").GetInt32(), context + " refund gold");
                Check(p.Items.SequenceEqual(Strings(arranged.GetProperty("items"))), context + " returned items");
                Check(returned.SequenceEqual(arranged.GetProperty("returned").EnumerateArray().Select(e => e.GetInt32())), context + " returned units");
            }
        }
        Console.WriteLine($"AI decision parity: {checks:N0} checks across 22 variants.");
        return checks;
    }
}

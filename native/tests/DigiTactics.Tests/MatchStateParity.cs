using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class MatchStateParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/match-state.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        int checks = 0;
        void Check(bool condition, string context) { checks++; if (!condition) throw new InvalidOperationException("Match state parity: " + context); }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string variant = trace.GetProperty("variant").GetString()!;
            uint seed = trace.GetProperty("seed").GetUInt32();
            var rules = catalog.Variants.Single(v => v.Id == variant);
            var seats = Enumerable.Range(0, 8).Select(i => new MatchSeat($"Player {i}", seed == 0 ? i % 3 == 0 : i == 0)).ToArray();
            var match = new MatchState(rules, seed, seats, trace.GetProperty("duo").GetBoolean(), trace.GetProperty("teams").Deserialize<int[][]>()) { PlayOut = seed != 143 };
            string line = trace.GetProperty("line").GetString()!;
            foreach (var player in match.Players)
            {
                var unit = match.CreateUnit(line, player.Id % 3);
                player.Roster.AddToBench(unit);
                match.Pool.Take(line, UnitValues.Copies(rules.Lines.Single(l => l.Id == line), unit.Stage));
                player.Orbs.Add(new(0, "blue", 0, 0, 0, [], line)); match.Pool.Take(line); player.Items.Add(rules.CoreItems.FirstOrDefault() ?? "test_inventory");
                if (player.Id == 2) player.Augments.Add("duo_guard");
            }
            void State(JsonElement state)
            {
                Check(match.Over == state.GetProperty("over").GetBoolean(), variant + " over");
                foreach (var pair in state.GetProperty("pool").EnumerateObject()) Check(match.Pool.Remaining(pair.Name) == pair.Value.GetInt32(), variant + " pool");
                for (int i = 0; i < 8; i++)
                {
                    var expected = state.GetProperty("players")[i]; var player = match.Players[i];
                    Check(player.Health == expected.GetProperty("hp").GetInt32(), variant + " player HP");
                    Check(player.Alive == expected.GetProperty("alive").GetBoolean(), variant + " player alive");
                    Check(player.Placement == expected.GetProperty("placement").GetInt32(), variant + " player placement");
                    Check(player.LastOpponent == expected.GetProperty("lastOpponent").Deserialize<int?>(), variant + " last opponent");
                    Check(player.Team == expected.GetProperty("team").Deserialize<int?>(), variant + " team assignment");
                    Check(player.Roster.Entries.Select(e => e.Unit.Id).SequenceEqual(expected.GetProperty("units").EnumerateArray().Select(e => e.GetInt32())), variant + " units");
                    Check(player.Shop.Slots.SequenceEqual(expected.GetProperty("shop").EnumerateArray().Select(e => e.GetString())), variant + " shop");
                    Check(player.Orbs.Count == expected.GetProperty("orbs").GetInt32() && player.Items.Count == expected.GetProperty("items").GetInt32(), variant + " eliminated inventory");
                }
                for (int i = 0; i < match.Teams.Count; i++)
                {
                    var expected = state.GetProperty("teams")[i]; var team = match.Teams[i];
                    Check(team.Health == expected.GetProperty("hp").GetInt32() && team.Alive == expected.GetProperty("alive").GetBoolean(), variant + " team health");
                    Check(team.Placement == expected.GetProperty("placement").GetInt32(), variant + " team placement");
                    Check(team.LastOpponent == expected.GetProperty("lastOpponent").Deserialize<int?>(), variant + " team opponent");
                    Check(team.Members.Select(p => p.Id).SequenceEqual(expected.GetProperty("members").EnumerateArray().Select(e => e.GetInt32())), variant + " team order");
                }
            }
            State(trace.GetProperty("initial"));
            foreach (var step in trace.GetProperty("steps").EnumerateArray())
            {
                var pairs = match.MakePairs(step.GetProperty("type").GetString()!);
                Check(pairs.SequenceEqual(step.GetProperty("pairs").Deserialize<MatchPair[]>(options)!), variant + " pairing");
                foreach (var pair in pairs)
                    if (match.Duos) { match.Teams[pair.A].LastOpponent = pair.B; if (!pair.Ghost) match.Teams[pair.B].LastOpponent = pair.A; }
                    else { match.Players[pair.A].LastOpponent = pair.B; if (!pair.Ghost) match.Players[pair.B].LastOpponent = pair.A; }
                for (int i = 0; i < 8; i++) match.Players[i].Health += step.GetProperty("changes")[i].GetInt32();
                match.SyncTeams();
                for (int i = 0; i < match.Teams.Count; i++) Check(match.DamageTeam(i, step.GetProperty("damage")[i].GetInt32()) == step.GetProperty("actual")[i].GetInt32(), variant + " guarded damage");
                match.Eliminate();
                State(step.GetProperty("state"));
            }
        }
        using var lossInput = File.OpenRead(Path.Combine(root, "tests/fixtures/match-loss.json.gz"));
        using var lossGzip = new GZipStream(lossInput, CompressionMode.Decompress);
        using var losses = JsonDocument.Parse(lossGzip);
        foreach (var trace in losses.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var lines = rules.Lines.Concat(rules.ExtraLines).ToDictionary(l => l.Id);
            foreach (var sample in trace.GetProperty("cases").EnumerateArray())
            {
                var line = lines[sample.GetProperty("line").GetString()!];
                int stars = sample.GetProperty("stars").GetInt32(), stage = sample.GetProperty("stage").GetInt32();
                Survivor[] units = [new(line, stars, 1, true, false), new(line, stars, 1, false, false),
                    new(line, stars, 1, true, true), new(line, stars, 0, true, false)];
                Check(rules.Loss!.Loss(stage, 1, false, units) == sample.GetProperty("solo").GetInt32(), rules.Id + " solo damage");
                Check(rules.Loss.Loss(stage, 1, true, units) == sample.GetProperty("pve").GetInt32(), rules.Id + " PvE damage");
                Check(rules.Loss.Loss(stage, 1, false, units, rules.Duo!.UnitScale) == sample.GetProperty("duo").GetInt32(), rules.Id + " Duos damage");
                Check(rules.Loss.Loss(stage, -1, false, units) == sample.GetProperty("negative").GetInt32(), rules.Id + " no winner damage");
            }
        }
        Console.WriteLine($"Match state parity: {checks:N0} checks across 132 Solo/Duos traces.");
        return checks;
    }
}

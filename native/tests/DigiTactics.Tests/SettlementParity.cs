using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class SettlementParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/settlement.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        void Check(bool condition, string context) { checks++; if (!condition) throw new InvalidOperationException("Settlement parity: " + context); }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string id = trace.GetProperty("variant").GetString()!;
            var rules = catalog.Variants.Single(v => v.Id == id);
            bool duo = trace.GetProperty("duo").GetBoolean();
            var state = new MatchState(rules, 143, Enumerable.Range(0, 8).Select(i => new MatchSeat($"Player {i}", i == 0 || i == 3)).ToArray(), duo) { PlayOut = true };
            state.Players[2].Augments.Add("duo_guard");
            if (trace.GetProperty("scenario").GetString() == "ghost")
            {
                if (duo) state.DamageTeam(3, state.Teams[3].Health); else state.Players[7].Health = 0;
                state.Eliminate();
            }
            var line = rules.Lines.Single(l => l.Id == trace.GetProperty("line").GetString());
            foreach (var player in state.Players.Where(p => p.Alive))
                for (int j = 0; j < line.Copies; j++)
                {
                    var unit = state.CreateUnit(line.Id, 0);
                    player.Roster.AddToBench(unit);
                    if (j == 0) player.Roster.Move(UnitSpot.Bench(0), UnitSpot.Board(3, 4), false, () => 100);
                    state.Pool.Take(line.Id);
                }
            var lines = rules.Lines.Concat(rules.ExtraLines).ToDictionary(l => l.Id);
            var matches = trace.GetProperty("inputs").EnumerateArray().Select(m => new ResolvedMatch(
                m.GetProperty("a").GetInt32(), m.GetProperty("b").Deserialize<int?>(), m.GetProperty("pve").GetBoolean(), m.GetProperty("ghost").GetBoolean(), m.GetProperty("winner").GetInt32(),
                m.GetProperty("units").EnumerateArray().Select(u => new ResolvedUnit(new(lines[u.GetProperty("line").GetString()!],
                    u.GetProperty("stage").GetInt32(), u.GetProperty("team").GetInt32(), u.GetProperty("alive").GetBoolean(), u.GetProperty("summoned").GetBoolean()),
                    u.GetProperty("owner").Deserialize<int?>(), u.GetProperty("uid").GetInt32())).ToArray(),
                m.GetProperty("traitsA").Deserialize<Dictionary<string, int>>()!, m.GetProperty("traitsB").Deserialize<Dictionary<string, int>>()!,
                m.TryGetProperty("ta", out var ta) ? ta.GetInt32() : null, m.TryGetProperty("tb", out var tb) ? tb.GetInt32() : null)).ToArray();
            int logIndex = 0;
            void Log(string kind, MatchPlayer player, RoundEndContext? context = null, bool? won = null)
            {
                var expected = trace.GetProperty("log")[logIndex++];
                Check(expected.GetProperty("kind").GetString() == kind && expected.GetProperty("id").GetInt32() == player.Id, id + " hook order");
                Check(expected.GetProperty("hp").GetInt32() == player.Health, id + " hook HP");
                Check(expected.GetProperty("gold").GetInt32() == player.Economy.Gold && expected.GetProperty("streak").GetInt32() == player.Economy.Streak, id + " hook economy");
                if (won is { } w) Check(expected.GetProperty("won").GetBoolean() == w, id + " reward winner");
                if (context is { } c)
                {
                    Check(expected.GetProperty("won").GetBoolean() == c.Won && expected.GetProperty("pve").GetBoolean() == c.Pve, id + " hook outcome");
                    Check(expected.GetProperty("team").GetInt32() == c.Team && expected.GetProperty("owner").Deserialize<int?>() == c.Owner, id + " hook owner");
                    Check(c.Units.Select(u => u.Uid).SequenceEqual(expected.GetProperty("units").EnumerateArray().Select(u => u.GetInt32())), id + " filtered units");
                    Check(c.Traits.OrderBy(p => p.Key).SequenceEqual(expected.GetProperty("counts").Deserialize<Dictionary<string, int>>()!.OrderBy(p => p.Key)), id + " trait counts");
                }
                if (kind == "eliminated") Check(expected.GetProperty("placement").GetInt32() == player.Placement, id + " eliminated hook");
            }
            var settlement = new MatchSettlement(state, trace.GetProperty("stage").GetInt32(), new Hooks(Log));
            var results = settlement.Settle(matches);
            Check(logIndex == trace.GetProperty("log").GetArrayLength(), id + " all hooks");
            for (int i = 0; i < results.Count; i++)
            {
                var actual = results[i]; var expected = trace.GetProperty("results")[i];
                Check(actual.DamageA == expected.GetProperty("damageA").GetInt32() && actual.DamageB == expected.GetProperty("damageB").GetInt32(), id + " damage");
                Check(actual.TeamDamageA == expected.GetProperty("teamDamageA").GetInt32(), id + " shared PvE damage");
                Check(actual.BonusA == expected.GetProperty("bonusA").GetInt32() && actual.BonusB == expected.GetProperty("bonusB").GetInt32() &&
                    actual.BonusA2 == expected.GetProperty("bonusA2").GetInt32() && actual.BonusB2 == expected.GetProperty("bonusB2").GetInt32(), id + " bonuses");
                Check(actual.Reward is null ? expected.GetProperty("reward").ValueKind == JsonValueKind.Null :
                    actual.Reward.Gold == expected.GetProperty("reward").GetProperty("gold").GetInt32() && actual.Reward.Orbs == 1, id + " reward summary");
            }
            var final = trace.GetProperty("state");
            Check(state.Over == final.GetProperty("over").GetBoolean(), id + " game over");
            foreach (var pair in final.GetProperty("pool").EnumerateObject()) Check(state.Pool.Remaining(pair.Name) == pair.Value.GetInt32(), id + " returned pool");
            for (int i = 0; i < state.Players.Count; i++)
            {
                var player = state.Players[i]; var expected = final.GetProperty("players")[i];
                Check(player.Health == expected.GetProperty("hp").GetInt32() && player.Economy.Gold == expected.GetProperty("gold").GetInt32(), id + " final HP/gold");
                Check(player.Economy.Streak == expected.GetProperty("streak").GetInt32(), id + " final streak");
                Check(player.Alive == expected.GetProperty("alive").GetBoolean() && player.Placement == expected.GetProperty("placement").GetInt32(), id + " placement");
                Check(player.LastOpponent == expected.GetProperty("lastOpponent").Deserialize<int?>(), id + " player opponent");
                Check(player.Roster.Entries.Select(e => (e.Unit.Id, e.Unit.Line, e.Unit.Stage)).SequenceEqual(expected.GetProperty("units").EnumerateArray()
                    .Select(u => (u.GetProperty("uid").GetInt32(), u.GetProperty("line").GetString()!, u.GetProperty("stage").GetInt32()))), id + " final merges");
                Check(player.Shop.Slots.SequenceEqual(expected.GetProperty("shop").EnumerateArray().Select(e => e.GetString())), id + " final offers");
            }
            for (int i = 0; i < state.Teams.Count; i++)
            {
                var team = state.Teams[i]; var expected = final.GetProperty("teams")[i];
                Check(team.Health == expected.GetProperty("hp").GetInt32() && team.Alive == expected.GetProperty("alive").GetBoolean(), id + " final team HP");
                Check(team.Placement == expected.GetProperty("placement").GetInt32() && team.LastOpponent == expected.GetProperty("lastOpponent").Deserialize<int?>(), id + " final team placement/opponent");
            }
            try { settlement.Settle(matches); throw new InvalidOperationException("Duplicate settlement accepted."); }
            catch (InvalidOperationException ex) when (ex.Message == "This round has already been settled.") { checks++; }
        }
        Console.WriteLine($"Settlement parity: {checks:N0} checks across 396 Solo/Duos rounds.");
        return checks;
    }
    private sealed class Hooks(Action<string, MatchPlayer, RoundEndContext?, bool?> log) : IMatchSettlementHooks
    {
        public PveRewardSummary Reward(MatchPlayer player, bool won)
        {
            log("reward", player, null, won); player.Economy.Credit(2); return new(2, null, [], 1);
        }
        public int RoundEnd(RoundEndContext context)
        {
            log("roundEnd", context.Player, context, null); context.Player.Health += context.Won ? 1 : -1; return context.Player.Id % 3 + 1;
        }
        public void AfterCombat(MatchPlayer player) => log("after", player, null, null);
        public void Eliminated(MatchPlayer player) => log("eliminated", player, null, null);
    }
}

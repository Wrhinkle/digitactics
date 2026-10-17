using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;
internal static class GizmosPlanningParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/gizmos-planning.json.gz"));
        using var zip = new GZipStream(input, CompressionMode.Decompress); using var doc = JsonDocument.Parse(zip);
        int checks = 0;
        void Check(bool ok, string c) { checks++; if (!ok) throw new InvalidOperationException("Gizmos planning parity: " + c); }
        static int Int(JsonElement e, string k) => e.GetProperty(k).GetInt32();
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        static bool Truth(JsonElement e) => e.ValueKind == JsonValueKind.Number ? e.GetInt32() != 0 : e.GetBoolean();
        static PlanningUnit Unit(MatchState state, JsonElement e) => state.CreateUnit(e.GetProperty("line").GetString()!, Int(e, "stage")) with { Items = Strings(e.GetProperty("items")), Mods = e.TryGetProperty("mods", out var mods) ? mods.Clone() : null, BonusTraits = e.TryGetProperty("bonusTraits", out var traits) ? traits.Deserialize<Dictionary<string, int>>() : null };
        foreach (var trace in doc.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString()); uint seed = trace.GetProperty("seed").GetUInt32(); string c = $"{rules.Id}/{seed}";
            var state = new MatchState(rules, seed, [new("Player", true)]) { GizmosGates = trace.GetProperty("gates").ValueKind == JsonValueKind.Null ? null : trace.GetProperty("gates").Deserialize<int[]>() };
            var p = state.Players[0]; var mode = new GizmosPlanning(state); var m = p.Mechanics.Gizmos;
            var before = trace.GetProperty("before"); var initial = before.GetProperty("mech");
            p.Economy.Credit(Int(before, "gold") - p.Economy.Gold); p.Shop.FreeRolls = Int(before, "rolls"); p.Augments.UnionWith(Strings(before.GetProperty("augs"))); p.Items.AddRange(Strings(before.GetProperty("items")));
            m.CalculatedRoll = Truth(initial.GetProperty("calcRoll")); m.GoldStart = Int(initial, "goldStart"); m.Junk = Int(initial, "junk");
            if (initial.GetProperty("chest") is var chest && chest.ValueKind != JsonValueKind.Null) { m.Chest = new() { Gold = Int(chest, "gold"), Shut = Int(chest, "shut") }; m.Chest.Items.AddRange(Strings(chest.GetProperty("items"))); }
            foreach (var entry in trace.GetProperty("added").EnumerateArray()) { var u = Unit(state, entry.GetProperty("unit")); int slot = p.Roster.FreeBenchSlot; p.Roster.AddToBench(u); if (entry.GetProperty("board").GetBoolean()) p.Roster.Move(UnitSpot.Bench(slot), UnitSpot.Board(Int(entry, "col"), 4), false, () => 100); }
            foreach (var pair in before.GetProperty("pool").EnumerateObject()) state.Pool.Take(pair.Name, state.Pool.Remaining(pair.Name) - pair.Value.GetInt32());
            void Snapshot(JsonElement e)
            {
                var em = e.GetProperty("mech"); Check(p.Economy.Gold == Int(e, "gold") && p.Shop.FreeRolls == Int(e, "rolls"), c + " economy"); Check(p.Items.SequenceEqual(Strings(e.GetProperty("items"))), c + " inventory");
                Check(m.CalculatedRoll == Truth(em.GetProperty("calcRoll")) && m.GoldStart == Int(em, "goldStart") && m.Junk == Int(em, "junk"), c + " mechanic");
                var ec = em.GetProperty("chest"); Check(ec.ValueKind == JsonValueKind.Null ? m.Chest is null : m.Chest is not null && m.Chest.Gold == Int(ec, "gold") && m.Chest.Shut == Int(ec, "shut") && m.Chest.Items.SequenceEqual(Strings(ec.GetProperty("items"))), c + " chest");
            }
            mode.RoundStart(p, new(Int(trace, "stage"), Int(trace, "round")), trace.GetProperty("type").GetString()!); Snapshot(trace.GetProperty("started"));
            var counts = trace.GetProperty("counts").Deserialize<Dictionary<string, int>>()!;
            var combat = trace.GetProperty("missing").GetBoolean() ? null : new CombatSimulation(rules, [], [], 1, setup: new(new(), new()));
            var match = new ResolvedMatch(0, null, trace.GetProperty("pve").GetBoolean(), false, trace.GetProperty("won").GetBoolean() ? 0 : 1, [], counts, counts, Combat: combat);
            int combines = 0;
            int gold = mode.RoundEnd(new(match, p, 0, null, [], counts), Int(trace, "stage"), player => { combines++; player.Roster.Combine(false, player.GiveItem, onCombine: (u, sources) => mode.Combined(player, u, sources)); player.Shop.ClearMaxed(); });
            Check(gold == Int(trace, "gold") && combines == Int(trace, "combines"), c + " rewards/combine"); p.Economy.Credit(gold);
            mode.Sold(p, Unit(state, trace.GetProperty("sold")));
            void Mods(PlanningUnit u, JsonElement e) { Check(u.Mods is { } actual && e.EnumerateObject().All(prop => actual.TryGetProperty(prop.Name, out var value) && value.ToString() == prop.Value.ToString()) && actual.EnumerateObject().Count() == e.EnumerateObject().Count(), c + " Glutton mods"); }
            var fed = p.Roster.Entries.Select(e => e.Unit).Where(u => u.Line == "giz_gekomon").ToArray(); for (int i = 0; i < fed.Length; i++) Mods(fed[i], trace.GetProperty("fed")[i]);
            var sources = trace.GetProperty("sources").EnumerateArray().Select(e => new PlanningUnit(0, "giz_gekomon", 0, [], Mods: e.TryGetProperty("mods", out var mods) ? mods.Clone() : null)).ToArray(); mode.Combined(p, fed[0], sources);
            var after = trace.GetProperty("after"); Snapshot(after);
            void CheckUnit(PlanningUnit? u, JsonElement e) { Check(e.ValueKind == JsonValueKind.Null ? u is null : u is not null && u.Id == Int(e, "uid") && u.Line == e.GetProperty("line").GetString() && u.Stage == Int(e, "stage") && u.Items.SequenceEqual(Strings(e.GetProperty("items"))), c + " unit"); if (u is not null && e.TryGetProperty("mods", out var mods)) Mods(u, mods); }
            var board = after.GetProperty("board").EnumerateObject().ToArray(); Check(board.Length == p.Roster.Board.Count, c + " board"); for (int i = 0; i < board.Length; i++) { var entry = p.Roster.Board[i]; Check($"{entry.Spot.Column},{entry.Spot.Row}" == board[i].Name, c + " board order"); CheckUnit(entry.Unit, board[i].Value); } for (int i = 0; i < 9; i++) CheckUnit(p.Roster.Bench[i], after.GetProperty("bench")[i]);
            foreach (var pair in after.GetProperty("pool").EnumerateObject()) Check(state.Pool.Remaining(pair.Name) == pair.Value.GetInt32(), c + " pool");
            Check(state.CreateUnit(rules.Lines[0].Id, 0).Id == Int(trace, "nextId"), c + " ID"); Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), c + " RNG");
        }
        Console.WriteLine($"Gizmos planning parity: {checks:N0} checks across {doc.RootElement.GetArrayLength()} traces."); return checks;
    }
}

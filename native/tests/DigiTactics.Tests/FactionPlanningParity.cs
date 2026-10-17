using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class FactionPlanningParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/faction-planning.json.gz"));
        using var zip = new GZipStream(input, CompressionMode.Decompress);
        using var document = JsonDocument.Parse(zip);
        int checks = 0;
        void Check(bool ok, string c) { checks++; if (!ok) throw new InvalidOperationException("Faction planning parity: " + c); }
        static string[] Strings(JsonElement e) => e.EnumerateArray().Select(x => x.GetString()!).ToArray();
        static int Int(JsonElement e, string key) => e.GetProperty(key).GetInt32();
        static double Number(JsonElement e, string key) => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(key, out var v) ? v.GetDouble() : 0;
        foreach (var trace in document.RootElement.EnumerateArray())
        {
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            string c = $"{rules.Id}/{trace.GetProperty("augment")}/{trace.GetProperty("before").GetProperty("wisp")}/{Int(trace,"n")}";
            var phase = Enum.Parse<MatchPhase>(trace.GetProperty("phase").GetString()!, true);
            var state = new MatchState(rules, trace.GetProperty("seed").GetUInt32(), [new("Player", trace.GetProperty("human").GetBoolean())]);
            var p = state.Players[0]; var mode = new FactionWarsPlanning(state);
            var lifecycle = new MatchUnitLifecycle(state, () => RoundPosition.First);
            foreach (var added in trace.GetProperty("added").EnumerateArray())
            {
                var e = added.GetProperty("unit");
                var u = state.CreateUnit(e.GetProperty("line").GetString()!, Int(e, "stage")) with { Items = Strings(e.GetProperty("items")), Mods = e.GetProperty("mods").Clone() };
                Check(u.Id == Int(e,"uid"), c + " setup ID"); int slot = p.Roster.FreeBenchSlot; p.Roster.AddToBench(u);
                if (added.GetProperty("board").GetBoolean()) p.Roster.Move(UnitSpot.Bench(slot), UnitSpot.Board(Int(added,"col"), 4), false, () => 100);
            }
            if (trace.GetProperty("drained").GetString() is { } drained) state.Pool.Take(drained, state.Pool.Remaining(drained));
            var before = trace.GetProperty("before");
            p.Economy.Credit(Int(before,"gold") - p.Economy.Gold); p.Health = Int(before,"hp"); p.Shop.FreeRolls = Int(before,"rolls"); p.TeamBonus = Int(before,"teamBonus");
            p.Items.AddRange(Strings(before.GetProperty("items")));
            var buffs = before.GetProperty("buffs"); if (buffs.ValueKind != JsonValueKind.Null) p.NextBuffs = new(Number(buffs,"as"), Number(buffs,"armor"), Number(buffs,"mana"));
            var wisp = before.GetProperty("wisp"); if (wisp.ValueKind != JsonValueKind.Null) p.Wisp = new(wisp.GetProperty("id").GetString()!, Int(wisp,"cost"));
            void Snapshot(JsonElement expected, string step)
            {
                string label = c + "/" + step;
                Check(p.Economy.Gold == Int(expected,"gold") && p.Health == Int(expected,"hp") && p.Shop.FreeRolls == Int(expected,"rolls") && p.TeamBonus == Int(expected,"teamBonus"), label + " economy");
                Check(p.Items.SequenceEqual(Strings(expected.GetProperty("items"))), label + " inventory");
                Check(p.Shop.Slots.SequenceEqual(expected.GetProperty("shop").EnumerateArray().Select(x => x.GetString())), label + " shop");
                var w = expected.GetProperty("wisp"); Check((p.Wisp is null) == (w.ValueKind == JsonValueKind.Null), label + " wisp presence");
                if (p.Wisp is { } actualWisp) Check(actualWisp.Id == w.GetProperty("id").GetString() && actualWisp.Cost == Int(w,"cost"), label + " wisp");
                var b = expected.GetProperty("buffs"); Check((p.NextBuffs is null) == (b.ValueKind == JsonValueKind.Null), label + " buff presence");
                if (p.NextBuffs is { } actualBuffs) Check(actualBuffs.AttackSpeed == Number(b,"as") && actualBuffs.Armor == Number(b,"armor") && actualBuffs.Mana == Number(b,"mana"), label + " buffs");
                void Unit(PlanningUnit? u, JsonElement e)
                {
                    Check((u is null) == (e.ValueKind == JsonValueKind.Null), label + " unit presence"); if (u is null) return;
                    Check(u.Id == Int(e,"uid") && u.Line == e.GetProperty("line").GetString() && u.Stage == Int(e,"stage") && u.Items.SequenceEqual(Strings(e.GetProperty("items"))), label + " unit");
                    Check((u.Mods is not null) == e.TryGetProperty("mods", out var mods), label + " modifier presence");
                    if (u.Mods is { } actual) Check(JsonElement.DeepEquals(actual, mods), label + " modifiers");
                }
                var board = expected.GetProperty("board").EnumerateObject().ToArray(); Check(board.Length == p.Roster.Board.Count, label + " board count");
                for (int i = 0; i < board.Length; i++) { Check($"{p.Roster.Board[i].Spot.Column},{p.Roster.Board[i].Spot.Row}" == board[i].Name, label + " board order"); Unit(p.Roster.Board[i].Unit, board[i].Value); }
                for (int i = 0; i < 9; i++) Unit(p.Roster.Bench[i], expected.GetProperty("bench")[i]);
                foreach (var pair in expected.GetProperty("pool").EnumerateObject()) Check(state.Pool.Remaining(pair.Name) == pair.Value.GetInt32(), label + " pool " + pair.Name);
            }
            if (trace.GetProperty("augment").GetString() is { } augment) { p.Augments.Add(augment); mode.AugmentPicked(p, augment); lifecycle.Combine(p, phase); }
            Snapshot(trace.GetProperty("picked"), "augment");
            var income = mode.Income(p, new(5, 3, 2, 1, 0)); Check(income.Augment == Int(trace.GetProperty("income"),"augment"), c + " income");
            if (Int(trace,"n") % 2 == 0) mode.AiTurn(p, phase); Snapshot(trace.GetProperty("ai"), "AI");
            Check(mode.Act(p, phase) == trace.GetProperty("result").GetString(), c + " action result"); Snapshot(trace.GetProperty("acted"), "action");
            foreach (var round in trace.GetProperty("rounds").EnumerateArray())
            { mode.RoundStart(p, new(Int(round,"stage"), Int(round,"round"))); Snapshot(round.GetProperty("after"), "round" + Int(round,"stage")); }
            Check(state.CreateUnit(rules.Lines[0].Id, 0).Id == Int(trace,"nextId"), c + " next ID");
            Check(state.Random.Next() == trace.GetProperty("nextRandom").GetDouble(), c + " RNG");
        }
        Console.WriteLine($"Faction planning parity: {checks:N0} checks across {document.RootElement.GetArrayLength()} traces."); return checks;
    }
}

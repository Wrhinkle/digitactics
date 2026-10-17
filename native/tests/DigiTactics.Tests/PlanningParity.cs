using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Rules;

internal static class PlanningParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/planning.json.gz"));
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        int checks = 0;
        void Check(bool condition, string context)
        {
            checks++;
            if (!condition) throw new InvalidOperationException("Planning parity: " + context);
        }
        void Unit(PlanningUnit? actual, JsonElement expected, string context)
        {
            if (expected.ValueKind == JsonValueKind.Null) { Check(actual is null, context + " empty"); return; }
            Check(actual is not null, context + " missing");
            Check(actual!.Id == expected.GetProperty("uid").GetInt32(), context + " UID");
            Check(actual.Line == expected.GetProperty("line").GetString(), context + " line");
            Check(actual.Stage == expected.GetProperty("stage").GetInt32(), context + " stage");
            Check(actual.Items.SequenceEqual(expected.GetProperty("items").EnumerateArray().Select(e => e.GetString()!)), context + " items");
        }
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string variant = trace.GetProperty("variant").GetString()!;
            var rules = catalog.Variants.Single(v => v.Id == variant);
            int uid = 0, step = 0;
            PlanningUnit Create(string line, int stage) => new(++uid, line, stage, []);
            var roster = new PlanningRoster(rules, Create);
            List<string> inventory = [];
            foreach (var frame in trace.GetProperty("snapshots").EnumerateArray())
            {
                string context = $"{variant}/{step++}";
                bool locked = frame.GetProperty("locked").GetBoolean();
                var command = frame.GetProperty("command");
                switch (command.GetProperty("kind").GetString())
                {
                    case "add":
                        string line = command.GetProperty("line").GetString()!;
                        bool can = roster.CanAcceptPurchase(line, locked);
                        Check(can == command.GetProperty("accepted").GetBoolean(), context + " purchase capacity");
                        if (can)
                        {
                            var unit = Create(line, 0) with { Items = command.GetProperty("items").EnumerateArray().Select(e => e.GetString()!).ToArray() };
                            var extra = roster.AddToBench(unit) ? null : unit;
                            roster.Combine(locked, inventory.Add, extra);
                        }
                        break;
                    case "move":
                        var from = command.GetProperty("from").Deserialize<UnitSpot>(options);
                        var to = command.GetProperty("to").Deserialize<UnitSpot>(options);
                        Check(roster.Move(from, to, locked, () => 5 + roster.TeamSizeBonus).ToString() == command.GetProperty("result").GetString(), context + " move");
                        break;
                    case "remove":
                        Check((roster.Remove(command.GetProperty("id").GetInt32(), locked) is not null) == command.GetProperty("accepted").GetBoolean(), context + " removal");
                        break;
                }
                var state = frame.GetProperty("state");
                var board = state.GetProperty("board");
                Check(roster.Board.Count == board.GetArrayLength(), context + " board size");
                int i = 0;
                foreach (var entry in board.EnumerateArray())
                {
                    Check(roster.Board[i].Spot == entry.GetProperty("spot").Deserialize<UnitSpot>(options), context + " board order");
                    Unit(roster.Board[i++].Unit, entry.GetProperty("unit"), context);
                }
                i = 0;
                foreach (var unit in state.GetProperty("bench").EnumerateArray()) Unit(roster.Bench[i++], unit, context);
                Check(inventory.SequenceEqual(state.GetProperty("inventory").EnumerateArray().Select(e => e.GetString()!)), context + " returned items");
            }
        }
        Console.WriteLine($"Planning parity: {checks:N0} checks across 22 variants.");
        return checks;
    }
}

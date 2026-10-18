using System.Text.Json;
using DigiTactics.Presentation;

internal static class PedestalInteractionChecks
{
    public static int Run(string root)
    {
        using var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root, "tests/fixtures/pedestal-interaction.json")));
        int checks = 0, cases = 0;
        string context = "initial";
        void Check(bool valid, string message)
        {
            checks++;
            if (!valid) throw new InvalidOperationException($"Pedestal interaction {context}: {message}");
        }
        void Near(double actual, double expected, string message)
            => Check(double.IsFinite(actual) && Math.Abs(actual - expected) < 1e-12, $"{message}: {actual:R} vs {expected:R}");
        foreach (var row in document.RootElement.GetProperty("cases").EnumerateArray())
        {
            string name = row.GetProperty("name").GetString()!;
            var random = row.GetProperty("random").EnumerateArray().Select(x => x.GetDouble()).ToArray();
            int randomCalls = 0;
            var interaction = new PedestalInteraction(() => random[randomCalls++]);
            List<string> roles = [];
            var initial = row.GetProperty("initial");
            bool hasView = initial.GetProperty("hasView").GetBoolean(), motion = initial.GetProperty("motion").GetBoolean();
            double time = initial.GetProperty("time").GetDouble();
            context = name + " initial";
            Snapshot(initial);
            int step = 0;
            foreach (var item in row.GetProperty("steps").EnumerateArray())
            {
                context = name + " step " + step++;
                var action = item.GetProperty("action");
                switch (action.GetProperty("type").GetString())
                {
                    case "down": interaction.Down(action.GetProperty("x").GetDouble()); break;
                    case "move": interaction.Move(action.GetProperty("x").GetDouble()); break;
                    case "up": if (interaction.Up(hasView) is { } role) roles.Add(role); break;
                    case "view": hasView = action.GetProperty("present").GetBoolean(); break;
                    case "frame":
                        time = action.GetProperty("time").GetDouble();
                        if (action.TryGetProperty("motion", out var enabled)) motion = enabled.GetBoolean();
                        break;
                    default: throw new InvalidDataException("Unexpected source pedestal action.");
                }
                Snapshot(item.GetProperty("expected"));
            }
            cases++;
            void Snapshot(JsonElement expected)
            {
                Near(interaction.YawOffset, expected.GetProperty("yawOffset").GetDouble(), "yaw offset");
                Near(interaction.Yaw(time, motion), expected.GetProperty("yaw").GetDouble(), "sway plus drag yaw");
                Check(interaction.Dragging == expected.GetProperty("dragging").GetBoolean(), "capture state");
                Check(interaction.HasMoved == expected.GetProperty("hasMoved").GetBoolean(), "sticky movement threshold");
                Check(randomCalls == expected.GetProperty("randomCalls").GetInt32(), "only live clicks draw random values");
                Check(roles.SequenceEqual(expected.GetProperty("roles").EnumerateArray().Select(x => x.GetString()!)), "source reaction role sequence");
                Check(roles.Count == expected.GetProperty("sounds").GetArrayLength(), "one returned reaction corresponds to each source emote sound");
            }
        }

        context = "native pointer cancellation";
        int draws = 0;
        var canceled = new PedestalInteraction(() => { draws++; return .5; });
        canceled.Down(10); canceled.Move(20);
        double offset = canceled.YawOffset;
        canceled.Cancel();
        Check(!canceled.Dragging && !canceled.HasMoved, "cancellation clears captured gesture");
        Near(canceled.YawOffset, offset, "cancellation preserves chosen yaw");
        canceled.Move(90);
        Near(canceled.YawOffset, offset, "moves after cancellation are ignored");
        Check(canceled.Up(true) is null && draws == 0, "release after cancellation does not react or draw random values");
        canceled.Down(25); canceled.Cancel(); canceled.Cancel();
        Check(canceled.Up(true) is null && draws == 0, "canceling a click and repeated cancellation are safe");
        canceled.Down(30);
        Check(canceled.Up(true) == "dance" && draws == 1, "a fresh gesture works after cancellation");
        Near(canceled.Yaw(42, false), offset, "reduced motion preserves interactive yaw");
        Console.WriteLine($"Pedestal interaction: {checks:N0} source checks across {cases} gesture cases passed (thresholds, accumulated drag, click roles, capture cancellation and reduced motion).");
        return checks;
    }
}

using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Presentation;

internal static class TrainingGameChecks
{
    public static int Run(string root)
    {
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/training-games.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip);
        int checks = 0;
        foreach (var test in fixture.RootElement.EnumerateArray())
        {
            string name = test.GetProperty("name").GetString()!;
            var game = TrainingGame.Create(name);
            double delta = test.GetProperty("delta").GetDouble();
            string context = name + "/" + test.GetProperty("style") + "/" + delta;
            void Check(bool ok, string detail) { checks++; if (!ok) throw new InvalidOperationException(context + ": " + detail); }
            foreach (var key in test.GetProperty("keys").EnumerateObject())
                Check(game.Key(key.Name) == (key.Value.ValueKind == JsonValueKind.Null ? null : key.Value.GetInt32()), "key " + key.Name);
            foreach (var frame in test.GetProperty("frames").EnumerateArray())
            {
                double time = frame.GetProperty("time").GetDouble();
                game.Update(delta, time, frame.GetProperty("wallDelta").GetDouble());
                double meterBeforeInput = (game as EnduranceTraining)?.Meter ?? 0;
                foreach (var action in frame.GetProperty("actions").EnumerateArray()) game.Input(action.GetInt32());
                void Number(double actual, string field) => Check(Math.Abs(actual - frame.GetProperty(field).GetDouble()) < 1e-9, $"{time} {field}: {actual} vs {frame.GetProperty(field)}");
                void Text(string actual, string field) => Check(actual == frame.GetProperty(field).GetString(), $"{time} {field}: {actual} vs {frame.GetProperty(field)}");
                Number(game.TimeLeft, "timeLeft"); Text(game.Left, "left"); Text(game.Right, "right"); Text(game.Feedback, "feedback");
                Check(game.DrainSounds().SequenceEqual(frame.GetProperty("sounds").EnumerateArray().Select(x => x.GetString()!)), time + " sounds");
                if (frame.GetProperty("score").ValueKind == JsonValueKind.Null) Check(game.Score is null, time + " premature result");
                else { Check(game.Score is not null, time + " missing result"); Number(game.Score!.Value, "score"); }
                if (game is SwingTraining swing) Number(swing.Position, "position");
                if (game is MemoryTraining memory) Number(memory.LitPad, "litPad");
                if (game is EnduranceTraining) Number(meterBeforeInput, "meter");
            }
            var final = game.Score; game.Input(); game.Update(.05, 100);
            Check(game.Score == final && game.DrainSounds().Length == 0, "finished game cannot change score or produce input sounds");
        }
        Console.WriteLine($"Training games: {checks} original frame, input, score, sound and completed-state checks passed.");
        return checks;
    }
}

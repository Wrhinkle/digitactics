using System.Text.Json;
using DigiTactics.Presentation;
using DigiTactics.Rules;

internal static class MovePlaybackChecks
{
    public static int Run(string root)
    {
        var catalog = ContentCatalog.Load(Path.Combine(root, "content/catalog.json"));
        var lines = catalog.Variants.ToDictionary(v => v.Id, v => v.Lines.Concat(v.ExtraLines).ToDictionary(l => l.Id, StringComparer.Ordinal), StringComparer.Ordinal);
        string libraryPath = Path.Combine(root, "content/move-effects/specs.json");
        var library = new MoveLibrary(libraryPath);
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/move-playback.jsonl.gz"));
        using var gzip = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Decompress);
        using var reader = new StreamReader(gzip);
        int checks = 0, cases = 0;
        using (var header = JsonDocument.Parse(reader.ReadLine() ?? throw new InvalidDataException("Missing move playback fixture header.")))
        using (var exported = JsonDocument.Parse(File.ReadAllBytes(libraryPath)))
        {
            void Check(bool valid, string path)
            {
                checks++;
                if (!valid) throw new InvalidOperationException("Move library source mapping: " + path);
            }
            var expectedVariants = header.RootElement.GetProperty("variants");
            Check(JsonElement.DeepEquals(expectedVariants, exported.RootElement.GetProperty("variants")), "exported resolver data differs from original source");
            foreach (var variant in expectedVariants.EnumerateObject())
            foreach (var line in variant.Value.EnumerateObject())
            {
                string path = variant.Name + "/" + line.Name;
                var expected = line.Value;
                var actual = library.Line(variant.Name, line.Name);
                Check(actual.Type == expected.GetProperty("type").GetString(), path + " type");
                Check(actual.Radius == expected.GetProperty("radius").GetDouble(), path + " radius");
                var expectedMoves = expected.GetProperty("moves");
                Check(actual.Moves.Length == expectedMoves.GetArrayLength(), path + " stage count");
                for (int stage = 0; stage < actual.Moves.Length; stage++)
                {
                    var e = expectedMoves[stage];
                    var a = actual.Moves[stage];
                    string element = e.TryGetProperty("elem", out var elem) ? elem.GetString()! : "beast";
                    Check(a.Element == element, path + " element");
                    Check(a.Charge == (e.TryGetProperty("charge", out var charge) ? charge.GetString() : "aura"), path + " charge");
                    Check(a.Release == (e.TryGetProperty("release", out var release) ? release.GetDouble() : .3), path + " release");
                    Check(a.Impact == (e.TryGetProperty("impact", out var impact) ? impact.GetString() : "hit"), path + " impact");
                    Check(a.Shake == (e.TryGetProperty("shake", out var shake) ? shake.GetDouble() : 0), path + " shake");
                    var scale = e.TryGetProperty("scale", out var customScale) ? customScale : header.RootElement.GetProperty("stageScale");
                    Check(a.Scale.SequenceEqual(scale.EnumerateArray().Select(n => n.GetDouble())), path + " scale");
                    var colors = e.TryGetProperty("colors", out var customColors) ? customColors : header.RootElement.GetProperty("palette").GetProperty(element);
                    Check(a.Colors.SequenceEqual(colors.EnumerateArray().Select(n => n.GetString()!)), path + " colors");
                    var steps = e.GetProperty("fx");
                    Check(a.Steps.Length == steps.GetArrayLength(), path + " step count");
                    for (int i = 0; i < a.Steps.Length; i++)
                    {
                        Check(a.Steps[i].Primitive == steps[i][0].GetString(), path + $" step {i} primitive");
                        if (steps[i].GetArrayLength() > 1)
                        foreach (var option in steps[i][1].EnumerateObject())
                            Check(a.Steps[i].Options.Raw(option.Name) is JsonElement value && JsonElement.DeepEquals(value, option.Value), path + $" step {i}/{option.Name}");
                    }
                }
                var expectedBasic = expected.GetProperty("basic");
                Check(actual.Basic.Length == expectedBasic.GetArrayLength(), path + " basic count");
                for (int ranged = 0; ranged < actual.Basic.Length; ranged++)
                foreach (var option in expectedBasic[ranged].EnumerateObject())
                    Check(actual.Basic[ranged].Raw(option.Name) is JsonElement value && JsonElement.DeepEquals(value, option.Value), path + $" basic {ranged}/{option.Name}");
            }
            var coverage = header.RootElement.GetProperty("coverage");
            Check(coverage.GetProperty("charges").GetArrayLength() == 7, "all charge families covered");
            Check(coverage.GetProperty("families").GetArrayLength() == 10, "all basic families covered");
        }
        string? text;
        while ((text = reader.ReadLine()) is not null)
        {
            using var document = JsonDocument.Parse(text);
            var row = document.RootElement;
            string variant = row.GetProperty("variant").GetString()!, id = row.GetProperty("line").GetString()!, mode = row.GetProperty("mode").GetString()!;
            string name = row.GetProperty("name").GetString()!;
            int stage = row.GetProperty("stage").GetInt32();
            var line = lines[variant][id];
            var playback = new MovePlayback(library, variant, row.GetProperty("seed").GetUInt32());
            List<(double Amplitude, double Duration)> shakes = [];
            playback.Effects.Shake = (amplitude, duration) => shakes.Add((amplitude, duration));
            var verify = new MoveEffectSnapshotChecks(playback.Effects, name, shakes);
            static WorldPosition Point(JsonElement value) => new(value.GetProperty("x").GetDouble(), value.TryGetProperty("y", out var y) ? y.GetDouble() : 0, value.GetProperty("z").GetDouble());
            static double? OptionalNumber(JsonElement row, string property) => row.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number ? value.GetDouble() : null;
            var from = Point(row.GetProperty("from"));
            var to = Point(row.GetProperty("to"));
            var targets = row.TryGetProperty("targets", out var targetArray) ? targetArray.EnumerateArray().Select(Point).ToArray() : [];
            double? height = OptionalNumber(row, "height"), duration = OptionalNumber(row, "specialDuration");
            void Begin(WorldPosition? current) => playback.Begin(line, stage, from, current, height, duration);
            switch (mode)
            {
                case "demo": playback.Demo(line, stage, from, targets, height, duration); break;
                case "basic": playback.Basic(line, stage, from, to, height!.Value, row.GetProperty("big").GetBoolean(), row.GetProperty("ranged").GetBoolean()); break;
                case "current-target": Begin(to); playback.Flush(); break;
                case "empty-area": Begin(null); playback.Area(new(1.6, 0, 1.9), 1.75); playback.Flush(); break;
                case "empty": Begin(null); playback.Flush(); break;
                case "collected": Begin(new(9, 0, 9)); playback.Target(targets[0], new(-3, 0, -2)); playback.Target(targets[1]); playback.Area(new(1.6, 0, 1.9), .9); playback.Flush(); break;
                case "chain": Begin(to); playback.Target(targets[0]); Begin(targets[1]); playback.Target(targets[1]); playback.Flush(); break;
                case "clear": Begin(to); playback.Target(targets[0]); playback.Clear(); playback.Flush(); break;
                case "zero-height": Begin(to); playback.Target(to); playback.Flush(); break;
                default: throw new InvalidDataException("Unknown playback fixture mode: " + mode);
            }
            verify.Snapshot(row.GetProperty("initial"));
            int frameIndex = 0;
            foreach (var frame in row.GetProperty("frames").EnumerateArray())
            {
                verify.At = $"frame {frameIndex++}";
                playback.Update(frame.GetProperty("dt").GetDouble());
                if (frame.TryGetProperty("snapshot", out var expected)) verify.Snapshot(expected);
            }
            verify.Near(playback.Effects.Random(), row.GetProperty("nextRandom").GetDouble(), "random draw ordering", 1e-12);
            checks += verify.Checks;
            cases++;
        }
        Console.WriteLine($"Move playback: {checks:N0} source checks across {cases:N0} compositions passed (all exported mappings, charges, basic families, release clipping and target collection).");
        return checks;
    }
}

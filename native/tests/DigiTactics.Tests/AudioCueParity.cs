using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Presentation;
using DigiTactics.Rules;

internal static class AudioCueParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        var elements = new AbilityElements(Path.Combine(root, "content/ability-elements.json"));
        int checks = 0, cues = 0;
        void Check(bool ok, string field) { checks++; if (!ok) throw new InvalidOperationException("Audio cues: " + field); }
        var valid = new HashSet<string>(["fire", "water", "ice", "thunder", "plant", "wind", "holy", "dark", "metal", "beast"]);
        foreach (var rules in catalog.Variants)
            foreach (var line in rules.Lines.Concat(rules.ExtraLines))
                for (int stage = 0; stage < 3; stage++) Check(valid.Contains(elements.Get(rules.Id, line.Id, stage)), "element coverage");
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/audio-cues.json.gz"));
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        using var data = JsonDocument.Parse(gzip);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        foreach (var trace in data.RootElement.EnumerateArray())
        {
            string variant = trace.GetProperty("variant").GetString()!;
            var combat = new CombatSimulation(catalog.Variants.Single(v => v.Id == variant),
                trace.GetProperty("sideA").Deserialize<CombatEntry[]>(options)!, trace.GetProperty("sideB").Deserialize<CombatEntry[]>(options)!,
                trace.GetProperty("seed").GetUInt32(), trace.GetProperty("cols").GetInt32(), setup: new(new(), new()));
            var audio = new CombatAudioCues(combat, variant, elements, skipHistory: false);
            int tick = 0;
            foreach (var sample in trace.GetProperty("samples").EnumerateArray())
            {
                while (tick < sample.GetProperty("ticks").GetInt32()) { combat.Step(); tick++; }
                var actual = new List<string>(); audio.Update(actual.Add);
                var expected = sample.GetProperty("calls").EnumerateArray().Select(e => e.GetString()!).ToArray();
                Check(actual.SequenceEqual(expected), $"{variant}/{tick}: {string.Join(',', actual)} != {string.Join(',', expected)}");
                cues += actual.Count; actual.Clear(); audio.Update(actual.Add);
                Check(actual.Count == 0, "repeated frame replays audio");
                var scouting = new CombatAudioCues(combat, variant, elements);
                scouting.Update(actual.Add); Check(actual.Count == 0, "scouting replays historical audio");
            }
        }
        Console.WriteLine($"Audio cue parity: {checks} checks, {cues} original-renderer cues.");
        return checks;
    }
}

using System.Text.Json;
using DigiTactics.Assets;

internal static class DanceChoreographyChecks
{
    public static int Run(string root)
    {
        using var file = File.OpenRead(Path.Combine(root, "tests/fixtures/dance-choreography.json.gz"));
        using var gzip = new System.IO.Compression.GZipStream(file, System.IO.Compression.CompressionMode.Decompress);
        using var fixture = JsonDocument.Parse(gzip); int checks = 0;
        foreach (var row in fixture.RootElement.EnumerateArray())
        {
            string id = row.GetProperty("id").GetString()!; double seconds = row.GetProperty("seconds").GetDouble();
            void Check(bool ok, string why) { checks++; if (!ok) throw new InvalidOperationException($"Choreography {id}/{seconds:R}: {why}"); }
            void Near(double a, double e, string key) => Check(Math.Abs(a - e) < 1e-5, $"{key}: {a:R} vs {e:R}");
            Check(DanceChoreography.Bpm(id) == row.GetProperty("bpm").GetInt32(), "tempo");
            var pose = DanceChoreography.Evaluate(id, seconds); int vectors = 0, scalars = 0, roots = 0;
            foreach (var p in row.GetProperty("pose").EnumerateObject())
            {
                if (p.Value.ValueKind == JsonValueKind.Array)
                {
                    vectors++; Check(pose.Vectors.ContainsKey(p.Name), "missing " + p.Name); var v = pose.Vectors[p.Name];
                    Near(v.X, p.Value[0].GetDouble(), p.Name + ".x"); Near(v.Y, p.Value[1].GetDouble(), p.Name + ".y"); Near(v.Z, p.Value[2].GetDouble(), p.Name + ".z");
                }
                else if (p.Value.ValueKind == JsonValueKind.Number)
                { scalars++; Check(pose.Scalars.ContainsKey(p.Name), "missing " + p.Name); Near(pose.Scalars[p.Name], p.Value.GetDouble(), p.Name); }
                else if (p.Name == "rt") foreach (var r in p.Value.EnumerateObject())
                { roots++; Check(pose.Root.ContainsKey(r.Name), "missing root " + r.Name); Near(pose.Root[r.Name], r.Value.GetDouble(), r.Name); }
            }
            Check(vectors == pose.Vectors.Count && scalars == pose.Scalars.Count && roots == pose.Root.Count, "unexpected channels");
        }
        foreach (string? id in new string?[] { null, "unknown", "mystery_daily_dance_0", "mystery_vault_dance_21" })
        {
            checks++; if (DanceChoreography.Bpm(id) != 120 || DanceChoreography.Duration(id) != 4) throw new InvalidOperationException("Default dance fallback failed.");
        }
        Console.WriteLine($"Dance choreography: {checks:N0} original continuous pose/channel/tempo checks passed."); return checks;
    }
}

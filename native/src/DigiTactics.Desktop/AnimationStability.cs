using System.Text.Json;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>
/// Whole-catalog run of <see cref="CombatAnimationProbe"/> (scale stability and transition continuity of board playback).
/// Launch: DigiTactics.Desktop.exe --animation-stability &lt;dir&gt; [--stability-filter text] [--stability-start N --stability-count N]
/// Writes stability.json (all sources) and exits nonzero when a source breaks the scale or continuity limits.
/// </summary>
internal static class AnimationStability
{
    /// <summary>Lists every scale channel per clip for one source (diagnosis of a body-scale flag).</summary>
    public static void DumpScales(string directory, string key)
    {
        var catalog = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
        string assets = Path.Combine(AppContext.BaseDirectory, "assets");
        var source = catalog.Sources[key]; using var asset = catalog.Open(assets, source.Asset);
        var rig = GltfRig.Load(asset).Prepare(source, assets);
        var lines = new List<string> { $"{key} {source.Asset} uid={(source.Metadata.TryGetProperty("uid"u8, out var uid) ? uid.ToString() : "?")}" };
        foreach (var clip in rig.Animations)
            foreach (var channel in clip.Channels.Where(c => c.Path == AnimationPath.Scale))
            {
                var node = rig.Nodes[channel.Node]; double min = channel.Values.Min(v => Math.Min(v.X, Math.Min(v.Y, v.Z))), max = channel.Values.Max(v => Math.Max(v.X, Math.Max(v.Y, v.Z)));
                if (Math.Abs(min - node.Rest.Scale.X) < 1e-3 && Math.Abs(max - node.Rest.Scale.X) < 1e-3) continue;
                lines.Add($"{clip.Name,-10} {node.Name,-24} parent={(node.Parent >= 0 ? rig.Nodes[node.Parent].Name : "-"),-20} rest={node.Rest.Scale.X:0.###} min={min:0.###} max={max:0.###} first={channel.Values[0].X:0.###} keys={channel.Times.Length} dur={clip.Duration:0.##}");
            }
        Directory.CreateDirectory(directory);
        File.AppendAllLines(Path.Combine(directory, "scales.txt"), lines);
    }

    public static int Run(string directory, string? filter, int start, int count)
    {
        Directory.CreateDirectory(directory);
        var catalog = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
        string assets = Path.Combine(AppContext.BaseDirectory, "assets");
        var sources = catalog.Sources.OrderBy(pair => pair.Key, StringComparer.Ordinal).Skip(start).Take(count)
            .Where(pair => filter is null || pair.Key.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToArray();
        var reports = new CombatAnimationProbe.Report[sources.Length];
        Parallel.For(0, sources.Length, new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 2) }, i =>
        {
            var (key, source) = sources[i];
            try
            {
                using var asset = catalog.Open(assets, source.Asset);
                var rig = GltfRig.Load(asset).Prepare(source, assets); var mesh = GltfMesh.Load(asset);
                reports[i] = CombatAnimationProbe.Measure(key, source.Asset, rig, mesh);
            }
            catch (Exception error) { reports[i] = new(key, source.Asset, 0, [], [], [], [], 0, [], 0, 0, ["error"], error.Message); }
        });
        int failures = reports.Count(r => r.Flags.Length > 0);
        var summary = new
        {
            Generated = DateTime.Now, Sources = reports.Length, Failures = failures,
            Limits = new { CombatAnimationProbe.MaxBodyScale, CombatAnimationProbe.MinBodyScale, CombatAnimationProbe.PopLimit, CombatAnimationProbe.PopFactor, CombatAnimationProbe.Step },
            Script = CombatAnimationProbe.Script.Select(e => $"{e.Time:0.00}s {e.Kind}").Append($"{CombatAnimationProbe.ScriptEnd:0.00}s end").ToArray(),
            Flagged = reports.Where(r => r.Flags.Length > 0).Select(r => $"{r.Source}: {string.Join(", ", r.Flags)}").ToArray(),
            Results = reports,
        };
        File.WriteAllText(Path.Combine(directory, "stability.json"), JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Animation stability: {reports.Length} sources, {failures} flagged -> {Path.Combine(directory, "stability.json")}");
        return failures == 0 ? 0 : 1;
    }
}

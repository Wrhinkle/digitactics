using System.Text.Json;
using DigiTactics.Assets;

internal static class BoardSceneChecks
{
    public static int Run(string root)
    {
        string folder = Path.Combine(root, "content/boards");
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "manifest.json")));
        int checks = 0, instances = 0;
        var kinds = new HashSet<string>();
        var ids = new HashSet<string>();
        void Check(bool ok, string message) { checks++; if (!ok) throw new InvalidOperationException("Board scenes: " + message); }
        Check(manifest.RootElement.GetProperty("version").GetInt32() == 1, "manifest version");
        foreach (var entry in manifest.RootElement.GetProperty("themes").EnumerateArray())
        {
            var scene = BoardScene.Load(Path.Combine(folder, entry.GetProperty("file").GetString()!));
            Check(ids.Add(scene.Id) && scene.Id == entry.GetProperty("id").GetString(), "theme identity");
            Check(scene.Objects.Length == entry.GetProperty("objects").GetInt32(), "object count");
            int count = scene.Objects.Sum(o => o.Instances?.Length ?? 1); instances += count;
            Check(count == entry.GetProperty("instances").GetInt32(), "instance count");
            Check(scene.Geometries.Length == entry.GetProperty("geometries").GetInt32(), "geometry count");
            Check(scene.Textures.Length == entry.GetProperty("textures").GetInt32(), "texture count");
            Check(scene.TickCount == entry.GetProperty("ticks").GetInt32(), "animation inventory");
            foreach (var obj in scene.Objects) kinds.Add(obj.Kind);
        }
        Check(ids.Count == 37, "all original themes");
        Check(kinds.Contains("mesh") && kinds.Contains("points") && kinds.Contains("lines"), "mesh, particles and grid outlines");
        Console.WriteLine($"Board scenes: {checks:N0} asset inventory checks passed for {ids.Count} themes and {instances:N0} instances; geometry, materials and references validated.");
        return checks;
    }
}

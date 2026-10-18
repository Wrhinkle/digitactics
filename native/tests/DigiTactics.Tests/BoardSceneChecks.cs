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
            foreach (var obj in scene.Objects.Where(o => o.Kind == "mesh"))
            {
                var parts = BoardMesh.Create(scene, obj);
                var source = scene.Geometries[obj.Geometry];
                Check(parts.Length == (obj.Materials.Length == 1 ? 1 : source.Groups.Length), "material group conversion");
                foreach (var part in parts)
                {
                    Check(part.Primitive.VertexCount == source.Position.Length / 3, "converted vertex count");
                    Check(part.Primitive.Colors.Length == (scene.Materials[part.Material].VertexColors ? source.Color?.Length ?? 0 : 0), "converted vertex colors");
                }
            }
        }
        Check(ids.Count == 37, "all original themes");
        Check(kinds.Contains("mesh") && kinds.Contains("points") && kinds.Contains("lines"), "mesh, particles and grid outlines");
        var sample = BoardScene.Load(Path.Combine(folder, "file-island.json.gz"));
        using var uvFixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"tests/fixtures/board-uv.json")));
        foreach (var row in uvFixture.RootElement.EnumerateArray())
        {
            float[] Values(string key) => row.GetProperty(key).EnumerateArray().Select(x => x.GetSingle()).ToArray();
            var texture = new BoardTexture(1,2,Convert.ToBase64String([255,0,0,255,0,0,255,255]),true,Values("repeat"),Values("offset"),Values("center"),row.GetProperty("rotation").GetSingle(),1000,1001,"srgb");
            var obj = sample.Objects[0] with { Geometry = 0, Materials = [0] };
            var scene = sample with { Textures = [texture], Materials = [sample.Materials[0] with { Map = 0 }],
                Geometries = [new([0,0,0],null,Values("uv"),null,[0],[])] };
            var converted = BoardMesh.Create(scene,obj)[0].Primitive.Vertices;
            var expected = Values("expected");
            Check(Math.Abs(converted[6]-expected[0]) < 2e-5 && Math.Abs(converted[7]-expected[1]) < 2e-5, "Three UV matrix parity");
            Check(BoardMesh.Image(texture).Rgba.SequenceEqual(new byte[] {0,0,255,255,255,0,0,255}), "canvas texture row orientation");
        }
        Console.WriteLine($"Board scenes: {checks:N0} asset inventory checks passed for {ids.Count} themes and {instances:N0} instances; geometry, materials and references validated.");
        return checks;
    }
}

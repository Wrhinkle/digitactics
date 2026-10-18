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
            foreach (var obj in scene.Objects)
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
        using var lightingFixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"tests/fixtures/board-lighting.json")));
        foreach (var row in lightingFixture.RootElement.EnumerateArray())
        {
            var lighting = new BoardLighting(BoardScene.Load(Path.Combine(folder,row.GetProperty("id").GetString()+".json.gz")));
            var input=row.GetProperty("input");
            lighting.HemisphereMultiplier=input.GetProperty("hemi").GetSingle(); lighting.SunMultiplier=input.GetProperty("sun").GetSingle();
            lighting.Flash=input.GetProperty("flash").GetSingle(); lighting.TintWeight=input.GetProperty("tintK").GetSingle();
            lighting.Tint=BoardLighting.Linear(input.GetProperty("tint").GetString()!); lighting.FlashColor=BoardLighting.Linear(input.GetProperty("flashColor").GetString()!);
            lighting.Update(input.GetProperty("dt").GetSingle());
            void Near(float a,float b)=>Check(Math.Abs(a-b)<2e-5,"source light controls");
            void Vector(System.Numerics.Vector3 value,string key) { var e=row.GetProperty(key); Near(value.X,e[0].GetSingle()); Near(value.Y,e[1].GetSingle()); Near(value.Z,e[2].GetSingle()); }
            Vector(lighting.Sky,"sky"); Vector(lighting.Ground,"ground"); Vector(lighting.Sun,"sun");
            Near(lighting.HemisphereIntensity,row.GetProperty("hemiIntensity").GetSingle()); Near(lighting.SunIntensity,row.GetProperty("sunIntensity").GetSingle()); Near(lighting.Flash,row.GetProperty("flash").GetSingle());
        }
        using var surfaceFixture=JsonDocument.Parse(File.ReadAllText(Path.Combine(root,"tests/fixtures/board-surface-materials.json")));
        foreach(var row in surfaceFixture.RootElement.EnumerateArray())
        {
            var surfaces=BoardSurfaces.From(BoardScene.Load(Path.Combine(folder,row.GetProperty("id").GetString()+".json.gz")));
            foreach(var expected in row.GetProperty("materials").EnumerateArray())
            {
                var material=expected.GetProperty("kind").GetString() switch { "mine"=>surfaces.Mine,"enemy"=>surfaces.Enemy,"enemyPlanning"=>surfaces.EnemyPlanning,"slot"=>surfaces.Slot,"ped"=>surfaces.Ped,_=>throw new InvalidDataException() };
                void Near(float a,float b)=>Check(Math.Abs(a-b)<2e-6,"source surface material");
                for(int i=0;i<4;i++) Near(material.Color[i],expected.GetProperty("color")[i].GetSingle());
                for(int i=0;i<3;i++) Near(material.Emissive[i],expected.GetProperty("emissive")[i].GetSingle());
                Near(material.Roughness,expected.GetProperty("roughness").GetSingle()); Near(material.Metalness,expected.GetProperty("metalness").GetSingle());
                Check((material.Map is not null)==expected.GetProperty("map").GetBoolean(),"source surface texture assignment");
            }
        }
        Console.WriteLine($"Board scenes: {checks:N0} asset, surface and lighting checks passed for {ids.Count} themes and {instances:N0} instances; geometry, materials and references validated.");
        return checks+BoardFeedbackChecks.Run(root)+BoardBurstChecks.Run(root)+BoardMotionChecks.Run(root)+BoardParticleChecks.Run(root);
    }
}

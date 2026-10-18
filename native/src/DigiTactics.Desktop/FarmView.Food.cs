using System.IO.Compression;
using System.Numerics;
using System.Text.Json;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

internal sealed partial class FarmView
{
    private sealed record FoodModel(Room Data, GpuPrimitive[] Geometry);
    private sealed class FoodInstance(string kind, double x, double z)
    {
        public string Kind { get; } = kind;
        public double X { get; } = x;
        public double Z { get; } = z;
        public double Remaining { get; set; } = 1.6;
        public double Scale => Math.Max(.01,Math.Min(1,Remaining/.5)) * (Kind == "bigmeat" ? 1.6 : 1);
    }
    private readonly Dictionary<string, FoodModel> foodModels = [];
    private readonly List<FoodInstance> food = [];
    public int FoodCount => food.Count;
    private void ShowFood(string id, string kind)
    {
        if (!actors.TryGetValue(id, out var actor)) return;
        if (kind is not ("meat" or "carrot" or "bigmeat" or "sirloin" or "cake")) throw new InvalidDataException("Unknown farm food prop.");
        if (!foodModels.ContainsKey(kind))
        {
            using var file = File.OpenRead(Path.Combine(AppContext.BaseDirectory,"content/farm-rooms","food-"+kind+".json.gz"));
            using var gzip = new GZipStream(file,CompressionMode.Decompress);
            var data = JsonSerializer.Deserialize<Room>(gzip,new JsonSerializerOptions { PropertyNameCaseInsensitive=true }) ?? throw new InvalidDataException("Missing food geometry.");
            if (data.SchemaVersion != 2 || data.Theme != "food-"+kind || data.Textures.Length != 0) throw new InvalidDataException("Unsupported food geometry.");
            var geometry = new List<GpuPrimitive>();
            try { foreach(var mesh in data.Meshes) geometry.Add(new(new(mesh.Vertices,mesh.Indices,-1,true,false))); }
            catch { foreach(var primitive in geometry) primitive.Dispose(); throw; }
            foodModels.Add(kind,new(data,geometry.ToArray()));
        }
        var a = actor.Resident;
        food.Add(new(kind,a.X+Math.Sin(actor.Animator.Yaw)*.45,a.Z+Math.Cos(actor.Animator.Yaw)*.45));
    }
    private void UpdateFood(double delta)
    {
        foreach(var item in food) item.Remaining -= delta;
        food.RemoveAll(item => item.Remaining <= 0);
    }
    private void DrawFood(Matrix4x4 camera, bool shadow)
    {
        foreach(var item in food)
        {
            var model = foodModels[item.Kind];
            var placement = GpuModel.Transform(item.X,.25,item.Z,0,item.Scale);
            foreach(bool transparent in new[] {false,true}) for(int i=0;i<model.Data.Meshes.Length;i++)
            {
                var mesh=model.Data.Meshes[i]; var material=model.Data.Materials[mesh.Material];
                if(material.Transparent != transparent || shadow && !mesh.CastShadow) continue;
                renderer.ReceiveShadow=mesh.ReceiveShadow;
                renderer.DrawRoom(model.Geometry[i],Matrix4D.From(mesh.Matrix)*placement,camera,
                    new(material.Color[0],material.Color[1],material.Color[2],material.Color[3]),null,material.Unlit,transparent,material.DepthWrite,
                    new(material.Emissive[0]*material.EmissiveIntensity,material.Emissive[1]*material.EmissiveIntensity,material.Emissive[2]*material.EmissiveIntensity),material.Roughness,material.Metalness,material.Side,material.ShadowSide);
                if(shadow) ShadowDrawCalls++; else DrawCalls++;
            }
        }
    }
}

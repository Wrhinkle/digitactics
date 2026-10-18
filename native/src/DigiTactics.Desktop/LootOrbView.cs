using System.Numerics;
using System.Text.Json;
using DigiTactics.Assets;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed class LootOrbView : IDisposable
{
    private sealed record Shape(float[] Vertices, uint[] Indices);
    private sealed record Geometry(Shape Core, Shape Glow, Shape Burst);
    private readonly GpuPrimitive core, glow, burst;
    private readonly Dictionary<int,LootOrb> previous = [];
    private readonly Dictionary<int,double> rotations = [];
    private readonly List<(LootOrb Orb,double Start)> bursts = [];
    private IReadOnlyList<LootOrb> orbs = [];
    private bool visible;
    private double time;
    public int DrawCalls { get; private set; }
    public int ShadowDrawCalls { get; private set; }
    public int BurstDrawCalls { get; private set; }
    public static Vector3 Position(LootOrb orb, double time) => new((float)(-5.2+orb.X*10.4), (float)(.6+Math.Sin(time*2.5+orb.Id*1.7)*.12), (float)(.9+orb.Z*4.2));
    public static bool InRange(LootOrb orb,double x,double z) { return Math.Sqrt(Math.Pow((-5.2+orb.X*10.4)-x,2)+Math.Pow((.9+orb.Z*4.2)-z,2)) <= .85; }
    private static Vector3 Color(LootOrb orb) => BoardLighting.Linear(orb.Kind == "gold" ? "#ffcf4a" : "#4fb3ff");
    public LootOrbView()
    {
        var data=JsonSerializer.Deserialize<Geometry>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"content/loot-geometry.json")),new JsonSerializerOptions {PropertyNameCaseInsensitive=true})!;
        static GpuPrimitive Upload(Shape shape) => new(new(shape.Vertices,shape.Indices,-1,true,false));
        core=Upload(data.Core); glow=Upload(data.Glow); burst=Upload(data.Burst);
    }
    public void Reset() { previous.Clear(); rotations.Clear(); bursts.Clear(); orbs=[]; }
    public void Sync(IReadOnlyList<LootOrb> value,double now,bool show)
    {
        time=now; visible=show; DrawCalls=ShadowDrawCalls=BurstDrawCalls=0;
        if(!show) return;
        var ids=value.Select(o=>o.Id).ToHashSet();
        foreach(var old in previous.Values) if(!ids.Contains(old.Id)) bursts.Add((old,now));
        foreach(var id in rotations.Keys.Where(id=>!ids.Contains(id)).ToArray()) rotations.Remove(id);
        previous.Clear(); foreach(var orb in value) { previous.Add(orb.Id,orb); rotations[orb.Id]=rotations.GetValueOrDefault(orb.Id)+.02; }
        orbs=value.ToArray(); bursts.RemoveAll(b=>now-b.Start>=.45);
    }
    public void Draw(ModelRenderer renderer,Matrix4x4 camera,bool transparent)
    {
        if(!visible || renderer.ShadowPass && transparent) return;
        foreach(var orb in orbs)
        {
            var p=Position(orb,time); var color=Color(orb);
            var rotation=Quaternion.CreateFromAxisAngle(Vector3.UnitY,(float)rotations[orb.Id]);
            var world=Matrix4D.Compose(new(p.X,p.Y,p.Z),new(rotation.X,rotation.Y,rotation.Z,rotation.W),new(1,1,1));
            renderer.DrawRoom(transparent?glow:core,world,camera,new(color.X,color.Y,color.Z,transparent?.25f:1),null,
                transparent,transparent,!transparent,transparent?Vector3.Zero:color*.8f,roughness:.2f,side:0);
            if(renderer.ShadowPass) ShadowDrawCalls++; else DrawCalls++;
        }
        if(!transparent) return;
        foreach(var effect in bursts)
        {
            double age=(time-effect.Start)/.45, scale=.5+age*.6;
            var p=Position(effect.Orb,time); var color=Color(effect.Orb); var q=Quaternion.CreateFromAxisAngle(Vector3.UnitX,-MathF.PI/2);
            var world=Matrix4D.Compose(new(p.X,.3,p.Z),new(q.X,q.Y,q.Z,q.W),new(scale,scale,scale));
            renderer.DrawRoom(burst,world,camera,new(color.X,color.Y,color.Z,(float)(.9*(1-age))),null,true,true,false);
            DrawCalls++; BurstDrawCalls++;
        }
    }
    public void Dispose() { core.Dispose(); glow.Dispose(); burst.Dispose(); }
}

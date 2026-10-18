using System.Numerics;
using DigiTactics.Assets;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

/// <summary>Owns board triangle, line and point geometry. Scene animation updates are separate.</summary>
internal sealed class BoardMeshLayer : IDisposable
{
    private sealed record Part(GpuPrimitive Geometry, BoardMaterial Material, bool HasNormals, BoardTexture? Texture);
    private sealed record ObjectParts(BoardObject Object, Part[] Parts);
    private readonly List<GpuPrimitive> geometry = [];
    private readonly List<GpuTexture> textures = [];
    private readonly List<ObjectParts> objects = [];
    private readonly List<(GpuPrimitive Geometry,float[] Vertices,BoardMotion Motion)> animatedGeometry=[];
    private readonly List<(GpuPrimitive Geometry,float[] Vertices,float[] Initial,string Kind)> animatedUv=[];
    private double time;
    private BoardLightning? lightning;
    private readonly BoardCastleMotion? castle;
    private readonly BoardKowloonMotion? kowloon;
    private readonly BoardCircuitMotion? circuit;
    private readonly BoardTempleMotion? temple;
    private static readonly Vector3 templeSunTint=BoardLighting.Linear("#fff6e0");
    public (float Multiplier,float Weight,Vector3 Tint)? SunFlare=>temple is null?null:(temple.SunMultiplier,temple.TintWeight,templeSunTint);
    public (float Near,float Far)? AnimatedFog=>kowloon is null?null:(kowloon.FogNear,kowloon.FogFar);
    public float LightFlash=>(float)(lightning?.Flash??circuit?.LightFlash??temple?.LightFlash??0);
    public Vector3? FlashColor { get; private set; }
    private readonly Dictionary<BoardObject,BoardSpiralMotion> spirals=[];
    public int DrawCalls { get; private set; }
    public Matrix4D Root { get; set; } = Matrix4D.Identity;
    public BoardMeshLayer(BoardScene scene,bool initialCombat=false,bool initialWon=false,int initialEvolution=0)
    {
        if(scene.Id=="vampire-castle")castle=new(initialCombat);
        if(scene.Id=="kowloon")kowloon=new();
        if(scene.Id=="circuit-core") {circuit=new(initialWon);FlashColor=BoardLighting.Linear("#a8ffc8");}
        if(scene.Id=="flame-temple") {temple=new(scene.Objects.Select(o=>o.Motion).FirstOrDefault(m=>m?.Kind=="temple-geysers")?.Temple??[],initialEvolution);FlashColor=BoardLighting.Linear("#fff0c0");}
        try
        {
            foreach (var texture in scene.Textures)
                textures.Add(new(BoardMesh.Image(texture), new(WrapS: BoardMesh.Wrap(texture.WrapS), WrapT: BoardMesh.Wrap(texture.WrapT)), texture.ColorSpace == "srgb"));
            var cache = new Dictionary<string, Part[]>();
            foreach (var obj in scene.Objects)
            {
                if(lightning is null && obj.Motion?.Lightning is {} data)
                {
                    lightning=new(data.Spots,data.Flash,data.DelaySeed,data.DelayCalls,data.BoltCalls);
                    lightning.Advance(0,0,initialCombat);
                    FlashColor=BoardLighting.Linear(data.FlashColor);
                }
                if(obj.Motion is { Spirals:not null } spiral) spirals.Add(obj,new(spiral));
                uint topology = obj.Kind switch { "mesh" => Brutal.OpenGlApi.OpenGl.GL_TRIANGLES, "lines" => Brutal.OpenGlApi.OpenGl.GL_LINES, "lineStrip" => Brutal.OpenGlApi.OpenGl.GL_LINE_STRIP, "points" => Brutal.OpenGlApi.OpenGl.GL_POINTS, _ => throw new NotSupportedException("Board object: " + obj.Kind) };
                string key = obj.Kind + ":" + obj.Geometry + ":" + string.Join(',', obj.Materials);
                if (!cache.TryGetValue(key, out var parts))
                {
                    parts = BoardMesh.Create(scene, obj).Select(part =>
                    {
                        var gpu = new GpuPrimitive(part.Primitive, topology); geometry.Add(gpu);
                        if(obj.Motion is { Kind:"sea" or "particles" or "circuit-pulses" } motion) animatedGeometry.Add((gpu,part.Primitive.Vertices,motion));
                        if(obj.Motion is { Kind:"desert-haze" or "grid-floor" or "circuit-screen" or "circuit-floor" or "temple-lava" } scrolling) animatedUv.Add((gpu,part.Primitive.Vertices,(float[])part.Primitive.Vertices.Clone(),scrolling.Kind));
                        var material = scene.Materials[part.Material];
                        return new Part(gpu, material, part.Primitive.HasNormals, material.Map is { } map ? scene.Textures[map] : null);
                    }).ToArray();
                    cache.Add(key, parts);
                }
                objects.Add(new(obj, parts));
            }
        }
        catch { Dispose(); throw; }
    }

    public void Update(double seconds,double delta=0,bool combat=false,int stage=1,bool won=false,int evolution=0)
    {
        time=seconds;
        lightning?.Advance(delta,time,combat);
        castle?.Advance(delta,time,combat,stage);
        kowloon?.Advance(delta,combat);
        circuit?.Advance(delta,combat,won);
        temple?.Advance(delta,time,combat,stage,evolution);
        foreach(var spiral in spirals.Values) spiral.Advance(delta,time,combat);
        foreach(var uv in animatedUv)
        {
            if(uv.Kind is "circuit-screen" or "circuit-floor") circuit!.Scroll(uv.Vertices,uv.Initial,uv.Kind,time);
            else if(uv.Kind=="temple-lava") BoardTempleMotion.Scroll(uv.Vertices,uv.Initial,time);
            else BoardSceneMotion.ScrollingUv(uv.Vertices,uv.Initial,time,uv.Kind);
            uv.Geometry.UpdateVertices(uv.Vertices);
        }
        foreach(var wave in animatedGeometry)
        {
            if(wave.Motion.Kind=="sea") BoardSceneMotion.Sea(wave.Vertices,time,wave.Motion);
            else if(wave.Motion.Kind=="circuit-pulses")circuit!.Pulses(wave.Vertices,wave.Motion.Pulses!);
            else BoardSceneMotion.Particles(wave.Vertices,delta,time,wave.Motion);
            wave.Geometry.UpdateVertices(wave.Vertices);
        }
    }

    public void Draw(ModelRenderer renderer, Matrix4x4 camera, bool transparent, int viewportHeight)
    {
        DrawCalls = 0;
        foreach (var item in objects)
        {
            var bolt=item.Object.Motion?.Lightning;
            if(bolt is not null) { if(lightning is not { Visible:true } || lightning.Active!=bolt.Index) continue; }
            else if (!item.Object.Visible) continue;
            if (renderer.ShadowPass && !item.Object.CastShadow) continue;
            var world = Matrix4D.From(item.Object.Matrix) * Root;
            if(bolt is not null) world=new CompanionTransform(new(lightning!.X,lightning.Y,lightning.Z),new(0,lightning.Yaw,0),new(1,1,1)).Matrix*Root;
            if(item.Object.Motion is { Kind:"rotating-parent" } rotating) world=BoardSceneMotion.RotatingParent(rotating,time)*Root;
            float? opacity=BoardSceneMotion.Opacity(item.Object.Motion?.Kind,time);
            if(item.Object.Motion is {Kind:"circuit-screen" or "circuit-floor"} code)opacity=circuit!.Opacity(code.Kind);
            if(item.Object.Motion is {Kind:"kowloon-band" or "kowloon-lattice"} scan)
            {
                var animated=BoardKowloonMotion.Scan(scan.Kind,time);
                world=(Matrix4D.From(item.Object.Matrix) with {M42=animated.Y})*Root;opacity=animated.Opacity;
            }
            if(item.Object.Motion?.KowloonRain==true)opacity=kowloon!.RainOpacity;
            if(item.Object.Motion?.Kind=="castle-halo")opacity=castle!.HaloOpacity(time);
            if(item.Object.Motion is { Kind:"beach-surf" } surf)
            {
                var animated=BoardSceneMotion.Surf(surf,time); world=animated.Matrix*Root; opacity=animated.Opacity;
            }
            foreach (var part in item.Parts)
            {
                var material = part.Material;
                var color=item.Object.Motion?.Kind=="castle-moon"?castle!.MoonColor:new Vector3(material.Color[0],material.Color[1],material.Color[2]);
                if(item.Object.Motion?.Kind=="temple-lava")color=new(temple!.LavaColor);
                if (material.Transparent != transparent) continue;
                void Draw(Matrix4D matrix, float r, float g, float b)
                {
                    renderer.ReceiveShadow = item.Object.ReceiveShadow;
                    renderer.DrawRoom(part.Geometry, matrix, camera,
                        new(color.X * r, color.Y * g, color.Z * b, opacity??material.Color[3]),
                        material.Map is { } map ? textures[map] : null, material.Unlit, transparent, material.DepthWrite,
                        new Vector3(material.Emissive[0], material.Emissive[1], material.Emissive[2]) * material.EmissiveIntensity,
                        material.Roughness, material.Metalness, material.Side, additive: material.Blending == 2,
                        vertexColors: material.VertexColors, flatShading: material.FlatShading || !part.HasNormals, alphaTest: material.AlphaTest, wireframe: material.Wireframe,
                        points: item.Object.Kind == "points" ? PointSprite.Board(material, part.Texture, viewportHeight,item.Object.Motion?.KowloonRain==true?kowloon!.RainSize:item.Object.Motion?.TempleEmbers==true?temple!.EmberSize:null) : null, fog: material.Fog,
                        emissiveMap:material.EmissiveMap is {} emissionMap?textures[emissionMap]:null);
                    DrawCalls++;
                }
                if (item.Object.Instances is { } instances)
                    for(int i=0;i<instances.Length;i++)
                    {
                        var instance=instances[i];
                        var matrix=spirals.TryGetValue(item.Object,out var spiral)?spiral.Matrices[i]:item.Object.Motion switch
                        {
                            { Kind:"island-crystals",Items:{ } crystals }=>BoardSceneMotion.Crystal(crystals[i],time),
                            { Kind:"beach-palms",Palms:{ } palms }=>BoardSceneMotion.Palm(palms[i],time),
                            { Props:{ } props } motion=>BoardSceneMotion.Prop(motion.Kind,props[i],time),
                            { Castle:{ } items } motion=>castle!.Instance(motion.Kind,items[i],i,time),
                            { Kind:"temple-geysers" }=>temple!.Geysers[i],
                            { Temple:{ } flames } motion=>temple!.Flame(flames[i],motion.Kind=="temple-inner",time),
                            _=>Matrix4D.From(instance.Matrix)
                        };
                        var tint=item.Object.Motion?.Signs is {} signs?BoardKowloonMotion.Sign(signs[i],time):new Vector3(instance.Color[0],instance.Color[1],instance.Color[2]);
                        Draw(matrix*world,tint.X,tint.Y,tint.Z);
                    }
                else Draw(world, 1, 1, 1);
            }
        }
    }

    public void Dispose()
    {
        foreach (var primitive in geometry) primitive.Dispose(); geometry.Clear();
        foreach (var texture in textures) texture.Dispose(); textures.Clear();
        objects.Clear();
        spirals.Clear();
        animatedGeometry.Clear();
        animatedUv.Clear();
    }
}

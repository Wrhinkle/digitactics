using System.Numerics;
using DigiTactics.Assets;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

/// <summary>Owns board triangle, line and point geometry. Scene animation updates are separate.</summary>
internal sealed class BoardMeshLayer : IDisposable
{
    private sealed record Part(GpuPrimitive Geometry, BoardMaterial Material, bool HasNormals, BoardTexture? Texture);
    private sealed record ObjectParts(BoardObject Object, Part[] Parts) {public double SortDepth;}
    private readonly List<GpuPrimitive> geometry = [];
    private readonly List<GpuTexture> textures = [];
    private readonly List<ObjectParts> objects = [];
    private readonly List<ObjectParts> transparentObjects=[];
    private readonly List<(GpuPrimitive Geometry,float[] Vertices,BoardMotion Motion)> animatedGeometry=[];
    private readonly List<(GpuPrimitive Geometry,float[] Vertices,float[] Initial,string Kind)> animatedUv=[];
    private double time;
    private int currentStage;
    private readonly Dictionary<BoardObject,BoardCityBuilding> buildings=[];
    private readonly Dictionary<BoardObject,BoardMysteryMotion> mystery=[];
    private BoardLightning? lightning;
    private readonly BoardCastleMotion? castle;
    private readonly BoardKowloonMotion? kowloon;
    private readonly BoardCircuitMotion? circuit;
    private readonly BoardTempleMotion? temple;
    private readonly BoardUnderwaterMotion? underwater;
    private readonly BoardFreezeMotion? freeze;
    private readonly BoardSewerMotion? sewer;
    private readonly BoardLibraryMotion? library;
    private readonly BoardCrowdMotion? crowd;
    private readonly BoardStudioMotion? studio;
    private readonly BoardTrainMotion? train;
    private readonly BoardGraveyardMotion? graveyard;
    private readonly BoardPyramidMotion? pyramid;
    private readonly BoardHolidayMotion? holiday;
    private readonly BoardFireworks? fireworks;
    private readonly Vector3[] fireworkColors=[];
    private readonly BoardConfetti? confetti;
    private static readonly Vector3 libraryTint=BoardLighting.Linear("#ffd090");
    private static readonly Vector3 freezeCalmFog=BoardLighting.Linear("#24405e"),freezeStormFog=BoardLighting.Linear("#8aa6c0");
    public Vector3? AnimatedFogColor=>freeze is null?null:Vector3.Lerp(freezeCalmFog,freezeStormFog,(float)freeze.Weather);
    private static readonly Vector3 templeSunTint=BoardLighting.Linear("#fff6e0");
    public (float Multiplier,float Weight,Vector3 Tint)? SunFlare=>library is not null?(1,library.TintWeight,libraryTint):temple is null?null:(temple.SunMultiplier,temple.TintWeight,templeSunTint);
    public (float Near,float Far)? AnimatedFog=>graveyard is not null?(graveyard.FogNear,graveyard.FogFar):freeze is not null?(freeze.FogNear,freeze.FogFar):kowloon is null?null:(kowloon.FogNear,kowloon.FogFar);
    public float LightFlash=>(float)(lightning?.Flash??circuit?.LightFlash??temple?.LightFlash??library?.LightFlash??0);
    public Vector3? FlashColor { get; private set; }
    private readonly Dictionary<BoardObject,BoardSpiralMotion> spirals=[];
    public int DrawCalls { get; private set; }
    public Matrix4D Root { get; set; } = Matrix4D.Identity;
    public BoardMeshLayer(BoardScene scene,bool initialCombat=false,bool initialWon=false,int initialEvolution=0,int initialStage=1)
    {
        currentStage=initialStage;
        foreach(var obj in scene.Objects)if(obj.Motion is {Kind:"mystery-satellites",Props:{} props})mystery.Add(obj,new(props));
        foreach(var obj in scene.Objects)if(obj.Motion?.Building is {} building)buildings.Add(obj,new(building,initialStage));
        if(scene.Objects.Select(o=>o.Motion?.Fireworks).FirstOrDefault(f=>f is not null) is {} fw)
        {fireworks=new(fw.Count,fw.Max,fw.Colors.Length,fw.Seed,fw.Calls,initialWon);fireworkColors=fw.Colors.Select(BoardLighting.Linear).ToArray();}
        if(scene.Id=="vampire-castle")castle=new(initialCombat);
        if(scene.Id=="holiday-world")holiday=new(initialWon);
        if(scene.Id=="server-pyramid")pyramid=new(initialCombat);
        if(scene.Id=="overdell-graveyard")graveyard=new();
        if(scene.Id=="trailmon-station")train=new(initialCombat);
        if(scene.Id=="battle-studio"){studio=new();confetti=new(scene.Objects.FirstOrDefault(o=>o.Motion?.Kind=="studio-confetti")?.Instances?.Length??0,initialWon);}
        if(scene.Id=="knights-coliseum"){crowd=new(initialCombat,initialWon);confetti=new(scene.Objects.FirstOrDefault(o=>o.Motion?.Kind=="knights-confetti")?.Instances?.Length??0,initialWon);}
        if(scene.Id=="witchelny-library"){library=new(initialEvolution);FlashColor=BoardLighting.Linear("#ffe8a0");}
        if(scene.Id=="numemon-sewer")sewer=new(initialWon);
        if(scene.Id=="freezeland")freeze=new();
        if(scene.Id=="kowloon")kowloon=new();
        if(scene.Id=="gekomon-castle")underwater=new(initialCombat);
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
                    lightning=new(data.Spots,data.Flash,data.DelaySeed,data.DelayCalls,data.BoltCalls,data.FirstDelay,data.DelayBase,data.DelayRange,data.StrikeOnCombat);
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
                        if(obj.Motion is { Kind:"sea" or "particles" or "circuit-pulses" or "freeze-aurora" or "knights-flashes" or "studio-sticks" or "carnival-fireworks" } motion) animatedGeometry.Add((gpu,part.Primitive.Vertices,motion));
                        if(obj.Motion is { Kind:"desert-haze" or "grid-floor" or "circuit-screen" or "circuit-floor" or "temple-lava" or "underwater-caustics" or "underwater-caustics2" or "sewer-pour" or "studio-led" or "pyramid-bands" } scrolling) animatedUv.Add((gpu,part.Primitive.Vertices,(float[])part.Primitive.Vertices.Clone(),scrolling.Kind));
                        var material = scene.Materials[part.Material];
                        return new Part(gpu, material, part.Primitive.HasNormals, material.Map is { } map ? scene.Textures[map] : null);
                    }).ToArray();
                    cache.Add(key, parts);
                }
                var item=new ObjectParts(obj,parts);objects.Add(item);
                if(parts.Any(p=>p.Material.Transparent))transparentObjects.Add(item);
            }
        }
        catch { Dispose(); throw; }
    }

    public void Update(double seconds,double delta=0,bool combat=false,int stage=1,bool won=false,int evolution=0)
    {
        time=seconds;currentStage=stage;
        foreach(var building in buildings.Values)building.Advance(delta,stage);
        foreach(var motion in mystery.Values)motion.Advance(delta);
        freeze?.Advance(delta,combat);
        sewer?.Advance(time,won);
        library?.Advance(delta,evolution);
        crowd?.Advance(time,combat,won);
        studio?.Advance(delta,combat);
        train?.Advance(time,combat);
        graveyard?.Advance(delta,combat);
        pyramid?.Advance(delta,time,combat,stage);
        holiday?.Advance(time,won);
        fireworks?.Advance(delta,time,won);
        confetti?.Advance(delta,won);
        lightning?.Advance(delta,time,combat);
        castle?.Advance(delta,time,combat,stage);
        kowloon?.Advance(delta,combat);
        circuit?.Advance(delta,combat,won);
        temple?.Advance(delta,time,combat,stage,evolution);
        underwater?.Advance(delta,time,combat);
        foreach(var spiral in spirals.Values) spiral.Advance(delta,time,combat);
        foreach(var uv in animatedUv)
        {
            if(uv.Kind is "circuit-screen" or "circuit-floor") circuit!.Scroll(uv.Vertices,uv.Initial,uv.Kind,time);
            else if(uv.Kind=="temple-lava") BoardTempleMotion.Scroll(uv.Vertices,uv.Initial,time);
            else if(uv.Kind is "underwater-caustics" or "underwater-caustics2") BoardUnderwaterMotion.Scroll(uv.Vertices,uv.Initial,uv.Kind,time);
            else BoardSceneMotion.ScrollingUv(uv.Vertices,uv.Initial,time,uv.Kind);
            uv.Geometry.UpdateVertices(uv.Vertices);
        }
        foreach(var wave in animatedGeometry)
        {
            if(wave.Motion.Kind=="sea") BoardSceneMotion.Sea(wave.Vertices,time,wave.Motion);
            else if(wave.Motion.Kind=="circuit-pulses")circuit!.Pulses(wave.Vertices,wave.Motion.Pulses!);
            else if(wave.Motion.Fireworks is {} fw)
            {var burst=fireworks!.Bursts[fw.Index];if(!burst.Visible)continue;var positions=burst.Positions;for(int i=0;i<positions.Length/3;i++)Array.Copy(positions,i*3,wave.Vertices,i*GltfPrimitive.Stride,3);}
            else if(wave.Motion.Kind=="studio-sticks")BoardStudioMotion.Sticks(wave.Vertices,wave.Motion.Crowd!,time);
            else if(wave.Motion.Kind=="knights-flashes")crowd!.Flashes(wave.Vertices,wave.Motion.Crowd!,time);
            else if(wave.Motion.Aurora is {} aurora)BoardFreezeMotion.Aurora(wave.Vertices,aurora,time);
            else {BoardSceneMotion.Particles(wave.Vertices,delta,time,wave.Motion);if(wave.Motion.FreezeSnow)freeze!.Snow(wave.Vertices,delta); }
            wave.Geometry.UpdateVertices(wave.Vertices);
        }
    }

    public void Draw(ModelRenderer renderer, Matrix4x4 camera, bool transparent, int viewportHeight)
    {
        DrawCalls = 0;
        var drawObjects=objects;
        if(transparent && !renderer.ShadowPass)
        {
            var view=new Matrix4D(camera.M11,camera.M12,camera.M13,camera.M14,camera.M21,camera.M22,camera.M23,camera.M24,camera.M31,camera.M32,camera.M33,camera.M34,camera.M41,camera.M42,camera.M43,camera.M44);
            foreach(var item in transparentObjects)item.SortDepth=BoardDrawOrder.Depth(item.Object,World(item.Object),view);
            transparentObjects.Sort((a,b)=>BoardDrawOrder.Compare(a.Object,a.SortDepth,b.Object,b.SortDepth));drawObjects=transparentObjects;
        }
        foreach (var item in drawObjects)
        {
            var bolt=item.Object.Motion?.Lightning;
            if(bolt is not null) { if(lightning is not { Visible:true } || lightning.Active!=bolt.Index) continue; }
            else if(item.Object.Motion?.Kind is "knights-confetti" or "studio-confetti"){if(confetti?.Visible!=true)continue;}
            else if(item.Object.Motion?.Fireworks is {} burst){if(!fireworks!.Bursts[burst.Index].Visible)continue;}
            else if(buildings.TryGetValue(item.Object,out var building)){if(!building.Visible)continue;}
            else if(item.Object.Motion?.Kind=="city-crops"){if(currentStage<2)continue;}
            else if (!item.Object.Visible) continue;
            if (renderer.ShadowPass && !item.Object.CastShadow) continue;
            var world = World(item.Object);
            float? opacity=BoardSceneMotion.Opacity(item.Object.Motion?.Kind,time);
            if(item.Object.Motion?.Fireworks is {} firework)opacity=(float)fireworks!.Bursts[firework.Index].Opacity;
            if(item.Object.Motion?.Kind is "pyramid-beams" or "pyramid-bands")opacity=pyramid!.Opacity(item.Object.Motion.Kind,(int)item.Object.Motion.Speed,time);
            if(item.Object.Motion?.HolidaySparkle==true)opacity=holiday!.SparkleOpacity;
            if(item.Object.Motion?.TrainSteam==true)opacity=train!.SteamOpacity;
            if(item.Object.Motion?.Kind=="library-circle")opacity=library!.Opacity(time);
            if(item.Object.Motion?.Aurora is {} aurora)opacity=freeze!.Opacity((int)(aurora.Ph/2));
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
                if(item.Object.Motion?.Fireworks is {} fw){int c=fireworks!.Bursts[fw.Index].ColorIndex;color=c<0?Vector3.One:fireworkColors[c];}
                if(item.Object.Motion?.Kind is "studio-beam" or "studio-pool")color=studio!.LightColor((int)item.Object.Motion.Speed);
                if(item.Object.Motion?.Kind=="studio-strip")color=BoardStudioMotion.Strip((int)item.Object.Motion.Speed,time);
                if(item.Object.Motion?.Kind=="studio-sticks")color=studio!.StickColor;
                if(item.Object.Motion?.Kind=="library-circle")color=library!.CircleColor;
                if(item.Object.Motion?.LibrarySparks==true)color=library!.SparkColor;
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
                        points: item.Object.Kind == "points" ? PointSprite.Board(material, part.Texture, viewportHeight,item.Object.Motion?.HolidaySnow==true?holiday!.SnowSize:item.Object.Motion?.FreezeSnow==true?freeze!.SnowSize:item.Object.Motion?.KowloonRain==true?kowloon!.RainSize:item.Object.Motion?.TempleEmbers==true?temple!.EmberSize:null) : null, fog: material.Fog,
                        emissiveMap:material.EmissiveMap is {} emissionMap?textures[emissionMap]:null,
                        renderOrder:item.Object.RenderOrder,sortDepth:item.SortDepth);
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
                            {Kind:"mystery-satellites"} motion=>mystery[item.Object].Satellite(i,(int)motion.Speed,time),
                            {Carnival:{} carnival} motion=>BoardCarnivalMotion.Instance(motion.Kind,i,carnival[i],time),
                            {Ghosts:{} ghosts}=>graveyard!.Ghost(ghosts[i],time),
                            {Kind:"train-cars" or "train-engine"} motion=>train!.Instance(motion.Kind,i,instance.Matrix),
                            {Kind:"knights-confetti" or "studio-confetti"}=>ConfettiMatrix(confetti!.Poses[i]),
                            {Kind:"studio-audience",Crowd:{} audience}=>BoardStudioMotion.Audience(audience[i],time),
                            {Crowd:{} people} motion=>crowd!.Instance(motion.Kind,people[i],time),
                            {Library:{} books} motion=>BoardLibraryMotion.Instance(motion.Kind,books[i],time),
                            {Kind:"sewer-numemon" or "sewer-duck",Props:{} sewerItems} motion=>sewer!.Instance(motion.Kind,sewerItems[i],time),
                            { Props:{ } props } motion=>BoardSceneMotion.Prop(motion.Kind,props[i],time),
                            { Castle:{ } items } motion=>castle!.Instance(motion.Kind,items[i],i,time),
                            { Kind:"temple-geysers" }=>temple!.Geysers[i],
                            { Temple:{ } flames } motion=>temple!.Flame(flames[i],motion.Kind=="temple-inner",time),
                            { Underwater:{ } swimmers } motion=>underwater!.Instance(motion.Kind,swimmers[i],time),
                            _=>Matrix4D.From(instance.Matrix)
                        };
                        var tint=item.Object.Motion?.Phases is {} phases?holiday!.Color(item.Object.Motion.Kind,i,phases[i],time):item.Object.Motion?.Signs is {} signs?BoardKowloonMotion.Sign(signs[i],time):new Vector3(instance.Color[0],instance.Color[1],instance.Color[2]);
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
        transparentObjects.Clear();
        spirals.Clear();buildings.Clear();mystery.Clear();
        animatedGeometry.Clear();
        animatedUv.Clear();
    }
    internal static Matrix4D ConfettiMatrix(BoardConfettiPose p)=>new CompanionTransform(new(p.X,p.Y,p.Z),new(p.Rx,0,p.Rz),new(1,1,1)).Matrix;
    private Matrix4D World(BoardObject obj)
    {
        if(obj.Motion is {Kind:"mystery-monument" or "mystery-center" or "mystery-blades" or "mystery-rings" or "mystery-meteor"} motion)return BoardMysteryMotion.World(motion,time)*Root;
        if(obj.Motion?.Kind=="carnival-wheel")return BoardCarnivalMotion.Wheel(time)*Root;
        if(buildings.TryGetValue(obj,out var building))return building.Matrix*Root;
        if(obj.Motion?.Kind=="pyramid-beams")return pyramid!.Beam(obj.Matrix)*Root;
        if(obj.Motion?.TrainSteam==true)return train!.Steam*Root;
        if(obj.Motion?.Kind=="train-clock")return BoardTrainMotion.Clock(obj.Matrix,time)*Root;
        if(obj.Motion?.Kind is "studio-beam" or "studio-pool")return studio!.Light((int)obj.Motion.Speed,obj.Motion.Kind=="studio-beam",time)*Root;
        if(obj.Motion?.Kind=="studio-crane")return BoardStudioMotion.Crane(obj.Motion,time)*Root;
        if(obj.Motion?.Kind is "library-circle" or "library-outer")return library!.Circle(obj.Motion.Kind,obj.Matrix)*Root;
        if(obj.Motion?.Lightning is not null)return new CompanionTransform(new(lightning!.X,lightning.Y,lightning.Z),new(0,lightning.Yaw,0),new(1,1,1)).Matrix*Root;
        if(obj.Motion is {Kind:"rotating-parent"} rotating)return BoardSceneMotion.RotatingParent(rotating,time)*Root;
        if(obj.Motion is {Kind:"beach-surf"} surf)return BoardSceneMotion.Surf(surf,time).Matrix*Root;
        if(obj.Motion is {Kind:"kowloon-band" or "kowloon-lattice"} scan)return (Matrix4D.From(obj.Matrix) with {M42=BoardKowloonMotion.Scan(scan.Kind,time).Y})*Root;
        return Matrix4D.From(obj.Matrix)*Root;
    }
}

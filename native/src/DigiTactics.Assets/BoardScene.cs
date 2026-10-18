using System.IO.Compression;
using System.Text.Json;

namespace DigiTactics.Assets;

/// <summary>Source board geometry and materials. Animation is implemented separately; these are initial scene states.</summary>
public sealed record BoardScene(int Version, string Id, string[] Sky, JsonElement Fog, JsonElement Hemi,
    JsonElement Sun, JsonElement Tiles, string Slot, string Ped, JsonElement Look,
    Dictionary<string, int> Surface, int TickCount, BoardTexture[] Textures, BoardMaterial[] Materials,
    BoardGeometry[] Geometries, BoardObject[] Objects, int? Background = null)
{
    public static BoardScene Load(string path)
    {
        using var file = File.OpenRead(path);
        using var gzip = new GZipStream(file, CompressionMode.Decompress);
        var scene = JsonSerializer.Deserialize<BoardScene>(gzip, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("Missing board scene.");
        scene.Validate();
        return scene;
    }

    private void Validate()
    {
        void Require(bool valid, string field) { if (!valid) throw new InvalidDataException($"Invalid board {Id}: {field}."); }
        static bool Finite(float[] values) => values.All(float.IsFinite);
        Require(Version == 1 && !string.IsNullOrWhiteSpace(Id), "schema");
        Require(Sky.Length > 0 && TickCount >= 0, "environment");
        Require(Background is null || Background >= 0 && Background < Textures.Length, "background reference");
        Require(Sky.Length == 1 || Background is not null, "gradient background");
        foreach (var texture in Textures)
        {
            Require(texture.Width > 0 && texture.Height > 0, "texture dimensions");
            Require(Convert.FromBase64String(texture.Rgba).LongLength == (long)texture.Width * texture.Height * 4, "texture pixels");
            Require(texture.Repeat.Length == 2 && texture.Offset.Length == 2 && texture.Center.Length == 2, "texture transform");
        }
        foreach (var material in Materials)
        {
            Require(material.Color.Length == 4 && Finite(material.Color) && material.Emissive.Length == 3 && Finite(material.Emissive), "material color");
            Require(material.Map is null || material.Map >= 0 && material.Map < Textures.Length, "texture reference");
            Require(material.EmissiveMap is null || material.EmissiveMap >= 0 && material.EmissiveMap < Textures.Length,"emissive texture reference");
        }
        foreach (int texture in Surface.Values) Require(texture >= 0 && texture < Textures.Length, "surface reference");
        foreach (var geometry in Geometries)
        {
            int vertices = geometry.Position.Length / 3;
            Require(vertices > 0 && geometry.Position.Length % 3 == 0 && Finite(geometry.Position), "positions");
            Require(geometry.Normal is null || geometry.Normal.Length == vertices * 3 && Finite(geometry.Normal), "normals");
            Require(geometry.Uv is null || geometry.Uv.Length == vertices * 2 && Finite(geometry.Uv), "UVs");
            Require(geometry.Color is null || geometry.Color.Length == vertices * 3 && Finite(geometry.Color), "vertex colors");
            Require(geometry.Indices is null || geometry.Indices.All(i => i < vertices), "indices");
            int count = geometry.Indices?.Length ?? vertices;
            foreach (var group in geometry.Groups) Require(group.Start >= 0 && group.Count >= 0 && (long)group.Start + group.Count <= count, "geometry group");
        }
        foreach (var obj in Objects)
        {
            Require(obj.Kind is "mesh" or "sprite" or "points" or "lines" or "lineStrip", "object kind");
            Require(obj.Geometry >= 0 && obj.Geometry < Geometries.Length, "geometry reference");
            Require(obj.Materials.Length > 0 && obj.Materials.All(i => i >= 0 && i < Materials.Length), "material references");
            Require(obj.Matrix.Length == 16 && obj.Matrix.All(double.IsFinite), "world matrix");
            Require(obj.SortCenter is null || obj.SortCenter.Length==3 && obj.SortCenter.All(double.IsFinite),"sort center");
            if(obj.Motion?.Ghosts is {} ghosts)
            {
                Require(Id=="overdell-graveyard" && obj.Motion.Kind=="graveyard-ghosts" && obj.Instances is not null && ghosts.Length==obj.Instances.Length,"graveyard ghosts");
                Require(ghosts.All(p=>new[] {p.A0,p.Rad,p.H,p.Ph,p.Sp}.All(double.IsFinite)),"ghost values");
            }
            if(obj.Motion?.TrainSteam==true)Require(Id=="trailmon-station" && obj.Motion.Kind=="particles" && obj.Kind=="points","train steam");
            if(obj.Motion?.Kind is "train-cars" or "train-engine")Require(Id=="trailmon-station" && obj.Instances is not null,"train instances");
            if(obj.Motion?.Crowd is {} crowd)
            {
                Require((Id=="knights-coliseum" && obj.Motion.Kind is "knights-crowd" or "knights-banners" or "knights-flashes") || (Id=="battle-studio" && obj.Motion.Kind is "studio-audience" or "studio-sticks"),"crowd kind");
                Require(crowd.Length==(obj.Kind=="points"?Geometries[obj.Geometry].Position.Length/3:obj.Instances?.Length),"crowd count");
                Require(crowd.All(p=>new[] {p.X,p.Y,p.Z,p.By,p.A,p.Ry,p.Ph,p.S}.All(double.IsFinite)),"crowd values");
            }
            if(obj.Motion?.Library is {} library)
            {
                Require(Id=="witchelny-library" && obj.Motion.Kind is "library-pages" or "library-candles" or "library-flames" or "library-halos","library kind");
                Require(obj.Instances is not null && library.Length==obj.Instances.Length,"library instance count");
                Require(library.All(p=>new[] {p.X,p.Z,p.By,p.Ph,p.A0,p.Sp,p.Rad,p.H}.All(double.IsFinite)),"library values");
            }
            if(obj.Motion?.LibrarySparks==true)Require(Id=="witchelny-library" && obj.Motion.Kind=="particles" && obj.Kind=="points","library sparks");
            if(obj.Motion?.Aurora is {} aurora)
            {
                Require(Id=="freezeland" && obj.Motion.Kind=="freeze-aurora","aurora kind");
                Require(aurora.Base.Length==Geometries[obj.Geometry].Position.Length/3 && aurora.Base.All(v=>v.Length==2 && v.All(double.IsFinite)),"aurora base");
                Require(new[] {aurora.R,aurora.A0,aurora.A1,aurora.Y,aurora.H,aurora.Ph}.All(double.IsFinite),"aurora values");
            }
            if(obj.Motion?.FreezeSnow==true)Require(Id=="freezeland" && obj.Motion.Kind=="particles" && obj.Kind=="points","freezeland snow");
            if(obj.Motion?.Underwater is {} underwater)
            {
                Require(Id=="gekomon-castle" && obj.Motion.Kind is "underwater-shafts" or "underwater-kelp" or "underwater-clams" or "underwater-fish","underwater kind");
                Require(obj.Instances is not null && underwater.Length==obj.Instances.Length,"underwater instance count");
                Require(underwater.All(p=>new[] {p.X,p.Y,p.Z,p.Ph,p.S,p.Ry,p.Rz,p.Sx??1,p.Sy??1,p.Sz??1,p.Rad,p.H,p.Off,p.Dir}.All(double.IsFinite)),"underwater values");
            }
            if(obj.Motion?.Temple is {} temple)
            {
                Require(Id=="flame-temple" && obj.Motion.Kind is "temple-outer" or "temple-inner" or "temple-geysers","temple kind");
                Require(obj.Instances is not null && temple.Length==obj.Instances.Length,"temple instance count");
                Require(temple.All(p=>new[] {p.X,p.Y,p.Z,p.Bx,p.Bz,p.Ph,p.H,p.T0}.All(double.IsFinite)),"temple values");
            }
            if(obj.Motion?.TempleEmbers==true)Require(Id=="flame-temple" && obj.Motion.Kind=="particles" && obj.Kind=="points","temple embers");
            if(obj.Motion?.Pulses is {} pulses)
            {
                Require(Id=="circuit-core" && obj.Motion.Kind=="circuit-pulses" && pulses.Length==Geometries[obj.Geometry].Position.Length/3,"circuit pulse count");
                Require(pulses.All(p=>double.IsFinite(p.Lane) && double.IsFinite(p.Off) && double.IsFinite(p.Sp)),"circuit pulse values");
            }
            if(obj.Motion?.Signs is {} signs)
            {
                Require(obj.Motion.Kind=="kowloon-signs" && obj.Instances is not null && signs.Length==obj.Instances.Length,"neon sign count");
                Require(signs.All(s=>double.IsFinite(s.Phase) && s.Color.Length==3 && Finite(s.Color)),"neon sign values");
            }
            if(obj.Motion?.KowloonRain==true)Require(Id=="kowloon" && obj.Kind=="points" && obj.Motion.Kind=="particles","Kowloon rain");
            if(obj.Motion?.Castle is {} castle)
            {
                Require(Id=="vampire-castle" && obj.Motion.Kind is "castle-flames" or "castle-bats","castle kind");
                Require(obj.Instances is not null && castle.Length==obj.Instances.Length,"castle instance count");
                Require(castle.All(p=>new[] {p.X,p.Y,p.Z,p.Ph,p.S,p.A0,p.Rad,p.H,p.Sp}.All(double.IsFinite)),"castle values");
            }
            if(obj.Motion?.Lightning is {} lightning)
            {
                Require((obj.Motion.Kind is "spiral-lightning" or "graveyard-lightning") && lightning.Index is >=0 and <3,"lightning kind/index");
                Require(lightning.Spots.Length>0 && lightning.Spots.All(s=>s.Length==3 && s.All(double.IsFinite)),"lightning spots");
                Require(lightning.DelayCalls>=0 && lightning.BoltCalls>=0 && new[] {lightning.Flash,lightning.FirstDelay,lightning.DelayBase,lightning.DelayRange}.All(double.IsFinite) && lightning.FirstDelay>=0 && lightning.DelayBase>=0 && lightning.DelayRange>=0,"lightning parameters");
                _=BoardLighting.Linear(lightning.FlashColor);
            }
            if(obj.Motion is { Spirals:{ } spirals } spiralMotion)
            {
                Require(spiralMotion.Kind is "spiral-rocks" or "spiral-islets" or "spiral-crystals" or "spiral-halos","spiral kind");
                Require(obj.Instances is not null && spirals.Length==obj.Instances.Length,"spiral instance count");
                Require(spirals.All(p=>new[] {p.X,p.Y,p.Z,p.K,p.A0,p.Rad,p.S,p.Sp,p.Ph,p.Phase}.All(double.IsFinite)),"spiral values");
            }
            if(obj.Motion is { Kind:"rotating-parent" } rotating)
            {
                Require(rotating.Matrix?.Length==16 && rotating.Parent?.Length==16,"rotating parent matrices");
                Require(rotating.Matrix!.All(double.IsFinite) && rotating.Parent!.All(double.IsFinite) && double.IsFinite(rotating.Speed),"rotating parent values");
            }
            if(obj.Motion is { Props:{ } props } propMotion)
            {
                Require(propMotion.Kind is "sewer-numemon" or "sewer-duck" or "freeze-men" or "village-eggs" or "village-blocks" or "toy-tops" or "toy-train" or "coliseum-banners" or "coliseum-flames" or "savanna-gears" or "grid-polys","prop motion kind");
                Require(obj.Instances is not null && props.Length==obj.Instances.Length,"prop motion count");
                Require(props.All(p=>new[] {p.X,p.Y,p.Z,p.S,p.Phase,p.Base,p.Cx,p.Cz,p.K,p.Ry,p.Rx,p.Spin}.All(double.IsFinite)),"prop motion values");
            }
            if(obj.Motion is { Kind:"particles" } motion)
            {
                Require(motion.Box?.Length==6 && motion.Velocity?.Length==3 && motion.Speeds?.Length==Geometries[obj.Geometry].Position.Length/3,"particle motion dimensions");
                Require(motion.Box!.All(double.IsFinite) && motion.Velocity!.All(double.IsFinite) && Finite(motion.Speeds!),"particle motion values");
                var box=motion.Box!; Require(box[0]<box[1] && box[2]<box[3] && box[4]<box[5],"particle bounds");
            }
            // Three ignores geometry groups for single-material meshes.
            if (obj.Materials.Length > 1)
                foreach (var group in Geometries[obj.Geometry].Groups) Require(group.MaterialIndex >= 0 && group.MaterialIndex < obj.Materials.Length, "group material");
            foreach (var instance in obj.Instances ?? [])
            {
                Require(instance.Matrix.Length == 16 && instance.Matrix.All(double.IsFinite), "instance matrix");
                Require(instance.Color.Length == 3 && Finite(instance.Color), "instance color");
            }
        }
    }
}

public sealed record BoardTexture(int Width, int Height, string Rgba, bool FlipY, float[] Repeat,
    float[] Offset, float[] Center, float Rotation, int WrapS, int WrapT, string ColorSpace);
public sealed record BoardMaterial(float[] Color, int? Map, bool Unlit, bool Transparent, bool DepthWrite,
    int Side, bool Fog, bool FlatShading, float AlphaTest, float[] Emissive, float EmissiveIntensity,
    float Roughness, float Metalness, bool VertexColors, bool Wireframe, int Blending, float Size, bool SizeAttenuation,int? EmissiveMap=null);
public sealed record BoardGeometry(float[] Position, float[]? Normal, float[]? Uv, float[]? Color,
    uint[]? Indices, BoardGeometryGroup[] Groups);
public sealed record BoardGeometryGroup(int Start, int Count, int MaterialIndex);
public sealed record BoardObject(string Kind, int Geometry, int[] Materials, double[] Matrix, bool Visible,
    bool CastShadow, bool ReceiveShadow, BoardInstance[]? Instances, BoardMotion? Motion = null,double[]? SortCenter=null,int SortId=0,int RenderOrder=0);
public sealed record BoardInstance(double[] Matrix, float[] Color);
public sealed record BoardMotion(string Kind,double Amp=0,double Speed=0,BoardCrystal[]? Items=null,double[]? Box=null,double[]? Velocity=null,float[]? Speeds=null,double Wobble=0,BoardPalm[]? Palms=null,double[]? Matrix=null,BoardProp[]? Props=null,double[]? Parent=null,BoardSpiralItem[]? Spirals=null,BoardLightningData? Lightning=null,BoardCastleItem[]? Castle=null,BoardNeonSign[]? Signs=null,bool KowloonRain=false,BoardCircuitPulse[]? Pulses=null,BoardTempleItem[]? Temple=null,bool TempleEmbers=false,BoardUnderwaterItem[]? Underwater=null,BoardAurora? Aurora=null,bool FreezeSnow=false,BoardLibraryItem[]? Library=null,bool LibrarySparks=false,BoardCrowdItem[]? Crowd=null,bool TrainSteam=false,BoardGhostItem[]? Ghosts=null);
public sealed record BoardUnderwaterItem(double X=0,double Y=0,double Z=0,double Ph=0,double S=1,double Ry=0,double Rz=0,double? Sx=null,double? Sy=null,double? Sz=null,double Rad=0,double H=0,double Off=0,double Dir=0);
public sealed record BoardTempleItem(double X=0,double Y=0,double Z=0,double Bx=0,double Bz=0,double Ph=0,double H=0,double T0=-9);
public sealed record BoardCircuitPulse(bool Along,double Lane,double Off,double Sp);
public sealed record BoardNeonSign(double Phase,float[] Color);
public sealed record BoardCastleItem(double X=0,double Y=0,double Z=0,double Ph=0,double S=1,double A0=0,double Rad=0,double H=0,double Sp=0);
public sealed record BoardLightningData(int Index,double[][] Spots,double Flash,string FlashColor,uint DelaySeed,int DelayCalls,int BoltCalls,double FirstDelay=1.5,double DelayBase=1.2,double DelayRange=2.5,bool StrikeOnCombat=true);
public sealed record BoardSpiralItem(double X=0,double Y=0,double Z=0,double K=0,double A0=0,double Rad=0,double S=1,double Sp=0,double Ph=0,double Phase=0);
public sealed record BoardProp(double X=0,double Y=0,double Z=0,double S=1,double Phase=0,double Base=0,double Cx=0,double Cz=0,double K=0,double Ry=0,double Rx=0,double Spin=0);
public sealed record BoardCrystal(double X,double Z,double Sx,double Sy,double Sz,double Phase);
public sealed record BoardPalm(double X,double Z,double Ry,double S,double Phase);

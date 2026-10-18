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
    float Roughness, float Metalness, bool VertexColors, bool Wireframe, int Blending, float Size, bool SizeAttenuation);
public sealed record BoardGeometry(float[] Position, float[]? Normal, float[]? Uv, float[]? Color,
    uint[]? Indices, BoardGeometryGroup[] Groups);
public sealed record BoardGeometryGroup(int Start, int Count, int MaterialIndex);
public sealed record BoardObject(string Kind, int Geometry, int[] Materials, double[] Matrix, bool Visible,
    bool CastShadow, bool ReceiveShadow, BoardInstance[]? Instances, BoardMotion? Motion = null);
public sealed record BoardInstance(double[] Matrix, float[] Color);
public sealed record BoardMotion(string Kind,double Amp=0,double Speed=0,BoardCrystal[]? Items=null,double[]? Box=null,double[]? Velocity=null,float[]? Speeds=null,double Wobble=0);
public sealed record BoardCrystal(double X,double Z,double Sx,double Sy,double Sz,double Phase);

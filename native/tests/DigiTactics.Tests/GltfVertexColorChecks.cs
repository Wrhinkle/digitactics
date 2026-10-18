using System.IO.Compression;
using System.Text.Json;
using DigiTactics.Assets;

internal static class GltfVertexColorChecks
{
    public static int Run(string root)
    {
        int checks = 0;
        void Check(bool condition, string label)
        {
            checks++;
            if (!condition) throw new InvalidDataException("Vertex color contract: " + label);
        }
        using (var asset = Fixture(null, "VEC4", 5126, false, "OPAQUE"))
            Check(GltfMesh.Load(asset).Meshes[0][0].ColorComponents == 0, "absent color remains absent");
        foreach (int components in new[] { 3, 4 })
        {
            float[] colors = components == 3 ? [1,0,0, 0,1,0, 0,0,1] : [1,0,0,0, 0,1,0,.5f, 0,0,1,1];
            using var asset = Fixture(FloatBytes(colors), "VEC" + components, 5126, false, "BLEND");
            var primitive = GltfMesh.Load(asset).Meshes[0][0];
            Check(primitive.Colors.SequenceEqual(colors) && primitive.ColorComponents == components, "float colors and shape preserved");
            var material = GltfMaterial.Load(asset, 0, vertexAlpha: components == 4);
            Check(components == 4 ? material.Transparent && !material.DepthWrite && material.AlphaCutoff == 0 :
                !material.Transparent && material.DepthWrite && material.AlphaCutoff == .5, "RGBA preserves BLEND; RGB retains legacy cutout");
            var sharedRgb = GltfMaterial.Load(asset, 0);
            Check(!sharedRgb.Transparent && sharedRgb.DepthWrite && sharedRgb.AlphaCutoff == .5, "shared material variant remains independent");
        }
        using (var asset = Fixture([255,0,0,0, 0,255,0,128, 0,0,255,255], "VEC4", 5121, true, "BLEND"))
        {
            var colors = GltfMesh.Load(asset).Meshes[0][0].Colors;
            Check(colors.Length == 12 && Math.Abs(colors[7] - 128f / 255) < 1e-7 && colors[11] == 1 && colors[3] == 0, "normalized byte colors");
        }
        ushort[] values = [65535,0,0,0, 0,65535,0,32768, 0,0,65535,65535];
        byte[] shorts = new byte[values.Length * 2]; Buffer.BlockCopy(values, 0, shorts, 0, shorts.Length);
        using (var asset = Fixture(shorts, "VEC4", 5123, true, "BLEND"))
            Check(Math.Abs(GltfMesh.Load(asset).Meshes[0][0].Colors[7] - 32768f / 65535) < 1e-7, "normalized ushort colors");
        float[] rgba = [1,1,1,.5f, 1,1,1,.5f, 1,1,1,.5f];
        foreach (string mode in new[] { "OPAQUE", "MASK" })
        {
            using var asset = Fixture(FloatBytes(rgba), "VEC4", 5126, false, mode);
            var material = GltfMaterial.Load(asset, 0, vertexAlpha: true);
            Check(!material.Transparent && material.DepthWrite && material.AlphaCutoff == (mode == "MASK" ? .5 : 0), "authored alpha mode " + mode);
        }
        using (var asset = Fixture(FloatBytes([-1,2,0, 0,1,0, 0,0,1]), "VEC3", 5126, false, "OPAQUE"))
            Check(GltfMesh.Load(asset).Meshes[0][0].Colors.Take(3).SequenceEqual([0f,1f,0f]), "glTF color range clamping");
        Reject(FloatBytes([1,1,1,1,1,1]), "VEC2", 5126, false, 3, "shape");
        Reject(FloatBytes([1,1,1,1,1,1,1,1]), "VEC4", 5126, false, 2, "count");
        foreach (float bad in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
        {
            rgba[0] = bad; Reject(FloatBytes(rgba), "VEC4", 5126, false, 3, "nonfinite value");
        }
        Reject(new byte[12], "VEC4", 5121, false, 3, "unnormalized integer");
        Reject(new byte[12], "VEC4", 5120, true, 3, "signed integer");
        var catalog = ModelCatalog.Load(Path.Combine(root,"content/models.json"));
        using (var recovered = catalog.Open(Path.Combine(root,"artifacts/model-content"),catalog.Sources["gaomon:2"].Asset))
        {
            var primitives = GltfMesh.Load(recovered).Meshes.SelectMany(m => m).Where(p => p.ColorComponents == 4).ToArray();
            Check(primitives.Length == 4 && primitives.Sum(p => p.VertexCount) == 216 && primitives.Sum(p => p.Indices.Length / 3) == 320, "actual recovered sonic geometry");
            foreach (var primitive in primitives)
            {
                var alpha = Enumerable.Range(0,primitive.VertexCount).Select(i => primitive.Colors[i*4+3]).GroupBy(a => a).OrderBy(g => g.Key).ToArray();
                Check(alpha.Select(g => g.Count()).SequenceEqual([18,9,9,9,9]), "actual source opacity distribution");
                Check(alpha.Select(g => g.Key).Zip(new[] {0f,.250980407f,.501960814f,.749019623f,1f}).All(p => Math.Abs(p.First-p.Second) < 1e-7), "actual source opacity values");
                Check(Enumerable.Range(0,primitive.VertexCount).All(i => primitive.Colors[i*4] == 1 && primitive.Colors[i*4+1] == 1 && primitive.Colors[i*4+2] == 1), "actual source white RGB");
                var material = GltfMaterial.Load(recovered,primitive.Material,vertexAlpha:true);
                Check(material.Transparent && !material.DepthWrite && material.AlphaCutoff == 0, "actual source blend variant");
            }
        }
        Console.WriteLine($"Passed {checks} glTF RGB/RGBA loader, alpha-mode and invalid-input checks.");
        return checks;

        void Reject(byte[] bytes, string type, int component, bool normalized, int count, string label)
        {
            using var asset = Fixture(bytes, type, component, normalized, "OPAQUE", count);
            bool rejected = false;
            try { GltfMesh.Load(asset); } catch (InvalidDataException) { rejected = true; }
            Check(rejected, "reject " + label);
        }
    }

    private static byte[] FloatBytes(float[] values)
    {
        var bytes = new byte[values.Length * 4]; Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length); return bytes;
    }
    private static GltfAsset Fixture(byte[]? colors, string type, int component, bool normalized, string mode, int colorCount = 3)
    {
        byte[] positions = FloatBytes([-.5f,-.5f,0, .5f,-.5f,0, 0,.5f,0]);
        var buffer = positions.Concat(colors ?? []).ToArray(); Array.Resize(ref buffer, (buffer.Length + 3) & ~3);
        var views = new List<object> { new { buffer = 0, byteOffset = 0, byteLength = positions.Length } };
        var accessors = new List<object> { new { bufferView = 0, componentType = 5126, count = 3, type = "VEC3" } };
        var attributes = new Dictionary<string,int> { ["POSITION"] = 0 };
        if (colors is not null)
        {
            views.Add(new { buffer = 0, byteOffset = positions.Length, byteLength = colors.Length });
            accessors.Add(new { bufferView = 1, componentType = component, count = colorCount, type, normalized }); attributes["COLOR_0"] = 1;
        }
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new {
            asset = new { version = "2.0" }, buffers = new[] { new { byteLength = buffer.Length } }, bufferViews = views, accessors,
            meshes = new[] { new { primitives = new[] { new { attributes, material = 0 } } } }, nodes = new[] { new { mesh = 0 } },
            scenes = new[] { new { nodes = new[] { 0 } } }, scene = 0,
            materials = new[] { new { alphaMode = mode, pbrMetallicRoughness = new { baseColorTexture = new { index = 0 } } } }
        });
        int length = (json.Length + 3) & ~3; var padded = Enumerable.Repeat((byte)32, length).ToArray(); json.CopyTo(padded, 0);
        using var glb = new MemoryStream();
        using (var writer = new BinaryWriter(glb, System.Text.Encoding.UTF8, true))
        {
            writer.Write(0x46546c67); writer.Write(2); writer.Write(28 + length + buffer.Length);
            writer.Write(length); writer.Write(0x4e4f534a); writer.Write(padded);
            writer.Write(buffer.Length); writer.Write(0x004e4942); writer.Write(buffer);
        }
        using var gzip = new MemoryStream();
        using (var stream = new GZipStream(gzip, CompressionLevel.Fastest, true)) stream.Write(glb.ToArray());
        return GltfAsset.LoadCompressed(gzip.ToArray());
    }
}

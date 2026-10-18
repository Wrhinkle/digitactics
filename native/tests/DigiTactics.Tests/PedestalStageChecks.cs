using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using DigiTactics.Assets;
using DigiTactics.Presentation;

internal static class PedestalStageChecks
{
    public static int Run(string root)
    {
        using var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root, "tests/fixtures/pedestal-stage.json")));
        var expected = document.RootElement;
        string directory = Path.Combine(root, "content/pedestal-stage");
        using var geometry = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(directory, "geometry.json")));
        using var scene = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(directory, "scene.json")));
        int checks = 0;
        string context = "scene";
        void Check(bool valid, string message)
        {
            checks++;
            if (!valid) throw new InvalidOperationException($"Pedestal stage {context}: {message}");
        }
        void Near(double actual, double reference, string message)
            => Check(double.IsFinite(actual) && Math.Abs(actual - reference) < 1e-10, $"{message}: {actual:R} vs {reference:R}");
        void Point(WorldPosition actual, JsonElement reference, string message)
        {
            Near(actual.X, reference[0].GetDouble(), message + " x");
            Near(actual.Y, reference[1].GetDouble(), message + " y");
            Near(actual.Z, reference[2].GetDouble(), message + " z");
        }
        Check(JsonElement.DeepEquals(scene.RootElement, expected.GetProperty("scene")), "exported source lights, camera and texture sampler metadata");
        Near(PedestalStage.LegendHeight, expected.GetProperty("scene").GetProperty("legendHeight").GetDouble(), "source Little Legend height");
        Check(geometry.RootElement.EnumerateObject().Select(p => p.Name).SequenceEqual(new[] { "drum", "base", "gold", "ring", "ring2", "top" }), "original six pieces and draw order");
        foreach (var entry in expected.GetProperty("parts").EnumerateObject())
        {
            context = entry.Name;
            var reference = entry.Value; var actual = geometry.RootElement.GetProperty(entry.Name);
            foreach (string key in new[] { "position", "rotation", "scale", "material" })
                Check(JsonElement.DeepEquals(actual.GetProperty(key), reference.GetProperty(key)), "source " + key);
            var vertices = actual.GetProperty("vertices"); var indices = actual.GetProperty("indices");
            int count = reference.GetProperty("vertices").GetInt32();
            Check(vertices.GetArrayLength() == count * 16, "source vertex count and native stride");
            Check(indices.GetArrayLength() == reference.GetProperty("indices").GetInt32(), "source index count");
            var vertexBytes = new byte[vertices.GetArrayLength() * sizeof(float)];
            int offset = 0;
            foreach (var value in vertices.EnumerateArray())
            {
                float number = value.GetSingle();
                Check(float.IsFinite(number), "finite mesh component");
                BinaryPrimitives.WriteSingleLittleEndian(vertexBytes.AsSpan(offset, sizeof(float)), number); offset += sizeof(float);
            }
            var indexBytes = new byte[indices.GetArrayLength() * sizeof(uint)]; offset = 0;
            foreach (var value in indices.EnumerateArray())
            {
                uint index = value.GetUInt32(); Check(index < count, "valid source triangle index");
                BinaryPrimitives.WriteUInt32LittleEndian(indexBytes.AsSpan(offset, sizeof(uint)), index); offset += sizeof(uint);
            }
            Check(Convert.ToHexStringLower(SHA256.HashData(vertexBytes)) == reference.GetProperty("vertexSha256").GetString(), "exact original positions, normals and native UV orientation");
            Check(Convert.ToHexStringLower(SHA256.HashData(indexBytes)) == reference.GetProperty("indexSha256").GetString(), "exact source triangulation");
        }
        foreach (var camera in expected.GetProperty("cameras").EnumerateArray())
        {
            int width = camera.GetProperty("width").GetInt32(), height = camera.GetProperty("height").GetInt32();
            context = $"camera {width}x{height}";
            var actual = PedestalStage.Camera(width, height);
            Point(actual.Eye, camera.GetProperty("eye"), "aspect-aware eye");
            Point(actual.Target, camera.GetProperty("target"), "look-at point");
            Near(actual.FovRadians, camera.GetProperty("fov").GetDouble() * Math.PI / 180, "source field of view");
            Near(actual.Near, camera.GetProperty("near").GetDouble(), "near plane");
            Near(actual.Far, camera.GetProperty("far").GetDouble(), "far plane");
            Near(actual.Aspect, (double)width / height, "aspect");
            Check(actual == PedestalStage.Camera((double)width / height), "aspect and dimension overloads agree");
        }
        foreach (var pulse in expected.GetProperty("rings").EnumerateArray())
        {
            double time = pulse.GetProperty("time").GetDouble(); context = "ring at " + time;
            var actual = PedestalStage.RingColor(time); var color = pulse.GetProperty("color");
            Near(actual.R, color[0].GetDouble(), "working-linear red");
            Near(actual.G, color[1].GetDouble(), "working-linear green");
            Near(actual.B, color[2].GetDouble(), "working-linear blue");
        }
        context = "source hex texture";
        byte[] png = File.ReadAllBytes(Path.Combine(directory, "hex.png"));
        Check(Convert.ToHexStringLower(SHA256.HashData(png)) == expected.GetProperty("texture").GetProperty("sha256").GetString(), "exact Canvas2D export");
        var image = PngImage.Decode(png);
        Check(image.Width == 256 && image.Height == 256, "original 256px hex grid");
        foreach (var sample in expected.GetProperty("texture").GetProperty("samples").EnumerateArray())
        {
            int x = sample.GetProperty("x").GetInt32(), y = sample.GetProperty("y").GetInt32();
            var rgba = sample.GetProperty("rgba");
            for (int channel = 0; channel < 4; channel++)
                Check(image.Rgba[(y * image.Width + x) * 4 + channel] == rgba[channel].GetByte(), $"decoded Canvas pixel {x},{y}/{channel}");
        }
        Console.WriteLine($"Pedestal stage: {checks:N0} source checks passed (six meshes/materials, lighting, hex texture, eight camera aspects and fifteen ring colors).");
        return checks;
    }
}

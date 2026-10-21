using System.Text.Json;
using DigiTactics.Assets;

internal static class MaterialParity
{
    public static int Run(string root)
    {
        var catalog = ModelCatalog.Load(Path.Combine(root, "content/models.json"));
        using var fixture = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(root, "tests/fixtures/materials.json")));
        int checks = 0, materials = 0;
        foreach (var trace in fixture.RootElement.EnumerateArray())
        {
            string id = trace.GetProperty("asset").GetString()!;
            using var asset = catalog.Open(Path.Combine(root, "artifacts/model-content"), id);
            int index = 0;
            void Check(bool condition, string field)
            {
                checks++;
                if (!condition) throw new InvalidOperationException($"Material differs from original loader: {id}/{index}/{field}.");
            }
            void Validate(JsonElement expected, bool vertexAlpha = false)
            {
                var actual = GltfMaterial.Load(asset, index, vertexAlpha: vertexAlpha);
                double[] color = [actual.Color.R, actual.Color.G, actual.Color.B, actual.Color.A];
                for (int c = 0; c < 4; c++) Check(Math.Abs(color[c] - expected.GetProperty("color")[c].GetDouble()) < 1e-10, "color");
                Check(actual.Unlit == expected.GetProperty("unlit").GetBoolean(), "unlit");
                Check(actual.DoubleSided == expected.GetProperty("doubleSided").GetBoolean(), "doubleSided");
                Check(!GltfMaterial.Load(asset, index, frontOnly: true, vertexAlpha: vertexAlpha).DoubleSided, "frontOnly");
                Check(actual.Transparent == expected.GetProperty("transparent").GetBoolean(), "transparent");
                Check(actual.DepthWrite == expected.GetProperty("depthWrite").GetBoolean(), "depthWrite");
                Check(actual.Additive == expected.GetProperty("additive").GetBoolean(), "additive");
                Check(actual.AlphaCutoff == expected.GetProperty("alphaCutoff").GetDouble(), "alphaCutoff");
                Check(actual.Roughness == expected.GetProperty("roughness").GetDouble(), "roughness");
                Check(actual.SpecularIntensity == expected.GetProperty("specularIntensity").GetDouble(), "specularIntensity");
                Check(actual.NormalScale == expected.GetProperty("normalScale").GetDouble(), "normalScale");
                double[] emissive = [actual.Emissive.R, actual.Emissive.G, actual.Emissive.B];
                for (int c = 0; c < 3; c++) Check(Math.Abs(emissive[c] - expected.GetProperty("emissive")[c].GetDouble()) < 1e-10, "emissive");
                Check(actual.EmissiveIntensity == expected.GetProperty("emissiveIntensity").GetDouble(), "emissiveIntensity");
                void Binding(TextureBinding? binding, string field)
                {
                    var value = expected.GetProperty(field);
                    if (value.ValueKind == JsonValueKind.Null) { Check(binding is null, field); return; }
                    Check(binding is not null && binding.Texture == value.GetProperty("texture").GetInt32() &&
                        binding.TexCoord == value.GetProperty("texCoord").GetInt32() && binding.Srgb == value.GetProperty("srgb").GetBoolean(), field);
                }
                Binding(actual.ColorMap, "colorMap"); Binding(actual.RoughnessMap, "roughnessMap");
                Binding(actual.NormalMap, "normalMap"); Binding(actual.SpecularMap, "specularMap");
                Binding(actual.EmissiveMap, "emissiveMap");
                materials++;
            }
            foreach (var expected in trace.GetProperty("materials").EnumerateArray()) { Validate(expected); index++; }
            if (trace.TryGetProperty("vertexAlphaMaterials", out var variants)) foreach (var variant in variants.EnumerateArray())
            {
                index = variant.GetProperty("index").GetInt32(); Validate(variant.GetProperty("material"), vertexAlpha: true);
            }
            index = 0;
            foreach (var expected in trace.GetProperty("textures").EnumerateArray())
            {
                var actual = GltfTexture.Load(asset, index);
                Check(actual.Image == expected.GetProperty("image").GetInt32(), "image");
                Check(actual.Sampler.MagFilter == expected.GetProperty("magFilter").GetInt32(), "magFilter");
                Check(actual.Sampler.MinFilter == expected.GetProperty("minFilter").GetInt32(), "minFilter");
                Check(actual.Sampler.WrapS == expected.GetProperty("wrapS").GetInt32(), "wrapS");
                Check(actual.Sampler.WrapT == expected.GetProperty("wrapT").GetInt32(), "wrapT");
                Check(actual.Sampler.Mipmaps == expected.GetProperty("mipmaps").GetBoolean(), "mipmaps");
                index++;
            }
        }
        Console.WriteLine($"Passed {checks} original-loader material checks across {materials} materials.");
        return checks;
    }
}

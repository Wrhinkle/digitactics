using DigiTactics.Assets;
using Brutal.OpenGlApi;

namespace DigiTactics.Desktop;

internal static class TextureSmoke
{
    public static void Run()
    {
        VerifyTransferState();
        var catalog = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
        int count = 0;
        foreach (string id in catalog.Assets.Keys)
        {
            using var model = catalog.Open(Path.Combine(AppContext.BaseDirectory, "assets"), id);
            if (!model.Root.TryGetProperty("textures"u8, out var textures)) continue;
            var usages = new HashSet<(int Index, bool Srgb)>();
            if (model.Root.TryGetProperty("materials"u8, out var materials))
                for (int i = 0; i < materials.GetArrayLength(); i++)
                    foreach (var binding in GltfMaterial.Load(model, i).Textures()) usages.Add((binding.Texture, binding.Srgb));
            for (int i = 0; i < textures.GetArrayLength(); i++)
                if (!usages.Any(use => use.Index == i)) usages.Add((i, false));
            foreach (var usage in usages)
            {
                var texture = GltfTexture.Load(model, usage.Index);
                var pixels = model.Image(texture.Image);
                using var gpu = new GpuTexture(pixels, texture.Sampler, usage.Srgb);
                if (!gpu.ReadPixels().AsSpan().SequenceEqual(pixels.Rgba))
                    throw new InvalidOperationException($"GPU texture differs from source: {id}/{usage.Index}.");
                VerifySampler(gpu, texture.Sampler, usage.Srgb);
                count++;
            }
        }
        Console.WriteLine($"Native GPU texture smoke passed: {count} uploads and exact pixel readbacks.");
    }

    private static void VerifySampler(GpuTexture gpu, TextureSampler sampler, bool srgb)
    {
        int Parameter(GlEnum name)
        {
            Span<int> value = stackalloc int[1]; OpenGl.GetTextureParameterFloat(gpu.Id, name, value); return value[0];
        }
        int Level(int level, GlEnum name)
        {
            Span<int> value = stackalloc int[1]; OpenGl.GetTextureLevelParameteriv(gpu.Id, level, name, value); return value[0];
        }
        if (Parameter(OpenGl.GL_TEXTURE_MIN_FILTER) != sampler.MinFilter || Parameter(OpenGl.GL_TEXTURE_MAG_FILTER) != sampler.MagFilter ||
            Parameter(OpenGl.GL_TEXTURE_WRAP_S) != sampler.WrapS || Parameter(OpenGl.GL_TEXTURE_WRAP_T) != sampler.WrapT ||
            Level(0, OpenGl.GL_TEXTURE_INTERNAL_FORMAT) != (srgb ? OpenGl.GL_SRGB8_ALPHA8 : OpenGl.GL_RGBA8))
            throw new InvalidOperationException("GPU sampler or color format differs from material.");
        for (int level = 0; level < gpu.Levels; level++)
            if (Level(level, OpenGl.GL_TEXTURE_WIDTH) != Math.Max(1, gpu.Width >> level) ||
                Level(level, OpenGl.GL_TEXTURE_HEIGHT) != Math.Max(1, gpu.Height >> level))
                throw new InvalidOperationException("GPU mip chain is incomplete.");
        if (OpenGl.GetError() != OpenGl.GL_NO_ERROR) throw new InvalidOperationException("GPU texture inspection failed.");
    }

    private static void VerifyTransferState()
    {
        GlEnum[] names = [OpenGl.GL_UNPACK_ALIGNMENT, OpenGl.GL_UNPACK_ROW_LENGTH, OpenGl.GL_UNPACK_SKIP_ROWS, OpenGl.GL_UNPACK_SKIP_PIXELS,
            OpenGl.GL_PACK_ALIGNMENT, OpenGl.GL_PACK_ROW_LENGTH, OpenGl.GL_PACK_SKIP_ROWS, OpenGl.GL_PACK_SKIP_PIXELS];
        var previous = new int[names.Length];
        for (int i = 0; i < names.Length; i++) OpenGl.GetIntegerv(names[i], previous.AsSpan(i, 1));
        try
        {
            for (int i = 0; i < names.Length; i++) OpenGl.PixelStorei(names[i], i % 4 == 0 ? 8 : 3);
            var pixels = new PngImage(2, 3, Enumerable.Range(0, 24).Select(i => (byte)(i * 11)).ToArray());
            using var gpu = new GpuTexture(pixels, new TextureSampler(), srgb: true);
            if (!gpu.ReadPixels().AsSpan().SequenceEqual(pixels.Rgba)) throw new InvalidOperationException("Pixel-store state corrupted texture transfer.");
            Span<int> actual = stackalloc int[1];
            for (int i = 0; i < names.Length; i++)
            {
                OpenGl.GetIntegerv(names[i], actual);
                if (actual[0] != (i % 4 == 0 ? 8 : 3)) throw new InvalidOperationException("Texture transfer leaked pixel-store state.");
            }
            VerifySampler(gpu, new TextureSampler(), srgb: true);
        }
        finally { for (int i = 0; i < names.Length; i++) OpenGl.PixelStorei(names[i], previous[i]); }
    }
}

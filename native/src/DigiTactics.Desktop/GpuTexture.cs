using Brutal.OpenGlApi;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>Owns one OpenGL texture. Create, use and dispose on the context thread.</summary>
internal sealed class GpuTexture : IDisposable
{
    private GlTexture handle;
    public uint Id => handle.Id;
    public int Width { get; }
    public int Height { get; }
    public int Levels { get; }

    public unsafe GpuTexture(PngImage image, TextureSampler sampler, bool srgb)
    {
        Width = image.Width; Height = image.Height;
        if (image.Rgba.Length != checked(Width * Height * 4)) throw new InvalidDataException("Texture pixel size mismatch.");
        Levels = sampler.Mipmaps ? 1 + System.Numerics.BitOperations.Log2((uint)Math.Max(Width, Height)) : 1;
        Span<GlTexture> names = stackalloc GlTexture[1];
        OpenGl.CreateTextures(OpenGl.GL_TEXTURE_2D, names); handle = names[0];
        try
        {
            using var transfer = new PixelTransfer(unpack: true);
            OpenGl.TextureStorage2D(Id, Levels, srgb ? OpenGl.GL_SRGB8_ALPHA8 : OpenGl.GL_RGBA8, Width, Height);
            // PNG rows retain glTF's top-down convention (Three.js flipY=false).
            fixed (byte* pixels = image.Rgba)
                OpenGl.TextureSubImage2D(Id, 0, 0, 0, Width, Height, OpenGl.GL_RGBA, OpenGl.GL_UNSIGNED_BYTE, pixels);
            OpenGl.TextureParameterFloat(Id, OpenGl.GL_TEXTURE_MAG_FILTER, sampler.MagFilter);
            OpenGl.TextureParameterFloat(Id, OpenGl.GL_TEXTURE_MIN_FILTER, sampler.MinFilter);
            OpenGl.TextureParameterFloat(Id, OpenGl.GL_TEXTURE_WRAP_S, sampler.WrapS);
            OpenGl.TextureParameterFloat(Id, OpenGl.GL_TEXTURE_WRAP_T, sampler.WrapT);
            if (sampler.Mipmaps) OpenGl.GenerateTextureMipmap(Id);
            CheckError("upload");
        }
        catch { Dispose(); throw; }
    }

    public unsafe byte[] ReadPixels()
    {
        ObjectDisposedException.ThrowIf(Id == 0, this);
        using var transfer = new PixelTransfer(unpack: false);
        var bytes = new byte[checked(Width * Height * 4)];
        fixed (byte* pixels = bytes)
            OpenGl.GetTextureImage(Id, 0, OpenGl.GL_RGBA, OpenGl.GL_UNSIGNED_BYTE, bytes.Length, pixels);
        CheckError("readback");
        return bytes;
    }

    public void Dispose()
    {
        if (Id == 0) return;
        OpenGl.DeleteTextures(new[] { handle }); handle = default;
    }

    private static void CheckError(string operation)
    {
        var error = OpenGl.GetError();
        if (error != OpenGl.GL_NO_ERROR) throw new InvalidOperationException($"GPU texture {operation} failed: {error.Id:X}.");
    }

    // Pixel-store state belongs to the context, not the texture. ImGui and future
    // asynchronous uploads must not change our row layout or turn pointers into PBO offsets.
    private sealed class PixelTransfer : IDisposable
    {
        private readonly GlEnum[] parameters;
        private readonly int[] values;
        private readonly GlEnum bufferTarget;
        private readonly uint buffer;
        public unsafe PixelTransfer(bool unpack)
        {
            parameters = unpack
                ? [OpenGl.GL_UNPACK_ALIGNMENT, OpenGl.GL_UNPACK_ROW_LENGTH, OpenGl.GL_UNPACK_SKIP_ROWS, OpenGl.GL_UNPACK_SKIP_PIXELS]
                : [OpenGl.GL_PACK_ALIGNMENT, OpenGl.GL_PACK_ROW_LENGTH, OpenGl.GL_PACK_SKIP_ROWS, OpenGl.GL_PACK_SKIP_PIXELS];
            values = new int[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                OpenGl.GetIntegerv(parameters[i], values.AsSpan(i, 1));
                OpenGl.PixelStorei(parameters[i], i == 0 ? 1 : 0);
            }
            bufferTarget = unpack ? OpenGl.GL_PIXEL_UNPACK_BUFFER : OpenGl.GL_PIXEL_PACK_BUFFER;
            Span<int> binding = stackalloc int[1];
            OpenGl.GetIntegerv(unpack ? OpenGl.GL_PIXEL_UNPACK_BUFFER_BINDING : OpenGl.GL_PIXEL_PACK_BUFFER_BINDING, binding);
            buffer = unchecked((uint)binding[0]);
            OpenGl.Native.glBindBuffer(bufferTarget, default);
        }
        public unsafe void Dispose()
        {
            for (int i = 0; i < parameters.Length; i++) OpenGl.PixelStorei(parameters[i], values[i]);
            OpenGl.Native.glBindBuffer(bufferTarget, new GlBuffer(buffer));
        }
    }
}

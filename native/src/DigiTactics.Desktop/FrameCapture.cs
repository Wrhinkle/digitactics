using System.Buffers.Binary;
using System.IO.Compression;
using Brutal.OpenGlApi;

namespace DigiTactics.Desktop;

internal static class FrameCapture
{
    public static unsafe void SaveBitmap(string path, int width, int height)
    {
        int stride = (width * 3 + 3) & ~3;
        var pixels = new byte[checked(stride * height)];
        fixed (byte* ptr = pixels)
            OpenGl.ReadPixels(0, 0, width, height, OpenGl.GL_BGR, OpenGl.GL_UNSIGNED_BYTE, ptr);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var writer = new BinaryWriter(File.Create(path));
        writer.Write((ushort)0x4d42);
        writer.Write(54 + pixels.Length);
        writer.Write(0);
        writer.Write(54);
        writer.Write(40);
        writer.Write(width);
        writer.Write(height);
        writer.Write((ushort)1);
        writer.Write((ushort)24);
        writer.Write(0);
        writer.Write(pixels.Length);
        writer.Write(2835); writer.Write(2835);
        writer.Write(0); writer.Write(0);
        writer.Write(pixels);
    }

    /// <summary>Reads the current framebuffer as an opaque RGBA PNG (top row first).</summary>
    public static unsafe void SavePng(string path, int width, int height)
    {
        byte[] rgba = new byte[checked(width * height * 4)];
        OpenGl.PixelStorei(OpenGl.GL_PACK_ALIGNMENT, 1);
        fixed (byte* data = rgba) OpenGl.ReadPixels(0, 0, width, height, OpenGl.GL_RGBA, OpenGl.GL_UNSIGNED_BYTE, data);
        for (int i = 3; i < rgba.Length; i += 4) rgba[i] = 255;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var output = File.Create(path); output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        byte[] header = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(header, width); BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height); header[8] = 8; header[9] = 6;
        PngChunk(output, "IHDR"u8, header);
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, true))
        {
            byte[] row = new byte[width * 4 + 1]; row[0] = 1;
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width * 4; x++) row[x + 1] = unchecked((byte)(rgba[y * width * 4 + x] - (x >= 4 ? rgba[y * width * 4 + x - 4] : 0)));
                zlib.Write(row);
            }
        }
        PngChunk(output, "IDAT"u8, compressed.ToArray()); PngChunk(output, "IEND"u8, []);
    }
    private static void PngChunk(Stream stream, ReadOnlySpan<byte> name, ReadOnlySpan<byte> payload)
    {
        Span<byte> value = stackalloc byte[4]; BinaryPrimitives.WriteInt32BigEndian(value, payload.Length); stream.Write(value); stream.Write(name); stream.Write(payload);
        uint crc = uint.MaxValue;
        foreach (byte b in name) { crc ^= b; for (int bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xedb88320u : 0); }
        foreach (byte b in payload) { crc ^= b; for (int bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xedb88320u : 0); }
        BinaryPrimitives.WriteUInt32BigEndian(value, ~crc); stream.Write(value);
    }
}

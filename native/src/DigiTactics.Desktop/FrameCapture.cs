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
}

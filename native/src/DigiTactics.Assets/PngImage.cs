using System.Buffers.Binary;
using System.IO.Compression;

namespace DigiTactics.Assets;

/// <summary>Top-down RGBA pixels from the build pipeline's non-interlaced RGBA8 PNGs.</summary>
public sealed record PngImage(int Width, int Height, byte[] Rgba)
{
    public static PngImage Decode(ReadOnlySpan<byte> data)
    {
        if (data.Length < 8 || !data[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            throw new InvalidDataException("Invalid PNG signature.");
        int width = 0, height = 0; bool ended = false, hasData = false;
        using var compressed = new MemoryStream();
        for (int offset = 8; offset < data.Length;)
        {
            if (data.Length - offset < 12) throw new InvalidDataException("Truncated PNG chunk.");
            int size = checked((int)BinaryPrimitives.ReadUInt32BigEndian(data[offset..]));
            if (size > data.Length - offset - 12) throw new InvalidDataException("PNG chunk outside image.");
            var type = data.Slice(offset + 4, 4);
            var payload = data.Slice(offset + 8, size);
            uint crc = uint.MaxValue;
            foreach (byte value in data.Slice(offset + 4, size + 4))
            {
                crc ^= value;
                for (int bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xedb88320u : 0);
            }
            if (~crc != BinaryPrimitives.ReadUInt32BigEndian(data[(offset + 8 + size)..]))
                throw new InvalidDataException("PNG checksum mismatch.");
            if (type.SequenceEqual("IHDR"u8))
            {
                if (offset != 8 || size != 13) throw new InvalidDataException("Invalid PNG header.");
                width = checked((int)BinaryPrimitives.ReadUInt32BigEndian(payload));
                height = checked((int)BinaryPrimitives.ReadUInt32BigEndian(payload[4..]));
                if (width <= 0 || height <= 0 || width > 16384 || height > 16384 ||
                    payload[8] != 8 || payload[9] != 6 || payload[10] != 0 || payload[11] != 0 || payload[12] != 0)
                    throw new InvalidDataException("Expected non-interlaced RGBA8 PNG.");
            }
            else if (width == 0) throw new InvalidDataException("Missing PNG header.");
            else if (type.SequenceEqual("IDAT"u8)) { compressed.Write(payload); hasData = true; }
            else if (type.SequenceEqual("IEND"u8))
            {
                if (size != 0 || offset + 12 != data.Length || !hasData) throw new InvalidDataException("Invalid PNG end.");
                ended = true; break;
            }
            else if ((type[0] & 32) == 0 && !type.SequenceEqual("PLTE"u8))
                throw new InvalidDataException("Unsupported critical PNG chunk.");
            offset += size + 12;
        }
        if (!ended) throw new InvalidDataException("Missing PNG end.");
        int stride = checked(width * 4);
        var rgba = new byte[checked(stride * height)];
        var row = new byte[stride];
        compressed.Position = 0;
        using var inflate = new ZLibStream(compressed, CompressionMode.Decompress);
        for (int y = 0; y < height; y++)
        {
            int filter = inflate.ReadByte();
            if (filter < 0 || filter > 4) throw new InvalidDataException("Invalid PNG scanline filter.");
            inflate.ReadExactly(row);
            for (int x = 0; x < stride; x++)
            {
                int at = y * stride + x;
                int left = x >= 4 ? rgba[at - 4] : 0;
                int up = y > 0 ? rgba[at - stride] : 0;
                int corner = y > 0 && x >= 4 ? rgba[at - stride - 4] : 0;
                int prediction = filter switch { 0 => 0, 1 => left, 2 => up, 3 => (left + up) / 2, 4 => Paeth(left, up, corner), _ => 0 };
                rgba[at] = unchecked((byte)(row[x] + prediction));
            }
        }
        if (inflate.ReadByte() != -1) throw new InvalidDataException("Extra PNG scanline data.");
        return new(width, height, rgba);
    }

    private static int Paeth(int left, int up, int corner)
    {
        int p = left + up - corner;
        int a = Math.Abs(p - left), b = Math.Abs(p - up), c = Math.Abs(p - corner);
        return a <= b && a <= c ? left : b <= c ? up : corner;
    }
}

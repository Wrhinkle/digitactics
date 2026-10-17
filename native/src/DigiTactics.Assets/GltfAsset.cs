using System.Buffers.Binary;
using System.IO.Compression;
using System.Text.Json;

namespace DigiTactics.Assets;

/// <summary>Embedded glTF 2 payload produced by the native asset exporter. Meshopt is decoded
/// at build time; node hierarchy, skins, quantized accessors, images and animations are retained.</summary>
public sealed class GltfAsset : IDisposable
{
    private readonly JsonDocument document;
    private readonly byte[] binary;
    private readonly JsonElement[] accessors;
    private readonly JsonElement[] views;
    public JsonElement Root => document.RootElement;
    private GltfAsset(JsonDocument document, byte[] binary)
    {
        this.document = document; this.binary = binary;
        accessors = Root.TryGetProperty("accessors", out var a) ? a.EnumerateArray().ToArray() : [];
        views = Root.TryGetProperty("bufferViews", out var v) ? v.EnumerateArray().ToArray() : [];
    }

    public static GltfAsset LoadCompressed(byte[] bytes)
    {
        using var source = new MemoryStream(bytes, writable: false);
        using var gzip = new GZipStream(source, CompressionMode.Decompress);
        using var output = new MemoryStream(); gzip.CopyTo(output);
        var data = output.ToArray(); var span = data.AsSpan();
        if (span.Length < 28 || BinaryPrimitives.ReadUInt32LittleEndian(span) != 0x46546c67 ||
            BinaryPrimitives.ReadUInt32LittleEndian(span[4..]) != 2 || BinaryPrimitives.ReadUInt32LittleEndian(span[8..]) != span.Length)
            throw new InvalidDataException("Invalid native GLB header.");
        JsonDocument? json = null; byte[]? bin = null;
        try
        {
            for (int offset = 12; offset < data.Length;)
            {
                if (data.Length - offset < 8) throw new InvalidDataException("Truncated GLB chunk.");
                int size = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(span[offset..]));
                uint type = BinaryPrimitives.ReadUInt32LittleEndian(span[(offset + 4)..]);
                if (size % 4 != 0 || size > data.Length - offset - 8) throw new InvalidDataException("Invalid GLB chunk length.");
                if (type == 0x4e4f534a) { if (json is not null) throw new InvalidDataException("Duplicate JSON chunk."); json = JsonDocument.Parse(data.AsMemory(offset + 8, size)); }
                if (type == 0x004e4942) { if (bin is not null) throw new InvalidDataException("Duplicate binary chunk."); bin = span.Slice(offset + 8, size).ToArray(); }
                offset += size + 8;
            }
            if (json is null || bin is null) throw new InvalidDataException("Missing GLB data.");
            return new(json, bin);
        }
        catch { json?.Dispose(); throw; }
    }

    public ReadOnlyMemory<byte> BufferView(int index)
    {
        var view = views[index];
        if (view.GetProperty("buffer").GetInt32() != 0 || view.TryGetProperty("extensions", out var extensions) && extensions.TryGetProperty("EXT_meshopt_compression", out _))
            throw new InvalidDataException("Model has not been converted for native loading.");
        int start = Integer(view, "byteOffset"), length = view.GetProperty("byteLength").GetInt32();
        if (start < 0 || length < 0 || start > binary.Length - length) throw new InvalidDataException("Buffer view outside model.");
        return binary.AsMemory(start, length);
    }

    public PngImage Image(int index)
    {
        var image = Root.GetProperty("images")[index];
        if (image.GetProperty("mimeType").GetString() != "image/png")
            throw new InvalidDataException("Image has not been converted for native loading.");
        return PngImage.Decode(BufferView(image.GetProperty("bufferView").GetInt32()).Span);
    }

    public double[] AccessorElement(int index, int element)
    {
        var accessor = accessors[index];
        if (accessor.TryGetProperty("sparse", out _)) throw new NotSupportedException("Sparse accessor conversion is required.");
        if (element < 0 || element >= accessor.GetProperty("count").GetInt32()) throw new ArgumentOutOfRangeException(nameof(element));
        string type = accessor.GetProperty("type").GetString()!;
        int components = type switch { "SCALAR" => 1, "VEC2" => 2, "VEC3" => 3, "VEC4" => 4, "MAT2" => 4, "MAT3" => 9, "MAT4" => 16, _ => throw new InvalidDataException("Invalid accessor type.") };
        int component = accessor.GetProperty("componentType").GetInt32();
        int size = component switch { 5120 or 5121 => 1, 5122 or 5123 => 2, 5125 or 5126 => 4, _ => throw new InvalidDataException("Invalid accessor component.") };
        bool normalized = accessor.TryGetProperty("normalized", out var n) && n.GetBoolean();
        var result = new double[components];
        if (!accessor.TryGetProperty("bufferView", out var viewIndex)) return result;
        int viewId = viewIndex.GetInt32(); var view = views[viewId];
        int rows = type.StartsWith("MAT", StringComparison.Ordinal) ? type[3] - '0' : components;
        int columnBytes = type.StartsWith("MAT", StringComparison.Ordinal) ? (rows * size + 3) & ~3 : rows * size;
        int packed = type.StartsWith("MAT", StringComparison.Ordinal) ? columnBytes * rows : columnBytes;
        int stride = view.TryGetProperty("byteStride", out var step) ? step.GetInt32() : packed;
        int offset = checked(Integer(accessor, "byteOffset") + element * stride);
        var bytes = BufferView(viewId).Span;
        for (int i = 0; i < components; i++)
        {
            int at = checked(offset + i / rows * columnBytes + i % rows * size);
            if (at < 0 || at > bytes.Length - size) throw new InvalidDataException("Accessor outside buffer view.");
            double value = component switch {
                5120 => (sbyte)bytes[at], 5121 => bytes[at], 5122 => BinaryPrimitives.ReadInt16LittleEndian(bytes[at..]),
                5123 => BinaryPrimitives.ReadUInt16LittleEndian(bytes[at..]), 5125 => BinaryPrimitives.ReadUInt32LittleEndian(bytes[at..]),
                5126 => BinaryPrimitives.ReadSingleLittleEndian(bytes[at..]), _ => throw new InvalidDataException()
            };
            result[i] = normalized ? component switch { 5120 => Math.Max(-1, value / 127), 5121 => value / 255,
                5122 => Math.Max(-1, value / 32767), 5123 => value / 65535, _ => value } : value;
        }
        return result;
    }

    private static int Integer(JsonElement value, string name) => value.TryGetProperty(name, out var n) ? n.GetInt32() : 0;
    public void Dispose() => document.Dispose();
}

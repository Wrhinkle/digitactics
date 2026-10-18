using Brutal.OpenGlApi;
using DigiTactics.Assets;
namespace DigiTactics.Desktop;

internal sealed class GpuPrimitive : IDisposable
{
    private GlVertexArray array;
    private readonly GlBuffer[] buffers = new GlBuffer[4];
    public bool HasColors { get; }
    public int ColorComponents { get; }
    public System.Numerics.Vector3 SortCenter { get; }
    private readonly int count;
    private readonly int colorCount;
    private readonly uint topology;
    public unsafe GpuPrimitive(GltfPrimitive data, uint topology = OpenGl.GL_TRIANGLES)
    {
        this.topology = topology;
        count = data.Indices.Length;
        ColorComponents = data.ColorComponents;
        colorCount = data.Colors.Length;
        HasColors = ColorComponents > 0;
        var min = new System.Numerics.Vector3(float.PositiveInfinity); var max = new System.Numerics.Vector3(float.NegativeInfinity);
        for (int i = 0; i < data.Vertices.Length; i += GltfPrimitive.Stride)
        {
            var point = new System.Numerics.Vector3(data.Vertices[i], data.Vertices[i + 1], data.Vertices[i + 2]);
            min = System.Numerics.Vector3.Min(min, point); max = System.Numerics.Vector3.Max(max, point);
        }
        SortCenter = data.VertexCount == 0 ? default : (min + max) / 2;
        Span<GlVertexArray> arrays = stackalloc GlVertexArray[1]; OpenGl.CreateVertexArrays(arrays); array = arrays[0];
        try
        {
            OpenGl.CreateBuffers(buffers);
            fixed (float* vertices = data.Vertices) OpenGl.NamedBufferData(buffers[0], data.Vertices.Length * sizeof(float), vertices, OpenGl.GL_STATIC_DRAW);
            fixed (uint* indices = data.Indices) OpenGl.NamedBufferData(buffers[1], data.Indices.Length * sizeof(uint), indices, OpenGl.GL_STATIC_DRAW);
            OpenGl.VertexArrayVertexBuffer(array.Id, 0, buffers[0], 0, GltfPrimitive.Stride * sizeof(float));
            OpenGl.VertexArrayElementBuffer(array.Id, buffers[1]);
            if (HasColors)
            {
                fixed (float* colors = data.Colors) OpenGl.NamedBufferData(buffers[3], data.Colors.Length * sizeof(float), colors, OpenGl.GL_STATIC_DRAW);
                OpenGl.VertexArrayVertexBuffer(array.Id, 2, buffers[3], 0, ColorComponents * sizeof(float));
                OpenGl.EnableVertexArrayAttrib(array.Id, 6); OpenGl.VertexArrayAttribBinding(array.Id, 6, 2);
                OpenGl.VertexArrayAttribFormat(array.Id, 6, ColorComponents, OpenGl.GL_FLOAT, false, 0);
            }
            if (data.Tangents.Length > 0)
            {
                fixed (float* tangents = data.Tangents) OpenGl.NamedBufferData(buffers[2], data.Tangents.Length * sizeof(float), tangents, OpenGl.GL_STATIC_DRAW);
                OpenGl.VertexArrayVertexBuffer(array.Id, 1, buffers[2], 0, 4 * sizeof(float));
                OpenGl.EnableVertexArrayAttrib(array.Id, 5); OpenGl.VertexArrayAttribBinding(array.Id, 5, 1);
                OpenGl.VertexArrayAttribFormat(array.Id, 5, 4, OpenGl.GL_FLOAT, false, 0);
            }
            int[] sizes = [3, 3, 2, 4, 4]; uint offset = 0;
            for (uint i = 0; i < sizes.Length; i++)
            {
                OpenGl.EnableVertexArrayAttrib(array.Id, i); OpenGl.VertexArrayAttribBinding(array.Id, i, 0);
                OpenGl.VertexArrayAttribFormat(array.Id, i, sizes[i], OpenGl.GL_FLOAT, false, offset);
                offset += (uint)sizes[i] * sizeof(float);
            }
        }
        catch { Dispose(); throw; }
    }
    public unsafe void Draw(bool wireframe = false)
    {
        array.Bind();
        if (wireframe) OpenGl.Native.glPolygonMode((GlFace)OpenGl.GL_FRONT_AND_BACK, OpenGl.GL_LINE);
        OpenGl.Native.glDrawElements(topology, count, OpenGl.GL_UNSIGNED_INT, null);
        if (wireframe) OpenGl.Native.glPolygonMode((GlFace)OpenGl.GL_FRONT_AND_BACK, OpenGl.GL_FILL);
    }
    public unsafe void UpdateVertices(float[] vertices)
    {
        fixed (float* data = vertices) OpenGl.NamedBufferSubData(buffers[0], 0, vertices.Length * sizeof(float), data);
    }
    public unsafe void UpdateColors(ReadOnlySpan<float> colors)
    {
        if (!HasColors || colors.Length != colorCount) throw new InvalidDataException("Updated vertex color count differs.");
        foreach (float value in colors) if (!float.IsFinite(value)) throw new InvalidDataException("Non-finite updated vertex color.");
        fixed (float* data = colors) OpenGl.NamedBufferSubData(buffers[3], 0, colors.Length * sizeof(float), data);
    }
    public void Dispose() { OpenGl.DeleteBuffers(buffers); OpenGl.DeleteVertexArrays(new[] { array }); }
}


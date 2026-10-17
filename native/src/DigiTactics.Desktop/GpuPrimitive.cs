using Brutal.OpenGlApi;
using DigiTactics.Assets;
namespace DigiTactics.Desktop;

internal sealed class GpuPrimitive : IDisposable
{
    private GlVertexArray array;
    private readonly GlBuffer[] buffers = new GlBuffer[2];
    private readonly int count;
    public unsafe GpuPrimitive(GltfPrimitive data)
    {
        count = data.Indices.Length;
        Span<GlVertexArray> arrays = stackalloc GlVertexArray[1]; OpenGl.CreateVertexArrays(arrays); array = arrays[0];
        try
        {
            OpenGl.CreateBuffers(buffers);
            fixed (float* vertices = data.Vertices) OpenGl.NamedBufferData(buffers[0], data.Vertices.Length * sizeof(float), vertices, OpenGl.GL_STATIC_DRAW);
            fixed (uint* indices = data.Indices) OpenGl.NamedBufferData(buffers[1], data.Indices.Length * sizeof(uint), indices, OpenGl.GL_STATIC_DRAW);
            OpenGl.VertexArrayVertexBuffer(array.Id, 0, buffers[0], 0, GltfPrimitive.Stride * sizeof(float));
            OpenGl.VertexArrayElementBuffer(array.Id, buffers[1]);
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
    public unsafe void Draw() { array.Bind(); OpenGl.Native.glDrawElements(OpenGl.GL_TRIANGLES, count, OpenGl.GL_UNSIGNED_INT, null); }
    public void Dispose() { OpenGl.DeleteBuffers(buffers); OpenGl.DeleteVertexArrays(new[] { array }); }
}


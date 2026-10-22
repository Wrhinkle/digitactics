using Brutal.ImGuiApi;
using Brutal.ImGuiApi.Backends;
using Brutal.OpenGlApi;

namespace DigiTactics.Desktop;

/// <summary>
/// Draws the frame's ImGui geometry with ClearType-style text, the way Chromium renders the original client on
/// Windows: font-atlas coverage is sampled at the three RGB subpixel centres and each colour channel blends with its own
/// coverage (dual-source blending), which gives glyph stems their cyan left and orange right edges. Every non-glyph
/// pixel blends exactly as the ImGui OpenGL backend's own shader does. The backend still owns the textures: it is
/// called first with the frame's texture requests and no geometry.
/// </summary>
internal sealed unsafe class SubpixelText : IDisposable
{
    /// <summary>Glyphs are rasterized at three texels per pixel horizontally, one per subpixel.</summary>
    internal const sbyte Oversample = 3;
    private const uint GlOneMinusSrc1Color = 0x88FA;
    private GlProgram program;
    private GlVertexArray array;
    private readonly GlBuffer[] buffers = new GlBuffer[2];
    private const int MaxGrayscale = 16;
    private int projection, linesRect, whitePixel, grayscaleRects, grayscaleCount;
    private bool ready, failed;
    private static readonly List<(float X0, float Y0, float X1, float Y1)> grayscale = [];

    /// <summary>
    /// Chromium draws text in composited layers (animated or transformed boxes, such as the result screen, the Home
    /// hero or the docked Codex window) with grayscale anti-aliasing. Screens mark those areas, in ImGui display
    /// coordinates, for the frame being built.
    /// </summary>
    internal static void Grayscale(Brutal.Numerics.float2 min, Brutal.Numerics.float2 max)
    {
        if (grayscale.Count < MaxGrayscale && max.X > min.X && max.Y > min.Y) grayscale.Add((min.X, min.Y, max.X, max.Y));
    }

    public void Render(OpenGlBackend backend, ImDrawDataPtr drawData)
    {
        if (failed) { grayscale.Clear(); backend.RenderDrawData(drawData); return; }
        var source = drawData;
        // Texture creation, updates and destruction for this frame.
        var textures = new ImDrawData { Valid = true, DisplayPos = source.DisplayPos, DisplaySize = source.DisplaySize, FramebufferScale = source.FramebufferScale,
            OwnerViewport = source.OwnerViewport, Textures = source.Textures };
        backend.RenderDrawData(new(&textures));
        try
        {
            if (!ready) Create();
            Draw(source);
        }
        catch (Exception error)
        {
            failed = true;
            Console.Error.WriteLine($"Subpixel text is unavailable, using the ImGui backend: {error.Message}");
            backend.RenderDrawData(drawData);
        }
        finally { grayscale.Clear(); }
    }

    private void Draw(ImDrawDataPtr data)
    {
        int width = (int)(data.DisplaySize.X * data.FramebufferScale.X), height = (int)(data.DisplaySize.Y * data.FramebufferScale.Y);
        if (width <= 0 || height <= 0 || data.CmdListsCount == 0) return;
        Span<int> saved = stackalloc int[16];
        OpenGl.GetIntegerv(OpenGl.GL_CURRENT_PROGRAM, saved[..1]); int lastProgram = saved[0];
        OpenGl.GetIntegerv(OpenGl.GL_VERTEX_ARRAY_BINDING, saved[..1]); int lastArray = saved[0];
        OpenGl.GetIntegerv(OpenGl.GL_ACTIVE_TEXTURE, saved[..1]); int lastActive = saved[0];
        OpenGl.Native.glActiveTexture(OpenGl.GL_TEXTURE0);
        OpenGl.GetIntegerv(OpenGl.GL_TEXTURE_BINDING_2D, saved[..1]); int lastTexture = saved[0];
        OpenGl.GetIntegerv(OpenGl.GL_BLEND_SRC_RGB, saved[..1]); int srcRgb = saved[0];
        OpenGl.GetIntegerv(OpenGl.GL_BLEND_DST_RGB, saved[..1]); int dstRgb = saved[0];
        OpenGl.GetIntegerv(OpenGl.GL_BLEND_SRC_ALPHA, saved[..1]); int srcAlpha = saved[0];
        OpenGl.GetIntegerv(OpenGl.GL_BLEND_DST_ALPHA, saved[..1]); int dstAlpha = saved[0];
        OpenGl.GetIntegerv(OpenGl.GL_SCISSOR_BOX, saved[..4]); var lastScissor = (saved[0], saved[1], saved[2], saved[3]);
        bool blend = Enabled(GlCapability.Blend), cull = Enabled(GlCapability.CullFace), depth = Enabled(GlCapability.DepthTest),
            stencil = Enabled(GlCapability.StencilTest), scissor = Enabled(GlCapability.ScissorTest);
        try
        {
            OpenGl.Native.glEnable(GlCapability.Blend);
            OpenGl.Native.glBlendFuncSeparate(OpenGl.GL_ONE, GlOneMinusSrc1Color, OpenGl.GL_ONE, OpenGl.GL_ONE_MINUS_SRC_ALPHA);
            OpenGl.Native.glDisable(GlCapability.CullFace); OpenGl.Native.glDisable(GlCapability.DepthTest);
            OpenGl.Native.glDisable(GlCapability.StencilTest); OpenGl.Native.glEnable(GlCapability.ScissorTest);
            OpenGl.Viewport(0, 0, width, height);
            float l = data.DisplayPos.X, r = l + data.DisplaySize.X, t = data.DisplayPos.Y, b = t + data.DisplaySize.Y;
            Span<float> ortho = [2 / (r - l), 0, 0, 0, 0, 2 / (t - b), 0, 0, 0, 0, -1, 0, (r + l) / (l - r), (t + b) / (b - t), 0, 1];
            fixed (float* matrix = ortho) OpenGl.Native.glProgramUniformMatrix4fv(program, projection, 1, false, matrix);
            var atlas = ImGui.GetIO().Fonts;
            // TexUvLines holds IM_DRAWLIST_TEX_LINES_WIDTH_MAX + 1 = 33 rows.
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            for (int i = 0; i < 33; i++)
            {
                var uv = atlas.TexUvLines[i];
                x0 = Math.Min(x0, Math.Min(uv.X, uv.Z)); y0 = Math.Min(y0, Math.Min(uv.Y, uv.W));
                x1 = Math.Max(x1, Math.Max(uv.X, uv.Z)); y1 = Math.Max(y1, Math.Max(uv.Y, uv.W));
            }
            OpenGl.Native.glProgramUniform4f(program, linesRect, x0, y0, x1, y1);
            OpenGl.Native.glProgramUniform2f(program, whitePixel, atlas.TexUvWhitePixel.X, atlas.TexUvWhitePixel.Y);
            // Grayscale areas in framebuffer pixels with a bottom-left origin, as gl_FragCoord reports them.
            Span<float> rects = stackalloc float[MaxGrayscale * 4];
            for (int i = 0; i < grayscale.Count; i++)
            {
                var g = grayscale[i];
                rects[i * 4] = (g.X0 - data.DisplayPos.X) * data.FramebufferScale.X; rects[i * 4 + 2] = (g.X1 - data.DisplayPos.X) * data.FramebufferScale.X;
                rects[i * 4 + 1] = height - (g.Y1 - data.DisplayPos.Y) * data.FramebufferScale.Y; rects[i * 4 + 3] = height - (g.Y0 - data.DisplayPos.Y) * data.FramebufferScale.Y;
            }
            fixed (float* values = rects) OpenGl.Native.glProgramUniform4fv(program, grayscaleRects, MaxGrayscale, values);
            OpenGl.Native.glProgramUniform1i(program, grayscaleCount, grayscale.Count);
            program.Use(); array.Bind();
            var clipOffset = data.DisplayPos; var clipScale = data.FramebufferScale;
            for (int n = 0; n < data.CmdListsCount; n++)
            {
                var list = data.CmdLists[n];
                OpenGl.NamedBufferData(buffers[0], list.VtxBuffer.Count * sizeof(ImDrawVert), list.VtxBuffer.DataRaw, OpenGl.GL_STREAM_DRAW);
                OpenGl.NamedBufferData(buffers[1], list.IdxBuffer.Count * sizeof(ushort), list.IdxBuffer.DataRaw, OpenGl.GL_STREAM_DRAW);
                for (int c = 0; c < list.CmdBuffer.Count; c++)
                {
                    var command = list.CmdBuffer[c];
                    if (command.UserCallback != default) throw new NotSupportedException("ImGui draw callbacks are not supported by the subpixel text renderer.");
                    float minX = (command.ClipRect.X - clipOffset.X) * clipScale.X, minY = (command.ClipRect.Y - clipOffset.Y) * clipScale.Y;
                    float maxX = (command.ClipRect.Z - clipOffset.X) * clipScale.X, maxY = (command.ClipRect.W - clipOffset.Y) * clipScale.Y;
                    if (maxX <= minX || maxY <= minY || command.ElemCount == 0) continue;
                    OpenGl.Native.glScissor((int)minX, (int)(height - maxY), (int)(maxX - minX), (int)(maxY - minY));
                    OpenGl.Native.glBindTexture(OpenGl.GL_TEXTURE_2D, (uint)(nint)command.GetTexID());
                    OpenGl.Native.glDrawElementsBaseVertex(OpenGl.GL_TRIANGLES, (int)command.ElemCount, OpenGl.GL_UNSIGNED_SHORT,
                        (void*)(command.IdxOffset * sizeof(ushort)), (int)command.VtxOffset);
                }
            }
        }
        finally
        {
            OpenGl.Native.glUseProgram(new((uint)lastProgram));
            OpenGl.Native.glBindVertexArray(new((uint)lastArray));
            OpenGl.Native.glBindTexture(OpenGl.GL_TEXTURE_2D, (uint)lastTexture);
            OpenGl.Native.glActiveTexture((uint)lastActive);
            OpenGl.Native.glBlendFuncSeparate((uint)srcRgb, (uint)dstRgb, (uint)srcAlpha, (uint)dstAlpha);
            Toggle(GlCapability.Blend, blend); Toggle(GlCapability.CullFace, cull); Toggle(GlCapability.DepthTest, depth);
            Toggle(GlCapability.StencilTest, stencil); Toggle(GlCapability.ScissorTest, scissor);
            OpenGl.Native.glScissor(lastScissor.Item1, lastScissor.Item2, lastScissor.Item3, lastScissor.Item4);
        }
    }

    private static bool Enabled(GlCapability capability) => OpenGl.Native.glIsEnabled((GlEnum)(uint)capability);

    private static void Toggle(GlCapability capability, bool on)
    {
        if (on) OpenGl.Native.glEnable(capability); else OpenGl.Native.glDisable(capability);
    }

    private static int Location(GlProgram target, ReadOnlySpan<byte> name)
    {
        Span<byte> terminated = stackalloc byte[name.Length + 1];
        name.CopyTo(terminated);
        fixed (byte* text = terminated) return OpenGl.Native.glGetUniformLocation(target, text);
    }

    private void Create()
    {
        using var vertex = OpenGl.CreateShader(GlShaderType.Vertex).SetSource("""
            #version 410 core
            layout (location = 0) in vec2 Position;
            layout (location = 1) in vec2 UV;
            layout (location = 2) in vec4 Color;
            uniform mat4 Projection;
            out vec2 Frag_UV;
            out vec4 Frag_Color;
            void main() { Frag_UV = UV; Frag_Color = Color; gl_Position = Projection * vec4(Position.xy, 0, 1); }
            """u8).Compile();
        // Font-atlas texels are white with coverage in alpha. Baked anti-aliased lines and the white pixel share the
        // atlas but are shapes, so they keep one coverage, as do textures that are not pure white.
        using var fragment = OpenGl.CreateShader(GlShaderType.Fragment).SetSource("""
            #version 410 core
            in vec2 Frag_UV;
            in vec4 Frag_Color;
            uniform sampler2D Texture;
            uniform vec4 LinesRect;
            uniform vec2 WhitePixel;
            uniform vec4 GrayscaleRects[16];
            uniform int GrayscaleCount;
            const float ChromaGain = 1.5;
            layout (location = 0, index = 0) out vec4 Out_Color;
            layout (location = 0, index = 1) out vec4 Out_Cover;
            void main()
            {
                vec4 texel = texture(Texture, Frag_UV);
                float alpha = Frag_Color.a * texel.a;
                vec3 cover = vec3(alpha);
                bool plain = all(lessThan(abs(Frag_UV - WhitePixel), vec2(1e-6))) ||
                    (Frag_UV.x >= LinesRect.x && Frag_UV.y >= LinesRect.y && Frag_UV.x <= LinesRect.z && Frag_UV.y <= LinesRect.w);
                for (int i = 0; i < GrayscaleCount; i++)
                    if (all(greaterThanEqual(gl_FragCoord.xy, GrayscaleRects[i].xy)) && all(lessThan(gl_FragCoord.xy, GrayscaleRects[i].zw))) plain = true;
                if (!plain && all(greaterThan(texel.rgb, vec3(0.996))))
                {
                    vec2 third = dFdx(Frag_UV) / 3.0;
                    vec3 subpixels = vec3(texture(Texture, Frag_UV - third).a, texel.a, texture(Texture, Frag_UV + third).a);
                    cover = Frag_Color.a * clamp(texel.a + (subpixels - texel.a) * ChromaGain, 0.0, 1.0);
                }
                Out_Color = vec4(Frag_Color.rgb * texel.rgb * cover, alpha);
                Out_Cover = vec4(cover, alpha);
            }
            """u8).Compile();
        program = OpenGl.CreateProgram();
        program.Attach(vertex).Attach(fragment).Link();
        projection = Location(program, "Projection"u8);
        linesRect = Location(program, "LinesRect"u8);
        whitePixel = Location(program, "WhitePixel"u8);
        grayscaleRects = Location(program, "GrayscaleRects"u8);
        grayscaleCount = Location(program, "GrayscaleCount"u8);
        OpenGl.Native.glProgramUniform1i(program, Location(program, "Texture"u8), 0);
        Span<GlVertexArray> arrays = stackalloc GlVertexArray[1]; OpenGl.CreateVertexArrays(arrays); array = arrays[0];
        OpenGl.CreateBuffers(buffers);
        OpenGl.VertexArrayVertexBuffer(array.Id, 0, buffers[0], 0, sizeof(ImDrawVert));
        OpenGl.VertexArrayElementBuffer(array.Id, buffers[1]);
        OpenGl.EnableVertexArrayAttrib(array.Id, 0); OpenGl.VertexArrayAttribBinding(array.Id, 0, 0); OpenGl.VertexArrayAttribFormat(array.Id, 0, 2, OpenGl.GL_FLOAT, false, 0);
        OpenGl.EnableVertexArrayAttrib(array.Id, 1); OpenGl.VertexArrayAttribBinding(array.Id, 1, 0); OpenGl.VertexArrayAttribFormat(array.Id, 1, 2, OpenGl.GL_FLOAT, false, 8);
        OpenGl.EnableVertexArrayAttrib(array.Id, 2); OpenGl.VertexArrayAttribBinding(array.Id, 2, 0); OpenGl.VertexArrayAttribFormat(array.Id, 2, 4, OpenGl.GL_UNSIGNED_BYTE, true, 16);
        ready = true;
    }

    public void Dispose()
    {
        if (!ready) return;
        OpenGl.DeleteBuffers(buffers); OpenGl.DeleteVertexArrays(new[] { array }); program.Dispose();
    }
}

using System.Numerics;
using Brutal.GlfwApi;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>Real raster checks for RGB/RGBA interpolation, coverage, depth and global blend order.</summary>
internal static class ModelVertexColorSmoke
{
    public static unsafe void Run(string? capture)
    {
        if (!Glfw.Init()) throw new InvalidOperationException("GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 4); Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 5);
            Glfw.WindowHint(GlfwWindowHint.Visible, 0);
            using var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo { Title = "Vertex color verification", Size = new int2(256,256) });
            window.MakeContextCurrent();
            if (!Glad.Initialize(name => Glfw.PInvoke.GetProcAddress(name))) throw new InvalidOperationException("OpenGL initialization failed.");
            using var renderer = new ModelRenderer();
            int checks = 0;
            void Check(bool condition, string label) { checks++; if (!condition) throw new InvalidDataException("Vertex color raster: " + label); }
            using var plain = Quad([]);
            using var rgb = Quad([1,1,1, 1,1,1, 1,1,1, 1,1,1]);
            using var rgba = Quad([1,1,1,0, 1,1,1,1, 1,1,1,1, 1,1,1,0]);
            using var rgbaWhite = Quad([1,1,1,1, 1,1,1,1, 1,1,1,1, 1,1,1,1]);
            using var halfRed = Quad([1,0,0,.5f, 1,0,0,.5f, 1,0,0,.5f, 1,0,0,.5f]);
            using var halfBlue = Quad([0,0,1,.5f, 0,0,1,.5f, 0,0,1,.5f, 0,0,1,.5f]);
            Check(rgb.ColorComponents == 3 && rgba.ColorComponents == 4 && !plain.HasColors, "GPU attribute widths");
            byte[] Render(GpuPrimitive primitive, bool vertexColors, bool transparent = false, float cutoff = 0)
            {
                Begin();
                renderer.DrawRoom(primitive, Matrix4D.Identity, Matrix4x4.Identity, new(1,1,1,1), null, true, transparent, !transparent,
                    vertexColors: vertexColors, alphaTest: cutoff);
                var pixels = Pixels(); ModelRenderer.End(); return pixels;
            }
            var noColor = Render(plain, false);
            Check(noColor.SequenceEqual(Render(rgb, true)), "RGB white is byte-identical to no color");
            Check(noColor.SequenceEqual(Render(rgbaWhite, true)), "RGBA white opaque is byte-identical to no color");
            Check(noColor.SequenceEqual(Render(rgba, true)), "OPAQUE ignores vertex opacity");
            var blended = Render(rgba, true, transparent: true);
            for (int x = 32; x <= 224; x += 32)
            {
                int expected = (int)Math.Round((x + .5) / 256 * 255);
                Check(Math.Abs(blended[(128 * 256 + x) * 4] - expected) <= 1, "linear alpha interpolation at " + x);
            }
            float DepthAt(int x)
            {
                float depth; OpenGl.ReadPixels(x,128,1,1,OpenGl.GL_DEPTH_COMPONENT,OpenGl.GL_FLOAT,&depth); return depth;
            }
            Check(DepthAt(128) == 1, "BLEND preserves background depth");
            var masked = Render(rgba, true, cutoff: .5f);
            Check(masked[(128 * 256 + 64) * 4] == 0 && masked[(128 * 256 + 192) * 4] == 255, "MASK discards only low opacity");
            float discardedDepth = DepthAt(64), retainedDepth = DepthAt(192);
            Check(discardedDepth == 1 && Math.Abs(retainedDepth - .5f) < 1e-6, $"MASK writes depth only for retained fragments ({discardedDepth}, {retainedDepth})");
            byte[] Ordered(bool reversedSubmission, bool reverseCamera)
            {
                Begin(); renderer.BeginTransparency();
                var camera = reverseCamera ? Matrix4x4.CreateScale(1,1,-1) * Matrix4x4.CreateTranslation(0,0,1) : Matrix4x4.Identity;
                void Draw(bool near) => renderer.DrawRoom(near ? halfBlue : halfRed, Matrix4D.Identity with { M43 = near ? -.2 : .2 },
                    camera, new(1,1,1,1), null, true, true, false, vertexColors: true);
                Draw(reversedSubmission); Draw(!reversedSubmission);
                Check(renderer.PendingTransparentDraws == 2, "RGBA enters global transparent queue");
                renderer.FlushTransparency(); var pixels = Pixels(); ModelRenderer.End(); return pixels;
            }
            var ordered = Ordered(false, false);
            Check(ordered.SequenceEqual(Ordered(true, false)), "back-to-front order is independent of submission");
            int center = (128 * 256 + 128) * 4;
            Check(Math.Abs(ordered[center] - 64) <= 1 && ordered[center+1] == 0 && Math.Abs(ordered[center+2] - 128) <= 1, "two RGBA surfaces composite over background");
            var reversed = Ordered(false, true);
            Check(Math.Abs(reversed[center] - 128) <= 1 && Math.Abs(reversed[center+2] - 64) <= 1, "reversed camera reverses transparent depth order");
            Check(DepthAt(128) == 1, "queued blend does not write depth");
            // An opaque foreground must still occlude a later alpha surface.
            Begin(); renderer.DrawRoom(plain, Matrix4D.Identity with { M43 = -.4 }, Matrix4x4.Identity, new(0,1,0,1), null, true, false, true);
            renderer.DrawRoom(halfRed, Matrix4D.Identity, Matrix4x4.Identity, new(1,1,1,1), null, true, true, false, vertexColors: true);
            var occluded = Pixels(); ModelRenderer.End();
            Check(occluded[center] == 0 && occluded[center+1] == 255 && occluded[center+2] == 0, "opaque depth occludes alpha geometry");
            Render(rgba, true, transparent: true);
            if (capture is not null) FrameCapture.SaveBitmap(capture,256,256);
            Console.WriteLine($"Passed {checks} RGB/RGBA raster, interpolation, alpha coverage, depth and sorting checks.");
            CheckRecoveredModel(renderer, capture);
        }
        finally { Glfw.Terminate(); }
    }
    private static unsafe void CheckRecoveredModel(ModelRenderer renderer, string? capture)
    {
        var catalog = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory,"content/models.json"));
        using var model = new GpuModel(catalog,catalog.Sources["gaomon:2"]);
        var colored = model.Mesh.Meshes.SelectMany(m => m).Where(p => p.ColorComponents == 4).ToArray();
        if (colored.Length != 4 || colored.Sum(p => p.Indices.Length / 3) != 320 || colored.Sum(p => p.VertexCount) != 216)
            throw new InvalidDataException("Recovered MachGaogamon source RGBA geometry is absent.");
        foreach (var primitive in colored)
        {
            var material = model.MaterialFor(primitive);
            if (!material.Transparent || material.DepthWrite || material.AlphaCutoff != 0 || model.Materials[primitive.Material].Transparent)
                throw new InvalidDataException("Actual model RGBA material variant lost its source opacity or changed the legacy variant.");
            if (material.ColorMap is not { } map || model.Textures[map.Texture].ReadPixels().Where((_,i) => i % 4 == 3).Any(a => a != 255))
                throw new InvalidDataException("The recovered sonic control texture must be fully opaque to isolate vertex opacity.");
        }
        var animator = new ModelAnimator(model.Rig); animator.Update(.17);
        var camera = Matrix4x4.CreateLookAt(new(2.6f,1.8f,5),new(0,.9f,0),Vector3.UnitY)
            * Matrix4x4.CreateOrthographic(3,3,.01f,100);
        byte[] Render()
        {
            ModelRenderer.Begin(256,256); OpenGl.ClearColor(new(0,0,0,0)); OpenGl.Clear(GlClearBit.Color);
            renderer.BeginTransparency(); renderer.Draw(model,animator.Pose,model.PreviewPlacement,camera,true);
            if (renderer.PendingTransparentDraws != 4) throw new InvalidDataException("Recovered sonic meshes did not enter the model transparency queue.");
            renderer.FlushTransparency();
            byte[] result = Pixels(); var depths = new float[256*256];
            fixed (float* p = depths) OpenGl.ReadPixels(0,0,256,256,OpenGl.GL_DEPTH_COMPONENT,OpenGl.GL_FLOAT,p);
            if (depths.Any(d => d != 1)) throw new InvalidDataException("Actual model blend wrote background depth.");
            ModelRenderer.End(); return result;
        }
        byte[] pixels = Render();
        int partial = Enumerable.Range(0,256*256).Count(p => pixels[p*4+3] is > 0 and < 255);
        if (partial < 12)
            throw new InvalidDataException($"Recovered source did not rasterize variable opacity without depth writes: {partial} partial pixels.");
        if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"model.bmp"),256,256);
        var geometry = model.Mesh.Meshes.SelectMany(m => m).ToArray();
        void Upload(bool opaque)
        {
            for (int i = 0; i < geometry.Length; i++) if (geometry[i].ColorComponents == 4)
            {
                float[] colors = (float[])geometry[i].Colors.Clone();
                if (opaque) for (int c = 3; c < colors.Length; c += 4) colors[c] = 1;
                model.Primitives[i].UpdateColors(colors);
            }
        }
        byte[] disabled;
        try { Upload(true); disabled = Render(); if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"alpha-disabled.bmp"),256,256); }
        finally { Upload(false); }
        int changed = Enumerable.Range(0,256*256).Count(p => pixels[p*4+3] != disabled[p*4+3]);
        int disabledPartial = Enumerable.Range(0,256*256).Count(p => disabled[p*4+3] is > 0 and < 255);
        if (changed < 12 || disabledPartial != 0 || !Render().SequenceEqual(pixels))
            throw new InvalidDataException($"Same-pose vertex-alpha control failed: {changed} changed pixels, {disabledPartial} remaining partial pixels.");
        Console.WriteLine($"Actual MachGaogamon model path: 4 RGBA meshes, 320 triangles, 216 vertices, {partial} partial-alpha pixels; alpha-disabled control changes {changed} pixels with {disabledPartial} fractional pixels; source upload restores byte-identical output. Material variants, skinned upload, model queue and background depth verified.");
    }
    private static GpuPrimitive Quad(float[] colors)
    {
        float[] vertices = new float[4 * GltfPrimitive.Stride];
        float[] xy = [-1,-1, 1,-1, 1,1, -1,1];
        for (int i = 0; i < 4; i++) { vertices[i * GltfPrimitive.Stride] = xy[i*2]; vertices[i * GltfPrimitive.Stride+1] = xy[i*2+1]; vertices[i * GltfPrimitive.Stride+2] = .5f; vertices[i * GltfPrimitive.Stride+5] = 1; }
        return new(new(vertices,[0,1,2,0,2,3],-1,true,false) { Colors = colors });
    }
    private static void Begin() { ModelRenderer.Begin(256,256); OpenGl.ClearColor(new(0,0,0,1)); OpenGl.Clear(GlClearBit.Color); }
    private static unsafe byte[] Pixels()
    {
        var bytes = new byte[256*256*4]; fixed (byte* data = bytes) OpenGl.ReadPixels(0,0,256,256,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,data); return bytes;
    }
}

using System.Numerics;
using Brutal.GlfwApi;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

internal static class BoardMeshSmoke
{
    public static unsafe void Run(string? capture)
    {
        if (!Glfw.Init()) throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 4); Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 5); Glfw.WindowHint(GlfwWindowHint.Visible, 0);
            const int columns = 6, rows = 7;
            using var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo { Title = "Board mesh verification", Size = new int2(1440, 1680) });
            window.MakeContextCurrent();
            if (!Glad.Initialize(name => Glfw.PInvoke.GetProcAddress(name))) throw new InvalidOperationException("OpenGL initialization failed.");
            int cell = Math.Min(window.FramebufferSize.X / columns, window.FramebufferSize.Y / rows);
            if (cell < 32) throw new InvalidOperationException("Board capture framebuffer is too small.");
            using var renderer = new ModelRenderer();
            CheckVertexColors(renderer);
            CheckPoints(renderer);
            CheckLines(renderer);
            OpenGl.ClearColor(new float4(.025f, .035f, .06f, 1)); OpenGl.Clear(GlClearBit.Color);
            var camera = Matrix4x4.CreateLookAt(new(0, 25, 28), Vector3.Zero, Vector3.UnitY) * Matrix4x4.CreatePerspectiveFieldOfView(.9f, 1, .1f, 150);
            using var board = new ArenaBoard(7);
            int index = 0, draws = 0;
            foreach (string file in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "content/boards"), "*.json.gz").Order())
            {
                var scene = BoardScene.Load(file);
                using var layer = new BoardMeshLayer(scene);
                ModelRenderer.Begin(cell, cell); OpenGl.Viewport(index % columns * cell, (rows - 1 - index / columns) * cell, cell, cell);
                int expected = scene.Objects.Where(o => o.Visible).Sum(o => (o.Instances?.Length ?? 1) * (o.Materials.Length == 1 ? 1 : scene.Geometries[o.Geometry].Groups.Length));
                layer.Draw(renderer, camera, false, cell); int actual = layer.DrawCalls;
                board.Draw(renderer, camera);
                layer.Draw(renderer, camera, true, cell); actual += layer.DrawCalls;
                if (actual != expected || actual == 0) throw new InvalidOperationException("Board mesh draw inventory mismatch: " + scene.Id);
                ModelRenderer.End(); draws += actual; index++;
            }
            if (index != 37) throw new InvalidOperationException("Board mesh smoke must cover every theme.");
            if (capture is not null) FrameCapture.SaveBitmap(capture, cell * columns, cell * rows);
            Console.WriteLine($"Board geometry GPU smoke: {index} themes, {draws:N0} material/instance draws; vertex colors, wireframe, lines, point sizing/orientation and shader state reset verified.");
        }
        finally { Glfw.Terminate(); }
    }

    private static unsafe void CheckVertexColors(ModelRenderer renderer)
    {
        float[] vertices = new float[3 * GltfPrimitive.Stride];
        float[] positions = [-1,-1,.5f, 1,-1,.5f, 0,1,.5f];
        for (int i = 0; i < 3; i++) { Array.Copy(positions, i * 3, vertices, i * GltfPrimitive.Stride, 3); vertices[i * GltfPrimitive.Stride + 5] = 1; }
        using var triangle = new GpuPrimitive(new(vertices, [0,1,2], -1, true, false) { Colors = [1,0,0, 1,0,0, 1,0,0] });
        byte[] Pixel() { var p = new byte[4]; fixed (byte* bytes = p) OpenGl.ReadPixels(16, 16, 1, 1, OpenGl.GL_RGBA, OpenGl.GL_UNSIGNED_BYTE, bytes); return p; }
        ModelRenderer.Begin(32,32);
        renderer.DrawRoom(triangle, Matrix4D.Identity, Matrix4x4.Identity, new(1,1,1,1), null, true, false, true, vertexColors: true);
        var red = Pixel(); ModelRenderer.End();
        if (red[0] < 250 || red[1] > 2 || red[2] > 2) throw new InvalidOperationException("Vertex colors did not reach the framebuffer.");
        ModelRenderer.Begin(32,32);
        OpenGl.ClearColor(new(0,0,0,1)); OpenGl.Clear(GlClearBit.Color);
        renderer.DrawRoom(triangle, Matrix4D.Identity, Matrix4x4.Identity, new(1,1,1,1), null, true, false, true, wireframe: true);
        var interior = Pixel(); ModelRenderer.End();
        if (interior.Take(3).Any(c => c > 2)) throw new InvalidOperationException("Wireframe triangle interior was filled.");
        ModelRenderer.Begin(32,32);
        renderer.DrawStatic(triangle, Matrix4D.Identity, Matrix4x4.Identity, new(1,1,1,1));
        var white = Pixel(); ModelRenderer.End();
        if (white.Take(3).Any(c => c < 250)) throw new InvalidOperationException("Vertex colors leaked into an unrelated draw.");
    }

    private static unsafe void CheckPoints(ModelRenderer renderer)
    {
        float[] vertices = new float[GltfPrimitive.Stride]; vertices[2] = -4; vertices[5] = 1;
        using var point = new GpuPrimitive(new(vertices,[0],-1,true,false), OpenGl.GL_POINTS);
        var camera = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI/2,1,.1f,100);
        byte[] Draw(float depth, float size, bool attenuate, GpuTexture? texture = null)
        {
            vertices[2] = -depth; point.UpdateVertices(vertices);
            ModelRenderer.Begin(64,64); OpenGl.ClearColor(new(0,0,0,1)); OpenGl.Clear(GlClearBit.Color);
            renderer.DrawRoom(point,Matrix4D.Identity,camera,new(1,1,1,1),texture,true,false,true,
                points:new(size,64,attenuate,new(1,0,0,0),new(0,1,0,0)));
            byte[] pixels = new byte[64*64*4];
            fixed(byte* p=pixels) OpenGl.ReadPixels(0,0,64,64,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,p);
            ModelRenderer.End(); return pixels;
        }
        foreach (var sample in new[] { (4f,4f,true,32), (8f,4f,true,16), (4f,12f,false,12), (8f,12f,false,12) })
        {
            var pixels = Draw(sample.Item1,sample.Item2,sample.Item3);
            int width = Enumerable.Range(0,64).Count(x=>pixels[(32*64+x)*4]>250);
            if (width != sample.Item4) throw new InvalidOperationException($"Point size mismatch: {width} vs {sample.Item4}.");
        }
        var source = new BoardTexture(1,2,Convert.ToBase64String([255,0,0,255,0,0,255,255]),true,[1,1],[0,0],[0,0],0,1001,1001,"srgb");
        using var texture = new GpuTexture(BoardMesh.Image(source),new(MagFilter:9728,MinFilter:9728,WrapS:33071,WrapT:33071),true);
        var image = Draw(4,32,false,texture);
        int top = (40*64+32)*4, bottom = (24*64+32)*4;
        if (image[top]<250 || image[top+2]>2 || image[bottom]>2 || image[bottom+2]<250) throw new InvalidOperationException("Point sprite texture orientation changed.");
    }

    private static unsafe void CheckLines(ModelRenderer renderer)
    {
        var vertices = new float[GltfPrimitive.Stride*2];
        vertices[0]=-.5f; vertices[2]=.5f; vertices[GltfPrimitive.Stride]=.5f; vertices[GltfPrimitive.Stride+2]=.5f;
        using var line = new GpuPrimitive(new(vertices,[0,1],-1,false,false),OpenGl.GL_LINES);
        ModelRenderer.Begin(32,32); OpenGl.ClearColor(new(0,0,0,1)); OpenGl.Clear(GlClearBit.Color);
        renderer.DrawRoom(line,Matrix4D.Identity,Matrix4x4.Identity,new(1,1,1,1),null,true,false,true);
        var pixels=new byte[32*32*4];
        fixed(byte* p=pixels) OpenGl.ReadPixels(0,0,32,32,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,p);
        ModelRenderer.End();
        int visible=Enumerable.Range(0,32*32).Count(i=>pixels[i*4]>250);
        if (visible<14 || visible>18) throw new InvalidOperationException("Native line segment did not rasterize correctly.");
    }
}

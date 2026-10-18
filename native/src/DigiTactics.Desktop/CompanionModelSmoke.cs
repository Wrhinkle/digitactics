using System.Numerics;
using Brutal.GlfwApi;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

internal static class CompanionModelSmoke
{
    public static unsafe void Run(string? capture)
    {
        var catalog = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
        var companions = CompanionCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/companion-models.json"));
        if (!Glfw.Init()) throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 4); Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 5); Glfw.WindowHint(GlfwWindowHint.Visible, 0);
            const int cell = 140, columns = 11, rows = 7;
            using var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo { Title = "Companion model verification", Size = new int2(cell * columns, cell * rows) });
            window.MakeContextCurrent();
            if (!Glad.Initialize(name => Glfw.PInvoke.GetProcAddress(name))) throw new InvalidOperationException("OpenGL initialization failed.");
            OpenGl.ClearColor(new float4(.025f, .035f, .06f, 1)); OpenGl.Clear(GlClearBit.Color);
            using var renderer = new ModelRenderer();
            var camera = Matrix4x4.CreateLookAt(new(0, 1.8f, 5), new(0, .9f, 0), Vector3.UnitY) * Matrix4x4.CreatePerspectiveFieldOfView(.65f, 1, .01f, 100);
            int index = 0;
            foreach (var (id, visual) in companions.Bases)
            {
                using var model = new GpuModel(catalog, catalog.Sources[visual.Source]);
                var pose = new RigPose(model.Rig); pose.Sample(ModelAnimator.FindIdle(model.Rig), .37);
                int left = index % columns * cell, bottom = (rows - 1 - index / columns) * cell;
                ModelRenderer.Begin(cell, cell); OpenGl.Viewport(left, bottom, cell, cell);
                int draws = 0;
                foreach (bool transparent in new[] { false, true }) { renderer.Draw(model, pose, model.PreviewPlacement, camera, transparent); draws += renderer.DrawCalls; }
                ModelRenderer.End();
                var pixels = new byte[cell * cell * 4];
                fixed (byte* data = pixels) OpenGl.ReadPixels(left, bottom, cell, cell, OpenGl.GL_RGBA, OpenGl.GL_UNSIGNED_BYTE, data);
                int visible = 0;
                for (int p = 0; p < pixels.Length; p += 4) if (pixels[p] > 30 || pixels[p + 1] > 30 || pixels[p + 2] > 40) visible++;
                if (draws == 0 || visible < 80) throw new InvalidOperationException($"Companion produced no visible model: {id} ({visible} pixels).");
                index++;
            }
            if (capture is not null) FrameCapture.SaveBitmap(capture, cell * columns, cell * rows);
            Console.WriteLine($"Companion GPU smoke passed: {index} original companion models rendered with textures and prepared poses.");
        }
        finally { Glfw.Terminate(); }
    }
}

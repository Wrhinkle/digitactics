using Brutal.GlfwApi;
using Brutal.ImGuiApi;
using Brutal.ImGuiApi.Backends;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Rules;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

public static class NativeApplication
{
    public static unsafe void Run(ContentCatalog catalog, bool smoke, string? capture = null)
    {
        if (smoke)
        {
            var models = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
            foreach (string asset in models.Assets.Keys)
            {
                using var model = models.Open(Path.Combine(AppContext.BaseDirectory, "assets"), asset);
                if (model.Root.GetProperty("asset"u8).GetProperty("version"u8).GetString() != "2.0")
                    throw new InvalidDataException($"Unsupported packaged model: {asset}");
                _ = GltfRig.Load(model);
                if (model.Root.TryGetProperty("images"u8, out var images))
                    for (int i = 0; i < images.GetArrayLength(); i++) _ = model.Image(i);
            }
            Console.WriteLine($"Native asset smoke passed: {models.Assets.Count} verified model payloads, rigs and textures.");
        }
        if (!Glfw.Init()) throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 4);
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 5);
            Glfw.WindowHint(GlfwWindowHint.Visible, smoke ? 0 : 1);
            using var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo
            { Title = "Digi-Tactics | BRUTAL migration", Size = new int2(1280, 800) });
            window.MakeContextCurrent();
            if (!Glad.Initialize(name => Glfw.PInvoke.GetProcAddress(name))) throw new InvalidOperationException("BRUTAL OpenGL initialization failed.");
            Glfw.SwapInterval(1);
            if (smoke) TextureSmoke.Run();
            var context = ImGui.CreateContext();
            try
            {
                ImGui.StyleColorsDark();
                ImGui.GetIO().IniFilename = default;
                ImGui.GetStyle().FontSizeBase = 18;
                using var platform = new GlfwBackend(window, GlfwBackend.ClientApi.OpenGL);
                using var renderer = new OpenGlBackend();
                var screen = new ShopScreen(catalog);
                int frames = 0;
                while (!window.ShouldClose)
                {
                    Glfw.PollEvents();
                    renderer.NewFrame();
                    platform.NewFrame();
                    if (smoke && frames is 3 or 4)
                    {
                        // Exercise the actual ImGui purchase button, not just the rules object.
                        var io = ImGui.GetIO();
                        io.AddMousePosEvent(260, 235);
                        io.AddMouseButtonEvent(0, frames == 3);
                    }
                    ImGui.NewFrame();
                    screen.Draw(window.Size);
                    ImGui.Render();
                    var size = window.FramebufferSize;
                    OpenGl.Viewport(0, 0, size.X, size.Y);
                    OpenGl.ClearColor(new float4(0.025f, 0.035f, 0.06f, 1));
                    OpenGl.Clear(GlClearBit.Color);
                    renderer.RenderDrawData(ImGui.GetDrawData());
                    if (smoke && frames == 9 && capture is not null)
                        FrameCapture.SaveBitmap(capture, size.X, size.Y);
                    window.SwapBuffers();
                    frames++;
                    if (smoke && frames >= 10)
                    {
                        if (screen.OwnedCount != 1) throw new InvalidOperationException("Native UI purchase smoke failed.");
                        Console.WriteLine($"Native smoke passed: {frames} rendered BRUTAL frames, native UI purchase, {catalog.Variants.Length} catalogs.");
                        break;
                    }
                }
            }
            finally { ImGui.DestroyContext(context); }
        }
        finally { Glfw.Terminate(); }
    }
}

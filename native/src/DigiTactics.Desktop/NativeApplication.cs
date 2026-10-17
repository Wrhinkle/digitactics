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
                var rig = GltfRig.Load(model);
                foreach (var source in models.Sources.Values.Where(s => s.Asset == asset))
                    _ = rig.Prepare(source, Path.Combine(AppContext.BaseDirectory, "assets"));
                if (model.Root.TryGetProperty("images"u8, out var images))
                    for (int i = 0; i < images.GetArrayLength(); i++) _ = model.Image(i);
            }
            Console.WriteLine($"Native asset smoke passed: {models.Assets.Count} verified model payloads, rigs and textures; {models.Sources.Count} prepared motions.");
            CombatAnimationSmoke.Run(catalog);
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
                using var preview = new ModelPreview();
                var screen = new ShopScreen(catalog);
                var clock = System.Diagnostics.Stopwatch.StartNew();
                int frames = 0;
                double lastFrame = 0;
                BattleView? battle = null;
                try
                {
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
                        double frameTime = clock.Elapsed.TotalSeconds;
                        double delta = smoke ? CombatSimulation.Tick : Math.Min(.1, frameTime - lastFrame); lastFrame = frameTime;
                        if (battle is not null)
                        {
                            battle.Advance(delta); battle.Controls(window.Size);
                            battle.Labels(window.Size.X, (int)(window.Size.Y * .8), window.Size.Y);
                            if (battle.BackRequested) { battle.Dispose(); battle = null; }
                        }
                        else
                        {
                            screen.Draw(window.Size);
                            if (screen.Preview is { } selected) preview.Select(selected.Variant, selected.Line, selected.Stage);
                            if (screen.RequestedAction == "idle") preview.EndAction();
                            else if (screen.RequestedAction is { } action) preview.Play(action);
                            if (smoke)
                            {
                                if (frames == 5 && !preview.Play("attack") || frames == 6 && !preview.Play("special") || frames == 8 && !preview.Play("win"))
                                    throw new InvalidOperationException("Native model action smoke failed.");
                                if (frames is 7 or 9 && preview.Play("attack")) throw new InvalidOperationException("Attack interrupted a protected action.");
                            }
                            if (screen.BattleRequested) battle = new(screen.Rules, screen.BattleRoster());
                        }
                        ImGui.Render();
                        var size = window.FramebufferSize;
                        OpenGl.Viewport(0, 0, size.X, size.Y);
                        OpenGl.ClearColor(new float4(0.025f, 0.035f, 0.06f, 1));
                        OpenGl.Clear(GlClearBit.Color);
                        if (battle is not null) battle.Draw(size.X, (int)(size.Y * .8));
                        else preview.Draw(size.X, (int)(size.Y * .42), smoke ? frames / 60.0 : clock.Elapsed.TotalSeconds);
                        if (smoke && frames == 9) ModelPreview.VerifyVisible(size.X, (int)(size.Y * .42));
                        OpenGl.Viewport(0, 0, size.X, size.Y);
                        renderer.RenderDrawData(ImGui.GetDrawData());
                        if (smoke && frames == 9 && capture is not null)
                            FrameCapture.SaveBitmap(capture, size.X, size.Y);
                        if (smoke && frames == 129 && capture is not null)
                            FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "battle.bmp"), size.X, size.Y);
                        window.SwapBuffers();
                        frames++;
                        if (smoke && frames == 10)
                        {
                            if (screen.OwnedCount != 1) throw new InvalidOperationException("Native UI purchase smoke failed.");
                            if (preview.DrawCalls == 0) throw new InvalidOperationException("Native model drawing smoke failed.");
                            Console.WriteLine($"Native smoke passed: {frames} rendered BRUTAL frames, native UI purchase, {catalog.Variants.Length} catalogs.");
                            battle = new(screen.Rules);
                        }
                        if (smoke && frames >= 130)
                        {
                            if (battle is null || battle.DrawCalls == 0 || battle.Combat.Time < 3) throw new InvalidOperationException("Native battle rendering smoke failed.");
                            Console.WriteLine($"Native battle rendering passed: {battle.Combat.Time:F1}s, {battle.DrawCalls} model draws, {battle.ModelCount} shared models.");
                            break;
                        }
                    }
                }
                finally { battle?.Dispose(); }
            }
            finally { ImGui.DestroyContext(context); }
        }
        finally { Glfw.Terminate(); }
    }
}

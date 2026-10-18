using Brutal.GlfwApi;
using Brutal.ImGuiApi;
using Brutal.ImGuiApi.Backends;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal static class MatchApplication
{
    public static unsafe void Run(ContentCatalog catalog, bool smoke, string? capture, bool duos = false, string? profileDirectory = null, bool collectionSmoke = false, bool evolutionSmoke = false, bool trainingSmoke = false)
    {
        if (!Glfw.Init()) throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 4); Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 5);
            Glfw.WindowHint(GlfwWindowHint.Visible, smoke ? 0 : 1);
            using var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo { Title = "Digi-Tactics", Size = new int2(1440, 900) });
            window.SetSizeLimits(new(1100, 760), new(-1, -1));
            window.MakeContextCurrent();
            if (!Glad.Initialize(name => Glfw.PInvoke.GetProcAddress(name))) throw new InvalidOperationException("OpenGL initialization failed.");
            Glfw.SwapInterval(smoke ? 0 : 1);
            var context = ImGui.CreateContext();
            try
            {
                ImGui.StyleColorsDark(); ImGui.GetIO().IniFilename = default; ImGui.GetStyle().FontSizeBase = 16;
                string font = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "segoeui.ttf");
                if (File.Exists(font)) ImGui.GetIO().Fonts.AddFontFromFileTTF(font, 17);
                var style = ImGui.GetStyle(); style.WindowRounding = 8; style.FrameRounding = 5;
                style.WindowPadding = new(12, 10); style.ItemSpacing = new(6, 6);
                style.Colors[(int)ImGuiCol.WindowBg] = new(.045f, .065f, .095f, .98f);
                style.Colors[(int)ImGuiCol.TitleBgActive] = new(.08f, .14f, .2f, 1);
                style.Colors[(int)ImGuiCol.Button] = new(.10f, .22f, .31f, 1);
                style.Colors[(int)ImGuiCol.ButtonHovered] = new(.13f, .38f, .47f, 1);
                style.Colors[(int)ImGuiCol.ButtonActive] = new(.12f, .5f, .48f, 1);
                using var platform = new GlfwBackend(window, GlfwBackend.ClientApi.OpenGL);
                using var audio = new GameAudio();
                audio.EffectsVolume = smoke ? 0 : .8f; audio.MusicVolume = smoke ? 0 : .6f;
                using var preferences = smoke ? null : new AudioSettingsBinding(audio);
                var profile = smoke && profileDirectory is null ? null : new NativeProfileStore(profileDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DigiTactics", "native"));
                using var renderer = new OpenGlBackend(); using var screen = new MatchScreen(catalog, audio, smoke ? 143u : null, duos, profile);
                var driver = smoke && !trainingSmoke ? new MatchScreenSmoke(collectionSmoke ? profile : null, evolutionSmoke) : null;
                using var trainer = trainingSmoke ? new TrainingSmoke(profile!, profileDirectory!) : null;
                var clock = System.Diagnostics.Stopwatch.StartNew(); double last = 0;
                for (int frame = 0; !window.ShouldClose; frame++)
                {
                    Glfw.PollEvents(); renderer.NewFrame(); platform.NewFrame(); driver?.Input(screen); trainer?.Input(screen);
                    ImGui.NewFrame(); double now = clock.Elapsed.TotalSeconds;
                    double delta = smoke ? CombatSimulation.Tick : Math.Clamp(now - last, 0, .1);
                    screen.Update(delta, smoke ? delta : now - last); last = now;
                    screen.Draw(window.Size); ImGui.Render();
                    audio.Update();
                    preferences?.Update(delta);
                    var size = window.FramebufferSize;
                    OpenGl.Viewport(0, 0, size.X, size.Y); OpenGl.ClearColor(new float4(.025f, .035f, .06f, 1)); OpenGl.Clear(GlClearBit.Color);
                    screen.Render(size); OpenGl.Viewport(0, 0, size.X, size.Y); renderer.RenderDrawData(ImGui.GetDrawData());
                    if (driver is not null && driver.Verify(screen, size, capture)) break;
                    if (trainer is not null && trainer.Verify(screen, size, capture)) break;
                    window.SwapBuffers();
                    if (smoke && frame > (trainingSmoke ? 6000 : 900))
                    {
                        if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "failure.bmp"), size.X, size.Y);
                        throw new InvalidOperationException("Match UI smoke did not complete: " + (trainer?.Pending ?? driver?.Pending));
                    }
                }
            }
            finally { ImGui.DestroyContext(context); }
        }
        finally { Glfw.Terminate(); }
    }
}

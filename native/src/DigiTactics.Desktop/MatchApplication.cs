using Brutal.GlfwApi;
using Brutal.ImGuiApi;
using Brutal.ImGuiApi.Backends;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal static class MatchApplication
{
    public static unsafe void Run(ContentCatalog catalog, bool smoke, string? capture, bool duos = false)
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
                using var renderer = new OpenGlBackend(); using var screen = new MatchScreen(catalog, smoke ? 143u : null, duos);
                var driver = smoke ? new MatchScreenSmoke() : null;
                var clock = System.Diagnostics.Stopwatch.StartNew(); double last = 0;
                for (int frame = 0; !window.ShouldClose; frame++)
                {
                    Glfw.PollEvents(); renderer.NewFrame(); platform.NewFrame(); driver?.Input(screen);
                    ImGui.NewFrame(); double now = clock.Elapsed.TotalSeconds;
                    screen.Update(smoke ? CombatSimulation.Tick : Math.Clamp(now - last, 0, .1)); last = now;
                    screen.Draw(window.Size); ImGui.Render();
                    var size = window.FramebufferSize;
                    OpenGl.Viewport(0, 0, size.X, size.Y); OpenGl.ClearColor(new float4(.025f, .035f, .06f, 1)); OpenGl.Clear(GlClearBit.Color);
                    screen.Render(size); OpenGl.Viewport(0, 0, size.X, size.Y); renderer.RenderDrawData(ImGui.GetDrawData());
                    if (driver is not null && driver.Verify(screen, size, capture)) break;
                    window.SwapBuffers();
                    if (smoke && frame > 900) throw new InvalidOperationException("Match UI smoke did not complete.");
                }
            }
            finally { ImGui.DestroyContext(context); }
        }
        finally { Glfw.Terminate(); }
    }
}

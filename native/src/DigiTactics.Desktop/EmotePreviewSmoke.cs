using System.Security.Cryptography;
using System.Text.Json;
using Brutal.GlfwApi;
using Brutal.ImGuiApi;
using Brutal.ImGuiApi.Backends;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>Actual cached native preview composition against full source SVG captures.</summary>
internal static class EmotePreviewSmoke
{
    public static unsafe void Run(string directory)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "manifest.json")));
        using var shipped = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "content/emotes/manifest.json")));
        var manifest = document.RootElement;
        if (manifest.GetProperty("sourceHash"u8).GetString() != shipped.RootElement.GetProperty("sourceHash"u8).GetString())
            throw new InvalidOperationException("Emote preview references do not match shipped artwork.");
        var expected = shipped.RootElement.GetProperty("clips"u8).EnumerateObject().SelectMany(clip => new long[] { 2550, 2600, 5950, 6000 }.Select(age => (clip.Name, Age: age))).OrderBy(cell => cell.Name, StringComparer.Ordinal).ThenBy(cell => cell.Age);
        var actual = manifest.GetProperty("pages"u8).EnumerateArray().SelectMany(page => page.GetProperty("cells"u8).EnumerateArray().Select(cell => (Name: cell.GetProperty("id"u8).GetString()!, Age: cell.GetProperty("age"u8).GetInt64()))).OrderBy(cell => cell.Name, StringComparer.Ordinal).ThenBy(cell => cell.Age);
        if (!actual.SequenceEqual(expected)) throw new InvalidOperationException("Preview references must cover every shipped emote at all four boundary times exactly once.");
        int width = manifest.GetProperty("width"u8).GetInt32(), height = manifest.GetProperty("height"u8).GetInt32();
        if (!Glfw.Init()) throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 4); Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 5); Glfw.WindowHint(GlfwWindowHint.Visible, 0);
            using var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo { Title = "Emote preview verification", Size = new(width, height) }); window.MakeContextCurrent();
            if (!Glad.Initialize(name => Glfw.PInvoke.GetProcAddress(name))) throw new InvalidOperationException("OpenGL initialization failed.");
            var context = ImGui.CreateContext();
            try
            {
                ImGui.GetIO().IniFilename = default;
                using var platform = new GlfwBackend(window, GlfwBackend.ClientApi.OpenGL); using var backend = new OpenGlBackend(); using var artwork = new EmoteArtwork();
                int pages = 0, samples = 0; long peakBytes = 0; double peakError = 0;
                foreach (var page in manifest.GetProperty("pages"u8).EnumerateArray())
                {
                    int uploaded = 0;
                    for (int repeat = 0; repeat < 2; repeat++)
                    {
                        UiFrameLifetime.Begin(); Glfw.PollEvents(); backend.NewFrame(); platform.NewFrame(); ImGui.NewFrame(); artwork.BeginFrame();
                        ImGui.SetNextWindowPos(new(0, 0)); ImGui.SetNextWindowSize(new(width, height)); ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new float2(0, 0));
                        ImGui.Begin("Emote source preview checks"u8, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoScrollbar);
                        int index = 0;
                        foreach (var cell in page.GetProperty("cells"u8).EnumerateArray())
                        {
                            artwork.Draw(cell.GetProperty("id"u8).GetString()!, cell.GetProperty("age"u8).GetInt64(), new(index % 6 * 160 + 20, index / 6 * 160 + 20), 120);
                            index++;
                        }
                        ImGui.End(); ImGui.PopStyleVar(); ImGui.Render();
                        if (window.FramebufferSize != new int2(width, height)) throw new InvalidOperationException("Emote reference requires a 1:1 framebuffer.");
                        OpenGl.Viewport(0, 0, width, height); OpenGl.ClearColor(new float4(20 / 255f, 33 / 255f, 48 / 255f, 1)); OpenGl.Clear(GlClearBit.Color); backend.RenderDrawData(ImGui.GetDrawData());
                        if (repeat == 0) uploaded = artwork.Uploads;
                        else
                        {
                            if (artwork.Uploads != uploaded) throw new InvalidOperationException("Unchanged preview layers were decoded/uploaded again.");
                            byte[] bytes = File.ReadAllBytes(Path.Combine(directory, page.GetProperty("file"u8).GetString()!));
                            if (Convert.ToHexStringLower(SHA256.HashData(bytes)) != page.GetProperty("sha256"u8).GetString()) throw new InvalidOperationException("Source preview reference changed.");
                            var source = PngImage.Decode(bytes); var pixels = new byte[width * height * 3];
                            fixed (byte* ptr = pixels) OpenGl.ReadPixels(0, 0, width, height, OpenGl.GL_RGB, OpenGl.GL_UNSIGNED_BYTE, ptr);
                            var errors = new long[index]; var large = new long[index];
                            for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) for (int channel = 0; channel < 3; channel++)
                            {
                                int cell = y / 160 * 6 + x / 160; if (cell >= index) continue;
                                int delta = Math.Abs(source.Rgba[(y * width + x) * 4 + channel] - pixels[((height - 1 - y) * width + x) * 3 + channel]);
                                errors[cell] += delta; if (delta > 16) large[cell]++;
                            }
                            double mean = errors.Max() / (160d * 160 * 3); peakError = Math.Max(peakError, mean); peakBytes = Math.Max(peakBytes, artwork.CachedBytes);
                            if (mean > .75 || large.Max() / (160d * 160 * 3) > .005)
                            {
                                FrameCapture.SaveBitmap(Path.Combine(directory, $"native-failure-{pages}.bmp"), width, height);
                                throw new InvalidOperationException($"Native preview composition differs on page {pages}: RGB max cell MAE {mean:F4}.");
                            }
                            if (pages is 0 or 6) FrameCapture.SaveBitmap(Path.Combine(directory, $"native-{pages}.bmp"), width, height);
                            samples += index; pages++;
                        }
                        if (OpenGl.GetError() != OpenGl.GL_NO_ERROR) throw new InvalidOperationException("Emote preview generated an OpenGL error.");
                        window.SwapBuffers();
                    }
                }
                Console.WriteLine($"Emote previews: {samples} source/native samples across {pages} pages passed; RGB max cell MAE {peakError:F4}; peak cached RGBA bytes {peakBytes}; repeat frames reused cached textures.");
            }
            finally { ImGui.DestroyContext(context); }
        }
        finally { Glfw.Terminate(); }
    }
}

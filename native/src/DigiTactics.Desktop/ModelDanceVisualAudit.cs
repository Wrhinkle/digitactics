using System.Security.Cryptography;
using System.Text.Json;
using Brutal.GlfwApi;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Renders the actual kill-screen stage, not a substitute model turntable.
/// Wide and narrow rows retain production cameras and camera smoothing.</summary>
internal static class ModelDanceVisualAudit
{
    public static unsafe void Run(string directory, string source, string dance, bool companion)
    {
        Directory.CreateDirectory(directory);
        var models = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
        var companions = CompanionCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/companion-models.json"));
        var content = ContentCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/catalog.json"));
        var alias = models.Variants.SelectMany(variant => variant.Value.SelectMany(line => line.Value.Select((key, stage) => new { Variant = variant.Key, Line = line.Key, Stage = stage, Source = key }))).FirstOrDefault(a => a.Source == source);
        string? legend = companions.Bases.FirstOrDefault(p => p.Value.Source == source).Key;
        if (legend is null) foreach (var reachable in companions.Reachable)
            foreach (string form in reachable.Value) if (companions.Forms.TryGetValue(form, out var visual) && visual.Source == source) { legend = reachable.Key + "@" + form; break; }
        if (companion && legend is null || !companion && alias is null) throw new ArgumentException("Source is not reachable in requested dance context: " + source);
        var rules = alias is null ? content.Variants[0] : content.Variants.Single(r => r.Id == alias.Variant);
        EliminationMember[] team = companion ? [] : [new(alias!.Line, alias.Stage), new(alias.Line, alias.Stage)];
        var player = new EliminationPlayer(0, "Dance audit", true, 0, null, team);
        var entry = new EliminationEvent(player, player, false, [player], [player], null, team);
        double[] beats = [0, .25, .5, 1, 2, 4, 6, 8, 12, 16, 24, 32];
        if(dance=="macarena")beats=[..beats,48,64];
        double[] times = beats.Select(b => b * 60 / DanceChoreography.Bpm(dance)).Concat([3.99, 4, 4.01]).Distinct().Order().ToArray();
        const int cellWidth = 480, cellHeight = 360;
        int width = cellWidth * times.Length, height = cellHeight * 2, stride = width * 3;
        var sheet = Enumerable.Repeat((byte)16, stride * height).ToArray();
        var evidence = new List<object>();
        if (!Glfw.Init()) throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 4); Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 5); Glfw.WindowHint(GlfwWindowHint.Visible, 0);
            using var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo { Title = "Postmatch dance audit", Size = new int2(512, 512) });
            window.MakeContextCurrent();
            if (!Glad.Initialize(name => Glfw.PInvoke.GetProcAddress(name))) throw new InvalidOperationException("OpenGL initialization failed.");
            for (int row = 0; row < 2; row++)
            {
                int w = row == 0 ? 480 : 288, h = row == 0 ? 270 : 360;
                var playback = new EliminationPlayback(entry, 0);
                using var stage = new EliminationStage(rules, playback, companion ? legend : null, dance, "gate");
                for (int frame = 0; frame < times.Length; frame++)
                {
                    double time = times[frame];
                    while (playback.Time < time - 1e-9) { playback.Advance(Math.Min(1d / 30, time - playback.Time)); stage.Draw(w, h); }
                    stage.Draw(w, h);
                    var rgba = new byte[w * h * 4];
                    fixed (byte* p = rgba) OpenGl.GetTextureImage(stage.Texture, 0, OpenGl.GL_RGBA, OpenGl.GL_UNSIGNED_BYTE, rgba.Length, p);
                    int visible = 0, edge = 0;
                    for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
                    {
                        int p = (y * w + x) * 4; double alpha = rgba[p + 3] / 255d;
                        if (rgba[p + 3] > 60) { visible++; if (x < 2 || y < 2 || x >= w - 2 || y >= h - 2) edge++; }
                        int outX = frame * cellWidth + (cellWidth - w) / 2 + x, outY = (1 - row) * cellHeight + (cellHeight - h) / 2 + y;
                        int at = (outY * width + outX) * 3;
                        for (int c = 0; c < 3; c++) sheet[at + c] = (byte)Math.Round(rgba[p + 2 - c] * alpha + (c == 0 ? 31 : c == 1 ? 24 : 18) * (1 - alpha));
                    }
                    evidence.Add(new { row, frame, time, beat = time * DanceChoreography.Bpm(dance) / 60, aspect = (double)w / h, visiblePixels = visible, edgePixels = edge, stage.DrawCalls, stage.Members, stage.PoseSignature,
                        review = time > .6 && visible < 40 ? "empty-or-tiny" : edge > 5 ? "frame-edge" : "unreviewed" });
                }
            }
        }
        finally { Glfw.Terminate(); }
        string name = string.Concat(source.Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '_')) + "." + dance + (companion ? ".companion" : ".unit");
        using (var writer = new BinaryWriter(File.Create(Path.Combine(directory, name + ".bmp"))))
        {
            writer.Write((ushort)0x4d42); writer.Write(54 + sheet.Length); writer.Write(0); writer.Write(54); writer.Write(40); writer.Write(width); writer.Write(height); writer.Write((ushort)1); writer.Write((ushort)24);
            writer.Write(0); writer.Write(sheet.Length); writer.Write(2835); writer.Write(2835); writer.Write(0); writer.Write(0); writer.Write(sheet);
        }
        string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
        File.WriteAllText(Path.Combine(directory, name + ".json"), JsonSerializer.Serialize(new { source, dance, companion, status = "UNVERIFIED", renderer = "Production EliminationStage; no substitutions; two identical unit slots or original companion center slot; unbounded collection-style preview through 32 beats (64 for Macarena)",
            executable = Hash(typeof(EliminationStage).Assembly.Location), assets = Hash(Path.Combine(AppContext.BaseDirectory, "content/models.json")), bindings = Hash(Path.Combine(AppContext.BaseDirectory, "content/dance-bindings.json")), samples = evidence }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Dance render evidence: {name}; {evidence.Count} production stage captures. Visual review required.");
    }
}

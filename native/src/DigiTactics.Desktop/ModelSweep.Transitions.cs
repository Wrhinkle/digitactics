using System.Text.Json;
using Brutal.GlfwApi;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>
/// Frame strips of animation switches for review: the CombatAnimationProbe script runs through CombatModelAnimator
/// twice per source, once with the original unsmoothed switches and once as shipped. Each switch gets two rows
/// (unsmoothed above, shipped below) of consecutive 60 fps frames from 2 frames before the switch.
/// Launch: DigiTactics.Desktop.exe --transition-strips &lt;dir&gt; --strip-source &lt;key&gt;[,&lt;key&gt;...] [--strip-switches attack,end-special,...]
/// Writes &lt;dir&gt;/&lt;source&gt;.png and &lt;source&gt;.json (row order and frame times).
/// </summary>
internal static partial class ModelSweep
{
    private const int StripCell = 160, StripFrames = 12, StripLead = 2;

    public static unsafe void RunTransitionStrips(string directory, string[] keys, string[]? kinds)
    {
        Directory.CreateDirectory(directory);
        var catalog = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
        if (!Glfw.Init()) throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 4); Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 5); Glfw.WindowHint(GlfwWindowHint.Visible, 0);
            using var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo { Title = "Transition strips", Size = new int2(256, 256) }); window.MakeContextCurrent();
            if (!Glad.Initialize(name => Glfw.PInvoke.GetProcAddress(name))) throw new InvalidOperationException("OpenGL initialization failed.");
            using var renderer = new ModelRenderer { CosmeticStage = true };
            foreach (string key in keys)
            {
                using var model = new GpuModel(catalog, catalog.Sources[key]);
                var runs = new[] { false, true }.Select(smoothing => Record(model.Rig, key, smoothing)).ToArray();
                var switches = runs[1].Switches.Where(s => kinds is null || kinds.Contains(s.Kind)).ToArray();
                if (switches.Length == 0) continue;
                int width = StripCell * StripFrames, height = StripCell * 2 * switches.Length;
                using var target = new Target(width, height);
                var idle = new RigPose(model.Rig); runs[1].Poses[0].CopyTo(idle.Local, 0); idle.UpdateWorld();
                var (camera, eye) = FittedCamera(model, idle); renderer.FarmEye = eye;
                OpenGl.Native.glDisable(GlCapability.ScissorTest); OpenGl.ClearColor(new float4(.025f, .04f, .065f, 1)); OpenGl.Clear(GlClearBit.Color | GlClearBit.Depth);
                var pose = new RigPose(model.Rig);
                for (int row = 0; row < switches.Length; row++)
                    for (int pass = 0; pass < 2; pass++)
                    {
                        int first = runs[pass].Switches.First(s => s.Kind == switches[row].Kind && Math.Abs(s.Time - switches[row].Time) < 1e-6).Frame - StripLead;
                        for (int column = 0; column < StripFrames; column++)
                        {
                            int frame = Math.Clamp(first + column, 0, runs[pass].Poses.Count - 1);
                            runs[pass].Poses[frame].CopyTo(pose.Local, 0); pose.UpdateWorld();
                            // GL rows run bottom-up; row 0 / unsmoothed is drawn at the top of the image.
                            ModelRenderer.Begin(StripCell, StripCell); OpenGl.Viewport(column * StripCell, height - (row * 2 + pass + 1) * StripCell, StripCell, StripCell);
                            renderer.Draw(model, pose, model.PreviewPlacement, camera, false);
                            renderer.BeginTransparency(); renderer.Draw(model, pose, model.PreviewPlacement, camera, true); renderer.FlushTransparency(); ModelRenderer.End();
                        }
                    }
                string safe = key.Replace(':', '_').Replace(' ', '_');
                WritePngSized(Path.Combine(directory, safe + ".png"), width, height, ReadTopDown(width, height));
                File.WriteAllText(Path.Combine(directory, safe + ".json"), JsonSerializer.Serialize(new
                {
                    Source = key, CellPixels = StripCell, FramesPerRow = StripFrames, FrameSeconds = CombatAnimationProbe.Step, LeadFrames = StripLead,
                    Rows = switches.SelectMany(s => new[] { $"{s.Kind} @ {s.Time:0.000}s unsmoothed", $"{s.Kind} @ {s.Time:0.000}s shipped" }).ToArray(),
                }, new JsonSerializerOptions { WriteIndented = true }));
                Console.WriteLine($"Transition strips {key}: {switches.Length} switches -> {Path.Combine(directory, safe + ".png")}");
            }
        }
        finally { Glfw.Terminate(); }
    }

    private sealed record Recording(List<NodeTransform[]> Poses, List<(string Kind, double Time, int Frame)> Switches);

    private static Recording Record(GltfRig rig, string key, bool smoothing)
    {
        var animator = new CombatModelAnimator(rig, key) { Smoothing = smoothing };
        var poses = new List<NodeTransform[]>(); var switches = new List<(string, double, int)>();
        animator.Update(0, false); poses.Add(animator.Pose.Local.ToArray());
        int next = 0; bool moving = false; double time = 0;
        while (time < CombatAnimationProbe.ScriptEnd - 1e-9)
        {
            var script = CombatAnimationProbe.Script;
            while (next < script.Length && script[next].Time <= time + 1e-9)
            {
                var e = script[next++];
                if (e.Kind == "move") moving = true; else if (e.Kind == "stop") moving = false;
                else if (!animator.Play(e.Kind)) continue;
                switches.Add((e.Kind, e.Time, poses.Count));
            }
            string? before = animator.Role;
            animator.Update(CombatAnimationProbe.Step, moving); time += CombatAnimationProbe.Step;
            if (before is not null && animator.Role is null) switches.Add(("end-" + before, Math.Round(time, 3), poses.Count));
            poses.Add(animator.Pose.Local.ToArray());
        }
        return new(poses, switches);
    }

    private static void WritePngSized(string path, int width, int height, byte[] rgba)
    {
        using var output = File.Create(path); output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        byte[] header = new byte[13]; System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(header, width); System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height); header[8] = 8; header[9] = 6;
        Chunk(output, "IHDR"u8, header);
        using var compressed = new MemoryStream();
        using (var zlib = new System.IO.Compression.ZLibStream(compressed, System.IO.Compression.CompressionLevel.Fastest, true))
        {
            byte[] row = new byte[width * 4 + 1];
            for (int y = 0; y < height; y++) { Buffer.BlockCopy(rgba, y * width * 4, row, 1, width * 4); zlib.Write(row); }
        }
        Chunk(output, "IDAT"u8, compressed.ToArray()); Chunk(output, "IEND"u8, []);
    }
}

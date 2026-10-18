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
        var dances = DanceCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/dance-bindings.json"));
        if (!Glfw.Init()) throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 4); Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 5); Glfw.WindowHint(GlfwWindowHint.Visible, 0);
            const int cell = 140, columns = 11, rows = 8;
            using var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo { Title = "Companion model verification", Size = new int2(cell * columns, cell * rows) });
            window.MakeContextCurrent();
            if (!Glad.Initialize(name => Glfw.PInvoke.GetProcAddress(name))) throw new InvalidOperationException("OpenGL initialization failed.");
            OpenGl.ClearColor(new float4(.025f, .035f, .06f, 1)); OpenGl.Clear(GlClearBit.Color);
            using var renderer = new ModelRenderer { FarmLight = new FarmLighting(), FarmEye = new(0, 1.8f, 5) };
            var camera = Matrix4x4.CreateLookAt(new(0, 1.8f, 5), new(0, .9f, 0), Vector3.UnitY) * Matrix4x4.CreatePerspectiveFieldOfView(.65f, 1, .01f, 100);
            int index = 0;
            string normalSource = catalog.Sources.First(x => x.Value.Asset == "models/files/rip_nc_vulcanusmon.glb").Key;
            var samples = companions.Bases.Append(new KeyValuePair<string, CompanionVisual>("normal-map-vulcanusmon", companions.Bases.First().Value with { Source = normalSource }));
            foreach (var (id, visual) in samples)
            {
                using var model = new GpuModel(catalog, catalog.Sources[visual.Source]);
                if (id == "normal-map-vulcanusmon" && (!model.Materials.Any(m => m.NormalMap is not null) || !model.Mesh.Meshes.SelectMany(x => x).Any(p => p.Tangents.Length > 0))) throw new InvalidOperationException("Normal-map smoke did not load the original texture and tangents.");
                var animator = model.CreateCompanionAnimator(visual, companions.Fallbacks, height: 2);
                animator.Update(.37); animator.SetMoving(true); animator.Update(.07);
                animator.Play("happy"); animator.Update(.2); animator.SetMoving(false); animator.Update(.12);
                animator.Play("dance"); animator.Update(.31);
                var pose = animator.Pose;
                DanceRoot? danceRoot = id == "normal-map-vulcanusmon" ? null : new DanceSkeleton(dances.Binding(visual.Source)).Apply(DanceChoreography.Evaluate("floss", .7), pose);
                int left = index % columns * cell, bottom = (rows - 1 - index / columns) * cell;
                ModelRenderer.Begin(cell, cell); OpenGl.Viewport(left, bottom, cell, cell);
                int draws = 0;
                var placement = model.PlaceCompanion(animator, overlay: CompanionEmoteMotion.Evaluate("wiggle", 900, 2600), dance: danceRoot);
                foreach (bool transparent in new[] { false, true }) { renderer.Draw(model, pose, placement, camera, transparent); draws += renderer.DrawCalls; }
                ModelRenderer.End();
                var pixels = new byte[cell * cell * 4];
                fixed (byte* data = pixels) OpenGl.ReadPixels(left, bottom, cell, cell, OpenGl.GL_RGBA, OpenGl.GL_UNSIGNED_BYTE, data);
                int visible = 0;
                for (int p = 0; p < pixels.Length; p += 4) if (pixels[p] > 30 || pixels[p + 1] > 30 || pixels[p + 2] > 40) visible++;
                if (draws == 0 || visible < 80) throw new InvalidOperationException($"Companion produced no visible model: {id} ({visible} pixels).");
                index++;
            }
            if (capture is not null) FrameCapture.SaveBitmap(capture, cell * columns, cell * rows);
            using (var arena = new ArenaCompanionView())
            {
                var emote = new DigiTactics.Presentation.ActiveEmote(0, "mydance", 1000);
                arena.Update("tokomon", emote, 1000, 0, "butterfly");
                arena.Update("tokomon", null, 4000, .1, "butterfly");
                if (!arena.Dancing) throw new InvalidOperationException("Equipped dance ended with the sticker instead of its beat duration.");
                arena.Update("tokomon", null, 6000, .1, "butterfly");
                if (arena.Dancing) throw new InvalidOperationException("Equipped dance did not expire.");
                arena.Update("tokomon", new(0,"mydance",7000),7000,.1,"floss");
                arena.Travel.Move(0,5);arena.Update("tokomon",null,7100,.1,"floss");
                if (arena.Dancing) throw new InvalidOperationException("Walking did not stop the equipped dance.");
            }
            Console.WriteLine($"Companion GPU smoke passed: {index} models (77 companions plus normal-mapped Vulcanusmon) rendered with farm lighting, surface textures, walking/reaction playback and procedural overlays.");
        }
        finally { Glfw.Terminate(); }
    }
}

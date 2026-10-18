using System.Buffers.Binary;
using System.IO.Compression;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using Brutal.GlfwApi;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Evidence generator, never a visual approval gate. Review status is recorded separately.</summary>
internal static class ModelVisualAudit
{
    private const int Cell = 192, Frames = 12;
    private static readonly string[] Roles = ["idle", "movement", "attack", "special", "win"];
    private sealed record Sample(string Role, int Frame, double Time, string? Clip, string Policy, string? LocomotionKind, int VisiblePixels, int EdgePixels, int Draws);
    private sealed record Evidence(int Index, string Source, string Asset, string? Motion, string Image, string TextureImage, int Cell, int Frames,
        string[] Roles, Sample[] Samples, string[] Flags, Dictionary<string, string> RuntimeHashes, string Status = "unreviewed");

    public static unsafe void Run(string directory, int start = 0, int count = int.MaxValue)
    {
        if (start < 0 || count < 1) throw new ArgumentOutOfRangeException(nameof(start));
        Directory.CreateDirectory(directory);
        var catalog = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
        var companions = CompanionCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/companion-models.json"));
        var sources = catalog.Sources.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray();
        var contexts = companions.Bases.Values.Concat(companions.Forms.Values).GroupBy(value => value.Source).ToDictionary(group => group.Key, group => group.First());
        var identities = new Dictionary<string, int>(); var representatives = new int[sources.Length]; var signatures = new string[sources.Length];
        for (int index = 0; index < sources.Length; index++)
        {
            var (key, source) = sources[index]; contexts.TryGetValue(key, out var context);
            using var identity = JsonDocument.Parse(JsonSerializer.Serialize(new { Asset = catalog.Assets[source.Asset].Sha256, Motion = source.Motion?.Sha256, source.Metadata,
                Companion = context, Locomotion = CompanionLocomotion.SourcePolicy(key), Policy = "ModelAnimator+CombatModelAnimator+CombatChoreography+CompanionAnimator/v3" }));
            string signature = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(Canonical(identity.RootElement)))).ToLowerInvariant();
            signatures[index] = signature;
            if (!identities.TryGetValue(signature, out int representative)) identities.Add(signature, representative = index);
            representatives[index] = representative;
        }
        var runtimeHashes = CaptureHashes(directory);
        File.WriteAllText(Path.Combine(directory, "identity-index.json"), JsonSerializer.Serialize(new { CatalogSha256 = runtimeHashes["models.json"], CompanionCatalogSha256 = runtimeHashes["companion-models.json"], SourceCount = sources.Length, IdentityCount = identities.Count,
            Sources = sources.Select((source, index) => new { Index = index, Source = source.Key, Signature = signatures[index], Representative = representatives[index] }).ToArray() }, new JsonSerializerOptions { WriteIndented = true }));
        if (!Glfw.Init()) throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 4); Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 5); Glfw.WindowHint(GlfwWindowHint.Visible, 0);
            const int width = Cell * Frames, height = Cell * 6;
            using var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo { Title = "Creature animation visual audit", Size = new int2(512, 512) });
            window.MakeContextCurrent();
            if (!Glad.Initialize(name => Glfw.PInvoke.GetProcAddress(name))) throw new InvalidOperationException("OpenGL initialization failed.");
            using var target = new AuditTarget(width, height);
            var eye = new Vector3(2.6f, 1.8f, 5);
            using var renderer = new ModelRenderer { CosmeticStage = true, FarmEye = eye };
            var camera = Matrix4x4.CreateLookAt(eye, new(0, .9f, 0), Vector3.UnitY) * Matrix4x4.CreatePerspectiveFieldOfView(.65f, 1, .01f, 100);
            for (int index = start; index < sources.Length && index - start < count; index++)
            {
                var (key, source) = sources[index];
                string name = index.ToString("D4");
                string imagePath = Path.Combine(directory, name + ".png"), evidencePath = Path.Combine(directory, name + ".json");
                if (File.Exists(evidencePath))
                {
                    using var existing = JsonDocument.Parse(File.ReadAllText(evidencePath));
                    if (existing.RootElement.TryGetProperty("RuntimeHashes"u8, out var recorded) && runtimeHashes.All(pair => recorded.TryGetProperty(pair.Key, out var hash) && hash.GetString() == pair.Value))
                    { Console.WriteLine($"Existing evidence {index + 1}/{sources.Length}: {key}"); continue; }
                    throw new InvalidOperationException("Existing audit evidence has different runtime/content hashes. Preserve it and select a new output directory: " + evidencePath);
                }
                int representativeIndex = representatives[index];
                string representativeFile = representativeIndex.ToString("D4") + ".json";
                if (representativeIndex != index && File.Exists(Path.Combine(directory, representativeFile)))
                {
                    using var representative = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, representativeFile)));
                    if (representative.RootElement.TryGetProperty("Image"u8, out var representativeImage))
                    {
                        File.WriteAllText(evidencePath, JsonSerializer.Serialize(new { Index = index, Source = key, Asset = source.Asset, AliasOf = sources[representativeIndex].Key,
                            RepresentativeIndex = representativeIndex, Signature = signatures[index], Image = representativeImage.GetString(), RuntimeHashes = runtimeHashes, Status = "alias-unreviewed" }, new JsonSerializerOptions { WriteIndented = true }));
                        Console.WriteLine($"Shared exact-context evidence {index + 1}/{sources.Length}: {key} -> {sources[representativeIndex].Key}"); continue;
                    }
                }
                var samples = new List<Sample>(); var flags = new List<string>();
                try
                {
                    using var model = new GpuModel(catalog, source);
                    var movement = ProductionMovement(model.Rig, key);
                    contexts.TryGetValue(key, out var visual);
                    string[] roles = visual is null ? Roles : [.. Roles, "companion-walk"];
                    OpenGl.Native.glDisable(GlCapability.ScissorTest); OpenGl.ClearColor(new float4(.025f, .04f, .065f, 1)); OpenGl.Clear(GlClearBit.Color | GlClearBit.Depth);
                    for (int row = 0; row < roles.Length; row++)
                    {
                        string role = roles[row];
                        var animator = new ModelAnimator(model.Rig);
                        CompanionAnimator? companion = role == "companion-walk" ? model.CreateCompanionAnimator(visual!, companions.Fallbacks, height: 2) : null;
                        RigAnimation? clip = role == "idle" ? ModelAnimator.FindIdle(model.Rig)
                            : role == "movement" ? model.Rig.Animations.FirstOrDefault(c => c.Name is "run" or "walk" or "move") ?? ModelAnimator.FindIdle(model.Rig)
                            : role == "companion-walk" ? model.Rig.Animations.FirstOrDefault(c => c.Name is "run" or "walk" or "move")
                            : model.Rig.Animations.FirstOrDefault(c => c.Name == role);
                        string policy = role == "movement" ? "Production CombatChoreography + CombatModelAnimator / " + movement[0].LocomotionKind
                            : role == "companion-walk" ? "Production CompanionAnimator.SetMoving"
                            : role == "idle" ? "Production ModelAnimator idle" : "Production ModelAnimator.Play";
                        if (role is "attack" or "special" or "win" && !animator.Play(role)) flags.Add(role + ": missing authored role; rendered production idle fallback");
                        if (role == "companion-walk")
                        {
                            companion!.SetMoving(true);
                            policy += " / " + companion.LocomotionKind;
                        }
                        if (clip is null && role != "companion-walk") flags.Add(role + ": no clip");
                        double span = role switch
                        {
                            "movement" => movement[^1].Time,
                            "attack" => Math.Min(clip?.Duration ?? .55, .55) + .24,
                            "special" => Math.Min(clip?.Duration ?? 1.1, 1.1) + .24,
                            _ => Math.Max(.5, clip?.Duration ?? 1) * 1.1
                        };
                        double previous = 0;
                        for (int frame = 0; frame < Frames; frame++)
                        {
                            double time = span * frame / (Frames - 1);
                            while (previous + 1e-10 < time)
                            {
                                double step = Math.Min(1d / 60, time - previous); animator.Update(step); companion?.Update(step); previous += step;
                            }
                            animator.Update(0); companion?.Update(0);
                            RigPose pose = role == "movement" ? movement[frame].Pose : companion?.Pose ?? animator.Pose;
                            Matrix4D placement = companion is null ? model.PreviewPlacement : model.PlaceCompanion(companion);
                            if (role == "movement")
                            {
                                var sample = movement[frame];
                                placement *= GpuModel.Transform(sample.Position.X, sample.Position.Y, sample.Position.Z, 0, sample.Scale);
                            }
                            if (pose.World.Any(matrix => !matrix.IsFinite)) flags.Add($"{role}/{frame}: nonfinite pose");
                            int left = frame * Cell, bottom = (roles.Length - 1 - row) * Cell;
                            ModelRenderer.Begin(Cell, Cell); OpenGl.Viewport(left, bottom, Cell, Cell);
                            renderer.Draw(model, pose, placement, camera, false); int draws = renderer.DrawCalls;
                            renderer.BeginTransparency(); renderer.Draw(model, pose, placement, camera, true); draws += renderer.DrawCalls;
                            renderer.FlushTransparency(); ModelRenderer.End();
                            var pixels = new byte[Cell * Cell * 4];
                            fixed (byte* data = pixels) OpenGl.ReadPixels(left, bottom, Cell, Cell, OpenGl.GL_RGBA, OpenGl.GL_UNSIGNED_BYTE, data);
                            int visible = 0, edge = 0;
                            for (int y = 0; y < Cell; y++) for (int x = 0; x < Cell; x++)
                            {
                                int p = (y * Cell + x) * 4;
                                if (pixels[p] <= 22 && pixels[p + 1] <= 27 && pixels[p + 2] <= 34) continue;
                                visible++; if (x < 2 || y < 2 || x >= Cell - 2 || y >= Cell - 2) edge++;
                            }
                            samples.Add(new(role, frame, time, clip?.Name, policy, role == "movement" ? movement[frame].LocomotionKind : companion?.LocomotionKind.ToString(), visible, edge, draws));
                            if (draws == 0 || visible < 40) flags.Add($"{role}/{frame}: empty or tiny surface ({visible} pixels, {draws} draws)");
                            if (edge > 5) flags.Add($"{role}/{frame}: {edge} frame-edge pixels; inspect clipping or root motion");
                        }
                    }
                    SavePng(imagePath, width, roles.Length * Cell);
                    TextureViews(model, renderer, Path.Combine(directory, name + ".texture.png")); renderer.FarmEye = eye;
                    File.WriteAllText(evidencePath, JsonSerializer.Serialize(new Evidence(index, key, source.Asset, source.Motion?.Sha256, name + ".png", name + ".texture.png", Cell, Frames, roles, samples.ToArray(), flags.ToArray(), runtimeHashes), new JsonSerializerOptions { WriteIndented = true }));
                    Console.WriteLine($"Rendered evidence {index + 1}/{sources.Length}: {key}; {flags.Count} flags; awaiting visual review.");
                }
                catch (Exception error) when (error is InvalidDataException or IOException or InvalidOperationException or ArgumentException)
                {
                    File.WriteAllText(evidencePath, JsonSerializer.Serialize(new { Index = index, Source = key, Asset = source.Asset, Status = "render-error", Error = error.ToString() }, new JsonSerializerOptions { WriteIndented = true }));
                    Console.WriteLine($"Render error {index + 1}/{sources.Length}: {key}: {error.Message}");
                }
            }
        }
        finally { Glfw.Terminate(); }
    }

    private static string Canonical(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Object => "{" + string.Join(",", value.EnumerateObject().OrderBy(property => property.Name, StringComparer.Ordinal).Select(property => JsonSerializer.Serialize(property.Name) + ":" + Canonical(property.Value))) + "}",
        JsonValueKind.Array => "[" + string.Join(",", value.EnumerateArray().Select(Canonical)) + "]",
        _ => value.GetRawText()
    };

    private static Dictionary<string, string> CaptureHashes(string directory)
    {
        var hashes = new Dictionary<string, string>();
        foreach (var assembly in new[] { typeof(ModelRenderer).Assembly, typeof(ModelAnimator).Assembly, typeof(CombatChoreography).Assembly, typeof(CombatSimulation).Assembly })
            hashes[Path.GetFileName(assembly.Location)] = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))).ToLowerInvariant();
        foreach (string name in new[] { "models.json", "companion-models.json" })
            hashes[name] = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "content", name)))).ToLowerInvariant();
        string manifestPath = Path.Combine(directory, "capture-manifest.json");
        if (File.Exists(manifestPath))
        {
            using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
            foreach (var item in manifest.RootElement.GetProperty("runtimeHashes"u8).EnumerateObject())
                if (!hashes.TryGetValue(item.Name, out var actual) || item.Value.GetString() != actual)
                    throw new InvalidOperationException("Frozen audit runtime differs: " + item.Name);
            foreach (var item in manifest.RootElement.GetProperty("frozenCatalogs"u8).EnumerateObject())
                if (!hashes.TryGetValue(item.Name, out var actual) || item.Value.GetProperty("sha256"u8).GetString() != actual)
                    throw new InvalidOperationException("Frozen audit catalog differs: " + item.Name);
        }
        return hashes;
    }

    private static unsafe void TextureViews(GpuModel model, ModelRenderer renderer, string path)
    {
        var animator = new ModelAnimator(model.Rig); animator.Update(.17);
        OpenGl.Native.glDisable(GlCapability.ScissorTest); OpenGl.ClearColor(new float4(.025f, .04f, .065f, 1)); OpenGl.Clear(GlClearBit.Color | GlClearBit.Depth);
        for (int side = 0; side < 2; side++)
        {
            var (camera, eye) = FittedCamera(model, animator.Pose, side == 1);
            renderer.FarmEye = eye; ModelRenderer.Begin(512, 512); OpenGl.Viewport(side * 512, 0, 512, 512);
            renderer.Draw(model, animator.Pose, model.PreviewPlacement, camera, false);
            renderer.BeginTransparency(); renderer.Draw(model, animator.Pose, model.PreviewPlacement, camera, true); renderer.FlushTransparency(); ModelRenderer.End();
        }
        SavePng(path, 1024, 512);
    }

    private static (Matrix4x4 Camera, Vector3 Eye) FittedCamera(GpuModel model, RigPose pose, bool rear = false)
    {
        var (min, max) = model.Mesh.Bounds(model.Rig, pose);
        Matrix4D placement = model.PreviewPlacement;
        var corners = new Vector3[8];
        for (int corner = 0; corner < 8; corner++)
        {
            double x = (corner & 1) == 0 ? min.X : max.X, y = (corner & 2) == 0 ? min.Y : max.Y, z = (corner & 4) == 0 ? min.Z : max.Z;
            corners[corner] = new((float)(x * placement.M11 + y * placement.M21 + z * placement.M31 + placement.M41),
                (float)(x * placement.M12 + y * placement.M22 + z * placement.M32 + placement.M42), (float)(x * placement.M13 + y * placement.M23 + z * placement.M33 + placement.M43));
        }
        var center = corners.Aggregate(Vector3.Zero, (total, corner) => total + corner) / 8;
        var eye = center + Vector3.Normalize(rear ? new Vector3(-2.6f, 1.8f, -5) : new Vector3(2.6f, 1.8f, 5)) * 10;
        var view = Matrix4x4.CreateLookAt(eye, center, Vector3.UnitY);
        var projected = corners.Select(corner => Vector3.Transform(corner, view)).ToArray();
        float span = Math.Max(.01f, Math.Max(projected.Max(corner => corner.X) - projected.Min(corner => corner.X), projected.Max(corner => corner.Y) - projected.Min(corner => corner.Y))) * 1.16f;
        return (view * Matrix4x4.CreateOrthographic(span, span, .01f, 100), eye);
    }

    public static unsafe void RunFitted(string directory, int index, string role)
    {
        if (role is not ("idle" or "attack" or "special" or "win")) throw new ArgumentException("Choose idle, attack, special or win.");
        Directory.CreateDirectory(directory);
        var runtimeHashes = CaptureHashes(directory);
        var catalog = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
        var (key, source) = catalog.Sources.OrderBy(pair => pair.Key, StringComparer.Ordinal).ElementAt(index);
        if (!Glfw.Init()) throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 4); Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 5); Glfw.WindowHint(GlfwWindowHint.Visible, 0);
            using var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo { Title = "Fitted model motion audit", Size = new int2(512, 512) }); window.MakeContextCurrent();
            if (!Glad.Initialize(name => Glfw.PInvoke.GetProcAddress(name))) throw new InvalidOperationException("OpenGL initialization failed.");
            using var target = new AuditTarget(1536, 2048);
            using var model = new GpuModel(catalog, source); using var renderer = new ModelRenderer { CosmeticStage = true };
            var idle = new ModelAnimator(model.Rig); idle.Update(.17);
            var (camera, eye) = FittedCamera(model, idle.Pose); renderer.FarmEye = eye;
            var animator = new ModelAnimator(model.Rig); bool available = role == "idle" || animator.Play(role);
            var clip = role == "idle" ? ModelAnimator.FindIdle(model.Rig) : model.Rig.Animations.FirstOrDefault(value => value.Name == role);
            double span = role == "attack" ? Math.Min(clip?.Duration ?? .55, .55) + .24 : role == "special" ? Math.Min(clip?.Duration ?? 1.1, 1.1) + .24 : Math.Max(.5, clip?.Duration ?? 1) * 1.1;
            double previous = 0; var times = new double[Frames];
            OpenGl.Native.glDisable(GlCapability.ScissorTest); OpenGl.ClearColor(new float4(.025f, .04f, .065f, 1)); OpenGl.Clear(GlClearBit.Color | GlClearBit.Depth);
            for (int frame = 0; frame < Frames; frame++)
            {
                double time = times[frame] = span * frame / (Frames - 1);
                while (previous + 1e-10 < time) { double step = Math.Min(1d / 60, time - previous); animator.Update(step); previous += step; }
                animator.Update(0);
                ModelRenderer.Begin(512, 512); OpenGl.Viewport(frame % 3 * 512, (3 - frame / 3) * 512, 512, 512);
                renderer.Draw(model, animator.Pose, model.PreviewPlacement, camera, false);
                renderer.BeginTransparency(); renderer.Draw(model, animator.Pose, model.PreviewPlacement, camera, true); renderer.FlushTransparency(); ModelRenderer.End();
            }
            string image = $"{index:D4}.fitted-{role}.png";
            SavePng(Path.Combine(directory, image), 1536, 2048);
            File.WriteAllText(Path.Combine(directory, $"{index:D4}.fitted-{role}.json"), JsonSerializer.Serialize(new { Source = key, Image = image, Role = role, Times = times, Clip = clip?.Name, Available = available,
                Policy = "Production ModelAnimator; fixed camera fitted to idle bounds, no per-frame reframing", RuntimeHashes = runtimeHashes, Status = "unreviewed" }, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Fitted temporal evidence: {key}/{role}, {Frames} samples at 512 pixels.");
        }
        finally { Glfw.Terminate(); }
    }

    public static unsafe void RunBoard(string directory, int index, string role, double peakTime, int width = 908, int height = 394, int cellX = 3, int cellY = 4)
    {
        if (role is not ("attack" or "special" or "win" or "movement") || peakTime < 0) throw new ArgumentException("Choose attack, special, win or movement and a nonnegative peak time.");
        if (width < 300 || height < 120 || cellX is < 0 or > 6 || cellY is < 0 or > 7) throw new ArgumentOutOfRangeException(nameof(width));
        Directory.CreateDirectory(directory);
        var runtimeHashes = CaptureHashes(directory);
        var catalog = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
        var (key, source) = catalog.Sources.OrderBy(pair => pair.Key, StringComparer.Ordinal).ElementAt(index);
        var content = ContentCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/catalog.json"));
        var alias = catalog.Variants.SelectMany(variant => variant.Value.SelectMany(line => line.Value.Select((value, stage) => new { Variant = variant.Key, Line = line.Key, Stage = stage, Source = value }))).FirstOrDefault(value => value.Source == key);
        int stage = alias?.Stage ?? 2;
        int tier = alias is null ? 1 : content.Variants.First(rules => rules.Id == alias.Variant).Lines.Concat(content.Variants.First(rules => rules.Id == alias.Variant).ExtraLines).First(line => line.Id == alias.Line).Tier;
        if (!Glfw.Init()) throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 4); Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 5); Glfw.WindowHint(GlfwWindowHint.Visible, 0);
            using var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo { Title = "Model board-context audit", Size = new int2(512, 512) }); window.MakeContextCurrent();
            if (!Glad.Initialize(name => Glfw.PInvoke.GetProcAddress(name))) throw new InvalidOperationException("OpenGL initialization failed.");
            using var target = new AuditTarget(width * 3, height);
            using var model = new GpuModel(catalog, source); using var renderer = new ModelRenderer(); using var board = new ArenaEnvironment();
            board.SetTheme("file-island");
            var clip = model.Rig.Animations.FirstOrDefault(clip => clip.Name == role);
            double duration = clip?.Duration ?? 1;
            double finish = role == "attack" ? Math.Min(duration, .55) : role == "special" ? Math.Min(duration, 1.1) : duration;
            var movement = role == "movement" ? ProductionMovement(model.Rig, key, throughArrival: true) : null;
            int[] movementFrames = movement is null ? [] : [0, movement.Length / 2, movement.Length - 1];
            double[] times = movement is null ? [0, peakTime, Math.Max(peakTime, finish) + .25] : movementFrames.Select(frame => movement[frame].Time).ToArray();
            var camera = board.Camera.Matrix(width, height, 7); var eye = board.Camera.Eye(width, height, 7);
            var location = CombatLayout.Cell(new(cellX, cellY), 7, false);
            var view = new CombatVisual(0, location, Math.PI, default, 1, 0, 0, 1, null, false);
            for (int frame = 0; frame < times.Length; frame++)
            {
                var animator = new ModelAnimator(model.Rig); animator.Play(role); double time = 0;
                while (time + 1e-10 < times[frame]) { double delta = Math.Min(1d / 60, times[frame] - time); animator.Update(delta); time += delta; }
                animator.Update(0); board.Time = time;
                RigPose pose = animator.Pose;
                CombatVisual frameView = view;
                if (movement is not null)
                {
                    var sample = movement[movementFrames[frame]]; pose = sample.Pose;
                    frameView = view with { Position = new(location.X + sample.Position.X, sample.Position.Y, location.Z + sample.Position.Z), Scale = sample.Scale };
                }
                board.Draw(renderer, camera, eye, 7, false, frame * width, 0, width, height,
                    (renderCamera, transparent) => renderer.Draw(model, pose, model.Place(frameView, stage, tier), renderCamera, transparent));
            }
            string stem = $"{index:D4}.board-{role}-{width}x{height}-cell{cellX}-{cellY}";
            string image = stem + ".png";
            SavePng(Path.Combine(directory, image), width * 3, height);
            File.WriteAllText(Path.Combine(directory, stem + ".json"), JsonSerializer.Serialize(new { Source = key, Image = image, Role = role, Times = times, Stage = stage, Tier = tier, Variant = alias?.Variant, Line = alias?.Line,
                BoardContextPolicy = "live-arena-camera/v1", Viewport = new { Width = width, Height = height }, Cell = new { X = cellX, Y = cellY },
                SamplePhases = movement is null ? new[] { "start", "requested-action-time", "settled" } : new[] { "first-moving-tick", "travel", "first-arrival-tick" },
                Policy = "ArenaEnvironment.Camera default + GpuModel.Place + production animation; movement uses actual CombatChoreography", RuntimeHashes = runtimeHashes, Status = "unreviewed" }, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Board context evidence: {key}/{role}, {string.Join(", ", times.Select(time => time.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)))} seconds.");
        }
        finally { Glfw.Terminate(); }
    }

    private sealed class AuditTarget : IDisposable
    {
        private readonly GlFramebuffer frame;
        private readonly GlTexture color, depth;
        public AuditTarget(int width, int height)
        {
            Span<GlFramebuffer> frames = stackalloc GlFramebuffer[1]; OpenGl.CreateFramebuffers(frames); frame = frames[0];
            Span<GlTexture> textures = stackalloc GlTexture[2]; OpenGl.CreateTextures(OpenGl.GL_TEXTURE_2D, textures); color = textures[0]; depth = textures[1];
            OpenGl.TextureStorage2D(color.Id, 1, OpenGl.GL_RGBA8, width, height); OpenGl.TextureStorage2D(depth.Id, 1, OpenGl.GL_DEPTH_COMPONENT24, width, height);
            OpenGl.NamedFramebufferTexture(frame, OpenGl.GL_COLOR_ATTACHMENT0, color.Id, 0); OpenGl.NamedFramebufferTexture(frame, OpenGl.GL_DEPTH_ATTACHMENT, depth.Id, 0);
            OpenGl.NamedFramebufferDrawBuffer(frame, OpenGl.GL_COLOR_ATTACHMENT0);
            if (OpenGl.CheckNamedFramebufferStatus(frame, OpenGl.GL_FRAMEBUFFER) != OpenGl.GL_FRAMEBUFFER_COMPLETE) throw new InvalidOperationException("Audit framebuffer incomplete.");
            OpenGl.BindFramebuffer(OpenGl.GL_FRAMEBUFFER, frame);
        }
        public void Dispose() { OpenGl.BindFramebuffer(OpenGl.GL_FRAMEBUFFER, default); OpenGl.DeleteTextures(new[] { color, depth }); OpenGl.DeleteFramebuffers(new[] { frame }); }
    }

    // Sample actual simulation movement, including its production bob and end position.
    private sealed record Movement(double Time, WorldPosition Position, double Scale, RigPose Pose, string LocomotionKind);
    private sealed class MovementModels(GltfRig rig, string source) : ICombatModelDriver
    {
        private readonly Dictionary<int, (CombatUnit Unit, CombatModelAnimator Animator)> actors = [];
        public void Create(CombatUnit unit) => actors[unit.Id] = (unit, new(rig, source));
        public void Remove(int unit) => actors.Remove(unit);
        public bool Play(int unit, string role) => actors.TryGetValue(unit, out var actor) && actor.Animator.Play(role);
        public void Update(int unit, double delta)
        {
            var actor = actors[unit];
            actor.Animator.Update(delta, actor.Unit.Alive && actor.Unit.MoveElapsed < actor.Unit.MoveDuration && actor.Unit.MoveDuration >= .3 && actor.Unit.MoveOrigin != actor.Unit.Cell);
        }
        public double MovementHeight(int unit, double sourceHeight)
        {
            var actor = actors[unit];
            return actor.Unit.MoveDuration >= .3 && !actor.Animator.UsesTravelHop ? actor.Animator.GlideOffset : sourceHeight;
        }
        public (RigPose Pose, string Kind) Snapshot(int unit)
        {
            var actor = actors[unit]; var pose = new RigPose(rig);
            actor.Animator.Pose.Local.CopyTo(pose.Local, 0); pose.UpdateWorld();
            return (pose, actor.Animator.LocomotionKind.ToString());
        }
    }
    private static Movement[] ProductionMovement(GltfRig rig, string source, bool throughArrival = false)
    {
        var rules = ContentCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/catalog.json")).Variants.First();
        var line = rules.Lines.First(line => !line.Combat.Leap);
        var combat = new CombatSimulation(rules, [new(line.Id, 0, 0, 7)], [new(line.Id, 0, 0, 7)], 711);
        var models = new MovementModels(rig, source);
        var choreography = new CombatChoreography(combat, models);
        var collected = new List<Movement>(); WorldPosition origin = default; double started = 0;
        bool arrived = false;
        for (int tick = 0; tick < 300 && (throughArrival || collected.Count < Frames); tick++)
        {
            combat.Step(); choreography.Update(CombatSimulation.Tick);
            var unit = combat.Units[0];
            if (collected.Count == 0 && unit.MoveElapsed >= unit.MoveDuration) continue;
            var view = choreography.Views.First(view => view.UnitId == unit.Id);
            if (collected.Count == 0) { origin = view.Position; started = combat.Time; }
            var snapshot = models.Snapshot(unit.Id);
            collected.Add(new(combat.Time - started, new(view.Position.X - origin.X, view.Position.Y, view.Position.Z - origin.Z), view.Scale, snapshot.Pose, snapshot.Kind));
            if (throughArrival && unit.MoveElapsed >= unit.MoveDuration) { arrived = true; break; }
        }
        if (throughArrival ? !arrived || collected.Count < 3 : collected.Count != Frames) throw new InvalidOperationException("Could not collect a production movement path.");
        // Center the complete path without changing travel distance or vertical bob.
        double centerX = (collected.Min(sample => sample.Position.X) + collected.Max(sample => sample.Position.X)) / 2;
        double centerZ = (collected.Min(sample => sample.Position.Z) + collected.Max(sample => sample.Position.Z)) / 2;
        return collected.Select(sample => sample with { Position = sample.Position with { X = sample.Position.X - centerX, Z = sample.Position.Z - centerZ } }).ToArray();
    }

    private static unsafe void SavePng(string path, int width, int height)
    {
        byte[] rgba = new byte[width * height * 4];
        fixed (byte* data = rgba) OpenGl.ReadPixels(0, 0, width, height, OpenGl.GL_RGBA, OpenGl.GL_UNSIGNED_BYTE, data);
        using var output = File.Create(path); output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        byte[] header = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(header, width); BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height); header[8] = 8; header[9] = 6;
        Chunk(output, "IHDR"u8, header);
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, true))
        {
            byte[] row = new byte[width * 4 + 1]; row[0] = 1;
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width * 4; x++) row[x + 1] = unchecked((byte)(rgba[y * width * 4 + x] - (x >= 4 ? rgba[y * width * 4 + x - 4] : 0)));
                zlib.Write(row);
            }
        }
        Chunk(output, "IDAT"u8, compressed.ToArray()); Chunk(output, "IEND"u8, []);
    }
    private static void Chunk(Stream stream, ReadOnlySpan<byte> name, ReadOnlySpan<byte> payload)
    {
        Span<byte> value = stackalloc byte[4]; BinaryPrimitives.WriteInt32BigEndian(value, payload.Length); stream.Write(value); stream.Write(name); stream.Write(payload);
        uint crc = uint.MaxValue;
        foreach (byte b in name) { crc ^= b; for (int bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xedb88320u : 0); }
        foreach (byte b in payload) { crc ^= b; for (int bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xedb88320u : 0); }
        BinaryPrimitives.WriteUInt32BigEndian(value, ~crc); stream.Write(value);
    }
}

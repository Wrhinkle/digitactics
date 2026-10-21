using System.Buffers.Binary;
using System.IO.Compression;
using System.Numerics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Brutal.GlfwApi;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>
/// Diagnostic model sweep for the model/animation workstream. For every model source it writes one strip:
/// shop portrait | fitted idle | attack mid | special mid | win mid, and below it the live-board camera at idle and at attack mid.
/// It also runs automatic pose checks (bind-pose/T-pose, motionless idle, action scale jumps) and writes sweep.json.
/// Evidence only; it never changes content or approves a model.
/// Launch: DigiTactics.Desktop.exe --model-sweep &lt;dir&gt; [--sweep-start N] [--sweep-count N] [--sweep-filter text] [--sweep-times attack=0.3,special=0.5,win=0.4]
/// The action cells default to the middle of each clip's played span; --sweep-times sets explicit seconds.
/// </summary>
internal static partial class ModelSweep
{
    // Board cells: the live camera rendered at 1600x695 (default aspect), cropped to 640x278 around the unit (2.5x zoom).
    private const int Cell = 256, BoardWidth = 640, BoardHeight = 278, Width = Cell * 5, Height = Cell + BoardHeight, LiveWidth = 1600, LiveHeight = 695, TargetWidth = LiveWidth * 2, TargetHeight = Cell + LiveHeight;
    private static readonly string[] Labels = ["portrait", "idle", "attack-mid", "special-mid", "win-mid", "board-idle", "board-attack-mid"];

    private sealed record PoseCheck(double BindDeviation, double ArmBindDeviation, int ArmsFound, int ArmsHorizontal, double ArmMaxSlope);
    private sealed record Result(int Index, string Source, string Asset, string? Line, string? Variant, int Stage, int Tier, string Image, bool Portrait,
        string? IdleClip, bool Attack, bool Special, bool Win, double IdleMotion, double IdleBindDeviation, int ArmsFound, int ArmsHorizontal, double ArmMaxSlope,
        double AttackHeightRatio, double SpecialHeightRatio, double WinHeightRatio, string[] Flags, string? Error);

    [GeneratedRegex(@"(upper_?arm|uparm|(^|[^a-z])arm([^a-z]|$)|ude(?!2)|shoulder|clavicle|kata(?!mari))", RegexOptions.IgnoreCase)]
    private static partial Regex ArmName();
    [GeneratedRegex(@"(fore|lower|low|hand|elbow|finger|weapon|wing|hiji|te_|wrist|twist|roll|_end|nub)", RegexOptions.IgnoreCase)]
    private static partial Regex NotUpperArm();

    public static unsafe void Run(string directory, int start, int count, string? filter, IReadOnlyDictionary<string, double>? times = null)
    {
        Directory.CreateDirectory(directory);
        var catalog = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
        var content = ContentCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/catalog.json"));
        string portraitFolder = Path.Combine(AppContext.BaseDirectory, "content/portraits");
        var portraits = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(portraitFolder, "manifest.json")))!;
        var sources = catalog.Sources.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToArray();
        var aliases = catalog.Variants.SelectMany(variant => variant.Value.SelectMany(line => line.Value.Select((value, stage) => (Variant: variant.Key, Line: line.Key, Stage: stage, Source: value))))
            .Where(value => value.Source is not null).GroupBy(value => value.Source!).ToDictionary(group => group.Key, group => group.First());
        var results = new List<Result>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        if (!Glfw.Init()) throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 4); Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 5); Glfw.WindowHint(GlfwWindowHint.Visible, 0);
            using var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo { Title = "Model sweep", Size = new int2(512, 512) }); window.MakeContextCurrent();
            if (!Glad.Initialize(name => Glfw.PInvoke.GetProcAddress(name))) throw new InvalidOperationException("OpenGL initialization failed.");
            using var target = new Target(TargetWidth, TargetHeight);
            using var fitted = new ModelRenderer { CosmeticStage = true };
            using var boardRenderer = new ModelRenderer();
            using var board = new ArenaEnvironment(); board.SetTheme("file-island");
            var boardCamera = board.Camera.Matrix(LiveWidth, LiveHeight, 7); var boardEye = board.Camera.Eye(LiveWidth, LiveHeight, 7);
            var location = CombatLayout.Cell(new(3, 4), 7, false);
            var view = new CombatVisual(0, location, Math.PI, default, 1, 0, 0, 1, null, false);
            var clip4 = Vector4.Transform(new Vector4((float)location.X, (float)location.Y + .8f, (float)location.Z, 1), boardCamera);
            int cropX = Math.Clamp((int)((clip4.X / clip4.W * .5f + .5f) * LiveWidth) - BoardWidth / 2, 0, LiveWidth - BoardWidth);
            int cropY = Math.Clamp((int)((.5f - clip4.Y / clip4.W * .5f) * LiveHeight) - BoardHeight / 2, 0, LiveHeight - BoardHeight);
            for (int index = start; index < sources.Length && index - start < count; index++)
            {
                var (key, source) = sources[index];
                if (filter is not null && (catalog.Sources.ContainsKey(filter) ? key != filter : !key.Contains(filter, StringComparison.OrdinalIgnoreCase))) continue;
                string image = index.ToString("D4") + ".png";
                aliases.TryGetValue(key, out var alias);
                int stage = alias.Source is null ? 2 : alias.Stage;
                int tier = 1;
                if (alias.Source is not null && content.Variants.FirstOrDefault(rules => rules.Id == alias.Variant) is { } rules
                    && rules.Lines.Concat(rules.ExtraLines).FirstOrDefault(line => line.Id == alias.Line) is { } unitLine) tier = unitLine.Tier;
                var flags = new List<string>();
                try
                {
                    using var model = new GpuModel(catalog, source);
                    var rig = model.Rig;
                    var idleClip = ModelAnimator.FindIdle(rig);
                    // Idle samples through the production combat animator (board idle path).
                    var idle = new CombatModelAnimator(rig, key);
                    var idleSamples = new List<RigPose>();
                    double clock = 0;
                    foreach (double time in new[] { .3, .8, 1.3, 1.9 })
                    {
                        while (clock + 1e-9 < time) { double step = Math.Min(1d / 60, time - clock); idle.Update(step, false); clock += step; }
                        idleSamples.Add(Copy(rig, idle.Pose));
                    }
                    var idlePose = idleSamples[1];
                    double idleHeight = MeshHeight(model, idlePose);
                    var checks = idleSamples.Select(pose => Check(model, pose)).ToArray();
                    double motion = 0;
                    for (int i = 1; i < idleSamples.Count; i++) motion = Math.Max(motion, Motion(model, idleSamples[0], idleSamples[i]));
                    // Action mid-frames through the same animator that boards use.
                    (RigPose Pose, bool Played, double Ratio) Action(string role)
                    {
                        var animator = new CombatModelAnimator(rig, key); bool played = animator.Play(role);
                        var clip = rig.Animations.FirstOrDefault(value => value.Name == role);
                        double finish = role == "attack" ? Math.Min(clip?.Duration ?? .55, .55) : role == "special" ? Math.Min(clip?.Duration ?? 1.1, 1.1) : Math.Max(.5, clip?.Duration ?? 1);
                        double mid = times is not null && times.TryGetValue(role, out double requested) ? requested : finish / 2, t = 0;
                        while (t + 1e-9 < mid) { double step = Math.Min(1d / 60, mid - t); animator.Update(step, false); t += step; }
                        var pose = Copy(rig, animator.Pose);
                        return (pose, played, idleHeight > 1e-9 ? MeshHeight(model, pose) / idleHeight : 1);
                    }
                    var attack = Action("attack"); var special = Action("special"); var win = Action("win");

                    // T-pose: both upper-arm chains horizontal in every idle sample (what the board shows at rest).
                    bool bindStatic = checks.All(check => check.BindDeviation < .003) && motion < .01;
                    bool motionless = motion < .002;
                    bool armsHorizontal = checks.All(check => check.ArmsFound >= 2 && check.ArmsHorizontal >= 2);
                    if (idleClip is null) flags.Add("no-idle-clip");
                    if (armsHorizontal) flags.Add("t-pose");
                    if (bindStatic) flags.Add("bind-pose-idle");
                    if (motionless) flags.Add("motionless-idle");
                    foreach (var (role, action) in new[] { ("attack", attack), ("special", special), ("win", win) })
                    {
                        if (action.Ratio > 1.6 || action.Ratio < .5) flags.Add($"{role}-height-x{action.Ratio:0.00}");
                        var roleCheck = Check(model, action.Pose);
                        if (!armsHorizontal && roleCheck.ArmsFound >= 2 && roleCheck.ArmsHorizontal >= 2 && roleCheck.ArmBindDeviation < .03) flags.Add($"{role}-t-pose");
                    }
                    if (!attack.Played) flags.Add("no-attack-role");
                    if (!special.Played) flags.Add("no-special-role");

                    // Render the strip.
                    OpenGl.Native.glDisable(GlCapability.ScissorTest); OpenGl.ClearColor(new float4(.025f, .04f, .065f, 1)); OpenGl.Clear(GlClearBit.Color | GlClearBit.Depth);
                    var (camera, eye) = FittedCamera(model, idlePose); fitted.FarmEye = eye;
                    RigPose[] cells = [idlePose, attack.Pose, special.Pose, win.Pose];
                    for (int i = 0; i < cells.Length; i++)
                    {
                        ModelRenderer.Begin(Cell, Cell); OpenGl.Viewport((i + 1) * Cell, LiveHeight, Cell, Cell);
                        fitted.Draw(model, cells[i], model.PreviewPlacement, camera, false);
                        fitted.BeginTransparency(); fitted.Draw(model, cells[i], model.PreviewPlacement, camera, true); fitted.FlushTransparency(); ModelRenderer.End();
                    }
                    RigPose[] boardPoses = [idlePose, attack.Pose];
                    for (int i = 0; i < 2; i++)
                    {
                        var pose = boardPoses[i]; board.Time = i == 0 ? .8 : .3;
                        board.Draw(boardRenderer, boardCamera, boardEye, 7, false, i * LiveWidth, 0, LiveWidth, LiveHeight,
                            (renderCamera, transparent) => boardRenderer.Draw(model, pose, model.Place(view, stage, tier), renderCamera, transparent));
                    }
                    byte[] full = ReadTopDown(TargetWidth, TargetHeight), rgba = new byte[Width * Height * 4];
                    for (int y = 0; y < Cell; y++) Buffer.BlockCopy(full, y * TargetWidth * 4, rgba, y * Width * 4, Width * 4);
                    for (int i = 0; i < 2; i++) for (int y = 0; y < BoardHeight; y++)
                        Buffer.BlockCopy(full, ((Cell + cropY + y) * TargetWidth + i * LiveWidth + cropX) * 4, rgba, ((Cell + y) * Width + i * BoardWidth) * 4, BoardWidth * 4);
                    bool hasPortrait = portraits.TryGetValue(key, out var portraitFile);
                    if (hasPortrait) Blit(rgba, PngImage.Decode(File.ReadAllBytes(Path.Combine(portraitFolder, portraitFile!))), 0, 0, Cell);
                    else flags.Add("no-portrait");
                    WritePng(Path.Combine(directory, image), Width, Height, rgba);
                    var mid = checks[1];
                    results.Add(new(index, key, source.Asset, alias.Line, alias.Variant, stage, tier, image, hasPortrait, idleClip?.Name, attack.Played, special.Played, win.Played,
                        Math.Round(motion, 5), Math.Round(checks.Max(check => check.BindDeviation), 5), mid.ArmsFound, mid.ArmsHorizontal, Math.Round(mid.ArmMaxSlope, 3),
                        Math.Round(attack.Ratio, 3), Math.Round(special.Ratio, 3), Math.Round(win.Ratio, 3), flags.ToArray(), null));
                    Console.WriteLine($"{index:D4} {key}: {(flags.Count == 0 ? "ok" : string.Join(", ", flags))}");
                }
                catch (Exception error) when (error is InvalidDataException or IOException or InvalidOperationException or ArgumentException or KeyNotFoundException)
                {
                    results.Add(new(index, key, source.Asset, alias.Line, alias.Variant, stage, tier, image, false, null, false, false, false, 0, 0, 0, 0, 0, 0, 0, 0, ["render-error"], error.Message));
                    Console.WriteLine($"{index:D4} {key}: render error: {error.Message}");
                }
            }
        }
        finally { Glfw.Terminate(); }
        var flagged = results.Where(result => result.Flags.Any(flag => flag is "t-pose" or "bind-pose-idle" || flag.EndsWith("-t-pose", StringComparison.Ordinal))).ToArray();
        var summary = new
        {
            Generated = DateTimeOffset.Now, Seconds = Math.Round(stopwatch.Elapsed.TotalSeconds, 1), Layout = Labels,
            Definitions = new Dictionary<string, string>
            {
                ["t-pose"] = "both upper-arm chains (found by bone name: arm/upperarm/ude/shoulder/clavicle/kata) are within ~17 degrees of horizontal in every idle sample (0.3/0.8/1.3/1.9 s, CombatModelAnimator)",
                ["<role>-t-pose"] = "idle is fine but the attack/special/win mid-frame has both arms horizontal and still at their skin bind pose (< 3% of skeleton extent)",
                ["bind-pose-idle"] = "every idle sample equals the skin bind pose up to one rigid transform (< 0.3% of skeleton extent) and barely moves: the authored mesh pose is shown unanimated",
                ["motionless-idle"] = "idle samples differ by < 0.2% of skeleton extent",
                ["<role>-height-xR"] = "mid-action skinned mesh height / idle height outside 0.5..1.6 (e.g. a model growing huge on attack)",
            },
            Count = results.Count,
            TPose = results.Count(result => result.Flags.Contains("t-pose")),
            BindPoseIdle = results.Count(result => result.Flags.Contains("bind-pose-idle")),
            Flagged = flagged.Length,
            ActionTPose = results.Count(result => result.Flags.Any(flag => flag.EndsWith("-t-pose", StringComparison.Ordinal) && flag != "t-pose")),
            MotionlessIdle = results.Count(result => result.Flags.Contains("motionless-idle")),
            HeightJumps = results.Count(result => result.Flags.Any(flag => flag.Contains("-height-x"))),
            RenderErrors = results.Count(result => result.Error is not null),
            Results = results,
        };
        string reportName = start == 0 && count == int.MaxValue && filter is null ? "sweep.json" : $"sweep-{start:D4}-{(filter ?? "all").Replace(':', '_')}.json";
        File.WriteAllText(Path.Combine(directory, reportName), JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Model sweep: {results.Count} sources in {stopwatch.Elapsed.TotalSeconds:0}s; t-pose {summary.TPose}, bind-pose idle {summary.BindPoseIdle}, flagged (any t-pose/bind) {summary.Flagged}, " +
            $"action t-pose {summary.ActionTPose}, motionless {summary.MotionlessIdle}, height jumps {summary.HeightJumps}, errors {summary.RenderErrors}. Report: {Path.Combine(directory, reportName)}");
    }

    private static RigPose Copy(GltfRig rig, RigPose source)
    {
        var pose = new RigPose(rig); source.Local.CopyTo(pose.Local, 0); pose.UpdateWorld(); return pose;
    }

    private static Vector3D Position(Matrix4D matrix) => new(matrix.M41, matrix.M42, matrix.M43);
    private static Vector3D Apply(Vector3D p, Matrix4D m) => new(p.X * m.M11 + p.Y * m.M21 + p.Z * m.M31 + m.M41, p.X * m.M12 + p.Y * m.M22 + p.Z * m.M32 + m.M42, p.X * m.M13 + p.Y * m.M23 + p.Z * m.M33 + m.M43);
    private static double Distance(Vector3D a, Vector3D b) => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y) + (a.Z - b.Z) * (a.Z - b.Z));

    private static double MeshHeight(GpuModel model, RigPose pose)
    {
        var (min, max) = model.Mesh.Bounds(model.Rig, pose, precise: false);
        return double.IsFinite(max.Y - min.Y) ? max.Y - min.Y : 0;
    }

    // Skin-space comparison: in the bind pose every palette matrix maps the joint's bind position to the same place
    // as one shared rigid transform. Deviation is measured against the shallowest joint of each skin.
    private static PoseCheck Check(GpuModel model, RigPose pose)
    {
        var rig = model.Rig; double deviation = 0, armDeviation = 0; int armsFound = 0, armsHorizontal = 0; double maxSlope = 0;
        var depth = new int[rig.Nodes.Length];
        for (int i = 0; i < rig.Nodes.Length; i++) { int d = 0; for (int p = rig.Nodes[i].Parent; p >= 0; p = rig.Nodes[p].Parent) d++; depth[i] = d; }
        var arms = new HashSet<int>();
        for (int node = 0; node < rig.Nodes.Length; node++)
        {
            string name = rig.Nodes[node].Name;
            if (!ArmName().IsMatch(name) || NotUpperArm().IsMatch(name)) continue;
            int parent = rig.Nodes[node].Parent;
            if (parent >= 0 && ArmName().IsMatch(rig.Nodes[parent].Name) && !NotUpperArm().IsMatch(rig.Nodes[parent].Name)) continue; // keep the top of a chain
            arms.Add(node);
        }
        foreach (var instance in model.Mesh.Instances.Where(instance => instance.Skin >= 0).DistinctBy(instance => instance.Skin))
        {
            var skin = rig.Skins[instance.Skin]; if (skin.Joints.Length < 2) continue;
            var palette = new Matrix4D[skin.Joints.Length]; pose.SkinMatrices(instance.Skin, instance.Node, palette);
            var bind = new Vector3D[skin.Joints.Length];
            for (int i = 0; i < bind.Length; i++) bind[i] = Matrix4D.Invert(skin.InverseBindMatrices[i], out var inverse) ? Position(inverse) : default;
            double extent = 0; for (int i = 1; i < bind.Length; i++) extent = Math.Max(extent, Distance(bind[i], bind[0]));
            if (extent < 1e-9) continue;
            int reference = Enumerable.Range(0, bind.Length).MinBy(i => depth[skin.Joints[i]]);
            for (int i = 0; i < bind.Length; i++)
            {
                if (!palette[i].IsFinite || !palette[reference].IsFinite) continue;
                double value = Distance(Apply(bind[i], palette[i]), Apply(bind[i], palette[reference])) / extent;
                deviation = Math.Max(deviation, value);
                if (arms.Contains(skin.Joints[i]) || arms.Any(arm => IsDescendant(rig, skin.Joints[i], arm, 2))) armDeviation = Math.Max(armDeviation, value);
            }
        }
        var placement = model.PreviewPlacement;
        foreach (int arm in arms)
        {
            int end = Descendant(rig, arm, 2);
            if (end < 0) continue;
            var a = Position(pose.World[arm] * placement); var b = Position(pose.World[end] * placement);
            double length = Distance(a, b); if (length < 1e-6) continue;
            armsFound++; double slope = Math.Abs(b.Y - a.Y) / length; maxSlope = Math.Max(maxSlope, slope);
            if (slope < .3) armsHorizontal++;
        }
        return new(deviation, armDeviation, armsFound, armsHorizontal, maxSlope);
    }

    private static bool IsDescendant(GltfRig rig, int node, int ancestor, int levels)
    {
        for (int p = rig.Nodes[node].Parent, l = 0; p >= 0 && l < levels; p = rig.Nodes[p].Parent, l++) if (p == ancestor) return true;
        return false;
    }

    // Deepest node up to `levels` below the arm along its first child chain (elbow, then hand).
    private static int Descendant(GltfRig rig, int node, int levels)
    {
        int current = node, found = -1;
        for (int l = 0; l < levels; l++)
        {
            int child = Array.FindIndex(rig.Nodes, n => n.Parent == current);
            if (child < 0) break; found = current = child;
        }
        return found;
    }

    private static double Motion(GpuModel model, RigPose a, RigPose b)
    {
        double extent = 0, change = 0;
        var first = a.World.Select(Position).ToArray(); var second = b.World.Select(Position).ToArray();
        for (int i = 1; i < first.Length; i++) extent = Math.Max(extent, Distance(first[i], first[0]));
        if (extent < 1e-9) return 0;
        for (int i = 0; i < first.Length; i++) change = Math.Max(change, Distance(first[i], second[i]));
        return change / extent;
    }

    private static (Matrix4x4 Camera, Vector3 Eye) FittedCamera(GpuModel model, RigPose pose)
    {
        var (min, max) = model.Mesh.Bounds(model.Rig, pose);
        Matrix4D placement = model.PreviewPlacement;
        var corners = new Vector3[8];
        for (int corner = 0; corner < 8; corner++)
        {
            double x = (corner & 1) == 0 ? min.X : max.X, y = (corner & 2) == 0 ? min.Y : max.Y, z = (corner & 4) == 0 ? min.Z : max.Z;
            var p = Apply(new(x, y, z), placement); corners[corner] = new((float)p.X, (float)p.Y, (float)p.Z);
        }
        var center = corners.Aggregate(Vector3.Zero, (total, corner) => total + corner) / 8;
        var eye = center + Vector3.Normalize(new Vector3(2.6f, 1.8f, 5)) * 10;
        var view = Matrix4x4.CreateLookAt(eye, center, Vector3.UnitY);
        var projected = corners.Select(corner => Vector3.Transform(corner, view)).ToArray();
        // 1.6x margin so action poses that leave the idle bounds stay visible in the fixed idle framing.
        float span = Math.Max(.01f, Math.Max(projected.Max(c => c.X) - projected.Min(c => c.X), projected.Max(c => c.Y) - projected.Min(c => c.Y))) * 1.6f;
        return (view * Matrix4x4.CreateOrthographic(span, span, .01f, 100), eye);
    }

    private static unsafe byte[] ReadTopDown(int width, int height)
    {
        byte[] raw = new byte[width * height * 4];
        fixed (byte* data = raw) OpenGl.ReadPixels(0, 0, width, height, OpenGl.GL_RGBA, OpenGl.GL_UNSIGNED_BYTE, data);
        byte[] flipped = new byte[raw.Length];
        for (int y = 0; y < height; y++) Buffer.BlockCopy(raw, (height - 1 - y) * width * 4, flipped, y * width * 4, width * 4);
        for (int i = 3; i < flipped.Length; i += 4) flipped[i] = 255;
        return flipped;
    }

    // Nearest-neighbour scale of a straight-alpha RGBA image into a square cell, composited over the background.
    private static void Blit(byte[] target, PngImage image, int left, int top, int size)
    {
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
        {
            int sx = x * image.Width / size, sy = y * image.Height / size; int s = (sy * image.Width + sx) * 4, d = ((top + y) * Width + left + x) * 4;
            int alpha = image.Rgba[s + 3];
            for (int c = 0; c < 3; c++) target[d + c] = (byte)((image.Rgba[s + c] * alpha + target[d + c] * (255 - alpha)) / 255);
        }
    }

    private static void WritePng(string path, int width, int height, byte[] rgba)
    {
        using var output = File.Create(path); output.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        byte[] header = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(header, width); BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height); header[8] = 8; header[9] = 6;
        Chunk(output, "IHDR"u8, header);
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, true))
        {
            byte[] row = new byte[width * 4 + 1];
            for (int y = 0; y < height; y++) { Buffer.BlockCopy(rgba, y * width * 4, row, 1, width * 4); zlib.Write(row); }
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

    private sealed class Target : IDisposable
    {
        private readonly GlFramebuffer frame;
        private readonly GlTexture color, depth;
        public Target(int width, int height)
        {
            Span<GlFramebuffer> frames = stackalloc GlFramebuffer[1]; OpenGl.CreateFramebuffers(frames); frame = frames[0];
            Span<GlTexture> textures = stackalloc GlTexture[2]; OpenGl.CreateTextures(OpenGl.GL_TEXTURE_2D, textures); color = textures[0]; depth = textures[1];
            OpenGl.TextureStorage2D(color.Id, 1, OpenGl.GL_RGBA8, width, height); OpenGl.TextureStorage2D(depth.Id, 1, OpenGl.GL_DEPTH_COMPONENT24, width, height);
            OpenGl.NamedFramebufferTexture(frame, OpenGl.GL_COLOR_ATTACHMENT0, color.Id, 0); OpenGl.NamedFramebufferTexture(frame, OpenGl.GL_DEPTH_ATTACHMENT, depth.Id, 0);
            OpenGl.NamedFramebufferDrawBuffer(frame, OpenGl.GL_COLOR_ATTACHMENT0);
            if (OpenGl.CheckNamedFramebufferStatus(frame, OpenGl.GL_FRAMEBUFFER) != OpenGl.GL_FRAMEBUFFER_COMPLETE) throw new InvalidOperationException("Sweep framebuffer incomplete.");
            OpenGl.BindFramebuffer(OpenGl.GL_FRAMEBUFFER, frame);
        }
        public void Dispose() { OpenGl.BindFramebuffer(OpenGl.GL_FRAMEBUFFER, default); OpenGl.DeleteTextures(new[] { color, depth }); OpenGl.DeleteFramebuffers(new[] { frame }); }
    }
}

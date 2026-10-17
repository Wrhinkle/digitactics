using System.Numerics;
using System.Text.Json;

namespace DigiTactics.Assets;

public readonly record struct Vector3D(double X, double Y, double Z);
public readonly record struct QuaternionD(double X, double Y, double Z, double W);
internal readonly record struct Vector4D(double X, double Y, double Z, double W);
public readonly record struct NodeTransform(Vector3D Translation, QuaternionD Rotation, Vector3D Scale)
{
    public Matrix4D Matrix => Matrix4D.Compose(Translation, Rotation, Scale);
}
public sealed record RigNode(string Name, int Parent, NodeTransform Rest);
public sealed record RigSkin(int[] Joints, Matrix4D[] InverseBindMatrices);
public enum AnimationPath { Translation, Rotation, Scale }
public sealed record RigChannel(int Node, AnimationPath Path, double[] Times, Vector4[] Values, bool Step);
public sealed record RigAnimation(string Name, double Duration, RigChannel[] Channels);

/// <summary>Shared immutable rig data, independent of an instance's sampled pose and GPU resources.
/// glTF column-major matrices map to System.Numerics row-vector matrices without reordering values.</summary>
public sealed class GltfRig
{
    public RigNode[] Nodes { get; }
    public RigSkin[] Skins { get; }
    public RigAnimation[] Animations { get; }
    public int RepairedKeys { get; }
    public int RepairedBindMatrices { get; private set; }
    private GltfRig(RigNode[] nodes, RigSkin[] skins, RigAnimation[] animations, int repairedKeys)
    { Nodes = nodes; Skins = skins; Animations = animations; RepairedKeys = repairedKeys; }

    public static GltfRig Load(GltfAsset asset)
    {
        var root = asset.Root;
        var definitions = root.GetProperty("nodes").EnumerateArray().ToArray();
        var parents = Enumerable.Repeat(-1, definitions.Length).ToArray();
        for (int i = 0; i < definitions.Length; i++)
            if (definitions[i].TryGetProperty("children", out var children)) foreach (var child in children.EnumerateArray()) {
                int index = child.GetInt32();
                if (index < 0 || index >= parents.Length || parents[index] != -1) throw new InvalidDataException("Invalid rig hierarchy.");
                parents[index] = i;
            }
        var nodes = definitions.Select((n, i) => new RigNode(n.TryGetProperty("name", out var name) ? name.GetString()! : $"node_{i}", parents[i], Rest(n))).ToArray();
        var skins = new List<RigSkin>();
        if (root.TryGetProperty("skins", out var sourceSkins)) foreach (var skin in sourceSkins.EnumerateArray()) {
            var joints = skin.GetProperty("joints").EnumerateArray().Select(j => j.GetInt32()).ToArray();
            var inverse = joints.Select((_, i) => skin.TryGetProperty("inverseBindMatrices", out var a) ? Matrix(asset.AccessorElement(a.GetInt32(), i)) : Matrix4D.Identity).ToArray();
            skins.Add(new(joints, inverse));
        }
        var animations = new List<RigAnimation>(); int repaired = 0;
        var arrays = new Dictionary<int, double[][]>();
        double[][] Read(int index) {
            if (arrays.TryGetValue(index, out var values)) return values;
            int count = root.GetProperty("accessors")[index].GetProperty("count").GetInt32();
            return arrays[index] = Enumerable.Range(0, count).Select(i => asset.AccessorElement(index, i)).ToArray();
        }
        if (root.TryGetProperty("animations", out var sourceAnimations)) foreach (var animation in sourceAnimations.EnumerateArray()) {
            var channels = new List<RigChannel>(); double duration = 0;
            foreach (var channel in animation.GetProperty("channels").EnumerateArray()) {
                var target = channel.GetProperty("target"); int node = target.GetProperty("node").GetInt32();
                var path = target.GetProperty("path").GetString() switch { "translation" => AnimationPath.Translation, "rotation" => AnimationPath.Rotation, "scale" => AnimationPath.Scale, _ => throw new NotSupportedException("Unsupported animation target.") };
                var sampler = animation.GetProperty("samplers")[channel.GetProperty("sampler").GetInt32()];
                string interpolation = sampler.TryGetProperty("interpolation", out var mode) ? mode.GetString()! : "LINEAR";
                if (interpolation is not ("LINEAR" or "STEP")) throw new NotSupportedException($"Unsupported animation interpolation: {interpolation}");
                var times = Read(sampler.GetProperty("input").GetInt32()).Select(x => x[0]).ToArray();
                var raw = Read(sampler.GetProperty("output").GetInt32());
                if (times.Length == 0 || raw.Length != times.Length) throw new InvalidDataException("Invalid animation key counts.");
                for (int i = 0; i < times.Length; i++) if (!double.IsFinite(times[i]) || times[i] < 0 || i > 0 && times[i] < times[i - 1]) throw new InvalidDataException("Invalid animation timeline.");
                var rest = nodes[node].Rest;
                Vector4 previous = path switch { AnimationPath.Translation => new((float)rest.Translation.X, (float)rest.Translation.Y, (float)rest.Translation.Z, 0), AnimationPath.Scale => new((float)rest.Scale.X, (float)rest.Scale.Y, (float)rest.Scale.Z, 0), _ => new((float)rest.Rotation.X, (float)rest.Rotation.Y, (float)rest.Rotation.Z, (float)rest.Rotation.W) };
                var values = new Vector4[raw.Length];
                for (int i = 0; i < raw.Length; i++) {
                    var v = raw[i];
                    bool valid = v.Length == (path == AnimationPath.Rotation ? 4 : 3) && v.All(x => double.IsFinite(x) && Math.Abs(x) <= float.MaxValue);
                    if (valid) {
                        var next = new Vector4((float)v[0], (float)v[1], (float)v[2], v.Length == 4 ? (float)v[3] : 0);
                        if (path != AnimationPath.Rotation || next.LengthSquared() > 1e-12f) previous = next;
                        else repaired++;
                    } else repaired++;
                    // A corrupt key holds the last valid pose (or bind pose at the beginning).
                    values[i] = previous;
                }
                duration = Math.Max(duration, times[^1]); channels.Add(new(node, path, times, values, interpolation == "STEP"));
            }
            animations.Add(new(animation.TryGetProperty("name", out var name) ? name.GetString()! : "", duration, channels.ToArray()));
        }
        var rig = new GltfRig(nodes, skins.ToArray(), animations.ToArray(), repaired);
        var bindPose = new RigPose(rig); // Validate hierarchy and repair corrupt inverse binds from rest transforms.
        foreach (var skin in rig.Skins) for (int i = 0; i < skin.Joints.Length; i++) {
            var m = skin.InverseBindMatrices[i];
            if (m.IsFinite) continue;
            if (!Matrix4D.Invert(bindPose.World[skin.Joints[i]], out var inverse) || !inverse.IsFinite)
                throw new InvalidDataException("Cannot reconstruct corrupt inverse bind matrix.");
            skin.InverseBindMatrices[i] = inverse; rig.RepairedBindMatrices++;
        }
        return rig;
    }

    private static NodeTransform Rest(JsonElement node)
    {
        double[] Values(string key, double[] fallback) => node.TryGetProperty(key, out var value) ? value.EnumerateArray().Select(v => v.GetDouble()).ToArray() : fallback;
        if (node.TryGetProperty("matrix", out var matrix)) {
            if (!Matrix4x4.Decompose(FloatMatrix(matrix.EnumerateArray().Select(x => x.GetDouble()).ToArray()), out var scale, out var rotation, out var translation)) throw new InvalidDataException("Invalid node matrix.");
            return new(new(translation.X,translation.Y,translation.Z),new(rotation.X,rotation.Y,rotation.Z,rotation.W),new(scale.X,scale.Y,scale.Z));
        }
        var t = Values("translation", [0, 0, 0]); var r = Values("rotation", [0, 0, 0, 1]); var s = Values("scale", [1, 1, 1]);
        return new(new(t[0], t[1], t[2]), new(r[0], r[1], r[2], r[3]), new(s[0], s[1], s[2]));
    }

    /// <summary>Applies build-time fallback clips, rest poses and weapon parenting for a model source.</summary>
    public GltfRig Prepare(ModelSource source, string assetRoot)
    {
        var motion = source.Motion ?? throw new InvalidDataException("Missing prepared model motion. Run the native asset exporters.");
        using var bytes = new MemoryStream(ModelCatalog.ReadPayload(assetRoot, motion.File, motion.Sha256, motion.Bytes), writable: false);
        using var gzip = new System.IO.Compression.GZipStream(bytes, System.IO.Compression.CompressionMode.Decompress);
        using var json = JsonDocument.Parse(gzip);
        var root = json.RootElement;
        if (root.GetProperty("schemaVersion").GetInt32() != 1) throw new InvalidDataException("Unsupported prepared motion schema.");
        var definitions = root.GetProperty("nodes");
        if (definitions.GetArrayLength() != Nodes.Length) throw new InvalidDataException("Prepared rig node count differs.");
        var nodes = new RigNode[Nodes.Length];
        for (int i = 0; i < nodes.Length; i++)
        {
            var value = definitions[i]; int parent = value.GetProperty("parent").GetInt32();
            if (parent < -1 || parent >= nodes.Length) throw new InvalidDataException("Invalid prepared parent.");
            var rest = Rest(value);
            if (!rest.Matrix.IsFinite) throw new InvalidDataException("Nonfinite prepared rest pose.");
            nodes[i] = new(Nodes[i].Name, parent, rest);
        }
        var clips = new List<RigAnimation>();
        foreach (var clip in root.GetProperty("clips").EnumerateArray())
        {
            if (clip.TryGetProperty("authored", out var authored)) { clips.Add(Animations[authored.GetInt32()]); continue; }
            double duration = clip.GetProperty("duration").GetDouble();
            if (!double.IsFinite(duration) || duration < 0) throw new InvalidDataException("Invalid prepared clip duration.");
            var channels = new List<RigChannel>();
            foreach (var channel in clip.GetProperty("channels").EnumerateArray())
            {
                int node = channel.GetProperty("node").GetInt32();
                if (node < 0 || node >= nodes.Length) throw new InvalidDataException("Invalid prepared channel target.");
                var path = channel.GetProperty("path").GetString() switch { "position" => AnimationPath.Translation, "scale" => AnimationPath.Scale, "quaternion" => AnimationPath.Rotation, _ => throw new InvalidDataException("Unsupported prepared track.") };
                var times = channel.GetProperty("times").EnumerateArray().Select(t => t.GetDouble()).ToArray();
                int width = path == AnimationPath.Rotation ? 4 : 3;
                var raw = channel.GetProperty("values").EnumerateArray().Select(v => v.GetSingle()).ToArray();
                if (times.Length == 0 || raw.Length != times.Length * width || raw.Any(v => !float.IsFinite(v))) throw new InvalidDataException("Invalid prepared key values.");
                var values = new Vector4[times.Length];
                for (int i = 0; i < times.Length; i++)
                {
                    if (!double.IsFinite(times[i]) || times[i] < 0 || i > 0 && times[i] < times[i - 1]) throw new InvalidDataException("Invalid prepared timeline.");
                    int offset = i * width;
                    values[i] = new(raw[offset], raw[offset+1], raw[offset+2], width == 4 ? raw[offset+3] : 0);
                }
                channels.Add(new(node, path, times, values, false));
            }
            clips.Add(new(clip.GetProperty("name").GetString()!, duration, channels.ToArray()));
        }
        var prepared = new GltfRig(nodes, Skins, clips.ToArray(), RepairedKeys) { RepairedBindMatrices = RepairedBindMatrices };
        _ = new RigPose(prepared); // Validate new parenting before it reaches a renderer.
        return prepared;
    }
    private static Matrix4D Matrix(double[] a) => Matrix4D.From(a);
    private static Matrix4x4 FloatMatrix(double[] a) => new((float)a[0], (float)a[1], (float)a[2], (float)a[3], (float)a[4], (float)a[5], (float)a[6], (float)a[7], (float)a[8], (float)a[9], (float)a[10], (float)a[11], (float)a[12], (float)a[13], (float)a[14], (float)a[15]);
}

/// <summary>Per-unit pose. Sampling resets to bind pose, then applies one clip without mutating the rig.</summary>
public sealed class RigPose
{
    private readonly GltfRig rig;
    private readonly int[] order;
    public NodeTransform[] Local { get; }
    public Matrix4D[] World { get; }
    public RigPose(GltfRig rig)
    {
        this.rig = rig; Local = new NodeTransform[rig.Nodes.Length]; World = new Matrix4D[rig.Nodes.Length];
        var ordered = new List<int>(); var state = new byte[rig.Nodes.Length];
        void Visit(int node) {
            if (state[node] == 2) return;
            if (state[node] == 1) throw new InvalidDataException("Cyclic rig hierarchy.");
            state[node] = 1; if (rig.Nodes[node].Parent >= 0) Visit(rig.Nodes[node].Parent);
            state[node] = 2; ordered.Add(node);
        }
        for (int i = 0; i < rig.Nodes.Length; i++) Visit(i);
        order = ordered.ToArray(); Sample(null, 0);
    }
    public void Sample(RigAnimation? animation, double time, bool loop = false)
    {
        for (int i = 0; i < Local.Length; i++) Local[i] = rig.Nodes[i].Rest;
        if (animation is not null) {
            time = loop && animation.Duration > 0 ? Math.Max(0, time) % animation.Duration : Math.Clamp(time, 0, animation.Duration);
            foreach (var channel in animation.Channels) {
                int right = Array.BinarySearch(channel.Times, time); if (right < 0) right = ~right;
                int left = Math.Max(0, right - 1); right = Math.Min(right, channel.Times.Length - 1);
                double fraction = channel.Times[right] > channel.Times[left] ? (time - channel.Times[left]) / (channel.Times[right] - channel.Times[left]) : 0;
                var a = channel.Values[left]; var b = channel.Values[right];
                var step = time >= channel.Times[right] ? b : a;
                var value = channel.Step ? new Vector4D(step.X,step.Y,step.Z,step.W) : Linear(a, b, fraction);
                var current = Local[channel.Node];
                Local[channel.Node] = channel.Path switch {
                    AnimationPath.Translation => current with { Translation = new(value.X, value.Y, value.Z) },
                    AnimationPath.Scale => current with { Scale = new(value.X, value.Y, value.Z) },
                    _ => current with { Rotation = channel.Step ? new(value.X, value.Y, value.Z, value.W) : Slerp(a, b, fraction) }
                };
            }
        }
        UpdateWorld();
    }
    public void UpdateWorld()
    {
        foreach (int node in order) World[node] = rig.Nodes[node].Parent < 0 ? Local[node].Matrix : Local[node].Matrix * World[rig.Nodes[node].Parent];
    }
    private static Vector4D Linear(Vector4 a, Vector4 b, double t) => new(
        a.X * (1 - t) + b.X * t, a.Y * (1 - t) + b.Y * t,
        a.Z * (1 - t) + b.Z * t, a.W * (1 - t) + b.W * t);

    // Preserve the original interpolant's endpoint and nearly parallel behavior for quantized quaternions.
    private static QuaternionD Slerp(Vector4 a, Vector4 b, double t)
        => Blend(new(a.X,a.Y,a.Z,a.W), new(b.X,b.Y,b.Z,b.W), t);

    internal static QuaternionD Blend(QuaternionD a, QuaternionD b, double t)
    {
        if (t == 0 || a == b) return new(a.X, a.Y, a.Z, a.W);
        if (t == 1) return new(b.X, b.Y, b.Z, b.W);
        double s = 1 - t, cos = (double)a.X*b.X + (double)a.Y*b.Y + (double)a.Z*b.Z + (double)a.W*b.W;
        double direction = cos >= 0 ? 1 : -1, squaredSine = 1 - cos*cos;
        if (squaredSine > 2.220446049250313e-16) {
            double sine = Math.Sqrt(squaredSine), angle = Math.Atan2(sine, cos*direction);
            s = Math.Sin(s*angle)/sine; t = Math.Sin(t*angle)/sine;
        }
        double x=a.X*s+b.X*t*direction,y=a.Y*s+b.Y*t*direction,z=a.Z*s+b.Z*t*direction,w=a.W*s+b.W*t*direction;
        if (s == 1-t) { double factor=1/Math.Sqrt(x*x+y*y+z*z+w*w);x*=factor;y*=factor;z*=factor;w*=factor; }
        return new(x,y,z,w);
    }

    public void SkinMatrices(int skin, int meshNode, Span<Matrix4D> destination)
    {
        var binding = rig.Skins[skin];
        if (destination.Length < binding.Joints.Length) throw new ArgumentException("Skin palette is too short.", nameof(destination));
        if (!Matrix4D.Invert(World[meshNode], out var inverseMesh)) {
            // Zero-scale attachments are hidden in several original rigs; Three.js uses a zero inverse.
            destination[..binding.Joints.Length].Clear(); return;
        }
        for (int i = 0; i < binding.Joints.Length; i++) destination[i] = binding.InverseBindMatrices[i] * World[binding.Joints[i]] * inverseMesh;
    }
}

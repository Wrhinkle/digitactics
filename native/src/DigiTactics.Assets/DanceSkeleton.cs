using System.Numerics;

namespace DigiTactics.Assets;

public sealed record DanceBone(int Node, string Kind, int ParentEntry, Vector3 Direction,
    Quaternion ParentRotation, Quaternion RestRotation, int Side = 0, float Share = 1, int Index = 0, float Limit = 0);
public sealed record DanceBinding(DanceBone[] Bones, bool Limbless, bool Stubby, float Yaw = 0);
public sealed record DancePose(Dictionary<string, Vector3> Vectors, Dictionary<string, float> Scalars, Dictionary<string, float> Root);
public sealed record DanceRoot(Vector3 Position, Vector3 Rotation, float Pivot, float Squash, Quaternion Facing);

/// <summary>Original character-space dance retargeting. Bindings are immutable; each instance owns its accumulated rotations.</summary>
public sealed class DanceSkeleton(DanceBinding binding)
{
    private const float Degree = MathF.PI / 180;
    private readonly Quaternion[] accumulated = new Quaternion[binding.Bones.Length];
    private readonly Quaternion facing = Quaternion.CreateFromAxisAngle(Vector3.UnitY, binding.Yaw);
    private static float Limit(string kind) => kind switch
    {
        "hips" => 30, "spine" => 28, "neck" => 25, "head" => 40, "up" => 155,
        "fore" => 140, "hand" => 35, "thigh" => 95, "shin" => 110, "foot" => 35,
        "tail" => 35, "wing" => 50, "ear" => 45, _ => 40
    };
    private Vector3 Direction(Vector3 value) => Vector3.Transform(value.LengthSquared() < 1e-8 ? Vector3.UnitY : Vector3.Normalize(value), facing);
    private Quaternion Euler(Vector3 degrees, float scale) => facing * Yxz(degrees * new Vector3(Degree * scale, Degree * scale, -Degree * scale)) * Quaternion.Conjugate(facing);
    private static Quaternion Yxz(Vector3 radians) => Quaternion.CreateFromYawPitchRoll(radians.Y, radians.X, radians.Z);
    private static Quaternion Between(Vector3 from, Vector3 to)
    {
        float r = Vector3.Dot(from, to) + 1;
        Vector3 xyz = r < 1e-15 ? (Math.Abs(from.X) > Math.Abs(from.Z) ? new(-from.Y, from.X, 0) : new(0, -from.Z, from.Y)) : Vector3.Cross(from, to);
        return Quaternion.Normalize(new(xyz, r < 1e-15 ? 0 : r));
    }
    private Quaternion Aim(DanceBone bone, Vector3 target) => Between(bone.Direction,
        Vector3.Transform(target, Quaternion.Conjugate(bone.ParentEntry >= 0 ? accumulated[bone.ParentEntry] : Quaternion.Identity)));
    private static Quaternion Clamp(Quaternion q, float limit)
    {
        if (q.W < 0) q = new(-q.X, -q.Y, -q.Z, -q.W);
        float angle = 2 * MathF.Acos(Math.Min(1, q.W));
        if (angle <= limit || angle <= 1e-6) return q;
        float s = MathF.Sin(limit / 2) / MathF.Sin(angle / 2);
        return new(q.X * s, q.Y * s, q.Z * s, MathF.Cos(limit / 2));
    }
    public DanceRoot Apply(DancePose pose, RigPose target, float weight = 1)
    {
        float w = Math.Clamp(weight, 0, 1), body = binding.Limbless ? 1.5f : binding.Stubby ? 1.25f : 1;
        float Scalar(string key) => pose.Scalars.GetValueOrDefault(key);
        Vector3 V(string key) => pose.Vectors.GetValueOrDefault(key);
        Vector3? Arm(string key) => pose.Vectors.TryGetValue(key, out var value) ? Direction(value) : null;
        Vector3? left = Arm("aL"), right = Arm("aR");
        for (int i = 0; i < binding.Bones.Length; i++)
        {
            var bone = binding.Bones[i]; Quaternion rotation = Quaternion.Identity;
            string side = bone.Side > 0 ? "L" : "R";
            switch (bone.Kind)
            {
                case "hips": rotation = Euler(V("hp"), body); break;
                case "spine": rotation = Euler(V("sp"), bone.Share * body); break;
                case "neck": rotation = Euler(V("hd"), .3f); break;
                case "head": rotation = Euler(V("hd"), bone.Share); break;
                case "up": case "fore":
                    if (pose.Vectors.TryGetValue((bone.Kind == "up" ? "a" : "f") + side, out var arm)) rotation = Aim(bone, Direction(arm));
                    break;
                case "thigh": case "shin": case "foot":
                    rotation = Aim(bone, bone.Kind != "foot" && pose.Vectors.TryGetValue((bone.Kind == "thigh" ? "l" : "k") + side, out var leg) ? Direction(leg) : bone.Direction);
                    break;
                case "tail": rotation = Yxz(new(MathF.Sin((bone.Index + 1) * .9f) * .1f * (Scalar("tl") != 0 ? 1 : 0), Scalar("tl") * Degree * bone.Share * 1.6f, 0)); break;
                case "wing":
                    float flap = pose.Scalars.TryGetValue("w", out var wing) ? wing : left.HasValue && right.HasValue ? (left.Value.Y + right.Value.Y) / 2 : 0;
                    rotation = Quaternion.CreateFromAxisAngle(Vector3.Transform(Vector3.UnitZ, facing), flap * .5f * bone.Side * bone.Share); break;
                case "ear":
                    var a = bone.Side > 0 ? left : right;
                    if (a.HasValue && binding.Stubby)
                    {
                        var v = a.Value;
                        rotation = Aim(bone, Vector3.Normalize(new(v.X, Math.Max(Math.Abs(v.Y), .35f), v.Z - (v.Y > .5 ? .3f : 0))));
                        rotation = Quaternion.Slerp(rotation, Quaternion.Identity, 1 - .6f * bone.Share);
                        if (Scalar("w") != 0) rotation = Quaternion.CreateFromAxisAngle(Vector3.Transform(Vector3.UnitZ, facing), Scalar("w") * .35f * bone.Side) * rotation;
                    }
                    else rotation = Euler(new(0, 0, V("hd").Z * .6f + Scalar("tl") * .3f), 1);
                    break;
            }
            rotation = Clamp(rotation, bone.Limit != 0 ? bone.Limit : Limit(bone.Kind) * Degree);
            accumulated[i] = (bone.ParentEntry >= 0 ? accumulated[bone.ParentEntry] : Quaternion.Identity) * rotation;
            var local = Quaternion.Conjugate(bone.ParentRotation) * rotation * bone.ParentRotation * bone.RestRotation;
            var current = target.Local[bone.Node]; var q = current.Rotation;
            if (w < 1) local = Quaternion.Slerp(new((float)q.X, (float)q.Y, (float)q.Z, (float)q.W), local, w);
            target.Local[bone.Node] = current with { Rotation = new(local.X, local.Y, local.Z, local.W) };
        }
        target.UpdateWorld();
        float Root(string key) => pose.Root.GetValueOrDefault(key);
        var position = new Vector3(Root("x"), Root("y"), Root("z")) * w;
        var angles = new Vector3(Root("rx"), Root("ry"), Root("rz")) * w;
        float squash = Root("sq");
        if (binding.Limbless)
        {
            position.Y *= 1.4f;
            squash = 1 + .08f * (left.HasValue && right.HasValue ? (left.Value.Y + right.Value.Y) / 2 : 0) * w;
            if (left.HasValue && right.HasValue) angles.Z -= .12f * (left.Value.X + right.Value.X) / 2 * w;
        }
        return new(position, angles, Root("piv"), squash == 0 ? 1 : squash, facing);
    }
}

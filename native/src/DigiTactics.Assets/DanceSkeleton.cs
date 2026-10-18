using System.Numerics;

namespace DigiTactics.Assets;

public sealed record DanceBone(int Node, string Kind, int ParentEntry, Vector3 Direction,
    Quaternion ParentRotation, Quaternion RestRotation, int Side = 0, float Share = 1, int Index = 0, float Limit = 0, QuaternionD? ExactParentRotation = null, QuaternionD? ExactRestRotation = null);
public sealed record DanceBinding(DanceBone[] Bones, bool Limbless, bool Stubby, float Yaw = 0, bool Quadruped = false);
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
    // Source rips contain slightly non-unit quaternions. Preserve Three.js's
    // dot >= 1 early return instead of introducing movement while blending them.
    private static Quaternion Blend(Quaternion from, Quaternion to, float weight)
    {
        if (weight == 0) return from;
        if (weight == 1) return to;
        double dot = (double)from.X * to.X + (double)from.Y * to.Y + (double)from.Z * to.Z + (double)from.W * to.W;
        if (dot < 0) { to = new(-to.X, -to.Y, -to.Z, -to.W); dot = -dot; }
        if (dot >= 1) return from;
        double sinSquared = 1 - dot * dot;
        if (sinSquared <= 2.220446049250313e-16) return Quaternion.Normalize(from * (1 - weight) + to * weight);
        double sin = Math.Sqrt(sinSquared), angle = Math.Atan2(sin, dot);
        return from * (float)(Math.Sin((1 - weight) * angle) / sin) + to * (float)(Math.Sin(weight * angle) / sin);
    }
    private static QuaternionD D(Quaternion q) => new(q.X,q.Y,q.Z,q.W);
    private static QuaternionD Multiply(QuaternionD a,QuaternionD b) => new(a.X*b.W+a.W*b.X+a.Y*b.Z-a.Z*b.Y,a.Y*b.W+a.W*b.Y+a.Z*b.X-a.X*b.Z,a.Z*b.W+a.W*b.Z+a.X*b.Y-a.Y*b.X,a.W*b.W-a.X*b.X-a.Y*b.Y-a.Z*b.Z);
    private static QuaternionD BlendExact(QuaternionD from,QuaternionD to,double weight)
    {
        double dot=from.X*to.X+from.Y*to.Y+from.Z*to.Z+from.W*to.W;
        if(dot<0){to=new(-to.X,-to.Y,-to.Z,-to.W);dot=-dot;}
        if(dot>=1)return from;
        double squared=1-dot*dot;
        double a=1-weight,b=weight;
        if(squared>2.220446049250313e-16){double sin=Math.Sqrt(squared),angle=Math.Atan2(sin,dot);a=Math.Sin((1-weight)*angle)/sin;b=Math.Sin(weight*angle)/sin;}
        var result=new QuaternionD(from.X*a+to.X*b,from.Y*a+to.Y*b,from.Z*a+to.Z*b,from.W*a+to.W*b);
        if(squared<=2.220446049250313e-16){double length=Math.Sqrt(result.X*result.X+result.Y*result.Y+result.Z*result.Z+result.W*result.W);result=new(result.X/length,result.Y/length,result.Z/length,result.W/length);}
        return result;
    }
    public DanceRoot Apply(DancePose pose, RigPose? target, float weight = 1)
    {
        if (target is null && binding.Bones.Length != 0) throw new ArgumentNullException(nameof(target));
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
                    string channel=(bone.Kind=="up"?"a":"f")+side;
                    bool foreleg=binding.Quadruped&&Scalar("familyAdapt")!=0;
                    if(foreleg)channel=(bone.Kind=="up"?"l":"k")+side;
                    if (pose.Vectors.TryGetValue(channel, out var arm)) rotation = Aim(bone, Direction(foreleg?new(arm.X,arm.Y,-arm.Z):arm));
                    break;
                case "thigh": case "shin": case "foot":
                    rotation = Aim(bone, bone.Kind != "foot" && pose.Vectors.TryGetValue((bone.Kind == "thigh" ? "l" : "k") + side, out var leg) ? Direction(leg) : bone.Direction);
                    break;
                case "tail": rotation = Yxz(new(MathF.Sin((bone.Index + 1) * .9f) * .1f * (Scalar("familyAdapt")!=0?Math.Min(1,Math.Abs(Scalar("tl"))/20):Scalar("tl") != 0 ? 1 : 0), Scalar("tl") * Degree * bone.Share * 1.6f, 0)); break;
                case "wing":
                    float flap = pose.Scalars.TryGetValue("w", out var wing) ? wing : left.HasValue && right.HasValue ? (left.Value.Y + right.Value.Y) / 2 : 0;
                    rotation = Quaternion.CreateFromAxisAngle(Vector3.Transform(Vector3.UnitZ, facing), flap * .5f * bone.Side * bone.Share); break;
                case "ear":
                    var a = bone.Side > 0 ? left : right;
                    if (a.HasValue && binding.Stubby)
                    {
                        var v = a.Value;
                        rotation = Aim(bone, Vector3.Normalize(new(v.X, Math.Max(Math.Abs(v.Y), .35f), v.Z - (v.Y > .5 ? .3f : 0))));
                        rotation = Blend(rotation, Quaternion.Identity, 1 - .6f * bone.Share);
                        if (Scalar("w") != 0) rotation = Quaternion.CreateFromAxisAngle(Vector3.Transform(Vector3.UnitZ, facing), Scalar("w") * .35f * bone.Side) * rotation;
                    }
                    else rotation = Euler(new(0, 0, V("hd").Z * .6f + Scalar("tl") * .3f), 1);
                    break;
            }
            rotation = Clamp(rotation, bone.Limit != 0 ? bone.Limit : Limit(bone.Kind) * Degree);
            accumulated[i] = (bone.ParentEntry >= 0 ? accumulated[bone.ParentEntry] : Quaternion.Identity) * rotation;
            var parent = bone.ExactParentRotation ?? D(bone.ParentRotation);
            var rest = bone.ExactRestRotation ?? D(bone.RestRotation);
            var local = Multiply(Multiply(Multiply(new(-parent.X,-parent.Y,-parent.Z,parent.W),D(rotation)),parent),rest);
            var current = target!.Local[bone.Node];
            if (w < 1) local = BlendExact(current.Rotation, local, w);
            target.Local[bone.Node] = current with { Rotation = local };
        }
        target?.UpdateWorld();
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
        if(Scalar("preserveScale")!=0)squash=1;
        return new(position, angles, Root("piv"), squash == 0 ? 1 : squash, facing);
    }
}

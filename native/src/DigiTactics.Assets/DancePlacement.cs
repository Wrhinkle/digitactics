using System.Numerics;

namespace DigiTactics.Assets;

public static class DancePlacement
{
    public static Matrix4D Apply(CompanionTransform basis, DanceRoot root, double modelScale)
    {
        var e = basis.Rotation;
        double c1 = Math.Cos(e.X / 2), c2 = Math.Cos(e.Y / 2), c3 = Math.Cos(e.Z / 2), s1 = Math.Sin(e.X / 2), s2 = Math.Sin(e.Y / 2), s3 = Math.Sin(e.Z / 2);
        var original = new Quaternion((float)(s1*c2*c3+c1*s2*s3), (float)(c1*s2*c3-s1*c2*s3), (float)(c1*c2*s3+s1*s2*c3), (float)(c1*c2*c3-s1*s2*s3));
        var turn = root.Facing * Quaternion.CreateFromYawPitchRoll(root.Rotation.Y, root.Rotation.X, root.Rotation.Z) * Quaternion.Conjugate(root.Facing);
        var pivot = new Vector3(0, root.Pivot * (float)modelScale, 0);
        var offset = Vector3.Transform(pivot - Vector3.Transform(pivot, turn), original)
            + Vector3.Transform(Vector3.Transform(root.Position * (float)modelScale, root.Facing), original);
        var q = original * turn; double squash = root.Squash, inverse = 1 / Math.Sqrt(squash);
        return Matrix4D.Compose(new(basis.Position.X + offset.X, basis.Position.Y + offset.Y, basis.Position.Z + offset.Z),
            new(q.X, q.Y, q.Z, q.W), new(basis.Scale.X * inverse, basis.Scale.Y * squash, basis.Scale.Z * inverse));
    }
}

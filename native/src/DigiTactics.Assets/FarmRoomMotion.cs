namespace DigiTactics.Assets;

/// <summary>Original procedural room transforms, with baked static ancestors and descendants.</summary>
public sealed record FarmRoomMotion(string Kind, double Phase, double[] Position, double[] Rotation, double[] Scale, double[] Parent, double[] Relative)
{
    public Matrix4D Evaluate(double time)
    {
        double y = Position[1], ry = Rotation[1], rz = Rotation[2];
        switch (Kind)
        {
            case "water": y = -.55 + Math.Sin(time * .9) * .04; break;
            case "palm": rz = Math.Sin(time * .8 + Phase) * .025; break;
            case "drip": y = 1.2 - (time * .8 + Phase * .37) % 1.4; break;
            case "crystal": ry = time * .6 + Phase; y = 1.1 + Math.Sin(time * 1.4 + Phase) * .2; break;
            default: throw new InvalidDataException("Unknown farm room motion: " + Kind);
        }
        var local = new CompanionTransform(new(Position[0], y, Position[2]), new(Rotation[0], ry, rz), new(Scale[0], Scale[1], Scale[2]));
        return Matrix4D.From(Relative) * local.Matrix * Matrix4D.From(Parent);
    }
}

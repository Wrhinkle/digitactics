using System.Numerics;

namespace DigiTactics.Assets;

/// <summary>Linear-space theme lights and the original transient flash/tint controls.</summary>
public sealed class BoardLighting
{
    private readonly Vector3 baseSky, baseSun;
    private readonly float baseHemisphere, baseSunIntensity;
    public Vector3 Sky { get; private set; }
    public Vector3 Ground { get; }
    public Vector3 Sun { get; private set; }
    public float HemisphereIntensity { get; private set; }
    public float SunIntensity { get; private set; }
    public Vector3 Direction { get; } = Vector3.Normalize(new(-8,18,10));
    public Vector3 FogColor { get; set; }
    public float FogNear { get; set; }
    public float FogFar { get; set; }
    public float HemisphereMultiplier { get; set; } = 1;
    public float SunMultiplier { get; set; } = 1;
    public Vector3 Tint { get; set; }
    public float TintWeight { get; set; }
    public Vector3 FlashColor { get; set; } = Vector3.One;
    public float Flash { get; set; }

    public BoardLighting(BoardScene scene)
    {
        baseSky = Sky = Linear(scene.Hemi[0].GetString()!); Ground = Linear(scene.Hemi[1].GetString()!);
        baseHemisphere = HemisphereIntensity = scene.Hemi[2].GetSingle();
        baseSun = Sun = Tint = Linear(scene.Sun[0].GetString()!); baseSunIntensity = SunIntensity = scene.Sun[1].GetSingle();
        FogColor = Linear(scene.Fog[0].GetString()!); FogNear = scene.Fog[1].GetSingle(); FogFar = scene.Fog[2].GetSingle();
    }

    public void Update(float delta)
    {
        Flash = Math.Max(0, Flash - delta * 5);
        HemisphereIntensity = baseHemisphere * HemisphereMultiplier + Flash;
        Sky = Vector3.Lerp(baseSky, FlashColor, Math.Min(1, Flash));
        SunIntensity = baseSunIntensity * SunMultiplier;
        Sun = Vector3.Lerp(baseSun, Tint, TintWeight);
    }

    public static Vector3 Linear(string hex)
    {
        if (hex.Length != 7 || hex[0] != '#') throw new InvalidDataException("Expected six-digit board color.");
        static float ConvertChannel(int value) { float c=value/255f; return c<=.04045f ? c/12.92f : MathF.Pow((c+.055f)/1.055f,2.4f); }
        return new(ConvertChannel(Convert.ToInt32(hex[1..3],16)),ConvertChannel(Convert.ToInt32(hex[3..5],16)),ConvertChannel(Convert.ToInt32(hex[5..7],16)));
    }

    public static Vector3 Srgb(Vector3 linear)
    {
        static float Channel(float c) => c <= .0031308f ? c*12.92f : 1.055f*MathF.Pow(c,1/2.4f)-.055f;
        return new(Channel(linear.X),Channel(linear.Y),Channel(linear.Z));
    }
}

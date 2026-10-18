using System.Text.Json;

namespace DigiTactics.Assets;

public sealed record CompanionStep(string? Clip, double Loops = 1, double Speed = 1, double Length = .9, string? Effect = null);
public sealed record CompanionTransform(Vector3D Position, Vector3D Rotation, Vector3D Scale)
{
    public Matrix4D Matrix
    {
        get
        {
            double c1 = Math.Cos(Rotation.X / 2), c2 = Math.Cos(Rotation.Y / 2), c3 = Math.Cos(Rotation.Z / 2);
            double s1 = Math.Sin(Rotation.X / 2), s2 = Math.Sin(Rotation.Y / 2), s3 = Math.Sin(Rotation.Z / 2);
            return Matrix4D.Compose(Position, new(s1*c2*c3+c1*s2*s3,c1*s2*c3-s1*c2*s3,c1*c2*s3+s1*s2*c3,c1*c2*c3-s1*s2*s3), Scale);
        }
    }
}

/// <summary>Little Legend clip selection and procedural overlays from the original view.</summary>
public static class CompanionMotion
{
    public static CompanionStep[] Plan(CompanionVisual visual, JsonElement fallbacks, IEnumerable<string> clips, string role, bool moving, bool hasMixer)
    {
        var available = clips.ToHashSet(StringComparer.Ordinal);
        if (!moving && hasMixer)
        {
            var candidates = new List<JsonElement>();
            if (visual.Clips.TryGetProperty(role, out var own)) candidates.Add(own);
            if (fallbacks.TryGetProperty(role, out var fallback)) candidates.AddRange(fallback.EnumerateArray());
            else candidates.Add(JsonSerializer.SerializeToElement(role));
            foreach (var candidate in candidates)
            {
                if (candidate.ValueKind is JsonValueKind.Null or JsonValueKind.False or JsonValueKind.Undefined) continue;
                var steps = new List<CompanionStep>();
                foreach (var value in candidate.ValueKind == JsonValueKind.Array ? candidate.EnumerateArray().ToArray() : [candidate])
                {
                    JsonElement names = value.ValueKind == JsonValueKind.String ? value : value.TryGetProperty("clip", out var clip) ? clip : default;
                    string? selected = names.ValueKind switch
                    {
                        JsonValueKind.String => available.Contains(names.GetString()!) ? names.GetString() : null,
                        JsonValueKind.Array => names.EnumerateArray().Select(x => x.GetString()!).FirstOrDefault(available.Contains), _ => null
                    };
                    double Number(string name, double defaultValue) => value.ValueKind == JsonValueKind.Object && value.TryGetProperty(name, out var property) && property.GetDouble() != 0 ? property.GetDouble() : defaultValue;
                    if (names.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined) && selected is null) continue;
                    if (selected is null && Number("len", 0) == 0) continue;
                    string? effect = value.ValueKind == JsonValueKind.Object && value.TryGetProperty("fx", out var fx) ? fx.GetString() : null;
                    steps.Add(new(selected, Number("loops", 1), Number("speed", 1), Number("len", .9), effect));
                }
                if (steps.Any(x => x.Clip is not null)) return steps.ToArray();
            }
        }
        string proc = visual.Procedural.GetValueOrDefault(role, "hop");
        return [new(null, Length: role == "dance" ? 2.6 : role == "wave" ? 1.4 : proc == "slump" ? 1.6 : .9, Effect: proc)];
    }
    public static CompanionTransform Evaluate(string? effect, double k, double loops = 1, double duration = .9, double scale = 1, double yaw = 0)
    {
        double x = 0, y = 0, z = 0, rx = 0, ry = yaw, rz = 0, sx = scale, sy = scale, sz = scale;
        static double Round(double value) => Math.Floor(value + .5);
        double ease = k * k * (3 - 2 * k), sine = Math.Sin(Math.PI * k);
        switch (effect)
        {
            case "spin": ry += ease * Math.PI * 2; y = sine * .12; break;
            case "twirl": ry += ease * Math.PI * 2 * Math.Max(1, Round(loops / 2)); y = Math.Abs(Math.Sin(k * Math.PI * loops)) * .18; break;
            case "hop": y = Math.Abs(Math.Sin(k * Math.PI * Math.Max(1, Round(duration / .45)))) * .3; break;
            case "sway": { double s = Math.Sin(k * Math.PI * 2 * loops); x = s * .2; rz = -s * .2; break; }
            case "sway2": { double s = Math.Sin(k * Math.PI * 2 * loops); x = -s * .2; rz = s * .2; ry += s * .5; break; }
            case "bounce":
                double b = Math.Sin(k * Math.PI * Math.Max(2, Round(duration / .35))); y = Math.Max(0, b) * .16;
                double sq = b < 0 ? 1 + .12 * b : 1 + .06 * b; sx = sz = scale / Math.Sqrt(sq); sy = scale * sq; break;
            case "wave": { double s = Math.Sin(k * Math.PI * 6) * sine; rz = s * .3; y = Math.Abs(s) * .08; break; }
            case "droop": rx = .35 * sine; y = -.05 * sine; break;
            case "slump":
                double slump = Math.Min(1, Math.Min(k * 4, (1 - k) * 3)); rx = .5 * slump; rz = Math.Sin(k * Math.PI * 3) * .05 * slump;
                sx = sz = scale * (1 + .06 * slump); sy = scale * (1 - .12 * slump); break;
            case "wiggle": rz = Math.Sin(k * Math.PI * 8) * .3 * sine; break;
            case "bob": rx = Math.Max(0, Math.Sin(k * Math.PI * 6)) * .35; break;
            case "idle":
                double idle = Math.Sin(k * 3.2); y = Math.Max(0, idle) * .035; rz = Math.Sin(k * 1.6) * .06;
                double idleScale = 1 + .035 * idle; sx = sz = scale / Math.Sqrt(idleScale); sy = scale * idleScale; break;
            case "shake": z = -.15 * sine; rz = Math.Sin(k * Math.PI * 10) * .15 * (1 - k); break;
        }
        return new(new(x, y, z), new(rx, ry, rz), new(sx, sy, sz));
    }
}

using DigiTactics.Rules;

namespace DigiTactics.Presentation;

/// <summary>
/// Equipment beam, area burst and shield cues from render3d.js. These follow the
/// combat clock, while signature moves use the independent MoveEffects timeline.
/// </summary>
public sealed class LegacyCombatEffects(MoveEffects effects)
{
    private enum Cue { Beam, Burst, Shield }
    private readonly record struct Active(MoveMesh Mesh, Cue Cue, double Start, double Duration, double Radius = 0);
    private readonly List<Active> active = [];
    private static readonly MoveColor friendly = MoveColor.Hex("#6fd3ff"), hostile = MoveColor.Hex("#ff7a6a"), shield = MoveColor.Hex("#dfe9ff");
    public int ActiveCount => active.Count;

    // The source palette is checked against js/data.js by the reference exporter.
    public static MoveColor AttributeColor(string attribute) => MoveColor.Hex(attribute switch
    {
        "Vaccine" => "#4fb3ff",
        "Data" => "#4fe08a",
        "Virus" => "#c45cff",
        "Free" => "#c8c8c8",
        _ => throw new ArgumentOutOfRangeException(nameof(attribute), attribute, "Unknown combat attribute.")
    });

    public void Beam(WorldPosition from, WorldPosition to, MoveColor attributeColor, double combatTime)
    {
        var mesh = effects.Mesh("legacyBeam");
        double dx = to.X - from.X, dz = to.Z - from.Z;
        mesh.Color = attributeColor;
        mesh.Position.Set((from.X + to.X) / 2, 0.9, (from.Z + to.Z) / 2);
        mesh.Scale.Set(1, Math.Sqrt(dx * dx + dz * dz), 1);
        mesh.RotationOrder = "YXZ";
        mesh.Rotation.Set(Math.PI / 2, Math.Atan2(dx, dz), 0);
        active.Add(new(mesh, Cue.Beam, combatTime, 0.3));
    }

    public void Burst(WorldPosition center, double radius, int viewTeam, double combatTime)
    {
        var mesh = effects.Mesh("legacyBurst");
        mesh.Color = viewTeam == 0 ? friendly : hostile;
        mesh.Rotation.X = -Math.PI / 2;
        mesh.Position.Set(center.X, 0.3, center.Z);
        active.Add(new(mesh, Cue.Burst, combatTime, 0.45, radius));
    }

    public void Shield(WorldPosition point, double combatTime)
    {
        var mesh = effects.Mesh("legacyShield");
        mesh.Color = shield;
        mesh.Opacity = 0.4;
        mesh.Position.Set(point.X, 0.8, point.Z);
        active.Add(new(mesh, Cue.Shield, combatTime, 0.7));
    }

    public void Update(double combatTime)
    {
        int write = 0;
        for (int read = 0; read < active.Count; read++)
        {
            var item = active[read];
            double k = (combatTime - item.Start) / item.Duration;
            // Source updateFx removes future cues too, including after a rewind.
            if (k >= 1 || k < 0 || !item.Mesh.Visible) { effects.Free(item.Mesh); continue; }
            var mesh = item.Mesh;
            switch (item.Cue)
            {
                case Cue.Beam:
                    mesh.Opacity = 1 - k;
                    mesh.Scale.X = mesh.Scale.Z = 3 * (1 - k) + 0.5;
                    break;
                case Cue.Burst:
                    mesh.Scale.SetScalar(0.4 + k * (1 + item.Radius * 1.5));
                    mesh.Opacity = 1 - k;
                    break;
                case Cue.Shield:
                    mesh.Scale.SetScalar(0.9 + k * 0.3);
                    mesh.Opacity = 0.45 * (1 - k);
                    break;
            }
            active[write++] = item;
        }
        if (write < active.Count) active.RemoveRange(write, active.Count - write);
    }

    /// <summary>Call when clearing the shared effect engine or changing combat.</summary>
    public void Clear()
    {
        foreach (var item in active) effects.Free(item.Mesh);
        active.Clear();
    }
}

using System.Numerics;
using System.Text.RegularExpressions;

namespace DigiTactics.Assets;

public enum CompanionLocomotionKind { Authored, Grounded, Glide, Hop }

/// <summary>Conservative movement overlay for companions without an authored run clip.
/// Complete, paired leg chains articulate; deliberately airborne forms glide; other rigs keep the source hop.</summary>
public sealed class CompanionLocomotion
{
    private sealed record Leg(int Hip, int Knee, int Foot, int Side, int Pair, double Bend);
    private readonly GltfRig rig;
    private readonly Leg[] legs;
    private readonly int root;
    private double weight;
    public CompanionLocomotionKind Kind { get; }
    public bool UsesTravelHop => Kind == CompanionLocomotionKind.Hop;
    public int LegCount => legs.Length;
    public void Reset() => weight = 0;
    public static string SourcePolicy(string source) => Gliders.Contains(source) ? "glide" : ForelegSources.Contains(source) ? "forelegs" : "default";

    // These reviewed forms move through the air in their standing presentation. A leg rig
    // is not by itself permission to give an airborne dragon, fairy or insect a walking gait.
    private static readonly HashSet<string> Gliders = new(StringComparer.Ordinal)
    {
        "ryudamon:2", "hagurumon:0", "biyomon:1", "biyomon:2", "demidevimon:0", "demidevimon:1",
        "dracomon:2", "extra:Bakemon", "extra:DW1_Airdramon", "extra:DW1_Bakemon", "extra:DW1_Birdramon",
        "extra:DW1_Gigadramon", "extra:DW1_Vademon", "extra:Phoenixmon", "falcomon:2",
        "fanbeemon:0", "fanbeemon:1", "fanbeemon:2", "hawkmon:1", "lalamon:0", "lalamon:1", "lalamon:2",
        "lucemon:0", "machinedramon:0", "palmon:2", "patamon:1", "patamon:2", "salamon:2",
        "tentomon:0", "tentomon:1", "tentomon:2", "wormmon:1", "wormmon:2",
        // Same source assets or the same airborne forms in other catalog sets.
        "phoenixmon:0", "phoenixmon:1", "phoenixmon:2", "bakemon:0", "bakemon:1", "bakemon:2",
        "ac:Sunflowmon", "ac:Aquilamon", "drg:Sunflowmon", "drg:Aquilamon",
        "extra:DW1_Hagurumon", "extra:Vademon", "drg:Tentomon", "ac:Airdramon", "drg:Airdramon",
        "ac:Devimon", "ac:Gigadramon", "drg:Gigadramon", "catalog:CS_DemiDevimon:chr097",
        "catalog:CS_Hagurumon:chr009", "catalog:CS_Lalamon:chr056", "catalog:CS_Lucemon:chr390",
        "catalog:CS_Angemon:chr087", "catalog:CS_Bakemon:chr013", "catalog:CS_Birdramon:chr308",
        "catalog:CS_Stingmon:chr091", "catalog:L_Hagurumon:chr009", "catalog:L_Aquilamon:chr015",
        "catalog:L_Devimon:chr093", "catalog:L_Sunflowmon:chr058", "catalog:L_Waspmon:chr454",
        "catalog:L_MagnaAngemon:chr101", "catalog:TS_Funbeemon:chr042", "catalog:TS_Lalamon:chr056",
        "catalog:TS_Lucemon:chr390", "catalog:TS_PicoDevimon:chr097", "catalog:TS_Solarmon_Hagurumon:chr009",
        "catalog:TS_Tentomon:chr303", "ac:Dianamon", "ac:Flymon",
        "catalog:CS_Seadramon:chr347", "ac:Phantomon", "drg:Phantomon"
    };
    // These visually reviewed quadrupeds name their weight-bearing forelegs as arms.
    // Do not apply this rule to humanoids merely because they share the bone naming scheme.
    private static readonly HashSet<string> ForelegSources = new(StringComparer.Ordinal)
    { "ac:Dobermon", "bearmon:1", "ac:Grizzlymon" };
    private static readonly Regex HipName = new(@"(?:^|[_ ])(?:momo|thigh|upleg|leg)(?:0?1)?[_ ]?([lr])(?:[_ ]|$)|(?:^|[_ ])([lr])[_ ](?:momo|thigh|leg)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex KneeName = new(@"(?:^|[_ ])(?:hiza|knee\d*|shin|calf)(?:[_ ]?[lr])?(?:[_ ]|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex FootName = new(@"(?:^|[_ ])(?:kakato|foot\d*|ankle|ancle|unkle|asi)(?:[_ ]?[lr])?(?:[_ ]|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex ShoulderName = new(@"(?:^|[_ ])(?:upperarm|arm)[_ ]?([lr])(?:[_ ]|$)|(?:^|[_ ])([lr])[_ ](?:upperarm|arm)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex ElbowName = new(@"(?:^|[_ ])(?:elbow|forearm)(?:[_ ]?[lr])?(?:[_ ]|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex HandName = new(@"(?:^|[_ ])hand(?:[_ ]?[lr])?(?:[_ ]|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static string BoneName(string value) => Regex.Replace(value, @"_0\d+$", "", RegexOptions.CultureInvariant);

    public CompanionLocomotion(GltfRig rig, string source, RigPose idle, bool authored)
    {
        this.rig = rig; legs = []; root = 0;
        if (authored) { Kind = CompanionLocomotionKind.Authored; return; }
        string policy = SourcePolicy(source);
        if (policy == "glide") { Kind = CompanionLocomotionKind.Glide; return; }
        var joints = rig.Skins.SelectMany(s => s.Joints).ToHashSet();
        var candidates = new List<(int Hip, int Knee, int Foot, int Side, string Pair)>();
        bool Below(int node, int ancestor)
        {
            for (int parent = rig.Nodes[node].Parent; parent >= 0; parent = rig.Nodes[parent].Parent) if (parent == ancestor) return true;
            return false;
        }
        foreach (int hip in joints.Order())
        {
            string name = BoneName(rig.Nodes[hip].Name); var match = HipName.Match(name);
            bool foreleg = false;
            if (!match.Success && policy == "forelegs") { match = ShoulderName.Match(name); foreleg = match.Success; }
            if (!match.Success) continue;
            var side = match.Groups[1].Success ? match.Groups[1] : match.Groups[2];
            int knee = joints.Order().FirstOrDefault(i => (foreleg ? ElbowName : KneeName).IsMatch(BoneName(rig.Nodes[i].Name)) && Below(i, hip), -1);
            int foot = knee < 0 ? -1 : joints.Order().FirstOrDefault(i => (foreleg ? HandName : FootName).IsMatch(BoneName(rig.Nodes[i].Name)) && Below(i, knee), -1);
            if (foot < 0) continue;
            string pair = name[..side.Index].ToLowerInvariant() + "?" + name[(side.Index + side.Length)..].ToLowerInvariant();
            candidates.Add((hip, knee, foot, side.Value.Equals("l", StringComparison.OrdinalIgnoreCase) ? 1 : -1, pair));
        }
        var pairs = candidates.GroupBy(c => c.Pair).Where(g => g.Count() == 2 && g.Sum(c => c.Side) == 0).OrderBy(g => g.Key, StringComparer.Ordinal).ToArray();
        if (pairs.Length is < 1 or > 2) { Kind = CompanionLocomotionKind.Hop; return; }
        Vector3 Point(int node) { var m = idle.World[node]; return new((float)m.M41, (float)m.M42, (float)m.M43); }
        var first = pairs[0];
        Vector3 across = Point(first.Single(c => c.Side > 0).Hip) - Point(first.Single(c => c.Side < 0).Hip);
        if (across.LengthSquared() < 1e-16f) { Kind = CompanionLocomotionKind.Hop; return; }
        var lateral = Vector3.Normalize(across);
        var prepared = new List<Leg>();
        for (int pair = 0; pair < pairs.Length; pair++) foreach (var c in pairs[pair])
        {
            Vector3 thigh = Point(c.Knee) - Point(c.Hip), shin = Point(c.Foot) - Point(c.Knee);
            if (thigh.LengthSquared() < 1e-18f || shin.LengthSquared() < 1e-18f) { Kind = CompanionLocomotionKind.Hop; return; }
            double bend = Vector3.Dot(Vector3.Cross(thigh, shin), lateral) >= 0 ? 1 : -1;
            prepared.Add(new(c.Hip, c.Knee, c.Foot, c.Side, pair, bend));
        }
        legs = prepared.ToArray(); root = legs[0].Hip;
        while (rig.Nodes[root].Parent >= 0) root = rig.Nodes[root].Parent;
        if (legs.Any(leg => !Below(leg.Hip, root))) { legs = []; Kind = CompanionLocomotionKind.Hop; return; }
        Kind = CompanionLocomotionKind.Grounded;
    }

    public double Apply(RigPose pose, bool moving, double time, double delta)
    {
        weight = Math.Clamp(weight + (moving ? 1 : -1) * delta * 5, 0, 1);
        if (weight == 0 || Kind is CompanionLocomotionKind.Authored or CompanionLocomotionKind.Hop) return 0;
        if (Kind == CompanionLocomotionKind.Glide) return Math.Sin(time * 4) * .025 * weight;
        double floor = legs.Min(leg => pose.World[leg.Foot].M42);
        var left = pose.World[legs.Single(leg => leg.Pair == 0 && leg.Side > 0).Hip];
        var right = pose.World[legs.Single(leg => leg.Pair == 0 && leg.Side < 0).Hip];
        var lateral = Vector3.Normalize(new Vector3((float)(left.M41 - right.M41), (float)(left.M42 - right.M42), (float)(left.M43 - right.M43)));
        foreach (var leg in legs)
        {
            double phase = time * 8.5 + (leg.Side < 0 ? Math.PI : 0) + leg.Pair * Math.PI;
            double stride = Math.Sin(phase), hip = stride * .3 * weight;
            double knee = Math.Max(0, stride) * .28 * leg.Bend * weight;
            Turn(leg.Hip, hip); Turn(leg.Knee, knee); Turn(leg.Foot, -hip - knee);
        }
        // Keep the stance foot on its current idle floor without changing limb lengths or root X/Z.
        double lift = floor - legs.Min(leg => pose.World[leg.Foot].M42);
        var body = pose.Local[root]; pose.Local[root] = body with { Translation = body.Translation with { Y = body.Translation.Y + lift } };
        pose.UpdateWorld();
        return 0;

        void Turn(int node, double angle)
        {
            int parent = rig.Nodes[node].Parent;
            var parentWorld = parent >= 0 ? pose.World[parent] : Matrix4D.Identity;
            if (!Matrix4D.Invert(parentWorld, out var inverse)) return;
            Vector3 axis = new((float)(lateral.X * inverse.M11 + lateral.Y * inverse.M21 + lateral.Z * inverse.M31),
                (float)(lateral.X * inverse.M12 + lateral.Y * inverse.M22 + lateral.Z * inverse.M32),
                (float)(lateral.X * inverse.M13 + lateral.Y * inverse.M23 + lateral.Z * inverse.M33));
            if (axis.LengthSquared() < 1e-20f) return;
            axis = Vector3.Normalize(axis); double s = Math.Sin(angle / 2);
            var a = new QuaternionD(axis.X * s, axis.Y * s, axis.Z * s, Math.Cos(angle / 2));
            var value = pose.Local[node]; var b = value.Rotation;
            var rotation = new QuaternionD(a.X * b.W + a.W * b.X + a.Y * b.Z - a.Z * b.Y,
                a.Y * b.W + a.W * b.Y + a.Z * b.X - a.X * b.Z,
                a.Z * b.W + a.W * b.Z + a.X * b.Y - a.Y * b.X,
                a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z);
            pose.Local[node] = value with { Rotation = rotation }; pose.UpdateWorld();
        }
    }
}

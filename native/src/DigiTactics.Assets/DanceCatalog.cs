using System.Numerics;
using System.Text.Json;

namespace DigiTactics.Assets;

/// <summary>Original bone-role analysis exported for each companion source, indexed by native glTF node.</summary>
public sealed class DanceCatalog
{
    private readonly Dictionary<string, DanceBinding> bindings = [];
    public DanceBinding Binding(string source) => bindings.TryGetValue(source, out var binding) ? binding : throw new InvalidDataException("Missing dance binding: " + source);
    public static DanceCatalog Load(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        if (root.GetProperty("version").GetInt32() != 1) throw new InvalidDataException("Unsupported dance binding version.");
        static Vector3 V(JsonElement e) => new(e[0].GetSingle(), e[1].GetSingle(), e[2].GetSingle());
        static Quaternion Q(JsonElement e) => new(e[0].GetSingle(), e[1].GetSingle(), e[2].GetSingle(), e[3].GetSingle());
        var catalog = new DanceCatalog();
        foreach (var entry in root.GetProperty("bindings").EnumerateObject())
        {
            var b = entry.Value;
            var bones = b.GetProperty("bones").EnumerateArray().Select(e => new DanceBone(
                e.GetProperty("node").GetInt32(), e.GetProperty("kind").GetString()!, e.GetProperty("parentEntry").GetInt32(),
                V(e.GetProperty("direction")), Q(e.GetProperty("parentRotation")), Q(e.GetProperty("restRotation")),
                e.GetProperty("side").GetInt32(), e.GetProperty("share").GetSingle(), e.GetProperty("index").GetInt32(), e.GetProperty("limit").GetSingle())).ToArray();
            if (bones.Where((bone, i) => bone.Node < 0 || bone.ParentEntry < -1 || bone.ParentEntry >= i).Any() || bones.Select(bone => bone.Node).Distinct().Count() != bones.Length)
                throw new InvalidDataException("Invalid dance bone hierarchy: " + entry.Name);
            catalog.bindings.Add(entry.Name, new(bones, b.GetProperty("limbless").GetBoolean(), b.GetProperty("stubby").GetBoolean(), b.GetProperty("yaw").GetSingle()));
        }
        return catalog;
    }
}

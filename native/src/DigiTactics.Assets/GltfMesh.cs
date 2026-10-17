namespace DigiTactics.Assets;

/// <summary>Shared interleaved geometry: position, normal, UV, joints, normalized weights.</summary>
public sealed record GltfPrimitive(float[] Vertices, uint[] Indices, int Material, bool HasNormals, bool HasSkin)
{
    public const int Stride = 16;
    public int VertexCount => Vertices.Length / Stride;
}
public sealed record MeshInstance(int Node, int Mesh, int Skin);
public sealed record GltfMesh(GltfPrimitive[][] Meshes, MeshInstance[] Instances)
{
    public static GltfMesh Load(GltfAsset asset)
    {
        var root = asset.Root;
        var meshes = new List<GltfPrimitive[]>();
        foreach (var mesh in root.GetProperty("meshes").EnumerateArray())
        {
            var primitives = new List<GltfPrimitive>();
            foreach (var primitive in mesh.GetProperty("primitives").EnumerateArray())
            {
                if (primitive.TryGetProperty("mode", out var mode) && mode.GetInt32() != 4)
                    throw new NotSupportedException("Expected triangle primitives.");
                var attributes = primitive.GetProperty("attributes");
                int position = attributes.GetProperty("POSITION").GetInt32();
                int count = root.GetProperty("accessors")[position].GetProperty("count").GetInt32();
                var vertices = new float[checked(count * GltfPrimitive.Stride)];
                bool normal = attributes.TryGetProperty("NORMAL", out _), skin = attributes.TryGetProperty("JOINTS_0", out _);
                foreach (var attribute in new[] { ("POSITION", 0, 3), ("NORMAL", 3, 3), ("TEXCOORD_0", 6, 2), ("JOINTS_0", 8, 4), ("WEIGHTS_0", 12, 4) })
                {
                    if (!attributes.TryGetProperty(attribute.Item1, out var accessor)) continue;
                    int index = accessor.GetInt32();
                    if (root.GetProperty("accessors")[index].GetProperty("count").GetInt32() != count) throw new InvalidDataException("Mesh attribute counts differ.");
                    for (int vertex = 0; vertex < count; vertex++)
                    {
                        var values = asset.AccessorElement(index, vertex);
                        if (values.Length != attribute.Item3) throw new InvalidDataException("Invalid mesh attribute shape.");
                        for (int c = 0; c < values.Length; c++)
                        {
                            float value = (float)values[c];
                            if (!float.IsFinite(value)) throw new InvalidDataException("Non-finite mesh attribute.");
                            vertices[vertex * GltfPrimitive.Stride + attribute.Item2 + c] = value;
                        }
                    }
                }
                for (int vertex = 0; vertex < count; vertex++)
                {
                    int at = vertex * GltfPrimitive.Stride + 12;
                    double sum = 0; for (int c = 0; c < 4; c++) sum += Math.Abs(vertices[at + c]);
                    if (sum == 0) vertices[at] = 1;
                    else for (int c = 0; c < 4; c++) vertices[at + c] = (float)(vertices[at + c] / sum);
                }
                uint[] indices;
                if (primitive.TryGetProperty("indices", out var indexAccessor))
                {
                    int accessor = indexAccessor.GetInt32();
                    indices = new uint[root.GetProperty("accessors")[accessor].GetProperty("count").GetInt32()];
                    for (int i = 0; i < indices.Length; i++) indices[i] = checked((uint)asset.AccessorElement(accessor, i)[0]);
                }
                else indices = Enumerable.Range(0, count).Select(i => (uint)i).ToArray();
                if (indices.Length % 3 != 0 || indices.Any(i => i >= count)) throw new InvalidDataException("Invalid triangle indices.");
                primitives.Add(new(vertices, indices, primitive.TryGetProperty("material", out var material) ? material.GetInt32() : -1, normal, skin));
            }
            meshes.Add(primitives.ToArray());
        }
        var instances = new List<MeshInstance>();
        var visited = new HashSet<int>();
        void Visit(int index)
        {
            if (!visited.Add(index)) throw new InvalidDataException("Repeated scene node.");
            var node = root.GetProperty("nodes")[index];
            if (node.TryGetProperty("mesh", out var mesh)) instances.Add(new(index, mesh.GetInt32(), node.TryGetProperty("skin", out var skin) ? skin.GetInt32() : -1));
            if (node.TryGetProperty("children", out var children)) foreach (var child in children.EnumerateArray()) Visit(child.GetInt32());
        }
        int scene = root.TryGetProperty("scene", out var sceneIndex) ? sceneIndex.GetInt32() : 0;
        foreach (var node in root.GetProperty("scenes")[scene].GetProperty("nodes").EnumerateArray()) Visit(node.GetInt32());
        return new(meshes.ToArray(), instances.ToArray());
    }

    public (Vector3D Min, Vector3D Max) Bounds(GltfRig rig, RigPose pose, bool precise = true)
    {
        var min = new Vector3D(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
        var max = new Vector3D(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
        foreach (var instance in Instances)
        {
            var palette = instance.Skin >= 0 ? new Matrix4D[rig.Skins[instance.Skin].Joints.Length] : [];
            if (instance.Skin >= 0) pose.SkinMatrices(instance.Skin, instance.Node, palette);
            foreach (var primitive in Meshes[instance.Mesh])
            {
                var localMin = new Vector3D(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
                var localMax = new Vector3D(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
                for (int vertex = 0; vertex < primitive.VertexCount; vertex++)
                {
                    var values = primitive.Vertices.AsSpan(vertex * GltfPrimitive.Stride, GltfPrimitive.Stride);
                    var point = new Vector3D(values[0], values[1], values[2]);
                    if (palette.Length > 0 && primitive.HasSkin)
                    {
                        var skinned = new Vector3D();
                        for (int c = 0; c < 4; c++)
                        {
                            var p = Transform(point, palette[checked((int)values[8 + c])]); double weight = values[12 + c];
                            skinned = new(skinned.X + p.X * weight, skinned.Y + p.Y * weight, skinned.Z + p.Z * weight);
                        }
                        point = skinned;
                    }
                    if (precise) Include(Transform(point, pose.World[instance.Node]));
                    else
                    {
                        localMin = new(Math.Min(localMin.X, point.X), Math.Min(localMin.Y, point.Y), Math.Min(localMin.Z, point.Z));
                        localMax = new(Math.Max(localMax.X, point.X), Math.Max(localMax.Y, point.Y), Math.Max(localMax.Z, point.Z));
                    }
                }
                if (!precise) for (int corner = 0; corner < 8; corner++)
                    Include(Transform(new((corner & 1) == 0 ? localMin.X : localMax.X, (corner & 2) == 0 ? localMin.Y : localMax.Y,
                        (corner & 4) == 0 ? localMin.Z : localMax.Z), pose.World[instance.Node]));
            }
        }
        return (min, max);
        void Include(Vector3D point)
        {
            min = new(Math.Min(min.X, point.X), Math.Min(min.Y, point.Y), Math.Min(min.Z, point.Z));
            max = new(Math.Max(max.X, point.X), Math.Max(max.Y, point.Y), Math.Max(max.Z, point.Z));
        }
    }
    private static Vector3D Transform(Vector3D v, Matrix4D m) => new(v.X*m.M11+v.Y*m.M21+v.Z*m.M31+m.M41,
        v.X*m.M12+v.Y*m.M22+v.Z*m.M32+m.M42, v.X*m.M13+v.Y*m.M23+v.Z*m.M33+m.M43);
}

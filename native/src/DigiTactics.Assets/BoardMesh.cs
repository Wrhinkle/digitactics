namespace DigiTactics.Assets;

/// <summary>Converts one Three material group to native interleaved geometry without changing its winding.</summary>
public sealed record BoardMesh(int Material, GltfPrimitive Primitive)
{
    public static BoardMesh[] Create(BoardScene scene, BoardObject obj)
    {
        if (obj.Kind is not ("mesh" or "lines" or "lineStrip" or "points")) throw new ArgumentException("Expected board geometry.", nameof(obj));
        var geometry = scene.Geometries[obj.Geometry];
        int count = geometry.Position.Length / 3;
        var indices = geometry.Indices ?? Enumerable.Range(0, count).Select(i => (uint)i).ToArray();
        var groups = obj.Materials.Length == 1 ? [new BoardGeometryGroup(0, indices.Length, 0)] : geometry.Groups;
        return groups.Select(group =>
        {
            int materialIndex = obj.Materials[group.MaterialIndex];
            var material = scene.Materials[materialIndex];
            var texture = material.Map is { } map ? scene.Textures[map] : null;
            var vertices = new float[count * GltfPrimitive.Stride];
            for (int i = 0; i < count; i++)
            {
                int at = i * GltfPrimitive.Stride;
                Array.Copy(geometry.Position, i * 3, vertices, at, 3);
                if (geometry.Normal is not null) Array.Copy(geometry.Normal, i * 3, vertices, at + 3, 3);
                if (geometry.Uv is not null)
                {
                    double u = geometry.Uv[i * 2], v = geometry.Uv[i * 2 + 1];
                    if (texture is not null)
                    {
                        double c = Math.Cos(texture.Rotation), s = Math.Sin(texture.Rotation);
                        double x = u - texture.Center[0], y = v - texture.Center[1];
                        u = texture.Repeat[0] * (c * x + s * y) + texture.Center[0] + texture.Offset[0];
                        v = texture.Repeat[1] * (-s * x + c * y) + texture.Center[1] + texture.Offset[1];
                    }
                    vertices[at + 6] = (float)u; vertices[at + 7] = (float)v;
                }
                vertices[at + 12] = 1;
            }
            return new BoardMesh(materialIndex, new(vertices, indices.AsSpan(group.Start, group.Count).ToArray(), materialIndex, geometry.Normal is not null, false)
            { Colors = material.VertexColors ? geometry.Color ?? [] : [] });
        }).ToArray();
    }

    public static PngImage Image(BoardTexture texture)
    {
        var bytes = Convert.FromBase64String(texture.Rgba);
        if (texture.FlipY)
        {
            int stride = texture.Width * 4;
            var row = new byte[stride];
            for (int y = 0; y < texture.Height / 2; y++)
            {
                int top = y * stride, bottom = (texture.Height - y - 1) * stride;
                Array.Copy(bytes, top, row, 0, stride); Array.Copy(bytes, bottom, bytes, top, stride); Array.Copy(row, 0, bytes, bottom, stride);
            }
        }
        return new(texture.Width, texture.Height, bytes);
    }

    public static int Wrap(int threeWrap) => threeWrap switch
    {
        1000 => 10497, 1001 => 33071, 1002 => 33648,
        _ => throw new InvalidDataException("Unsupported board texture wrap mode.")
    };
}

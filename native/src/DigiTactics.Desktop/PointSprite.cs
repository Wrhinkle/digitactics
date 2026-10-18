using System.Numerics;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

internal sealed record PointSprite(float Size, int ViewportHeight, bool Attenuate, Vector4 U, Vector4 V)
{
    public static PointSprite Board(BoardMaterial material, BoardTexture? texture, int height,float? size=null)
    {
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (texture is null) return new(size??material.Size, height, material.SizeAttenuation, new(1,0,0,0), new(0,1,0,0));
        float c = MathF.Cos(texture.Rotation), s = MathF.Sin(texture.Rotation);
        float sx = texture.Repeat[0], sy = texture.Repeat[1], cx = texture.Center[0], cy = texture.Center[1];
        return new(size??material.Size, height, material.SizeAttenuation,
            new(sx*c, sx*s, cx+texture.Offset[0]-sx*(c*cx+s*cy), 0),
            new(-sy*s, sy*c, cy+texture.Offset[1]-sy*(-s*cx+c*cy), 0));
    }
}

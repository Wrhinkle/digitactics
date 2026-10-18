using System.Numerics;
using System.Runtime.CompilerServices;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

internal sealed partial class ModelRenderer
{
    private readonly record struct TransparentDraw(int Order, double Depth, int Sequence, bool ReceiveShadow, Action Draw);
    private readonly List<TransparentDraw> transparentDraws = [];
    private readonly ConditionalWeakTable<RigPose, Dictionary<(int Node, int Primitive), Vector3D>> sortCenters = new();
    private bool collecting;
    internal int PendingTransparentDraws => transparentDraws.Count;
    public void BeginTransparency()
    {
        if (collecting) throw new InvalidOperationException("Transparent collection is already active.");
        transparentDraws.Clear(); collecting = true;
    }
    public void ClearTransparency() { collecting = false; transparentDraws.Clear(); }
    public void FlushTransparency()
    {
        collecting = false;
        transparentDraws.Sort((a, b) => {
            int order = a.Order.CompareTo(b.Order);
            if (order != 0) return order;
            int depth = b.Depth.CompareTo(a.Depth);
            return depth != 0 ? depth : a.Sequence.CompareTo(b.Sequence);
        });
        bool receiveShadow = ReceiveShadow;
        try { foreach (var draw in transparentDraws) { ReceiveShadow = draw.ReceiveShadow; draw.Draw(); } }
        finally { ReceiveShadow = receiveShadow; transparentDraws.Clear(); }
    }
    private void Enqueue(int order, double depth, Action draw) => transparentDraws.Add(new(order, depth, transparentDraws.Count, ReceiveShadow, draw));
    private static double Depth(Vector3D center, Matrix4D world, Matrix4x4 camera)
    {
        var matrix = world * From(camera);
        return center.X * matrix.M13 + center.Y * matrix.M23 + center.Z * matrix.M33 + matrix.M43;
    }
    private void QueueModel(GpuModel model, RigPose pose, Matrix4D placement, Matrix4x4 camera, double emission)
    {
        DrawCalls = 0;
        var centers = sortCenters.GetOrCreateValue(pose);
        foreach (var instance in model.Mesh.Instances)
        for (int i = 0; i < model.Mesh.Meshes[instance.Mesh].Length; i++)
        {
            var geometry = model.Mesh.Meshes[instance.Mesh][i];
            if (!model.Materials[geometry.Material].Transparent) continue;
            var key = (instance.Node, i);
            if (!centers.TryGetValue(key, out var center))
            {
                // Like Three's cached bounding sphere, take the box center of the first displayed pose.
                Matrix4D[] skin = instance.Skin >= 0 && geometry.HasSkin ? new Matrix4D[model.Rig.Skins[instance.Skin].Joints.Length] : [];
                if (skin.Length > 0) pose.SkinMatrices(instance.Skin, instance.Node, skin);
                double minX=double.PositiveInfinity,minY=minX,minZ=minX,maxX=double.NegativeInfinity,maxY=maxX,maxZ=maxX;
                var v = geometry.Vertices;
                for (int at = 0; at < v.Length; at += GltfPrimitive.Stride)
                {
                    double x=v[at],y=v[at+1],z=v[at+2];
                    if (skin.Length > 0)
                    {
                        double sx=0,sy=0,sz=0;
                        for(int j=0;j<4;j++)
                        {
                            var m=skin[(int)v[at+8+j]]; double w=v[at+12+j];
                            sx+=(x*m.M11+y*m.M21+z*m.M31+m.M41)*w;
                            sy+=(x*m.M12+y*m.M22+z*m.M32+m.M42)*w;
                            sz+=(x*m.M13+y*m.M23+z*m.M33+m.M43)*w;
                        }
                        x=sx;y=sy;z=sz;
                    }
                    minX=Math.Min(minX,x);minY=Math.Min(minY,y);minZ=Math.Min(minZ,z);
                    maxX=Math.Max(maxX,x);maxY=Math.Max(maxY,y);maxZ=Math.Max(maxZ,z);
                }
                center = geometry.VertexCount == 0 ? default : new((minX+maxX)/2,(minY+maxY)/2,(minZ+maxZ)/2);
                centers.Add(key, center);
            }
            int primitive = i;
            Enqueue(0, Depth(center, pose.World[instance.Node] * placement, camera),
                () => Draw(model, pose, placement, camera, true, emission, instance.Node, primitive));
            DrawCalls++;
        }
    }
}

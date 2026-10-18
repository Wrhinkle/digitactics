using System.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

/// <summary>Opaque-alpha fixtures make draw order irrelevant, so queued mesh draws must
/// preserve skinning, materials and normal maps. Coplanar primitive order can select
/// a different color at an exactly equal depth; those pixels are counted separately.</summary>
internal static class ModelTransparencyChecks
{
    public static unsafe void Run(ModelCatalog catalog, CompanionCatalog companions, string normalSource)
    {
        using var renderer = new ModelRenderer { FarmLight = new FarmLighting(), FarmEye = new(0, 1.8f, 5) };
        var camera = Matrix4x4.CreateLookAt(renderer.FarmEye, new(0, .9f, 0), Vector3.UnitY)
            * Matrix4x4.CreatePerspectiveFieldOfView(.65f, 1, .01f, 100);
        int models = 0, primitives = 0, skinned = 0, multiMaterial = 0, equalDepthTies = 0;
        var samples = companions.Bases.Append(new KeyValuePair<string, CompanionVisual>("normal-map-vulcanusmon", companions.Bases.First().Value with { Source = normalSource }));
        foreach (var (id, visual) in samples)
        {
            using var model = new GpuModel(catalog, catalog.Sources[visual.Source]);
            for (int i = 0; i < model.Materials.Length; i++)
                model.Materials[i] = model.Materials[i] with { Transparent = true, DepthWrite = true, AlphaCutoff = .999, Color = model.Materials[i].Color with { A = 1 } };
            var animator = model.CreateCompanionAnimator(visual, companions.Fallbacks, height: 2);
            animator.SetMoving(true);
            if (model.Mesh.Instances.Any(i => i.Skin >= 0)) skinned++;
            if (model.Materials.Length > 1) multiMaterial++;
            for (int frame = 0; frame < 2; frame++)
            {
                animator.Update(.37);
                var placement = model.PlaceCompanion(animator);
                float[] referenceDepth = [], queuedDepth = [];
                byte[] Render(bool queued)
                {
                    ModelRenderer.Begin(128,128); OpenGl.ClearColor(new(0,0,0,1)); OpenGl.Clear(GlClearBit.Color);
                    if (queued) renderer.BeginTransparency();
                    renderer.Draw(model,animator.Pose,placement,camera,true);
                    if (queued) { primitives += renderer.PendingTransparentDraws; renderer.FlushTransparency(); }
                    ModelRenderer.End();
                    var pixels = new byte[128*128*4];
                    fixed(byte* p=pixels) OpenGl.ReadPixels(0,0,128,128,OpenGl.GL_RGBA,OpenGl.GL_UNSIGNED_BYTE,p);
                    var depth = new float[128*128];
                    fixed(float* p=depth) OpenGl.ReadPixels(0,0,128,128,OpenGl.GL_DEPTH_COMPONENT,OpenGl.GL_FLOAT,p);
                    if (queued) queuedDepth = depth; else referenceDepth = depth;
                    return pixels;
                }
                var immediate = Render(false); var deferred = Render(true);
                int changed = 0, visible = 0;
                for (int p = 0; p < immediate.Length; p += 4)
                {
                    if (Math.Abs(referenceDepth[p/4] - queuedDepth[p/4]) > 1e-6f)
                        throw new InvalidOperationException($"Queued model depth mismatch for {id}, frame {frame}, pixel {p/4}.");
                    if (immediate[p]+immediate[p+1]+immediate[p+2]>30) visible++;
                    if (Math.Abs(immediate[p]-deferred[p])+Math.Abs(immediate[p+1]-deferred[p+1])+Math.Abs(immediate[p+2]-deferred[p+2])>3)
                    {
                        if (referenceDepth[p/4] == queuedDepth[p/4]) equalDepthTies++;
                        else changed++;
                    }
                }
                if (visible < 40 || changed > 0) throw new InvalidOperationException($"Queued model mismatch for {id}, frame {frame}: {changed} differing / {visible} visible pixels.");
            }
            models++;
        }
        if (skinned == 0 || multiMaterial == 0 || primitives < models * 2) throw new InvalidOperationException("Queued model fixtures omitted skinning or multi-material geometry.");
        // The current corpus has one coplanar tie in mystery_daily_legend_7's second pose.
        // Do not let equal-depth differences hide a material or texture regression.
        if (equalDepthTies > 1) throw new InvalidOperationException($"Queued model coplanar differences grew to {equalDepthTies}; inspect before accepting.");
        Console.WriteLine($"Queued model GPU equivalence: {models} models, {skinned} skinned, {multiMaterial} multi-material, {primitives} primitive draws across two animated poses; {equalDepthTies} color differences restricted to exact depth ties.");
    }
}

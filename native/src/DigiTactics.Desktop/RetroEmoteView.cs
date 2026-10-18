using System.Text.Json;
using DigiTactics.Assets;

namespace DigiTactics.Desktop;

internal sealed class RetroEmoteView : IDisposable
{
    private readonly GpuModel model;
    private readonly RigPose pose;
    private readonly string action;
    private readonly double scale, yaw;
    private readonly long started;
    private readonly RetroParticles particles;
    private readonly RetroParticleRenderer effects;
    private RetroEmotePose? motion;
    private Matrix4D placement;
    internal int PreparedFrames { get; private set; }
    internal RetroEmotePose? Motion => motion;
    public int EffectDrawCalls { get; private set; }
    public bool Expired(long now) => now - started >= 3000;
    public RetroEmoteView(ModelCatalog catalog, JsonElement data, long now)
    {
        var source = catalog.Sources[data.GetProperty("model"u8).GetString()!];
        model = new(catalog, source); pose = new(model.Rig); pose.Sample(ModelAnimator.FindIdle(model.Rig), 0);
        action = data.GetProperty("act"u8).GetString()!; started = now;
        bool glyph=data.TryGetProperty("glyph"u8,out _);
        particles=new(action,glyph);effects=new(action,glyph);
        scale = Math.Min(data.TryGetProperty("size"u8, out var size) ? size.GetDouble() : .7, 1.05 / Math.Max(model.Footprint, .01));
        yaw = source.Metadata.TryGetProperty("rotY"u8, out var rotation) ? rotation.GetDouble() : 0;
    }
    public void Prepare(System.Numerics.Matrix4x4 camera, System.Numerics.Vector3 eye, Vector3D companion, long now)
    {
        // The view-projection's first column is camera-right times the horizontal projection scale.
        var right = System.Numerics.Vector3.Normalize(new(camera.M11, camera.M21, camera.M31));
        motion = RetroEmoteMotion.Evaluate(action, (now-started)/1000d, scale, yaw, companion, new(eye.X,eye.Y,eye.Z), new(right.X,right.Y,right.Z));
        particles.Update((now-started)/1000d,motion);EffectDrawCalls=0;
        PreparedFrames++;
        if (motion is not null) placement = model.Normalization * motion.Model.Matrix * motion.Holder.Matrix;
    }
    public int DrawPass(ModelRenderer renderer, System.Numerics.Matrix4x4 camera, bool transparent)
    {
        if (motion is null) return 0;
        renderer.Draw(model,pose,placement,camera,transparent);
        int draws = renderer.DrawCalls;
        if (transparent && !renderer.ShadowPass)
        {
            EffectDrawCalls=effects.Draw(renderer,camera,particles,motion,action=="ufo");
            draws+=EffectDrawCalls;
        }
        return draws;
    }
    public int Draw(ModelRenderer renderer, System.Numerics.Matrix4x4 camera, System.Numerics.Vector3 eye, Vector3D companion, long now)
    {
        Prepare(camera,eye,companion,now);
        return DrawPass(renderer,camera,false) + DrawPass(renderer,camera,true);
    }
    public void Dispose() {effects.Dispose();model.Dispose();}
}

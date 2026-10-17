using System.Numerics;
using DigiTactics.Assets;
using Brutal.OpenGlApi;
namespace DigiTactics.Desktop;

internal sealed class ModelPreview : IDisposable
{
    private readonly ModelCatalog catalog = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
    private readonly ModelRenderer renderer = new();
    private GpuModel? model;
    private ModelAnimator? animator;
    private double? lastTime;
    public int DrawCalls { get; private set; }
    public bool Play(string role) => animator?.Play(role) ?? false;
    public void EndAction() => animator?.EndAction();
    public void Select(string variant, string line, int stage)
    {
        var source = catalog.Resolve(variant, line, stage);
        if (source == model?.Source) return;
        model?.Dispose(); model = null; animator = null; lastTime = null;
        if (source is null) return;
        model = new(catalog, source); animator = new(model.Rig);
    }
    public void Draw(int width, int height, double time)
    {
        DrawCalls = 0; if (model is null || animator is null || width <= 0 || height <= 0) return;
        animator.Update(lastTime is { } previous ? Math.Max(0, time - previous) : 0); lastTime = time;
        var camera = Matrix4x4.CreateLookAt(new(3, 2, 5), new(0, .9f, 0), Vector3.UnitY) * Matrix4x4.CreatePerspectiveFieldOfView(.65f, (float)width / height, .01f, 100);
        ModelRenderer.Begin(width, height);
        foreach (bool transparent in new[] { false, true }) { renderer.Draw(model, animator.Pose, model.PreviewPlacement, camera, transparent); DrawCalls += renderer.DrawCalls; }
        ModelRenderer.End();
    }
    public void Dispose() { model?.Dispose(); renderer.Dispose(); }
    public static unsafe void VerifyVisible(int width, int height)
    {
        var bytes = new byte[checked(width * height * 4)];
        fixed (byte* pixels = bytes) OpenGl.ReadPixels(0, 0, width, height, OpenGl.GL_RGBA, OpenGl.GL_UNSIGNED_BYTE, pixels);
        int visible = 0;
        for (int i = 0; i < bytes.Length; i += 4)
            if (bytes[i] > 30 || bytes[i + 1] > 30 || bytes[i + 2] > 40) visible++;
        if (visible < 200) throw new InvalidOperationException("Native model produced no visible surface.");
        Console.WriteLine($"Native model smoke passed: {visible} visible pixels.");
    }

}

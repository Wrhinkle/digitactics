using System.Numerics;
using Brutal.OpenGlApi;

namespace DigiTactics.Desktop;

internal static class CompanionArenaChecks
{
    public static unsafe void Run(ModelRenderer renderer)
    {
        using var companion = new ArenaCompanionView();
        companion.Update("tokomon", null, 0, 0);
        using var arena = new ArenaEnvironment(); arena.SetTheme("file-island");
        var eye = new Vector3(-7.6f, 4.5f, 13.4f);
        var camera = Matrix4x4.CreateLookAt(eye, new(-7.6f, 0, 8.4f), Vector3.UnitY)
            * Matrix4x4.CreatePerspectiveFieldOfView(.72f, 1, .1f, 100);
        byte[] Render(bool shadows, bool light)
        {
            companion.BeginFrame();
            arena.Actors = (shared, view, transparent) =>
            {
                if (shared.ShadowPass && !shadows) return;
                var boardLight = shared.BoardLight;
                try
                {
                    if (!light) shared.BoardLight = null;
                    companion.DrawModel(shared, view, transparent);
                }
                finally { shared.BoardLight = boardLight; }
            };
            arena.Draw(renderer, camera, eye, 7, true, 0, 0, 256, 256, (_, _) => { });
            var pixels = new byte[256 * 256 * 4];
            fixed (byte* p = pixels) OpenGl.ReadPixels(0, 0, 256, 256, OpenGl.GL_RGBA, OpenGl.GL_UNSIGNED_BYTE, p);
            return pixels;
        }
        var lit = Render(true, true);
        if (companion.DrawCalls == 0 || companion.ShadowDrawCalls == 0) throw new InvalidOperationException("Arena companion did not render in color and shadow passes.");
        var noShadow = Render(false, true);
        var noLight = Render(false, false);
        int shadowPixels = 0, lightingPixels = 0;
        for (int i = 0; i < lit.Length; i += 4)
        {
            if (noShadow[i] + noShadow[i + 1] + noShadow[i + 2] - lit[i] - lit[i + 1] - lit[i + 2] > 12) shadowPixels++;
            if (Math.Abs(noShadow[i] - noLight[i]) + Math.Abs(noShadow[i + 1] - noLight[i + 1]) + Math.Abs(noShadow[i + 2] - noLight[i + 2]) > 12) lightingPixels++;
        }
        if (shadowPixels < 5 || lightingPixels < 20) throw new InvalidOperationException($"Companion lighting/shadow pixels missing: {lightingPixels}/{shadowPixels}.");
        Console.WriteLine($"Companion arena GPU: {lightingPixels} lighting pixels and {shadowPixels} shadow pixels verified.");
        renderer.BoardLight = null; renderer.ShadowTexture = 0; renderer.ReceiveShadow = false;
    }
}

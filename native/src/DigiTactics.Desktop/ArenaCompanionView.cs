using System.Text.Json;
using DigiTactics.Assets;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed class ArenaCompanionView(bool opponent = false) : IDisposable
{
    private readonly ModelCatalog models = ModelCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/models.json"));
    private readonly CompanionCatalog companions = CompanionCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/companion-models.json"));
    private readonly JsonDocument emoteData = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "content/emotes/manifest.json")));
    private readonly ModelRenderer renderer = new();
    private GpuModel? model;
    private CompanionAnimator? animator;
    private string? key, move;
    private ActiveEmote? previous;
    private double duration;
    private CompanionTransform? overlay;
    public int DrawCalls { get; private set; }
    public CompanionTravel Travel { get; } = new();
    public void Update(string nextKey, ActiveEmote? emote, long now, double delta)
    {
        if (key != nextKey)
        {
            var visual = companions.Resolve(nextKey);
            var nextModel = new GpuModel(models, models.Sources[visual.Source]);
            CompanionAnimator nextAnimator;
            try { nextAnimator = nextModel.CreateCompanionAnimator(visual, companions.Fallbacks, yaw: Math.PI); }
            catch { nextModel.Dispose(); throw; }
            model?.Dispose(); model = nextModel; animator = nextAnimator;
            key = nextKey; previous = null; move = null;
        }
        if (emote is not null && emote != previous)
        {
            previous = emote; move = null;
            var data = emoteData.RootElement.GetProperty("clips"u8).GetProperty(emote.Id).GetProperty("data"u8);
            string? role = data.TryGetProperty("role"u8, out var r) ? r.GetString() : null;
            if (role is not null)
            {
                animator!.Play(role); bool handled = animator.Role is not null;
                string? explicitMove = data.TryGetProperty("move"u8, out var m) ? m.GetString() : null;
                move = explicitMove ?? (!handled ? role : null); duration = explicitMove is null ? 1600 : 2600;
            }
        }
        if (!opponent) Travel.Update(delta);
        animator!.Yaw = opponent ? 0 : Travel.Yaw; animator.SetMoving(!opponent && Travel.Moving); animator.Update(delta);
        overlay = previous is not null && move is not null ? CompanionEmoteMotion.Evaluate(move, now - previous.Started, duration) : null;
    }
    public void Draw(int left, int bottom, int width, int height, int columns)
    {
        DrawCalls = 0; if (model is null || animator is null) return;
        ModelRenderer.Begin(width, height, false); Brutal.OpenGlApi.OpenGl.Viewport(left, bottom, width, height);
        var camera = BattleView.Camera(width, height, columns);
        var placement = model.PlaceCompanion(animator, opponent ? 7.6 : Travel.X, opponent ? 0 : Travel.Hop(animator.HasRun), opponent ? -6.6 : Travel.Z, overlay);
        foreach (bool transparent in new[] { false, true }) { renderer.Draw(model, animator.Pose, placement, camera, transparent); DrawCalls += renderer.DrawCalls; }
        ModelRenderer.End();
    }
    public void Dispose() { model?.Dispose(); emoteData.Dispose(); renderer.Dispose(); }
}

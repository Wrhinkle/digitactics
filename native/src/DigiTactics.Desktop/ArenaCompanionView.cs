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
    private static readonly Lazy<DanceCatalog> Dances = new(() => DanceCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content/dance-bindings.json")));
    private DanceSkeleton? dancer;
    private DanceRoot? danceRoot;
    private string? danceId;
    private long danceStarted;
    public bool Dancing => danceRoot is not null;
    private GpuModel? model;
    private RetroEmoteView? retro;
    private long frameNow;
    public int RetroDrawCalls { get; private set; }
    private CompanionAnimator? animator;
    private string? key, move;
    private ActiveEmote? previous;
    private double duration;
    private double height;
    private CompanionTransform? overlay;
    public int DrawCalls { get; private set; }
    public CompanionTravel Travel { get; } = new();
    public void Update(string nextKey, ActiveEmote? emote, long now, double delta, string? equippedDance = null)
    {
        frameNow = now;
        if (retro?.Expired(now) == true) { retro.Dispose(); retro = null; }
        if (key != nextKey)
        {
            var visual = companions.Resolve(nextKey);
            height = 1.2 * visual.Height;
            var nextModel = new GpuModel(models, models.Sources[visual.Source]);
            CompanionAnimator nextAnimator;
            try { nextAnimator = nextModel.CreateCompanionAnimator(visual, companions.Fallbacks, yaw: Math.PI); }
            catch { nextModel.Dispose(); throw; }
            model?.Dispose(); model = nextModel; animator = nextAnimator;
            dancer = new(Dances.Value.Binding(visual.Source)); danceId = null; danceRoot = null;
            key = nextKey; previous = null; move = null;
        }
        if (emote is not null && emote != previous)
        {
            previous = emote; move = null;
            var data = emoteData.RootElement.GetProperty("clips"u8).GetProperty(emote.Id).GetProperty("data"u8);
            if (data.TryGetProperty("retro"u8, out var retroData))
            {
                var nextRetro = new RetroEmoteView(models, retroData, now);
                retro?.Dispose(); retro = nextRetro;
            }
            string? role = data.TryGetProperty("role"u8, out var r) ? r.GetString() : null;
            if (role is not null)
            {
                bool handled;
                if (role == "dance") { danceId = equippedDance ?? "floss"; danceStarted = now; handled = true; }
                else { animator!.Play(role); handled = animator.Role is not null; }
                string? explicitMove = data.TryGetProperty("move"u8, out var m) ? m.GetString() : null;
                move = explicitMove ?? (!handled ? role : null); duration = explicitMove is null ? 1600 : 2600;
            }
        }
        if (!opponent) Travel.Update(delta);
        animator!.Yaw = opponent ? 0 : Travel.Yaw; animator.SetMoving(!opponent && Travel.Moving); animator.Update(delta);
        danceRoot = null;
        if (danceId is not null)
        {
            double t = (now - danceStarted) / 1000d, length = DanceChoreography.Duration(danceId);
            if (t >= length || animator.Moving) danceId = null;
            else danceRoot = dancer!.Apply(DanceChoreography.Evaluate(danceId, t), animator.Pose, (float)Math.Min(1, Math.Min(t / .25, (length - t) / .3)));
        }
        overlay = previous is not null && move is not null ? CompanionEmoteMotion.Evaluate(move, now - previous.Started, duration) : null;
    }
    public System.Numerics.Vector3 Anchor => new((float)(opponent ? 7.6 : Travel.X),
        (float)(height + .3 + (opponent ? 0 : Travel.Hop(animator?.HasRun ?? false))), (float)(opponent ? -6.6 : Travel.Z));
    public void Draw(int left, int bottom, int width, int height, int columns)
    {
        DrawCalls = 0; RetroDrawCalls = 0; if (model is null || animator is null) return;
        ModelRenderer.Begin(width, height, false); Brutal.OpenGlApi.OpenGl.Viewport(left, bottom, width, height);
        var camera = BattleView.Camera(width, height, columns);
        var placement = model.PlaceCompanion(animator, opponent ? 7.6 : Travel.X, opponent ? 0 : Travel.Hop(animator.HasRun), opponent ? -6.6 : Travel.Z, overlay, danceRoot);
        foreach (bool transparent in new[] { false, true }) { renderer.Draw(model, animator.Pose, placement, camera, transparent); DrawCalls += renderer.DrawCalls; }
        RetroDrawCalls = retro?.Draw(renderer, camera, BattleView.CameraEye(width,height,columns),
            new(opponent?7.6:Travel.X,opponent?0:Travel.Hop(animator.HasRun),opponent?-6.6:Travel.Z),frameNow) ?? 0;
        ModelRenderer.End();
    }
    public void Dispose() { retro?.Dispose(); model?.Dispose(); emoteData.Dispose(); renderer.Dispose(); }
}

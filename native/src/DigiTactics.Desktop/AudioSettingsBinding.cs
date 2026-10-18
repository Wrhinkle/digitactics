using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Debounces slider writes; flushes the last edit on normal app exit.</summary>
internal sealed class AudioSettingsBinding : IDisposable
{
    private readonly AudioSettingsStore store;
    private readonly GameAudio audio;
    private readonly NativeProfileStore profile;
    private AudioSettings observed = new();
    private bool dirty;
    private double quiet;
    private double importRetry;
    private AudioSettings Snapshot => new(audio.MusicVolume, audio.EffectsVolume, audio.Muted, audio.MusicOff, audio.BoardVolume, audio.MuteOthers);
    public AudioSettingsBinding(GameAudio audio, NativeProfileStore profile, string directory)
    {
        this.audio = audio; this.profile = profile;
        store = new(Path.Combine(directory, "audio-settings.json"));
        store.ImportLegacy(profile.Storage);
        Apply();
    }
    private void Apply()
    {
        observed = store.Settings;
        audio.MusicVolume = observed.Music; audio.EffectsVolume = observed.Effects;
        audio.Muted = observed.Muted; audio.MusicOff = observed.MusicOff; audio.SettingsError = store.Error;
        audio.BoardVolume = observed.Board; audio.MuteOthers = observed.MuteOthers;
    }
    public void Update(double delta)
    {
        var current = Snapshot;
        if (current != observed) { observed = current; dirty = true; quiet = 0; }
        else quiet += delta;
        if (dirty && quiet >= .35) Flush();
        importRetry -= delta;
        if (!dirty && !store.HasSavedSettings && !store.ReadOnly && importRetry <= 0)
        {
            importRetry = 5;
            if (store.ImportLegacy(profile.Storage)) Apply();
            else audio.SettingsError = store.Error;
        }
    }
    private void Flush()
    {
        dirty = !store.Save(Snapshot); audio.SettingsError = store.Error; quiet = 0;
    }
    public void Dispose() { if (dirty || Snapshot != observed) Flush(); }
}

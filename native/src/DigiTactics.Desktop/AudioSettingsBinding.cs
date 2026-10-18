using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Debounces slider writes; flushes the last edit on normal app exit.</summary>
internal sealed class AudioSettingsBinding : IDisposable
{
    private readonly AudioSettingsStore store;
    private readonly GameAudio audio;
    private AudioSettings observed;
    private bool dirty;
    private double quiet;
    private AudioSettings Snapshot => new(audio.MusicVolume, audio.EffectsVolume, audio.Muted, audio.MusicOff);
    public AudioSettingsBinding(GameAudio audio)
    {
        this.audio = audio;
        store = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DigiTactics", "native", "audio-settings.json"));
        observed = store.Settings;
        audio.MusicVolume = observed.Music; audio.EffectsVolume = observed.Effects;
        audio.Muted = observed.Muted; audio.MusicOff = observed.MusicOff; audio.SettingsError = store.Error;
    }
    public void Update(double delta)
    {
        var current = Snapshot;
        if (current != observed) { observed = current; dirty = true; quiet = 0; }
        else quiet += delta;
        if (dirty && quiet >= .35) Flush();
    }
    private void Flush()
    {
        store.Save(Snapshot); audio.SettingsError = store.Error; dirty = false;
    }
    public void Dispose() { if (dirty || Snapshot != observed) Flush(); }
}

using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal static class AudioSettingsSmoke
{
    public static void Run()
    {
        string directory = Path.Combine(Path.GetTempPath(), "digitactics-audio-binding-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var profile = new NativeProfileStore(directory);
            using var audio = new GameAudio();
            using (var binding = new AudioSettingsBinding(audio, profile, directory))
            {
                if (!profile.UpdateStorage(new Dictionary<string, string> { ["dt.audio.v1"] = "{\"music\":0.2,\"sfx\":0.3,\"board\":0.4,\"muted\":true,\"musicOff\":true,\"muteOthers\":true}" }))
                    throw new InvalidOperationException(profile.Error);
                binding.Update(.1);
                if (audio.MusicVolume != .2f || audio.EffectsVolume != .3f || audio.BoardVolume != .4f || !audio.Muted || !audio.MusicOff || !audio.MuteOthers)
                    throw new InvalidOperationException("Late profile import did not update the live mixer.");
                using (var guard = new FileStream(Path.Combine(directory, "audio-settings.json.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    audio.BoardVolume = .7f; binding.Update(.1); binding.Update(.5);
                    if (audio.SettingsError is null) throw new InvalidOperationException("Locked settings write was not reported.");
                }
                binding.Update(.5);
                if (audio.SettingsError is not null || new AudioSettingsStore(Path.Combine(directory, "audio-settings.json")).Settings.Board != .7f)
                    throw new InvalidOperationException("Failed slider write did not retry after unlocking.");
                audio.MusicVolume = .9f;
            }
            audio.MusicVolume = 0; audio.BoardVolume = 0;
            using (var restart = new AudioSettingsBinding(audio, new NativeProfileStore(directory), directory))
                if (audio.MusicVolume != .9f || audio.BoardVolume != .7f) throw new InvalidOperationException("Restart did not preserve native edits over imported preferences.");
            Console.WriteLine("Audio settings smoke passed: late profile import applies all controls, locked writes retry, exit flushes edits, restart preserves native choices.");
        }
        finally
        {
            string resolved = Path.GetFullPath(directory);
            if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("digitactics-audio-binding-", StringComparison.Ordinal))
                throw new InvalidOperationException("Unsafe settings smoke cleanup path.");
            if (Directory.Exists(resolved)) Directory.Delete(resolved, true);
        }
    }
}

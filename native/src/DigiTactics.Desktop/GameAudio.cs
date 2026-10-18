using System.Text.Json;
using System.Runtime.InteropServices;
using Brutal.SdlApi;

namespace DigiTactics.Desktop;

internal sealed record AudioClipInfo(string File, int Frames, int? LoopFrame, int? EndFrame, double Gap, double? Duration);
internal sealed record AudioManifest(int SampleRate, int Channels, Dictionary<string, AudioClipInfo> Clips);

/// <summary>One bounded PCM mixer feeding BRUTAL SDL. Audio never reads or advances gameplay RNG.</summary>
internal sealed unsafe class GameAudio : IDisposable
{
    private sealed class Voice(float[] samples, float gain, bool music = false, int loop = 0, int end = 0, bool board = false)
    {
        public readonly float[] Samples = samples;
        public readonly bool Music = music;
        public readonly bool Board = board;
        public readonly int Loop = loop, End = end;
        public int Position;
        public int TailPosition = -1;
        public float Gain = gain, Target = gain;
    }
    private readonly AudioManifest manifest;
    private readonly string directory;
    private readonly Dictionary<string, float[]> samples = [];
    private readonly Dictionary<string, long> last = [];
    private readonly List<Voice> voices = [];
    private readonly float[] buffer = new float[882 * 2];
    private SdlAudioStream stream;
    private readonly Func<long> deviceClock;
    private bool audioInitialized, hiddenRequested;
    private long retryAt;
    private long frame;
    private long duckStart = -1000000, duckEnd = -1000000;
    private float duckFrom = 1;
    private float MusicDuck(long at)
    {
        if (at < duckStart + 2205) return duckFrom + (.45f - duckFrom) * Math.Clamp((at - duckStart) / 2205f, 0, 1);
        if (at <= duckEnd) return .45f;
        return .45f + .55f * Math.Clamp((at - duckEnd) / 17640f, 0, 1);
    }
    private string? music;
    public bool Available => !stream.IsNull();
    public string? Error { get; private set; }
    public float EffectsVolume { get; set; } = .8f;
    public float MusicVolume { get; set; } = .6f;
    public bool Suspended { get; private set; }
    public bool Muted { get; set; }
    public bool MusicOff { get; set; }
    public float BoardVolume { get; set; } = .8f;
    public bool MuteOthers { get; set; }
    public string? SettingsError { get; set; }
    public int Played { get; private set; }
    public static void Smoke()
    {
        long deviceNow = 0;
        using var audio = new GameAudio(() => deviceNow) { EffectsVolume = 0, MusicVolume = 0, BoardVolume = 0 };
        if (!audio.Available) throw new InvalidOperationException(audio.Error);
        foreach (var (id, info) in audio.manifest.Clips)
        {
            var pcm = audio.Load(id);
            if (pcm.Length != info.Frames * 2 || !pcm.Any(x => Math.Abs(x) > .001f) || pcm.Any(x => !float.IsFinite(x)))
                throw new InvalidDataException("Invalid decoded audio: " + id);
            if (info.LoopFrame is { } loop && (loop < 0 || info.EndFrame <= loop || info.EndFrame > info.Frames))
                throw new InvalidDataException("Invalid music loop: " + id);
        }
        audio.Play("buy"); audio.Play("hit_normal"); audio.Play("hit_normal"); audio.Play("hit_big");
        if (audio.Played != 2) throw new InvalidOperationException("Sound rate limiting failed.");
        string boardClip = audio.manifest.Clips.First(p => p.Key.StartsWith("clip_")).Key[5..];
        audio.PlayBoard(boardClip);
        if (audio.Played != 3 || !audio.voices.Any(v => v.Board) || Math.Abs(audio.MusicDuck(audio.duckStart + 2205) - .45f) > .001f || audio.MusicDuck(audio.duckEnd + 17640) != 1)
            throw new InvalidOperationException("Soundboard bus or music duck failed.");
        audio.Music("planning");
        var track = audio.voices.Single(v => v.Music);
        track.Position = track.End - 2;
        audio.Update();
        if (track.Position < track.Loop || track.Position >= track.End || track.TailPosition <= track.End)
            throw new InvalidOperationException("Music did not loop with its release tail.");
        int queued = Sdl.PInvoke.GetAudioStreamQueued(audio.stream);
        Thread.Sleep(100);
        if (queued <= 0 || Sdl.PInvoke.GetAudioStreamQueued(audio.stream) >= queued)
            throw new InvalidOperationException("SDL audio did not consume queued PCM.");
        audio.Music("combat"); audio.Update();
        audio.SetHidden(true);
        if (!audio.Suspended) throw new InvalidOperationException("Hidden audio failed to suspend.");
        var positions = audio.voices.Select(v => (v.Position,v.TailPosition)).ToArray();
        long pausedFrame = audio.frame;
        int pausedQueue = Sdl.PInvoke.GetAudioStreamQueued(audio.stream);
        var settings = (audio.Muted,audio.MusicOff,audio.MusicVolume,audio.EffectsVolume,audio.BoardVolume);
        audio.SetHidden(true); audio.Update(); Thread.Sleep(100); audio.Update();
        if (!positions.SequenceEqual(audio.voices.Select(v => (v.Position,v.TailPosition))) || audio.frame != pausedFrame || Sdl.PInvoke.GetAudioStreamQueued(audio.stream) != pausedQueue)
            throw new InvalidOperationException("Hidden audio advanced its mixer or drained queued PCM.");
        audio.SetHidden(false); audio.SetHidden(false);
        if (audio.Suspended || settings != (audio.Muted,audio.MusicOff,audio.MusicVolume,audio.EffectsVolume,audio.BoardVolume))
            throw new InvalidOperationException("Audio visibility changed saved preferences or failed to resume.");
        Thread.Sleep(100);
        if (Sdl.PInvoke.GetAudioStreamQueued(audio.stream) >= pausedQueue) throw new InvalidOperationException("Resumed audio did not consume PCM.");
        audio.Update();
        var beforeReconnect = audio.voices.Select(v => (v.Position,v.TailPosition)).ToArray();
        if (!audio.Reconnect() || !beforeReconnect.SequenceEqual(audio.voices.Select(v => (v.Position,v.TailPosition))))
            throw new InvalidOperationException("Manual audio reconnect lost the mixer position.");
        audio.DeviceFailed("Simulated device loss.");
        audio.Music("planning");
        var requested = audio.voices.Single(v => v.Music);
        if (requested.Target != 1) throw new InvalidOperationException("Music requested during an outage was discarded.");
        deviceNow = 1999; audio.Update();
        if (audio.Available || requested.Position != 0) throw new InvalidOperationException("Audio retried too early or advanced during an outage.");
        deviceNow = 2000; audio.Update();
        if (!audio.Available || audio.Error is not null || requested.Position == 0 || Sdl.PInvoke.GetAudioStreamQueued(audio.stream) <= 0)
            throw new InvalidOperationException("Audio failed to recover and queue the requested phase music.");
        audio.SetHidden(true); audio.DeviceFailed("Simulated hidden device loss.");
        var hiddenPositions = audio.voices.Select(v => (v.Position,v.TailPosition)).ToArray();
        deviceNow = 4000; audio.Update();
        if (!audio.Available || !audio.Suspended || !hiddenPositions.SequenceEqual(audio.voices.Select(v => (v.Position,v.TailPosition))) || Sdl.PInvoke.GetAudioStreamQueued(audio.stream) != 0)
            throw new InvalidOperationException("Device recovery played audio while the window was hidden.");
        audio.SetHidden(false); audio.Update();
        int recoveredQueue = Sdl.PInvoke.GetAudioStreamQueued(audio.stream);
        Thread.Sleep(100);
        if (recoveredQueue <= 0 || Sdl.PInvoke.GetAudioStreamQueued(audio.stream) >= recoveredQueue || settings != (audio.Muted,audio.MusicOff,audio.MusicVolume,audio.EffectsVolume,audio.BoardVolume))
            throw new InvalidOperationException("Recovered device failed to play PCM or changed preferences.");
        audio.Music(null);
        Console.WriteLine("Audio recovery passed: real stream replacement, bounded retry after simulated loss, phase music requested offline, hidden recovery and preserved preferences.");
        Console.WriteLine($"Audio smoke passed: {audio.manifest.Clips.Count} PCM assets, loop bounds, rate limiting, visibility pause/resume and BRUTAL SDL playback.");
    }

    public GameAudio(Func<long>? deviceClock = null)
    {
        this.deviceClock = deviceClock ?? (() => Environment.TickCount64);
        directory = Path.Combine(AppContext.BaseDirectory, "audio");
        manifest = JsonSerializer.Deserialize<AudioManifest>(File.ReadAllText(Path.Combine(directory, "manifest.json")),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        if (manifest.SampleRate != 44100 || manifest.Channels != 2) throw new InvalidDataException("Unsupported audio asset format.");
        Reconnect();
    }
    /// <summary>Replace playback on the current default device without resetting mixer state or preferences.</summary>
    public bool Reconnect()
    {
        retryAt = deviceClock() + 2000;
        if (!audioInitialized)
        {
            if (Sdl.PInvoke.InitSubSystem(SdlInitFlags.Audio) == 0) { Error = "Audio device unavailable. Retrying..."; return false; }
            audioInitialized = true;
        }
        var spec = new SdlAudioSpec { format = SdlAudioFormat.F32, channels = 2, freq = manifest.SampleRate };
        var replacement = Sdl.PInvoke.OpenAudioDeviceStream(uint.MaxValue, &spec, null, null);
        if (replacement.IsNull()) { Error = Available ? "Could not reconnect audio. Existing playback was kept." : "Audio device unavailable. Retrying..."; return false; }
        // SDL opens a paused stream; hidden windows must leave it paused.
        if (!hiddenRequested && Sdl.PInvoke.ResumeAudioStreamDevice(replacement) == 0)
        {
            replacement.Dispose(); Error = Available ? "Could not reconnect audio. Existing playback was kept." : "Audio playback could not start. Retrying..."; return false;
        }
        stream.Dispose(); stream = replacement; Suspended = hiddenRequested; Error = null;
        return true;
    }
    private void DeviceFailed(string error)
    {
        stream.Dispose(); stream = default; Suspended = false;
        Error = error + " Retrying..."; retryAt = deviceClock() + 2000;
    }
    private float[] Load(string id)
    {
        if (samples.TryGetValue(id, out var data)) return data;
        var bytes = File.ReadAllBytes(Path.Combine(directory, manifest.Clips[id].File));
        if (bytes.Length != 44 + manifest.Clips[id].Frames * 4 ||
            !bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8) || !bytes.AsSpan(8, 8).SequenceEqual("WAVEfmt "u8) ||
            BitConverter.ToUInt16(bytes, 20) != 1 || BitConverter.ToUInt16(bytes, 22) != 2 ||
            BitConverter.ToInt32(bytes, 24) != 44100 || BitConverter.ToUInt16(bytes, 34) != 16)
            throw new InvalidDataException("Invalid audio asset: " + id);
        var pcm = MemoryMarshal.Cast<byte, short>(bytes.AsSpan(44)); data = new float[pcm.Length];
        for (int i = 0; i < pcm.Length; i++) data[i] = pcm[i] / 32768f;
        samples[id] = data; return data;
    }
    public void Play(string id)
    {
        if (!Available || Muted || !manifest.Clips.TryGetValue(id, out var info)) return;
        // Source rate limits are per sound family, shared by its element/star/critical variants.
        string family = id.StartsWith("cast_") ? "cast" : id.StartsWith("hit_") ? "hit" : id.StartsWith("tick_") ? "tick" : id;
        if (last.TryGetValue(family, out long previous) && frame - previous < info.Gap * manifest.SampleRate) return;
        if (voices.Count(v => !v.Music) >= 40) return;
        last[family] = frame; voices.Add(new(Load(id), 1)); Played++;
    }
    public void Music(string? id)
    {
        if (music == id) return;
        music = id;
        if (!Available) voices.RemoveAll(v => v.Music);
        foreach (var voice in voices.Where(v => v.Music)) voice.Target = 0;
        if (id is null) return;
        string key = "music_" + id; var clip = manifest.Clips[key];
        voices.Add(new(Load(key), 0, true, clip.LoopFrame!.Value * 2, clip.EndFrame!.Value * 2) { Target = 1 });
    }
    public void PlayBoard(string id)
    {
        string key = "clip_" + id;
        if (!Available || Muted || !manifest.Clips.ContainsKey(key) || voices.Count(v => !v.Music) >= 40) return;
        duckFrom = MusicDuck(frame); duckStart = frame; duckEnd = frame + (long)((manifest.Clips[key].Duration ?? 1) * manifest.SampleRate);
        voices.Add(new(Load(key), 1, board: true)); Played++;
    }
    public void SetHidden(bool hidden)
    {
        hiddenRequested = hidden;
        if (!Available || Suspended == hidden) return;
        byte result = hidden ? Sdl.PInvoke.PauseAudioStreamDevice(stream) : Sdl.PInvoke.ResumeAudioStreamDevice(stream);
        if (result == 0) { DeviceFailed(hidden ? "Audio playback could not pause." : "Audio playback could not resume."); return; }
        Suspended = hidden;
    }
    public void Update()
    {
        if (!Available && deviceClock() >= retryAt) Reconnect();
        if (!Available || Suspended) return;
        int queued = Sdl.PInvoke.GetAudioStreamQueued(stream);
        if (queued < 0) { DeviceFailed("Audio device stopped."); return; }
        while (queued < buffer.Length * sizeof(float) * 3)
        {
            Array.Clear(buffer);
            foreach (var voice in voices)
            {
                for (int i = 0; i < buffer.Length; i += 2)
                {
                    if (voice.Music && voice.Position >= voice.End) { voice.TailPosition = voice.End; voice.Position = voice.Loop; }
                    if (voice.Position >= voice.Samples.Length) break;
                    voice.Gain += Math.Clamp(voice.Target - voice.Gain, -1f / 66150, 1f / 52920);
                    // Assets carry 6 dB of headroom; restore it alongside the original bus gains.
                    float gain = Muted || voice.Music && MusicOff ? 0 : voice.Gain * (voice.Music ? .84f * MusicVolume : voice.Board ? 1.5f * BoardVolume : 1.4f * EffectsVolume);
                    if (voice.Music) gain *= MusicDuck(frame + i / 2);
                    buffer[i] += voice.Samples[voice.Position++] * gain;
                    buffer[i + 1] += voice.Samples[voice.Position++] * gain;
                    // The final notes and reverb decay over the next loop rather than being cut off.
                    if (voice.TailPosition >= 0 && voice.TailPosition + 1 < voice.Samples.Length)
                    {
                        buffer[i] += voice.Samples[voice.TailPosition++] * gain;
                        buffer[i + 1] += voice.Samples[voice.TailPosition++] * gain;
                    }
                }
            }
            voices.RemoveAll(v => !v.Music && v.Position >= v.Samples.Length || v.Music && v.Target == 0 && v.Gain <= 0);
            for (int i = 0; i < buffer.Length; i++) buffer[i] = Math.Clamp(buffer[i], -1, 1);
            fixed (float* ptr = buffer)
                if (Sdl.PInvoke.PutAudioStreamData(stream, ptr, buffer.Length * sizeof(float)) == 0) { DeviceFailed("Audio queue failed."); return; }
            frame += buffer.Length / 2; queued += buffer.Length * sizeof(float);
        }
    }
    public void Dispose() { stream.Dispose(); stream = default; if (audioInitialized) { Sdl.PInvoke.QuitSubSystem(SdlInitFlags.Audio); audioInitialized = false; } }
}

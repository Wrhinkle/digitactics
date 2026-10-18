using System.Text.Json;
using System.Runtime.InteropServices;
using Brutal.SdlApi;

namespace DigiTactics.Desktop;

internal sealed record AudioClipInfo(string File, int Frames, int? LoopFrame, int? EndFrame, double Gap);
internal sealed record AudioManifest(int SampleRate, int Channels, Dictionary<string, AudioClipInfo> Clips);

/// <summary>One bounded PCM mixer feeding BRUTAL SDL. Audio never reads or advances gameplay RNG.</summary>
internal sealed unsafe class GameAudio : IDisposable
{
    private sealed class Voice(float[] samples, float gain, bool music = false, int loop = 0, int end = 0)
    {
        public readonly float[] Samples = samples;
        public readonly bool Music = music;
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
    private long frame;
    private string? music;
    public bool Available => !stream.IsNull();
    public string? Error { get; private set; }
    public float EffectsVolume { get; set; } = .8f;
    public float MusicVolume { get; set; } = .6f;
    public bool Muted { get; set; }
    public int Played { get; private set; }
    public static void Smoke()
    {
        using var audio = new GameAudio { EffectsVolume = 0, MusicVolume = 0 };
        if (!audio.Available) throw new InvalidOperationException(audio.Error);
        foreach (var (id, info) in audio.manifest.Clips)
        {
            var pcm = audio.Load(id);
            if (pcm.Length != info.Frames * 2 || !pcm.Any(x => Math.Abs(x) > .001f) || pcm.Any(x => !float.IsFinite(x)))
                throw new InvalidDataException("Invalid decoded audio: " + id);
            if (info.LoopFrame is { } loop && (loop < 0 || info.EndFrame <= loop || info.EndFrame > info.Frames))
                throw new InvalidDataException("Invalid music loop: " + id);
        }
        audio.Play("buy"); audio.Play("hit_normal"); audio.Play("hit_normal");
        if (audio.Played != 2) throw new InvalidOperationException("Sound rate limiting failed.");
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
        audio.Music("combat"); audio.Update(); audio.Music(null);
        Console.WriteLine($"Audio smoke passed: {audio.manifest.Clips.Count} PCM assets, loop bounds, rate limiting and BRUTAL SDL playback.");
    }

    public GameAudio()
    {
        directory = Path.Combine(AppContext.BaseDirectory, "audio");
        manifest = JsonSerializer.Deserialize<AudioManifest>(File.ReadAllText(Path.Combine(directory, "manifest.json")),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        if (manifest.SampleRate != 44100 || manifest.Channels != 2) throw new InvalidDataException("Unsupported audio asset format.");
        if (Sdl.PInvoke.InitSubSystem(SdlInitFlags.Audio) == 0) { Error = "Audio device unavailable."; return; }
        var spec = new SdlAudioSpec { format = SdlAudioFormat.F32, channels = 2, freq = manifest.SampleRate };
        stream = Sdl.PInvoke.OpenAudioDeviceStream(uint.MaxValue, &spec, null, null);
        if (stream.IsNull()) { Error = "Audio device unavailable."; return; }
        if (Sdl.PInvoke.ResumeAudioStreamDevice(stream) == 0) { Error = "Audio playback could not start."; stream.Dispose(); stream = default; }
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
        if (last.TryGetValue(id, out long previous) && frame - previous < info.Gap * manifest.SampleRate) return;
        if (voices.Count(v => !v.Music) >= 40) return;
        last[id] = frame; voices.Add(new(Load(id), 1)); Played++;
    }
    public void Music(string? id)
    {
        if (music == id) return;
        music = id;
        foreach (var voice in voices.Where(v => v.Music)) voice.Target = 0;
        if (id is null || !Available) return;
        string key = "music_" + id; var clip = manifest.Clips[key];
        voices.Add(new(Load(key), 0, true, clip.LoopFrame!.Value * 2, clip.EndFrame!.Value * 2) { Target = 1 });
    }
    public void Update()
    {
        if (!Available) return;
        int queued = Sdl.PInvoke.GetAudioStreamQueued(stream);
        if (queued < 0) { Error = "Audio device stopped."; return; }
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
                    float gain = Muted ? 0 : voice.Gain * (voice.Music ? .84f * MusicVolume : 1.4f * EffectsVolume);
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
                if (Sdl.PInvoke.PutAudioStreamData(stream, ptr, buffer.Length * sizeof(float)) == 0) { Error = "Audio queue failed."; return; }
            frame += buffer.Length / 2; queued += buffer.Length * sizeof(float);
        }
    }
    public void Dispose() { stream.Dispose(); stream = default; Sdl.PInvoke.QuitSubSystem(SdlInitFlags.Audio); }
}

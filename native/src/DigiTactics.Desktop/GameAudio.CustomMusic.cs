using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed unsafe partial class GameAudio
{
    private sealed class CustomVoice(StreamingMusicDecoder decoder)
    {
        public readonly StreamingMusicDecoder Decoder=decoder;
        public float Gain,Target,Step;
        public void Fade(float target,double seconds)
        {
            Target=target; Step=seconds==0?1:(float)(Math.Abs(target-Gain)/(seconds*StreamingMusicDecoder.SampleRate));
        }
    }
    private readonly List<CustomVoice> customVoices=[];
    private readonly float[] customBuffer=new float[StreamingMusicDecoder.ChunkSamples];
    private CustomVoice? activeCustom;
    public bool CustomMusicEnded { get; private set; }
    public string? CustomMusicError { get; private set; }
    public void PlayCustomMusic(StreamingMusicDecoder decoder,double fadeSeconds=1)
    {
        ValidateFade(fadeSeconds);
        if(!decoder.Ready.IsCompletedSuccessfully || !decoder.Ready.Result) throw new InvalidOperationException("Custom music must have decoded audio before playback.");
        // Reuse the source player's two-slot crossfade model, even when Next is pressed rapidly.
        while(customVoices.Count>=2) { customVoices[0].Decoder.Dispose(); customVoices.RemoveAt(0); }
        activeCustom?.Fade(0,fadeSeconds);
        foreach(var voice in voices.Where(v=>v.Music)) { voice.Target=0; voice.FadeDown=voice.Gain/44100; }
        music=null; CustomMusicEnded=false; CustomMusicError=null;
        activeCustom=new(decoder); activeCustom.Fade(1,fadeSeconds); customVoices.Add(activeCustom);
    }
    public void StopCustomMusic(double fadeSeconds=1)
    {
        ValidateFade(fadeSeconds); activeCustom?.Fade(0,fadeSeconds); activeCustom=null;
    }
    private static void ValidateFade(double seconds)
    {
        if(!double.IsFinite(seconds) || seconds<0) throw new ArgumentOutOfRangeException(nameof(seconds));
    }
    private void MixCustomMusic()
    {
        foreach(var voice in customVoices)
        {
            Array.Clear(customBuffer); int count=voice.Decoder.Read(customBuffer);
            for(int i=0;i<buffer.Length;i+=2)
            {
                voice.Gain+=Math.Clamp(voice.Target-voice.Gain,-voice.Step,voice.Step);
                // User audio has no exported-asset headroom correction: use the original .42 music bus.
                float gain=Muted || MusicOff?0:voice.Gain*.42f*MusicVolume*MusicDuck(frame+i/2);
                if(i<count) { buffer[i]+=customBuffer[i]*gain; buffer[i+1]+=customBuffer[i+1]*gain; }
            }
            if(voice==activeCustom && voice.Decoder.Ended)
            {
                CustomMusicEnded=true; CustomMusicError=voice.Decoder.Error; activeCustom=null;
            }
        }
        foreach(var voice in customVoices.Where(v=>v.Decoder.Ended || v.Target==0 && v.Gain<=0).ToArray())
        { voice.Decoder.Dispose(); customVoices.Remove(voice); }
    }
    private void DisposeCustomMusic()
    {
        foreach(var voice in customVoices) voice.Decoder.Dispose(); customVoices.Clear(); activeCustom=null;
    }
}

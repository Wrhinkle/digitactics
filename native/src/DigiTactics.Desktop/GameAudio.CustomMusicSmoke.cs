using System.Diagnostics;
using Brutal.SdlApi;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed unsafe partial class GameAudio
{
    internal static void CustomMusicSmoke(string executable)
    {
        string directory=Path.Combine(Path.GetTempPath(),"digitactics-custom-mixer-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        var decoders=new List<StreamingMusicDecoder>();
        try
        {
            string file=Path.Combine(directory,"constant.wav"); int frames=88200;
            using(var writer=new BinaryWriter(File.Create(file)))
            {
                writer.Write("RIFF"u8); writer.Write(36+frames*4); writer.Write("WAVEfmt "u8); writer.Write(16); writer.Write((short)1); writer.Write((short)2); writer.Write(44100); writer.Write(176400); writer.Write((short)4); writer.Write((short)16); writer.Write("data"u8); writer.Write(frames*4);
                for(int i=0;i<frames*2;i++) writer.Write((short)8192);
            }
            StreamingMusicDecoder Decode()
            {
                var decoder=new StreamingMusicDecoder(executable,new FileStream(file,FileMode.Open,FileAccess.Read,FileShare.Read)); decoders.Add(decoder);
                if(!decoder.Ready.WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult()) throw new InvalidOperationException(decoder.Error);
                return decoder;
            }
            using(var audio=new GameAudio {EffectsVolume=0,MusicVolume=1,BoardVolume=0})
            {
                if(!audio.Available) throw new InvalidOperationException(audio.Error);
                var first=Decode(); audio.PlayCustomMusic(first,.1); audio.MixCustomMusic();
                if(Math.Abs(audio.buffer[^1]-.25*.42*.2)>.00001 || Math.Abs(audio.activeCustom!.Gain-.2)>.00001) throw new InvalidOperationException("Custom fade-in or music bus gain differs from source.");
                var second=Decode(); audio.PlayCustomMusic(second,.2); Array.Clear(audio.buffer); audio.MixCustomMusic();
                if(audio.customVoices.Count!=2 || Math.Abs(audio.customVoices[0].Gain-.18)>.00001 || Math.Abs(audio.activeCustom!.Gain-.1)>.00001) throw new InvalidOperationException("Custom music did not crossfade both tracks.");
                audio.Muted=true; Array.Clear(audio.buffer); audio.MixCustomMusic();
                if(audio.buffer.Any(v=>v!=0)) throw new InvalidOperationException("Custom music ignored global mute.");
                audio.Muted=false; audio.MusicOff=true; Array.Clear(audio.buffer); audio.MixCustomMusic();
                if(audio.buffer.Any(v=>v!=0)) throw new InvalidOperationException("Custom music ignored music-off preference.");
                audio.MusicOff=false; var third=Decode(); audio.PlayCustomMusic(third,0);
                first.Completion.WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
                if(audio.customVoices.Count!=2) throw new InvalidOperationException("Rapid track switches exceeded two decoder slots.");
                audio.duckStart=-2205; audio.duckEnd=100000; Array.Clear(audio.buffer); audio.MixCustomMusic();
                if(Math.Abs(audio.buffer[^1]-.25*.42*.45)>.00001) throw new InvalidOperationException("Custom music ignored soundboard ducking.");
                audio.MusicVolume=0; audio.Update(); int queue=Sdl.PInvoke.GetAudioStreamQueued(audio.stream); Thread.Sleep(100);
                if(queue<=0 || Sdl.PInvoke.GetAudioStreamQueued(audio.stream)>=queue) throw new InvalidOperationException("SDL did not consume streamed music PCM.");
                audio.SetHidden(true); var gain=audio.activeCustom!.Gain; audio.Update();
                if(audio.activeCustom.Gain!=gain) throw new InvalidOperationException("Hidden custom music advanced.");
                if(!audio.Reconnect() || !audio.Suspended || audio.activeCustom.Gain!=gain) throw new InvalidOperationException("Hidden reconnect reset custom music.");
                audio.SetHidden(false);
                var clock=Stopwatch.StartNew();
                while(!audio.CustomMusicEnded && clock.Elapsed.TotalSeconds<10) { Array.Clear(audio.buffer); audio.MixCustomMusic(); Thread.Sleep(1); }
                if(!audio.CustomMusicEnded || audio.CustomMusicError is not null || audio.customVoices.Count!=0) throw new InvalidOperationException("Custom music did not finish and release its decoders.");
                var final=Decode(); audio.PlayCustomMusic(final,0); audio.Music("planning");
                for(int i=0;i<80;i++) { Array.Clear(audio.buffer); audio.MixCustomMusic(); }
                if(audio.customVoices.Count!=0 || !audio.voices.Any(v=>v.Music && v.Target==1)) throw new InvalidOperationException("Returning to built-in music left custom playback running.");
            }
            Console.WriteLine("Custom music mixer passed: real decoded PCM, source gain, linear fades, two-slot crossfades, mute/music-off/ducking, SDL queue playback, hidden reconnect, end-of-track and built-in handoff.");
        }
        finally
        {
            foreach(var decoder in decoders) decoder.Dispose();
            Task.WhenAll(decoders.Select(d=>d.Completion)).WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
            string resolved=Path.GetFullPath(directory);
            if(!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()),StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("digitactics-custom-mixer-",StringComparison.Ordinal)) throw new InvalidOperationException("Unsafe custom music smoke cleanup.");
            Directory.Delete(resolved,true);
        }
    }
}

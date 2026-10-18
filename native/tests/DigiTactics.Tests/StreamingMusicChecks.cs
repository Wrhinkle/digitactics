using System.Diagnostics;
using DigiTactics.Presentation;
using DigiTactics.Storage;

internal static class StreamingMusicChecks
{
    public static async Task Run(string executable)
    {
        int checks=0;
        void Check(bool value,string why) { checks++; if(!value) throw new InvalidOperationException("Music streaming: "+why); }
        string directory=Path.Combine(Path.GetTempPath(),"digitactics-decoder-test-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        async Task Encode(string file,string codec,string duration="0.3")
        {
            using var process=new Process { StartInfo=new ProcessStartInfo(executable) {UseShellExecute=false,CreateNoWindow=true,RedirectStandardError=true} };
            foreach(var arg in new[] {"-hide_banner","-loglevel","error","-nostdin","-f","lavfi","-i","sine=frequency=441:sample_rate=32000:duration="+duration,"-ac","1","-c:a",codec,file}) process.StartInfo.ArgumentList.Add(arg);
            process.Start(); var errors=process.StandardError.ReadToEndAsync(); await process.WaitForExitAsync();
            if(process.ExitCode!=0) throw new InvalidOperationException(await errors);
        }
        try
        {
            var library=new MusicLibrary(Path.Combine(directory,"library"));
            foreach(var (extension,codec) in new[] {("wav","pcm_s16le"),("mp3","libmp3lame"),("ogg","libvorbis"),("oga","libvorbis"),("m4a","aac"),("aac","aac"),("flac","flac"),("opus","libopus"),("webm","libopus")})
            {
                string file=Path.Combine(directory,"test '+$ Unicode \u03a9."+extension); await Encode(file,codec);
                Check(library.AddFile(file),library.Error??"library import"); var track=library.Tracks.Last();
                using var decoder=new StreamingMusicDecoder(executable,library.OpenAudio(track.Id));
                Check(await decoder.Ready.WaitAsync(TimeSpan.FromSeconds(10)),decoder.Error??"initial PCM");
                await Task.Delay(100);
                Check(decoder.BufferedChunks==StreamingMusicDecoder.BufferChunks,"bounded backpressure for "+extension);
                float[] block=new float[1764]; int samples=0; double energy=0,stereoError=0;
                var timer=Stopwatch.StartNew();
                while(!decoder.Ended && timer.Elapsed.TotalSeconds<10)
                {
                    int count=decoder.Read(block); samples+=count;
                    for(int i=0;i<count;i+=2) { Check(float.IsFinite(block[i]) && float.IsFinite(block[i+1]),"finite PCM"); energy+=block[i]*block[i]; stereoError+=Math.Abs(block[i]-block[i+1]); }
                    if(count==0) await Task.Delay(1);
                }
                await decoder.Completion.WaitAsync(TimeSpan.FromSeconds(10));
                Check(decoder.Error is null,decoder.Error??"successful decode");
                Check(decoder.Ended && samples>22000 && samples<40000 && energy>1 && stereoError<.01,"resampled stereo signal and duration for "+extension);
            }
            string longFile=Path.Combine(directory,"long.flac"); await Encode(longFile,"flac","60");
            Check(library.AddFile(longFile),"long track import"); var longTrack=library.Tracks.Last();
            var lease=library.OpenAudio(longTrack.Id); using var cancelled=new StreamingMusicDecoder(executable,lease);
            Check(await cancelled.Ready.WaitAsync(TimeSpan.FromSeconds(10)),"long track starts"); await Task.Delay(100);
            Check(cancelled.BufferedChunks==8 && !cancelled.Completion.IsCompleted,"long track cannot decode beyond bounded queue");
            cancelled.Dispose(); cancelled.Dispose(); await cancelled.Completion.WaitAsync(TimeSpan.FromSeconds(5));
            Check(lease.SafeFileHandle.IsClosed && cancelled.Error is null && cancelled.Read(new float[2])==0,"cancellation stops process, closes file and silences reads");
            for(int i=0;i<10;i++)
            {
                var earlyLease=library.OpenAudio(longTrack.Id);
                using var early=new StreamingMusicDecoder(executable,earlyLease); early.Dispose();
                await early.Completion.WaitAsync(TimeSpan.FromSeconds(5));
                Check(earlyLease.SafeFileHandle.IsClosed && early.Error is null,"immediate cancellation releases its input lease");
            }
            string corrupt=Path.Combine(directory,"bad.wav"); File.WriteAllText(corrupt,"Not audio"); Check(library.AddFile(corrupt),"store preserves bytes before decode");
            using var failed=new StreamingMusicDecoder(executable,library.OpenAudio(library.Tracks.Last().Id));
            Check(!await failed.Ready.WaitAsync(TimeSpan.FromSeconds(10)),"corrupt file cannot become ready"); await failed.Completion;
            Check(failed.Error is not null && failed.Ended,"decode error is reported without hanging");
            using var missing=new StreamingMusicDecoder(Path.Combine(directory,"missing.exe"),library.OpenAudio(longTrack.Id));
            Check(!await missing.Ready.WaitAsync(TimeSpan.FromSeconds(5)),"missing runtime cannot become ready"); await missing.Completion;
            Check(missing.Error is not null,"missing runtime error");
            Console.WriteLine($"Music streaming: {checks} checks passed across nine original formats, 32kHz mono to 44.1kHz stereo, bounded buffering, cancellation, corrupt input and missing runtime.");
        }
        finally
        {
            string resolved=Path.GetFullPath(directory);
            if(!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()),StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("digitactics-decoder-test-",StringComparison.Ordinal)) throw new InvalidOperationException("Unsafe decoder test cleanup.");
            Directory.Delete(resolved,true);
        }
    }
}

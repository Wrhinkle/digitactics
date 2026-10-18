using System.Diagnostics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed unsafe partial class GameAudio
{
    internal static void MusicRoutingSmoke(string executable)
    {
        string directory=Path.Combine(Path.GetTempPath(),"digitactics-music-routing-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        try
        {
            var profile=new NativeProfileStore(directory); var library=profile.OpenMusicLibrary();
            void Add(string name,string use)
            {
                string path=Path.Combine(directory,name+".wav"); int frames=44100;
                using(var writer=new BinaryWriter(File.Create(path)))
                {
                    writer.Write("RIFF"u8); writer.Write(36+frames*4); writer.Write("WAVEfmt "u8); writer.Write(16); writer.Write((short)1); writer.Write((short)2); writer.Write(44100); writer.Write(176400); writer.Write((short)4); writer.Write((short)16); writer.Write("data"u8); writer.Write(frames*4);
                    for(int i=0;i<frames*2;i++) writer.Write((short)8192);
                }
                if(!library.AddFile(path) || !library.SetUse(library.Tracks.Last().Id,use)) throw new InvalidOperationException(library.Error);
            }
            Add("Plan","planning"); Add("Fight","combat");
            string planning=library.Tracks.Single(t=>t.Use=="planning").Id, combat=library.Tracks.Single(t=>t.Use=="combat").Id;
            using var audio=new GameAudio {MusicSource="own",MusicVolume=.2f};
            using var controller=new CustomMusicController(audio,profile,executable);
            void Until(Func<bool> done,string error)
            {
                var clock=Stopwatch.StartNew();
                while(!done() && clock.Elapsed.TotalSeconds<10) { controller.Update(); Array.Clear(audio.buffer); audio.MixCustomMusic(); Thread.Sleep(1); }
                if(!done()) throw new InvalidOperationException(error+": "+controller.Error);
            }
            audio.Music("planning"); Until(()=>controller.CurrentTrackId==planning && audio.activeCustom is not null && !controller.Loading,"Planning routing failed");
            audio.Music("combat"); Until(()=>controller.CurrentTrackId==combat && audio.activeCustom is not null && !controller.Loading,"Combat routing failed");
            var old=audio.activeCustom; controller.Next(); Until(()=>audio.activeCustom is not null && audio.activeCustom!=old && !controller.Loading,"Next did not replace the stream");
            old=audio.activeCustom; Until(()=>audio.activeCustom is not null && audio.activeCustom!=old && !controller.Loading,"End-of-track did not advance");
            audio.Muted=true; controller.Update(); if(audio.activeCustom is not null) throw new InvalidOperationException("Mute did not stop custom music.");
            audio.Muted=false; Until(()=>audio.activeCustom is not null && !controller.Loading,"Unmute did not restart custom music");
            audio.MusicSource="builtin"; controller.Update(); if(audio.activeCustom is not null || !audio.voices.Any(v=>v.Music && v.Target==1)) throw new InvalidOperationException("Source change did not restore built-in music.");
            for(int i=0;i<20;i++) controller.Update();
            if(audio.voices.Count(v=>v.Music && v.Target==1)!=1) throw new InvalidOperationException("Repeated routing duplicated the built-in track.");
            audio.MusicSource="own"; Until(()=>audio.activeCustom is not null && !controller.Loading,"Own source did not restart");
            audio.Music(null); controller.Update(); if(audio.activeCustom is not null) throw new InvalidOperationException("Leaving the match did not stop custom music.");
            foreach(var track in library.Tracks.ToArray()) if(!library.Remove(track.Id)) throw new InvalidOperationException(library.Error);
            string corrupt=Path.Combine(directory,"broken.wav"); File.WriteAllText(corrupt,"not audio"); if(!library.AddFile(corrupt)) throw new InvalidOperationException(library.Error);
            controller.Reload(); audio.Music("planning");
            Until(()=>!controller.Loading && controller.Error is not null && audio.activeCustom is null,"Corrupt-only library did not stop after bounded failures");
            for(int i=0;i<20;i++) controller.Update();
            if(controller.Loading) throw new InvalidOperationException("Failed playlist restarted automatically in a loop.");
            controller.Next(); if(!controller.Loading) throw new InvalidOperationException("Explicit retry did not restart failed music.");
            controller.Dispose(); audio.DisposeCustomMusic();
            // Wait for cancellation continuations to release verified file leases before test cleanup.
            var deadline=Stopwatch.StartNew();
            while(deadline.Elapsed.TotalSeconds<5)
            {
                try { using var file=new FileStream(Path.Combine(directory,"music",library.Tracks.Single().Sha256+".audio"),FileMode.Open,FileAccess.Write,FileShare.None); break; }
                catch(IOException) { Thread.Sleep(10); }
            }
            Console.WriteLine("Music routing passed: planning/combat, Next, natural completion, mute/restart, source changes, leave, corrupt-file failure limit and explicit retry.");
        }
        finally
        {
            string resolved=Path.GetFullPath(directory);
            if(!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()),StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("digitactics-music-routing-",StringComparison.Ordinal)) throw new InvalidOperationException("Unsafe routing test cleanup.");
            Directory.Delete(resolved,true);
        }
    }
}

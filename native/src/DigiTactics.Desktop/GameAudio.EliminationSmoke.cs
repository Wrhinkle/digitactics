using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class GameAudio
{
    public static void EliminationAudioSmoke()
    {
        using var audio=new GameAudio(){EffectsVolume=0,MusicVolume=0,BoardVolume=0};
        if(!audio.Available)throw new InvalidOperationException(audio.Error);
        int checks=0;
        void Check(bool pass,string message){checks++;if(!pass)throw new InvalidOperationException("Elimination audio: "+message);}
        foreach(string kind in new[]{"sound","elimsound"})foreach(var item in CosmeticCollection.Items(kind))
        {
            string key="clip_"+(item.Data.TryGetProperty("clip"u8,out var clip)?clip.GetString():item.Id);
            Check(audio.manifest.Clips.ContainsKey(key),"every cosmetic clip exported: "+item.Id);
            var samples=audio.Load(key);Check(samples.Any(v=>Math.Abs(v)>.001f),"audible original synthesis: "+item.Id);
            if(item.Id.StartsWith("mystery_",StringComparison.Ordinal))
                Check(audio.manifest.Clips[key].Duration==item.Data.GetProperty("dur"u8).GetDouble(),"authored mystery preview/duck duration: "+item.Id);
        }
        audio.PlayEliminationTheme("ks_gate",true);Check(audio.voices.Count==1&&ReferenceEquals(audio.voices[0].Samples,audio.Load("killscreen_gate_big")),"base theme sting");
        audio.PlayEliminationTheme("mystery_daily_killscreen_1",false);Check(ReferenceEquals(audio.voices[^1].Samples,audio.Load("killscreen_basic_normal")),"mystery original default sting");
        audio.PlayEliminationTheme("ks_gate",false,true);Check(ReferenceEquals(audio.voices[^1].Samples,audio.Load("killscreen_sting_normal")),"neutral PvE sting");
        audio.voices.Clear();audio.MuteOthers=true;
        Check(!audio.PlayEliminationSound("es_horn",false)&&audio.voices.Count==0,"mute other players");
        Check(audio.PlayEliminationSound("es_horn",true),"own sound ignores mute others");
        Check(audio.PlayEliminationSound("es_horn",false,true),"preview ignores mute others");
        audio.voices.Clear();audio.scheduledBoardDucks.Clear();audio.Muted=true;
        Check(!audio.PlayEliminationSound("es_horn",true),"master mute");audio.Muted=false;
        Check(audio.PlayEliminationSound("unknown",true),"invalid cosmetic uses source default");
        var voice=audio.voices.Single();long start=audio.frame+13230;
        Check(voice.Board&&voice.StartFrame==start&&ReferenceEquals(voice.Samples,audio.Load("clip_es_chime")),"board bus and exact .3 second sample delay");
        Check(audio.MusicDuck(start-1)==1&&audio.MusicDuck(start)==1,"music remains unducked before scheduled onset");
        Check(Math.Abs(audio.MusicDuck(start+2205)-.45f)<.0001,"source 50 ms music attack");
        Check(audio.MusicDuck(start-1)==1,"querying a later sample never changes earlier samples");
        long end=start+(long)(audio.manifest.Clips["clip_es_chime"].Duration!.Value*44100);
        Check(audio.MusicDuck(end)==.45f&&audio.MusicDuck(end+17640)==1,"duration and 400 ms music release");
        audio.Update();Check(voice.Position==0,"queued buffers do not play delayed voice early");
        audio.SetHidden(true);long paused=audio.frame;audio.Update();Check(audio.frame==paused&&voice.Position==0,"hidden window preserves delayed onset");audio.SetHidden(false);
        var timer=System.Diagnostics.Stopwatch.StartNew();while(voice.Position==0&&timer.Elapsed.TotalSeconds<2){audio.Update();Thread.Sleep(10);}
        Check(voice.Position>0&&audio.frame>start,"SDL plays scheduled voice after its sample boundary");
        Check(audio.scheduledBoardDucks.Count==0,"consumed schedules are released");
        foreach(var (kind,id) in new[]{("sound","mystery_daily_sound_1"),("elimsound","mystery_vault_elimsound_8")})
        {
            using var preview=new GameAudio(){EffectsVolume=0,MusicVolume=0,BoardVolume=0};
            Check(preview.Available,"isolated preview audio device");
            if(kind=="sound")preview.PlayBoard(id);else Check(preview.PlayEliminationSound(id,false,true,0),"secret cadence preview schedules");
            var rendered=preview.voices.Single();
            Check(rendered.Board&&ReferenceEquals(rendered.Samples,preview.Load("clip_"+id)),"actual mystery preview uses its exported PCM and board bus: "+id);
            preview.Update();int queued=Brutal.SdlApi.Sdl.PInvoke.GetAudioStreamQueued(preview.stream);
            Thread.Sleep(100);
            Check(rendered.Position>0&&queued>0&&Brutal.SdlApi.Sdl.PInvoke.GetAudioStreamQueued(preview.stream)<queued,"SDL consumes authored preview: "+id);
        }
        Console.WriteLine($"Elimination audio: {checks} cosmetic coverage, bus, mute, scheduling, ducking and real SDL checks passed.");
    }
}

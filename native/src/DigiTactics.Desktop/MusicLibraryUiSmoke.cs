using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed class MusicLibraryUiSmoke : IDisposable
{
    private readonly string directory;
    private readonly CustomMusicController controller;
    private readonly AudioSettingsBinding preferences;
    private readonly GameAudio audio;
    private readonly string[] paths;
    private MusicSoundControlsSmoke? matchControls;
    private int step,cooldown=3;
    private bool down,done,navigationClick,sourceClick;
    private int sourceStep;
    private string first="",second="";
    public MusicLibraryUiSmoke(MatchScreen screen,GameAudio audio)
    {
        this.audio=audio;
        directory=Path.Combine(Path.GetTempPath(),"digitactics-music-ui-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        var profile=new NativeProfileStore(directory); preferences=new(audio,profile,directory);
        controller=new(audio,profile,Path.Combine(AppContext.BaseDirectory,"media","ffmpeg.exe"));
        paths=[Path.Combine(directory,"Planning 100% \u03a9.wav"),Path.Combine(directory,"not-audio.txt"),Path.Combine(directory,"Combat ## music.wav")];
        foreach(string path in paths)
        {
            using var writer=new BinaryWriter(File.Create(path));
            int bytes=44100*30*4;
            writer.Write("RIFF"u8); writer.Write(36+bytes); writer.Write("WAVEfmt "u8); writer.Write(16); writer.Write((short)1); writer.Write((short)2); writer.Write(44100); writer.Write(176400); writer.Write((short)4); writer.Write((short)16); writer.Write("data"u8); writer.Write(bytes); writer.Write(new byte[bytes]);
        }
        screen.PickMusicFiles=()=>Task.FromResult(paths);
    }
    public void Input(MatchScreen screen)
    {
        preferences.Update(.1);
        if(matchControls is not null){matchControls.Input();return;}
        // Completion happens after drawing. Wait for enabled controls and their
        // status-dependent positions to be rendered before using cached centers.
        if(controller.LibraryBusy){cooldown=3;return;}
        if(cooldown-->0)return;
        if(down) { ImGui.GetIO().AddMouseButtonEvent(0,false); down=false;if(sourceClick)sourceStep++;else if(!navigationClick)step++;navigationClick=sourceClick=false;cooldown=3;return; }
        if(step==1&&screen.MusicStatusText!="Built-in: Tamer's Morning for planning, Digital World Showdown for combat.")throw new InvalidOperationException("Idle music library did not name both original built-in themes.");
        if(step==2)
        {
            if(controller.Library?.Tracks.Count!=2) return;
            first=controller.Library.Tracks[0].Id; second=controller.Library.Tracks[1].Id;
            if(controller.EditError is null) throw new InvalidOperationException("Skipped non-audio file was not reported.");
            if(audio.MusicSource!="own") throw new InvalidOperationException("Import did not select own music.");
        }
        if(step==4)
        {
            var saved=new MusicLibrary(Path.Combine(directory,"music"));
            if(saved.Tracks.Single(t=>t.Id==first).Use!="planning" || saved.Tracks.Single(t=>t.Id==second).Use!="combat") throw new InvalidOperationException("Phase buttons did not persist assignments.");
            if(sourceStep==1&&audio.MusicSource!="builtin"||sourceStep==2&&audio.MusicSource!="own")throw new InvalidOperationException("Music source choices did not switch the active source.");
            if(screen.MusicStatusText!=(sourceStep==1?"Built-in: Tamer's Morning for planning, Digital World Showdown for combat.":"Your files play during a game."))throw new InvalidOperationException("Idle source selection did not update the original music status.");
            if(screen.MusicNextEnabled)throw new InvalidOperationException("Next track is enabled without active custom music playback.");
        }
        if(step==6 && controller.Library!.Tracks.Count!=2) throw new InvalidOperationException("Cancel removal deleted a track.");
        if(step==8 && controller.Library!.Tracks.Count!=1) throw new InvalidOperationException("Confirmed removal failed.");
        if(step==10 && (controller.Library!.Tracks.Count!=0 || audio.MusicSource!="builtin")) throw new InvalidOperationException("Removing the last track did not restore built-in music.");
        if(step==12 && controller.Library!.Tracks.Count!=2) throw new InvalidOperationException("Reimport before bulk clear failed.");
        if(step==13 && controller.Library!.Tracks.Count!=2) throw new InvalidOperationException("Cancel clear deleted tracks.");
        if(step==15 && (controller.Library!.Tracks.Count!=0 || new MusicLibrary(Path.Combine(directory,"music")).Tracks.Count!=0 || audio.MusicSource!="builtin")) throw new InvalidOperationException("Confirmed clear did not persist or restore built-in music.");
        if(step==17)
        {
            if(!screen.Buttons.ContainsKey("start") || screen.Buttons.ContainsKey("addMusic")) throw new InvalidOperationException("Library did not close to menu.");
            preferences.Update(1);
            if(new AudioSettingsStore(Path.Combine(directory,"audio-settings.json")).Settings.Source!="builtin") throw new InvalidOperationException("Music source was not saved.");
            matchControls=new(screen,audio,controller,paths.Where(path=>path.EndsWith(".wav",StringComparison.Ordinal)).ToArray());return;
        }
        string key=step switch {0=>"musicLibrary",1=>"musicOwn",2=>$"musicUse/{first}/planning",3=>$"musicUse/{second}/combat",4 when sourceStep<2=>sourceStep==0?"musicBuiltin":"musicOwn",4=>$"removeMusic/{first}",5=>"cancelRemoveMusic",6=>$"removeMusic/{first}",7=>$"confirmRemoveMusic/{first}",8=>$"removeMusic/{second}",9=>$"confirmRemoveMusic/{second}",10=>"addMusic",11=>"clearMusic",12=>"cancelClearMusic",13=>"clearMusic",14=>"confirmClearMusic",15=>"closeMusicLibrary",16=>"homeOverview",_=>throw new InvalidOperationException()};
        if(!screen.Buttons.TryGetValue(key,out var point))
        {
            if(step!=0||screen.SettingsVisible||!screen.Buttons.TryGetValue("settings",out point))return;
            navigationClick=true;
        }
        if(step==0&&!navigationClick&&(point.Y<screen.SettingsScrollMin.Y+8||point.Y>screen.SettingsScrollMax.Y-18))
        {
            var hover=screen.SettingsScrollMin+new float2(100,100);
            ImGui.GetIO().AddMousePosEvent(hover.X,hover.Y);ImGui.GetIO().AddMouseWheelEvent(0,point.Y<screen.SettingsScrollMin.Y+8?3:-3);cooldown=3;return;
        }
        if(step>0&&step<15&&(point.Y<screen.MusicScrollMin.Y+8||point.Y>screen.MusicScrollMax.Y-18))
        {
            var center=(screen.MusicScrollMin+screen.MusicScrollMax)*.5f;
            ImGui.GetIO().AddMousePosEvent(center.X,center.Y);ImGui.GetIO().AddMouseWheelEvent(0,point.Y<screen.MusicScrollMin.Y+8?3:-3);cooldown=3;return;
        }
        sourceClick=step==4&&sourceStep<2;
        ImGui.GetIO().AddMousePosEvent(point.X,point.Y); ImGui.GetIO().AddMouseButtonEvent(0,true); down=true;
    }
    public bool Verify(int2 size,string? capture)
    {
        if(matchControls is not null)done=matchControls.Verify(size,capture);
        if(step==1&&capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"music-built-in.bmp"),size.X,size.Y);
        if(step==14 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"music-clear.bmp"),size.X,size.Y);
        if(step==5 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"music-remove.bmp"),size.X,size.Y);
        if(step==4 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"music-library.bmp"),size.X,size.Y);
        if(done) Console.WriteLine("Music library UI passed: Settings navigation, empty own-source chooser, source switching, multi-file import, literal names, phase assignment persistence, cancel/confirm removal, last-track fallback, cancelled/confirmed bulk clear and saved source; live-match empty/cancel chooser, playback/Next/mute/source/single-track guards and inline track phase selection; original built-in track names, planning/combat phase and idle/playing/muted statuses; "+matchControls!.PreviewEvidence+".");
        return done;
    }
    public void Dispose()
    {
        controller.Dispose(); preferences.Dispose();
        typeof(GameAudio).GetMethod("DisposeCustomMusic",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.Invoke(audio,null);
        string resolved=Path.GetFullPath(directory);
        if(!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()),StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("digitactics-music-ui-",StringComparison.Ordinal)) throw new InvalidOperationException("Unsafe music UI cleanup.");
        // Decoder cancellation releases its verified file lease on a continuation.
        // Match the routing smoke's bounded wait before removing this test-only save.
        var cleanup=System.Diagnostics.Stopwatch.StartNew();
        while(Directory.Exists(resolved))
        {
            try{Directory.Delete(resolved,true);break;}
            catch(IOException)when(cleanup.Elapsed.TotalSeconds<5){Thread.Sleep(10);}
        }
    }
}

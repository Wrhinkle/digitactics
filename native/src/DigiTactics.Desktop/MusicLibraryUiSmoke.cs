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
    private int step,cooldown=3;
    private bool down,done;
    private string first="",second="";
    public MusicLibraryUiSmoke(MatchScreen screen,GameAudio audio)
    {
        this.audio=audio;
        directory=Path.Combine(Path.GetTempPath(),"digitactics-music-ui-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        var profile=new NativeProfileStore(directory); preferences=new(audio,profile,directory);
        controller=new(audio,profile,Path.Combine(directory,"unused-decoder.exe"));
        string[] paths=[Path.Combine(directory,"Planning 100% \u03a9.wav"),Path.Combine(directory,"not-audio.txt"),Path.Combine(directory,"Combat ## music.wav")];
        foreach(string path in paths)
        {
            using var writer=new BinaryWriter(File.Create(path));
            writer.Write("RIFF"u8); writer.Write(40); writer.Write("WAVEfmt "u8); writer.Write(16); writer.Write((short)1); writer.Write((short)2); writer.Write(44100); writer.Write(176400); writer.Write((short)4); writer.Write((short)16); writer.Write("data"u8); writer.Write(4); writer.Write(0);
        }
        screen.PickMusicFiles=()=>Task.FromResult(paths);
    }
    public void Input(MatchScreen screen)
    {
        preferences.Update(.1);
        if(cooldown-->0 || controller.LibraryBusy) return;
        if(down) { ImGui.GetIO().AddMouseButtonEvent(0,false); down=false; step++; cooldown=3; return; }
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
        }
        if(step==6 && controller.Library!.Tracks.Count!=2) throw new InvalidOperationException("Cancel removal deleted a track.");
        if(step==8 && controller.Library!.Tracks.Count!=1) throw new InvalidOperationException("Confirmed removal failed.");
        if(step==10 && (controller.Library!.Tracks.Count!=0 || audio.MusicSource!="builtin")) throw new InvalidOperationException("Removing the last track did not restore built-in music.");
        if(step==11)
        {
            if(!screen.Buttons.ContainsKey("start") || screen.Buttons.ContainsKey("addMusic")) throw new InvalidOperationException("Library did not close to menu.");
            preferences.Update(1); preferences.Dispose();
            if(new AudioSettingsStore(Path.Combine(directory,"audio-settings.json")).Settings.Source!="builtin") throw new InvalidOperationException("Music source was not saved.");
            done=true; return;
        }
        string key=step switch {0=>"musicLibrary",1=>"addMusic",2=>$"musicUse/{first}/planning",3=>$"musicUse/{second}/combat",4=>$"removeMusic/{first}",5=>"cancelRemoveMusic",6=>$"removeMusic/{first}",7=>$"confirmRemoveMusic/{first}",8=>$"removeMusic/{second}",9=>$"confirmRemoveMusic/{second}",10=>"closeMusicLibrary",_=>throw new InvalidOperationException()};
        if(!screen.Buttons.TryGetValue(key,out var point)) return;
        ImGui.GetIO().AddMousePosEvent(point.X,point.Y); ImGui.GetIO().AddMouseButtonEvent(0,true); down=true;
    }
    public bool Verify(int2 size,string? capture)
    {
        if(step==5 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"music-remove.bmp"),size.X,size.Y);
        if(step==4 && capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"music-library.bmp"),size.X,size.Y);
        if(done) Console.WriteLine("Music library UI passed: multi-file import, literal names, phase assignment persistence, cancel/confirm removal, last-track fallback and saved source.");
        return done;
    }
    public void Dispose()
    {
        controller.Dispose(); preferences.Dispose();
        string resolved=Path.GetFullPath(directory);
        if(!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()),StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("digitactics-music-ui-",StringComparison.Ordinal)) throw new InvalidOperationException("Unsafe music UI cleanup.");
        Directory.Delete(resolved,true);
    }
}

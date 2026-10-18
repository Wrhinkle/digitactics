using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Routes requested match music through the saved library without decoding on the render thread.</summary>
internal sealed class CustomMusicController : IDisposable
{
    private readonly GameAudio audio;
    private readonly NativeProfileStore profile;
    private readonly string executable;
    private readonly MusicPlaylist playlist=new();
    private Task<MusicLibrary>? loading;
    private Task<StreamingMusicDecoder>? opening;
    private StreamingMusicDecoder? pending;
    private double pendingFade;
    private string? desiredPhase,observedPhase;
    private bool active,disposed,wasWantedOwn,reloadAgain;
    public MusicLibrary? Library { get; private set; }
    public string? Error { get; private set; }
    public string? CurrentTrackId => playlist.Current?.Id;
    public bool Loading => loading is not null || opening is not null || pending is not null;
    public CustomMusicController(GameAudio audio,NativeProfileStore profile,string executable)
    {
        this.audio=audio; this.profile=profile; this.executable=executable;
        audio.MusicController=this; Reload();
    }
    public void RequestPhase(string? phase)
    {
        if(phase is not (null or "planning" or "combat")) throw new ArgumentException("Invalid music phase.",nameof(phase));
        desiredPhase=phase;
    }
    public void Reload()
    {
        if(disposed) return;
        if(loading is not null) { reloadAgain=true; return; }
        loading=Task.Run(profile.OpenMusicLibrary);
    }
    public void Next()
    {
        if(playlist.Failed && desiredPhase is { } phase) Apply(playlist.Start(phase)); else Apply(playlist.Next());
    }
    private void CancelPending()
    {
        pending?.Dispose(); pending=null;
        if(opening is { } old)
        {
            _=old.ContinueWith(t=> { if(t.IsCompletedSuccessfully)t.Result.Dispose(); else _=t.Exception; },TaskScheduler.Default);
            opening=null;
        }
    }
    private void Apply(MusicTransition? transition)
    {
        if(transition is null) return;
        CancelPending(); active=false;
        if(transition.TrackId is null) { audio.StopCustomMusic(transition.FadeSeconds); return; }
        var library=Library!; string id=transition.TrackId; pendingFade=transition.FadeSeconds;
        opening=Task.Run(()=>new StreamingMusicDecoder(executable,library.OpenAudio(id)));
    }
    private void Failed(string message)
    {
        Error=message; Apply(playlist.PlaybackFailed());
    }
    public void Update()
    {
        if(disposed) return;
        if(loading is { IsCompleted:true } loaded)
        {
            loading=null;
            try
            {
                Library=loaded.GetAwaiter().GetResult(); Error=Library.Error;
                playlist.SetTracks(Library.Tracks.Select(t=>new PlaylistTrack(t.Id,t.Use)));
                if(playlist.Current is { } current && !Library.Tracks.Any(t=>t.Id==current.Id)) Apply(playlist.Next(.5));
                observedPhase=null; wasWantedOwn=false;
            }
            catch(Exception e) { Error="Music library could not be opened: "+e.Message; }
            if(reloadAgain) { reloadAgain=false; Reload(); }
        }
        bool own=audio.MusicSource=="own" && Library?.Tracks.Count>0;
        bool wanted=desiredPhase is not null && !audio.Muted && !audio.MusicOff && audio.MusicVolume>.001;
        if(!own || !wanted)
        {
            Apply(playlist.Stop()); CancelPending(); active=false; observedPhase=null; wasWantedOwn=false;
            audio.BuiltinMusic(wanted?desiredPhase:null); return;
        }
        if(!wasWantedOwn || !playlist.Playing && !playlist.Failed) Apply(playlist.Start(desiredPhase!));
        else if(observedPhase!=desiredPhase) Apply(playlist.ChangePhase(desiredPhase!));
        wasWantedOwn=true; observedPhase=desiredPhase;
        if(opening is { IsCompleted:true } opened)
        {
            opening=null;
            try { pending=opened.GetAwaiter().GetResult(); }
            catch(Exception e) { Failed("Music file could not be opened: "+e.Message); }
        }
        if(pending is { } decoder && decoder.Ready.IsCompleted)
        {
            pending=null;
            if(decoder.Ready.GetAwaiter().GetResult())
            {
                audio.PlayCustomMusic(decoder,pendingFade); playlist.PlaybackStarted(); active=true; Error=null;
            }
            else { string failure=decoder.Error??"Music file contains no playable audio."; decoder.Dispose(); Failed(failure); }
        }
        if(active && audio.CustomMusicEnded)
        {
            active=false;
            if(audio.CustomMusicError is { } error) Failed(error); else Apply(playlist.Next(.2));
        }
    }
    public void Dispose()
    {
        if(disposed) return; disposed=true; CancelPending(); audio.StopCustomMusic(0);
        if(ReferenceEquals(audio.MusicController,this)) audio.MusicController=null;
        if(loading is { } old) _=old.ContinueWith(t=> { _=t.Exception; },TaskScheduler.Default);
    }
}

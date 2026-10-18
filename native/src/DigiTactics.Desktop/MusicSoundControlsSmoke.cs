using System.Reflection;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Real match Sound inputs, source chooser semantics, active playback and slider preview dispatch.</summary>
internal sealed class MusicSoundControlsSmoke
{
    private const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    private readonly MatchScreen screen;
    private readonly GameAudio audio;
    private readonly CustomMusicController controller;
    private readonly string[] paths;
    private int step,cooldown=3,pickerCalls,frames,played,previews;
    private bool down,removed,done;
    private string? previousTrack,captureState;
    public string PreviewEvidence=>previews==3?"both source slider previews dispatched":"slider preview dispatch skipped without an audio device";
    public MusicSoundControlsSmoke(MatchScreen screen,GameAudio audio,CustomMusicController controller,string[] paths)
    {
        this.screen=screen;this.audio=audio;this.controller=controller;this.paths=paths;
        var catalog=(ContentCatalog)typeof(MatchScreen).GetField("catalog",Private)!.GetValue(screen)!;
        var session=new MatchPlanningSession(catalog.Variants.Single(r=>r.Id=="faction-wars/normal"),771u,Enumerable.Range(0,8).Select(i=>new MatchSeat("Music Tamer "+i,i==0)).ToArray());
        if(session.Units.Carousel.Active){session.StartCombat();session.FinishCombat();session.Advance();}
        typeof(MatchScreen).GetProperty(nameof(MatchScreen.Session))!.SetValue(screen,session);
        typeof(MatchScreen).GetField("localPaused",Private)!.SetValue(screen,true);
        audio.Muted=audio.MusicOff=false;audio.MusicVolume=.01f;audio.Music("planning");
        screen.PickMusicFiles=()=>{pickerCalls++;return Task.FromResult(Array.Empty<string>());};
    }
    private static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
    private void Click(string id,bool slider=false)
    {
        if(!screen.Buttons.TryGetValue(id,out var at))return;
        if(id is not ("audioSettings" or "mute" or "closeAudio" or "closeMusicLibrary"))
        {
            var viewport=screen.MatchSoundScrollBounds;
            float halfHeight=slider?18:14;
            if(at.Y-halfHeight<viewport.Min.Y||at.Y+halfHeight>viewport.Max.Y)
            {
                ImGui.GetIO().AddMousePosEvent(viewport.Max.X-3,(viewport.Min.Y+viewport.Max.Y)/2);
                ImGui.GetIO().AddMouseWheelEvent(0,at.Y<viewport.Min.Y+halfHeight?2:-2);cooldown=3;return;
            }
        }
        if(slider){at.X+=38;played=audio.Played;}
        ImGui.GetIO().AddMousePosEvent(at.X,at.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);down=true;
    }
    public void Input()
    {
        if(done)return;
        if(++frames>950)throw new InvalidOperationException($"Live Sound UI timed out at step{step}; tracks={controller.Library?.Tracks.Count}, playing={controller.Playing}, loading={controller.Loading}, error={controller.Error}");
        if(controller.LibraryBusy){cooldown=3;return;}
        if(cooldown-->0)return;
        if(down){ImGui.GetIO().AddMouseButtonEvent(0,false);down=false;step++;cooldown=3;return;}
        switch(step)
        {
            case 0:Click("audioSettings");break;
            case 1:
                Require(!screen.MatchMusicNextEnabled,"Empty music library enables Next track.");captureState="sound-empty";Click("matchMusicOwn");break;
            case 2:
                Require(pickerCalls==1&&audio.MusicSource=="builtin"&&controller.Library!.Tracks.Count==0,"Empty live own-source did not open chooser or cancel preserved the wrong source.");
                screen.PickMusicFiles=()=>{pickerCalls++;return Task.FromResult(paths);};Click("matchMusicOwn");break;
            case 3:
                if(controller.Library?.Tracks.Count!=2||!controller.Playing||!screen.MatchMusicNextEnabled)return;
                Require(pickerCalls==2&&audio.MusicSource=="own","Live picker import did not select own source.");
                previousTrack=controller.CurrentTrackId;captureState="sound-playing";Click("nextMusic");break;
            case 4:
                if(!controller.Playing)return;
                Require(controller.CurrentTrackId!=previousTrack,"Actual Next track button did not advance playback.");Click("mute");break;
            case 5:
                Require(audio.Muted&&!screen.MatchMusicNextEnabled,"Muted music enables Next track.");Click("mute");break;
            case 6:if(controller.Playing&&screen.MatchMusicNextEnabled)Click("matchMusicOwn");break;
            case 7:
                Require(audio.MusicSource=="builtin"&&!screen.MatchMusicNextEnabled,"Built-in source enables custom Next track.");Click("matchMusicOwn");break;
            case 8:
                if(!controller.Playing)return;
                if(!removed){Require(controller.Remove(controller.Library!.Tracks.First(t=>t.Id!=controller.CurrentTrackId).Id),"Could not seed single-track UI fixture.");removed=true;return;}
                Require(controller.Library!.Tracks.Count==1&&!screen.MatchMusicNextEnabled,"Single-track library enables Next track.");
                captureState="sound-one-track";Click("nextMusic");break;
            case 9:Click("matchEffectsVolume",true);break;
            case 10:
                Require(!audio.Available||audio.Played>played,"Effects slider release did not preview the original buy sound.");if(audio.Available)previews|=1;audio.EffectsVolume=0;Click("matchBoardVolume",true);break;
            case 11:
                Require(!audio.Available||audio.Played>played,"Soundboard slider release did not preview the original Digivice clip.");if(audio.Available)previews|=2;audio.BoardVolume=0;Click("musicLibrary");break;
            case 12:
                Require(!screen.MusicNextEnabled,"Single-track Next is enabled in nested music library.");captureState="sound-library";Click("closeMusicLibrary");break;
            case 13:
                Require(screen.Buttons.ContainsKey("matchMusicOwn")&&!screen.Buttons.ContainsKey("closeMusicLibrary"),"Music Back did not return to live Sound.");Click("closeAudio");break;
            case 14:
                Require(!screen.Buttons.ContainsKey("matchMusicOwn")&&screen.Session is not null,"Closing Sound lost the match or retained popup inputs.");
                audio.MusicSource="builtin";typeof(MatchScreen).GetMethod("LeaveMatch",Private)!.Invoke(screen,null);controller.Update();done=true;break;
        }
    }
    public bool Verify(int2 size,string? capture)
    {
        if(captureState is {} state&&capture is not null){FrameCapture.SaveBitmap(Path.ChangeExtension(capture,state+".bmp"),size.X,size.Y);captureState=null;}
        return done;
    }
}

using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal bool MatchMusicNextEnabled {get;private set;}
    internal string MusicStatusText {get;private set;}="";
    internal (float2 Min,float2 Max) MusicStatusBounds {get;private set;}
    internal (float2 Min,float2 Max) MatchSoundScrollBounds {get;private set;}
    // The source popover enables Next only while custom music is actually playing.
    // A selected track ID also exists while its decoder is still opening.
    private bool CanAdvanceMusic(CustomMusicController controller)=>audio.MusicSource=="own"&&controller.Playing&&!audio.Muted&&!audio.MusicOff&&audio.MusicVolume>.001f&&controller.Library?.Tracks.Count>1;
    private void MusicStatus(CustomMusicController controller)
    {
        string? current=controller.Playing?controller.Library?.Tracks.FirstOrDefault(t=>t.Id==controller.CurrentTrackId)?.Name:null;
        if(audio.Muted||audio.MusicOff)MusicStatusText="Music is off.";
        else if(current is not null)MusicStatusText="Now playing: "+current;
        else if(audio.BuiltinMusicPhase is {} phase)
            MusicStatusText=$"Now playing: {audio.BuiltinTrackName(phase)} ({(phase=="combat"?"combat":"planning")} theme)";
        else if(audio.MusicSource=="own"&&controller.Library?.Tracks.Count>0)MusicStatusText="Your files play during a game.";
        else MusicStatusText=$"Built-in: {audio.BuiltinTrackName("planning")} for planning, {audio.BuiltinTrackName("combat")} for combat.";
        UiText.Wrapped(MusicStatusText);
        MusicStatusBounds=(ImGui.GetItemRectMin(),ImGui.GetItemRectMax());
    }
    private void AudioControls()
    {
        MatchSoundScrollBounds=(ImGui.GetWindowDrawList().GetClipRectMin(),ImGui.GetWindowDrawList().GetClipRectMax());
        float effects=audio.EffectsVolume,music=audio.MusicVolume,board=audio.BoardVolume;
        // Source soundui.js previews slider releases even when menu sounds are off.
        if(ClientSettingsSlider("matchEffectsVolume","Effects volume",ref effects))audio.EffectsVolume=effects;
        if(ImGui.IsItemDeactivatedAfterEdit())audio.Play("buy");
        if(ClientSettingsSlider("matchMusicVolume","Music volume",ref music))audio.MusicVolume=music;
        if(ClientSettingsSlider("matchBoardVolume","Soundboard volume",ref board))audio.BoardVolume=board;
        if(ImGui.IsItemDeactivatedAfterEdit())audio.PlayBoard("digivice");
        bool muteOthers=audio.MuteOthers;
        if(ImGui.Checkbox("Mute other players' soundboard"u8,ref muteOthers))audio.MuteOthers=muteOthers;
        bool musicOff=audio.MusicOff,muted=audio.Muted;
        if(ImGui.Checkbox("Music off"u8,ref musicOff))audio.MusicOff=musicOff;
        ImGui.SameLine();if(ImGui.Checkbox("Mute all"u8,ref muted))audio.Muted=muted;
        if(audio.Error is {} error)UiText.Wrapped(error);
        MatchMusicNextEnabled=false;
        if(audio.MusicController is {} controller)
        {
            if(Button("musicLibrary","Manage music..."))showMusicLibrary=true;
            ImGui.BeginDisabled(controller.LibraryBusy||musicPicker is not null||controller.Library is null||closeAfterFileOperations);
            bool own=audio.MusicSource=="own"&&controller.Library?.Tracks.Count>0;
            if(ImGui.Checkbox("Use my music"u8,ref own))
            {
                if(own&&controller.Library?.Tracks.Count is not >0)BeginMusicSelection();
                else audio.MusicSource=own?"own":"builtin";
            }
            Buttons["matchMusicOwn"]=(ImGui.GetItemRectMin()+ImGui.GetItemRectMax())*.5f;
            if(controller.Library is {} library)
            {
                UiText.Wrapped($"{library.Tracks.Count} saved music "+(library.Tracks.Count==1?"track":"tracks"));
                MusicStatus(controller);
                MatchMusicNextEnabled=CanAdvanceMusic(controller);
                ImGui.BeginDisabled(!MatchMusicNextEnabled);
                if(Button("nextMusic","Next track"))controller.Next();
                ImGui.EndDisabled();
            }
            ImGui.EndDisabled();
            if(controller.Loading||musicPicker is not null)ImGui.Text("Loading music..."u8);
            if(musicPickerError is {} pickerError)UiText.Wrapped(pickerError);
            if(controller.EditError is {} editError)UiText.Wrapped(editError);
            if(controller.Error is {} musicError)UiText.Wrapped(musicError);
        }
        if(audio.SettingsError is {} settingsError)UiText.Wrapped(settingsError);
    }
}

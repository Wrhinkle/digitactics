using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool showMusicLibrary,clearMusic;
    private string? removeMusicId,musicPickerError;
    private Task<string[]>? musicPicker;
    private string? musicPickerGeneration;
    internal Func<Task<string[]>> PickMusicFiles { get; set; }=MusicFilePicker.Open;
    internal bool MusicSelectionPending=>musicPicker is not null;
    private void UpdateMusicSelection()
    {
        if(musicPicker is not { IsCompleted:true } picked)return;
        musicPicker=null;
        try
        {
            var paths=picked.GetAwaiter().GetResult();
            if(musicPickerGeneration!=profile?.DataDirectory)musicPickerError="Music selection canceled because the active save changed. Choose the files again.";
            else if(paths.Length>0 && audio.MusicController?.AddFiles(paths)!=true)musicPickerError="Music library is busy. Try adding the files again.";
        }
        catch(Exception e){musicPickerError="Could not choose music: "+e.Message;}
        finally{musicPickerGeneration=null;}
    }
    private void MusicLibraryPanel()
    {
        if(!showMusicLibrary || audio.MusicController is not { } controller) return;
        float width=Math.Min(640,size.X-24),height=Math.Min(560,size.Y-40);
        Window("Music library",(size.X-width)/2,(size.Y-height)/2,width,height);
        UiText.Wrapped("Your audio files stay on this computer. Choose where each track plays."u8);
        ImGui.BeginDisabled(controller.LibraryBusy || musicPicker is not null || controller.Library is null || closeAfterFileOperations);
        if(Button("addMusic","Add audio files..."))
        {
            musicPickerError=null;
            try { musicPickerGeneration=profile?.DataDirectory;musicPicker=PickMusicFiles(); } catch(Exception e) { musicPickerError=e.Message; }
        }
        ImGui.SameLine();
        ImGui.BeginDisabled(controller.Library?.Tracks.Count is not >0);
        if(Button("clearMusic","Clear all")) { clearMusic=true; removeMusicId=null; }
        ImGui.EndDisabled();
        ImGui.EndDisabled(); ImGui.SameLine();
        if(Button("closeMusicLibrary","Close")) { showMusicLibrary=false; removeMusicId=null; clearMusic=false; }
        ImGui.BeginChild("musicStatus"u8,new(0,76),ImGuiChildFlags.None);
        if(controller.LibraryBusy || musicPicker is not null) ImGui.Text("Loading music..."u8); else ImGui.Text(" "u8);
        if(musicPickerError is { } pickerError) UiText.Wrapped(pickerError);
        if(controller.EditError is { } editError) UiText.Wrapped(editError);
        else if(controller.Library?.Error is { } error) UiText.Wrapped(error);
        if(controller.Error is { } playbackError && playbackError!=controller.Library?.Error) UiText.Wrapped(playbackError);
        ImGui.EndChild();
        if(clearMusic)
        {
            UiText.Wrapped("Remove every track from your music library? Original files are unchanged."u8);
            ImGui.BeginDisabled(controller.LibraryBusy);
            if(Button("confirmClearMusic","Remove all tracks")) { controller.Clear(); clearMusic=false; }
            ImGui.SameLine(); if(Button("cancelClearMusic","Cancel")) clearMusic=false;
            ImGui.EndDisabled();
        }
        ImGui.BeginChild("musicTracks"u8,new(0,0),ImGuiChildFlags.None);
        if(controller.Library is { } library)
        {
            if(library.Tracks.Count==0) UiText.Wrapped("No saved tracks yet. Add audio files or import a profile backup containing music."u8);
            foreach(var track in library.Tracks)
            {
                ImGui.Separator(); UiText.Wrapped(track.Name);
                ImGui.BeginDisabled(controller.LibraryBusy);
                foreach(string use in new[] {"both","planning","combat"})
                {
                    if(use!="both") ImGui.SameLine();
                    if(Button($"musicUse/{track.Id}/{use}",use=="both"?"Both":use=="planning"?"Planning":"Combat",active:track.Use==use)) controller.SetUse(track.Id,use);
                }
                ImGui.SameLine();
                if(Button("removeMusic/"+track.Id,"Remove")) { removeMusicId=track.Id; clearMusic=false; }
                if(removeMusicId==track.Id)
                {
                    UiText.Wrapped("Remove this track from your music library?"u8);
                    if(Button("confirmRemoveMusic/"+track.Id,"Remove track")) { controller.Remove(track.Id); removeMusicId=null; }
                    ImGui.SameLine(); if(Button("cancelRemoveMusic","Cancel")) removeMusicId=null;
                }
                ImGui.EndDisabled();
            }
        }
        ImGui.EndChild(); ImGui.End();
    }
}

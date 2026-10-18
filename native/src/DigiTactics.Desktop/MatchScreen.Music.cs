using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool showMusicLibrary,clearMusic;
    private string? removeMusicId,musicPickerError;
    private Task<string[]>? musicPicker;
    private string? musicPickerGeneration;
    internal Func<Task<string[]>> PickMusicFiles { get; set; }=MusicFilePicker.Open;
    internal bool MusicSelectionPending=>musicPicker is not null;
    internal float2 MusicScrollMin {get;private set;}
    internal float2 MusicScrollMax {get;private set;}
    internal bool MusicNextEnabled {get;private set;}
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
        float width=Math.Min(720,size.X-32),height=Math.Min(660,size.Y-ClientPageTop-28);
        ClientPageWindow("Music library",(size.X-width)/2,Math.Max(ClientPageTop+12,(size.Y-height)/2),width,height);
        if(HomeButton("closeMusicLibrary","Back",100,36)) { showMusicLibrary=false; removeMusicId=null; clearMusic=false; }
        ImGui.SameLine();ClientPageTitle("YOUR MUSIC FILES",24);ImGui.Spacing();
        ImGui.BeginChild("Music library content"u8,new(0,0));
        MusicScrollMin=ImGui.GetWindowPos();MusicScrollMax=MusicScrollMin+ImGui.GetWindowSize();
        ClientCardHeading("MUSIC SOURCE");
        bool own=audio.MusicSource=="own"&&controller.Library?.Tracks.Count>0;
        ImGui.BeginDisabled(controller.LibraryBusy || musicPicker is not null || controller.Library is null || closeAfterFileOperations);
        if(Button("musicBuiltin","Built-in soundtrack",240,36,!own))audio.MusicSource="builtin";
        ImGui.SameLine();
        if(Button("musicOwn","My music files",220,36,own))
        {
            if(controller.Library?.Tracks.Count>0)audio.MusicSource="own";
            else BeginMusicSelection();
        }
        ImGui.EndDisabled();
        MusicStatus(controller);
        ImGui.PushStyleColor(ImGuiCol.Text,ClientMuted);
        UiText.Wrapped("Add audio files you own. They stay on this computer and are never uploaded. Your files replace the built-in soundtrack and play in shuffled order."u8);ImGui.PopStyleColor();ImGui.Spacing();
        ImGui.BeginDisabled(controller.LibraryBusy || musicPicker is not null || controller.Library is null || closeAfterFileOperations);
        if(Button("addMusic","Add audio files...",200,36))BeginMusicSelection();
        ImGui.SameLine();
        MusicNextEnabled=CanAdvanceMusic(controller);
        ImGui.BeginDisabled(!MusicNextEnabled);
        if(Button("nextMusic","Next track",140,36))controller.Next();
        ImGui.EndDisabled();ImGui.SameLine();
        ImGui.BeginDisabled(controller.Library?.Tracks.Count is not >0);
        if(Button("clearMusic","Remove all",140,36)) { clearMusic=true; removeMusicId=null; }
        ImGui.EndDisabled();
        ImGui.EndDisabled();
        if(controller.LibraryBusy || musicPicker is not null) ImGui.Text("Loading music..."u8);
        if(musicPickerError is { } pickerError) UiText.Wrapped(pickerError);
        if(controller.EditError is { } editError) UiText.Wrapped(editError);
        else if(controller.Library?.Error is { } error) UiText.Wrapped(error);
        if(controller.Error is { } playbackError && playbackError!=controller.Library?.Error) UiText.Wrapped(playbackError);
        if(clearMusic)
        {
            ImGui.Spacing();
            UiText.Wrapped("Remove every track from your music library? Original files are unchanged."u8);
            ImGui.BeginDisabled(controller.LibraryBusy);
            if(Button("confirmClearMusic","Remove all tracks",220,36)) { controller.Clear(); clearMusic=false; }
            ImGui.SameLine(); if(Button("cancelClearMusic","Cancel",120,36)) clearMusic=false;
            ImGui.EndDisabled();
        }
        ImGui.Spacing();ClientCardHeading("SAVED TRACKS");
        if(controller.Library is { } library)
        {
            if(library.Tracks.Count==0) UiText.Wrapped("No saved tracks yet. Add audio files or import a profile backup containing music."u8);
            foreach(var track in library.Tracks)
            {
                ImGui.Separator();ImGui.PushStyleColor(ImGuiCol.Text,controller.CurrentTrackId==track.Id?new ImColor8(88,217,204):ClientInk);UiText.Wrapped(track.Name);ImGui.PopStyleColor();
                if(ImGui.IsItemHovered())ResultTooltip($"{track.Name} ({track.Size/1048576d:F1} MB)");
                ImGui.BeginDisabled(controller.LibraryBusy);
                foreach(string use in new[] {"both","planning","combat"})
                {
                    if(use!="both") ImGui.SameLine();
                    if(Button($"musicUse/{track.Id}/{use}",use=="both"?"Planning + combat":use=="planning"?"Planning":"Combat",use=="both"?190:130,36,track.Use==use)) controller.SetUse(track.Id,use);
                }
                ImGui.SameLine();
                if(Button("removeMusic/"+track.Id,"Remove",100,36)) { removeMusicId=track.Id; clearMusic=false; }
                if(removeMusicId==track.Id)
                {
                    UiText.Wrapped("Remove this track from your music library?"u8);
                    if(Button("confirmRemoveMusic/"+track.Id,"Remove track",180,36)) { controller.Remove(track.Id); removeMusicId=null; }
                    ImGui.SameLine(); if(Button("cancelRemoveMusic","Cancel",120,36)) removeMusicId=null;
                }
                ImGui.EndDisabled();
            }
        }
        ImGui.EndChild(); ImGui.End();
    }
    private void BeginMusicSelection()
    {
        musicPickerError=null;
        try { musicPickerGeneration=profile?.DataDirectory;musicPicker=PickMusicFiles(); } catch(Exception e) { musicPickerError=e.Message; }
    }
}

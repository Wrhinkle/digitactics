using Brutal.ImGuiApi;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool showSaveData, confirmResetSave;
    private readonly ImInputString resetSaveText = new(64);
    private Task<string?>? backupSelection;
    private Task<LegacyProfileSummary?>? backupExport;
    private string? backupDestination, saveDataMessage;
    internal bool SaveDataVisible => showSaveData;
    internal Func<string, Task<string?>> BackupPicker { private get; set; } = ProfileFilePicker.Save;
    internal Func<string, AudioSettings, Task<LegacyProfileSummary?>>? BackupWriter { private get; set; }
    internal bool SaveDataOperationPending => SaveDataBusy;
    internal string? SaveDataStatus => saveDataMessage;
    internal bool ResetSaveConfirmation => confirmResetSave;
    internal bool ResetSaveEnabled => NativeProfileStore.ConfirmsReset(resetSaveText.ToString()) && CanResetSave;
    private bool SaveDataBusy => backupSelection is not null || backupExport is not null;
    private bool CanResetSave => profile is not null && ClientHomeVisible && networkRoom is null && steam?.Room is null && training is null && !FileOperationsPending && !closeAfterFileOperations;
    private AudioSettings CurrentAudioSettings => new(audio.MusicVolume, audio.EffectsVolume, audio.Muted, audio.MusicOff, audio.BoardVolume, audio.MuteOthers, audio.MusicSource);

    private void OpenSaveData() { showSaveData = true; confirmResetSave = false; resetSaveText.SetValue("".AsSpan()); }
    private void UpdateSaveData()
    {
        if (backupSelection is { IsCompleted: true } selection)
        {
            backupSelection = null;
            try
            {
                backupDestination = selection.GetAwaiter().GetResult();
                if (backupDestination is null) saveDataMessage = "Backup canceled. Your save was not changed.";
                else
                {
                    var current = CurrentAudioSettings; string destination = backupDestination;
                    backupExport = BackupWriter?.Invoke(destination,current)??Task.Run(() => profile!.ExportBackup(destination, current));
                }
            }
            catch (Exception error) { saveDataMessage = "Backup could not be created: " + error.Message;FileOperationCloseFailed(saveDataMessage); }
        }
        if (backupExport is { IsCompleted: true } exporting)
        {
            backupExport = null;
            try
            {
                var result = exporting.GetAwaiter().GetResult();
                saveDataMessage = result is null ? "Backup canceled." : $"Backup saved: {backupDestination}\n{result.StorageEntries} saved entries and {result.MusicTracks} music files.";
            }
            catch (Exception error) { saveDataMessage = "Backup could not be created: " + error.Message;FileOperationCloseFailed(saveDataMessage); }
        }
    }
    private void SaveDataPanel()
    {
        float width=Math.Min(660,size.X-48),height=Math.Min(620,size.Y-128);
        ClientPageWindow("Save data",(size.X-width)/2,108,width,height);
        bool escape=ImGui.IsKeyPressed(ImGuiKey.Escape,false);
        if(escape&&confirmResetSave){confirmResetSave=false;escape=false;}
        if (HomeButton("closeSaveData", "Back", 110, 36) || escape) {confirmResetSave = false;BackClient(); ImGui.End(); return; }
        ImGui.SameLine();ClientPageTitle("SAVE DATA",26);ImGui.Spacing();
        ProfileControls(showSaveLink:false);ImGui.Spacing();SaveDataControls(showManageLink:false); ImGui.End();
    }
    private void SaveDataControls(bool showManageLink=true)
    {
        if (profile is null) return;
        ClientCardHeading("DATA");
        if(showManageLink && HomeButton("saveData","Manage save / import backup",Math.Min(280,ImGui.GetContentRegionAvail().X),38))
        NavigateClient("savedata");
        UiText.Wrapped("Your Bits, cosmetics, Little Legend levels, Digi-Farm, match history and settings are saved on this computer."u8);
        ImGui.BeginDisabled(!CanResetSave);
        ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(72,19,29));ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(180,66,82));
        if (Button("resetSave", "Reset save...", Math.Min(250,ImGui.GetContentRegionAvail().X), 38)) { confirmResetSave = true; resetSaveText.SetValue("".AsSpan()); ImGui.OpenPopup("Reset confirmation"u8); }
        ImGui.PopStyleColor(2);
        ImGui.EndDisabled();
        if (!CanResetSave && !SaveDataBusy) UiText.Wrapped("Leave your match or lobby and finish any file operation before resetting your save."u8);
        ImGui.Spacing();
        ImGui.BeginDisabled(FileOperationsPending || closeAfterFileOperations);
        if (HomeButton("exportSave", "Export save backup", Math.Min(250,ImGui.GetContentRegionAvail().X), 38))
        {
            try { backupSelection = BackupPicker($"digi-tactics-profile-{DateTime.UtcNow:yyyy-MM-dd}.json"); saveDataMessage = null; }
            catch (Exception error) { saveDataMessage = "The file chooser could not open: " + error.Message; }
        }
        ImGui.EndDisabled();
        UiText.Wrapped("Includes progress, settings and your music files. Keep this backup private."u8);
        if (SaveDataBusy) ImGui.Text(backupSelection is not null ? "Choose where to save the backup..."u8 : "Preparing backup..."u8);
        DrawResetSaveModal();
        if (saveDataMessage is not null) { ImGui.Separator(); UiText.Wrapped(saveDataMessage); }
    }
    private void DrawResetSaveModal()
    {
        if (profile is null) return;
        if (confirmResetSave||ImGui.IsPopupOpen("Reset confirmation"u8))
        {
            float width=Math.Min(540,size.X-48),height=Math.Min(410,size.Y-128);
            ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(180,66,82));
            ImGui.SetNextWindowPos(new((size.X-width)/2,Math.Max(108,(size.Y-height)/2)));ImGui.SetNextWindowSize(new(width,height));
            bool visible=ImGui.BeginPopupModal("Reset confirmation"u8,ref confirmResetSave,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoResize|ImGuiWindowFlags.NoMove);ImGui.PopStyleColor();
            if(visible)
            {
            ClientPageTitle("RESET YOUR SAVE?",25,new ImColor8(240,180,175));ImGui.Spacing();
            UiText.Wrapped("This clears your active profile: Bits, cosmetics, Little Legend levels, Digi-Farm, match history, settings and music. Export a backup first if you want to restore it later."u8);
            UiText.Wrapped("Previously exported backups and preserved import archives are kept."u8);
            ImGui.Text("Type RESET to confirm"u8);ImGui.SetNextItemWidth(-1);if(ImGui.IsWindowAppearing())ImGui.SetKeyboardFocusHere();ImGui.InputText("##resetSaveText"u8, resetSaveText);
            Buttons["resetSaveText"] = (ImGui.GetItemRectMin() + ImGui.GetItemRectMax()) * .5f;
            ImGui.Spacing();
            if (HomeButton("cancelResetSave", "Cancel", 130, 38)) { confirmResetSave = false; resetSaveText.SetValue("".AsSpan()); }
            ImGui.SameLine(); ImGui.BeginDisabled(!ResetSaveEnabled);
            ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(104,29,40));ImGui.PushStyleColor(ImGuiCol.ButtonHovered,new ImColor8(143,37,52));
            if (Button("confirmResetSave", "Delete everything", 220, 38))
            {
                if (profile.Reset(resetSaveText.ToString()))
                {
                    if(PostgameHomeVisible)LeaveMatch();
                    profileCandidate = null; profileMessage = null; showProfile = showCollection = showFarm = showMusicLibrary = false;
                    farm = new(profile); farmView?.Dispose(); farmView = null; farmPet = null; farmNotices.Clear();
                    homeLegend?.Dispose(); homeLegend = null; ClearEvolutionPreview(); DisposeCollectionPreview();
                    audio.Music(null); audio.MusicController?.Reload();
                    confirmResetSave = false; resetSaveText.SetValue("".AsSpan()); saveDataMessage = "Your save was reset. You can start fresh or import a backup from Home.";
                }
                else saveDataMessage = profile.Error ?? "Your save was not reset.";
            }
            ImGui.PopStyleColor(2);ImGui.EndDisabled();
            if(ImGui.IsKeyPressed(ImGuiKey.Escape,false))confirmResetSave=false;
            if(!confirmResetSave)ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            }
        }
    }
}

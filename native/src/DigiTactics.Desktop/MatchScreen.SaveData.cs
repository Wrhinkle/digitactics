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
        Window("Save data", size.X / 2 - 285, 70, 570, Math.Min(600, size.Y - 94));
        if (Button("closeSaveData", "Back", 110, 32) || ImGui.IsKeyPressed(ImGuiKey.Escape)) { showSaveData = confirmResetSave = false; ImGui.End(); return; }
        SaveDataControls(); ImGui.End();
    }
    private void SaveDataControls()
    {
        if (profile is null) return;
        ImGui.SeparatorText("Save data"u8);
        UiText.Wrapped("Your Bits, cosmetics, Little Legend levels, Digi-Farm, match history and settings are saved on this computer."u8);
        ImGui.BeginDisabled(FileOperationsPending || closeAfterFileOperations);
        if (Button("exportSave", "Export save backup", 220, 34))
        {
            try { backupSelection = BackupPicker($"digi-tactics-profile-{DateTime.UtcNow:yyyy-MM-dd}.json"); saveDataMessage = null; }
            catch (Exception error) { saveDataMessage = "The file chooser could not open: " + error.Message; }
        }
        ImGui.EndDisabled();
        UiText.Wrapped("Includes progress, settings and your music files. Keep this backup private."u8);
        if (SaveDataBusy) ImGui.Text(backupSelection is not null ? "Choose where to save the backup..."u8 : "Preparing backup..."u8);
        ImGui.Separator();
        ImGui.BeginDisabled(!CanResetSave);
        if (Button("resetSave", "Reset save...", 220, 34)) { confirmResetSave = true; resetSaveText.SetValue("".AsSpan()); }
        ImGui.EndDisabled();
        if (!CanResetSave && !SaveDataBusy) UiText.Wrapped("Leave your match or lobby and finish any file operation before resetting your save."u8);
        if (confirmResetSave)
        {
            ImGui.SeparatorText("Reset your save?"u8);
            UiText.Wrapped("This clears your active profile: Bits, cosmetics, Little Legend levels, Digi-Farm, match history, settings and music. Export a backup first if you want to restore it later."u8);
            UiText.Wrapped("Previously exported backups and preserved import archives are kept."u8);
            ImGui.Text("Type RESET to confirm"u8); ImGui.InputText("##resetSaveText"u8, resetSaveText);
            Buttons["resetSaveText"] = (ImGui.GetItemRectMin() + ImGui.GetItemRectMax()) * .5f;
            if (Button("cancelResetSave", "Cancel", 130, 32)) { confirmResetSave = false; resetSaveText.SetValue("".AsSpan()); }
            ImGui.SameLine(); ImGui.BeginDisabled(!ResetSaveEnabled);
            if (Button("confirmResetSave", "Delete everything", 190, 32))
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
            ImGui.EndDisabled();
        }
        if (saveDataMessage is not null) { ImGui.Separator(); UiText.Wrapped(saveDataMessage); }
    }
}

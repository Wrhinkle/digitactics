using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool closeAfterFileOperations;
    private string? closeObservedMusicError,fileCloseError;
    internal bool FileOperationsPending=>SaveDataBusy||musicPicker is not null||profilePreview is not null||audio.MusicController?.LibraryBusy==true;
    internal bool CloseDeferred=>closeAfterFileOperations&&FileOperationsPending;
    internal bool ExitAfterFileOperationsReady=>closeAfterFileOperations&&!FileOperationsPending;
    internal void RequestWindowClose(){closeAfterFileOperations=true;closeObservedMusicError=audio.MusicController?.EditError;fileCloseError=null;}
    private void FileOperationCloseFailed(string error)
    {
        if(!closeAfterFileOperations)return;
        closeAfterFileOperations=false;fileCloseError=error;
    }
    internal void UpdateFileOperationClose()
    {
        if(audio.MusicController?.EditError is { } error&&error!=closeObservedMusicError)FileOperationCloseFailed("Music could not be saved: "+error);
    }
    private static void ObserveAbandonedSelection(Task? task)
    {
        if(task is not null)_=task.ContinueWith(t=>{_=t.Exception;},TaskContinuationOptions.OnlyOnFaulted);
    }
    private void DrawFileOperationClose()
    {
        if(fileCloseError is { } error)
        {
            Window("File operation could not finish",Math.Max(12,size.X/2-260),Math.Max(12,size.Y/2-100),520,200);
            UiText.Wrapped("The game stayed open because a file operation failed."u8);UiText.Wrapped(error);
            if(Button("dismissFileOperationError","Stay in game",180,32))fileCloseError=null;
            ImGui.End();return;
        }
        if(!CloseDeferred)return;
        Window("Finishing file operations",Math.Max(12,size.X/2-260),Math.Max(12,size.Y/2-100),520,200);
        UiText.Wrapped("Your game will close after the current file operation finishes. Accepted backup and music changes are still being saved."u8);
        bool choosing=backupSelection is not null||musicPicker is not null||profilePreview is not null;
        if(choosing)UiText.Wrapped("Finish or cancel the file chooser. You can also cancel its selection below."u8);
        if(Button("stayAfterFileOperation","Stay in game",180,32))closeAfterFileOperations=false;
        if(choosing)
        {
            ImGui.SameLine();
            if(Button("cancelSelectionAndClose","Cancel selection and close",260,32))
            {
                // A selected destination/import already accepted by Update is a writer and is retained.
                ObserveAbandonedSelection(backupSelection);backupSelection=null;
                ObserveAbandonedSelection(musicPicker);musicPicker=null;musicPickerGeneration=null;
                ObserveAbandonedSelection(profilePreview);profilePreview=null;
            }
        }
        ImGui.End();
    }
}

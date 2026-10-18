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
            FileOperationWindow("File operation could not finish");ClientPageTitle("FILE OPERATION FAILED",24);
            ImGui.BeginChild("File operation error message"u8,new(0,130));
            UiText.Wrapped("The game stayed open because a file operation failed."u8);UiText.Wrapped(error);
            ImGui.EndChild();
            if(Button("dismissFileOperationError","Stay in game",180,36))fileCloseError=null;
            ImGui.End();return;
        }
        if(!CloseDeferred)return;
        FileOperationWindow("Finishing file operations");ClientPageTitle("FINISHING FILE OPERATIONS",24);
        ImGui.BeginChild("File operation progress message"u8,new(0,130));
        UiText.Wrapped("Your game will close after the current file operation finishes. Accepted backup and music changes are still being saved."u8);
        bool choosing=backupSelection is not null||musicPicker is not null||profilePreview is not null;
        if(choosing)UiText.Wrapped("Finish or cancel the file chooser. You can also cancel its selection below."u8);
        ImGui.EndChild();
        if(Button("stayAfterFileOperation","Stay in game",180,36))closeAfterFileOperations=false;
        if(choosing)
        {
            ImGui.SameLine();
            if(Button("cancelSelectionAndClose","Cancel selection and close",280,36))
            {
                // A selected destination/import already accepted by Update is a writer and is retained.
                ObserveAbandonedSelection(backupSelection);backupSelection=null;
                ObserveAbandonedSelection(musicPicker);musicPicker=null;musicPickerGeneration=null;
                ObserveAbandonedSelection(profilePreview);profilePreview=null;
            }
        }
        ImGui.End();
    }
    private void FileOperationWindow(string title)
    {
        float width=Math.Min(600,size.X-32),height=250;
        ClientPageWindow(title,(size.X-width)/2,Math.Max(ClientPageTop+12,(size.Y-height)/2),width,height);
    }
}

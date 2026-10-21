using Brutal.ImGuiApi;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    public NativeUpdateClient? UpdateClient {get;set;}
    public bool RestartForUpdateRequested {get;private set;}
    private string? updateError;
    private string? deferredUpdate;
    private void UpdatePanel()
    {
        if(!ClientHomeVisible || networkRoom is not null || steam?.Room is not null || UpdateClient is null)return;
        UpdateClient.Refresh();var update=UpdateClient.Status;
        if(update is null || update.Phase is "idle" or "current" or "unavailable" or "checking")return;
        string noticeKey=update.Version+"/"+update.Phase;
        if(deferredUpdate==noticeKey)return;
        // Original #dtUpd is a lower-left notification, not a full-width panel.
        float width=Math.Min(360,size.X-32),height=update.Phase=="manual"?206:updateError is not null?210:164;
        ClientPageWindow("Game update",16,size.Y-height-16,width,height);
        var at=ImGui.GetWindowPos();ImGui.GetWindowDrawList().AddRectFilled(at,at+new Brutal.Numerics.float2(3,height),NativeUiTheme.Gold);
        ClientPageTitle("UPDATE "+(update.Version??""),16);ImGui.Spacing();
        switch(update.Phase)
        {
            case "downloading":
                UiText.Wrapped($"Downloading... {update.Received/1048576d:F1} / {update.Total/1048576d:F1} MB");
                ImGui.ProgressBar(update.Total>0?(float)Math.Clamp(update.Received/(double)update.Total,0,1):0,new(-1,6),""u8);break;
            case "verifying": UiText.Wrapped("Preparing update..."u8);ImGui.ProgressBar(1,new(-1,6),""u8);break;
            case "ready":
                UiText.Wrapped("Update downloaded. Restart to apply."u8);ImGui.Spacing();
                ImGui.BeginDisabled(FileOperationsPending||closeAfterFileOperations||ClientNavigationBlocked||showMusicLibrary||confirmResetSave);
                if(HomeButton("restartUpdate","Restart to update",190,36))
                {
                    try {RestartForUpdateRequested=UpdateClient.RequestRestart();if(RestartForUpdateRequested && PostgameHomeVisible)LeaveMatch();}
                    catch(Exception error) when(error is IOException or UnauthorizedAccessException) {updateError="Could not request restart. Please close and reopen the game.";}
                }
                ImGui.EndDisabled();
                if(ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)&&FileOperationsPending)ResultTooltip("Finish the current file operation before restarting.");
                ImGui.SameLine();if(HomeButton("deferUpdate","Later",92,36))deferredUpdate=noticeKey;
                break;
            case "manual":
                UiText.Wrapped("This version requires a new full download."u8);ImGui.Spacing();
                if(HomeButton("updateReleasePage","Open release page",190,36) && Uri.TryCreate(update.Page,UriKind.Absolute,out var page) && page.Scheme=="https")
                {
                    try {System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(page.AbsoluteUri) {UseShellExecute=true});}
                    catch(Exception error) when(error is System.ComponentModel.Win32Exception or InvalidOperationException) {updateError="Could not open the release page.";}
                }
                ImGui.SameLine();if(HomeButton("deferUpdate","Later",92,36))deferredUpdate=noticeKey;
                break;
        }
        if(updateError is not null)UiText.Wrapped(updateError);
        ImGui.End();
    }
}


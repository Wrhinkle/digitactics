using Brutal.ImGuiApi;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    public NativeUpdateClient? UpdateClient {get;set;}
    public bool RestartForUpdateRequested {get;private set;}
    private string? updateError;
    private void UpdatePanel()
    {
        if(!ClientHomeVisible || networkRoom is not null || steam?.Room is not null || UpdateClient is null)return;
        UpdateClient.Refresh();var update=UpdateClient.Status;
        if(update is null || update.Phase is "idle" or "current" or "unavailable")return;
        Window("Game update",16,size.Y-100,size.X-32,84);
        switch(update.Phase)
        {
            case "checking": ImGui.Text("Checking for updates..."u8);break;
            case "downloading": ImGui.Text($"Downloading update {update.Version??""}: {update.Received/1048576d:F1} / {update.Total/1048576d:F1} MB");break;
            case "verifying": ImGui.Text($"Preparing update {update.Version??""}...");break;
            case "ready":
                ImGui.Text($"Update {update.Version??""} is ready. Restart when you're ready to play the new version.");
                ImGui.BeginDisabled(FileOperationsPending||closeAfterFileOperations);
                if(Button("restartUpdate","Restart to update"))
                {
                    try {RestartForUpdateRequested=UpdateClient.RequestRestart();if(RestartForUpdateRequested && PostgameHomeVisible)LeaveMatch();}
                    catch(Exception error) when(error is IOException or UnauthorizedAccessException) {updateError="Could not request restart. Please close and reopen the game.";}
                }
                ImGui.EndDisabled();
                if(ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)&&FileOperationsPending)ResultTooltip("Finish the current file operation before restarting.");
                break;
            case "manual":
                ImGui.Text($"Update {update.Version??""} requires a new full download.");
                if(Button("updateReleasePage","Open release page") && Uri.TryCreate(update.Page,UriKind.Absolute,out var page) && page.Scheme=="https")
                {
                    try {System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(page.AbsoluteUri) {UseShellExecute=true});}
                    catch(Exception error) when(error is System.ComponentModel.Win32Exception or InvalidOperationException) {updateError="Could not open the release page.";}
                }
                break;
        }
        if(updateError is not null)ImGui.Text(updateError);
        ImGui.End();
    }
}


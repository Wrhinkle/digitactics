using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool localPaused, showGameMenu, confirmLeave;
    internal bool IsPaused => localPaused && networkRoom is null && Session is not null;
    private void OpenGameMenu()
    {
        CancelArenaDrag(); cameraDrag = null; showGameMenu = true; confirmLeave = false;
    }
    private void PauseLocal()
    {
        if (networkRoom is not null || Session is null || Session.State.Over) return;
        OpenGameMenu(); localPaused = true;
    }
    private void LeaveMatch()
    {
        ClearEliminationCinematics();
        if (networkRoom is not null) LeaveSteamRoom();
        CancelArenaDrag(); cameraDrag = null;
        Session = null; history = null; audio.Music(null);
        battle?.Dispose(); battle = null; watching = null; combatAudio = null;
        planning.Dispose(); planning = new(arenaEnvironment);
        selectedUnit = selectedItem = null; resultCountdown = null;
        localPaused = showGameMenu = confirmLeave = showAudio = showPlayers = showEmotes = showSoundboard = false;
    }
    private void PausePanel()
    {
        Window(confirmLeave ? "Leave this match?" : IsPaused ? "Game paused" : "Game menu", size.X / 2 - 210, size.Y / 2 - 115, 420, 230);
        UiText.Wrapped(confirmLeave
            ? networkRoom is null ? "This ends your current match. You cannot resume it, and no unfinished match result will be saved." : "You will leave the lobby. Remaining players can continue the match."
            : IsPaused ? "Your local match is paused. Resume whenever you are ready." : "The match continues while this menu is open.");
        if (Button("resume", IsPaused ? "Resume game" : confirmLeave ? "Keep playing" : "Return to game", 220, 36) || !EliminationCinematicActive && ImGui.IsKeyPressed(ImGuiKey.Escape, false))
            localPaused = showGameMenu = confirmLeave = false;
        else if (confirmLeave)
        {
            if (Button("confirmLeave", "Leave match", 220, 36)) LeaveMatch();
        }
        else
        {
            if (networkRoom is null && !IsPaused && !Session!.State.Over && Button("menuPause", "Pause game", 220, 36)) PauseLocal();
            if (Button("requestLeave", "Leave match...", 220, 36)) confirmLeave = true;
        }
        ImGui.End();
    }
}

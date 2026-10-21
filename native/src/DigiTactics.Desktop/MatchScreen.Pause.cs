using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool localPaused, showGameMenu, confirmLeave, drawingPauseBackdrop;
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
        castFeed=null;
        planning.Dispose(); planning = new(arenaEnvironment);
        selectedUnit = selectedItem = null; resultCountdown = null;
        localPaused = showGameMenu = confirmLeave = showAudio = showPlayers = showEmotes = showSoundboard = showMatchInventory = false;
    }
    private void PausePanel()
    {
        // Keep the original match chrome visible underneath the dialog, while
        // exposing no actionable HUD targets to either pointer or test drivers.
        if(Session is not null)
        {
            drawingPauseBackdrop=true;ImGui.BeginDisabled();
            try {if(!Buttons.ContainsKey("gameMenu"))Header();Roster();Sidebar();}
            finally {ImGui.EndDisabled();drawingPauseBackdrop=false;Buttons.Clear();}
        }
        // Original #gameMenu: a centered 420px dialog, display heading,
        // full-screen dimmer and wrapping 44px actions (style.css).
        string message=confirmLeave
            ? networkRoom is null ? "This ends your current match. You cannot resume it, and no unfinished match result will be saved." : "You will leave the lobby. Remaining players can continue the match."
            : IsPaused ? "Your local match is paused. Resume whenever you are ready." : "The match continues while this menu is open.";
        bool canPause=!confirmLeave&&networkRoom is null&&!IsPaused&&!Session!.State.Over;
        float width=Math.Min(420,size.X-32),bodyHeight=ImGui.CalcTextSize(message,false,width-48).Y;
        float actionY=24+26+12+bodyHeight+22;
        float height=Math.Max(204,actionY+44+24)+(canPause?54:0);
        var position=new float2((size.X-width)/2,(size.Y-height)/2);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(24,24));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding,8);
        ImGui.PushStyleColor(ImGuiCol.WindowBg,new ImColor8(7,19,32));
        ImGui.PushStyleColor(ImGuiCol.Border,NativeUiTheme.Gold);
        ImGui.SetNextWindowFocus();
        Window("Game menu",position.X,position.Y,width,height,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoBackground);
        ImGui.PopStyleColor(2);ImGui.PopStyleVar(2);
        // One draw list keeps the dimmer above all HUD windows and below the
        // dialog without switching focus away from a held dialog button.
        var draw=ImGui.GetWindowDrawList();
        draw.PushClipRect(new(0,0),new(size.X,size.Y),false);
        draw.AddRectFilled(new(0,0),new(size.X,size.Y),new ImColor8(1,10,19,201));
        draw.AddRectFilled(position,position+new float2(width,height),new ImColor8(7,19,32),8);
        draw.AddRect(position,position+new float2(width,height),NativeUiTheme.Gold,8);
        draw.PopClipRect();
        ClientPageTitle(confirmLeave ? "Leave this match?" : IsPaused ? "Game paused" : "Game menu",26,NativeUiTheme.GoldBright);
        ImGui.SetCursorPos(new(24,62));UiText.Wrapped(message);
        ImGui.SetCursorPos(new(24,actionY));
        float actionWidth=(ImGui.GetContentRegionAvail().X-10)/2;
        if (Button("resume", IsPaused ? "Resume game" : confirmLeave ? "Keep playing" : "Return to game", actionWidth, 44) || !EliminationCinematicActive && ImGui.IsKeyPressed(ImGuiKey.Escape, false))
            localPaused = showGameMenu = confirmLeave = false;
        else if (confirmLeave)
        {
            ImGui.SameLine(0,10);
            ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(187,98,107));ImGui.PushStyleColor(ImGuiCol.Text,new ImColor8(255,190,197));
            if (Button("confirmLeave", "Leave match", actionWidth, 44)) LeaveMatch();
            ImGui.PopStyleColor(2);
        }
        else
        {
            ImGui.SameLine(0,10);
            if (canPause && Button("menuPause", "Pause game", actionWidth, 44)) PauseLocal();
            ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(187,98,107));ImGui.PushStyleColor(ImGuiCol.Text,new ImColor8(255,190,197));
            if (Button("requestLeave", "Leave match...", canPause?ImGui.GetContentRegionAvail().X:actionWidth, 44)) confirmLeave = true;
            ImGui.PopStyleColor(2);
        }
        ImGui.End();
    }
}

using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed class SteamLobbyUiSmoke
{
    private readonly string[] commands = ["multiplayer", "steamConnect", "planningSeconds/45", "steamCreate", "steamFree/0", "steamSeat/1", "steamStart", "leaveOnline", "confirmLeave", "steamBack"];
    private int index, cooldown = 5;
    private bool down, roomVerified, matchVerified;
    public string Pending => index < commands.Length ? commands[index] : "cleanup";
    public void Input(MatchScreen screen)
    {
        screen.UsePrivateLobbyForSmoke();
        if (index >= commands.Length || cooldown-- > 0) return;
        if (down) { ImGui.GetIO().AddMouseButtonEvent(0, false); down = false; index++; cooldown = 5; return; }
        if (!screen.Buttons.TryGetValue(commands[index], out var point)) return;
        ImGui.GetIO().AddMousePosEvent(point.X, point.Y); ImGui.GetIO().AddMouseButtonEvent(0, true); down = true;
    }
    public bool Verify(MatchScreen screen, int2 size, string? capture)
    {
        if (index == 4 && screen.InSteamRoom && !roomVerified)
        {
            roomVerified = true;
            if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "lobby.bmp"), size.X, size.Y);
        }
        if (index == 7 && screen.Session is not null && screen.NetworkStateHash is not null)
        {
            if (!screen.Session.State.Players[1].IsHuman || screen.Session.State.Players[0].IsHuman) throw new InvalidOperationException("Steam lobby seat switch did not reach the match.");
            if (screen.NetworkPlanningDuration != 45) throw new InvalidOperationException("Lobby planning duration did not reach match initialization.");
            matchVerified = true;
            if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "match.bmp"), size.X, size.Y);
        }
        if (index < commands.Length) return false;
        if (!roomVerified || !matchVerified || screen.Session is not null || screen.InSteamRoom) throw new InvalidOperationException("Steam lobby UI did not enter and cleanly leave a native match.");
        if (capture is not null) FrameCapture.SaveBitmap(capture, size.X, size.Y);
        Console.WriteLine("Steam lobby UI smoke passed: connect, private create, free/take seat, native match start, leave and return to menu."); return true;
    }
}

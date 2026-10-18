using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private (ImGuiMouseButton Button, float2 Start, float2 Last, bool Moved)? cameraDrag;
    internal ArenaCamera CameraState => arenaEnvironment.Camera;
    private void CameraShortcuts()
    {
        var camera = arenaEnvironment.Camera;
        int columns = battle?.Combat.Columns ?? 7;
        if (ImGui.IsKeyPressed(ImGuiKey.R)) camera.Reset();
        if (ImGui.IsKeyPressed(ImGuiKey.Equal) || ImGui.IsKeyPressed(ImGuiKey.KeypadAdd) || ImGui.IsKeyPressed(ImGuiKey.KeypadEqual))
            camera.ZoomAt(Width / 2f, Height / 2f, .8, Width, Height, columns);
        if (ImGui.IsKeyPressed(ImGuiKey.Minus) || ImGui.IsKeyPressed(ImGuiKey.KeypadSubtract))
            camera.ZoomAt(Width / 2f, Height / 2f, 1.25, Width, Height, columns);
    }
    private bool CameraNavigation(bool inside, UnitSpot? spot, int shelfHover)
    {
        var mouse = ImGui.GetMousePos();
        int columns = battle?.Combat.Columns ?? 7;
        var camera = arenaEnvironment.Camera;
        if (inside && ImGui.GetIO().MouseWheel != 0)
            camera.ZoomAt(mouse.X - Left, mouse.Y - Top, Math.Exp(-ImGui.GetIO().MouseWheel * .15), Width, Height, columns);
        if (inside && cameraDrag is null && itemDrag is null && arenaDrag is null)
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right)) cameraDrag = (ImGuiMouseButton.Right, mouse, mouse, false);
            else if (ImGui.IsMouseClicked(ImGuiMouseButton.Middle)) cameraDrag = (ImGuiMouseButton.Middle, mouse, mouse, false);
            else if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && shelfHover < 0 && selectedItem is null && selectedUnit is null &&
                (spot is null || Session!.State.Players[viewed].Roster.At(spot.Value) is null))
                cameraDrag = (ImGuiMouseButton.Left, mouse, mouse, false);
        }
        if (cameraDrag is not { } drag) return false;
        bool moved = drag.Moved || Math.Sqrt(Math.Pow(mouse.X - drag.Start.X, 2) + Math.Pow(mouse.Y - drag.Start.Y, 2)) >= 5;
        if (!ImGui.IsMouseDown(drag.Button))
        {
            if (!moved && drag.Button == ImGuiMouseButton.Right && !IsSpectating && arenaCompanion is not null)
            {
                var point = ArenaPicking.Ground(drag.Start.X - Left, drag.Start.Y - Top, Width, Height, columns, camera);
                if (point is { } p && arenaCompanion.Travel.Move(p.X, p.Z))
                    companionClick = (arenaCompanion.Travel.TargetX!.Value, arenaCompanion.Travel.TargetZ!.Value, Environment.TickCount64);
            }
            cameraDrag = null; return true;
        }
        if (moved)
        {
            if (drag.Button == ImGuiMouseButton.Right) camera.Orbit(mouse.X - drag.Last.X, mouse.Y - drag.Last.Y);
            else camera.Pan(drag.Last.X - Left, drag.Last.Y - Top, mouse.X - Left, mouse.Y - Top, Width, Height, columns);
        }
        cameraDrag = (drag.Button, drag.Start, moved ? mouse : drag.Last, moved);
        return true;
    }
}

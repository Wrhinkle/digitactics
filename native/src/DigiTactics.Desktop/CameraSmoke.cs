using Brutal.ImGuiApi;
using Brutal.Numerics;
using System.Numerics;

namespace DigiTactics.Desktop;

internal sealed class CameraSmoke
{
    private int step;
    private float2 point;
    private Vector3 target;
    public bool Input(MatchScreen screen)
    {
        var io = ImGui.GetIO(); var camera = screen.CameraState;
        switch (step++)
        {
            case 0:
                foreach (int columns in new[] { 7, 14 })
                {
                    var probe = new ArenaCamera();
                    var before = ArenaPicking.Ground(420, 310, 970, 565, columns, probe)!.Value;
                    probe.ZoomAt(420, 310, .8, 970, 565, columns);
                    var after = ArenaPicking.Ground(420, 310, 970, 565, columns, probe)!.Value;
                    if (Math.Abs(before.X - after.X) > .002 || Math.Abs(before.Z - after.Z) > .002)
                        throw new InvalidOperationException("Zoom failed to retain its ground anchor.");
                    probe.Orbit(10000, 10000); probe.ZoomAt(485, 282, .0001, 970, 565, columns);
                    if (probe.Pitch != 1.45 || probe.Zoom != .22) throw new InvalidOperationException("Camera limits failed.");
                }
                point = screen.Buttons["hex/5/6"]; io.AddMousePosEvent(point.X, point.Y); io.AddMouseButtonEvent((int)ImGuiMouseButton.Right, true); break;
            case 1: io.AddMousePosEvent(point.X + 24, point.Y + 12); break;
            case 2: io.AddMouseButtonEvent((int)ImGuiMouseButton.Right, false); break;
            case 3:
                if (Math.Abs(camera.Yaw + .192) > 1e-6 || Math.Abs(camera.Pitch - Math.Atan2(16.5, 15.5) - .072) > 1e-6)
                    throw new InvalidOperationException("Arena orbit input does not follow original camera sensitivity.");
                target = camera.Target; io.AddMouseButtonEvent((int)ImGuiMouseButton.Middle, true); break;
            case 4: io.AddMousePosEvent(point.X + 38, point.Y + 20); break;
            case 5: io.AddMouseButtonEvent((int)ImGuiMouseButton.Middle, false); break;
            case 6:
                if (Vector3.Distance(camera.Target, target) < .05f) throw new InvalidOperationException("Arena middle-drag did not pan.");
                io.AddMouseWheelEvent(0, 1); break;
            case 7:
                if (Math.Abs(camera.Zoom - Math.Exp(-.15)) > 1e-7) throw new InvalidOperationException("Arena wheel did not zoom.");
                io.AddKeyEvent(ImGuiKey.R, true); break;
            case 8: io.AddKeyEvent(ImGuiKey.R, false); break;
            case 9:
                if (camera.Zoom != 1 || camera.Yaw != 0 || camera.Target != new Vector3(0, 0, 1.8f) || camera.Pitch != Math.Atan2(16.5, 15.5))
                    throw new InvalidOperationException("Arena reset did not restore the original view.");
                return true;
        }
        return false;
    }
}

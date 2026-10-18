using Brutal.GlfwApi;
using Brutal.ImGuiApi;
using Brutal.ImGuiApi.Backends;
using Brutal.ImGuiApi.Internal;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

/// <summary>One logical coordinate system for layout, queued input, fonts, and physical rendering.</summary>
internal sealed class NativeUiPlatform : IDisposable
{
    private readonly GlfwWindow window;
    private readonly GlfwBackend backend;
    private readonly NativeUiInputMapper input = new();
    private readonly float? forcedScale;
    private nint boundsMonitor;
    private NativeUiMonitorArea boundsArea;
    internal int2 LogicalSize => input.Metrics.LogicalSize;
    internal float Scale => input.Metrics.Scale;

    internal NativeUiPlatform(GlfwWindow window, float? forcedScale = null)
    {
        this.window = window;
        this.forcedScale = forcedScale;
        if (ImGui.GetIO().ConfigFlags.HasFlag(ImGuiConfigFlags.ViewportsEnable))
            throw new InvalidOperationException("Native UI scaling uses the game's single main viewport.");
        backend = new GlfwBackend(window, GlfwBackend.ClientApi.OpenGL);
        // Logical layout already includes DPI through DisplayFramebufferScale.
        // Automatic font-size scaling would apply the monitor scale a second time.
        ImGui.GetIO().ConfigDpiScaleFonts = false;
        ImGui.GetIO().ConfigDpiScaleViewports = false;
    }

    internal void NewFrame()
    {
        if (forcedScale is null) RefreshWindowLimits();
        var io = ImGui.GetIO();
        if (io.WantSetMousePos)
        {
            // Honor keyboard-navigation cursor warps in GLFW's physical coordinates.
            window.CursorPos = new(io.MousePos.X * Scale, io.MousePos.Y * Scale);
            io.WantSetMousePos = false;
        }
        backend.NewFrame();
        float requested = forcedScale ?? (OperatingSystem.IsWindows() ? window.ContentScale.X : 1);
        input.Apply(NativeUiMetrics.Calculate(window.Size, window.FramebufferSize, requested));
    }

    private void RefreshWindowLimits()
    {
        var monitors = Glfw.GetMonitors();
        if (monitors.IsEmpty) return;
        Span<NativeUiMonitorArea> areas = monitors.Length <= 16 ? stackalloc NativeUiMonitorArea[monitors.Length] : new NativeUiMonitorArea[monitors.Length];
        for (int i = 0; i < monitors.Length; i++)
        {
            monitors[i].GetMonitorWorkArea(out var origin, out var size);
            areas[i] = new(origin, size, OperatingSystem.IsWindows() ? monitors[i].ContentScale.X : 1);
        }
        int chosen = NativeUiMetrics.MonitorIndex(window.Position, window.Size, areas);
        var area = areas[chosen];
        if (area.Size.X <= 0 || area.Size.Y <= 0 || (boundsMonitor == monitors[chosen].NativeHandle && boundsArea == area)) return;
        boundsMonitor = monitors[chosen].NativeHandle;
        boundsArea = area;
        Glfw.PInvoke.GetWindowFrameSize(window, out int left, out int top, out int right, out int bottom);
        // Changing to a smaller/lower-DPI monitor must release the previous large
        // minimum. A move in the other direction must not resize the user's window.
        window.SetSizeLimits(NativeUiMetrics.MinimumWindowSize(area.Size, area.Scale, new(left, top, right, bottom), window.Size), new(-1, -1));
    }

    // Call after ImGui.NewFrame (or Render), before the next GLFW event poll.
    // Synthetic test input added after NewFrame() is already in logical units.
    internal void CompleteFrame() => input.CompleteFrame();
    public void Dispose() => backend.Dispose();
}

internal readonly record struct NativeUiMonitorArea(int2 Origin, int2 Size, float Scale);

internal readonly record struct NativeUiMetrics(float Scale, int2 LogicalSize, float2 FramebufferScale)
{
    internal static readonly int2 MinimumLogicalSize = new(1100, 760);

    internal static NativeUiMetrics Calculate(int2 physical, int2 framebuffer, float requestedScale)
    {
        float requested = float.IsFinite(requestedScale) ? Math.Clamp(requestedScale, 1, 4) : 1;
        // Keep the supported minimum layout on smaller work areas. The startup
        // window is enlarged where space permits; constrained windows use the
        // largest scale that fits, never shrinking the 17px baseline below 100%.
        float fit = Math.Max(1, Math.Min(physical.X / (float)MinimumLogicalSize.X, physical.Y / (float)MinimumLogicalSize.Y));
        float scale = Math.Min(requested, fit);
        var logical = new int2(Math.Max(1, (int)MathF.Round(physical.X / scale)), Math.Max(1, (int)MathF.Round(physical.Y / scale)));
        return new(scale, logical, new(framebuffer.X / (float)logical.X, framebuffer.Y / (float)logical.Y));
    }

    internal static int2 WindowSize(int2 logical, int2 workArea, float requestedScale, int4 frame = default)
    {
        float scale = float.IsFinite(requestedScale) ? Math.Clamp(requestedScale, 1, 4) : 1;
        // GLFW reports each real decoration edge in the same coordinates as the
        // monitor work area. Do not guess fixed title-bar/taskbar allowances.
        return new(Math.Min(Math.Max(1, workArea.X - Math.Max(0, frame.X) - Math.Max(0, frame.Z)), (int)MathF.Round(logical.X * scale)),
            Math.Min(Math.Max(1, workArea.Y - Math.Max(0, frame.Y) - Math.Max(0, frame.W)), (int)MathF.Round(logical.Y * scale)));
    }

    internal static int2 WindowPosition(int2 clientSize, int2 workOrigin, int2 workArea, int4 frame)
        => workOrigin + new int2(Math.Max(0, (workArea.X-clientSize.X-Math.Max(0,frame.X)-Math.Max(0,frame.Z))/2)+Math.Max(0,frame.X),
            Math.Max(0, (workArea.Y-clientSize.Y-Math.Max(0,frame.Y)-Math.Max(0,frame.W))/2)+Math.Max(0,frame.Y));

    internal static int2 MinimumWindowSize(int2 workArea, float scale, int4 frame, int2 currentSize)
    {
        var minimum = WindowSize(MinimumLogicalSize, workArea, scale, frame);
        return new(Math.Min(minimum.X, Math.Max(1, currentSize.X)), Math.Min(minimum.Y, Math.Max(1, currentSize.Y)));
    }

    internal static int MonitorIndex(int2 position, int2 size, ReadOnlySpan<NativeUiMonitorArea> monitors)
    {
        int selected = 0; // GLFW guarantees that the primary monitor is first.
        long largest = 0;
        for (int i = 0; i < monitors.Length; i++)
        {
            var area = monitors[i];
            long width = Math.Max(0, Math.Min((long)position.X+size.X, (long)area.Origin.X+area.Size.X)-Math.Max(position.X,area.Origin.X));
            long height = Math.Max(0, Math.Min((long)position.Y+size.Y, (long)area.Origin.Y+area.Size.Y)-Math.Max(position.Y,area.Origin.Y));
            long overlap = width*height;
            if (overlap > largest) { selected = i; largest = overlap; }
        }
        return selected;
    }

    internal float2 ToLogical(float2 physical) => physical / Scale;
    internal float2 ToPhysical(float2 logical) => logical * Scale;
}

/// <summary>
/// Adapts the typed queue exposed by the pinned BRUTAL ImGui binding before
/// ImGui consumes input. Event ordering is preserved, including trickled clicks;
/// GLFW's cursor cache and callbacks retain their original physical semantics.
/// </summary>
internal sealed class NativeUiInputMapper
{
    private readonly HashSet<uint> logicalEvents = [];
    internal NativeUiMetrics Metrics { get; private set; } = new(1, new(1440, 900), new(1, 1));

    internal void Apply(NativeUiMetrics next)
    {
        var io = ImGui.GetIO();
        float ratio = Metrics.Scale / next.Scale;
        if (ratio != 1)
        {
            io.MousePos = ScaleValid(io.MousePos, ratio);
            io.MousePosPrev = ScaleValid(io.MousePosPrev, ratio);
            for (int i = 0; i < 5; i++) io.MouseClickedPos[i] = ScaleValid(io.MouseClickedPos[i], ratio);
            for (int i = 0; i < io.MouseDragMaxDistanceSqr.Length; i++) io.MouseDragMaxDistanceSqr[i] *= ratio * ratio;
        }
        foreach (ref var input in ImGui.GetCurrentContext().InputEventsQueue.Span)
        {
            if (input.Type != ImGuiInputEventType.MousePos) continue;
            float factor = logicalEvents.Contains(input.EventId) ? ratio : 1 / next.Scale;
            var point = ScaleValid(new(input.MousePos.PosX, input.MousePos.PosY), factor);
            input.MousePos.PosX = point.X;
            input.MousePos.PosY = point.Y;
        }
        Metrics = next;
        io.DisplaySize = float2.Unpack(next.LogicalSize);
        io.DisplayFramebufferScale = next.FramebufferScale;
        ImGui.GetMainViewport().FramebufferScale = next.FramebufferScale;
        CompleteFrame();
    }

    internal void CompleteFrame()
    {
        logicalEvents.Clear();
        foreach (var input in ImGui.GetCurrentContext().InputEventsQueue.Span)
            if (input.Type == ImGuiInputEventType.MousePos) logicalEvents.Add(input.EventId);
    }

    private static float2 ScaleValid(float2 point, float factor)
        => point.X <= -float.MaxValue / 2 || point.Y <= -float.MaxValue / 2 ? point : point * factor;
}

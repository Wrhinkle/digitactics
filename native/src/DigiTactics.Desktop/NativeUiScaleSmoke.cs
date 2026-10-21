using Brutal.ImGuiApi;
using Brutal.ImGuiApi.Internal;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

/// <summary>Exercises actual ImGui buttons and queued input without a graphics context.</summary>
internal static class NativeUiScaleSmoke
{
    internal static void Verify()
    {
        int checks = 0;
        void Check(bool value, string message)
        {
            if (!value) throw new InvalidOperationException("UI DPI check failed: " + message);
            checks++;
        }
        // Startup bounds must use the actual native decorations, not fixed pixel
        // guesses that become wrong with Windows DPI and taskbar placement.
        var startupFrame = new int4(12, 46, 12, 12);
        var fullSize = NativeUiMetrics.WindowSize(new(1440, 900), new(3840, 2080), 2, startupFrame);
        Check(fullSize == new int2(2880, 1800), "200% startup allocates the full physical client area");
        var limitedSize = NativeUiMetrics.WindowSize(new(1440, 900), new(1920, 1040), 2, startupFrame);
        Check(limitedSize == new int2(1896, 982), "constrained startup accounts for exact measured frame size");
        var monitorWork = new[] { new NativeUiMonitorArea(new(0,0),new(3840,2080),2), new NativeUiMonitorArea(new(3840,0),new(1920,1040),1) };
        Check(NativeUiMetrics.MonitorIndex(new(200,100),new(2880,1800),monitorWork)==0,"window starts on 4K monitor");
        Check(NativeUiMetrics.MonitorIndex(new(4000,100),new(1800,950),monitorWork)==1,"window moves to1080p monitor by greatest overlap");
        Check(NativeUiMetrics.MonitorIndex(new(-9000,100),new(1440,900),monitorWork)==0,"off-screen window chooses primary fallback");
        var lowered=NativeUiMetrics.MinimumWindowSize(new(1920,1040),1,startupFrame,new(1800,950));
        Check(lowered==new int2(1100,760),"moving from200% to100% releases old oversized minimum");
        var raised=NativeUiMetrics.MinimumWindowSize(new(3840,2080),2,startupFrame,new(1440,900));
        Check(raised==new int2(1440,900),"higher DPI minimum never forces an unsolicited resize");
        foreach (float scale in new[] { 1.25f, 1.5f, 2f })
        {
            var context = ImGui.CreateContext();
            try
            {
                NativeUiTheme.Initialize();
                var io = ImGui.GetIO();
                io.IniFilename = default;
                io.DeltaTime = 1 / 60f;
                io.BackendFlags |= ImGuiBackendFlags.RendererHasTextures;
                io.ConfigInputTrickleEventQueue = true;
                var physical = new int2((int)(1440 * scale), (int)(900 * scale));
                var metrics = NativeUiMetrics.Calculate(physical, physical, scale);
                var input = new NativeUiInputMapper();
                float2 center = default;
                bool hovered = false;
                bool Frame()
                {
                    ImGui.NewFrame();
                    ImGui.SetNextWindowPos(new(100, 100));
                    ImGui.SetNextWindowSize(new(420, 300));
                    ImGui.Begin("DPI fixture"u8, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoMove);
                    bool clicked = ImGui.Button("Known logical control"u8, new(240, 42));
                    center = (ImGui.GetItemRectMin() + ImGui.GetItemRectMax()) * .5f;
                    hovered = ImGui.IsItemHovered();
                    Check(Math.Abs(ImGui.GetFontSize() - NativeUiTheme.BodySize) < .001f, "body remains 17 logical pixels");
                    ImGui.PushFont(NativeUiTheme.DisplayFont, 24);
                    ImGui.Text("Chakra Petch / readable UI"u8);
                    ImGui.PopFont();
                    ImGui.PushFont(NativeUiTheme.BodyBoldFont,14);
                    ImGui.Text("Inter Bold / 30% / HP 620"u8);
                    ImGui.PopFont();
                    ImGui.End();
                    ImGui.Render();
                    input.CompleteFrame();
                    return clicked;
                }
                input.Apply(metrics); Frame();
                input.Apply(metrics); Frame();
                Check(metrics.LogicalSize == new int2(1440, 900), $"{scale}: logical layout size");
                Check(Near(io.DisplayFramebufferScale, new(scale, scale)), $"{scale}: physical framebuffer mapping");
                Check(NativeUiTheme.BodyFont.GetDebugName().ToString().Contains("Inter", StringComparison.Ordinal), "bundled body typeface is loaded");
                Check(NativeUiTheme.DisplayFont.GetDebugName().ToString().Contains("Chakra", StringComparison.Ordinal), "bundled display typeface is loaded");
                var physicalCenter = metrics.ToPhysical(center);
                io.AddMousePosEvent(physicalCenter.X, physicalCenter.Y);
                io.AddMouseButtonEvent(0, true);
                input.Apply(metrics); Frame();
                // ImGui quantizes queued mouse positions to integral pixels.
                Check(hovered && Math.Abs(io.MousePos.X-center.X)<=1 && Math.Abs(io.MousePos.Y-center.Y)<=1,
                    $"{scale}: physical pointer hovers actual logical button (hovered={hovered}, mouse={io.MousePos}, center={center})");
                io.AddMouseButtonEvent(0, false);
                input.Apply(metrics);
                Check(Frame(), $"{scale}: physical click activates actual logical button");
                var draw = ImGui.GetDrawData();
                Check(Near(draw.DisplaySize * draw.FramebufferScale, float2.Unpack(physical)), $"{scale}: render target covers exact physical framebuffer");
                Check(NativeUiTheme.BodyFont.CurrentRasterizerDensity >= scale - .01f, $"{scale}: font rasterizes at physical density");
                Check(NativeUiTheme.BodyBoldFont.CurrentRasterizerDensity >= scale - .01f, $"{scale}: bold font rasterizes at physical density");

                io.AddMousePosEvent(400 * scale, 300 * scale);
                io.AddMouseButtonEvent(0, true);
                io.AddMousePosEvent(600 * scale, 400 * scale);
                io.AddMouseButtonEvent(0, false);
                input.Apply(metrics); Frame();
                var pending = context.InputEventsQueue.Span.ToArray();
                Check(pending.Length > 0, "fixture exercises ImGui's deferred input queue");
                input.Apply(metrics);
                var repeated = context.InputEventsQueue.Span.ToArray();
                Check(pending.Zip(repeated).All(pair => pair.First.Type != ImGuiInputEventType.MousePos ||
                    pair.First.MousePos.PosX == pair.Second.MousePos.PosX && pair.First.MousePos.PosY == pair.Second.MousePos.PosY), "deferred input is not divided twice");
                Frame();
                input.Apply(metrics);
                io.AddMousePosEvent(250, 250); // Existing smoke drivers supply logical coordinates after platform.NewFrame.
                input.CompleteFrame();
                input.Apply(metrics);
                var logicalEvent = context.InputEventsQueue.Span.ToArray().Last(value => value.Type == ImGuiInputEventType.MousePos);
                Check(logicalEvent.MousePos.PosX == 250 && logicalEvent.MousePos.PosY == 250, "logical injected controls are preserved");
                var next = NativeUiMetrics.Calculate(new(2880, 1800), new(2880, 1800), 2);
                input.Apply(next);
                var moved = context.InputEventsQueue.Span.ToArray().Last(value => value.Type == ImGuiInputEventType.MousePos);
                Check(Math.Abs(moved.MousePos.PosX - 250 * scale / 2) < .001f, "monitor change remaps pending coordinates once");
                Check(NativeUiMetrics.Calculate(new(1100, 760), new(1100, 760), scale).LogicalSize == new int2(1100, 760), "constrained window preserves minimum layout");
                foreach (var work in new[] { new int2(1366, 728), new int2(1920, 1040), new int2(2560, 1400), new int2(3840, 2080) })
                foreach (var desired in new[] { NativeUiMetrics.MinimumLogicalSize, new int2(1440, 900) })
                {
                    var bounds = NativeUiMetrics.WindowSize(desired, work, scale, startupFrame);
                    var minimum = NativeUiMetrics.WindowSize(NativeUiMetrics.MinimumLogicalSize, work, scale, startupFrame);
                    var position = NativeUiMetrics.WindowPosition(bounds, new(-1920, 40), work, startupFrame);
                    Check(bounds.X+startupFrame.X+startupFrame.Z<=work.X && bounds.Y+startupFrame.Y+startupFrame.W<=work.Y,
                        "startup client and decorations fit monitor work area");
                    Check(minimum.X<=bounds.X && minimum.Y<=bounds.Y, "minimum size cannot enlarge startup outside the work area");
                    Check(position.X-startupFrame.X>=-1920 && position.Y-startupFrame.Y>=40 &&
                        position.X+bounds.X+startupFrame.Z<=-1920+work.X && position.Y+bounds.Y+startupFrame.W<=40+work.Y,
                        "centered window fits monitors with negative coordinates and top taskbar");
                    var mapping=NativeUiMetrics.Calculate(bounds,bounds,scale);
                    var known=new float2(mapping.LogicalSize.X*.5f,mapping.LogicalSize.Y*.5f);
                    Check(Near(mapping.ToLogical(mapping.ToPhysical(known)),known), "bounded startup preserves pointer coordinates");
                    Check(mapping.Scale>=1 && mapping.Scale<=scale, "constrained startup never shrinks base text or exceeds monitor DPI");
                    bool fullFit=desired.X*scale+startupFrame.X+startupFrame.Z<=work.X && desired.Y*scale+startupFrame.Y+startupFrame.W<=work.Y;
                    if(fullFit)Check(mapping.LogicalSize==desired && mapping.Scale==scale,"sufficient work area gets full requested logical size and DPI");
                    Console.WriteLine($"DPI startup: work={work}, requested={scale:P0}, client={bounds}, logical={mapping.LogicalSize}, effective={mapping.Scale:P1}, minimum={minimum}");
                }
            }
            finally { ImGui.DestroyContext(context); }
        }
        Console.WriteLine($"UI DPI checks passed: {checks}; actual ImGui controls and font rasterization at 125%, 150%, 200% without a GPU.");
    }

    private static bool Near(float2 a, float2 b) => Math.Abs(a.X - b.X) < .01f && Math.Abs(a.Y - b.Y) < .01f;
}

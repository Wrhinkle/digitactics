using System.Text.Json.Nodes;
using Brutal.GlfwApi;
using Brutal.Numerics;
using Brutal.OpenGlApi;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal static class FarmRoomSmoke
{
    public static unsafe void Run(string? capture)
    {
        if (!Glfw.Init()) throw new InvalidOperationException("BRUTAL GLFW initialization failed.");
        try
        {
            Glfw.WindowHint(GlfwWindowHint.ContextVersionMajor, 4); Glfw.WindowHint(GlfwWindowHint.ContextVersionMinor, 5); Glfw.WindowHint(GlfwWindowHint.Visible, 0);
            using var window = Glfw.CreateWindow(new GlfwWindow.CreateInfo { Title = "Farm room verification", Size = new int2(1000, 850) });
            window.MakeContextCurrent();
            if (!Glad.Initialize(name => Glfw.PInvoke.GetProcAddress(name))) throw new InvalidOperationException("OpenGL initialization failed.");
            using var room = new FarmView();
            var farm = JsonNode.Parse(FarmSimulation.Advance(null, null, null, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).Save)!;
            foreach (var theme in FarmCare.Themes)
            {
                farm["theme"] = theme.Id; room.Sync(farm.ToJsonString(), null, null);
                room.React("tokomon", "happy"); for (int frame = 0; frame < 60; frame++) room.Update(.05);
                OpenGl.ClearColor(new float4(.025f, .035f, .06f, 1)); OpenGl.Clear(GlClearBit.Color);
                room.Draw(0, 0, 1000, 850);
                if (room.Theme != theme.Id || room.DrawCalls < 150 || room.Residents.Count != 3) throw new InvalidOperationException("Farm room rendering failed: " + theme.Id);
                if (room.SnowParticles != (theme.Id == "snowy" ? 260 : 0)) throw new InvalidOperationException("Farm snow theme lifecycle failed: " + theme.Id);
                ModelPreview.VerifyVisible(1000, 850);
                if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, theme.Id + ".bmp"), 1000, 850);
                farm["lights"] = false;
                foreach (var pet in farm["pets"]!.AsObject()) pet.Value!["flags"]!["asleep"] = true;
                room.Sync(farm.ToJsonString(), null, null);
                for (int frame = 0; frame < 240; frame++) room.Update(.05);
                if (room.SleepingResidents != 3 || !room.Night) throw new InvalidOperationException("Farm residents did not reach their beds at night: " + theme.Id);
                OpenGl.Clear(GlClearBit.Color); room.Draw(0, 0, 1000, 850);
                if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, theme.Id + "-sleep.bmp"), 1000, 850);
                farm["lights"] = true;
                foreach (var pet in farm["pets"]!.AsObject()) pet.Value!["flags"]!["asleep"] = false;
                room.Sync(farm.ToJsonString(), null, null); room.Update(.05);
                if (room.SleepingResidents != 0) throw new InvalidOperationException("Farm residents did not wake: " + theme.Id);
            }
            Console.WriteLine("Farm room GPU smoke passed: all six themes, textures, care stations, resident movement and sleep/wake transitions.");
        }
        finally { Glfw.Terminate(); }
    }
}

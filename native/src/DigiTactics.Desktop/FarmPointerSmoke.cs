using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Clicks rendered geometry and reloads the profile to check the saved owner/key.</summary>
internal sealed class FarmPointerSmoke : IDisposable
{
    private readonly string directory;
    private int stage, cooldown = 4;
    private bool down;
    private FileStream? saveLock;
    public string Pending => $"farm pointer stage {stage}";

    public FarmPointerSmoke(NativeProfileStore profile, string directory)
    {
        this.directory = directory;
        if (profile.Exists) throw new InvalidOperationException("Farm pointer smoke requires a fresh isolated profile directory.");
        var farm = JsonNode.Parse(FarmSimulation.Advance(null, null, null, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).Save)!;
        farm["pets"]!["tokomon"]!["hunger"] = 10;
        farm["pets"]!["tsunomon"]!["happy"] = 85;
        // Equal keys on different owners catch dispatch that accidentally uses the selected resident.
        farm["pets"]!["tokomon"]!["poop"] = new JsonArray(new JsonObject { ["k"] = 901, ["x"] = -2, ["z"] = 2, ["n"] = false });
        farm["pets"]!["tsunomon"]!["poop"] = new JsonArray(new JsonObject { ["k"] = 901, ["x"] = 2, ["z"] = 2, ["n"] = true });
        if (!profile.UpdateStorage(new Dictionary<string, string> { ["dt.farm.v1"] = farm.ToJsonString() }))
            throw new IOException(profile.Error);
    }

    public void Input(MatchScreen screen)
    {
        if (cooldown-- > 0 || stage >= 4) return;
        var io = ImGui.GetIO();
        if (down) { io.AddMouseButtonEvent(0, false); down = false; stage++; cooldown = 30; return; }
        if (stage == 2)
        {
            var reloaded = new NativeProfileStore(directory);
            var farm = JsonNode.Parse(reloaded.Storage["dt.farm.v1"])!;
            if (farm["pets"]!["tsunomon"]!["poop"]!.AsArray().Count != 1 || reloaded.Progress.Bits != 0
                || screen.FarmPilePoint("tsunomon", 901) is null)
                throw new InvalidOperationException("Failed cleanup changed the saved pile or reward.");
            saveLock!.Dispose(); saveLock = null;
            stage++; cooldown = 3; return;
        }
        if (stage == 0)
        {
            if (!screen.Buttons.TryGetValue("farm", out var button)) return;
            io.AddMousePosEvent(button.X, button.Y);
        }
        else
        {
            if (screen.FarmSelectedResident != "tokomon") throw new InvalidOperationException("Pointer smoke needs a different selected resident.");
            if (screen.FarmPilePoint("tsunomon", 901) is not { } point) throw new InvalidOperationException("Target pile was not rendered.");
            if (stage == 1) saveLock = new FileStream(Path.Combine(directory, "profile.json.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            io.AddMousePosEvent(point.X, point.Y);
        }
        io.AddMouseButtonEvent(0, true); down = true;
    }

    public bool Verify(MatchScreen screen, int2 size, string? capture)
    {
        if (stage != 4 || cooldown > 0) return false;
        var reloaded = new NativeProfileStore(directory);
        var farm = JsonNode.Parse(reloaded.Storage["dt.farm.v1"])!;
        if (reloaded.Error is not null || farm["pets"]!["tsunomon"]!["poop"]!.AsArray().Count != 0
            || farm["pets"]!["tokomon"]!["poop"]!.AsArray().Count != 1
            || reloaded.Progress.Bits != 1 || screen.FarmSelectedResident != "tokomon"
            || screen.FarmPilePoint("tsunomon", 901) is not null || screen.FarmPilePoint("tokomon", 901) is null)
            throw new InvalidOperationException("Farm scene click did not persist only the clicked owner's pile and reward.");
        if (capture is not null) FrameCapture.SaveBitmap(capture, size.X, size.Y);
        Console.WriteLine("Farm pointer smoke passed: geometry click, nonselected owner, duplicate key isolation, locked-save retry, saved reward and profile reload.");
        return true;
    }
    public void Dispose() => saveLock?.Dispose();
}

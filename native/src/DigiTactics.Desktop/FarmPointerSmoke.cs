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
    private readonly string[] residents;
    private readonly string bankResident;
    public string Pending => $"farm pointer stage {stage}";

    public FarmPointerSmoke(NativeProfileStore profile, string directory)
    {
        this.directory = directory;
        if (profile.Exists) throw new InvalidOperationException("Farm pointer smoke requires a fresh isolated profile directory.");
        residents = new[] { "tokomon", "tsunomon" }.Concat(CosmeticCollection.Items("legend").Select(item => item.Id).Where(id => id is not ("tokomon" or "tsunomon")).Take(10)).ToArray();
        var wallet = new CosmeticCollection(null);
        foreach (string resident in residents) wallet.Grant("legend", resident);
        bankResident = CosmeticCollection.Items("legend").First(item => !residents.Contains(item.Id)).Id;
        wallet.Grant("legend", bankResident);
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var farm = JsonNode.Parse(FarmSimulation.Advance(null, wallet.Json, null, now).Save)!;
        farm["cap"] = 12;
        farm["residents"] = new JsonArray(residents.Select(id => (JsonNode?)JsonValue.Create(id)).ToArray());
        farm = JsonNode.Parse(FarmSimulation.Advance(farm.ToJsonString(), wallet.Json, null, now).Save)!;
        farm["pets"]!["tokomon"]!["hunger"] = 10;
        farm["pets"]!["tsunomon"]!["happy"] = 85;
        // Equal keys on different owners catch dispatch that accidentally uses the selected resident.
        farm["pets"]!["tokomon"]!["poop"] = new JsonArray(new JsonObject { ["k"] = 901, ["x"] = -2, ["z"] = 2, ["n"] = false });
        farm["pets"]!["tsunomon"]!["poop"] = new JsonArray(new JsonObject { ["k"] = 901, ["x"] = 2, ["z"] = 2, ["n"] = true });
        if (!profile.UpdateStorage(new Dictionary<string, string> { ["dt.farm.v1"] = farm.ToJsonString(), ["dt.cosmetics.v1"] = wallet.Json }))
            throw new IOException(profile.Error);
    }

    public void Input(MatchScreen screen)
    {
        if (cooldown-- > 0 || stage == 6 || stage >= 10) return;
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
        if (stage >= 7)
        {
            string key = stage == 7 ? "farmRoom" : stage == 8 ? "farmSelect/" + residents[^1] : "farmSwap/" + bankResident;
            if (!screen.Buttons.TryGetValue(key, out var target)) throw new InvalidOperationException("Missing full-room bank control: " + key);
            if (stage >= 8)
            {
                var bounds = screen.FarmManagementBounds;
                if (target.Y < bounds.Min.Y + 20 || target.Y > bounds.Max.Y - 20)
                {
                    io.AddMousePosEvent(bounds.Max.X - 10, (bounds.Min.Y + bounds.Max.Y) / 2);
                    io.AddMouseWheelEvent(0, target.Y < bounds.Min.Y + 20 ? 2 : -2); cooldown = 3; return;
                }
                if (stage == 9 && (screen.FarmSelectedResident != residents[^1] || screen.Buttons.ContainsKey("farmIn/" + bankResident)))
                    throw new InvalidOperationException("A full farm must offer swapping the selected resident, without a Move in action.");
            }
            io.AddMousePosEvent(target.X, target.Y);
        }
        else if (stage == 0)
        {
            if (!screen.Buttons.TryGetValue("farm", out var button)) return;
            io.AddMousePosEvent(button.X, button.Y);
        }
        else if (stage >= 4)
        {
            if (stage == 4)
            {
                if (screen.FarmSelectedResident != "tokomon" || residents.Any(id => !screen.Buttons.ContainsKey("farmResident/" + id)))
                    throw new InvalidOperationException("The twelve-resident farm did not render every portrait control.");
                var first = screen.Buttons["farmResident/" + residents[0]];
                var last = screen.Buttons["farmResident/" + residents[^1]];
                // farmstyle.js .df-chips: 42px chips with 5px gaps flex-wrap inside the scene (8px insets).
                var scene = screen.FarmSceneBounds;
                bool overflow = residents.Length * 47 - 5 > scene.Max.X - scene.Min.X - 16;
                if (overflow != (last.Y > first.Y + 24)) throw new InvalidOperationException("Twelve farm portraits did not wrap exactly when the chip row overflows the scene.");
                if (residents.Any(id => screen.Buttons["farmResident/" + id].X + 21 > scene.Max.X - 8)) throw new InvalidOperationException("A farm portrait chip extends past the scene.");
                io.AddMousePosEvent(last.X, last.Y);
            }
            else
            {
                if (screen.FarmSelectedResident != residents[^1]) throw new InvalidOperationException("The wrapped final resident could not be selected by pointer.");
                var first = screen.Buttons["farmResident/" + residents[0]];
                io.AddMousePosEvent(first.X, first.Y);
            }
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
        if (stage is not (6 or 10) || cooldown > 0) return false;
        var reloaded = new NativeProfileStore(directory);
        var farm = JsonNode.Parse(reloaded.Storage["dt.farm.v1"])!;
        if (reloaded.Error is not null || farm["pets"]!["tsunomon"]!["poop"]!.AsArray().Count != 0
            || farm["pets"]!["tokomon"]!["poop"]!.AsArray().Count != 1
            || reloaded.Progress.Bits != 1 || screen.FarmSelectedResident != "tokomon"
            || screen.FarmPilePoint("tsunomon", 901) is not null || screen.FarmPilePoint("tokomon", 901) is null)
            throw new InvalidOperationException("Farm scene click did not persist only the clicked owner's pile and reward.");
        if (stage == 6)
        {
            if (capture is not null) FrameCapture.SaveBitmap(capture, size.X, size.Y);
            stage = 7; cooldown = 3; return false;
        }
        var savedResidents = farm["residents"]!.AsArray().Select(id => id!.GetValue<string>()).ToArray();
        if (savedResidents.Length != 12 || !savedResidents.Contains(bankResident) || savedResidents.Contains(residents[^1]))
            throw new InvalidOperationException("Full-room Digi-Bank swap did not replace exactly the selected resident.");
        if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "bank-swap.bmp"), size.X, size.Y);
        Console.WriteLine("Farm pointer smoke passed: twelve-resident wrap and pointer selection, geometry click, nonselected owner, duplicate key isolation, locked-save retry, saved reward, full-room bank swap and profile reload.");
        return true;
    }
    public void Dispose() => saveLock?.Dispose();
}

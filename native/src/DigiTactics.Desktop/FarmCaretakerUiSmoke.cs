using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Real prepaid care, renewal and upgrade controls against an isolated profile.</summary>
internal sealed class FarmCaretakerUiSmoke
{
    private readonly string directory;
    private readonly string[] actions = ["farm", "farmCaretakers", "farmHire/basic", "farmHire/full", "farmHire/full", "farmHire/basic", "farmManagementClose", "closeFarm", "farm", "farmCaretakers"];
    private long firstExpiry;
    private int stage, cooldown = 4;
    private bool down;
    public string Pending => $"caretaker {stage}/{cooldown}";
    public FarmCaretakerUiSmoke(NativeProfileStore profile, string directory)
    {
        if (profile.Exists) throw new InvalidOperationException("Caretaker smoke requires a fresh isolated profile.");
        this.directory = directory;
        var wallet = new CosmeticCollection(null);
        foreach (var item in CosmeticCollection.Items("legend").Take(6)) wallet.Grant("legend", item.Id);
        var walletJson = JsonNode.Parse(wallet.Json)!; walletJson["bits"] = 2000;
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var farm = JsonNode.Parse(FarmSimulation.Advance(null, walletJson.ToJsonString(), null, now).Save)!;
        foreach (var (_, pet) in farm["pets"]!.AsObject())
        {
            pet!["hunger"] = 0; pet["energy"] = 3; pet["happy"] = 0; pet["disc"] = 37; pet["weight"] = 24;
            pet["sick"] = true; pet["sickWhy"] = "dirty"; pet["dirt"] = 500;
            pet["mistakes"] = 7; pet["poopNeglect"] = 9; pet["str"] = 13; pet["goodBond"] = 1.25;
            pet["poop"] = new JsonArray(new JsonObject { ["t"] = now, ["x"] = 1, ["z"] = 1, ["k"] = 1, ["n"] = true });
        }
        if (!profile.UpdateStorage(new Dictionary<string,string> { ["dt.farm.v1"] = farm.ToJsonString(), ["dt.cosmetics.v1"] = walletJson.ToJsonString(), ["dt.future"] = "keep" })) throw new IOException(profile.Error);
    }
    public void Input(MatchScreen screen)
    {
        if (cooldown-- > 0 || stage >= actions.Length) return;
        var io = ImGui.GetIO();
        if (down) { io.AddMouseButtonEvent(0, false); down = false; stage++; cooldown = 8; return; }
        if (!screen.Buttons.TryGetValue(actions[stage], out var point)) throw new InvalidOperationException("Missing caretaker control: " + actions[stage]);
        if (stage is >= 2 and <= 6)
        {
            var bounds = screen.FarmManagementBounds;
            if (point.Y < bounds.Min.Y + 22 || point.Y > bounds.Max.Y - 22)
            {
                io.AddMousePosEvent(bounds.Max.X - 10, (bounds.Min.Y + bounds.Max.Y) / 2);
                io.AddMouseWheelEvent(0, point.Y < bounds.Min.Y + 22 ? .5f : -.5f); cooldown = 2; return;
            }
        }
        io.AddMousePosEvent(point.X, point.Y); io.AddMouseButtonEvent(0, true); down = true;
    }
    public bool Verify(MatchScreen screen, int2 size, string? capture)
    {
        if (cooldown != 1 || stage < 2) return false;
        var reload = new NativeProfileStore(directory); var farm = JsonNode.Parse(reload.Storage["dt.farm.v1"])!;
        string? tier = FarmCaretaker.Current(farm)?.Id;
        string? expected = stage == 2 ? null : stage == 3 ? "basic" : "full";
        long bits = stage == 2 ? 2000 : stage == 3 ? 1900 : stage == 4 ? 1250 : 500;
        if (tier != expected || reload.Progress.Bits != bits || reload.Storage["dt.future"] != "keep") throw new InvalidOperationException("Caretaker UI charge/tier/reload mismatch: " + Pending);
        if (stage == 3) firstExpiry = FarmCaretaker.Until(farm);
        if (stage >= 4 && FarmCaretaker.Until(farm) != firstExpiry + (stage >= 5 ? FarmCaretaker.Day : 0)) throw new InvalidOperationException("Caretaker upgrade/renewal lost paid time.");
        if (tier is not null)
            foreach (var (_, pet) in farm["pets"]!.AsObject())
            {
                int floor = tier == "full" ? 100 : 40;
                if (new[] { "hunger", "energy", "happy" }.Any(key => pet![key]!.GetValue<double>() < floor)
                    || pet!["sick"]!.GetValue<bool>() || pet["poop"]!.AsArray().Count != 0
                    || pet["mistakes"]!.GetValue<int>() != 7 || pet["poopNeglect"]!.GetValue<int>() != 9 || pet["str"]!.GetValue<int>() != 13 || pet["goodBond"]!.GetValue<double>() != 1.25
                    || pet["disc"]!.GetValue<double>() != (tier == "full" ? 100 : 37))
                    throw new InvalidOperationException("Caretaker UI changed historical or trained progress, or missed upkeep.");
            }
        if (capture is not null && stage is 2 or 3 or 4 or 5 or 10)
            FrameCapture.SaveBitmap(Path.ChangeExtension(capture, $"caretaker-{stage}.bmp"), size.X, size.Y);
        if (stage != actions.Length) return false;
        Console.WriteLine("Caretaker UI passed: Basic100/24h, Full650 upgrade preserving expiry,750 renewal adding24h, blocked downgrade, persisted time and all-resident care/history."); return true;
    }
}

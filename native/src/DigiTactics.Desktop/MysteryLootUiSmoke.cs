using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Earns a real first-round box, then opens and equips it through normal UI controls.</summary>
internal sealed class MysteryLootUiSmoke
{
    private readonly NativeProfileStore profile;
    private int step, cooldown = 5;
    private bool down, earned, blocked, discovered;
    private MysteryReward? reward;
    internal int Step => step;
    public MysteryLootUiSmoke(MatchScreen screen, NativeProfileStore profile)
    {
        if (profile.Exists) throw new InvalidOperationException("Mystery loot UI smoke requires a new isolated profile.");
        this.profile = profile; screen.MysteryRandom = () => 0;
    }
    public void Input(MatchScreen screen)
    {
        if (cooldown-- > 0) return;
        if (down) { ImGui.GetIO().AddMouseButtonEvent(0, false); down = false; if(step!=1)step++; cooldown = 5; return; }
        if (step == 1 && screen.MysteryInventory is { Daily: 1 } && screen.Session?.Rounds.Phase == MatchPhase.Result) { step=3; cooldown=5; return; }
        if (step >= 12) return;
        string key = step switch
        {
            0 => "start", 1 => screen.Session?.Rounds.Phase switch { MatchPhase.Planning=>"fight",MatchPhase.Result=>"next",_=>"finish" }, 3 => "gameMenu", 4 => "requestLeave", 5 => "confirmLeave",
            6 => "collection", 7 => "toggleEggs", 8 => "mysteryOpen/vault", 9 => "mysteryOpen/daily", 10 => "equipMystery", 11 => "mysteryOpen/daily",
            _ => throw new InvalidOperationException("Unexpected mystery UI step.")
        };
        if (!screen.Buttons.TryGetValue(key, out var point)) return;
        if (point.X < 0 || point.Y < 0 || point.X >= ImGui.GetIO().DisplaySize.X || point.Y >= ImGui.GetIO().DisplaySize.Y) throw new InvalidOperationException("Mystery control outside window: " + key);
        ImGui.GetIO().AddMousePosEvent(point.X, point.Y); ImGui.GetIO().AddMouseButtonEvent(0, true); down = true;
    }
    public bool Verify(MatchScreen screen, int2 size, string? capture)
    {
        if (step == 3 && cooldown < 2 && !earned)
        {
            if (screen.CompletedRounds < 1 || screen.MysteryInventory is not { Daily: 1, Vault: 0, Keys: 0 }) throw new InvalidOperationException($"Live completed battle did not award the first daily box exactly once: rounds={screen.CompletedRounds}, phase={screen.Session?.Rounds.Phase}, inventory={screen.MysteryInventory}, profile={profile.Error}.");
            earned = true;
        }
        if (step == 9 && cooldown < 2 && !blocked)
        {
            if (screen.RevealedMysteryReward is not null || screen.MysteryInventory is not { Daily: 1, Vault: 0, Keys: 0 }) throw new InvalidOperationException("Unavailable Cipher box/key control altered inventory.");
            if (capture is not null) FrameCapture.SaveBitmap(Path.ChangeExtension(capture, "inventory.bmp"), size.X, size.Y);
            blocked = true;
        }
        if (step == 10 && cooldown < 2 && !discovered)
        {
            reward = screen.RevealedMysteryReward;
            if (reward is null || screen.MysteryInventory is not { Daily: 0, Pending: null } || !new CosmeticCollection(profile.Storage.GetValueOrDefault("dt.cosmetics.v1")).Owns(reward.Kind, reward.Id)) throw new InvalidOperationException("Open did not durably reveal one new discovery.");
            if (capture is not null) FrameCapture.SaveBitmap(capture, size.X, size.Y);
            discovered = true;
        }
        if (step < 12 || cooldown > 0) return false;
        var collection = new CosmeticCollection(profile.Storage.GetValueOrDefault("dt.cosmetics.v1"));
        if (!earned || !blocked || !discovered || reward is null || screen.RevealedMysteryReward is not null || screen.MysteryInventory is not { Daily: 0, Vault: 0, Keys: 0, Pending: null } || collection.Equipped(reward.Kind)?.GetValue<string>() != reward.Id || collection.Bits != 0)
            throw new InvalidOperationException("Equip/repeated empty-box action changed the discovery or spending currency.");
        Console.WriteLine("Mystery loot UI passed: actual live-round daily drop, locked Cipher action, discovery reveal, durable ownership, Equip and empty-box idempotence."); return true;
    }
}

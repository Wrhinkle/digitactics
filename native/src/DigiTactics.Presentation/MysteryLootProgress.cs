using DigiTactics.Storage;

namespace DigiTactics.Presentation;

/// <summary>Commits local drops and resumes the original pending-grant transaction after interruption.</summary>
public sealed class MysteryLootProgress(NativeProfileStore profile)
{
    public string? Error { get; private set; }
    public string? Message { get; private set; }
    public MysteryInventory Inventory => MysteryLoot.Inventory(profile.Storage.GetValueOrDefault(MysteryLoot.StorageKey));
    private bool Commit(MysteryLootUpdate update)
    {
        if (!update.Changed) { Error = null; return true; }
        var changes = new Dictionary<string, string> { [MysteryLoot.StorageKey] = update.Loot };
        if (update.Wallet is not null) changes["dt.cosmetics.v1"] = update.Wallet;
        bool result = profile.UpdateStorage(changes); Error = result ? null : profile.Error; return result;
    }
    public bool Recover()
    {
        try
        {
            var result = MysteryLoot.Settle(profile.Storage.GetValueOrDefault(MysteryLoot.StorageKey), profile.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            bool saved = Commit(result);
            if (saved && Inventory.Pending is not null) { Error = "This pending mystery reward is unavailable in this version. Your box and reward record were preserved."; return false; }
            return saved;
        }
        catch (Exception error) when (Expected(error)) { Error = "Mystery loot was preserved: " + error.Message; return false; }
    }
    public string[] RecordRound(MysteryRound round, DateOnly day, Func<double> random)
    {
        try
        {
            var update = MysteryLoot.RoundEnd(profile.Storage.GetValueOrDefault(MysteryLoot.StorageKey), round, day, random);
            return Commit(update) ? update.Drops : [];
        }
        catch (Exception error) when (Expected(error)) { Error = "Mystery loot could not be saved: " + error.Message; return []; }
    }
    public MysteryReward? Open(string pool, Func<double> random)
    {
        Message = null;
        if (!Recover()) return null;
        try
        {
            var pending = MysteryLoot.PrepareOpen(profile.Storage.GetValueOrDefault(MysteryLoot.StorageKey), profile.Storage.GetValueOrDefault("dt.cosmetics.v1"), pool, random);
            Message = pending.Message;
            if (!pending.Changed || !Commit(pending)) return null;
            // The selected item is durable before grant. A failed second write is retried without rolling again.
            return Recover() ? pending.Reward : null;
        }
        catch (Exception error) when (Expected(error)) { Error = "Mystery box could not be opened: " + error.Message; return null; }
    }
    private static bool Expected(Exception error) => error is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException or KeyNotFoundException;
}

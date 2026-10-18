using System.Security.Cryptography;
using Brutal.ImGuiApi;
using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private MysteryLootProgress? mysteryLoot;
    private string? mysteryObservedSave;
    private bool mysteryInitialized;
    private MysteryReward? mysteryReward;
    private string? mysteryNotice;
    private double mysteryNoticeUntil;
    internal Func<double> MysteryRandom { private get; set; } = SecureMysteryRoll;
    internal MysteryInventory? MysteryInventory => profile is null ? null : MysteryLoot.Inventory(profile.Storage.GetValueOrDefault(MysteryLoot.StorageKey));
    internal MysteryReward? RevealedMysteryReward => mysteryReward;
    private static double SecureMysteryRoll()
    {
        Span<byte> bytes = stackalloc byte[4]; RandomNumberGenerator.Fill(bytes);
        return System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(bytes) / 4294967296.0;
    }
    private void UpdateMysteryLoot()
    {
        if (profile is null) return;
        mysteryLoot ??= new(profile);
        string? current = profile.Storage.GetValueOrDefault(MysteryLoot.StorageKey);
        if (mysteryInitialized && current == mysteryObservedSave) return;
        mysteryInitialized = true; mysteryLoot.Recover();
        mysteryObservedSave = profile.Storage.GetValueOrDefault(MysteryLoot.StorageKey);
        if (current is null) mysteryReward = null;
    }
    private void AwardMysteryRound()
    {
        if (profile is null || Session is null || IsSpectating || networkReplica?.LastDesync is not null) return;
        mysteryLoot ??= new(profile);
        // Only local settlement and MatchReplica.Applied(end) reach RoundFinished; rebuilds never do.
        var drops = mysteryLoot.RecordRound(MysteryRoundProjection.From(Session, localSeat), DateOnly.FromDateTime(DateTime.Now), MysteryRandom);
        if (drops.Length > 0) { mysteryNotice = string.Join(" + ", drops) + " collected! Open it in Loot."; mysteryNoticeUntil = boardSceneTime + 2.7; }
        if (mysteryLoot.Error is { } error) profileMessage = error;
    }
    private void DrawMysteryLootNotice()
    {
        if (mysteryNotice is null || boardSceneTime >= mysteryNoticeUntil) return;
        Window("Mystery loot collected", 24, size.Y - 118, 360, 92, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoInputs);
        ImGui.Text("MYSTERY LOOT"u8); UiText.Wrapped(mysteryNotice); ImGui.End();
    }
    private void MysteryLootControls(CosmeticCollection collection)
    {
        UpdateMysteryLoot();
        ImGui.SeparatorText("Mystery boxes"u8);
        CollectionText("Exclusive surprises. Rewards appear in Collection only after you discover them.");
        try
        {
            var inventory = mysteryLoot!.Inventory;
            ImGui.Text($"Daybreak Box x {inventory.Daily}"); ImGui.SameLine();
            ImGui.BeginDisabled(inventory.Daily < 1 || inventory.Pending is not null);
            if (Button("mysteryOpen/daily", "Open")) OpenMysteryBox("daily"); ImGui.EndDisabled();
            CollectionText($"Complete a round on 2 different days to earn a Daybreak Box. Progress: {inventory.DayProgress}/2 active days. Breaks do not reset it.");
            ImGui.Text($"Cipher Box x {inventory.Vault} / Cipher Keys: {inventory.Keys}"); ImGui.SameLine();
            ImGui.BeginDisabled(inventory.Vault < 1 || inventory.Keys < 1 || inventory.Pending is not null);
            if (Button("mysteryOpen/vault", "Unlock")) OpenMysteryBox("vault"); ImGui.EndDisabled();
            CollectionText("From stage 2, each completed round you participate in has a 3% Cipher Box chance and an independent 2.5% key chance. Guaranteed by 32 eligible rounds without a box or 40 without a key. Each box reveals one new cosmetic; the two pools never overlap. No purchases required.");
            if (mysteryLoot.Message is { } message) CollectionText(message);
            if (mysteryLoot.Error is { } error)
            {
                CollectionText(error);
                if (inventory.Pending is { } pending && Button("retryMysteryGrant", "Retry saving discovery") && mysteryLoot.Recover()) mysteryReward = pending;
            }
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidOperationException or InvalidDataException or FormatException or OverflowException)
        { CollectionText("Mystery loot could not be read. Your save was preserved: " + error.Message); }
        ImGui.Separator();
    }
    private void OpenMysteryBox(string pool)
    {
        mysteryReward = mysteryLoot!.Open(pool, MysteryRandom);
        if (mysteryReward is not null) { collectionMessage = null; audio.Play("pick"); }
    }
    private void MysteryRevealPanel(CosmeticCollection collection)
    {
        var reward = mysteryReward!; var item = CosmeticCollection.Item(reward.Kind, reward.Id)!;
        ImGui.Text("NEW DISCOVERY"u8); ImGui.Text(item.Name);
        CosmeticPreview(reward.Kind, item, collection, Math.Min(420, ImGui.GetContentRegionAvail().X));
        if (item.Desc is not null) CollectionText(item.Desc);
        if (Button("equipMystery", "Equip", 130, 34))
        {
            if (ChangeCollection(c => c.Equip(reward.Kind, reward.Id), "Equipped " + item.Name + ".")) { mysteryReward = null; audio.PlayClient("equip", false); }
        }
        ImGui.SameLine(); if (Button("closeMystery", "Keep in collection", 200, 34)) mysteryReward = null;
    }
}

using System.Security.Cryptography;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private MysteryLootProgress? mysteryLoot;
    private string? mysteryObservedSave;
    private bool mysteryInitialized;
    private MysteryReward? mysteryReward;
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
        if (current is null) {mysteryReward = null;lootAwardNotices.Clear();}
    }
    private void AwardMysteryRound()
    {
        if (profile is null || Session is null || IsSpectating || networkReplica?.LastDesync is not null) return;
        mysteryLoot ??= new(profile);
        // Only local settlement and MatchReplica.Applied(end) reach RoundFinished; rebuilds never do.
        var drops = mysteryLoot.RecordRound(MysteryRoundProjection.From(Session, localSeat), DateOnly.FromDateTime(DateTime.Now), MysteryRandom);
        if (drops.Length > 0)
        {
            var round=Session.Rounds.Position;
            lootAwardNotices.Earned($"{Session.State.Rules.Id}:{Session.State.Duos}:{Session.State.Seed}:{localSeat}:{round.Stage}:{round.Round}",round.Label,drops);
        }
        if (mysteryLoot.Error is { } error) profileMessage = error;
    }
    private void MysteryLootControls(CosmeticCollection collection)
    {
        UpdateMysteryLoot();
        HomeLabel("MYSTERY BOXES",17,HomeInk);
        ImGui.PushFont(NativeUiTheme.BodyFont,15);
        CollectionText("Exclusive surprises. Rewards appear in Collection only after you discover them.");
        try
        {
            var inventory = mysteryLoot!.Inventory;
            MysteryBoxRow("daily",$"Daybreak Box × {inventory.Daily}",$"{inventory.DayProgress}/2 active days toward your next box",inventory.Daily>0&&inventory.Pending is null);
            MysteryBoxRow("vault",$"Cipher Box × {inventory.Vault}",$"Needs 1 Cipher Key. You have {inventory.Keys}.",inventory.Vault>0&&inventory.Keys>0&&inventory.Pending is null);
            CollectionText("Boxes and keys can drop after rounds from stage 2 onward. No purchases required. Each reveals one new cosmetic; the two pools never overlap.");
            if(ImGui.CollapsingHeader("Earning boxes and keys"u8))CollectionText("Complete a round on 2 different days for a Daybreak Box; breaks do not reset progress. Eligible rounds have a 3% Cipher Box chance and an independent 2.5% key chance. Guaranteed by 32 rounds without a box or 40 without a key.");
            if (mysteryLoot.Message is { } message) CollectionText(message);
            if (mysteryLoot.Error is { } error)
            {
                CollectionText(error);
                if (inventory.Pending is { } pending && Button("retryMysteryGrant", "Retry saving discovery") && mysteryLoot.Recover()) mysteryReward = pending;
            }
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidOperationException or InvalidDataException or FormatException or OverflowException)
        { CollectionText("Mystery loot could not be read. Your save was preserved: " + error.Message); }
        finally{ImGui.PopFont();}
        ImGui.Separator();
    }
    private void MysteryBoxRow(string pool,string title,string detail,bool available)
    {
        const float symbolWidth=32,textLeft=44,buttonWidth=76,gap=12;
        var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X,textWidth=width-textLeft-buttonWidth-gap;
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,17);float titleHeight=ImGui.CalcTextSize(title,false,textWidth).Y;ImGui.PopFont();
        ImGui.PushFont(NativeUiTheme.BodyFont,15);float detailHeight=ImGui.CalcTextSize(detail,false,textWidth).Y;ImGui.PopFont();
        float height=Math.Max(76,32+titleHeight+5+detailHeight);var center=at+new float2(symbolWidth*.5f,height*.5f);
        var draw=ImGui.GetWindowDrawList();var gold=new ImColor8(238,194,121);
        if(pool=="daily")
        {
            draw.AddCircleFilled(center,7,gold);
            for(int ray=0;ray<8;ray++)
            {
                float angle=ray*MathF.PI/4;var direction=new float2(MathF.Cos(angle),MathF.Sin(angle));
                draw.AddLine(center+direction*11,center+direction*15,gold,2);
            }
        }
        else
        {
            float2[] corners=[new(0,-11),new(11,0),new(0,11),new(-11,0)];
            for(int edge=0;edge<4;edge++)draw.AddLine(center+corners[edge],center+corners[(edge+1)%4],gold,2);
        }
        ImGui.SetCursorScreenPos(at+new float2(textLeft,16));ImGui.PushFont(NativeUiTheme.DisplayBoldFont,17);ImGui.PushStyleColor(ImGuiCol.Text,HomeInk);
        ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+textWidth);ImGui.Text(title);ImGui.PopTextWrapPos();ImGui.PopStyleColor();ImGui.PopFont();
        ImGui.SetCursorScreenPos(at+new float2(textLeft,21+titleHeight));ImGui.PushFont(NativeUiTheme.BodyFont,15);ImGui.PushStyleColor(ImGuiCol.Text,HomeMute);
        ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+textWidth);ImGui.Text(detail);ImGui.PopTextWrapPos();ImGui.PopStyleColor();ImGui.PopFont();
        ImGui.SetCursorScreenPos(at+new float2(width-buttonWidth,(height-36)*.5f));ImGui.BeginDisabled(!available);
        if(Button("mysteryOpen/"+pool,pool=="daily"?"Open":"Unlock",buttonWidth,36))OpenMysteryBox(pool);
        ImGui.EndDisabled();ImGui.SetCursorScreenPos(at+new float2(0,height));ImGui.Separator();
    }
    private void OpenMysteryBox(string pool)
    {
        mysteryReward = mysteryLoot!.Open(pool, MysteryRandom);
        if (mysteryReward is not null) { collectionMessage = null; audio.Play("pick"); }
    }
    private void MysteryRevealPanel(CosmeticCollection collection)
    {
        var reward = mysteryReward!; var item = CosmeticCollection.Item(reward.Kind, reward.Id)!;
        CollectionModalText("NEW DISCOVERY",HomeGold,17);CollectionModalTitle(item.Name);
        float previewWidth=Math.Min(440,Math.Min(ImGui.GetContentRegionAvail().X,Math.Max(240,(ImGui.GetContentRegionAvail().Y-150)*1.6f)));
        CollectionModalPreview(reward.Kind,item,collection,previewWidth);
        if (item.Desc is not null) CollectionModalText(item.Desc,HomeInk);
        CollectionModalButtons(338);
        if (HomeButton("equipMystery", "Equip", 130, 38,true))
        {
            if (ChangeCollection(c => c.Equip(reward.Kind, reward.Id), "Equipped " + item.Name + ".")) { mysteryReward = null; audio.PlayClient("equip", false); }
        }
        ImGui.SameLine(); if (HomeButton("closeMystery", "Keep in collection", 200, 38)) mysteryReward = null;
    }
}

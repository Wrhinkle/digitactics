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
    /// <summary>.mystery-boxes: heading, intro, the Daybreak and Cipher rows and the earning rules. Returns the next y.</summary>
    private float MysteryLootControls(CosmeticCollection collection,float2 at,float width)
    {
        UpdateMysteryLoot();
        var draw=ImGui.GetWindowDrawList();var text=new ImColor8(180,194,213);var body=CosmeticStyle.Body(12);
        float x=at.X+16,inner=width-32,y=at.Y+16;
        y+=4;CosmeticStyle.TextIn(draw,new(x,y),18.9f,CosmeticStyle.Gold1,"MYSTERY BOXES",CosmeticStyle.Display(12.5f,.1f));y+=18.9f+8;
        y+=CosmeticStyle.Paragraph(draw,new(x,y),text,"Exclusive surprises. Rewards appear in Collection only after you discover them.",body,inner,1.6f)+12;
        try
        {
            var inventory=mysteryLoot!.Inventory;
            y=MysteryBoxRow("daily",$"Daybreak Box × {inventory.Daily}",$"Complete a round on 2 different days. Progress: {inventory.DayProgress}/2 active days. Breaks do not reset it.",inventory.Daily>0&&inventory.Pending is null,new(x,y),inner);
            y=MysteryBoxRow("vault",$"Cipher Box × {inventory.Vault}",$"Needs 1 Cipher Key. You have {inventory.Keys}.",inventory.Vault>0&&inventory.Keys>0&&inventory.Pending is null,new(x,y),inner);
            y+=12;
            y+=CosmeticStyle.Paragraph(draw,new(x,y),text,"From stage 2, each completed round you participate in has a 3% Cipher Box chance and an independent 2.5% key chance. Guaranteed by 32 eligible rounds without a box or 40 without a key. No purchases required. Each box reveals one new cosmetic; the two pools never overlap.",body,inner,1.6f);
            if(mysteryLoot.Message is {} message){y+=8;y+=CosmeticStyle.Paragraph(draw,new(x,y),CosmeticStyle.Gold2,message,body,inner,1.6f);}
            if(mysteryLoot.Error is {} error)
            {
                y+=8;y+=CosmeticStyle.Paragraph(draw,new(x,y),new ImColor8(255,84,104),error,body,inner,1.6f)+6;
                if(inventory.Pending is {} pending)
                {
                    var type=CosmeticStyle.Body(14,true);
                    if(CosmeticButton("retryMysteryGrant",new(x,y),new(type.Width("Retry saving discovery")+32,35),"Retry saving discovery",type)&&mysteryLoot.Recover())mysteryReward=pending;
                    y+=35;
                }
            }
        }
        catch(Exception error) when (error is System.Text.Json.JsonException or InvalidOperationException or InvalidDataException or FormatException or OverflowException)
        {y+=CosmeticStyle.Paragraph(draw,new(x,y),text,"Mystery loot could not be read. Your save was preserved: "+error.Message,body,inner,1.6f);}
        y+=16;
        draw.AddLine(new(at.X,y+.5f),new(at.X+width,y+.5f),new ImColor8(88,96,116));
        return y+1+12;
    }
    private float MysteryBoxRow(string pool,string title,string detail,bool available,float2 at,float width)
    {
        var draw=ImGui.GetWindowDrawList();var gold=new ImColor8(238,194,121);var titleType=CosmeticStyle.Body(14,true);var small=CosmeticStyle.Body(11.7f);
        var buttonType=CosmeticStyle.Body(14,true);string label=pool=="daily"?"Open":"Unlock";float buttonWidth=buttonType.Width(label)+34;
        const float symbolWidth=32;float textLeft=symbolWidth+12,textWidth=width-textLeft-12-buttonWidth;
        float detailHeight=CosmeticStyle.ParagraphHeight(detail,small,textWidth,1.6f);
        float contentHeight=18.9f+5+detailHeight,height=16+Math.Max(contentHeight,40)+16;
        var center=at+new float2(symbolWidth*.5f,height*.5f);
        if(pool=="daily")
        {
            draw.AddCircleFilled(center,6,gold);
            for(int ray=0;ray<8;ray++){float angle=ray*MathF.PI/4;var direction=new float2(MathF.Cos(angle),MathF.Sin(angle));draw.AddLine(center+direction*9,center+direction*13,gold,2);}
        }
        else
        {
            float2[] corners=[new(0,-11),new(11,0),new(0,11),new(-11,0)];
            for(int edge=0;edge<4;edge++)draw.AddLine(center+corners[edge],center+corners[(edge+1)%4],gold,2);
        }
        float top=at.Y+(height-contentHeight)*.5f;
        CosmeticStyle.TextIn(draw,new(at.X+textLeft,top),18.9f,CosmeticStyle.Text1,title,titleType);
        CosmeticStyle.Paragraph(draw,new(at.X+textLeft,top+18.9f+5),new ImColor8(180,194,213),detail,small,textWidth,1.6f);
        if(CosmeticButton("mysteryOpen/"+pool,new(at.X+width-buttonWidth,at.Y+(height-35)*.5f),new(buttonWidth,35),label,buttonType,disabled:!available))OpenMysteryBox(pool);
        draw.AddLine(new(at.X,at.Y+height-.5f),new(at.X+width,at.Y+height-.5f),new ImColor8(255,255,255,21));
        return at.Y+height;
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

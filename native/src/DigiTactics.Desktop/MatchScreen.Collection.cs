using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool showCollection;
    private bool showEggs;
    private bool collectionStore;
    private string? evolutionLegend;
    private (string Id, double RarityRoll, double ItemRoll, long At)? eggPurchase;
    private int collectionKind, collectionSlot;
    private string? collectionMessage;
    private (string Kind, string Id, DateOnly Day)? purchase;
    private static readonly string[] CollectionNames = ["Little Legends", "Emotes", "Boards", "Kill Screens", "Dances", "Soundboard", "Elim Sounds"];
    private bool ChangeCollection(Func<CosmeticCollection, bool> change, string success)
    {
        try
        {
            var next = new CosmeticCollection(profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            if (!change(next)) { collectionMessage = "That change is unavailable. Check ownership and your Bits balance."; return false; }
            bool saved=profile.UpdateStorage(new Dictionary<string, string> { ["dt.cosmetics.v1"] = next.Json });
            collectionMessage = saved ? success : profile.Error; return saved;
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException)
        { collectionMessage = "Collection could not be updated: " + error.Message; return false; }
    }
    private static void CollectionText(string text)
    {
        ImGui.PushTextWrapPos(0); ImGui.Text(text); ImGui.PopTextWrapPos();
    }
    private void Collection()
    {
        ImGui.PushFont(default,17);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(10,9));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(8,8));
        try { CollectionPage(); }
        finally { ImGui.PopStyleVar(2); ImGui.PopFont(); }
    }
    private void CollectionPage()
    {
        ImGui.SetNextWindowBgAlpha(1);
        if(evolutionLegend is not null)
        {
            float top=ClientHomeVisible?ClientPageTop:10;
            float width=Math.Min(1180,size.X-20),height=Math.Min(740,size.Y-top-10);
            Window("Digivolve",(size.X-width)/2,top+(size.Y-top-10-height)/2,width,height);
        }
        else if(MatchCosmeticsVisible)
        {
            float width=Math.Min(1180,size.X-48),height=Math.Min(800,size.Y-48);
            var at=new float2((size.X-width)/2,(size.Y-height)/2);
            HomeCard("Match cosmetics",at,new(width,height),false,true);MatchCosmeticsBounds=(at,at+new float2(width,height));
        }
        else HomeCard("Collection page",new(16,ClientPageTop),new(size.X-32,size.Y-ClientPageTop-16),false,true);
        string backLabel=MatchCosmeticsVisible?(evolutionLegend is null?"Return to match":"Back to cosmetics"):"Back";
        if (Button("closeCollection", backLabel)&&!EvolutionBusy)
        {
            bool returnToCollection=MatchCosmeticsVisible&&evolutionLegend is not null;
            CloseEvolution();
            if(!returnToCollection){showCollection=false;purchase=null;eggPurchase=null;hatching=null;boughtCosmetic=null;mysteryReward=null;DisposeCollectionPreview();}
            ImGui.End();return;
        }
        try
        {
            var collection = new CosmeticCollection(profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            ImGui.SameLine(); ImGui.Text($"{collection.Bits} Bits");
            MatchCollectionNavigation();
            CollectionRewardControls();
            if (collectionMessage is not null) CollectionText(collectionMessage);
            if (evolutionLegend is not null) { EvolutionControls(evolutionLegend); ImGui.End(); return; }
            if(boughtCosmetic is not null){BoughtPanel(collection);ImGui.End();return;}
            if(mysteryReward is not null){MysteryRevealPanel(collection);ImGui.End();return;}
            if (hatching is not null) { HatchPanel(collection); ImGui.End(); return; }
            if (eggPurchase is { } eggPending)
            {
                var egg = DigiEggs.Definitions.Single(x => x.Id == eggPending.Id);
                ImGui.Separator(); CollectionText($"Hatch {egg.Name} for {egg.Price} Bits?"); EggArt(egg.Id,180);
                CollectionText("The result may be a duplicate. Duplicate Little Legends grant Digi-Data until max level; other duplicates refund 40% of their item price.");
                if (Button("confirmEgg", "Hatch"))
                {
                    var result = DigiEggs.Purchase(profile.Storage.GetValueOrDefault("dt.cosmetics.v1"), profile.Storage.GetValueOrDefault("dt.legendevo.v1"),
                        eggPending.Id, eggPending.RarityRoll, eggPending.ItemRoll, eggPending.At);
                    if (result is null) collectionMessage = "Not enough Bits to hatch this egg.";
                    else
                    {
                        var changes = new Dictionary<string, string> { ["dt.cosmetics.v1"] = result.Wallet };
                        if (result.Legends is not null && result.Legends != profile.Storage.GetValueOrDefault("dt.legendevo.v1")) changes["dt.legendevo.v1"] = result.Legends;
                        if (!profile.UpdateStorage(changes)) collectionMessage = profile.Error;
                        else
                        {
                            eggPurchase = null; hatching = (eggPending.Id, result.Award, boardSceneTime); hatchCracked = hatchRevealed = false; collectionMessage = null; audio.Play("reroll");
                        }
                    }
                }
                ImGui.SameLine(); if (Button("cancelEgg", "Cancel")) eggPurchase = null;
                ImGui.End(); return;
            }
            if (purchase is { } pending)
            {
                var item = CosmeticCollection.Item(pending.Kind, pending.Id)!;
                ImGui.Separator(); CollectionText($"Buy {item.Name} for {CosmeticCollection.Price(pending.Kind, item, pending.Day)} Bits?");
                CosmeticPreview(pending.Kind,item,collection,Math.Min(320,ImGui.GetContentRegionAvail().X));
                if (Button("confirmPurchase", "Buy")) { if(ChangeCollection(c => c.Buy(pending.Kind, pending.Id, pending.Day), "Bought " + item.Name + ".")){boughtCosmetic=(pending.Kind,pending.Id);previewCosmetic=pending.Id;audio.Play("buy");} purchase=null; }
                ImGui.SameLine(); if (Button("cancelPurchase", "Cancel")) purchase = null;
                ImGui.End(); return;
            }
            if (showEggs)
            {
                ImGui.SameLine();HomeLabel("LOOT / DIGI-EGGS",24,HomeInk);
                if (Button("toggleEggs", "Cosmetics")) showEggs=false;
                IncubatorControls(collection);
                ImGui.End(); return;
            }
            CollectionBrowser(collection);
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException or IOException or KeyNotFoundException)
        { CollectionText("Collection unavailable: " + error.Message); }
        ImGui.End();
    }
}

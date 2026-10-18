using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool showCollection;
    private bool showEggs;
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
        ImGui.SetNextWindowBgAlpha(1);
        if(evolutionLegend is not null)
        {
            float width=Math.Min(1180,size.X-20),height=Math.Min(740,size.Y-20);
            Window("Digivolve",(size.X-width)/2,(size.Y-height)/2,width,height);
        }
        else Window("Collection and store", size.X * .12f, size.Y * .06f, size.X * .76f, size.Y * .88f);
        if (Button("closeCollection", "Back")) { CloseEvolution();showCollection = false; purchase = null; eggPurchase = null; hatching = null; boughtCosmetic=null; mysteryReward=null; DisposeCollectionPreview(); ImGui.End(); return; }
        try
        {
            var collection = new CosmeticCollection(profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            ImGui.SameLine(); ImGui.Text($"{collection.Bits} Bits");
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
            if (Button("toggleEggs", showEggs ? "Cosmetics" : "Loot / Digi-Eggs")) showEggs = !showEggs;
            if (showEggs)
            {
                IncubatorControls(collection);
                ImGui.End(); return;
            }
            ImGui.SetNextItemWidth(240);bool categoryOpen=ImGui.BeginCombo("Category"u8, CollectionNames[collectionKind]);
            var categoryMin=ImGui.GetItemRectMin();var categoryMax=ImGui.GetItemRectMax();Buttons["collectionCategory"]=new((categoryMin.X+categoryMax.X)/2,(categoryMin.Y+categoryMax.Y)/2);
            if (categoryOpen)
            {
                for (int i = 0; i < CollectionNames.Length; i++)
                {
                    bool chosen=ImGui.Selectable(CollectionNames[i], collectionKind == i);
                    ClientItemSound("collectionCategory/"+i,chosen);
                    if (chosen) CollectionCategoryChanged(i);
                    var min=ImGui.GetItemRectMin();var max=ImGui.GetItemRectMax();Buttons["collectionCategory/"+i]=new((min.X+max.X)/2,(min.Y+max.Y)/2);
                }
                ImGui.EndCombo();
            }
            string kind = CosmeticCollection.Kinds[collectionKind];
            var visible=CosmeticCollection.Items(kind).Where(item=>!item.Secret||collection.Owns(kind,item.Id)).ToArray();
            ImGui.SameLine();ImGui.Text($"{visible.Count(item=>collection.Owns(kind,item.Id))} / {visible.Length} owned");
            var filtered=CollectionFilters(collection,kind);
            var equipped = collection.Equipped(kind);
            if (CosmeticCollection.Multi(kind))
            {
                for (int i = 0; i < 6; i++)
                {
                    string? id = equipped!.AsArray().ElementAtOrDefault(i)?.GetValue<string>();
                    if(i%2!=0)ImGui.SameLine();
                    if (Button("collectionSlot/" + i, $"Slot {i + 1}: {CosmeticCollection.Item(kind, id)?.Name ?? "Empty"}", (ImGui.GetWindowSize().X-42)/2,active: collectionSlot == i)) collectionSlot = i;
                }
                if (Button("clearCollectionSlot", "Empty selected slot")) ChangeCollection(c => c.Equip(kind, null, collectionSlot), "Slot cleared.");
            }
            float listWidth=ImGui.GetContentRegionAvail().X*.53f;
            ImGui.BeginChild("collectionItems"u8,new(listWidth,0),ImGuiChildFlags.Borders);
            CollectionListBounds=(ImGui.GetWindowPos(),ImGui.GetWindowPos()+ImGui.GetWindowSize());
            try
            {
            DateOnly day = DateOnly.FromDateTime(DateTime.Now);
            if(filtered.Length==0)CollectionText("Nothing matches these filters.");
            foreach (var item in filtered)
            {
                ImGui.Separator(); ImGui.Text(item.Name); ImGui.SameLine(); ImGui.Text(item.Rarity ?? "common");
                if (Button("preview/"+kind+"/"+item.Id,"Preview",active:previewCosmetic==item.Id)) previewCosmetic=item.Id;
                ImGui.SameLine();
                bool owns = collection.Owns(kind, item.Id);
                bool worn = CosmeticCollection.Multi(kind) ? equipped!.AsArray().ElementAtOrDefault(collectionSlot)?.GetValue<string>() == item.Id : equipped?.GetValue<string>() == item.Id;
                if (owns)
                {
                    if (kind == "legend" && Button("evolution/" + item.Id, "Evolution and care")) { evolutionLegend = item.Id; collectionMessage=null; }
                    if (worn) ImGui.Text("Equipped"u8);
                    else if (Button("equip/" + kind + "/" + item.Id, CosmeticCollection.Multi(kind) ? $"Put in slot {collectionSlot + 1}" : "Equip"))
                        { if(ChangeCollection(c => c.Equip(kind, item.Id, collectionSlot), "Equipped " + item.Name + "."))audio.PlayClient("equip",false); previewCosmetic=item.Id; }
                }
                else if (item.Linked is { Length: 2 } link) CollectionText("Unlocked by owning " + (CosmeticCollection.Item(link[0], link[1])?.Name ?? link[1]));
                else if (item.EggOnly) ImGui.Text("Digi-Egg exclusive"u8);
                else
                {
                    int price = CosmeticCollection.Price(kind, item, day);
                    ImGui.BeginDisabled(price > collection.Bits);
                    if (Button("buyCosmetic/" + kind + "/" + item.Id, $"Buy for {price} Bits")) { purchase = (kind, item.Id, day); previewCosmetic=item.Id; }
                    ImGui.EndDisabled();
                }
            }
            } finally { ImGui.EndChild(); } ImGui.SameLine();
            ImGui.BeginChild("collectionPreview"u8,new(0,0),ImGuiChildFlags.Borders); try { SelectedCosmetic(collection,kind); } finally {ImGui.EndChild();}
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException or IOException or KeyNotFoundException)
        { CollectionText("Collection unavailable: " + error.Message); }
        ImGui.End();
    }
}

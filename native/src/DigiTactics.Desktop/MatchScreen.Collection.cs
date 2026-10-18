using Brutal.ImGuiApi;
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
    private void ChangeCollection(Func<CosmeticCollection, bool> change, string success)
    {
        try
        {
            var next = new CosmeticCollection(profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            if (!change(next)) { collectionMessage = "That change is unavailable. Check ownership and your Bits balance."; return; }
            collectionMessage = profile.UpdateStorage(new Dictionary<string, string> { ["dt.cosmetics.v1"] = next.Json }) ? success : profile.Error;
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException)
        { collectionMessage = "Collection could not be updated: " + error.Message; }
    }
    private static void CollectionText(string text)
    {
        ImGui.PushTextWrapPos(0); ImGui.Text(text); ImGui.PopTextWrapPos();
    }
    private void Collection()
    {
        Window("Collection and store", size.X * .12f, size.Y * .06f, size.X * .76f, size.Y * .88f);
        if (Button("closeCollection", "Back")) { showCollection = false; purchase = null; eggPurchase = null; evolutionLegend = null; }
        try
        {
            var collection = new CosmeticCollection(profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            ImGui.SameLine(); ImGui.Text($"{collection.Bits} Bits");
            if (collectionMessage is not null) CollectionText(collectionMessage);
            if (evolutionLegend is not null) { EvolutionControls(evolutionLegend); ImGui.End(); return; }
            if (eggPurchase is { } eggPending)
            {
                var egg = DigiEggs.Definitions.Single(x => x.Id == eggPending.Id);
                ImGui.Separator(); CollectionText($"Hatch {egg.Name} for {egg.Price} Bits?");
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
                            string name = CosmeticCollection.Item(result.Award.Kind, result.Award.Id)!.Name;
                            collectionMessage = result.Award.Evolution is { } evo ? $"{name}: +{evo.Data} Digi-Data, now level {evo.Level}."
                                : result.Award.Duplicate ? $"Duplicate {name}: {result.Award.Refund} Bits refunded." : $"Hatched {name}! Added to your collection.";
                            eggPurchase = null;
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
                if (Button("confirmPurchase", "Buy")) { ChangeCollection(c => c.Buy(pending.Kind, pending.Id, pending.Day), "Bought " + item.Name + "."); purchase = null; }
                ImGui.SameLine(); if (Button("cancelPurchase", "Cancel")) purchase = null;
                ImGui.End(); return;
            }
            if (Button("toggleEggs", showEggs ? "Cosmetics" : "Digi-Eggs")) showEggs = !showEggs;
            if (showEggs)
            {
                foreach (var egg in DigiEggs.Definitions)
                {
                    ImGui.Separator(); ImGui.Text(egg.Name);
                    CollectionText(string.Join(" / ", DigiEggs.Odds(egg).Select(x => $"{x.Rarity}: {x.Pct:0.##}% ({x.Count} items)")));
                    ImGui.BeginDisabled(collection.Bits < egg.Price);
                    if (Button("egg/" + egg.Id, $"Hatch for {egg.Price} Bits")) eggPurchase = (egg.Id, Random.Shared.NextDouble(), Random.Shared.NextDouble(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                    ImGui.EndDisabled();
                }
                ImGui.End(); return;
            }
            if (ImGui.BeginCombo("Category"u8, CollectionNames[collectionKind]))
            {
                for (int i = 0; i < CollectionNames.Length; i++) if (ImGui.Selectable(CollectionNames[i], collectionKind == i)) { collectionKind = i; collectionSlot = 0; }
                ImGui.EndCombo();
            }
            string kind = CosmeticCollection.Kinds[collectionKind];
            var equipped = collection.Equipped(kind);
            if (CosmeticCollection.Multi(kind))
            {
                for (int i = 0; i < 6; i++)
                {
                    string? id = equipped!.AsArray().ElementAtOrDefault(i)?.GetValue<string>();
                    if (Button("collectionSlot/" + i, $"Slot {i + 1}: {CosmeticCollection.Item(kind, id)?.Name ?? "Empty"}", active: collectionSlot == i)) collectionSlot = i;
                }
                if (Button("clearCollectionSlot", "Empty selected slot")) ChangeCollection(c => c.Equip(kind, null, collectionSlot), "Slot cleared.");
            }
            DateOnly day = DateOnly.FromDateTime(DateTime.Now);
            foreach (var item in CosmeticCollection.Items(kind).Where(it => !it.Secret || collection.Owns(kind, it.Id)))
            {
                ImGui.Separator(); ImGui.Text(item.Name); ImGui.SameLine(); ImGui.Text(item.Rarity ?? "common");
                if (item.Desc is not null) CollectionText(item.Desc);
                bool owns = collection.Owns(kind, item.Id);
                bool worn = CosmeticCollection.Multi(kind) ? equipped!.AsArray().ElementAtOrDefault(collectionSlot)?.GetValue<string>() == item.Id : equipped?.GetValue<string>() == item.Id;
                if (owns)
                {
                    if (kind == "legend" && Button("evolution/" + item.Id, "Evolution and care")) evolutionLegend = item.Id;
                    if (worn) ImGui.Text("Equipped"u8);
                    else if (Button("equip/" + kind + "/" + item.Id, CosmeticCollection.Multi(kind) ? $"Put in slot {collectionSlot + 1}" : "Equip"))
                        ChangeCollection(c => c.Equip(kind, item.Id, collectionSlot), "Equipped " + item.Name + ".");
                }
                else if (item.Linked is { Length: 2 } link) CollectionText("Unlocked by owning " + (CosmeticCollection.Item(link[0], link[1])?.Name ?? link[1]));
                else if (item.EggOnly) ImGui.Text("Digi-Egg exclusive"u8);
                else
                {
                    int price = CosmeticCollection.Price(kind, item, day);
                    ImGui.BeginDisabled(price > collection.Bits);
                    if (Button("buyCosmetic/" + kind + "/" + item.Id, $"Buy for {price} Bits")) purchase = (kind, item.Id, day);
                    ImGui.EndDisabled();
                }
            }
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException)
        { CollectionText("Collection unavailable: " + error.Message); }
        ImGui.End();
    }
}

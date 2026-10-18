using Brutal.ImGuiApi;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool showCollection;
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
    private void Collection()
    {
        Window("Collection and store", size.X * .12f, size.Y * .06f, size.X * .76f, size.Y * .88f);
        if (Button("closeCollection", "Back")) { showCollection = false; purchase = null; }
        try
        {
            var collection = new CosmeticCollection(profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            ImGui.SameLine(); ImGui.Text($"{collection.Bits} Bits");
            if (collectionMessage is not null) ImGui.TextWrapped(collectionMessage);
            if (purchase is { } pending)
            {
                var item = CosmeticCollection.Item(pending.Kind, pending.Id)!;
                ImGui.Separator(); ImGui.TextWrapped($"Buy {item.Name} for {CosmeticCollection.Price(pending.Kind, item, pending.Day)} Bits?");
                if (Button("confirmPurchase", "Buy")) { ChangeCollection(c => c.Buy(pending.Kind, pending.Id, pending.Day), "Bought " + item.Name + "."); purchase = null; }
                ImGui.SameLine(); if (Button("cancelPurchase", "Cancel")) purchase = null;
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
                if (item.Desc is not null) ImGui.TextWrapped(item.Desc);
                bool owns = collection.Owns(kind, item.Id);
                bool worn = CosmeticCollection.Multi(kind) ? equipped!.AsArray().ElementAtOrDefault(collectionSlot)?.GetValue<string>() == item.Id : equipped?.GetValue<string>() == item.Id;
                if (owns)
                {
                    if (worn) ImGui.Text("Equipped"u8);
                    else if (Button("equip/" + kind + "/" + item.Id, CosmeticCollection.Multi(kind) ? $"Put in slot {collectionSlot + 1}" : "Equip"))
                        ChangeCollection(c => c.Equip(kind, item.Id, collectionSlot), "Equipped " + item.Name + ".");
                }
                else if (item.Linked is { Length: 2 } link) ImGui.TextWrapped("Unlocked by owning " + (CosmeticCollection.Item(link[0], link[1])?.Name ?? link[1]));
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
        { ImGui.TextWrapped("Collection unavailable: " + error.Message); }
        ImGui.End();
    }
}

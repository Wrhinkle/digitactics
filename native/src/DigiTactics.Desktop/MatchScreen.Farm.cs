using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool showFarm;
    private string? farmPet;
    private bool farmManagement;
    private FarmView? farmView;
    private string? farmViewError;
    private int FarmSceneWidth => Math.Max(250, (int)(size.X * .54f) - 24);
    private int FarmSceneHeight => Math.Max(200, size.Y - 150);
    public int FarmDrawCalls => farmView?.DrawCalls ?? 0;
    public string? FarmTheme => farmView?.Theme;
    private void FarmAct(FarmCareAction action, string? option = null, int pile = 0)
    {
        string? before = farm!.Save;
        farm.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), action, farmPet, option, pile);
        if (farm.Error is null && farm.LastResult is { } receipt) farmView?.Care(farmPet, action, option, receipt, before);
    }
    private void UpdateFarmView(double delta)
    {
        if (!showFarm || farm?.Save is null || farm.Error is not null) return;
        try
        {
            farmView ??= new();
            farmView.Sync(farm.Save, profile!.Storage.GetValueOrDefault("dt.legendevo.v1"), profile.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            farmView.Update(delta); farmViewError = null;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or System.Text.Json.JsonException or InvalidOperationException or KeyNotFoundException)
        { farmViewError = "Room view unavailable: " + error.Message; }
    }
    private void FarmPanel()
    {
        if (training is null)
        {
            Window("Digi-Farm room", 12, 70, FarmSceneWidth, FarmSceneHeight, ImGuiWindowFlags.NoBackground);
            if (farmViewError is not null) CollectionText(farmViewError);
            else if (farmView is not null)
                foreach (var (id, x, y) in farmView.Anchors(FarmSceneWidth, FarmSceneHeight))
                {
                    ImGui.SetCursorScreenPos(new(12 + x - 25, 70 + y));
                    if (Button("farmResident/" + id, CosmeticCollection.Item("legend", id)!.Name, active: id == farmPet)) farmPet = id;
                }
            ImGui.End();
        }
        Window("Digi-Farm", training is null ? size.X * .55f : size.X * .12f, size.Y * .06f, training is null ? size.X * .44f : size.X * .76f, size.Y * .88f);
        if (training is not null) { TrainingPanel(); ImGui.End(); return; }
        if (Button("closeFarm", "Back")) showFarm = false;
        if (farm!.Error is { } farmError) { CollectionText(farmError); if (Button("retryFarm", "Retry saving farm")) farm.Refresh(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()); ImGui.End(); return; }
        if (farm.Message is { Length: > 0 } message) CollectionText(message);
        try
        {
            if (farm.Save is null) { ImGui.End(); return; }
            var state = JsonNode.Parse(farm.Save)!;
            var residents = state["residents"]!.AsArray().Select(x => x!.GetValue<string>()).ToArray();
            ImGui.SameLine(); ImGui.Text($"{profile!.Progress.Bits} Bits");
            ImGui.Text($"Residents: {residents.Length}/{state["cap"]}    Meat: {state["meat"]!["n"]}/3    Bandages: {state["bandage"]!["n"]}/2");
            ImGui.Text($"Daily rewards: {state["daily"]!["bits"]}/60 Bits, {state["daily"]!["data"]}/100 Digi-Data");
            if (Button("farmRoom", farmManagement ? "Care" : "Room, bank and themes")) farmManagement = !farmManagement;
            if (!residents.Contains(farmPet)) farmPet = residents.FirstOrDefault();
            if (farmManagement) { FarmRoomControls(state, residents); ImGui.End(); return; }
            if (Button("farmLights", state["lights"]!.GetValue<bool>() ? "Turn lights off" : "Turn lights on")) FarmAct(FarmCareAction.Lights);
            ImGui.SameLine(); if (Button("farmClean", "Clean room")) FarmAct(FarmCareAction.Clean);
            if (!residents.Contains(farmPet)) farmPet = residents.FirstOrDefault();
            if (farmPet is not null && ImGui.BeginCombo("Resident"u8, CosmeticCollection.Item("legend", farmPet)!.Name))
            {
                foreach (string id in residents) if (ImGui.Selectable(CosmeticCollection.Item("legend", id)!.Name, id == farmPet)) farmPet = id;
                ImGui.EndCombo();
            }
            if (farmPet is not null)
            {
                var pet = state["pets"]![farmPet]!;
                ImGui.Text($"Hunger {pet["hunger"]!.GetValue<double>():0}    Energy {pet["energy"]!.GetValue<double>():0}    Happiness {pet["happy"]!.GetValue<double>():0}    Discipline {pet["disc"]!.GetValue<double>():0}");
                ImGui.Text($"Weight {pet["weight"]}    {(pet["sick"]!.GetValue<bool>() ? "Sick: " + pet["sickWhy"] : "Healthy")}    {(pet["flags"]?["asleep"]?.GetValue<bool>() == true ? "Sleeping" : "Awake")}");
                ImGui.Text($"Strength {pet["str"]}    Speed {pet["spd"]}    Wisdom {pet["wis"]}    Stamina {pet["sta"]} (cap 300)");
                foreach (var game in TrainingTitles)
                {
                    if (Button("farmTrain/" + game.Key, game.Value)) StartTraining(game.Key);
                    ImGui.SameLine();
                }
                ImGui.NewLine();
                foreach (var food in new[] { ("meat", "Meat (tray, then 5 Bits)"), ("carrot", "Supercarrot (8 Bits)"), ("bigmeat", "Giant Meat (15 Bits)"), ("sirloin", "Sirloin (25 Bits)"), ("cake", "Digi-Cake (10 Bits)") })
                    if (Button("farmFeed/" + food.Item1, food.Item2)) FarmAct(FarmCareAction.Feed, food.Item1);
                if (Button("farmBandage", "Use bandage")) FarmAct(FarmCareAction.Cure);
                ImGui.SameLine(); if (Button("farmMedicine", "Medicine (20 Bits)")) FarmAct(FarmCareAction.Cure, "medicine");
                if (Button("farmPraise", "Praise")) FarmAct(FarmCareAction.Praise);
                ImGui.SameLine(); if (Button("farmScold", "Scold")) FarmAct(FarmCareAction.Scold);
                ImGui.SameLine(); if (Button("farmPet", "Pet")) FarmAct(FarmCareAction.Pet);
                if (Button("farmPlay", "Play with ball")) FarmAct(FarmCareAction.Play);
                foreach (string other in residents.Where(x => x != farmPet))
                    if (Button("farmPlay/" + other, "Play with " + CosmeticCollection.Item("legend", other)!.Name)) FarmAct(FarmCareAction.Play, other);
                foreach (var pile in pet["poop"]!.AsArray()) if (Button("farmPile/" + pile!["k"], "Clean pile " + pile["k"])) FarmAct(FarmCareAction.CleanPile, pile: pile["k"]!.GetValue<int>());
            }
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or KeyNotFoundException)
        { CollectionText("Farm unavailable: " + error.Message); }
        ImGui.End();
    }
    private void FarmRoomControls(JsonNode state, string[] residents)
    {
        int cap = state["cap"]!.GetValue<int>(), price = FarmCare.ExpansionPrice(cap);
        ImGui.BeginDisabled(cap >= 12 || profile!.Progress.Bits < price);
        if (Button("farmExpand", cap >= 12 ? "Maximum room size" : $"Expand to {cap + 2} residents ({price} Bits)")) FarmAct(FarmCareAction.Expand);
        ImGui.EndDisabled();
        ImGui.Separator(); ImGui.Text("Residents"u8);
        foreach (string id in residents)
        {
            if (Button("farmSelect/" + id, CosmeticCollection.Item("legend", id)!.Name, active: farmPet == id)) farmPet = id;
            ImGui.SameLine(); ImGui.BeginDisabled(residents.Length <= 1);
            if (Button("farmOut/" + id, "Send to bank")) farm!.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), FarmCareAction.MoveOut, id);
            ImGui.EndDisabled();
        }
        var collection = new CosmeticCollection(profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"));
        ImGui.Separator(); ImGui.Text("Digi-Bank (care is paused)"u8);
        foreach (var item in CosmeticCollection.Items("legend").Where(x => collection.Owns("legend", x.Id) && !residents.Contains(x.Id)))
        {
            ImGui.Text(item.Name); ImGui.SameLine(); ImGui.BeginDisabled(residents.Length >= cap);
            if (Button("farmIn/" + item.Id, "Move in")) farm!.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), FarmCareAction.MoveIn, item.Id);
            ImGui.EndDisabled();
            if (farmPet is not null)
            {
                ImGui.SameLine(); if (Button("farmSwap/" + item.Id, "Swap with " + CosmeticCollection.Item("legend", farmPet)!.Name))
                    farm!.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), FarmCareAction.Swap, item.Id, farmPet);
            }
        }
        ImGui.Separator(); ImGui.Text("Room theme"u8);
        foreach (var theme in FarmCare.Themes)
        {
            bool owned = FarmCare.OwnsTheme(state, collection, theme), selected = state["theme"]!.GetValue<string>() == theme.Id;
            ImGui.BeginDisabled(selected || !owned && collection.Bits < theme.Price);
            if (Button("farmTheme/" + theme.Id, theme.Name + (selected ? " (selected)" : owned ? " - use" : $" - {theme.Price} Bits")))
                farm!.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), owned ? FarmCareAction.SetTheme : FarmCareAction.BuyTheme, theme.Id);
            ImGui.EndDisabled();
            if (theme.Board is not null && !owned) CollectionText("Also unlocked by owning " + (CosmeticCollection.Item("board", theme.Board)?.Name ?? theme.Board) + ".");
        }
    }
}

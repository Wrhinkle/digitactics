using System.Text.Json.Nodes;
using Brutal.ImGuiApi;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool showFarm;
    private string? farmPet;
    private void FarmAct(FarmCareAction action, string? option = null, int pile = 0) => farm!.Act(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), action, farmPet, option, pile);
    private void FarmPanel()
    {
        Window("Digi-Farm", size.X * .12f, size.Y * .06f, size.X * .76f, size.Y * .88f);
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
}

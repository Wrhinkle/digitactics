using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private IncubatorOverview? incubator;
    private (string? Wallet, string? Legends, string? Farm) incubatorSave;
    private (bool Legend, string Id)? incubatorSelection;
    internal IncubatorOverview? DisplayedIncubator => incubator;
    internal string? IncubatorSelected => incubatorSelection?.Id;
    internal int IncubatorExclusiveCount { get; private set; }
    internal (float2 Min, float2 Max)? IncubatorInventoryBounds { get; private set; }

    private void IncubatorControls(CosmeticCollection collection)
    {
        var saved = (profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"), profile.Storage.GetValueOrDefault("dt.legendevo.v1"), profile.Storage.GetValueOrDefault("dt.farm.v1"));
        if (incubator is null || incubatorSave != saved)
        { incubator = IncubatorOverview.Read(saved.Item1, saved.Item2, saved.Item3); incubatorSave = saved; }
        if (incubatorSelection is not { } selected || (selected.Legend
            ? !incubator.Legends.Any(legend => legend.Item.Id == selected.Id)
            : !incubator.Eggs.Any(egg => egg.Definition.Id == selected.Id)))
        {
            var ready = incubator.Legends.FirstOrDefault(legend => legend.Evolution.CanDigivolve);
            incubatorSelection = ready is not null ? (true, ready.Item.Id) : (false, incubator.Eggs[0].Definition.Id);
        }
        ImGui.SeparatorText("Incubator"u8);
        CollectionText($"{incubator.DigiData:N0} Digi-Data / {incubator.Ready} ready to digivolve / {incubator.Legends.Length} Little Legends");
        CollectionText("Digi-Data is XP for each Little Legend. Earn it through matches, duplicate legend hatches and Digi-Farm care. Bits are your spending currency.");
        float width = ImGui.GetContentRegionAvail().X;
        ImGui.BeginChild("Incubator detail"u8, new(width * .55f, 0), ImGuiChildFlags.Borders);
        try
        {
            var selection = incubatorSelection.Value;
            if (selection.Legend) IncubatorLegendControls(collection, incubator.Legends.Single(legend => legend.Item.Id == selection.Id));
            else IncubatorEggControls(collection, incubator.Eggs.Single(egg => egg.Definition.Id == selection.Id));
        }
        finally { ImGui.EndChild(); }
        ImGui.SameLine();
        ImGui.BeginChild("Incubator inventory"u8, new(0, 0), ImGuiChildFlags.Borders);
        try
        {
            IncubatorInventoryBounds = (ImGui.GetWindowPos(), ImGui.GetWindowPos() + ImGui.GetWindowSize());
            MysteryLootControls(collection);
            ImGui.SeparatorText("Digi-Eggs"u8);
            foreach (var egg in incubator.Eggs)
            {
                if (Button("lootEgg/" + egg.Definition.Id, egg.Definition.Name, active: incubatorSelection == (false, egg.Definition.Id))) incubatorSelection = (false, egg.Definition.Id);
                CollectionText($"{egg.Owned} / {egg.Total} owned");
                ImGui.SameLine();
                ImGui.BeginDisabled(collection.Bits < egg.Definition.Price || egg.Total == 0);
                if (Button("egg/" + egg.Definition.Id, $"Hatch / {egg.Definition.Price} Bits")) BeginIncubatorHatch(egg.Definition.Id);
                ImGui.EndDisabled();
            }
            ImGui.SeparatorText("Little Legends"u8);
            foreach (var legend in incubator.Legends)
            {
                var evolution = legend.Evolution;
                string label = LegendEvolution.Form(evolution.Form).Name + $" / Lv {evolution.Level}" + (evolution.CanDigivolve ? " / READY" : "");
                if (Button("lootLegend/" + legend.Item.Id, label, active: incubatorSelection == (true, legend.Item.Id))) incubatorSelection = (true, legend.Item.Id);
            }
            if (incubator.Legends.Length == 0) CollectionText("Owned Little Legends appear here.");
        }
        finally { ImGui.EndChild(); }
    }

    private void BeginIncubatorHatch(string id)
    {
        incubatorSelection = (false, id);
        eggPurchase = (id, Random.Shared.NextDouble(), Random.Shared.NextDouble(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

    private void IncubatorEggControls(CosmeticCollection collection, IncubatorEgg egg)
    {
        ImGui.Text(egg.Definition.Name);
        EggArt(egg.Definition.Id, Math.Min(150, ImGui.GetContentRegionAvail().X));
        CollectionText($"{egg.Owned} / {egg.Total} items owned / {egg.Definition.Price} Bits");
        ImGui.BeginDisabled(collection.Bits < egg.Definition.Price || egg.Total == 0);
        if (Button("hatchSelectedEgg", $"Hatch / {egg.Definition.Price} Bits")) BeginIncubatorHatch(egg.Definition.Id);
        ImGui.EndDisabled();
        CollectionText("Hatches one random cosmetic. Rarity is rolled first; each item of that rarity is equally likely.");
        foreach (var odd in egg.Odds)
            CollectionText($"{odd.Rarity}: {odd.Pct:0.#}% / {odd.Count} items");
        CollectionText("Duplicate Little Legends grant Digi-Data until max level, then Bits. Other duplicates refund 40% of their item price.");
        IncubatorExclusiveCount = egg.Exclusives.Length;
        if (egg.Exclusives.Length == 0) return;
        ImGui.SeparatorText("Egg-only items"u8);
        foreach (var exclusive in egg.Exclusives)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, RarityColor(exclusive.Item.Rarity));
            CollectionText($"{exclusive.Item.Name} / {exclusive.Chance}" + (exclusive.Owned ? " / owned" : ""));
            ImGui.PopStyleColor();
        }
    }

    private void IncubatorLegendControls(CosmeticCollection collection, IncubatorLegend legend)
    {
        var evolution = legend.Evolution;
        var form = LegendEvolution.Form(evolution.Form);
        ImGui.Text(form.Name); ImGui.Text($"{form.Level} / Lv {evolution.Level}");
        CosmeticPreview("legend", legend.Item, collection, Math.Min(340, ImGui.GetContentRegionAvail().X));
        CollectionText(evolution.Need == 0 ? "Max level" : $"{evolution.Data} / {evolution.Need} Digi-Data");
        ImGui.ProgressBar(evolution.Need == 0 ? 1 : (float)evolution.Data / evolution.Need, new(-1, 16), ""u8);
        CollectionText(evolution.CanDigivolve ? "Ready to digivolve. Open Digivolve to discover its next form."
            : evolution.Next is { } next ? $"Digivolves at level {next}. Gain Digi-Data from games played with it equipped, duplicate hatches and Digi-Farm care." : "Final form reached.");
        if (Button("lootDigivolve", "Digivolve")) { ClearEvolutionPreview(); evolutionLegend = legend.Item.Id; collectionMessage = null; }
        ImGui.SameLine();
        if (Button("lootFarm", "Digi-Farm"))OpenCollectionFarm(collection,legend.Item.Id);
        if (collection.Equipped("legend")?.GetValue<string>() == legend.Item.Id) ImGui.Text("Equipped"u8);
        else if (Button("lootEquip", "Equip") && ChangeCollection(next => next.Equip("legend", legend.Item.Id), "Equipped " + legend.Item.Name + ".")) audio.PlayClient("equip", false);
    }
    private void OpenCollectionFarm(CosmeticCollection collection,string legend)
    {
        farm!.Refresh(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        if (farm.Save is null || farm.Error is not null) { collectionMessage = farm.Error ?? "Digi-Farm could not be opened."; return; }
        var residents = System.Text.Json.Nodes.JsonNode.Parse(farm.Save)!["residents"]!.AsArray().Select(id => id!.GetValue<string>()).ToArray();
        string? equipped = collection.Equipped("legend")?.GetValue<string>();
        farmPet = residents.Contains(legend) ? legend : residents.Contains(equipped) ? equipped : residents.FirstOrDefault();
        farmManagement = false; showCollection = false; showFarm = true;
    }
}

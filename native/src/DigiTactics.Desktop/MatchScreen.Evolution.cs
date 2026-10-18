using Brutal.ImGuiApi;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private void Evolve(string id, LegendAction action)
    {
        var result = LegendEvolution.Apply(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"), profile.Storage.GetValueOrDefault("dt.cosmetics.v1"), profile.Storage.GetValueOrDefault("dt.farm.v1"), id, action);
        if (!result.Changed) { collectionMessage = "That action is unavailable. Check your Bits, level and care."; return; }
        var changes = new Dictionary<string, string> { ["dt.legendevo.v1"] = result.Legends! };
        if (result.Wallet is not null && result.Wallet != profile.Storage.GetValueOrDefault("dt.cosmetics.v1")) changes["dt.cosmetics.v1"] = result.Wallet;
        if (!profile.UpdateStorage(changes)) { collectionMessage = profile.Error; return; }
        collectionMessage = action switch {
            LegendAction.Digivolve => "Digivolved into " + LegendEvolution.Form(result.Result!["to"]!.GetValue<string>()).Name + "!",
            LegendAction.Revert => "Reverted to " + LegendEvolution.Form(result.Result!["to"]!.GetValue<string>()).Name + ".",
            LegendAction.Feed => "Fed your legend. Food care increased.",
            _ => "Your legend gained a level."
        };
    }
    private bool showEvolutionTree;
    internal int EvolutionTreeNodes { get; private set; }
    private void EvolutionControls(string id)
    {
        EvolutionTreeNodes=0;
        if (Button("closeEvolution", "Back to collection")) { evolutionLegend = null; showEvolutionTree=false; return; }
        var view = LegendEvolution.Inspect(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"), profile.Storage.GetValueOrDefault("dt.cosmetics.v1"), profile.Storage.GetValueOrDefault("dt.farm.v1"), id);
        if(showEvolutionTree)
        {
            if(Button("closeEvolutionTree","Back to care")) showEvolutionTree=false;
            EvolutionTreePanel(id,view); return;
        }
        var form = LegendEvolution.Form(view.Form);
        ImGui.Text(CosmeticCollection.Item("legend", id)!.Name); ImGui.Text($"Current form: {form.Name} ({form.Level})");
        ImGui.Text($"Level {view.Level} / 10");
        ImGui.Text(view.Need > 0 ? $"Digi-Data: {view.Data} / {view.Need}" : "Maximum level");
        if (view.Next is { } next) ImGui.Text($"Next digivolution unlocks at level {next}");
        foreach (var branch in view.Odds) CollectionText($"{LegendEvolution.Form(branch.Form).Name}: {branch.Pct}% ({branch.Care} care)");
        if (view.Odds.Length == 0) ImGui.Text("Final form"u8);
        ImGui.BeginDisabled(!view.CanDigivolve);
        if (Button("digivolve", "Digivolve")) Evolve(id, LegendAction.Digivolve);
        ImGui.EndDisabled();
        ImGui.BeginDisabled(!view.CanRevert || profile.Progress.Bits < 50);
        if (Button("revertLegend", "Revert one form (50 Bits)")) Evolve(id, LegendAction.Revert);
        ImGui.EndDisabled();
        ImGui.BeginDisabled(view.Need == 0 || profile.Progress.Bits < view.LevelBits);
        if (Button("levelLegend", $"Buy next level ({view.LevelBits} Bits)")) Evolve(id, LegendAction.BuyLevel);
        ImGui.EndDisabled();
        ImGui.BeginDisabled(profile.Progress.Bits < 30);
        if (Button("feedLegend", "Feed (30 Bits)")) Evolve(id, LegendAction.Feed);
        ImGui.EndDisabled();
        if(Button("openEvolutionTree","View evolution tree")) showEvolutionTree=true;
        CollectionText("Bond, battle and food care influence your next form. Digi-Farm care also contributes. Reverting changes the next roll; reloading does not.");
    }
}

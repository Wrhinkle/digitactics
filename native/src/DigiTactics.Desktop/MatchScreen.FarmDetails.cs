using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool evolutionReturnToFarm;
    internal FarmResidentDetails? DisplayedFarmDetails {get;private set;}
    internal int FarmEvolutionBranches {get;private set;}
    private FarmResidentDetails FarmDetails(string id)=>FarmResidentDetails.Read(farm!.Save!,profile!.Storage.GetValueOrDefault("dt.legendevo.v1"),
        profile.Storage.GetValueOrDefault("dt.cosmetics.v1"),id,DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    private void OpenFarmEvolution(string id)
    {
        ClearEvolutionPreview();evolutionLegend=id;evolutionReturnToFarm=true;collectionMessage=null;
        showFarm=false;showCollection=true;
    }
    private void CloseEvolution()
    {
        evolutionLegend=null;ClearEvolutionPreview();
        if(evolutionReturnToFarm){showCollection=false;showFarm=true;farmManagement=false;}
        evolutionReturnToFarm=false;
    }
    private void FarmEvolutionDetails(string id,FarmResidentDetails details)
    {
        FarmEvolutionBranches=0;
        ImGui.SeparatorText("Next digivolution"u8);
        var evolution=details.Evolution;
        UiText.Wrapped(evolution.CanDigivolve?"Ready to digivolve!":evolution.Next is {} next?$"Next form at level {next} / current level {evolution.Level}":"Final form. Revert in Digivolve to explore other branches.");
        var known=LegendEvolution.Discovery(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"),id).Dex.ToHashSet();
        foreach(var branch in evolution.Odds)
        {
            var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(32,32));portraits.DrawEvolution(branch.Form,at,32,known.Contains(branch.Form));
            ImGui.SameLine();ImGui.BeginGroup();
            ImGui.Text(known.Contains(branch.Form)?LegendEvolution.Form(branch.Form).Name:"???");
            ImGui.Text($"{branch.Care} / {branch.Pct}%");ImGui.EndGroup();FarmEvolutionBranches++;
        }
        ImGui.SeparatorText("Farm care pushes branches"u8);
        var bonus=details.Care.Bonus;
        UiText.Wrapped($"Bond +{bonus["bond"]:0}: praise, play and petting");
        UiText.Wrapped($"Battle +{bonus["battle"]:0}: training stats (every 25 points)");
        UiText.Wrapped($"Food +{bonus["food"]:0}: meals when hungry; carrots count more");
        UiText.Wrapped($"Wild +{bonus["wild"]:0}: {details.Mistakes} mistakes x3, {details.PoopNeglect} neglected piles x2"+
            (details.Fat?", chubby +6":"")+(details.Sick?", sick +3":"")+(details.Overfeed>=5?", frequent overfeeding +2":""));
    }
}

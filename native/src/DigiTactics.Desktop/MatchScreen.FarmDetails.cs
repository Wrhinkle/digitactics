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
        if(EvolutionBusy)return;
        evolutionLegend=null;ClearEvolutionPreview();
        if(evolutionReturnToFarm){showCollection=false;showFarm=true;farmManagement=false;}
        if(evolutionReturnToHome)showCollection=false;
        evolutionReturnToHome=false;
        evolutionReturnToFarm=false;
    }
}

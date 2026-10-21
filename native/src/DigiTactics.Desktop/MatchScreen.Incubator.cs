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
    private float incubatorInformationHeight;
    internal IncubatorOverview? DisplayedIncubator => incubator;
    internal string? IncubatorSelected => incubatorSelection?.Id;
    internal int IncubatorExclusiveCount { get; private set; }
    internal (float2 Min, float2 Max)? IncubatorInventoryBounds { get; private set; }
    internal (float2 Min, float2 Max)? IncubatorInformationBounds { get; private set; }
    internal (float2 Min, float2 Max)? IncubatorLastContentBounds { get; private set; }
    internal bool IncubatorContentsExpanded { get; private set; }
    internal int IncubatorContentRows { get; private set; }
    internal float IncubatorInformationScroll { get; private set; }
    internal float IncubatorInformationScrollMax { get; private set; }

    private void BeginIncubatorHatch(string id)
    {
        incubatorSelection = (false, id);
        eggPurchase = (id, Random.Shared.NextDouble(), Random.Shared.NextDouble(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

    private void OpenCollectionFarm(CosmeticCollection collection,string legend)
    {
        farm!.Refresh(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        if (farm.Save is null || farm.Error is not null) { collectionMessage = farm.Error ?? "Digi-Farm could not be opened."; return; }
        var residents = System.Text.Json.Nodes.JsonNode.Parse(farm.Save)!["residents"]!.AsArray().Select(id => id!.GetValue<string>()).ToArray();
        string? equipped = collection.Equipped("legend")?.GetValue<string>();
        farmPet = residents.Contains(legend) ? legend : residents.Contains(equipped) ? equipped : residents.FirstOrDefault();
        if(ClientHomeVisible)RememberClientRoute("farm");
        farmManagement = false; showCollection = false; showFarm = true;
    }
}

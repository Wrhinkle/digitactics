using Brutal.ImGuiApi;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private void Evolve(string id, LegendAction action)
    {
        if(evolutionPreview?.Evolving==true)return;
        var previous=action==LegendAction.Digivolve?LegendEvolution.Inspect(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"),profile.Storage.GetValueOrDefault("dt.cosmetics.v1"),profile.Storage.GetValueOrDefault("dt.farm.v1"),id):null;
        var previousDiscovery=action==LegendAction.Digivolve?LegendEvolution.Discovery(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"),id):null;
        var result = LegendEvolution.Apply(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"), profile.Storage.GetValueOrDefault("dt.cosmetics.v1"), profile.Storage.GetValueOrDefault("dt.farm.v1"), id, action);
        if (!result.Changed) { collectionMessage = "That action is unavailable. Check your Bits, level and care."; return; }
        var changes = new Dictionary<string, string> { ["dt.legendevo.v1"] = result.Legends! };
        if (result.Wallet is not null && result.Wallet != profile.Storage.GetValueOrDefault("dt.cosmetics.v1")) changes["dt.cosmetics.v1"] = result.Wallet;
        if (!profile.UpdateStorage(changes)) { collectionMessage = profile.Error; return; }
        collectionMessage=null;
        if(evolutionPreview is not null)
        {
            var current=LegendEvolution.Discovery(profile.Storage.GetValueOrDefault("dt.legendevo.v1"),id).Current;
            if(action==LegendAction.Digivolve) { evolutionSaid.Hide(boardSceneTime); evolutionBeforeReveal=previous; evolutionDiscoveryBeforeReveal=previousDiscovery; evolutionAnnouncement=$"{LegendEvolution.Form(previous!.Form).Name} digivolved into {LegendEvolution.Form(current).Name}!"; evolutionPreview.TransformTo(LegendEvolution.VisualKey(id,current)); audio.Play("reroll"); }
            else evolutionPreview.Select(LegendEvolution.VisualKey(id,current));
            if(action==LegendAction.Feed)evolutionPreview.Play("eat");

        }
        if(action==LegendAction.Revert) { audio.Play("sell"); evolutionSaid.Show("Back to " + LegendEvolution.Form(result.Result!["to"]!.GetValue<string>()).Name + ".",boardSceneTime,DigiTactics.Presentation.EvolutionAnnouncement.ActionSeconds); }
        if(action==LegendAction.BuyLevel) { audio.Play("levelup"); evolutionSaid.Show($"Level {result.Result!["level"]!.GetValue<int>()}!",boardSceneTime,DigiTactics.Presentation.EvolutionAnnouncement.ActionSeconds); }
        if(action==LegendAction.Feed) audio.Play("pick");
    }
    private EvolutionView? evolutionBeforeReveal;
    private EvolutionDiscovery? evolutionDiscoveryBeforeReveal;
    private EvolutionDiscovery DisplayedEvolutionDiscovery(string id)=>evolutionDiscoveryBeforeReveal??LegendEvolution.Discovery(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"),id);
    private string? evolutionAnnouncement;
    internal string? DisplayedEvolutionForm { get; private set; }
    internal bool EvolutionAnnouncementVisible=>evolutionSaid.Sample(boardSceneTime).Active;
    private void RevealEvolution()
    {
        evolutionBeforeReveal=null;
        evolutionDiscoveryBeforeReveal=null;
        if(evolutionAnnouncement is { } announcement)evolutionSaid.Show(announcement,boardSceneTime);
        evolutionAnnouncement=null;
    }
    private void ClearEvolutionPreview()
    {
        if(evolutionSaid.Text is not null || evolutionBeforeReveal is not null)collectionMessage=null;
        evolutionPreview?.Dispose();evolutionPreview=null;
        evolutionAnnouncementArtwork?.Dispose();evolutionAnnouncementArtwork=null;
        evolutionBeforeReveal=null;evolutionDiscoveryBeforeReveal=null;evolutionAnnouncement=null;evolutionSaid.Clear();evolutionSaidFrame=default;EvolutionAnnouncementLines=0;DisplayedEvolutionForm=null;showEvolutionTree=false;
    }
    private CompanionPreview? evolutionPreview;
    private bool showEvolutionTree;
    internal double EvolutionRaysOpacity=>evolutionPreview?.RaysOpacity??0;
    internal bool EvolutionFloorDrawn=>evolutionPreview?.FloorDrawn==true;
    internal double EvolutionTransitionTime=>evolutionPreview?.TransitionTime??0;
    internal bool EvolutionBusy=>evolutionPreview?.Evolving==true;
    internal int EvolutionPreviewDrawCalls=>evolutionPreview?.DrawCalls??0;
    internal double EvolutionPreviewTime=>evolutionPreview?.AnimationTime??0;
    internal int EvolutionTreeNodes { get; private set; }
    internal float EvolutionTreeScroll { get; private set; }
    internal float EvolutionTreeScrollMax { get; private set; }
    internal int EvolutionTreePortraits { get; private set; }
}

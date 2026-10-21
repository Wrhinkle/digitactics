using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private bool evolutionReturnToHome;
    internal string? EvolutionSelectedLegend=>evolutionLegend;
    internal string[] EvolutionSelectableLegends {get;private set;}=[];
    internal (float2 Min,float2 Max) EvolutionSelectorBounds {get;private set;}
    private (string? Wallet,string? Legends,string? Farm) evolutionNavigationSave;
    private (CosmeticItem Item,EvolutionView View)[] evolutionNavigationEntries=[];
    private string? evolutionNavigationEquipped;
    private bool EvolutionLegendSelector(string selected,float width=0,float height=76)
    {
        var save=(Wallet:profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"),Legends:profile.Storage.GetValueOrDefault("dt.legendevo.v1"),Farm:profile.Storage.GetValueOrDefault("dt.farm.v1"));
        // Freeze the original chips during the transformation, so the new form
        // is not revealed ahead of the main scene's flash.
        if(evolutionNavigationEntries.Length==0||!EvolutionBusy&&evolutionNavigationSave!=save)
        {
            var collection=new CosmeticCollection(save.Wallet);
            evolutionNavigationEntries=CosmeticCollection.Items("legend").Where(item=>collection.Owns("legend",item.Id))
                .Select(item=>(item,LegendEvolution.Inspect(save.Legends,save.Wallet,save.Farm,item.Id))).ToArray();
            evolutionNavigationEquipped=collection.Equipped("legend")?.GetValue<string>();evolutionNavigationSave=save;
        }
        EvolutionSelectableLegends=evolutionNavigationEntries.Select(entry=>entry.Item.Id).ToArray();
        bool changed=false;
        ImGui.BeginChild("Owned evolution legends"u8,new(width,height),ImGuiChildFlags.None,ImGuiWindowFlags.HorizontalScrollbar);
        EvolutionSelectorBounds=(ImGui.GetWindowPos(),ImGui.GetWindowPos()+ImGui.GetWindowSize());
        ImGui.BeginDisabled(EvolutionBusy);
        for(int i=0;i<evolutionNavigationEntries.Length;i++)
        {
            if(i>0)ImGui.SameLine(0,8);
            var (item,view)=evolutionNavigationEntries[i];
            var at=ImGui.GetCursorScreenPos();
            bool clicked=Button("evolutionLegend/"+item.Id,"##evolutionLegend"+item.Id,58,58,item.Id==selected);
            portraits.DrawEvolution(view.Form,at+new float2(5,2),46,true);
            ImGui.GetWindowDrawList().AddText(at+new float2(5,39),view.CanDigivolve?HomeGold:HomeInk,"Lv "+view.Level);
            if(ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))ResultTooltip($"{item.Name}{(evolutionNavigationEquipped==item.Id?" / Equipped":"")}\n{LegendEvolution.Form(view.Form).Name} / Level {view.Level}"+(view.CanDigivolve?" / Ready to digivolve":""));
            if(clicked&&item.Id!=selected&&!EvolutionBusy){ClearEvolutionPreview();evolutionLegend=item.Id;collectionMessage=null;changed=true;}
        }
        ImGui.EndDisabled();ImGui.EndChild();return changed;
    }
    private void EvolutionEquip(string id)
    {
        var collection=new CosmeticCollection(profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"));
        bool equipped=collection.Equipped("legend")?.GetValue<string>()==id;
        if(equipped){ImGui.Text("Equipped"u8);return;}
        ImGui.BeginDisabled(EvolutionBusy||equipped||!collection.Owns("legend",id));
        if(Button("equipEvolutionLegend",equipped?"Equipped":"Equip this Little Legend",-1,38)&&ChangeCollection(c=>c.Equip("legend",id),"Equipped "+CosmeticCollection.Item("legend",id)!.Name+"."))audio.PlayClient("equip",false);
        ImGui.EndDisabled();
    }
}

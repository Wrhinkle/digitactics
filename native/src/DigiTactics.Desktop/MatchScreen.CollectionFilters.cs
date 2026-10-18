using Brutal.ImGuiApi;
using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private readonly ImInputString collectionSearch=new(256);
    private string collectionOwnership="all",collectionRarity="all",collectionSort="rarity";
    private bool collectionSearchFocused;
    private readonly Dictionary<string,string> collectionSelections=[];
    private bool collectionOwnedOnly { get=>collectionOwnership=="owned"; set=>collectionOwnership=value?"owned":"all"; }
    private bool CollectionSearchEscape=>collectionSearchFocused&&collectionSearch.ToString().Length>0;
    internal string[] CollectionFilterIds {get;private set;}=[];
    internal string CollectionFilterQuery=>collectionSearch.ToString();
    internal string CollectionFilterOwnership=>collectionOwnership;
    internal string CollectionFilterRarity=>collectionRarity;
    internal string CollectionFilterSort=>collectionSort;
    internal string CollectionFilterKind=>CosmeticCollection.Kinds[collectionKind];
    internal string? CollectionSelectedId=>previewCosmetic;
    internal bool CollectionVisible=>showCollection;
    internal (Brutal.Numerics.float2 Min,Brutal.Numerics.float2 Max) CollectionListBounds {get;private set;}
    private void CollectionCategoryChanged(int next)
    {
        if(previewCosmetic is not null)collectionSelections[CosmeticCollection.Kinds[collectionKind]]=previewCosmetic;
        collectionKind=next;collectionSlot=0;collectionSearch.SetValue("");collectionResetScroll=true;
        previewCosmetic=collectionSelections.GetValueOrDefault(CosmeticCollection.Kinds[next]);
    }
    private CosmeticItem[] CollectionFilters(CosmeticCollection collection,string kind)
    {
        if(kind=="emote"&&Button("collectionRetro","Digimon World 1"))
        {collectionSearch.SetValue("Digimon World 1");collectionOwnership=collectionRarity="all";previewCosmetic="retro_hug";}
        float available=ImGui.GetContentRegionAvail().X;
        ImGui.SetNextItemWidth(Math.Max(160,available-80));
        ImGui.InputTextWithHint("##collectionSearch"u8,"Search name, description, rarity or tag"u8,collectionSearch,ImGuiInputTextFlags.EscapeClearsAll);
        collectionSearch.EvaluateLength();collectionSearchFocused=ImGui.IsItemActive();CollectionFilterAnchor("collectionSearch");
        ImGui.SameLine();if(Button("clearCollectionSearch","Clear",72,36))collectionSearch.SetValue("");
        int optionIndex=0;
        foreach(var option in new[]{(Id:"all",Label:"All",Width:56),(Id:"owned",Label:"Owned",Width:80),(Id:"locked",Label:"Not owned",Width:112)})
        {
            if(optionIndex++>0)ImGui.SameLine();if(Button("collectionOwnership/"+option.Id,option.Label,option.Width,36,active:collectionOwnership==option.Id))collectionOwnership=option.Id;
        }
        ImGui.SameLine();CollectionFilterCombo("collectionRarity","Rarity",ref collectionRarity,new[]{("all","All rarities")}.Concat(CosmeticBrowser.Rarities.Select(value=>(value,char.ToUpperInvariant(value[0])+value[1..]))).ToArray(),148);
        ImGui.SameLine();CollectionFilterCombo("collectionSort","Sort",ref collectionSort,[("rarity","Rarity: high first"),("rarity-asc","Rarity: low first"),("name","Name"),("owned","Owned first")],Math.Max(170,available-428));
        var items=CosmeticBrowser.Items(collection,kind,collectionSearch.ToString(),collectionOwnership,collectionRarity,collectionSort);
        CollectionFilterIds=items.Select(item=>item.Id).ToArray();

        return items;
    }
    private void CollectionFilterCombo(string key,string label,ref string value,(string Id,string Label)[] options,float width)
    {
        string selected=value;ImGui.SetNextItemWidth(width);bool open=ImGui.BeginCombo("##"+key,options.Single(option=>option.Id==selected).Label);CollectionFilterAnchor(key);
        if(ImGui.IsItemHovered())ImGui.SetTooltip(label);
        if(!open)return;
        foreach(var option in options)
        {
            bool chosen=ImGui.Selectable(option.Label,option.Id==value);CollectionFilterAnchor(key+"/"+option.Id);ClientItemSound(key+"/"+option.Id,chosen);
            if(chosen)value=option.Id;
        }
        ImGui.EndCombo();
    }
    private void CollectionFilterAnchor(string key)=>Buttons[key]=(ImGui.GetItemRectMin()+ImGui.GetItemRectMax())*.5f;
}

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
        collectionKind=next;collectionSlot=0;collectionSearch.SetValue("");collectionResetScroll=true;collectionStoreEggs=false;
        previewCosmetic=collectionSelections.GetValueOrDefault(CosmeticCollection.Kinds[next]);
    }
    private CosmeticItem[] CollectionFilters(CosmeticCollection collection,string kind)=>CollectionPageFilters(collection,kind);
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

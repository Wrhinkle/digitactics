using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private readonly PostgameArtwork collectionArtwork=new("collection");
    private static readonly Lazy<Dictionary<string,string>> CollectionArt=new(()=>JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"content/collection/manifest.json")))!);
    private static readonly string[] CollectionTabs=["Little Legends","Emotes","Boards","Kill Screens","Dances","Soundboard","Elim Sounds"];
    internal (float2 Min,float2 Max) CollectionDetailBounds {get;private set;}
    internal bool CollectionLootVisible=>showCollection&&showEggs;
    internal bool CollectionStoreVisible=>showCollection&&collectionStore&&!showEggs;
    private bool collectionResetScroll;
    private void OpenCollectionPage(bool store=false,bool loot=false,string? kind=null,string? item=null)
    {
        showCollection=true;showEggs=loot;collectionStore=store;collectionResetScroll=true;
        purchase=null;eggPurchase=null;hatching=null;boughtCosmetic=null;mysteryReward=null;collectionMessage=null;
        if(evolutionLegend is not null)CloseEvolution();
        if(kind is not null && Array.IndexOf(CosmeticCollection.Kinds,kind) is >=0 and var index)CollectionCategoryChanged(index);
        if(item is not null)previewCosmetic=item;
    }
    private void CollectionBrowser(CosmeticCollection collection)
    {
        string kind=CosmeticCollection.Kinds[collectionKind];
        var visible=CosmeticCollection.Items(kind).Where(item=>!item.Secret||collection.Owns(kind,item.Id)).ToArray();
        int owned=visible.Count(item=>collection.Owns(kind,item.Id));
        float fullWidth=ImGui.GetContentRegionAvail().X;
        var at=ImGui.GetCursorScreenPos();float bannerHeight=78;
        string art=collectionStore?"store-hero":kind switch{"legend"=>"coll-legends","board"=>"coll-boards","emote" or "sound"=>"coll-emotes",_=>"store-fx"};
        collectionArtwork.Draw(art,at,new(fullWidth,bannerHeight),255,true,.45f);
        var draw=ImGui.GetWindowDrawList();
        draw.AddRectFilledMultiColor(at,at+new float2(fullWidth,bannerHeight),new ImColor8(1,10,19,235),new ImColor8(1,10,19,95),new ImColor8(1,10,19,160),new ImColor8(1,10,19,235));
        draw.AddRect(at,at+new float2(fullWidth,bannerHeight),HomeLine);
        ImGui.SetCursorScreenPos(at+new float2(16,9));HomeLabel(collectionStore?"STORE":MatchLockerVisible?"LOCKER":"COLLECTION",26,HomeInk);
        HomeLabel($"{CollectionNames[collectionKind]}  /  {owned} of {visible.Length} owned",17,HomeGold);
        ImGui.SetCursorScreenPos(at+new float2(fullWidth-296,20));
        ImGui.SetNextItemWidth(184);bool open=ImGui.BeginCombo("##collectionCategory"u8,CollectionNames[collectionKind]);CollectionFilterAnchor("collectionCategory");
        if(open)
        {
            for(int i=0;i<CollectionNames.Length;i++)
            {
                bool chosen=ImGui.Selectable(CollectionNames[i],collectionKind==i);CollectionFilterAnchor("collectionCategory/"+i);ClientItemSound("collectionCategory/"+i,chosen);
                if(chosen)CollectionCategoryChanged(i);
            }
            ImGui.EndCombo();
        }
        if(!MatchLockerVisible){ImGui.SameLine();if(Button("toggleEggs","Loot",88,36))showEggs=true;}
        ImGui.SetCursorScreenPos(at+new float2(0,bannerHeight+10));
        for(int i=0;i<CollectionTabs.Length;i++)
        {
            if(i>0)ImGui.SameLine(0,6);
            if(Button("collectionTab/"+i,CollectionTabs[i],(fullWidth-36)/7,38,collectionKind==i))CollectionCategoryChanged(i);
        }
        kind=CosmeticCollection.Kinds[collectionKind];
        float right=Math.Clamp(fullWidth*.3f,292,360),left=fullWidth-right-12;
        ImGui.BeginChild("collectionBrowser"u8,new(left,0),ImGuiChildFlags.None,ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        try
        {
            var filtered=collectionStore?StoreItems(collection,kind):CollectionFilters(collection,kind);
            ImGui.BeginChild("collectionItems"u8,new(0,0),ImGuiChildFlags.None);
            if(collectionResetScroll){ImGui.SetScrollY(0);collectionResetScroll=false;}
            CollectionListBounds=(ImGui.GetWindowPos(),ImGui.GetWindowPos()+ImGui.GetWindowSize());
            try
            {
                if(collectionStore)CollectionFeatured(collection);
                if(filtered.Length==0)CollectionText("Nothing matches these filters.");
                CollectionCardGrid(collection,kind,filtered);
            }
            finally {ImGui.EndChild();}
        }
        finally {ImGui.EndChild();}
        ImGui.SameLine(0,12);
        ImGui.PushStyleColor(ImGuiCol.ChildBg,new ImColor8(5,15,25));ImGui.PushStyleColor(ImGuiCol.Border,HomeLine);
        ImGui.BeginChild("collectionPreview"u8,new(0,0),ImGuiChildFlags.Borders);
        CollectionDetailBounds=(ImGui.GetWindowPos(),ImGui.GetWindowPos()+ImGui.GetWindowSize());
        try {SelectedCosmetic(collection,kind);}
        finally {ImGui.EndChild();ImGui.PopStyleColor(2);}
    }
    private CosmeticItem[] StoreItems(CosmeticCollection collection,string kind)
    {
        var items=CosmeticCollection.Items(kind).Where(item=>!item.Secret&&item.Linked is null)
            .OrderBy(item=>item.Free?0:1).ThenBy(item=>Array.IndexOf(DigiTactics.Presentation.CosmeticBrowser.Rarities,item.Rarity??"common")).ThenBy(item=>item.Price).ToArray();
        CollectionFilterIds=items.Select(item=>item.Id).ToArray();
        if(kind=="elimsound")CollectionText("Owning a Soundboard clip also unlocks its linked elimination sound.");
        return items;
    }
    private void CollectionFeatured(CosmeticCollection collection)
    {
        HomeLabel("FEATURED TODAY",22,HomeInk);HomeLabel("25% off / refreshed daily",17,HomeGold);
        var offers=CosmeticCollection.Featured(DateOnly.FromDateTime(DateTime.Now));
        float width=(ImGui.GetContentRegionAvail().X-24)/4;
        for(int i=0;i<offers.Length;i++)
        {
            if(i>0)ImGui.SameLine(0,8);
            var offer=offers[i];var at=ImGui.GetCursorScreenPos();var extent=new float2(width,142);
            string key="collectionOffer/"+offer.Kind+"/"+offer.Item.Id;
            ImGui.InvisibleButton(key,extent);CollectionFilterAnchor(key);
            if(ImGui.IsItemClicked()){CollectionCategoryChanged(Array.IndexOf(CosmeticCollection.Kinds,offer.Kind));previewCosmetic=offer.Item.Id;ClientItemSound(key,true);}
            bool hovered=ImGui.IsItemHovered();var draw=ImGui.GetWindowDrawList();
            draw.AddRectFilled(at,at+extent,new ImColor8(11,25,41));
            CollectionThumbnail(collection,offer.Kind,offer.Item,at,new(width,82));
            draw.AddLine(at+new float2(0,82),at+new float2(width,82),RarityColor(offer.Item.Rarity),2);
            draw.PushClipRect(at+new float2(8,85),at+extent-new float2(8,4),true);
            draw.AddText(at+new float2(8,91),HomeInk,offer.Item.Name);
            draw.AddText(at+new float2(8,116),HomeGold,CosmeticCollection.Price(offer.Kind,offer.Item,DateOnly.FromDateTime(DateTime.Now))+" Bits");
            draw.PopClipRect();draw.AddRect(at,at+extent,hovered?HomeGold:HomeLine);
            if(hovered)ImGui.SetTooltip(offer.Item.Name+" / "+CollectionNames[Array.IndexOf(CosmeticCollection.Kinds,offer.Kind)]);
        }
        ImGui.Spacing();HomeLabel("BROWSE "+CollectionNames[collectionKind].ToUpperInvariant(),22,HomeInk);
    }
    private void CollectionThumbnail(CosmeticCollection collection,string kind,CosmeticItem item,float2 at,float2 extent)
    {
        if(!ImGui.IsRectVisible(at,at+extent))return;
        float square=extent.Y;
        if(kind=="legend")portraits.DrawCompanion(CurrentPreviewLegend(collection,item.Id),at+new float2((extent.X-square)/2,0),square);
        else if(kind=="emote")emoteArtwork.Draw(item.Id,(long)(boardSceneTime*1000)%2600,at+new float2((extent.X-square+10)/2,5),square-10);
        else if(kind=="killscreen")collectionTheme.Draw(item.Id,false,boardSceneTime%3.5,at,extent);
        else if(CollectionArt.Value.TryGetValue(kind+"/"+item.Id,out string? file))collectionArtwork.Draw(file,at,extent,255,kind=="board");
    }
    private void CollectionCardGrid(CosmeticCollection collection,string kind,CosmeticItem[] items)
    {
        float available=ImGui.GetContentRegionAvail().X;int columns=Math.Max(2,(int)((available+10)/162));
        float width=(available-(columns-1)*10)/columns,height=184;
        for(int i=0;i<items.Length;i++)
        {
            if(i%columns!=0)ImGui.SameLine(0,10);
            var item=items[i];var at=ImGui.GetCursorScreenPos();var end=at+new float2(width,height);
            bool selected=previewCosmetic==item.Id,owned=collection.Owns(kind,item.Id);
            ImGui.InvisibleButton("collectionCard/"+kind+"/"+item.Id,new(width,height));
            CollectionFilterAnchor("preview/"+kind+"/"+item.Id);
            if(ImGui.IsItemClicked()){previewCosmetic=item.Id;ClientItemSound("preview/"+kind+"/"+item.Id,true);}
            bool hovered=ImGui.IsItemHovered();var draw=ImGui.GetWindowDrawList();
            draw.AddRectFilledMultiColor(at,end,new ImColor8(15,29,46),new ImColor8(15,29,46),new ImColor8(1,10,19),new ImColor8(1,10,19));
            var pictureEnd=at+new float2(width,108);draw.AddRectFilled(at,pictureEnd,new ImColor8(11,23,37));
            CollectionThumbnail(collection,kind,item,at,new(width,108));
            if(!owned)draw.AddRectFilled(at,pictureEnd,new ImColor8(1,10,19,85));
            draw.AddLine(pictureEnd-new float2(width,0),pictureEnd,RarityColor(item.Rarity),2);
            draw.PushClipRect(at+new float2(7,111),end-new float2(7,5),true);
            draw.AddText(at+new float2(8,116),HomeInk,item.Name);
            draw.AddText(at+new float2(8,141),RarityColor(item.Rarity),char.ToUpperInvariant((item.Rarity??"common")[0])+(item.Rarity??"common")[1..]);
            draw.AddText(at+new float2(8,163),HomeMute,owned?CollectionWorn(collection,kind,item.Id)?"Equipped":"Owned":item.EggOnly?"Digi-Egg exclusive":CosmeticCollection.Price(kind,item,DateOnly.FromDateTime(DateTime.Now))+" Bits");
            draw.PopClipRect();
            draw.AddRect(at,end,selected?HomeGold:hovered?new ImColor8(162,150,116):HomeLine,thickness:selected?2:1);
            if(hovered)ImGui.SetTooltip(item.Name+(item.Tag is not null?" / "+item.Tag:""));
        }
    }
    private bool CollectionWorn(CosmeticCollection collection,string kind,string id)=>CosmeticCollection.Multi(kind)
        ?collection.Equipped(kind)!.AsArray().ElementAtOrDefault(collectionSlot)?.GetValue<string>()==id:collection.Equipped(kind)?.GetValue<string>()==id;
    private void CollectionActions(CosmeticCollection collection,string kind,CosmeticItem item)
    {
        if(CosmeticCollection.Multi(kind))
        {
            HomeLabel("EQUIP TO YOUR WHEEL",17,HomeGold);
            var equipped=collection.Equipped(kind)!.AsArray();float slotWidth=(ImGui.GetContentRegionAvail().X-16)/3;
            for(int i=0;i<6;i++)
            {
                if(i%3!=0)ImGui.SameLine(0,8);
                if(Button("collectionSlot/"+i,"Slot "+(i+1),slotWidth,36,collectionSlot==i))collectionSlot=i;
                if(ImGui.IsItemHovered())ImGui.SetTooltip(CosmeticCollection.Item(kind,equipped.ElementAtOrDefault(i)?.GetValue<string>())?.Name??"Empty");
            }
            if(Button("clearCollectionSlot","Empty selected slot",-1,36))ChangeCollection(c=>c.Equip(kind,null,collectionSlot),"Slot cleared.");
        }
        if(collection.Owns(kind,item.Id))
        {
            bool worn=CollectionWorn(collection,kind,item.Id);ImGui.BeginDisabled(worn);
            if(Button("equip/"+kind+"/"+item.Id,worn?"Equipped":CosmeticCollection.Multi(kind)?$"Put in slot {collectionSlot+1}":"Equip",-1,40))
                if(ChangeCollection(c=>c.Equip(kind,item.Id,collectionSlot),"Equipped "+item.Name+"."))audio.PlayClient("equip",false);
            ImGui.EndDisabled();
            if(kind=="legend"&&Button("evolution/"+item.Id,"Digivolve / care",-1,40)){evolutionLegend=item.Id;collectionMessage=null;}
            if(kind=="legend"&&Button("collectionFarm/"+item.Id,"Digi-Farm",-1,40))OpenCollectionFarm(collection,item.Id);
        }
        else if(item.Linked is {Length:2} link)
        {
            var source=CosmeticCollection.Item(link[0],link[1]);
            CollectionText("Unlocked by owning "+(source?.Name??link[1]));
            int price=source is null?0:CosmeticCollection.Price(link[0],source,DateOnly.FromDateTime(DateTime.Now));
            if(Button("linkedCosmeticStore","Get the soundboard clip / "+price+" Bits",-1,40))OpenCollectionPage(store:true,kind:link[0],item:link[1]);
        }
        else if(item.EggOnly)
        {
            CollectionText("Digi-Egg exclusive");
            if(Button("collectionItemLoot","Find in Loot",-1,40))showEggs=true;
        }
        else
        {
            DateOnly day=DateOnly.FromDateTime(DateTime.Now);int price=CosmeticCollection.Price(kind,item,day);
            ImGui.BeginDisabled(price>collection.Bits);
            if(Button("buyCosmetic/"+kind+"/"+item.Id,$"Buy for {price} Bits",-1,40))purchase=(kind,item.Id,day);
            ImGui.EndDisabled();
            if(price>collection.Bits)HomeLabel("Not enough Bits",17,HomeMute);
        }
    }
}

using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
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
    internal bool CollectionStoreEggsVisible=>CollectionStoreVisible&&collectionStoreEggs;
    private bool collectionResetScroll;
    private bool CollectionActionButton(string key,string label,float width=0,float height=36,bool primary=false,bool accent=false)
    {
        // The original Store uses orange/gold primaries and beveled navy
        // secondary actions (theme.js), independently of the client PLAY CTA.
        var transparent=new ImColor8(0,0,0,0);
        ImGui.PushStyleColor(ImGuiCol.Button,transparent);ImGui.PushStyleColor(ImGuiCol.ButtonHovered,transparent);ImGui.PushStyleColor(ImGuiCol.ButtonActive,transparent);
        ImGui.PushStyleColor(ImGuiCol.Border,transparent);ImGui.PushStyleColor(ImGuiCol.Text,transparent);
        bool pressed=Button(key,label,width,height);
        ImGui.PopStyleColor(5);
        var lo=ImGui.GetItemRectMin();var hi=ImGui.GetItemRectMax();var draw=ImGui.GetWindowDrawList();bool hover=ImGui.IsItemHovered();
        ImColor8 Styled(ImColor8 color)
        {
            var rgba=color.AsByte4();float brightness=primary&&hover?1.08f:1;
            return new((byte)Math.Min(255,Math.Round(rgba.X*brightness)),(byte)Math.Min(255,Math.Round(rgba.Y*brightness)),(byte)Math.Min(255,Math.Round(rgba.Z*brightness)),(byte)Math.Clamp(Math.Round(rgba.W*ImGui.GetStyle().Alpha),0,255));
        }
        if(primary)
        {
            var middle=new float2(hi.X,lo.Y+(hi.Y-lo.Y)*.3f);var split=new float2(lo.X,middle.Y);
            var top=Styled(new ImColor8(255,210,122));var mid=Styled(NativeUiTheme.PlayMiddle);var bottom=Styled(NativeUiTheme.Play);
            draw.AddRectFilledMultiColor(lo,middle,top,top,mid,mid);draw.AddRectFilledMultiColor(split,hi,mid,mid,bottom,bottom);
            draw.AddLine(lo+new float2(1,1),new(hi.X-1,lo.Y+1),Styled(new ImColor8(255,255,255,90)));
        }
        else
        {
            var top=Styled(hover?new ImColor8(38,45,54):new ImColor8(30,35,40));var bottom=Styled(hover?new ImColor8(26,33,43):new ImColor8(20,26,34));
            draw.AddRectFilledMultiColor(lo,hi,top,top,bottom,bottom);
        }
        draw.AddRect(lo,hi,Styled(primary?new ImColor8(240,210,140):hover||accent?HomeGold:NativeUiTheme.GoldBorder));
        draw.PushClipRect(lo+new float2(2,1),hi-new float2(2,1),true);
        draw.AddText(lo+(hi-lo-ImGui.CalcTextSize(label))*.5f,Styled(primary?NativeUiTheme.PlayText:accent?HomeGold:HomeInk),label);draw.PopClipRect();
        if(ImGui.IsItemFocused())draw.AddRect(lo+new float2(3,3),hi-new float2(3,3),Styled(new ImColor8(63,214,255)),thickness:2);
        return pressed;
    }
    private void OpenCollectionPage(bool store=false,bool loot=false,string? kind=null,string? item=null)
    {
        if(ClientHomeVisible)RememberClientRoute(loot?"loot":store?"store":"collection");
        showCollection=true;showEggs=loot;collectionStore=store;collectionStoreEggs=false;collectionResetScroll=true;
        purchase=null;eggPurchase=null;hatching=null;boughtCosmetic=null;mysteryReward=null;collectionMessage=null;
        if(evolutionLegend is not null)CloseEvolution();
        if(kind is not null && Array.IndexOf(CosmeticCollection.Kinds,kind) is >=0 and var index)CollectionCategoryChanged(index);
        if(item is not null)previewCosmetic=item;
    }
    private void CollectionBrowser(CosmeticCollection collection)
    {
        if(collectionStore){CollectionStoreBrowser(collection);return;}
        if(MatchLockerVisible){CollectionLockerBrowser(collection);return;}
        CollectionPageBrowser(collection);
    }
    private void CollectionBanner(CosmeticCollection collection,bool compact=false)
    {
        string kind=CosmeticCollection.Kinds[collectionKind];
        var visible=CosmeticCollection.Items(kind).Where(item=>compact&&collectionStore?!item.Secret&&item.Linked is null:!item.Secret||collection.Owns(kind,item.Id)).ToArray();
        int owned=visible.Count(item=>collection.Owns(kind,item.Id));
        if(!compact){CollectionPageBanner(collection,kind,visible,owned);return;}
        float width=ImGui.GetContentRegionAvail().X,height=compact?60:92;
        var at=ImGui.GetCursorScreenPos();var draw=ImGui.GetWindowDrawList();
        string art=compact&&collectionStore?kind switch{"legend"=>"store-legends","board"=>"store-boards","elimsound"=>"store-hero",_=>"store-fx"}:kind switch{"legend"=>"coll-legends","board"=>"coll-boards","emote" or "sound"=>"coll-emotes",_=>"store-fx"};
        collectionArtwork.Draw(art,at,new(width,height),255,true,.45f);
        draw.AddRectFilledMultiColor(at,at+new float2(width,height),new ImColor8(1,10,19,210),new ImColor8(1,10,19,95),new ImColor8(1,10,19,140),new ImColor8(1,10,19,210));
        draw.AddRect(at,at+new float2(width,height),HomeLine);
        ImGui.SetCursorScreenPos(at+new float2(24,compact?16:24));
        ImGui.BeginGroup();
        if(!compact)HomeLabel(MatchLockerVisible?"LOCKER":"COLLECTION",15,HomeGold);
        HomeLabel(CollectionNames[collectionKind].ToUpperInvariant(),compact?20:28,HomeInk);
        ImGui.EndGroup();
        string progress=$"{owned} / {visible.Length} owned";
        if(compact){ImGui.SameLine(0,12);HomeLabel(progress,15,HomeMute);}
        else
        {
            ImGui.PushFont(NativeUiTheme.BodyFont,16);float textWidth=ImGui.CalcTextSize(progress).X;ImGui.PopFont();
            ImGui.SetCursorScreenPos(at+new float2(width-textWidth-24,24));HomeLabel(progress,16,HomeGold);
            var all=CosmeticCollection.Kinds.SelectMany(category=>CosmeticCollection.Items(category).Where(item=>!item.Secret||collection.Owns(category,item.Id)).Select(item=>(Kind:category,Item:item))).ToArray();
            string total=$"{all.Count(entry=>collection.Owns(entry.Kind,entry.Item.Id))} / {all.Length} in total";
            ImGui.PushFont(NativeUiTheme.BodyFont,15);float totalWidth=ImGui.CalcTextSize(total).X;ImGui.PopFont();
            ImGui.SetCursorScreenPos(at+new float2(width-totalWidth-24,49));HomeLabel(total,15,HomeMute);
            var start=at+new float2(width-184,74);draw.AddRectFilled(start,start+new float2(160,4),new ImColor8(32,47,58));
            draw.AddRectFilled(start,start+new float2(visible.Length==0?0:160f*owned/visible.Length,4),new ImColor8(42,192,210));
        }
        ImGui.SetCursorScreenPos(at+new float2(0,height+8));
    }
    private void CollectionCategoryTabs(CosmeticCollection collection)
        =>CollectionPageTabs(collection,collectionStore?[0,1,2,3,4,5,6,7]:MatchLockerVisible?[0,1,2,3,4,5,6]:[0,2,3,4,1,5,6]);
    private void CollectionLockerBrowser(CosmeticCollection collection)
    {
        CollectionCategoryTabs(collection);
        string kind=CosmeticCollection.Kinds[collectionKind];
        var items=CosmeticCollection.Items(kind).Where(item=>collection.Owns(kind,item.Id)).ToArray();
        CollectionFilterIds=items.Select(item=>item.Id).ToArray();collectionOwnership="owned";collectionSearchFocused=false;
        if(!items.Any(item=>item.Id==previewCosmetic))
        {
            string? equipped=CosmeticCollection.Multi(kind)?collection.Equipped(kind)?.AsArray().FirstOrDefault(item=>item is not null)?.GetValue<string>():collection.Equipped(kind)?.GetValue<string>();
            previewCosmetic=items.FirstOrDefault(item=>item.Id==equipped)?.Id??items.FirstOrDefault()?.Id;
        }
        ImGui.BeginChild("collectionItems"u8,new(0,0),ImGuiChildFlags.None);
        try
        {
            if(collectionResetScroll){ImGui.SetScrollY(0);collectionResetScroll=false;}
            CollectionListBounds=(ImGui.GetWindowPos(),ImGui.GetWindowPos()+ImGui.GetWindowSize());CollectionDetailBounds=CollectionListBounds;
            if(CosmeticCollection.Multi(kind))CollectionLockerWheel(collection,kind);
            else if(kind=="legend")CollectionLockerLegendSummary(collection);
            CollectionOwnedBanner(collection,kind,items.Length);
            if(items.Length==0)CollectionText("You don't own any "+CollectionNames[collectionKind].ToLowerInvariant()+" yet.");
            CollectionStoreGrid(collection,kind,items,true);
        }
        finally{ImGui.EndChild();}
    }
    private void CollectionOwnedBanner(CosmeticCollection collection,string kind,int owned)
    {
        float width=ImGui.GetContentRegionAvail().X;var at=ImGui.GetCursorScreenPos();var draw=ImGui.GetWindowDrawList();
        string art=kind=="legend"?"store-legends":kind=="board"?"store-boards":kind=="elimsound"?"store-hero":"store-fx";
        collectionArtwork.Draw(art,at,new(width,62),255,true,.45f);
        draw.AddRectFilledMultiColor(at,at+new float2(width,62),new ImColor8(1,10,19,230),new ImColor8(1,10,19,85),new ImColor8(1,10,19,110),new ImColor8(1,10,19,235));draw.AddRect(at,at+new float2(width,62),HomeLine);
        ImGui.SetCursorScreenPos(at+new float2(18,26));ImGui.BeginGroup();HomeLabel("OWNED "+CollectionNames[collectionKind].ToUpperInvariant(),17,HomeInk);ImGui.SameLine();HomeLabel(owned.ToString(),15,HomeMute);
        int more=CosmeticCollection.Items(kind).Count(item=>!item.Secret&&!collection.Owns(kind,item.Id));
        if(more>0&&!CosmeticCollection.Multi(kind))
        {
            ImGui.SameLine(0,12);ImGui.PushFont(NativeUiTheme.BodyFont,15);ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(8,4));
            if(CollectionActionButton("collectionLockerStore",more+" more in the Store",0,28))OpenCollectionPage(store:true,kind:kind);
            ImGui.PopStyleVar();ImGui.PopFont();
        }
        ImGui.EndGroup();ImGui.SetCursorScreenPos(at+new float2(0,66));ImGui.Dummy(new(0,0));
    }
    private void CollectionLockerLegendSummary(CosmeticCollection collection)
    {
        string? id=collection.Equipped("legend")?.GetValue<string>();var item=CosmeticCollection.Item("legend",id);
        if(item is null)return;
        var evolution=LegendEvolution.Inspect(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"),profile.Storage.GetValueOrDefault("dt.cosmetics.v1"),profile.Storage.GetValueOrDefault("dt.farm.v1"),item.Id);
        var form=LegendEvolution.Form(evolution.Form);var discovery=LegendEvolution.Discovery(profile.Storage.GetValueOrDefault("dt.legendevo.v1"),item.Id);
        int reachable=LegendEvolution.Tree(item.Id).Tiers.Sum(tier=>tier.Length);
        float width=ImGui.GetContentRegionAvail().X;var at=ImGui.GetCursorScreenPos();var draw=ImGui.GetWindowDrawList();
        ImGui.BeginGroup();ImGui.Dummy(new(width,116));
        draw.AddRectFilledMultiColor(at,at+new float2(width,116),new ImColor8(44,37,24,180),new ImColor8(10,26,43,170),new ImColor8(7,21,35,170),new ImColor8(31,29,23,180));draw.AddRect(at,at+new float2(width,116),HomeLine);
        portraits.DrawCompanion(CurrentPreviewLegend(collection,item.Id),at+new float2(12,26),64);
        ImGui.SetCursorScreenPos(at+new float2(90,12));ImGui.BeginGroup();
        HomeLabel(form.Name,20,HomeInk);ImGui.SameLine();HomeLabel((evolution.Form==item.Id?"":item.Name+"'s ")+form.Level+" / Equipped",15,HomeMute);
        HomeLabel("Lv "+evolution.Level,16,HomeGold);ImGui.SameLine();
        ImGui.ProgressBar(evolution.Need==0?1:(float)evolution.Data/evolution.Need,new(Math.Min(320,width-440),8),""u8);ImGui.SameLine();HomeLabel(evolution.Need==0?"Max level":$"{evolution.Data}/{evolution.Need} Digi-Data",15,HomeMute);
        string next=evolution.CanDigivolve?"Ready to digivolve!":evolution.Next is {} level?"Digivolves at level "+level:"Final form reached";
        HomeLabel(next+$" / Dex {discovery.Dex.Length}/{reachable}",15,evolution.CanDigivolve?HomeGold:HomeMute);ImGui.EndGroup();
        ImGui.SetCursorScreenPos(at+new float2(width-132,16));
        if(CollectionActionButton("lockerLegendEvolution/"+item.Id,"Digivolve",120,36,true)){evolutionLegend=item.Id;collectionMessage=null;}
        ImGui.SetCursorScreenPos(at+new float2(width-132,60));
        if(CollectionActionButton("lockerLegendFarm/"+item.Id,"Digi-Farm",120,36))OpenCollectionFarm(collection,item.Id);
        ImGui.SetCursorScreenPos(at+new float2(0,116));ImGui.Dummy(new(0,0));ImGui.EndGroup();
    }
    private CosmeticItem[] StoreItems(CosmeticCollection collection,string kind)
    {
        var items=CosmeticCollection.Items(kind).Where(item=>!item.Secret&&item.Linked is null)
            .OrderBy(item=>item.Free?0:1).ThenBy(item=>Array.IndexOf(DigiTactics.Presentation.CosmeticBrowser.Rarities,item.Rarity??"common")).ThenBy(item=>item.Price).ToArray();
        CollectionFilterIds=items.Select(item=>item.Id).ToArray();
        return items;
    }
    private void CollectionStoreEggs(CosmeticCollection collection)
    {
        CollectionText("A Digi-Egg hatches one random cosmetic. Odds are per rarity, then every item of that rarity is equally likely. Duplicate Little Legends become Digi-Data toward digivolving, or Bits at max level. Other duplicates refund 40% of their price as Bits. Rare, epic and legendary Little Legends only come from eggs.");
        var eggs=DigiEggs.Definitions.Select(definition=>
        {
            var pool=DigiEggs.Pool(definition);var odds=DigiEggs.Odds(definition);
            var exclusives=pool.Where(entry=>entry.Item.EggOnly&&!entry.Item.Free).Select(entry=>
            {
                var rarity=odds.Single(odd=>odd.Rarity==(entry.Item.Rarity??"common"));
                return new EggExclusive(entry.Kind,entry.Item,collection.Owns(entry.Kind,entry.Item.Id),rarity.Pct/rarity.Count);
            }).ToArray();
            return new IncubatorEgg(definition,pool.Count(entry=>collection.Owns(entry.Kind,entry.Item.Id)),pool.Length,odds,exclusives);
        }).ToArray();
        CollectionFilterIds=eggs.Select(egg=>egg.Definition.Id).ToArray();
        float available=ImGui.GetContentRegionAvail().X;int columns=Math.Max(2,(int)((available+12)/222));
        float width=(available-(columns-1)*12)/columns;
        for(int row=0;row<eggs.Length;row+=columns)
        {
            int count=Math.Min(columns,eggs.Length-row);
            float height=440+Enumerable.Range(row,count).Max(i=>CollectionStoreExclusiveRows(eggs[i],width-24))*26;
            for(int column=0;column<count;column++)
            {
                if(column>0)ImGui.SameLine(0,12);
                var egg=eggs[row+column];
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(12,12));
                ImGui.BeginChild("storeEggCard/"+egg.Definition.Id,new(width,height),ImGuiChildFlags.Borders,ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
                ImGui.PopStyleVar();
                try
                {
                    EggArt(egg.Definition.Id);
                    HomeLabel(egg.Definition.Name,18,HomeInk);
                    ImGui.PushFont(NativeUiTheme.BodyFont,15);ImGui.PushStyleColor(ImGuiCol.Text,HomeMute);
                    string description=egg.Definition.Id=="mixed"?"Any cosmetic from any category.":"One of the "+CollectionNames[Array.IndexOf(CosmeticCollection.Kinds,egg.Definition.Kinds[0])].ToLowerInvariant()+".";
                    CollectionText(description+$" {egg.Owned}/{egg.Total} owned.");ImGui.PopStyleColor();ImGui.PopFont();
                    ImGui.SetCursorPosY(212);CollectionStoreEggOdds(egg);
                    ImGui.SetCursorPosY(340);
                    if(egg.Exclusives.Length>0)CollectionStoreEggExclusives(egg);
                    ImGui.SetCursorPosY(height-50);
                    ImGui.BeginDisabled(egg.Total==0||collection.Bits<egg.Definition.Price);
                    if(CollectionActionButton("egg/"+egg.Definition.Id,"Hatch / "+egg.Definition.Price+" Bits",-1,38))BeginIncubatorHatch(egg.Definition.Id);
                    ImGui.EndDisabled();
                }
                finally{ImGui.EndChild();}
            }
        }
    }
    private static int CollectionStoreExclusiveRows(IncubatorEgg egg,float width)
    {
        if(egg.Exclusives.Length==0)return 0;
        float x=0;int rows=1;ImGui.PushFont(NativeUiTheme.BodyFont,13);
        foreach(var exclusive in egg.Exclusives)
        {
            float badgeWidth=Math.Min(width,ImGui.CalcTextSize(exclusive.Item.Name+" "+exclusive.Chance).X+12);
            if(x>0&&x+badgeWidth>width){x=0;rows++;}x+=badgeWidth+4;
        }
        ImGui.PopFont();return rows;
    }
    private static void CollectionStoreEggOdds(IncubatorEgg egg)
    {
        float width=ImGui.GetContentRegionAvail().X;var at=ImGui.GetCursorScreenPos();var draw=ImGui.GetWindowDrawList();
        ImGui.PushFont(NativeUiTheme.BodyFont,14);
        for(int i=0;i<egg.Odds.Length;i++)
        {
            var odds=egg.Odds[i];float y=i*24;
            string percent=$"{odds.Pct:0.#}%",items=odds.Count+(odds.Count==1?" item":" items");
            draw.AddText(at+new float2(0,y),RarityColor(odds.Rarity),odds.Rarity);
            draw.AddText(at+new float2(width*.64f-ImGui.CalcTextSize(percent).X,y),HomeInk,percent);
            draw.AddText(at+new float2(width-ImGui.CalcTextSize(items).X,y),HomeMute,items);
            draw.AddLine(at+new float2(0,y+21),at+new float2(width,y+21),HomeLine);
        }
        ImGui.PopFont();ImGui.Dummy(new(width,egg.Odds.Length*24));
    }
    private static void CollectionStoreEggExclusives(IncubatorEgg egg)
    {
        HomeLabel("EGG ONLY",14,HomeMute);
        var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X,x=0,y=0;var draw=ImGui.GetWindowDrawList();
        ImGui.PushFont(NativeUiTheme.BodyFont,13);
        foreach(var exclusive in egg.Exclusives)
        {
            string text=exclusive.Item.Name+" "+exclusive.Chance;
            float badgeWidth=Math.Min(width,ImGui.CalcTextSize(text).X+12);
            if(x>0&&x+badgeWidth>width){x=0;y+=26;}
            var badgeAt=at+new float2(x,y);var color=exclusive.Owned?HomeMute:RarityColor(exclusive.Item.Rarity);
            draw.AddRectFilled(badgeAt,badgeAt+new float2(badgeWidth,22),new ImColor8(8,21,33));draw.AddRect(badgeAt,badgeAt+new float2(badgeWidth,22),color);
            draw.PushClipRect(badgeAt+new float2(6,0),badgeAt+new float2(badgeWidth-6,22),true);draw.AddText(badgeAt+new float2(6,3),color,text);draw.PopClipRect();
            if(ImGui.IsMouseHoveringRect(badgeAt,badgeAt+new float2(badgeWidth,22)))ImGui.SetTooltip(exclusive.Item.Name+" / "+exclusive.Chance+(exclusive.Owned?" / owned":""));
            x+=badgeWidth+4;
        }
        ImGui.PopFont();ImGui.Dummy(new(width,y+22));
    }
    private void CollectionThumbnail(CosmeticCollection collection,string kind,CosmeticItem item,float2 at,float2 extent,bool store=false)
    {
        if(!ImGui.IsRectVisible(at,at+extent))return;
        if(store&&kind=="dance"&&StoreDanceThumbnail(collection,item,at,extent))return;
        float square=extent.Y;
        if(kind=="legend")portraits.DrawCompanion(CurrentPreviewLegend(collection,item.Id),at+new float2((extent.X-square)/2,0),square);
        else if(kind=="emote")emoteArtwork.Draw(item.Id,(long)(boardSceneTime*1000),at+new float2((extent.X-square+10)/2,5),square-10);
        else if(kind=="killscreen")collectionTheme.Draw(item.Id,false,boardSceneTime%3.5,at,extent);
        else if(CollectionArt.Value.TryGetValue(kind+"/"+item.Id,out string? file))collectionArtwork.Draw(file,at,extent,255,kind=="board");
    }
    private void CollectionStoreGrid(CosmeticCollection collection,string kind,CosmeticItem[] items,bool locker=false)
    {
        var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;
        float end=StoreGrid(collection,kind,items,at,width,locker);
        ImGui.SetCursorScreenPos(at);ImGui.Dummy(new(width,Math.Max(0,end-at.Y)+12));
    }
    private bool CollectionWorn(CosmeticCollection collection,string kind,string id,bool selectedSlot=false)=>CosmeticCollection.Multi(kind)
        ?selectedSlot?collection.Equipped(kind)!.AsArray().ElementAtOrDefault(collectionSlot)?.GetValue<string>()==id:collection.Equipped(kind)!.AsArray().Any(value=>value?.GetValue<string>()==id)
        :collection.Equipped(kind)?.GetValue<string>()==id;
    private static string CollectionSlotLabel(string value,float width)
    {
        if(ImGui.CalcTextSize(value).X<=width)return value;
        while(value.Length>0&&ImGui.CalcTextSize(value+"...").X>width)value=value[..^1];
        return value+"...";
    }
    private void CollectionLockerWheel(CosmeticCollection collection,string kind)
    {
        float available=ImGui.GetContentRegionAvail().X,wheelWidth=Math.Min(380,available),height=290;
        float radiusX=Math.Min(128,(wheelWidth-112)/2),radiusY=104;var origin=ImGui.GetCursorScreenPos();var center=origin+new float2(wheelWidth/2,height/2);var draw=ImGui.GetWindowDrawList();
        ImGui.BeginGroup();ImGui.Dummy(new(wheelWidth,height));
        for(int i=0;i<64;i+=2)
        {
            double a=i*Math.PI*2/64,b=(i+1)*Math.PI*2/64;
            draw.AddLine(center+new float2((float)Math.Cos(a)*radiusX,(float)Math.Sin(a)*radiusY),center+new float2((float)Math.Cos(b)*radiusX,(float)Math.Sin(b)*radiusY),HomeLine,2);
        }
        ImGui.PushFont(NativeUiTheme.DisplayBoldFont,14);
        string label=kind=="emote"?"EMOTE":"SOUND";float labelWidth=ImGui.CalcTextSize(label).X;
        draw.AddText(center+new float2(-labelWidth/2,-16),HomeMute,label);float wheelLabelWidth=ImGui.CalcTextSize("WHEEL"u8).X;draw.AddText(center+new float2(-wheelLabelWidth/2,4),HomeMute,"WHEEL"u8);ImGui.PopFont();
        var equipped=collection.Equipped(kind)!.AsArray();
        for(int i=0;i<6;i++)
        {
            double angle=i*Math.PI/3-Math.PI/2;var at=center+new float2((float)Math.Cos(angle)*radiusX-52,(float)Math.Sin(angle)*radiusY-33);
            string? id=equipped.ElementAtOrDefault(i)?.GetValue<string>();var item=CosmeticCollection.Item(kind,id);
            ImGui.SetCursorScreenPos(at);
            if(Button("collectionSlot/"+i,"",104,66,collectionSlot==i))collectionSlot=i;
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,13);draw.AddText(at+new float2(6,4),HomeMute,(i+1).ToString());ImGui.PopFont();
            if(item is not null)CollectionThumbnail(collection,kind,item,at+new float2(34,4),new(36,32));
            ImGui.PushFont(NativeUiTheme.BodyFont,14);string text=CollectionSlotLabel(item?.Name??"Empty",92);float labelSize=ImGui.CalcTextSize(text).X;
            draw.AddText(at+new float2((104-labelSize)/2,45),HomeInk,text);ImGui.PopFont();
            if(ImGui.IsItemHovered())ImGui.SetTooltip("Slot "+(i+1)+" / "+(item?.Name??"Empty"));
        }
        ImGui.SetCursorScreenPos(origin+new float2(0,height));ImGui.Dummy(new(0,0));ImGui.EndGroup();
        bool beside=available-wheelWidth-24>=210;
        if(beside)ImGui.SameLine(0,24);
        ImGui.BeginChild("collectionWheelHelp"u8,new(Math.Min(360,beside?available-wheelWidth-24:available),beside?height:200),ImGuiChildFlags.None,ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        try
        {
            if(beside)ImGui.SetCursorPosY(62);
            HomeLabel("SLOT "+(collectionSlot+1)+" SELECTED",18,HomeInk);
            CollectionText("Pick a slot on the wheel, then choose what goes in it.");
            ImGui.BeginDisabled(equipped.ElementAtOrDefault(collectionSlot) is null);
            if(CollectionActionButton("clearCollectionSlot","Empty slot "+(collectionSlot+1),0,36))ChangeCollection(next=>next.Equip(kind,null,collectionSlot),"Slot cleared.");
            ImGui.EndDisabled();
            if(CollectionActionButton("collectionWheelStore","More in the Store",0,36))OpenCollectionPage(store:true,kind:kind);
        }
        finally{ImGui.EndChild();}
        ImGui.Spacing();
    }
}

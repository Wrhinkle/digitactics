using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;
using S = DigiTactics.Desktop.CosmeticStyle;

namespace DigiTactics.Desktop;

/// <summary>
/// Store and Locker from v1.0.14 js/shop.js with js/shopstyle.js and the theme.js #dtStore restyle:
/// art header (modes, daily missions, wallet), display-font tabs, the featured strip, art section
/// banners and 176px auto-fill .dts-card grids.
/// </summary>
internal sealed partial class MatchScreen
{
    private const float StoreButtonHeight=33;
    private static readonly ImColor8 StoreMuted=new(143,154,176);

    private void CollectionStoreBrowser(CosmeticCollection collection)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(0,0));
        try{StorePage(collection);}
        finally{ImGui.PopStyleVar();}
    }
    private void StorePage(CosmeticCollection collection)
    {
        var origin=ImGui.GetCursorScreenPos();var extent=ImGui.GetContentRegionAvail();var draw=ImGui.GetWindowDrawList();
        if(!MatchCosmeticsVisible)
        {
            // .dts-win: navy gradient panel with a gold-4 frame filling the page.
            draw.AddRectFilledMultiColor(origin,origin+extent,new ImColor8(10,20,40),new ImColor8(10,20,40),S.Navy0,S.Navy0);
            draw.AddRect(origin,origin+extent,S.Gold4);
            ImGui.SetCursorScreenPos(origin+new float2(1,1));
            StoreHeader(collection,extent.X-2);
        }
        ImGui.SetCursorScreenPos(new(origin.X+(MatchCosmeticsVisible?0:1),ImGui.GetCursorScreenPos().Y));
        StoreTabs(collection,MatchCosmeticsVisible?extent.X:extent.X-2);
        var bodyAt=ImGui.GetCursorScreenPos();
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(0,0));
        ImGui.BeginChild("collectionItems"u8,new(MatchCosmeticsVisible?extent.X:extent.X-2,origin.Y+extent.Y-1-bodyAt.Y),ImGuiChildFlags.None,ImGuiWindowFlags.NoBackground);
        try
        {
            if(collectionResetScroll){ImGui.SetScrollY(0);collectionResetScroll=false;}
            CollectionListBounds=(ImGui.GetWindowPos(),ImGui.GetWindowPos()+ImGui.GetWindowSize());CollectionDetailBounds=CollectionListBounds;
            // .dts-body padding: 14px 16px 24px
            var at=ImGui.GetCursorScreenPos()+new float2(16,14);float width=ImGui.GetContentRegionAvail().X-32;
            ImGui.SetCursorScreenPos(at);
            if(collectionStoreEggs){ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(8,8));ImGui.BeginGroup();CollectionStoreEggs(collection);ImGui.EndGroup();ImGui.PopStyleVar();}
            else
            {
                string kind=CosmeticCollection.Kinds[collectionKind];
                float y=StoreFeatured(collection,at,width);
                y+=18;
                var items=StoreItems(collection,kind);
                y=StoreSectionHeading(collection,kind,items,new(at.X,y),width);
                if(kind=="elimsound")y=StoreElimNote(new(at.X,y),width);
                y=StoreGrid(collection,kind,items,new(at.X,y),width,false);
                ImGui.SetCursorScreenPos(new(at.X,y+24));
            }
            ImGui.Dummy(new(width,0));
        }
        finally{CssScrollbar();ImGui.EndChild();ImGui.PopStyleVar(2);}
    }
    /// <summary>.dts-head over store-hero art: STORE/LOCKER modes, daily mission chips and the Bits wallet with History.</summary>
    private void StoreHeader(CosmeticCollection collection,float width)
    {
        var at=ImGui.GetCursorScreenPos();var draw=ImGui.GetWindowDrawList();const float height=63;
        var daily=MatchRewards.LiveProgress(profile!.Storage.GetValueOrDefault("dt.shop.v1"),null,HomeDay);
        CollectionMissions=daily.Missions;CollectionBitsHistoryRows=0;
        collectionArtwork.Draw("store-hero",at,new(width,height),255,true,.55f);
        float split=width*.55f;
        draw.AddRectFilledMultiColor(at,at+new float2(split,height),new ImColor8(1,10,19,242),new ImColor8(1,10,19,140),new ImColor8(1,10,19,140),new ImColor8(1,10,19,242));
        draw.AddRectFilledMultiColor(at+new float2(split,0),at+new float2(width,height),new ImColor8(1,10,19,140),new ImColor8(1,10,19,217),new ImColor8(1,10,19,217),new ImColor8(1,10,19,140));
        draw.AddLine(new(at.X,at.Y+height+.5f),new(at.X+width,at.Y+height+.5f),S.Gold5);
        // .dts-modes segment
        var modeType=S.Display(15,.1f,false);float storeWidth=modeType.Width("STORE")+34,lockerWidth=modeType.Width("LOCKER")+34;
        var seg=at+new float2(16,(height-36)*.5f);float segWidth=storeWidth+lockerWidth+6;
        draw.AddRectFilled(seg,seg+new float2(segWidth,36),new ImColor8(1,10,19,217));draw.AddRect(seg,seg+new float2(segWidth,36),S.Gold5);
        var storeAt=seg+new float2(3,3);var lockerAt=storeAt+new float2(storeWidth+2,0);
        CosmeticHit("storeStore",storeAt,new(storeWidth,30),out _);
        draw.AddRectFilledMultiColor(storeAt,storeAt+new float2(storeWidth,30),new ImColor8(30,35,40),new ImColor8(30,35,40),new ImColor8(42,50,64),new ImColor8(42,50,64));draw.AddRect(storeAt,storeAt+new float2(storeWidth,30),S.Gold4);
        S.TextIn(draw,storeAt+new float2(16,0),30,S.Gold1,"STORE",modeType);
        if(CosmeticHit("storeLocker",lockerAt,new(lockerWidth,30),out bool lockerHover))OpenCollectionPage();
        S.TextIn(draw,lockerAt+new float2(16,0),30,lockerHover?S.Gold1:S.LcMute,"LOCKER",modeType);
        // .dts-wallet (right)
        var walletType=S.BodyExtraBold(18);string bits=collection.Bits.ToString();var histType=S.Body(12);
        float historyWidth=histType.Width("History")+18,walletWidth=12+17+walletType.Width(bits)+8+historyWidth+6;
        var wallet=new float2(at.X+width-16-walletWidth,at.Y+(height-34)*.5f);
        draw.AddRectFilled(wallet,wallet+new float2(walletWidth,34),new ImColor8(1,10,19,217));draw.AddRect(wallet,wallet+new float2(walletWidth,34),S.Gold4);
        S.BitsGem(draw,wallet+new float2(12+6,17));S.TextIn(draw,wallet+new float2(12+17,0),34,S.Text1,bits,walletType);
        var historyAt=new float2(wallet.X+walletWidth-6-historyWidth,wallet.Y+6);
        ImGui.SetCursorScreenPos(historyAt);
        ImGui.PushFont(histType.Font,histType.Size);ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(9,(22-histType.Size)*.5f));
        ImGui.PushStyleColor(ImGuiCol.FrameBg,new ImColor8(30,35,40));ImGui.PushStyleColor(ImGuiCol.FrameBgHovered,new ImColor8(38,45,54));ImGui.PushStyleColor(ImGuiCol.Text,S.LcText);ImGui.PushStyleColor(ImGuiCol.Border,S.Gold4);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,1);
        CollectionHistoryControl(historyWidth);
        ImGui.PopStyleVar(2);ImGui.PopStyleColor(4);ImGui.PopFont();
        // .dts-missions: muted label and chips, flex:1 between the modes and the wallet
        // The label is a .muted span (13px); flex gap 10px separates it and every chip.
        var label=S.Body(13);var chip=S.Body(12);var chipBold=S.Body(12,true);
        float x=seg.X+segWidth+14,limit=wallet.X-14;
        x+=S.TextIn(draw,new(x,at.Y),height,StoreMuted,"Daily missions:",label)+10;
        foreach(var mission in daily.Missions)
        {
            string progress=mission.Done?"done":"";
            float w=8+chip.Width(mission.Name)+4+15+chipBold.Width(mission.Bits.ToString())+(progress.Length>0?4+chip.Width(progress):0)+8;
            if(x+w>limit)break;
            var c=new float2(x,at.Y+(height-23)*.5f);
            draw.AddRectFilled(c,c+new float2(w,23),new ImColor8(1,10,19,191));draw.AddRect(c,c+new float2(w,23),mission.Done?new ImColor8(79,224,143):S.Gold5);
            float tx=c.X+8;tx+=S.TextIn(draw,new(tx,c.Y),23,mission.Done?StoreMuted:S.Text1,mission.Name,chip)+4;
            S.BitsGem(draw,new(tx+6,c.Y+11.5f));tx+=15;tx+=S.TextIn(draw,new(tx,c.Y),23,S.Text1,mission.Bits.ToString(),chipBold);
            if(progress.Length>0)S.TextIn(draw,new(tx+4,c.Y),23,S.Success,progress,chip);
            ImGui.SetCursorScreenPos(c);ImGui.Dummy(new(w,23));
            if(ImGui.IsItemHovered())ImGui.SetTooltip("Complete it in one game today"u8);
            x+=w+10;
        }
        ImGui.SetCursorScreenPos(new(at.X,at.Y+height+1));ImGui.Dummy(new(0,0));
    }
    /// <summary>.dts-tabs: navy strip, display-font bold tabs with muted counts and the gold underline.</summary>
    private void StoreTabs(CosmeticCollection collection,float width)
    {
        int[] order=collectionStore?[0,1,2,3,4,5,6,7]:[0,1,2,3,4,5,6];
        var origin=ImGui.GetCursorScreenPos();var draw=ImGui.GetWindowDrawList();
        // The count span inherits the tab's Chakra Petch 700 and its computed 1.4px letter-spacing.
        var label=S.Display(14,.1f);var counter=S.Display(12,1.4f/12);const float height=42;
        float space=label.Font.CalcTextSizeA(label.Size,float.MaxValue,0," "u8).X+label.Tracking;
        draw.AddRectFilled(origin,origin+new float2(width,height),new ImColor8(1,10,19,153));
        draw.AddLine(new(origin.X,origin.Y+height+.5f),new(origin.X+width,origin.Y+height+.5f),S.Gold5);
        string? Count(int i)=>i==7?null:(collectionStore?CosmeticCollection.Items(CosmeticCollection.Kinds[i]).Count(item=>!item.Secret&&item.Linked is null):CosmeticCollection.Items(CosmeticCollection.Kinds[i]).Count(item=>collection.Owns(CosmeticCollection.Kinds[i],item.Id))).ToString();
        bool compact=S.Narrow||16+order.SkipLast(1).Sum(i=>28+label.Width(i==7?"DIGI-EGGS":CollectionTabs[i].ToUpperInvariant())+(Count(i) is {} c?space+counter.Width(c):0)+2)>=width-8;
        float x=16;
        foreach(int i in order)
        {
            bool eggs=i==7,selected=eggs?collectionStoreEggs:!collectionStoreEggs&&collectionKind==i;
            string name=eggs?"DIGI-EGGS":CollectionTabs[i].ToUpperInvariant();string? count=null;
            if(!eggs)
            {
                string kind=CosmeticCollection.Kinds[i];
                count=(collectionStore?CosmeticCollection.Items(kind).Count(item=>!item.Secret&&item.Linked is null):CosmeticCollection.Items(kind).Count(item=>collection.Owns(kind,item.Id))).ToString();
            }
            if(compact)count=null;
            float pad=compact?9:14;
            float nameWidth=label.Width(name),tabWidth=pad*2+nameWidth+(count is null?0:space+counter.Width(count));
            if(x>=width)break;
            var at=origin+new float2(x,0);
            bool clicked=CosmeticHit("collectionTab/"+i,at,new(Math.Min(tabWidth,width-x),height),out bool hover);
            draw.PushClipRect(origin,origin+new float2(width,height+1),true);
            float baseline=at.Y+(compact?9:11)+(18.9f-label.Size)*.5f+.992f*14;
            S.Text(draw,new(at.X+pad,baseline-.992f*14),selected||hover?S.Gold1:S.LcMute,name,label);
            if(count is not null)S.Text(draw,new(at.X+pad+nameWidth+space,baseline-.992f*12),StoreMuted,count,counter);
            if(selected)
            {
                draw.AddRectFilled(new(at.X+10,at.Y+height-6),new(at.X+tabWidth-10,at.Y+height),new ImColor8(200,155,60,26));
                draw.AddRectFilled(new(at.X+10,at.Y+height-1.5f),new(at.X+tabWidth-10,at.Y+height+.5f),S.Gold2);
            }
            draw.PopClipRect();
            if(clicked){if(eggs){collectionStoreEggs=true;collectionResetScroll=true;}else CollectionCategoryChanged(i);}
            x+=tabWidth+2;
        }
        ImGui.SetCursorScreenPos(origin+new float2(0,height+1));ImGui.Dummy(new(0,0));
    }
    /// <summary>.dts-feat: art-backed strip with the "Featured today" heading and the day's discounted cards.</summary>
    private float StoreFeatured(CosmeticCollection collection,float2 at,float width)
    {
        var draw=ImGui.GetWindowDrawList();var day=HomeDay;
        var offers=CosmeticCollection.Featured(day);
        if(offers.Length==0)return at.Y;
        float inner=width-2-24;int columns=Math.Max(1,(int)((inner+12)/188));float cardWidth=(inner-(columns-1)*12)/columns;
        float cardHeight=offers.Max(offer=>StoreCardHeight(collection,offer.Kind,offer.Item,cardWidth,true,false));
        int rows=(Math.Min(offers.Length,999)+columns-1)/columns;
        float height=1+10+6+18.9f+10+rows*cardHeight+(rows-1)*12+14+1;
        collectionArtwork.Draw("store-hero",at,new(width,height),255,true,.4f);
        float split=width*.6f;
        draw.AddRectFilledMultiColor(at,at+new float2(split,height),new ImColor8(1,10,19,235),new ImColor8(1,10,19,153),new ImColor8(1,10,19,153),new ImColor8(1,10,19,235));
        draw.AddRectFilledMultiColor(at+new float2(split,0),at+new float2(width,height),new ImColor8(1,10,19,153),new ImColor8(1,10,19,230),new ImColor8(1,10,19,230),new ImColor8(1,10,19,153));
        draw.AddRect(at,at+new float2(width,height),S.Gold3);
        var heading=S.Display(14,.12f);float y=at.Y+1+10+6;
        float w=S.TextIn(draw,new(at.X+13,y),18.9f,S.Gold1,"FEATURED TODAY",heading);
        S.TextIn(draw,new(at.X+13+w+10,y+1),18.9f,StoreMuted,"25% off, changes at midnight",S.Body(13));
        y+=18.9f+10;
        for(int i=0;i<offers.Length;i++)
        {
            var offer=offers[i];var cardAt=new float2(at.X+13+i%columns*(cardWidth+12),y+i/columns*(cardHeight+12));
            StoreCard(collection,offer.Kind,offer.Item,cardAt,new(cardWidth,cardHeight),true,false);
        }
        return at.Y+height;
    }
    /// <summary>Section h3 over its category art: 26px top padding, gold-5 frame, owned count.</summary>
    private float StoreSectionHeading(CosmeticCollection collection,string kind,CosmeticItem[] items,float2 at,float width)
    {
        var draw=ImGui.GetWindowDrawList();const float height=57;
        string art=kind switch{"legend"=>"store-legends","board"=>"store-boards","killscreen" or "dance" or "emote" or "sound"=>"store-fx",_=>"store-hero"};
        collectionArtwork.Draw(art,at,new(width,height),255,true,.45f);
        draw.AddRectFilledMultiColor(at,at+new float2(width*.7f,height),new ImColor8(1,10,19,235),new ImColor8(1,10,19,89),new ImColor8(1,10,19,89),new ImColor8(1,10,19,235));
        draw.AddRectFilled(at+new float2(width*.7f,0),at+new float2(width,height),new ImColor8(1,10,19,89));
        draw.AddRect(at,at+new float2(width,height),S.Gold5);
        var heading=S.Display(14,.12f);
        float w=S.TextIn(draw,new(at.X+19,at.Y+26),18.9f,S.Gold1,CollectionNames[collectionKind].ToUpperInvariant(),heading);
        int owned=items.Count(item=>collection.Owns(kind,item.Id));
        S.TextIn(draw,new(at.X+19+w+10,at.Y+27),18.9f,StoreMuted,$"{owned}/{items.Length} owned",S.Body(13));
        return at.Y+height+12;
    }
    private float StoreElimNote(float2 at,float width)
    {
        var draw=ImGui.GetWindowDrawList();var type=S.Body(13);string text="Every soundboard clip you own also works as an elimination sound. Equip one in the Locker.";
        var button=S.Body(12);float buttonWidth=button.Width("Soundboard clips")+16;
        float textHeight=S.ParagraphHeight(text,type,width-36-buttonWidth-8,1.35f);float height=34+Math.Max(textHeight,24)+14;
        collectionArtwork.Draw("store-hero",at,new(width,height),255,true,.4f);
        draw.AddRectFilledMultiColor(at,at+new float2(width*.75f,height),new ImColor8(1,10,19,242),new ImColor8(1,10,19,115),new ImColor8(1,10,19,115),new ImColor8(1,10,19,242));
        draw.AddRect(at,at+new float2(width,height),S.Gold5);
        S.Paragraph(draw,new(at.X+18,at.Y+34),S.Text1,text,type,width-36-buttonWidth-8,1.35f);
        if(CosmeticButton("storeSoundboardClips",new(at.X+width-18-buttonWidth,at.Y+34),new(buttonWidth,24),"Soundboard clips",button))OpenCollectionPage(store:true,kind:"sound");
        return at.Y+height+12;
    }
    /// <summary>.dts-grid: auto-fill minmax(176px,1fr) with 12px gaps; rows take their tallest card.</summary>
    private float StoreGrid(CosmeticCollection collection,string kind,CosmeticItem[] items,float2 at,float width,bool locker)
    {
        if(items.Length==0)return at.Y;
        int columns=Math.Max(1,(int)((width+12)/188));float cardWidth=(width-(columns-1)*12)/columns;float y=at.Y;
        for(int row=0;row<items.Length;row+=columns)
        {
            int count=Math.Min(columns,items.Length-row);
            float height=Enumerable.Range(row,count).Max(i=>StoreCardHeight(collection,kind,items[i],cardWidth,false,locker));
            for(int c=0;c<count;c++)StoreCard(collection,kind,items[row+c],new(at.X+c*(cardWidth+12),y),new(cardWidth,height),false,locker);
            y+=height+12;
        }
        return y-12;
    }
    private string StoreRarityLine(string kind,CosmeticItem item,bool featured)
        =>(S.RarityName(item.Rarity)+(item.Tag is not null?" · "+item.Tag:"")+(featured?" · "+CollectionKindName(kind):"")).ToUpperInvariant();
    private float StoreCardHeight(CosmeticCollection collection,string kind,CosmeticItem item,float width,bool featured,bool locker)
    {
        float inner=width-22;var name=S.Body(14,true);var rar=S.Body(11,true,1/11f);var desc=S.Body(12);
        float h=11+120+4+S.ParagraphHeight(item.Name,name,inner,1.35f)+4+S.ParagraphHeight(StoreRarityLine(kind,item,featured),rar,inner,1.35f);
        if(item.Desc is not null)h+=4+S.ParagraphHeight(item.Desc,desc,inner,1.35f);
        if(kind=="legend"&&collection.Owns(kind,item.Id))h+=4+18+4+4+25;
        if(locker&&kind=="legend"&&collection.Owns(kind,item.Id))h+=0;
        return h+4+4+StoreButtonHeight+11;
    }
    /// <summary>One .dts-card: rarity-lit preview, name, rarity line, description, legend level row and the action.</summary>
    private void StoreCard(CosmeticCollection collection,string kind,CosmeticItem item,float2 at,float2 extent,bool featured,bool locker)
    {
        bool visible=ImGui.IsRectVisible(at-new float2(16,16),at+extent+new float2(16,16));
        var draw=ImGui.GetWindowDrawList();var rarity=S.Rarity(item.Rarity);bool owned=collection.Owns(kind,item.Id);
        bool equipped=owned&&CollectionWorn(collection,kind,item.Id,locker&&CosmeticCollection.Multi(kind));
        float inner=extent.X-22;
        string key=(featured?"collectionOffer/":"storeCard/")+kind+"/"+item.Id;
        // Hover is geometric; the card's own hit target is submitted after its inner controls so they keep priority.
        bool hovered=ImGui.IsWindowHovered()&&ImGui.IsMouseHoveringRect(at,at+extent);
        if(hovered)at.Y-=2;
        var end=at+extent;
        if(hovered){for(int i=1;i<=7;i++)draw.AddRect(at-new float2(i,i),end+new float2(i,i),S.Alpha(rarity,.1f*(1-i/8f)),thickness:2);draw.AddRectFilled(at+new float2(0,10),end+new float2(0,14),new ImColor8(0,0,0,70));}
        else draw.AddRectFilled(at+new float2(0,2),end+new float2(0,4),new ImColor8(0,0,0,40));
        draw.AddRectFilledMultiColor(at,end,new ImColor8(11,26,44),new ImColor8(11,26,44),S.Navy0,S.Navy0);
        draw.AddRectFilled(at+new float2(1,1),new(end.X-1,at.Y+3),hovered?rarity:S.Alpha(rarity,.7f));
        // .dts-prev: 120px radial preview
        var prevAt=at+new float2(11,11);var prevSize=new float2(inner,120);
        CollectionRadial(draw,prevAt,prevAt+prevSize,new(.5f,.35f),S.Mix(rarity,.3f,new(28,35,51)),new ImColor8(13,17,26),.85f);
        bool livePreview=locker&&previewCosmetic==item.Id&&kind is ("legend" or "board" or "dance" or "killscreen");
        draw.PushClipRect(prevAt,prevAt+prevSize,true);
        if(livePreview)
        {
            var restore=ImGui.GetCursorScreenPos();ImGui.SetCursorScreenPos(prevAt);
            CosmeticPreview(kind,item,collection,inner,120,true,bare:kind!="killscreen");
            ImGui.SetCursorScreenPos(restore);
        }
        else if(visible)StoreCardArt(collection,kind,item,prevAt,prevSize);
        draw.PopClipRect();
        if(CosmeticHit(key,prevAt,prevSize,out _,sound:false))
        {
            if(featured)CollectionCategoryChanged(Array.IndexOf(CosmeticCollection.Kinds,kind));
            previewCosmetic=item.Id;ClientItemSound("preview/"+kind+"/"+item.Id,true);
        }
        Buttons["preview/"+kind+"/"+item.Id]=prevAt+prevSize*.5f;
        if(kind is "sound" or "elimsound")Buttons["previewSound/"+kind+"/"+item.Id]=prevAt+prevSize*.5f;
        if(featured)
        {
            // .dts-feat .dts-card::before "-25%"
            var sale=S.Display(10.5f,.06f);float sw=sale.Width("-25%")+12;var sa=at+new float2(7,7);
            draw.AddRectFilledMultiColor(sa,sa+new float2(sw,17),new ImColor8(232,67,63),new ImColor8(232,67,63),new ImColor8(165,34,31),new ImColor8(165,34,31));
            draw.AddRect(sa,sa+new float2(sw,17),new ImColor8(255,138,122));S.TextIn(draw,sa+new float2(6,0),17,new ImColor8(255,255,255),"-25%",sale);
        }
        if(item.EggOnly&&!item.Free&&!owned)
        {
            var eo=S.Body(10,true,.05f);float ew=eo.Width("EGG ONLY")+14;var ea=new float2(end.X-14-ew,at.Y+14);
            draw.AddRectFilled(ea,ea+new float2(ew,17),new ImColor8(255,179,71),8.5f);
            S.TextIn(draw,ea+new float2(7,0),17,new ImColor8(26,18,6),"EGG ONLY",eo);
        }
        var nameType=S.Body(14,true);var rarType=S.Body(11,true,1/11f);var descType=S.Body(12);
        float y=prevAt.Y+120+4;
        y+=S.Paragraph(draw,new(prevAt.X,y),S.Gold1,item.Name,nameType,inner,1.35f)+4;
        y+=S.Paragraph(draw,new(prevAt.X,y),rarity,StoreRarityLine(kind,item,featured),rarType,inner,1.35f);
        if(item.Desc is not null){y+=4;y+=S.Paragraph(draw,new(prevAt.X,y),StoreMuted,item.Desc,descType,inner,1.35f);}
        if(kind=="legend"&&owned)
        {
            var evolution=LegendEvolution.Inspect(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"),profile.Storage.GetValueOrDefault("dt.cosmetics.v1"),profile.Storage.GetValueOrDefault("dt.farm.v1"),item.Id);
            var form=LegendEvolution.Form(evolution.Form);var small=S.Body(12);var smallBold=S.Body(12,true);
            y+=4+2;float lx=prevAt.X;
            lx+=S.TextIn(draw,new(lx,y),16.2f,S.Accent,"Lv "+evolution.Level,smallBold)+6;
            string note=evolution.Form!=item.Id?$"Now {form.Name} ({form.Level})":evolution.Next is {} next?$"Digivolves at Lv {next}":"";
            float noteWidth=S.TextFit(draw,new(lx,y+(16.2f-small.Size)*.5f),StoreMuted,note,small,prevAt.X+inner-lx-(evolution.CanDigivolve?50:0));
            if(evolution.CanDigivolve)S.TextIn(draw,new(lx+noteWidth+6,y),16.2f,new ImColor8(255,207,74),"Ready!",S.Body(12,true));
            y+=16.2f+4+4;
            var buttonType=S.Body(12,true);
            if(CosmeticButton("evolution/"+item.Id,new(prevAt.X,y),new(locker?(inner-8)/2:inner,25),"Digivolve",buttonType,ink:evolution.CanDigivolve?new ImColor8(255,207,74):null))
            {evolutionLegend=item.Id;collectionMessage=null;}
            if(evolution.CanDigivolve)draw.AddRect(new(prevAt.X,y),new(prevAt.X+(locker?(inner-8)/2:inner),y+25),new ImColor8(255,207,74));
            if(locker&&CosmeticButton("collectionFarm/"+item.Id,new(prevAt.X+(inner+8)/2,y),new((inner-8)/2,25),"Digi-Farm",buttonType))OpenCollectionFarm(collection,item.Id);
        }
        StoreCardAction(collection,kind,item,new(prevAt.X,end.Y-11-StoreButtonHeight),inner,featured,locker,owned,equipped);
        var border=hovered?S.Gold2:equipped?S.Gold1:S.Mix(rarity,.3f,S.Gold5);
        draw.AddRect(at,end,border);
        if(locker&&previewCosmetic==item.Id)draw.AddRect(at,end,S.Gold2);
        if(hovered&&featured)ImGui.SetTooltip(item.Name+" / "+CollectionNames[Array.IndexOf(CosmeticCollection.Kinds,kind)]);
    }
    private void StoreCardAction(CosmeticCollection collection,string kind,CosmeticItem item,float2 at,float width,bool featured,bool locker,bool owned,bool equipped)
    {
        var draw=ImGui.GetWindowDrawList();var type=S.Body(13.33f,true);var extent=new float2(width,StoreButtonHeight);
        if(locker&&owned)
        {
            bool multi=CosmeticCollection.Multi(kind);
            if(multi||!equipped)
            {
                // button.primary: orange gradient with dark ink.
                bool pressed=CosmeticHit("equip/"+kind+"/"+item.Id,at,extent,out bool hover);
                draw.AddRectFilledMultiColor(at,at+new float2(width,extent.Y*.3f),new ImColor8(255,210,122),new ImColor8(255,210,122),S.Brand2,S.Brand2);
                draw.AddRectFilledMultiColor(at+new float2(0,extent.Y*.3f),at+extent,S.Brand2,S.Brand2,S.Brand,S.Brand);
                draw.AddRect(at,at+extent,new ImColor8(240,210,140));if(hover)draw.AddRect(at-new float2(1,1),at+extent+new float2(1,1),new ImColor8(255,170,70,150));
                string label=multi?"Put in slot "+(collectionSlot+1):"Equip";S.TextIn(draw,new(at.X+(width-type.Width(label))*.5f,at.Y),extent.Y,S.TextInv,label,type);
                if(pressed&&ChangeCollection(c=>c.Equip(kind,item.Id,collectionSlot),"Equipped "+item.Name+"."))
                {
                    if(multi)collectionSlot=(collectionSlot+1)%6;
                    audio.PlayClient("equip",false);
                }
            }
            else CosmeticButton("equip/"+kind+"/"+item.Id,at,extent,"Equipped",type,disabled:true);
            return;
        }
        string buyKey=(featured?"featuredBuy/":"buyCosmetic/")+kind+"/"+item.Id;
        if(equipped)
        {
            // .dts-eq: disabled at full opacity with a gold frame and gold text.
            CosmeticHit(buyKey,at,extent,out _,sound:false);
            S.Secondary(draw,at,at+extent,false);draw.AddRect(at,at+extent,new ImColor8(255,207,74));
            S.TextIn(draw,new(at.X+(width-type.Width("Equipped"))*.5f,at.Y),extent.Y,new ImColor8(255,207,74),"Equipped",type);
        }
        else if(owned)
        {
            if(CosmeticButton("equip/"+kind+"/"+item.Id,at,extent,(item.Free?"Free":"Owned")+" · Equip",type))
                if(ChangeCollection(c=>c.Equip(kind,item.Id,collectionSlot),"Equipped "+item.Name+"."))audio.PlayClient("equip",false);
            Buttons[buyKey]=at+extent*.5f;
        }
        else if(item.EggOnly&&!item.Free)
        {
            if(CosmeticButton("storeEgg/"+kind+"/"+item.Id,at,extent,"Egg only · See eggs",type,ink:new ImColor8(255,207,74))){collectionStoreEggs=true;collectionResetScroll=true;}
            draw.AddRect(at,at+extent,S.Mix(new ImColor8(255,207,74),.55f,new ImColor8(255,207,74,0)));
        }
        else
        {
            var day=HomeDay;int price=CosmeticCollection.Price(kind,item,day);bool poor=price>collection.Bits;
            ImGui.BeginDisabled(poor);
            bool pressed=CosmeticHit(buyKey,at,extent,out bool hover);
            ImGui.EndDisabled();
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha,ImGui.GetStyle().Alpha*(poor?.6f:1));
            if(poor)S.Secondary(draw,at,at+extent,false);
            else
            {
                draw.AddRectFilledMultiColor(at,at+extent,new ImColor8(11,58,74),new ImColor8(11,58,74),new ImColor8(6,32,51),new ImColor8(6,32,51));
                draw.AddRect(at,at+extent,hover?S.Gold1:S.Gold3);
                if(hover)for(int i=1;i<=5;i++)draw.AddRect(at-new float2(i,i),at+extent+new float2(i,i),new ImColor8(63,214,255,(byte)(18*(1-i/6f))),thickness:2);
            }
            var ink=poor?S.LcText:new ImColor8(205,250,250);
            float priceWidth=S.Price(draw,default,extent.Y,price,price<item.Price?item.Price:null,ink,14,true);
            S.Price(draw,new(at.X+(width-priceWidth)*.5f,at.Y),extent.Y,price,price<item.Price?item.Price:null,S.Faded(ink),14);
            ImGui.PopStyleVar();
            if(pressed&&!poor)purchase=(kind,item.Id,day);
        }
    }
    /// <summary>The store card's item.preview(el) output inside its 120px .dts-prev.</summary>
    private void StoreCardArt(CosmeticCollection collection,string kind,CosmeticItem item,float2 at,float2 extent)
    {
        var draw=ImGui.GetWindowDrawList();
        switch(kind)
        {
            case "legend":portraits.DrawCompanion(CurrentPreviewLegend(collection,item.Id),at+new float2((extent.X-extent.Y)/2,0),extent.Y);break;
            case "emote":{float side=Math.Min(extent.X,120);emoteArtwork.Draw(item.Id,(long)(boardSceneTime*1000),at+(extent-new float2(side,side))*.5f,side);break;}
            case "killscreen":
                collectionTheme.Draw(item.Id,false,boardSceneTime%3.5,at,extent);
                portraits.DrawCompanion(CurrentPreviewLegend(collection),at+new float2(extent.X*.5f-34,extent.Y*.5f-40),68);
                break;
            case "dance":
                if(!StoreDanceThumbnail(collection,item,at,extent))CollectionIcon("dance",item.Id,at+extent*.5f,54);
                break;
            case "sound":
            {
                float w=64,h=32;var b=at+(extent-new float2(w,h))*.5f;
                bool play=CosmeticHit("gridSound/sound/"+item.Id,b,new(w,h),out bool hover,sound:false);
                S.Secondary(draw,b,b+new float2(w,h),hover);
                CollectionPlayGlyph(draw,b+new float2(12,h*.5f),12,hover?S.Gold1:S.LcText);
                CollectionIcon("sound",item.Id,b+new float2(42,h*.5f),16);
                if(play){previewCosmetic=item.Id;audio.PlayBoard(item.Data.TryGetProperty("clip"u8,out var clip)?clip.GetString()!:item.Id);}
                break;
            }
            case "elimsound":
            {
                var playType=S.Body(13,true);float w=playType.Width("Play")+40,h=26,total=30+6+h;float top=at.Y+(extent.Y-total)*.5f;
                CollectionIcon("elimsound",item.Id,new(at.X+extent.X*.5f,top+15),30);
                var b=new float2(at.X+(extent.X-w)*.5f,top+36);
                bool play=CosmeticHit("gridSound/elimsound/"+item.Id,b,new(w,h),out bool hover,sound:false);
                S.Secondary(draw,b,b+new float2(w,h),hover);
                var ink=hover?S.Gold1:S.LcText;CollectionPlayGlyph(draw,b+new float2(13,h*.5f),10,ink);S.TextIn(draw,b+new float2(28,0),h,ink,"Play",playType);
                if(play){previewCosmetic=item.Id;audio.PlayEliminationSound(item.Id,true,true,0);}
                break;
            }
            default:
                if(CollectionArt.Value.TryGetValue(kind+"/"+item.Id,out string? file))collectionArtwork.Draw(file,at,extent,255,kind=="board");
                break;
        }
    }
}

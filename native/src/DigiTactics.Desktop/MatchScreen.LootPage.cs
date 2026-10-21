using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Storage;
using S = DigiTactics.Desktop.CosmeticStyle;

namespace DigiTactics.Desktop;

/// <summary>
/// Loot page from v1.0.14 client.js RENDER.loot and theme.js .loot: art stage with vignette, material chips,
/// the rotating altar ring with the selected egg or legend, the .altar-info card, the data help band and
/// the .loot-inv sidebar (mystery boxes, egg and legend tiles).
/// </summary>
internal sealed partial class MatchScreen
{
    private static readonly ImColor8 LootText=new(180,194,213);

    private void IncubatorControls(CosmeticCollection collection)
    {
        IncubatorContentsExpanded=false;IncubatorContentRows=0;IncubatorLastContentBounds=null;
        var saved=(profile!.Storage.GetValueOrDefault("dt.cosmetics.v1"),profile.Storage.GetValueOrDefault("dt.legendevo.v1"),profile.Storage.GetValueOrDefault("dt.farm.v1"));
        if(incubator is null||incubatorSave!=saved){incubator=IncubatorOverview.Read(saved.Item1,saved.Item2,saved.Item3);incubatorSave=saved;}
        if(incubatorSelection is not {} selected||(selected.Legend?!incubator.Legends.Any(legend=>legend.Item.Id==selected.Id):!incubator.Eggs.Any(egg=>egg.Definition.Id==selected.Id)))
        {
            var ready=incubator.Legends.FirstOrDefault(legend=>legend.Evolution.CanDigivolve);
            incubatorSelection=ready is not null?(true,ready.Item.Id):(false,incubator.Eggs[0].Definition.Id);
        }
        var origin=ImGui.GetCursorScreenPos();var full=ImGui.GetContentRegionAvail();
        // .loot: minmax(0,1fr) clamp(280px,26vw,380px), 12px gap.
        float inventoryWidth=S.Narrow?290:Math.Clamp(ImGui.GetIO().DisplaySize.X*.26f,280,380),stageWidth=full.X-inventoryWidth-12;
        ImGui.PushStyleColor(ImGuiCol.ChildBg,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(0,0,0,0));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(0,0));
        try
        {
            ImGui.BeginChild("Incubator detail"u8,new(stageWidth,full.Y),ImGuiChildFlags.None,ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse|ImGuiWindowFlags.NoBackground);
            try{LootStage(collection,stageWidth,full.Y);}
            finally{ImGui.EndChild();}
            ImGui.SetCursorScreenPos(origin+new float2(stageWidth+12,0));
            ImGui.BeginChild("Incubator inventory"u8,new(inventoryWidth,full.Y),ImGuiChildFlags.None,ImGuiWindowFlags.NoBackground|ImGuiWindowFlags.NoScrollbar);
            try{LootInventory(collection,inventoryWidth);}
            finally{ImGui.EndChild();}
        }
        finally{ImGui.PopStyleVar(2);ImGui.PopStyleColor(2);}
    }
    private void LootStage(CosmeticCollection collection,float width,float height)
    {
        var at=ImGui.GetWindowPos();var draw=ImGui.GetWindowDrawList();var end=at+new float2(width,height);
        collectionArtwork.Draw("loot-bg",at,new(width,height),255,true);
        // .loot-stage::before: radial-gradient(ellipse at 50% 55%, rgba(1,10,19,.15), rgba(1,10,19,.85) 75%)
        CollectionRadial(draw,at,end,new(.5f,.55f),new ImColor8(1,10,19,38),new ImColor8(1,10,19,217),.75f,height*.55f/(width*.5f));
        draw.AddRect(at,end,S.Gold4);
        // .loot-head
        var eyebrow=S.Display(11,.16f);var title=S.Display(26,.04f);
        // .loot-head padding: 16px 24px, or 12px 16px at <=820px tall
        float headX=S.Short?16:24,headY=S.Short?12:16;
        S.TextIn(draw,at+new float2(headX,headY),14.85f,S.Gold2,"LOOT",eyebrow);
        S.TextIn(draw,at+new float2(headX,headY+14.85f+2),28.6f,S.Gold1,"INCUBATOR",title);
        string bits=collection.Bits.ToString("N0");
        float x=at.X+width-headX;
        x-=LootMaterial(null,bits,"Bits",new(x,at.Y+headY+1),true)+8;
        x-=LootMaterial("loot-chest",incubator!.Ready.ToString(),"Ready to digivolve",new(x,at.Y+headY+1),true)+8;
        LootMaterial("loot-shard",incubator.DigiData.ToString("N0"),"Digi-Data · Legend XP",new(x,at.Y+headY+1),true);
        // .loot-data-help band at the bottom
        var help=S.Body(12);string helpText="Digi-Data is XP for each Little Legend. Earn it through matches, duplicate legend hatches and Digi-Farm care to level up and unlock digivolution. Bits are your spending currency.";
        float helpHeight=S.ParagraphHeight(helpText,help,width-48,1.6f)+20;
        var helpAt=new float2(at.X+1,end.Y-1-helpHeight);
        draw.AddRectFilled(helpAt,new(end.X-1,end.Y-1),new ImColor8(11,21,33,221));
        S.Paragraph(draw,helpAt+new float2(23,10),LootText,helpText,help,width-48,1.6f);
        // .altar: two equal columns, 24px gap, 0 32px 24px padding, vertically centred
        // .altar: equal columns, 24px gap, 0 32px 24px padding; .9fr/1.1fr, 16px gap and padding at <=1380px.
        float altarPad=S.Narrow?16:32,altarGap=S.Narrow?16:24,altarBottom=S.Narrow?16:24;
        float top=at.Y+headY+55+headY,bottom=helpAt.Y-altarBottom;float usable=width-altarPad*2-altarGap;
        float column=S.Narrow?usable*.45f:usable*.5f,infoColumn=usable-column;
        var selection=incubatorSelection!.Value;
        float itemSide=Math.Min(Math.Min(column,380),bottom-top);
        var itemAt=new float2(at.X+altarPad+(column-itemSide)*.5f,top+(bottom-top-itemSide)*.5f);
        LootAltarItem(collection,selection,itemAt,itemSide);
        float infoHeight=Math.Min(bottom-top,incubatorInformationHeight>0?incubatorInformationHeight:bottom-top);
        var infoAt=new float2(at.X+altarPad+column+altarGap,top+(bottom-top-infoHeight)*.5f);
        draw.AddRectFilled(infoAt,infoAt+new float2(infoColumn,infoHeight),new ImColor8(1,10,19,209));draw.AddRect(infoAt,infoAt+new float2(infoColumn,infoHeight),S.Gold4);
        ImGui.SetCursorScreenPos(infoAt);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(24,16));ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(8,8));ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize,6);
        ImGui.BeginChild("Incubator altar information"u8,new(infoColumn,infoHeight),ImGuiChildFlags.AlwaysUseWindowPadding,ImGuiWindowFlags.NoBackground);
        ImGui.PopStyleVar(3);
        IncubatorInformationBounds=(ImGui.GetWindowPos(),ImGui.GetWindowPos()+ImGui.GetWindowSize());
        try
        {
            if(selection.Legend)IncubatorLegendControls(collection,incubator.Legends.Single(legend=>legend.Item.Id==selection.Id));
            else IncubatorEggControls(collection,incubator.Eggs.Single(egg=>egg.Definition.Id==selection.Id));
        }
        finally
        {
            incubatorInformationHeight=ImGui.GetCursorPosY()+16;
            IncubatorInformationScroll=ImGui.GetScrollY();IncubatorInformationScrollMax=ImGui.GetScrollMaxY();
            ImGui.EndChild();
        }
    }
    /// <summary>.mat chip: 34px art over two rows (value, label), or the Bits column (gem, value, label). Draws right-aligned; returns its width.</summary>
    private float LootMaterial(string? art,string value,string label,float2 rightTop,bool rightAligned)
    {
        var draw=ImGui.GetWindowDrawList();var big=S.Display(16);var small=S.Body(11);const float height=55;
        float textWidth=Math.Max(big.Width(value),small.Width(label));
        float width=art is null?8+textWidth+12:8+34+8+textWidth+12;
        var at=new float2(rightTop.X-width,rightTop.Y);
        draw.AddRectFilled(at,at+new float2(width,height),new ImColor8(1,10,19,217));draw.AddRect(at,at+new float2(width,height),S.Gold5);
        if(art is null)
        {
            S.BitsGem(draw,at+new float2(8+8,13));
            S.TextIn(draw,at+new float2(8,20),16,S.Gold1,value,big);
            S.TextIn(draw,at+new float2(8,36),14.85f,S.LcMute,label,small);
        }
        else
        {
            collectionArtwork.Draw(art,at+new float2(8,(height-34)*.5f),new(34,34));
            S.TextIn(draw,at+new float2(50,height*.5f-16),16,S.Gold1,value,big);
            S.TextIn(draw,at+new float2(50,height*.5f+1),14.85f,S.LcMute,label,small);
        }
        return width;
    }
    /// <summary>.altar-item: gold ring with a slowly rotating cyan/orange conic band, inner cyan glow and the bobbing egg or legend.</summary>
    private void LootAltarItem(CosmeticCollection collection,(bool Legend,string Id) selection,float2 at,float side)
    {
        var draw=ImGui.GetWindowDrawList();var centre=at+new float2(side,side)*.5f;
        float outer=side*.46f,inner=outer*.55f;double spin=ClientMotion?boardSceneTime/12*Math.PI*2:0;
        // conic-gradient stops (fractions of a turn) masked to the outer annulus
        const int segments=96;
        for(int i=0;i<segments;i++)
        {
            double a0=i*Math.PI*2/segments,a1=(i+1)*Math.PI*2/segments;float turn=(float)((i+.5)/segments);
            ImColor8 color;float strength;
            static float Peak(float t,float centre)=>Math.Max(0,1-Math.Abs(t-centre)/.05f);
            float cyan=Math.Max(Peak(turn,.15f),Peak(turn,.85f)),orange=Peak(turn,.5f);
            if(cyan<=0&&orange<=0)continue;
            color=cyan>=orange?new ImColor8(63,214,255):new ImColor8(255,138,61);strength=Math.Max(cyan,orange)*.22f;
            double r0=a0+spin-Math.PI/2,r1=a1+spin-Math.PI/2;
            var c=S.Alpha(color,strength);
            var p0=centre+new float2((float)Math.Cos(r0),(float)Math.Sin(r0))*inner;var p1=centre+new float2((float)Math.Cos(r1),(float)Math.Sin(r1))*inner;
            var q0=centre+new float2((float)Math.Cos(r0),(float)Math.Sin(r0))*outer;var q1=centre+new float2((float)Math.Cos(r1),(float)Math.Sin(r1))*outer;
            draw.AddQuadFilled(p0,q0,q1,p1,c);
        }
        draw.AddCircle(centre,outer,new ImColor8(200,170,110,128),96,2);
        // ::after: inset 18% radial cyan glow, blurred
        float glow=side*.32f*(ClientMotion?(float)(1-.08*(.5-.5*Math.Cos(boardSceneTime*Math.PI*2/3))):1);
        for(int i=12;i>=1;i--)draw.AddCircleFilled(centre,glow*i/12f,new ImColor8(63,214,255,(byte)(4+2*(12-i)/12f)),48);
        float bob=ClientMotion?(float)(-3+3*Math.Cos(boardSceneTime*Math.PI*2/3)):0;
        if(selection.Legend)
        {
            var legend=incubator!.Legends.Single(legend=>legend.Item.Id==selection.Id);
            float size=side*.62f;var restore=ImGui.GetCursorScreenPos();
            ImGui.SetCursorScreenPos(centre-new float2(size,size)*.5f+new float2(0,bob));
            CosmeticPreview("legend",legend.Item,collection,size,size,bare:true);
            ImGui.SetCursorScreenPos(restore);
        }
        else
        {
            float size=side*.62f;var eggAt=centre-new float2(size,size)*.5f+new float2(0,bob);
            collectionArtwork.Draw("loot-egg-"+selection.Id,eggAt,new(size,size));
        }
    }
    private void LootInventory(CosmeticCollection collection,float width)
    {
        var at=ImGui.GetWindowPos();var extent=ImGui.GetWindowSize();var draw=ImGui.GetWindowDrawList();
        IncubatorInventoryBounds=(at,at+extent);
        CollectionOverlayScrollbar();
        var lo=at+new float2(0,ImGui.GetScrollY());
        draw.AddRectFilledMultiColor(lo,lo+extent,new ImColor8(10,20,40,235),new ImColor8(10,20,40,235),new ImColor8(1,10,19,245),new ImColor8(1,10,19,245));
        draw.AddRect(lo,lo+extent,S.Gold5);
        float x=at.X+13,inner=width-26,y=at.Y+13;
        y=MysteryLootControls(collection,new(x,y),inner);
        var tile=S.Display(12.5f,.1f);var count=S.Body(11);
        void Heading(string text,int n)
        {
            y+=4;S.TextIn(draw,new(x,y),18.9f,S.Gold1,text,tile);string c=n.ToString();S.TextIn(draw,new(x+inner-count.Width(c),y),18.9f,S.LcMute,c,count);y+=18.9f+8;
        }
        Heading("DIGI-EGGS",incubator!.Eggs.Length);
        int columns=Math.Max(1,(int)((inner+6)/82));float tileWidth=(inner-(columns-1)*6)/columns;const float tileHeight=89;
        for(int i=0;i<incubator.Eggs.Length;i++)
        {
            var egg=incubator.Eggs[i];var tileAt=new float2(x+i%columns*(tileWidth+6),y+i/columns*(tileHeight+6));
            bool chosen=LootTile("lootEgg/"+egg.Definition.Id,egg.Definition.Name.Replace(" Egg","").Replace("Digi-",""),egg.Definition.Price.ToString("N0"),tileAt,new(tileWidth,tileHeight),incubatorSelection==(false,egg.Definition.Id),false,true);
            collectionArtwork.Draw("loot-egg-"+egg.Definition.Id,tileAt+new float2((tileWidth-44)*.5f,7),new(44,44));
            if(chosen)incubatorSelection=(false,egg.Definition.Id);
            if(ImGui.IsItemHovered())ImGui.SetTooltip(egg.Definition.Name+$" / {egg.Owned} of {egg.Total} owned");
        }
        y+=(incubator.Eggs.Length+columns-1)/columns*(tileHeight+6)-6+12;
        Heading("LITTLE LEGENDS",incubator.Legends.Length);
        for(int i=0;i<incubator.Legends.Length;i++)
        {
            var legend=incubator.Legends[i];var evolution=legend.Evolution;var tileAt=new float2(x+i%columns*(tileWidth+6),y+i/columns*(tileHeight+6));
            bool chosen=LootTile("lootLegend/"+legend.Item.Id,LegendEvolution.Form(evolution.Form).Name,"Lv "+evolution.Level,tileAt,new(tileWidth,tileHeight),incubatorSelection==(true,legend.Item.Id),evolution.CanDigivolve,false);
            var c=tileAt+new float2(tileWidth*.5f,7+22);
            // .loot-item .pt: 44px circle, radial-gradient(circle at 50% 30%, #2a3a5c, #0a1428)
            for(int ring=0;ring<11;ring++){float t=ring/10f;draw.AddCircleFilled(c+new float2(0,-8.8f*t),22-21*t,S.Mix(new ImColor8(42,58,92),t,new ImColor8(10,20,40)),32);}
            portraits.DrawCompanion(CurrentPreviewLegend(collection,legend.Item.Id),c-new float2(22,22),44);
            if(chosen)incubatorSelection=(true,legend.Item.Id);
        }
        if(incubator.Legends.Length==0){S.TextIn(draw,new(x,y),18,S.Text3,"Owned Little Legends appear here.",S.Body(12.5f));y+=18;}
        else y+=(incubator.Legends.Length+columns-1)/columns*(tileHeight+6)-6;
        ImGui.SetCursorScreenPos(at);ImGui.Dummy(new(width,y-at.Y+13));
    }
    /// <summary>.loot-item: 44px art, bold name, small value (with a Bits gem for eggs); selected and ready frames.</summary>
    private bool LootTile(string key,string label,string sub,float2 at,float2 extent,bool selected,bool ready,bool bits)
    {
        bool clicked=CosmeticHit(key,at,extent,out bool hover,sound:false);ClientItemSound(key,clicked);
        var draw=ImGui.GetWindowDrawList();var lift=hover&&!selected?new float2(0,-1):new float2(0,0);var lo=at+lift;var hi=lo+extent;
        if(selected)for(int i=1;i<=7;i++)draw.AddRect(lo-new float2(1+i,1+i),hi+new float2(1+i,1+i),new ImColor8(240,210,140,(byte)(22*(1-i/8f))),thickness:2);
        else if(ready)for(int i=1;i<=6;i++)draw.AddRect(lo-new float2(i,i),hi+new float2(i,i),new ImColor8(79,224,143,(byte)(30*(1-i/7f))),thickness:2);
        draw.AddRectFilled(lo,hi,new ImColor8(1,10,19,191));
        draw.AddRect(lo,hi,selected?S.Gold1:ready?S.Success:hover?S.Gold3:S.Gold5);
        if(selected)draw.AddRect(lo-new float2(1,1),hi+new float2(1,1),S.Gold2);
        var name=S.Body(10.5f,true);var small=S.Body(10);
        string fitted=S.Fit(label,name,extent.X-8);S.TextIn(draw,new(lo.X+(extent.X-name.Width(fitted))*.5f,lo.Y+7+44+2),14.2f,S.Gold1,fitted,name);
        float subWidth=small.Width(sub)+(bits?14:0);float sx=lo.X+(extent.X-subWidth)*.5f,sy=lo.Y+7+44+2+14.2f+2;
        if(bits){S.BitsGem(draw,new(sx+6,sy+6.75f),9);sx+=14;}
        S.TextIn(draw,new(sx,sy),13.5f,S.LcMute,sub,small);
        return clicked;
    }
    private void IncubatorEggControls(CosmeticCollection collection,IncubatorEgg egg)
    {
        var draw=ImGui.GetWindowDrawList();float width=ImGui.GetContentRegionAvail().X;
        var at=ImGui.GetCursorScreenPos();float y=at.Y;
        S.TextIn(draw,new(at.X,y),14.85f,S.Gold2,"DIGI-EGG",S.Display(11,.16f));y+=14.85f+8;
        var h2=S.Display(S.Short?26:36,.04f);
        foreach(string line in CollectionNameLines(egg.Definition.Name.ToUpperInvariant(),h2,width)){S.TextIn(draw,new(at.X,y),h2.Size/1.3f,S.Gold1,line,h2);y+=h2.Size/1.3f;}
        y+=8;string description=egg.Definition.Id=="mixed"?"Any cosmetic from any category.":"A random "+CollectionNames[Array.IndexOf(CosmeticCollection.Kinds,egg.Definition.Kinds[0])].ToLowerInvariant()+" cosmetic.";
        y+=S.Paragraph(draw,new(at.X,y),S.Text2,description,S.Body(12.5f),width)+8;
        // .odds chips
        var odd=S.Body(11,true,.06f);float x=0;
        foreach(var o in egg.Odds)
        {
            string text=$"{o.Rarity.ToUpperInvariant()} {o.Pct:0.#}%";float chip=8+7+5+odd.Width(text)+8;
            if(x>0&&x+chip>width){x=0;y+=21+6;}
            var c=new float2(at.X+x,y);var color=S.Rarity(o.Rarity);
            draw.AddRectFilled(c,c+new float2(chip,21),new ImColor8(1,10,19,179));draw.AddRect(c,c+new float2(chip,21),S.Mix(color,.45f,new ImColor8(color.AsByte4().X,color.AsByte4().Y,color.AsByte4().Z,0)));
            S.Diamond(draw,c+new float2(8+3.5f,10.5f),7,color);S.TextIn(draw,c+new float2(20,0),21,color,text,odd);
            x+=chip+6;
        }
        y+=21+8+4;
        // .altar-acts: price and HATCH
        float priceWidth=S.Price(draw,new(at.X,y),41,egg.Definition.Price,null,S.Gold1,16);
        ImGui.BeginDisabled(collection.Bits<egg.Definition.Price||egg.Total==0);
        var hatchType=S.Display(14,.12f);float hatchWidth=hatchType.Width("HATCH")+52+4;
        if(CosmeticButton("hatchSelectedEgg",new(at.X+priceWidth+8,y),new(hatchWidth,41),"HATCH",hatchType,cta:true,disabled:collection.Bits<egg.Definition.Price||egg.Total==0))BeginIncubatorHatch(egg.Definition.Id);
        ImGui.EndDisabled();
        Buttons["egg/"+egg.Definition.Id]=Buttons["hatchSelectedEgg"];
        y+=41+6;
        // Native additions retained on one row: owned count and the contents disclosure.
        var ownedType=S.Body(12.5f);string owned=$"{egg.Owned} / {egg.Total} owned";
        S.TextIn(draw,new(at.X,y),18,StoreMuted,owned,ownedType);
        IncubatorExclusiveCount=egg.Exclusives.Length;
        var linkType=S.Display(11,.1f);string link=lootContentsOpen?"HIDE CONTENTS":"CONTENTS AND DUPLICATES";
        float lx=at.X+ownedType.Width(owned)+14;
        if(CosmeticHit("lootContents",new(lx,y),new(linkType.Width(link)+14,18),out bool linkHover))lootContentsOpen=!lootContentsOpen;
        var arrow=new float2(lx+4,y+9);var ink=linkHover?new ImColor8(205,250,250):S.Accent;
        if(lootContentsOpen)draw.AddTriangleFilled(arrow+new float2(-4,-2),arrow+new float2(4,-2),arrow+new float2(0,3),ink);else draw.AddTriangleFilled(arrow+new float2(-2,-4),arrow+new float2(-2,4),arrow+new float2(3,0),ink);
        S.TextIn(draw,new(lx+14,y),18,ink,link,linkType);y+=18;
        IncubatorContentsExpanded=lootContentsOpen;
        ImGui.SetCursorScreenPos(new(at.X,y));ImGui.Dummy(new(width,0));
        if(!IncubatorContentsExpanded)return;
        ImGui.PushFont(NativeUiTheme.BodyFont,S.Body(12.5f).Size);ImGui.PushStyleColor(ImGuiCol.Text,S.Text2);
        CollectionText("Rarity is rolled first, then each item of that rarity is equally likely. Duplicate legends grant Digi-Data until max level, then Bits. Other duplicates refund 40% of their item price.");
        ImGui.PopStyleColor();
        foreach(var exclusive in egg.Exclusives)
        {
            ImGui.PushStyleColor(ImGuiCol.Text,S.Rarity(exclusive.Item.Rarity));
            CollectionText($"{exclusive.Item.Name} / {exclusive.Chance}"+(exclusive.Owned?" / owned":""));
            IncubatorContentRows++;IncubatorLastContentBounds=(ImGui.GetItemRectMin(),ImGui.GetItemRectMax());
            ImGui.PopStyleColor();
        }
        ImGui.PopFont();
    }
    private bool lootContentsOpen;
    private void IncubatorLegendControls(CosmeticCollection collection,IncubatorLegend legend)
    {
        var evolution=legend.Evolution;var form=LegendEvolution.Form(evolution.Form);
        var draw=ImGui.GetWindowDrawList();float width=ImGui.GetContentRegionAvail().X;var at=ImGui.GetCursorScreenPos();float y=at.Y;
        S.TextIn(draw,new(at.X,y),14.85f,S.Gold2,"LITTLE LEGEND · "+form.Level.ToUpperInvariant(),S.Display(11,.16f));y+=14.85f+8;
        var h2=S.Display(S.Short?26:36,.04f);
        foreach(string line in CollectionNameLines(form.Name.ToUpperInvariant(),h2,width)){S.TextIn(draw,new(at.X,y),h2.Size/1.3f,S.Gold1,line,h2);y+=h2.Size/1.3f;}
        y+=8;
        S.TextIn(draw,new(at.X,y),19,StoreMuted,evolution.Need==0?"Max level":$"{evolution.Data}/{evolution.Need} Digi-Data",S.Body(13));
        var lvl=S.Display(11);string level="Lv "+evolution.Level;float lw=lvl.Width(level)+16;var lvlAt=new float2(at.X+width-lw,y+1);
        draw.AddRectFilled(lvlAt,lvlAt+new float2(lw,17),S.Navy0);draw.AddRect(lvlAt,lvlAt+new float2(lw,17),S.Gold3);S.TextIn(draw,lvlAt+new float2(8,0),17,S.Gold1,level,lvl);
        y+=19+8;S.Progress(draw,new(at.X,y),new(width,8),evolution.Need==0?1:(float)evolution.Data/evolution.Need);y+=8+8;
        string text=evolution.CanDigivolve?"Ready to digivolve. Pick its next form in the Digivolve panel.":evolution.Next is {} next?$"Digivolves at level {next}. Digi-Data comes from games played with it equipped, and from the Digi-Farm.":"Final form reached.";
        y+=S.Paragraph(draw,new(at.X,y),S.Text2,text,S.Body(12.5f),width)+8+4;
        var cta=S.Display(14,.12f);var secondary=S.Display(14,.08f);var ghost=S.Body(14,true);
        float x=at.X;
        float dw=(evolution.CanDigivolve?cta:secondary).Width("DIGIVOLVE")+(evolution.CanDigivolve?56:34);
        if(CosmeticButton("lootDigivolve",new(x,y),new(dw,evolution.CanDigivolve?41:37),"DIGIVOLVE",evolution.CanDigivolve?cta:secondary,cta:evolution.CanDigivolve)){ClearEvolutionPreview();evolutionLegend=legend.Item.Id;collectionMessage=null;}
        x+=dw+8;
        float LootGhost(string key,string label,float left)
        {
            float w=ghost.Width(label)+32;bool pressed=CosmeticHit(key,new(left,y),new(w,37),out bool hover);
            if(hover){draw.AddRectFilled(new(left,y),new(left+w,y+37),new ImColor8(200,170,110,20));draw.AddRect(new(left,y),new(left+w,y+37),S.Gold5);}
            S.TextIn(draw,new(left+16,y),37,hover?S.Gold1:S.LcMute,label,ghost);
            return pressed?w:-w;
        }
        float farm=LootGhost("lootFarm","Digi-Farm",x);if(farm>0)OpenCollectionFarm(collection,legend.Item.Id);x+=Math.Abs(farm)+8;
        if(collection.Equipped("legend")?.GetValue<string>()==legend.Item.Id)S.EquippedBadge(draw,new(x,y+9),20);
        else{float equip=LootGhost("lootEquip","Equip",x);if(equip>0&&ChangeCollection(next=>next.Equip("legend",legend.Item.Id),"Equipped "+legend.Item.Name+"."))audio.PlayClient("equip",false);}
        y+=41;
        ImGui.SetCursorScreenPos(new(at.X,y));ImGui.Dummy(new(width,0));
    }
}

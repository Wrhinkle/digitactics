using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;
using S = DigiTactics.Desktop.CosmeticStyle;

namespace DigiTactics.Desktop;

/// <summary>The Collection's .cprev-card (client.js previewPane): stage or big preview above a fixed info block with actions.</summary>
internal sealed partial class MatchScreen
{
    private void SelectedCosmetic(CosmeticCollection collection,string kind,bool fullHeight=false)
    {
        var item=CosmeticCollection.Item(kind,previewCosmetic);
        if(item is null || (item.Secret||MatchLockerVisible)&&!collection.Owns(kind,item.Id))
        {
            string? equipped=CosmeticCollection.Multi(kind)?collection.Equipped(kind)?.AsArray().FirstOrDefault(x=>x is not null)?.GetValue<string>():collection.Equipped(kind)?.GetValue<string>();
            item=CosmeticCollection.Item(kind,equipped)??CosmeticCollection.Items(kind).FirstOrDefault(x=>MatchLockerVisible?collection.Owns(kind,x.Id):!x.Secret||collection.Owns(kind,x.Id));
        }
        var origin=ImGui.GetCursorScreenPos();var extent=ImGui.GetContentRegionAvail();var draw=ImGui.GetWindowDrawList();
        if(item is null){CollectionEmpty("Nothing here yet.");return;}
        previewCosmetic=item.Id;
        var rarity=S.Rarity(item.Rarity);var end=origin+extent;
        // background: radial-gradient(420px 300px at 50% 30%, rarity 22%, transparent 70%), linear-gradient(#0b1a2c, #010a13)
        draw.AddRectFilledMultiColor(origin,end,new ImColor8(11,26,44),new ImColor8(11,26,44),S.Navy0,S.Navy0);
        CollectionEllipse(draw,origin,end,origin+new float2(extent.X*.5f,extent.Y*.3f),new(294,210),S.Alpha(rarity,.22f));
        bool owned=collection.Owns(kind,item.Id);
        float innerWidth=extent.X-34;
        var nameType=S.Display(26,.03f);var descType=S.Body(12.5f);
        var nameLines=CollectionNameLines(item.Name.ToUpperInvariant(),nameType,innerWidth);
        float descHeight=item.Desc is null?0:S.ParagraphHeight(item.Desc,descType,innerWidth);
        float actsHeight=CollectionPaneActions(collection,kind,item,default,innerWidth,true);
        // .cprev-top wraps: rarity, tag, kind label and the ownership badge flow onto extra rows when narrow.
        var muted=S.Body(14);string kindName=CollectionKindName(kind);
        string? status=owned?CollectionWorn(collection,kind,item.Id)?"Equipped":"Owned":null;
        var chips=new List<float>{S.BadgeWidth(S.RarityName(item.Rarity))};
        if(item.Tag is not null)chips.Add(S.BadgeWidth(item.Tag));
        chips.Add(muted.Width(kindName));
        if(status is not null)chips.Add(S.BadgeWidth(status));
        var chipAt=new List<float2>();float cx=0,cy=0;
        foreach(float w in chips){if(cx>0&&cx+w>innerWidth){cx=0;cy+=22+8;}chipAt.Add(new(cx,cy));cx+=w+8;}
        float topHeight=cy+22;
        float infoHeight=16+topHeight+8+nameLines.Count*27.3f+(item.Desc is null?0:8+descHeight)+8+4+actsHeight+16;
        float infoTop=Math.Max(origin.Y+170,end.Y-1-infoHeight);
        var stageAt=origin+new float2(1,1);var stageSize=new float2(extent.X-2,infoTop-origin.Y-1);
        CollectionPaneStage(collection,kind,item,stageAt,stageSize);
        // .cprev-info
        draw.AddRectFilled(new(origin.X+1,infoTop),end-new float2(1,1),new ImColor8(1,10,19,217));
        draw.AddLine(new(origin.X+1,infoTop+.5f),new(end.X-1,infoTop+.5f),S.Gold4);
        float x=origin.X+17,y=infoTop+17;
        int chip=0;var top=new float2(x,y);
        S.RarityBadge(draw,top+chipAt[chip++],item.Rarity,22);
        if(item.Tag is not null)S.Badge(draw,top+chipAt[chip++],item.Tag,S.LcText,new ImColor8(1,10,19,217),S.Gold4,22);
        S.TextIn(draw,top+chipAt[chip++],22,new ImColor8(143,154,176),kindName,muted);
        if(status=="Equipped")S.EquippedBadge(draw,top+chipAt[chip],22);
        else if(status=="Owned")S.OwnedBadge(draw,top+chipAt[chip],22);
        y+=topHeight+8;
        foreach(string line in nameLines){S.TextIn(draw,new(x,y),27.3f,S.Gold1,line,nameType);y+=27.3f;}
        if(item.Desc is not null){y+=8;S.Paragraph(draw,new(x,y),S.Text2,item.Desc,descType,innerWidth);y+=descHeight;}
        y+=12;
        CollectionPaneActions(collection,kind,item,new(x,y),innerWidth,false);
        draw.AddRect(origin,end,S.Gold4);
        ImGui.SetCursorScreenPos(origin);ImGui.Dummy(extent);
    }
    private static List<string> CollectionNameLines(string name,S.Type type,float width)
    {
        // overflow-wrap: anywhere; words wrap first, an overlong word breaks between letters.
        var lines=new List<string>();
        foreach(string line in S.Wrap(name,type,width))
        {
            string rest=line;
            while(type.Width(rest)>width&&rest.Length>1)
            {
                int count=1;while(count<rest.Length&&type.Width(rest[..(count+1)])<=width)count++;
                lines.Add(rest[..count]);rest=rest[count..];
            }
            lines.Add(rest);
        }
        return lines;
    }
    private static void CollectionEllipse(ImDrawListPtr draw,float2 lo,float2 hi,float2 centre,float2 radius,ImColor8 inner)
    {
        draw.PushClipRect(lo,hi,true);
        var flags=draw.Flags;draw.Flags&=~ImDrawListFlags.AntiAliasedFill;
        int first=draw.VtxBuffer.Count;const int segments=48;
        for(int i=0;i<segments;i++)
        {
            double a=i*Math.PI*2/segments,b=(i+1)*Math.PI*2/segments;
            draw.AddTriangleFilled(centre,centre+new float2((float)Math.Cos(a)*radius.X,(float)Math.Sin(a)*radius.Y),centre+new float2((float)Math.Cos(b)*radius.X,(float)Math.Sin(b)*radius.Y),inner);
        }
        var c=inner.AsByte4();RadialRecolor(draw,first,centre,radius.X,inner,new ImColor8(c.X,c.Y,c.Z,0),radius.Y/radius.X);
        draw.Flags=flags;draw.PopClipRect();
    }
    /// <summary>.cprev-stage (legend pedestal) or .cprev-big (12px padded centred preview).</summary>
    private void CollectionPaneStage(CosmeticCollection collection,string kind,CosmeticItem item,float2 at,float2 extent)
    {
        var draw=ImGui.GetWindowDrawList();
        var restore=ImGui.GetCursorScreenPos();
        switch(kind)
        {
            case "legend" or "dance" or "killscreen" or "emote":
                ImGui.SetCursorScreenPos(at);
                draw.PushClipRect(at,at+extent,true);
                CosmeticPreview(kind,item,collection,extent.X,extent.Y,bare:true);
                draw.PopClipRect();
                if(kind=="killscreen")CollectionKillPlay(collection,item,at,extent);
                break;
            case "board":
            {
                float width=extent.X,height=Math.Min(extent.Y,width*CosmeticScenePreview.Height/CosmeticScenePreview.Width);
                ImGui.SetCursorScreenPos(at+new float2(0,(extent.Y-height)*.5f));
                CosmeticPreview(kind,item,collection,width,height,bare:true);
                break;
            }
            case "sound":
            {
                float w=64,h=32;var b=at+(extent-new float2(w,h))*.5f;
                bool play=CosmeticHit("previewSound/sound/"+item.Id,b,new(w,h),out bool hover,sound:false);
                S.Secondary(draw,b,b+new float2(w,h),hover);
                CollectionPlayGlyph(draw,b+new float2(12,h*.5f),12,hover?S.Gold1:S.LcText);
                CollectionIcon("sound",item.Id,b+new float2(42,h*.5f),16);
                if(play)audio.PlayBoard(item.Data.TryGetProperty("clip"u8,out var clip)?clip.GetString()!:item.Id);
                CollectionPreviewKind=kind;CollectionPreviewDrawCalls=1;
                break;
            }
            case "elimsound":
            {
                var playType=S.Body(18,true);float w=playType.Width("Play")+52+24,h=44,total=96+6+h;
                float top=at.Y+(extent.Y-total)*.5f;
                CollectionIcon("elimsound",item.Id,new(at.X+extent.X*.5f,top+48),96);
                var b=new float2(at.X+(extent.X-w)*.5f,top+102);
                bool play=CosmeticHit("previewSound/elimsound/"+item.Id,b,new(w,h),out bool hover,sound:false);
                S.Secondary(draw,b,b+new float2(w,h),hover);
                var ink=hover?S.Gold1:S.LcText;CollectionPlayGlyph(draw,b+new float2(26,h*.5f),14,ink);S.TextIn(draw,b+new float2(26+24,0),h,ink,"Play",playType);
                if(play)audio.PlayEliminationSound(item.Id,true,true,0);
                CollectionPreviewKind=kind;CollectionPreviewDrawCalls=1;
                break;
            }
        }
        ImGui.SetCursorScreenPos(restore);
    }
    /// <summary>killscreens.js preview's top-right replay chip: plays the theme with the equipped elimination sound.</summary>
    private void CollectionKillPlay(CosmeticCollection collection,CosmeticItem item,float2 at,float2 extent)
    {
        var draw=ImGui.GetWindowDrawList();var type=S.Body(12.5f,true);
        var sound=CosmeticCollection.Item("elimsound",collection.Equipped("elimsound")?.GetValue<string>());
        string label=sound?.Name??"Play";bool icon=sound is not null&&CollectionArt.Value.ContainsKey("elimsound/"+sound.Id);
        float w=12+12+6+(icon?18:0)+type.Width(label)+12,h=26;var b=new float2(at.X+extent.X-w-8,at.Y+8);
        bool play=CosmeticHit("previewKill/"+item.Id,b,new(w,h),out bool hover,sound:false);
        draw.AddRectFilled(b,b+new float2(w,h),new ImColor8(1,10,19,(byte)(hover?240:217)));draw.AddRect(b,b+new float2(w,h),hover?S.Gold1:new ImColor8(220,225,235,150));
        CollectionPlayGlyph(draw,b+new float2(12,h*.5f),10,S.Text1);
        float tx=b.X+30;
        if(icon){CollectionIcon("elimsound",sound!.Id,new(tx+7,b.Y+h*.5f),14);tx+=18;}
        S.TextIn(draw,new(tx,b.Y),h,S.Text1,label,type);
        if(play)
        {
            collectionPreviewStarted=boardSceneTime;audio.PlayEliminationTheme(item.Id,true);
            audio.PlayEliminationSound(collection.Equipped("elimsound")?.GetValue<string>(),true,true,.3);
        }
    }
    /// <summary>.cprev-acts; with <paramref name="measure"/> only returns the height.</summary>
    private float CollectionPaneActions(CosmeticCollection collection,string kind,CosmeticItem item,float2 at,float width,bool measure)
    {
        var draw=ImGui.GetWindowDrawList();var cta=S.Display(14,.12f);var secondary=S.Display(14,.08f);var small=S.Body(12.5f);
        bool owned=collection.Owns(kind,item.Id);float y=at.Y;
        void Note(string text){if(!measure)S.Paragraph(draw,new(at.X,y),new ImColor8(143,154,176),text,small,width,1.35f);y+=S.ParagraphHeight(text,small,width,1.35f);}
        if(owned&&CosmeticCollection.Multi(kind))
        {
            // .slot-h and the 3-column .slots grid; each slot assigns this item directly.
            var head=S.Body(11,true,.08f);var mute=S.Body(11);
            if(!measure){float w=S.Text(draw,new(at.X,y+(14.85f-head.Size)*.5f),S.Text3,"WHEEL SLOTS",head);S.TextIn(draw,new(at.X+w+4,y),14.85f,new ImColor8(143,154,176),"pick a slot for "+item.Name,S.Body(13));}
            y+=14.85f+8;
            var equipped=collection.Equipped(kind)!.AsArray();float slotWidth=(width-12)/3;const float slotHeight=44;
            for(int i=0;i<6&&!measure;i++)
            {
                var slotAt=new float2(at.X+i%3*(slotWidth+6),y+i/3*(slotHeight+6));
                string? id=equipped.ElementAtOrDefault(i)?.GetValue<string>();var worn=CosmeticCollection.Item(kind,id);bool current=id==item.Id;
                if(CosmeticHit("collectionSlot/"+i,slotAt,new(slotWidth,slotHeight),out bool hover))
                {
                    collectionSlot=i;
                    if(ChangeCollection(next=>next.Equip(kind,item.Id,collectionSlot),item.Name+" is in slot "+(collectionSlot+1)+"."))audio.PlayClient("equip",false);
                }
                S.Secondary(draw,slotAt,slotAt+new float2(slotWidth,slotHeight),hover);
                if(current){draw.AddRect(slotAt,slotAt+new float2(slotWidth,slotHeight),S.Gold1);draw.AddRect(slotAt+new float2(1,1),slotAt+new float2(slotWidth-1,slotHeight-1),S.Gold2);}
                S.Text(draw,slotAt+new float2(9,7),S.Text3,(i+1).ToString(),S.Display(11));
                S.TextFit(draw,slotAt+new float2(9,23),hover?S.Gold1:S.LcText,worn?.Name??"Empty",S.Body(11,true),slotWidth-18);
                if(hover)ImGui.SetTooltip("Put it in slot "+(i+1));
            }
            y+=slotHeight*2+6;
            // Native addition kept from the restoration work: empty the selected slot.
            if(!measure)
            {
                Buttons["equip/"+kind+"/"+item.Id]=Buttons["collectionSlot/"+collectionSlot];
                var link=S.Display(11,.1f);string text="EMPTY SLOT "+(collectionSlot+1);bool filled=equipped.ElementAtOrDefault(collectionSlot) is not null;
                ImGui.BeginDisabled(!filled);
                bool clear=CosmeticHit("clearCollectionSlot",new(at.X,y+8),new(link.Width(text),16),out bool hover);
                ImGui.EndDisabled();
                S.TextIn(draw,new(at.X,y+8),16,filled?(hover?new ImColor8(205,250,250):S.Accent):S.Text3,text,link);
                if(clear&&filled)ChangeCollection(next=>next.Equip(kind,null,collectionSlot),"Slot cleared.");
            }
            y+=24;
            return y-at.Y;
        }
        if(owned)
        {
            bool worn=CollectionWorn(collection,kind,item.Id,true);
            if(worn){if(!measure)CosmeticButton("equip/"+kind+"/"+item.Id,new(at.X,y),new(width,35),"EQUIPPED",secondary,disabled:true);y+=35;}
            else
            {
                if(!measure&&CosmeticButton("equip/"+kind+"/"+item.Id,new(at.X,y),new(width,41),"EQUIP",cta,cta:true))
                    if(ChangeCollection(c=>c.Equip(kind,item.Id,collectionSlot),"Equipped "+item.Name+"."))audio.PlayClient("equip",false);
                y+=41;
            }
            if(kind=="legend")y+=8+CollectionPaneEvolution(collection,item,new(at.X,y+8),width,measure);
        }
        else if(item.Linked is {Length:2} link)
        {
            var source=CosmeticCollection.Item(link[0],link[1]);
            int price=source is null?0:CosmeticCollection.Price(link[0],source,DateOnly.FromDateTime(DateTime.Now));
            if(!measure&&CollectionPriceButton("linkedCosmeticStore",new(at.X,y),width,"GET THE "+CollectionKindName(link[0]).ToUpperInvariant()+" CLIP ·",price))
                OpenCollectionPage(store:true,kind:link[0],item:link[1]);
            y+=41+8;Note("Owning it on the soundboard unlocks it here too.");
        }
        else if(item.EggOnly)
        {
            if(!measure&&CosmeticButton("collectionItemLoot",new(at.X,y),new(width,41),"HATCH FROM DIGI-EGGS",cta,cta:true))showEggs=true;
            y+=41+8;Note("Rare and up Little Legends only come from eggs.");
        }
        else
        {
            DateOnly day=DateOnly.FromDateTime(DateTime.Now);int price=CosmeticCollection.Price(kind,item,day);
            ImGui.BeginDisabled(price>collection.Bits);
            if(!measure&&CollectionPriceButton("buyCosmetic/"+kind+"/"+item.Id,new(at.X,y),width,"BUY ·",price,price>collection.Bits))purchase=(kind,item.Id,day);
            ImGui.EndDisabled();
            y+=41;
            if(price>collection.Bits){y+=8;Note("Not enough Bits.");}
        }
        return y-at.Y;
    }
    private bool CollectionPriceButton(string key,float2 at,float width,string label,int price,bool disabled=false)
    {
        var type=S.Display(14,.12f);var draw=ImGui.GetWindowDrawList();
        bool pressed=CosmeticButton(key,at,new(width,41),"",type,cta:true,disabled:disabled);
        float priceWidth=S.Price(draw,default,41,price,null,S.Text1,14,true);float labelWidth=type.Width(label);
        float x=at.X+(width-labelWidth-6-priceWidth)*.5f;
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha,ImGui.GetStyle().Alpha*(disabled?.45f:1));
        S.TextIn(draw,new(x,at.Y),41,S.Faded(new ImColor8(205,250,250)),label,type);
        S.Price(draw,new(x+labelWidth+6,at.Y),41,price,null,new ImColor8(205,250,250),14);
        ImGui.PopStyleVar();
        return pressed;
    }
    /// <summary>.evo-box: form and level, Digi-Data bar, next step, Digivolve and Digi-Farm.</summary>
    private float CollectionPaneEvolution(CosmeticCollection collection,CosmeticItem item,float2 at,float width,bool measure)
    {
        var evolution=LegendEvolution.Inspect(profile!.Storage.GetValueOrDefault("dt.legendevo.v1"),profile.Storage.GetValueOrDefault("dt.cosmetics.v1"),profile.Storage.GetValueOrDefault("dt.farm.v1"),item.Id);
        var form=LegendEvolution.Form(evolution.Form);var small=S.Body(12.5f);float inner=width-26;
        string status=(evolution.CanDigivolve?"Ready to digivolve":evolution.Next is {} next?$"Digivolves at level {next}":"Final form reached")+" · "+(evolution.Need==0?"Max level":$"{evolution.Data}/{evolution.Need} Digi-Data");
        float statusHeight=S.ParagraphHeight(status,small,inner,1.35f);
        float height=1+12+19+6+6+6+statusHeight+6+27+12+1;
        if(measure)return height;
        var draw=ImGui.GetWindowDrawList();
        draw.AddRectFilled(at,at+new float2(width,height),new ImColor8(1,10,19,153));draw.AddRect(at,at+new float2(width,height),S.Gold5);
        float x=at.X+13,y=at.Y+13;var nameType=S.Body(14);
        float nameWidth=S.TextIn(draw,new(x,y),19,S.LcText,form.Name,nameType);
        S.TextIn(draw,new(x+nameWidth+5,y),19,new ImColor8(143,154,176),form.Level,S.Body(13));
        var lvl=S.Display(11);string level="Lv "+evolution.Level;float lw=lvl.Width(level)+16;var lvlAt=new float2(at.X+width-13-lw,y+1);
        draw.AddRectFilled(lvlAt,lvlAt+new float2(lw,17),S.Navy0);draw.AddRect(lvlAt,lvlAt+new float2(lw,17),S.Gold3);S.TextIn(draw,lvlAt+new float2(8,0),17,S.Gold1,level,lvl);
        y+=19+6;S.Progress(draw,new(x,y),new(inner,6),evolution.Need==0?1:(float)evolution.Data/evolution.Need);
        y+=6+6;
        if(evolution.CanDigivolve){S.Paragraph(draw,new(x,y),S.Success,status,S.Body(12.5f,true),inner,1.35f);}
        else S.Paragraph(draw,new(x,y),new ImColor8(143,154,176),status,small,inner,1.35f);
        y+=statusHeight+6;
        var buttonType=S.Display(12.5f,.08f);float digivolveWidth=buttonType.Width("DIGIVOLVE")+24;
        if(CosmeticButton("evolution/"+item.Id,new(x,y),new(digivolveWidth,27),"DIGIVOLVE",buttonType,ink:evolution.CanDigivolve?S.TextInv:null))
        {evolutionLegend=item.Id;collectionMessage=null;}
        if(evolution.CanDigivolve)
        {
            var lo=new float2(x,y);var hi=lo+new float2(digivolveWidth,27);
            draw.AddRectFilledMultiColor(lo,hi,new ImColor8(255,210,122),new ImColor8(255,210,122),S.Brand,S.Brand);draw.AddRect(lo,hi,new ImColor8(240,210,140));
            S.TextIn(draw,lo+new float2(12,0),27,S.TextInv,"DIGIVOLVE",buttonType);
        }
        var ghost=S.Body(12.5f,true);float farmWidth=ghost.Width("Digi-Farm")+22;var farmAt=new float2(x+digivolveWidth+4,y);
        if(CosmeticHit("collectionFarm/"+item.Id,farmAt,new(farmWidth,27),out bool hover))OpenCollectionFarm(collection,item.Id);
        if(hover){draw.AddRectFilled(farmAt,farmAt+new float2(farmWidth,27),new ImColor8(200,170,110,20));draw.AddRect(farmAt,farmAt+new float2(farmWidth,27),S.Gold5);}
        S.TextIn(draw,farmAt+new float2(11,0),27,hover?S.Gold1:S.LcMute,"Digi-Farm",ghost);
        return height;
    }
}

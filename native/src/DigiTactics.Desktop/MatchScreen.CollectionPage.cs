using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Storage;
using S = DigiTactics.Desktop.CosmeticStyle;

namespace DigiTactics.Desktop;

/// <summary>
/// Collection page from v1.0.14 js/client.js RENDER.collection and js/theme.js .coll/.ccard/.cprev:
/// art banner with counts, category tabs, search/ownership/rarity/sort tools, 132px auto-fill card grid
/// and the rarity-lit preview card with its actions.
/// </summary>
internal sealed partial class MatchScreen
{
    private const float CollectionToolHeight=37;
    // .ccard-prev is 104px, 88px when the viewport is at most 820px tall.
    private static float CollectionPreviewHeight=>S.Short?88:104;
    private static float CollectionCardHeight=>CollectionPreviewHeight+50;

    /// <summary>An invisible hit target at an absolute position with the shell's anchor, focus and sound bookkeeping.</summary>
    private bool CosmeticHit(string key,float2 at,float2 extent,out bool hover,bool overlap=false,bool sound=true)
    {
        ImGui.SetCursorScreenPos(at);
        if(overlap)ImGui.SetNextItemAllowOverlap();
        bool pressed=ImGui.InvisibleButton(key,new(Math.Max(1,extent.X),Math.Max(1,extent.Y)));hover=ImGui.IsItemHovered();
        if(ImGui.IsItemFocused())KeyboardFocusedButton=key;
        Buttons[key]=at+extent*.5f;
        if(sound)ClientButtonSound(key,pressed);
        return pressed;
    }
    /// <summary>A theme.js button drawn at an absolute position: default bevel, .btn-secondary display type, or .lc-cta.</summary>
    private bool CosmeticButton(string key,float2 at,float2 extent,string label,S.Type type,bool cta=false,bool disabled=false,ImColor8? ink=null)
    {
        ImGui.BeginDisabled(disabled);
        bool pressed=CosmeticHit(key,at,extent,out bool hover);
        ImGui.EndDisabled();
        var draw=ImGui.GetWindowDrawList();
        float alpha=disabled?.45f:1;
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha,ImGui.GetStyle().Alpha*alpha);
        if(cta&&!disabled)S.Cta(draw,at,at+extent,hover);else S.Secondary(draw,at,at+extent,hover&&!disabled);
        var color=ink??(cta&&!disabled?(hover?new ImColor8(255,255,255):new ImColor8(205,250,250)):hover&&!disabled?S.Gold1:S.LcText);
        float width=type.Width(label);
        draw.PushClipRect(at,at+extent,true);
        S.TextIn(draw,new(at.X+(extent.X-width)*.5f,at.Y),extent.Y,S.Faded(color),label,type);
        draw.PopClipRect();
        ImGui.PopStyleVar();
        return pressed&&!disabled;
    }

    /// <summary>
    /// Repaints the client backdrop (DrawClientBackdrop's navy gradient and hex lattice) over the frameless page.
    /// Home stops drawing beneath Collection/Loot/Store except while a match result is retained
    /// (PostgamePending); then this keeps the Home postgame card from showing through.
    /// </summary>
    private void CollectionPageBackdrop()=>PaintClientBackdrop(ImGui.GetWindowDrawList(),ImGui.GetWindowPos(),ImGui.GetWindowPos()+ImGui.GetWindowSize());
    private void CollectionPageBrowser(CosmeticCollection collection)
    {
        var origin=ImGui.GetCursorScreenPos();var full=ImGui.GetContentRegionAvail();
        // .coll: minmax(0,1fr) clamp(270px,24vw,360px), 12px gap.
        float right=S.Narrow?270:Math.Clamp(ImGui.GetIO().DisplaySize.X*.24f,270,360),left=full.X-right-12;
        ImGui.PushStyleColor(ImGuiCol.ChildBg,new ImColor8(0,0,0,0));ImGui.PushStyleColor(ImGuiCol.Border,new ImColor8(0,0,0,0));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(0,0));
        try{CollectionPageColumns(collection,origin,full,left,right);}
        finally{ImGui.PopStyleVar(2);ImGui.PopStyleColor(2);}
    }
    private void CollectionPageColumns(CosmeticCollection collection,float2 origin,float2 full,float left,float right)
    {
        ImGui.BeginChild("collectionBrowser"u8,new(left,full.Y),ImGuiChildFlags.None,ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse|ImGuiWindowFlags.NoBackground);
        try
        {
            CollectionBanner(collection);
            CollectionCategoryTabs(collection);
            string kind=CosmeticCollection.Kinds[collectionKind];
            var filtered=CollectionFilters(collection,kind);
            ImGui.BeginChild("collectionItems"u8,new(0,0),ImGuiChildFlags.None,ImGuiWindowFlags.NoBackground|ImGuiWindowFlags.NoScrollbar);
            CollectionOverlayScrollbar();
            if(collectionResetScroll){ImGui.SetScrollY(0);collectionResetScroll=false;}
            CollectionListBounds=(ImGui.GetWindowPos(),ImGui.GetWindowPos()+ImGui.GetWindowSize());
            try
            {
                if(filtered.Length==0)CollectionEmpty("Nothing matches these filters.");
                else CollectionCardGrid(collection,kind,filtered);
            }
            finally{ImGui.EndChild();}
        }
        finally{ImGui.EndChild();}
        ImGui.SetCursorScreenPos(origin+new float2(left+12,0));
        ImGui.BeginChild("collectionPreview"u8,new(right,full.Y),ImGuiChildFlags.None,ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse|ImGuiWindowFlags.NoBackground);
        CollectionDetailBounds=(ImGui.GetWindowPos(),ImGui.GetWindowPos()+ImGui.GetWindowSize());
        try{SelectedCosmetic(collection,CosmeticCollection.Kinds[collectionKind],true);}
        finally{ImGui.EndChild();}
    }
    private static readonly Dictionary<string,(float Scroll,double Time)> overlayScrollSeen=[];
    /// <summary>
    /// The 8px ::-webkit-scrollbar thumb drawn over the grid's right padding, so it never narrows the columns.
    /// The original's reference renders show no scrollbar, so the thumb appears only while the pointer is over the
    /// list or for a moment after it scrolls.
    /// </summary>
    private static void CollectionOverlayScrollbar()
    {
        float max=ImGui.GetScrollMaxY();if(max<=0)return;
        var lo=ImGui.GetWindowPos();var extent=ImGui.GetWindowSize();float scroll=ImGui.GetScrollY();
        if(!OverlayScrollbarVisible($"{lo.X:0},{lo.Y:0}",scroll))return;
        float thumb=Math.Max(24,extent.Y*extent.Y/(extent.Y+max)),top=lo.Y+(extent.Y-thumb)*scroll/max;
        var draw=ImGui.GetWindowDrawList();
        draw.PushClipRect(lo,lo+extent,false);
        draw.AddRectFilled(new(lo.X+extent.X-6,top+2),new(lo.X+extent.X-2,top+thumb-2),S.Gold5);
        draw.PopClipRect();
    }
    private static bool OverlayScrollbarVisible(string key,float scroll)
    {
        double now=ImGui.GetTime();
        if(!overlayScrollSeen.TryGetValue(key,out var seen)){overlayScrollSeen[key]=(scroll,double.NegativeInfinity);seen=overlayScrollSeen[key];}
        else if(seen.Scroll!=scroll)overlayScrollSeen[key]=seen=(scroll,now);
        return ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows)||now-seen.Time<.8;
    }
    /// <summary>.empty: centred muted text in a dashed gold-5 frame spanning the grid.</summary>
    private static void CollectionEmpty(string text)
    {
        var at=ImGui.GetCursorScreenPos()+new float2(2,2);float width=ImGui.GetContentRegionAvail().X-8;var draw=ImGui.GetWindowDrawList();
        var type=S.Body(14);float height=24*2+19;
        for(float x=0;x<width;x+=6){draw.AddLine(at+new float2(x,0),at+new float2(Math.Min(width,x+3),0),S.Gold5);draw.AddLine(at+new float2(x,height),at+new float2(Math.Min(width,x+3),height),S.Gold5);}
        for(float y=0;y<height;y+=6){draw.AddLine(at+new float2(0,y),at+new float2(0,Math.Min(height,y+3)),S.Gold5);draw.AddLine(at+new float2(width,y),at+new float2(width,Math.Min(height,y+3)),S.Gold5);}
        S.TextIn(draw,at+new float2((width-type.Width(text))*.5f,24),19,S.Text3,text,type);
        ImGui.Dummy(new(width,height+4));
    }

    /// <summary>.coll-banner: 92px category art, left-to-right navy wash, eyebrow/title and the right-hand count stack.</summary>
    private void CollectionPageBanner(CosmeticCollection collection,string kind,CosmeticItem[] visible,int owned)
    {
        float width=ImGui.GetContentRegionAvail().X;float height=S.Short?72:92;
        var at=ImGui.GetCursorScreenPos();var draw=ImGui.GetWindowDrawList();
        string art=kind switch{"legend"=>"coll-legends","board"=>"coll-boards","emote" or "sound"=>"coll-emotes",_=>"store-fx"};
        collectionArtwork.Draw(art,at,new(width,height),255,true,.45f);
        float split=width*.6f;
        draw.AddRectFilledMultiColor(at,at+new float2(split,height),new ImColor8(1,10,19,235),new ImColor8(1,10,19,89),new ImColor8(1,10,19,89),new ImColor8(1,10,19,235));
        draw.AddRectFilledMultiColor(at+new float2(split,0),at+new float2(width,height),new ImColor8(1,10,19,89),new ImColor8(1,10,19,204),new ImColor8(1,10,19,204),new ImColor8(1,10,19,89));
        draw.AddRect(at,at+new float2(width,height),S.Gold4);
        // Bottom-aligned blocks with 24px side and 12px bottom padding.
        var title=S.Display(26,.04f);var eyebrow=S.Display(11,.16f);
        float bottom=at.Y+height-12;
        S.TextIn(draw,new(at.X+24,bottom-28.6f),28.6f,S.Gold1,CollectionNames[collectionKind].ToUpperInvariant(),title);
        S.TextIn(draw,new(at.X+24,bottom-28.6f-2-14.85f),14.85f,S.Gold2,"COLLECTION",eyebrow);
        var big=S.Display(20);var line=S.Display(14,0,false);var small=S.Body(11);
        string ownedText=owned.ToString(),totalText=$"/ {visible.Length} owned";
        var all=CosmeticCollection.Kinds.SelectMany(category=>CosmeticCollection.Items(category).Where(item=>!item.Secret||collection.Owns(category,item.Id)).Select(item=>(Kind:category,Item:item))).ToArray();
        string total=$"{all.Count(entry=>collection.Owns(entry.Kind,entry.Item.Id))} / {all.Length} in total";
        float right=at.X+width-24;float blockWidth=Math.Max(160,Math.Max(line.Width(totalText),small.Width(total)));
        float y=bottom-6;S.Progress(draw,new(right-blockWidth,y),new(blockWidth,6),visible.Length==0?0:(float)owned/visible.Length);
        y-=3+14.85f;S.TextIn(draw,new(right-small.Width(total),y),14.85f,S.Text3,total,small);
        y-=3+18.9f;S.TextIn(draw,new(right-line.Width(totalText),y),18.9f,S.LcText,totalText,line);
        y-=3+27;S.TextIn(draw,new(right-big.Width(ownedText),y),27,S.Gold1,ownedText,big);
        ImGui.Dummy(new(width,height+8));
    }

    /// <summary>.ctabs: natural-width display-font tabs with muted counts, clipped like the original's hidden horizontal overflow.</summary>
    private void CollectionPageTabs(CosmeticCollection collection,int[] order)
    {
        var origin=ImGui.GetCursorScreenPos();float available=ImGui.GetContentRegionAvail().X;var draw=ImGui.GetWindowDrawList();
        var label=S.Display(14,.1f,false);var counter=S.Body(11);const float height=38;
        float space=label.Font.CalcTextSizeA(label.Size,float.MaxValue,0," "u8).X+label.Tracking;
        draw.AddLine(new(origin.X,origin.Y+height+.5f),new(origin.X+available,origin.Y+height+.5f),S.Gold5);
        // The original clips overflowing tabs but can scroll them; never leave a tab wholly unreachable:
        // when the last tab would start past the edge, use the compact (<=1380px) tab style instead.
        (string Name,string? Count) Tab(int i)
        {
            if(i==7)return("DIGI-EGGS",null);
            string kind=CosmeticCollection.Kinds[i];
            var items=CosmeticCollection.Items(kind).Where(item=>collectionStore?!item.Secret&&item.Linked is null:!item.Secret||collection.Owns(kind,item.Id)).ToArray();
            int owned=items.Count(item=>collection.Owns(kind,item.Id));
            return(CollectionTabs[i].ToUpperInvariant(),collectionStore?items.Length.ToString():MatchLockerVisible?owned.ToString():owned+"/"+items.Length);
        }
        var tabs=order.Select(Tab).ToArray();
        bool compact=S.Narrow||tabs.SkipLast(1).Sum(t=>28+label.Width(t.Name)+(t.Count is null?0:space+3+counter.Width(t.Count))+4)>=available-8;
        float x=0;
        for(int n=0;n<order.Length;n++)
        {
            int i=order[n];bool eggs=i==7,selected=eggs?collectionStoreEggs:!collectionStoreEggs&&collectionKind==i;
            var (name,count)=tabs[n];
            if(compact)count=null;
            float pad=compact?9:14;
            float nameWidth=label.Width(name),width=pad*2+nameWidth+(count is null?0:space+3+counter.Width(count));
            if(x>=available)break;
            float visibleWidth=Math.Min(width,available-x);
            bool clicked=CosmeticHit("collectionTab/"+i,origin+new float2(x,0),new(visibleWidth,height),out bool hover);
            var at=origin+new float2(x,0);
            draw.PushClipRect(origin,origin+new float2(available,height+1),true);
            // Baselines: the label and its smaller body-font count share one line box (9px top padding).
            float baseline=at.Y+9+(18.9f-label.Size)*.5f+.992f*14;
            S.Text(draw,new(at.X+pad,baseline-.992f*14),selected||hover?S.Gold1:S.LcMute,name,label);
            if(count is not null)S.Text(draw,new(at.X+pad+nameWidth+space+3,baseline-.969f*11),S.Text3,count,counter);
            if(selected)
            {
                draw.AddRectFilled(new(at.X+12,at.Y+height-6),new(at.X+width-12,at.Y+height),new ImColor8(200,155,60,26));
                draw.AddRectFilled(new(at.X+12,at.Y+height-1.5f),new(at.X+width-12,at.Y+height+.5f),S.Gold2);
            }
            draw.PopClipRect();
            if(clicked){if(eggs){collectionStoreEggs=true;collectionResetScroll=true;}else CollectionCategoryChanged(i);}
            x+=width+4;
        }
        ImGui.SetCursorScreenPos(origin+new float2(0,height+1+12));ImGui.Dummy(new(0,0));
    }

    /// <summary>.ctools: optional retro shortcut, flexible search, All/Owned/Not owned segment and two selects, wrapping when narrow.</summary>
    private CosmeticItem[] CollectionPageFilters(CosmeticCollection collection,string kind)
    {
        var origin=ImGui.GetCursorScreenPos();float available=ImGui.GetContentRegionAvail().X;var draw=ImGui.GetWindowDrawList();
        var body=S.Body(14);var segType=S.Body(12.5f,true);var retroType=S.Display(14,.08f);
        bool retro=kind=="emote";
        float retroWidth=retro?retroType.Width("DIGIMON WORLD 1")+34:0;
        (string Id,string Label)[] ownership=[("all","All"),("owned","Owned"),("locked","Not owned")];
        // Inter 600 sits between the bundled regular and bold faces: size by the regular face, draw bold.
        var segMeasure=S.Body(12.5f);
        float segWidth=6+ownership.Sum(option=>segMeasure.Width(option.Label)+24)+2*(ownership.Length-1);
        var rarityOptions=new[]{("all","All rarities")}.Concat(CosmeticBrowser.Rarities.Select(value=>(value,char.ToUpperInvariant(value[0])+value[1..]))).ToArray();
        (string,string)[] sortOptions=[("rarity","Rarity: high first"),("rarity-asc","Rarity: low first"),("name","Name"),("owned","Owned first")];
        float rarityWidth=rarityOptions.Max(option=>body.Width(option.Item2))+48,sortWidth=sortOptions.Max(option=>body.Width(option.Item2))+48;
        // Flex-wrap: lay out the fixed controls after a search box of at least 200px.
        float[] widths=[retroWidth,200,segWidth,rarityWidth,sortWidth];
        var rows=new List<List<int>>{new()};float rowWidth=0;
        for(int i=0;i<widths.Length;i++)
        {
            if(widths[i]==0)continue;
            float need=(rows[^1].Count>0?8:0)+widths[i];
            if(rows[^1].Count>0&&rowWidth+need>available){rows.Add([]);rowWidth=0;need=widths[i];}
            rows[^1].Add(i);rowWidth+=need;
        }
        float y=origin.Y;
        foreach(var row in rows)
        {
            float used=row.Sum(i=>widths[i])+8*(row.Count-1);float grow=Math.Max(0,available-used);float x=origin.X;
            foreach(int i in row)
            {
                float width=widths[i]+(i==1?grow:0);var at=new float2(x,y);
                switch(i)
                {
                    case 0:
                        if(CosmeticButton("collectionRetro",at,new(width,CollectionToolHeight),"DIGIMON WORLD 1",retroType))
                        {collectionSearch.SetValue("Digimon World 1");collectionOwnership=collectionRarity="all";previewCosmetic="retro_hug";}
                        break;
                    case 1:CollectionSearchBox(kind,at,width);break;
                    case 2:
                    {
                        var segAt=at+new float2(0,2);float segHeight=CollectionToolHeight-4;
                        draw.AddRectFilled(segAt,segAt+new float2(width,segHeight),new ImColor8(1,10,19,217));draw.AddRect(segAt,segAt+new float2(width,segHeight),S.Gold5);
                        float bx=segAt.X+3;
                        foreach(var option in ownership)
                        {
                            float bw=segMeasure.Width(option.Label)+24;var buttonAt=new float2(bx,segAt.Y+3);var extent=new float2(bw,segHeight-6);
                            bool on=collectionOwnership==option.Id;
                            if(CosmeticHit("collectionOwnership/"+option.Id,buttonAt,extent,out bool hover))collectionOwnership=option.Id;
                            if(on){draw.AddRectFilledMultiColor(buttonAt,buttonAt+extent,new ImColor8(30,35,40),new ImColor8(30,35,40),new ImColor8(42,50,64),new ImColor8(42,50,64));draw.AddRect(buttonAt,buttonAt+extent,S.Gold4);}
                            S.TextIn(draw,buttonAt+new float2((bw-segType.Width(option.Label))*.5f,0),extent.Y,on||hover?S.Gold1:S.LcMute,option.Label,segType);
                            bx+=bw+2;
                        }
                        break;
                    }
                    case 3:CollectionSelect("collectionRarity","Rarity",ref collectionRarity,rarityOptions,at,width);break;
                    case 4:CollectionSelect("collectionSort","Sort",ref collectionSort,sortOptions,at,width);break;
                }
                x+=width+8;
            }
            y+=CollectionToolHeight+8;
        }
        ImGui.SetCursorScreenPos(new(origin.X,y-8+10));ImGui.Dummy(new(0,0));
        var items=CosmeticBrowser.Items(collection,kind,collectionSearch.ToString(),collectionOwnership,collectionRarity,collectionSort);
        CollectionFilterIds=items.Select(item=>item.Id).ToArray();
        return items;
    }
    private void CollectionSearchBox(string kind,float2 at,float width)
    {
        var draw=ImGui.GetWindowDrawList();var type=S.Body(14);
        ImGui.SetCursorScreenPos(at);ImGui.SetNextItemWidth(width);
        ImGui.PushFont(type.Font,type.Size);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(11,(CollectionToolHeight-type.Size)*.5f));ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,0);
        ImGui.PushStyleColor(ImGuiCol.FrameBg,new ImColor8(1,10,19,217));ImGui.PushStyleColor(ImGuiCol.FrameBgHovered,new ImColor8(1,10,19,217));ImGui.PushStyleColor(ImGuiCol.FrameBgActive,new ImColor8(1,10,19,217));
        ImGui.PushStyleColor(ImGuiCol.Text,S.Text1);ImGui.PushStyleColor(ImGuiCol.TextDisabled,new ImColor8(117,124,136));
        string hint="Search "+CollectionNames[collectionKind].ToLowerInvariant();
        bool hasText=collectionSearch.ToString().Length>0;
        ImGui.InputTextWithHint("##collectionSearch"u8,hint,collectionSearch,ImGuiInputTextFlags.EscapeClearsAll);
        bool active=ImGui.IsItemActive();
        ImGui.PopStyleColor(5);ImGui.PopStyleVar(2);ImGui.PopFont();
        collectionSearch.EvaluateLength();collectionSearchFocused=active;CollectionFilterAnchor("collectionSearch");
        draw.AddRect(at,at+new float2(width,CollectionToolHeight),active?S.Gold2:S.Gold5);
        if(active)draw.AddRect(at-new float2(2,2),at+new float2(width+2,CollectionToolHeight+2),new ImColor8(200,170,110,46),thickness:2);
        // The type=search cancel control: shown only with a query, always addressable at the field's end.
        var clearAt=at+new float2(width-30,(CollectionToolHeight-20)*.5f);
        if(CosmeticHit("clearCollectionSearch",clearAt,new(20,20),out bool hover,overlap:true)&&hasText)collectionSearch.SetValue("");
        if(hasText)
        {
            var c=clearAt+new float2(10,10);var ink=hover?S.Gold1:S.Text2;
            draw.AddLine(c-new float2(4,4),c+new float2(4,4),ink,1.5f);draw.AddLine(c+new float2(-4,4),c+new float2(4,-4),ink,1.5f);
            if(hover)ImGui.SetTooltip("Clear search"u8);
        }
    }
    /// <summary>A native select: navy field, gold-5 frame, body text and a small chevron.</summary>
    private void CollectionSelect(string key,string label,ref string value,(string Id,string Label)[] options,float2 at,float width)
    {
        var type=S.Body(14);var draw=ImGui.GetWindowDrawList();string selected=value;
        ImGui.SetCursorScreenPos(at);ImGui.SetNextItemWidth(width);
        ImGui.PushFont(type.Font,type.Size);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(15,(CollectionToolHeight-type.Size)*.5f));ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,0);
        ImGui.PushStyleColor(ImGuiCol.FrameBg,new ImColor8(1,10,19,217));ImGui.PushStyleColor(ImGuiCol.FrameBgHovered,new ImColor8(1,10,19,217));ImGui.PushStyleColor(ImGuiCol.Text,S.Text1);
        ImGui.PushStyleColor(ImGuiCol.PopupBg,new ImColor8(21,30,49));ImGui.PushStyleColor(ImGuiCol.Header,new ImColor8(36,50,77));ImGui.PushStyleColor(ImGuiCol.HeaderHovered,new ImColor8(48,62,92));
        bool open=ImGui.BeginCombo("##"+key,options.Single(option=>option.Id==selected).Label,ImGuiComboFlags.NoArrowButton);
        bool hover=ImGui.IsItemHovered();CollectionFilterAnchor(key);
        if(hover&&!open)ImGui.SetTooltip(label);
        draw.AddRect(at,at+new float2(width,CollectionToolHeight),open?S.Gold2:S.Gold5);
        var chevron=at+new float2(width-15,CollectionToolHeight*.5f);
        draw.AddLine(chevron+new float2(-4.5f,-2),chevron+new float2(0,2.5f),S.Text1,1.5f);draw.AddLine(chevron+new float2(0,2.5f),chevron+new float2(4.5f,-2),S.Text1,1.5f);
        if(open)
        {
            foreach(var option in options)
            {
                bool chosen=ImGui.Selectable(option.Label,option.Id==value);CollectionFilterAnchor(key+"/"+option.Id);ClientItemSound(key+"/"+option.Id,chosen);
                if(chosen)value=option.Id;
            }
            ImGui.EndCombo();
        }
        ImGui.PopStyleColor(6);ImGui.PopStyleVar(2);ImGui.PopFont();
    }

    /// <summary>.cgrid of .ccard buttons: auto-fill minmax(132px,1fr), 8px gaps, 154px cards.</summary>
    private void CollectionCardGrid(CosmeticCollection collection,string kind,CosmeticItem[] items)
    {
        var origin=ImGui.GetCursorScreenPos()+new float2(2,2);float available=ImGui.GetContentRegionAvail().X-8;
        int columns=Math.Max(1,(int)((available+8)/140));
        float width=(available-(columns-1)*8)/columns;
        for(int i=0;i<items.Length;i++)
        {
            var at=origin+new float2(i%columns*(width+8),i/columns*(CollectionCardHeight+8));
            CollectionCard(collection,kind,items[i],at,width);
        }
        int rows=(items.Length+columns-1)/columns;
        ImGui.SetCursorScreenPos(origin);ImGui.Dummy(new(available,rows*(CollectionCardHeight+8)-8+16));
    }
    private void CollectionCard(CosmeticCollection collection,string kind,CosmeticItem item,float2 at,float width)
    {
        var extent=new float2(width,CollectionCardHeight);
        string key="collectionCard/"+kind+"/"+item.Id;
        var cardAt=at;
        void Hit()
        {
            // Submitted after the card's inner play control so that control keeps hover priority.
            bool clicked=CosmeticHit(key,cardAt,extent,out _,sound:false);
            Buttons["preview/"+kind+"/"+item.Id]=cardAt+extent*.5f;
            if(clicked){previewCosmetic=item.Id;ClientItemSound("preview/"+kind+"/"+item.Id,true);}
            else ClientItemSound("preview/"+kind+"/"+item.Id,false);
        }
        if(!ImGui.IsRectVisible(at-new float2(20,20),at+extent+new float2(20,20))){Hit();return;}
        bool hovered=ImGui.IsWindowHovered()&&ImGui.IsMouseHoveringRect(at,at+extent);
        bool selected=previewCosmetic==item.Id,owned=collection.Owns(kind,item.Id);
        if(hovered)at.Y-=2;
        var end=at+extent;var draw=ImGui.GetWindowDrawList();var rarity=S.Rarity(item.Rarity);
        if(selected)for(int i=1;i<=9;i++)draw.AddRect(at-new float2(1+i,1+i),end+new float2(1+i,1+i),new ImColor8(240,210,140,(byte)(28*(1-i/10f))),thickness:2);
        else if(hovered){for(int i=1;i<=7;i++)draw.AddRect(at-new float2(i,i),end+new float2(i,i),S.Alpha(rarity,.09f*(1-i/8f)),thickness:2);draw.AddRectFilled(at+new float2(0,8),end+new float2(0,12),new ImColor8(0,0,0,60));}
        draw.AddRectFilledMultiColor(at,end,new ImColor8(11,26,44),new ImColor8(11,26,44),S.Navy0,S.Navy0);
        var prevAt=at+new float2(1,1);var prevEnd=new float2(end.X-1,at.Y+CollectionPreviewHeight);float shift=CollectionPreviewHeight-104;
        int first=draw.VtxBuffer.Count;
        CollectionRadial(draw,prevAt,prevEnd,new(.5f,.4f),S.Mix(rarity,.3f,new(11,26,44)),S.Navy0,.85f);
        draw.PushClipRect(prevAt,prevEnd,true);
        CollectionCardArt(collection,kind,item,prevAt,prevEnd-prevAt,owned);
        draw.PopClipRect();
        draw.AddRectFilled(new(prevAt.X,prevEnd.Y-3),new(prevEnd.X,prevEnd.Y-1),S.Alpha(rarity,.7f));
        if(!owned)S.Filter(draw,first,.8f,.55f);
        draw.AddLine(new(prevAt.X,prevEnd.Y-.5f),new(prevEnd.X,prevEnd.Y-.5f),S.Gold5);
        var nameType=S.Body(12.5f,true);var metaType=S.Body(10.5f,true,.08f);
        S.TextFit(draw,at+new float2(9,112+shift),owned?S.Gold1:S.Text2,item.Name,nameType,width-18);
        S.Diamond(draw,at+new float2(12.5f,137+shift),7,rarity);
        string meta=S.RarityName(item.Rarity).ToUpperInvariant()+(item.Tag is not null?" · "+item.Tag.ToUpperInvariant():"");
        S.TextFit(draw,at+new float2(21,130.5f+shift),rarity,meta,metaType,width-26);
        bool equipped=owned&&CollectionWorn(collection,kind,item.Id);
        if(equipped||!owned)
        {
            string badge=equipped?"Equipped":item.EggOnly&&!item.Free?"Egg only":"Locked";
            var badgeAt=new float2(end.X-7-S.BadgeWidth(badge),at.Y+7);
            if(equipped)S.EquippedBadge(draw,badgeAt,22);else S.LockBadge(draw,badgeAt,badge,22);
        }
        var border=selected?S.Gold1:hovered?S.Gold2:S.Mix(rarity,.25f,S.Gold5);
        draw.AddRect(at,end,border);
        if(selected)draw.AddRect(at-new float2(1,1),end+new float2(1,1),S.Gold2);
        Hit();
        if(hovered)ImGui.SetTooltip(item.Name);
    }
    /// <summary>radial-gradient(circle at cx cy, inner, outer stop) sized to the farthest corner, drawn as a fan clipped to the box.</summary>
    private static void CollectionRadial(ImDrawListPtr draw,float2 lo,float2 hi,float2 centre,ImColor8 inner,ImColor8 outer,float stop,float scaleY=1)
    {
        var c=lo+(hi-lo)*centre;float fx=Math.Max(c.X-lo.X,hi.X-c.X),fy=Math.Max(c.Y-lo.Y,hi.Y-c.Y)/scaleY;
        float corner=MathF.Sqrt(fx*fx+fy*fy),radius=corner*stop,beyond=corner*1.05f;
        draw.PushClipRect(lo,hi,true);
        var flags=draw.Flags;draw.Flags&=~ImDrawListFlags.AntiAliasedFill;
        int first=draw.VtxBuffer.Count;const int segments=48;
        float2 Point(double angle,float r)=>c+new float2((float)Math.Cos(angle)*r,(float)Math.Sin(angle)*r*scaleY);
        for(int i=0;i<segments;i++)
        {
            double a=i*Math.PI*2/segments,b=(i+1)*Math.PI*2/segments;
            // Centre fan to the last colour stop, then a flat band out past the farthest corner;
            // translucent stops therefore replace rather than stack.
            draw.AddTriangleFilled(c,Point(a,radius),Point(b,radius),inner);
            if(beyond>radius)draw.AddQuadFilled(Point(a,radius),Point(a,beyond),Point(b,beyond),Point(b,radius),outer);
        }
        RadialRecolor(draw,first,c,radius,inner,outer,scaleY);
        draw.Flags=flags;draw.PopClipRect();
    }
    private static unsafe void RadialRecolor(ImDrawListPtr draw,int first,float2 centre,float radius,ImColor8 inner,ImColor8 outer,float scaleY)
    {
        var a=inner.AsByte4();var b=outer.AsByte4();
        for(int i=first;i<draw.VtxBuffer.Count;i++)
        {
            ref var vertex=ref draw.VtxBuffer.Data[i];var d=vertex.pos-centre;d.Y/=scaleY;
            float t=Math.Clamp(MathF.Sqrt(d.X*d.X+d.Y*d.Y)/radius,0,1);
            uint r=(uint)Math.Round(a.X+(b.X-a.X)*t),g=(uint)Math.Round(a.Y+(b.Y-a.Y)*t),bl=(uint)Math.Round(a.Z+(b.Z-a.Z)*t),al=(uint)Math.Round(a.W+(b.W-a.W)*t);
            vertex.col=(al<<24)|(bl<<16)|(g<<8)|r;
        }
    }
    /// <summary>The card's item.preview(el) output inside the 104px .ccard-prev box.</summary>
    private void CollectionCardArt(CosmeticCollection collection,string kind,CosmeticItem item,float2 at,float2 extent,bool owned)
    {
        var draw=ImGui.GetWindowDrawList();float gray=owned?0:.8f;
        switch(kind)
        {
            case "legend":portraits.DrawCompanion(CurrentPreviewLegend(collection,item.Id),at+new float2((extent.X-extent.Y)/2,0),extent.Y,grayscale:gray);break;
            // .em-prev > .em-svg: min(100%,120px) square, overflowing the 104px box.
            case "emote":{float side=Math.Min(extent.X,120);emoteArtwork.Draw(item.Id,(long)(boardSceneTime*1000),at+(extent-new float2(side,side))*.5f,side,grayscale:gray);break;}
            case "killscreen":
                collectionTheme.Draw(item.Id,false,boardSceneTime%3.5,at,extent);
                portraits.DrawCompanion(CurrentPreviewLegend(collection),at+new float2(extent.X*.5f-30,extent.Y*.5f-36),60,grayscale:gray);
                break;
            case "dance":
                if(!StoreDanceThumbnail(collection,item,at,extent)&&CollectionArt.Value.TryGetValue(kind+"/"+item.Id,out string? danceFile))collectionArtwork.Draw(danceFile,at+(extent-new float2(64,64))*.5f,new(64,64),grayscale:!owned);
                break;
            case "sound":
            {
                // soundboard.js: <button class="sb-prev">&#9654; <span>icon</span></button>
                float w=64,h=32;var b=at+(extent-new float2(w,h))*.5f;
                bool play=CosmeticHit("gridSound/sound/"+item.Id,b,new(w,h),out bool hover,sound:false);
                S.Secondary(draw,b,b+new float2(w,h),hover);
                CollectionPlayGlyph(draw,b+new float2(12,h*.5f),12,hover?S.Gold1:S.LcText);
                CollectionIcon(kind,item.Id,b+new float2(42,h*.5f),16,!owned);
                if(play){previewCosmetic=item.Id;audio.PlayBoard(item.Data.TryGetProperty("clip"u8,out var clip)?clip.GetString()!:item.Id);}
                break;
            }
            case "elimsound":
            {
                // elimsounds.js .es-wrap: 30px icon, 6px gap, &#9654; Play button.
                var playType=S.Body(13,true);float w=playType.Width("Play")+40,h=26,total=30+6+h;
                float top=at.Y+(extent.Y-total)*.5f;
                CollectionIcon(kind,item.Id,new(at.X+extent.X*.5f,top+15),30,!owned);
                var b=new float2(at.X+(extent.X-w)*.5f,top+36);
                bool play=CosmeticHit("gridSound/elimsound/"+item.Id,b,new(w,h),out bool hover,sound:false);
                S.Secondary(draw,b,b+new float2(w,h),hover);
                var ink=hover?S.Gold1:S.LcText;CollectionPlayGlyph(draw,b+new float2(13,h*.5f),10,ink);S.TextIn(draw,b+new float2(28,0),h,ink,"Play",playType);
                if(play){previewCosmetic=item.Id;audio.PlayEliminationSound(item.Id,true,true,0);}
                break;
            }
            default:
                if(CollectionArt.Value.TryGetValue(kind+"/"+item.Id,out string? file))collectionArtwork.Draw(file,at,extent,255,kind=="board",grayscale:!owned);
                break;
        }
    }
    /// <summary>Sound icons: the exported art is a 160x120 card render whose glyph is about 60px tall.</summary>
    private void CollectionIcon(string kind,string id,float2 centre,float glyph,bool grayscale=false)
    {
        if(!CollectionArt.Value.TryGetValue(kind+"/"+id,out string? file))return;
        float height=glyph*2,width=height*160/120;
        collectionArtwork.Draw(file,centre-new float2(width*.5f,height*.46f),new(width,height),grayscale:grayscale);
    }
    private static void CollectionPlayGlyph(ImDrawListPtr draw,float2 left,float size,ImColor8 color)
        =>draw.AddTriangleFilled(left+new float2(0,-size*.5f),left+new float2(0,size*.5f),left+new float2(size*.85f,0),S.Faded(color));
}

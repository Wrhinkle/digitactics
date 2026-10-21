using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using S = DigiTactics.Desktop.CosmeticStyle;

namespace DigiTactics.Desktop;

/// <summary>
/// Full Codex header from v1.0.14 js/codex.js with js/codexstyle.js and the theme.js .cx-* restyle:
/// .cx-head (display title, tabs with muted counts, search), the .cx-sets art strip and the .cx-filters segment.
/// </summary>
internal sealed partial class MatchScreen
{
    private static readonly ImColor8 CodexLine=new(34,48,73),CodexLine2=new(49,66,95),CodexBg=new(10,15,26),CodexPanel3=new(28,39,62),CodexSegMuted=new(143,154,176);

    private void CodexHead(CodexVariant data,float2 window,float width)
    {
        var draw=ImGui.GetWindowDrawList();
        var title=S.Display(22,.08f);var tabType=S.Body(13.33f,true);var countType=S.Body(12);
        string heading="CODEX · "+data.Name.ToUpperInvariant();
        float titleWidth=title.Width(heading);
        var tabs=data.Tabs.Select(tab=>(Id:tab,Label:tab=="wisps"?"Data Wisps":CodexTitle(tab),Count:tab=="rules"?null:CodexCount(tab).ToString())).ToArray();
        float TabWidth((string Id,string Label,string? Count) t)=>22+tabType.Width(t.Label)+(t.Count is null?0:4+countType.Width(t.Count));
        const float searchWidth=200,closeWidth=70,rowHeight=30;
        float tabsLeft=14+titleWidth+12,tabsRight=width-14-searchWidth-8-closeWidth-12;
        // Tabs sit beside the title; when they do not fit they wrap onto rows below it.
        bool below=tabs.Sum(TabWidth)+4*(tabs.Length-1)>tabsRight-tabsLeft;
        if(below){tabsLeft=14;tabsRight=width-14;}
        var rows=new List<List<int>>{new()};float x=tabsLeft;
        for(int i=0;i<tabs.Length;i++){float w=TabWidth(tabs[i]);if(rows[^1].Count>0&&x+w>tabsRight){rows.Add([]);x=tabsLeft;}rows[^1].Add(i);x+=w+4;}
        float headHeight=10+(below?rowHeight+4:0)+rows.Count*rowHeight+4*(rows.Count-1)+10;
        var top=window+new float2(1,1);
        draw.AddRectFilledMultiColor(top,top+new float2(width-2,headHeight),new ImColor8(10,20,40,242),new ImColor8(10,20,40,242),new ImColor8(1,10,19,179),new ImColor8(1,10,19,179));
        draw.AddLine(new(top.X,top.Y+headHeight-.5f),new(top.X+width-2,top.Y+headHeight-.5f),S.Gold5);
        S.TextIn(draw,new(window.X+14,top.Y+10),rowHeight,S.Gold1,heading,title);
        // .cx-search, then the native Back/Close control
        var searchAt=new float2(window.X+width-14-closeWidth-8-searchWidth,top.Y+10);
        ImGui.SetCursorScreenPos(searchAt);ImGui.SetNextItemWidth(searchWidth);
        var body=S.Body(14);ImGui.PushFont(body.Font,body.Size);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,0);ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(10,(rowHeight-body.Size)*.5f));
        ImGui.PushStyleColor(ImGuiCol.FrameBg,new ImColor8(1,10,19,217));ImGui.PushStyleColor(ImGuiCol.FrameBgHovered,new ImColor8(1,10,19,217));ImGui.PushStyleColor(ImGuiCol.FrameBgActive,new ImColor8(1,10,19,217));
        ImGui.PushStyleColor(ImGuiCol.Text,S.Text1);ImGui.PushStyleColor(ImGuiCol.TextDisabled,new ImColor8(117,124,136));
        if(ImGui.InputTextWithHint("##codexSearch"u8,"Search"u8,codexSearch,ImGuiInputTextFlags.EscapeClearsAll))codexResetScroll=true;
        bool active=ImGui.IsItemActive();
        ImGui.PopStyleColor(5);ImGui.PopStyleVar(2);ImGui.PopFont();codexSearch.EvaluateLength();
        codexSearchFocused=active;Buttons["codexSearch"]=(ImGui.GetItemRectMin()+ImGui.GetItemRectMax())*.5f;
        draw.AddRect(searchAt,searchAt+new float2(searchWidth,rowHeight),active?S.Gold2:S.Gold5);
        if(active)draw.AddRect(searchAt-new float2(2,2),searchAt+new float2(searchWidth+2,rowHeight+2),new ImColor8(200,170,110,46),thickness:2);
        if(CosmeticButton("closeCodex",new(searchAt.X+searchWidth+8,searchAt.Y),new(closeWidth,rowHeight),ClientHomeVisible?"Back":"Close",tabType))CloseCodexFull();
        // .cx-tabs
        float y=top.Y+10+(below?rowHeight+4:0);
        foreach(var row in rows)
        {
            x=window.X+tabsLeft;
            foreach(int i in row)
            {
                var tab=tabs[i];float w=TabWidth(tab);var at=new float2(x,y);bool on=codexTab==tab.Id;
                bool pressed=CosmeticHit("codexTab/"+tab.Id,at,new(w,rowHeight),out bool hover);
                if(on)
                {
                    draw.AddRectFilledMultiColor(at,at+new float2(w,rowHeight),new ImColor8(30,35,40),new ImColor8(30,35,40),new ImColor8(42,50,64),new ImColor8(42,50,64));
                    draw.AddRect(at,at+new float2(w,rowHeight),S.Gold2);draw.AddRect(at+new float2(1,1),at+new float2(w-1,rowHeight-1),S.Gold3);
                }
                else S.Secondary(draw,at,at+new float2(w,rowHeight),hover);
                // Label and the smaller count share a baseline.
                float baseline=at.Y+(rowHeight-tabType.Size)*.5f+.969f*13.33f;
                float tw=S.Text(draw,new(at.X+11,baseline-.969f*13.33f),on||hover?S.Gold1:S.LcText,tab.Label,tabType);
                if(tab.Count is not null)S.Text(draw,new(at.X+11+tw+4,baseline-.969f*12),CodexSegMuted,tab.Count,countType);
                if(pressed){codexTab=tab.Id;codexResetScroll=true;}
                x+=w+4;
            }
            y+=rowHeight+4;
        }
        ImGui.SetCursorScreenPos(new(window.X+16,top.Y+headHeight+10));
        CodexSetStrip(data,width-32);
        CodexFilterSegment(window,width);
    }
    /// <summary>.cx-sets: 170x58 art cards, desaturated until hovered or chosen, bottom-aligned name and trait count.</summary>
    private void CodexSetStrip(CodexVariant data,float width)
    {
        var start=ImGui.GetCursorScreenPos();
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(0,0));
        ImGui.BeginChild("Codex sets"u8,new(width,60),ImGuiChildFlags.None,ImGuiWindowFlags.HorizontalScrollbar|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoBackground);
        ImGui.PopStyleVar(2);
        var draw=ImGui.GetWindowDrawList();var origin=ImGui.GetCursorScreenPos()+new float2(1,1);float x=0;
        var nameType=S.Display(14,.06f);var small=S.Body(10,false,.04f);
        foreach(var set in codexCatalog.Value.Variants.GroupBy(v=>v.SetId).Select(group=>group.FirstOrDefault(v=>v.Pace==data.Pace)??group.First()))
        {
            bool locked=Session is not null&&set.SetId!=data.SetId,on=data.SetId==set.SetId;
            float setWidth=S.Narrow?140:170;const float setHeight=58;var at=origin+new float2(x,0);
            ImGui.BeginDisabled(locked);
            bool chosen=CosmeticHit("codexSet/"+set.SetId,at,new(setWidth,setHeight),out bool hover);
            ImGui.EndDisabled();
            string art=set.SetId switch{"faction-wars"=>"mode-faction-wars","dragonlands"=>"mode-dragonlands","autochess"=>"mode-autochess",_=>"mode-multiplayer"};
            int first=draw.VtxBuffer.Count;
            homeArtwork.Draw(art,at,new(setWidth,setHeight),255,true,.3f,draw);
            draw.AddRectFilledMultiColor(at,at+new float2(setWidth,setHeight),new ImColor8(1,10,19,64),new ImColor8(1,10,19,64),new ImColor8(1,10,19,235),new ImColor8(1,10,19,235));
            S.TextFit(draw,new(at.X+11,at.Y+setHeight-1-7-13.5f-18.9f+(18.9f-nameType.Size)*.5f),S.Gold1,set.Name.ToUpperInvariant(),nameType,setWidth-22);
            string sub=$"{set.Traits.Length} traits · {CodexTitle(set.Pace)}";
            S.TextFit(draw,new(at.X+11,at.Y+setHeight-1-7-13.5f+(13.5f-small.Size)*.5f),S.LcMute,sub,small,setWidth-22);
            // filter: saturate(.55) brightness(.8) unless hovered or chosen.
            if(!on&&!hover)S.Filter(draw,first,.45f,.8f);
            if(locked)S.Filter(draw,first,0,.55f);
            if(Session is not null&&on)
            {
                var live=S.Display(9,.08f);float lw=live.Width("IN GAME")+10;var la=new float2(at.X+setWidth-6-lw,at.Y+5);
                draw.AddRectFilled(la,la+new float2(lw,14),S.Accent);S.TextIn(draw,la+new float2(5,0),14,S.TextInv,"IN GAME",live);
            }
            if(on){for(int i=1;i<=6;i++)draw.AddRect(at-new float2(1+i,1+i),at+new float2(setWidth+1+i,setHeight+1+i),new ImColor8(240,210,140,(byte)(20*(1-i/7f))),thickness:2);draw.AddRect(at-new float2(1,1),at+new float2(setWidth+1,setHeight+1),S.Gold2);}
            draw.AddRect(at,at+new float2(setWidth,setHeight),on?S.Gold1:hover?S.Gold3:S.Gold5);
            if(chosen&&Session is null){codexVariant=set.Id;codexKind=codexCost="all";codexResetScroll=true;}
            x+=setWidth+6;
        }
        ImGui.SetCursorScreenPos(origin);ImGui.Dummy(new(x,58));
        ImGui.EndChild();
        ImGui.SetCursorScreenPos(start+new float2(0,58));
    }
    /// <summary>.cx-filters: one bordered .cx-seg of small buttons (traits: kinds; Digimon: tiers in their tier colours, Wild).</summary>
    private void CodexFilterSegment(float2 window,float width)
    {
        var draw=ImGui.GetWindowDrawList();var at=new float2(window.X+14,ImGui.GetCursorScreenPos().Y+8);
        if(codexTab is not("traits" or "digimon"))
        {
            ImGui.SetCursorScreenPos(new(window.X+14,at.Y+4));ImGui.Dummy(new(0,0));return;
        }
        var filters=codexTab=="traits"?new[]{"all"}.Concat(CodexData.TraitKinds).ToArray():new[]{"all"}.Concat(CodexData.Lines.Where(l=>l.Shop).Select(l=>l.Tier).Distinct().Order().Select(t=>t.ToString())).Concat(CodexData.Lines.Any(l=>!l.Shop)?["wild"]:[]).ToArray();
        var type=S.Body(12);const float buttonHeight=22.2f;
        string Label(string value)=>value switch{"family"=>"Families","role"=>"Roles","all"=>"All",_=>int.TryParse(value,out _)?value+" gold":CodexTitle(value)};
        float segWidth=4+filters.Sum(f=>type.Width(Label(f))+20)+2*(filters.Length-1);
        draw.AddRectFilled(at,at+new float2(segWidth,buttonHeight+6),CodexBg);draw.AddRect(at,at+new float2(segWidth,buttonHeight+6),CodexLine);
        float x=at.X+3;
        foreach(string value in filters)
        {
            string label=Label(value);float w=type.Width(label)+20;var b=new float2(x,at.Y+3);
            bool on=(codexTab=="traits"?codexKind:codexCost)==value;
            bool pressed=CosmeticHit("codexFilter/"+value,b,new(w,buttonHeight),out bool hover);
            ImColor8? tier=int.TryParse(value,out int cost)?CodexTierColor(cost):null;
            if(on){draw.AddRectFilled(b,b+new float2(w,buttonHeight),CodexPanel3);draw.AddRect(b,b+new float2(w,buttonHeight),tier??CodexLine2);}
            S.TextIn(draw,b+new float2(10,0),buttonHeight,on?tier??S.Text1:hover?S.Text1:CodexSegMuted,label,type);
            if(pressed){if(codexTab=="traits")codexKind=value;else codexCost=value;codexResetScroll=true;}
            x+=w+2;
        }
        float bottom=at.Y+buttonHeight+6+8;
        draw.AddLine(new(window.X+1,bottom+.5f),new(window.X+width-1,bottom+.5f),CodexLine);
        ImGui.SetCursorScreenPos(new(window.X+14,bottom+14));ImGui.Dummy(new(0,0));
    }
    private static ImColor8 CodexTierColor(int tier)=>tier switch{1=>new(154,166,187),2=>new(95,211,106),3=>new(79,179,255),4=>new(199,125,255),_=>new(255,179,71)};
}

using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private static readonly Lazy<CodexCatalog> codexCatalog=new(()=>new(Path.Combine(AppContext.BaseDirectory,"content/codex.json")));
    private readonly ImInputString codexSearch=new(256);
    private bool codexFull,codexSide,codexSideFocus,codexPool,codexSideRules,codexSearchFocused,codexConsumedShortcut,codexResetScroll;
    private string codexTab="traits",codexKind="all",codexCost="all";
    private string? codexVariant,codexUnit,codexFocusTrait,codexFocusItem;
    private string? codexModalLine;
    private int codexModalFrame;
    private Dictionary<string,int> codexCounts=[];
    private Dictionary<string,PlacedUnit[]> codexOwned=[];
    internal bool CodexVisible=>codexFull||codexSide;
    internal bool CodexFullVisible=>codexFull;
    internal string CodexTab=>codexTab;
    internal string CodexVariantId=>CodexData.Id;
    internal string? CodexUnit=>codexUnit;
    internal string CodexQuery=>codexSearch.ToString();
    internal int CodexRows {get;private set;}
    internal int CodexPortraits {get;private set;}
    internal int CodexRecipeCells {get;private set;}
    internal float2 CodexBodyMin {get;private set;}
    internal float2 CodexBodyMax {get;private set;}
    internal IReadOnlyDictionary<string,int> CodexBoardCounts=>codexCounts;
    private CodexVariant CodexData=>codexCatalog.Value.Variant(Session is not null?Rules.Id:codexVariant??Rules.Id);
    private void ToggleCodex()
    {
        if(codexFull){codexFull=false;codexUnit=null;return;}
        if(ClientHomeVisible)OpenCodexFull();else if(codexSide){codexSide=false;codexUnit=null;}else OpenCodexPanel();
    }
    private void OpenCodexFull(string? tab=null)
    {
        if(!codexFull)codexVariant=Rules.Id;
        if(tab is not null)codexTab=tab;
        codexFull=true;codexUnit=null;codexResetScroll=true;
        CancelArenaDrag();cameraDrag=null;
    }
    private void OpenCodexPanel(string? trait=null)
    {
        if(Session is null)return;
        codexSide=true;codexSideFocus=true;codexUnit=null;codexSideRules=false;
        if(trait is not null)
        {
            codexFocusTrait=trait;
            var counts=TraitRules.Count(Rules,Session.CombatProjection.Board(Human),Human.Augments.ToArray());
            if(counts.GetValueOrDefault(trait)==0)codexPool=true;
        }
    }
    private void CodexShortcuts()
    {
        codexConsumedShortcut=false;
        if(ClientHomeVisible)codexSide=false;
        if(KeyboardPopup||keyboardPopupLastFrame)
        {
            codexConsumedShortcut=ImGui.IsKeyPressed(ImGuiKey.Escape,false);
            return;
        }
        bool blocked=showCollection||showFarm||showProfile||showMultiplayer||showMusicLibrary||showAudio||showGameMenu||showAbout||showSaveData||showSettings||showHomePlay||EliminationCinematicActive||Session is not null&&Human.Offer is not null;
        if(blocked)return;
        var io=ImGui.GetIO();
        if(CodexVisible&&ImGui.IsKeyPressed(ImGuiKey.Escape,false))
        {
            codexConsumedShortcut=true;
            if(codexUnit is not null)codexUnit=null;
            else if(codexFull&&codexSearchFocused&&codexSearch.ToString().Length>0)codexResetScroll=true;
            else if(codexFull)codexFull=false;
            else codexSide=false;
        }
        else if(!io.WantTextInput&&!io.KeyCtrl&&!io.KeyAlt&&!io.KeySuper&&ImGui.IsKeyPressed(ImGuiKey.C,false))
        {ToggleCodex();codexConsumedShortcut=true;}
    }
    private void CodexContext()
    {
        CodexRows=CodexPortraits=CodexRecipeCells=0;
        codexCounts=Session is null?[]:new(TraitRules.Count(Rules,Session.CombatProjection.Board(Human),Human.Augments.ToArray()));
        codexOwned=Session is null?[]:Human.Roster.Entries.GroupBy(entry=>entry.Unit.Line).ToDictionary(group=>group.Key,group=>group.ToArray());
    }
    private static ImColor8 CodexColor(string color)
    {
        if(color.Length==7&&color[0]=='#'&&uint.TryParse(color.AsSpan(1),System.Globalization.NumberStyles.HexNumber,null,out uint value))return new((byte)(value>>16),(byte)(value>>8),(byte)value);
        return new(142,152,173);
    }
    private static int CodexKindOrder(string kind)=>kind=="family"?0:kind=="role"?1:2;
    private static string CodexTitle(string text)=>text.Length==0?text:char.ToUpperInvariant(text[0])+text[1..];
    private static void CodexText(string value,ImColor8? color=null,float font=17)
    {
        ImGui.PushFont(default,font);ImGui.PushStyleColor(ImGuiCol.Text,color??CodexBody);
        UiText.Wrapped(value);
        ImGui.PopStyleColor();ImGui.PopFont();
    }
    private bool CodexMatch(string text)=>CodexCatalog.Matches(codexSearch.ToString(),text);
    private int CodexCount(string tab)=>tab switch
    {
        "traits"=>CodexData.Traits.Count(x=>CodexMatch(x.SearchText)),"digimon"=>CodexData.Lines.Count(x=>CodexMatch(x.SearchText)),
        "items"=>CodexData.Items.Count(x=>CodexMatch(x.SearchText)),"augments"=>CodexData.Augments.Count(x=>CodexMatch(x.SearchText)),
        "wisps"=>CodexData.Wisps.Count(x=>CodexMatch(x.SearchText)),_=>CodexData.Rules.Count(x=>CodexMatch(x.SearchText))
    };
    private void DrawCodexFull()
    {
        CodexContext();var data=CodexData;
        if(codexUnit is null)codexModalLine=null;
        if(!data.Tabs.Contains(codexTab))codexTab="traits";
        float pageTop=ClientPageTop+12,width=Math.Min(1720,size.X-48),left=(size.X-width)/2;
        CodexBeginWindow("Codex",new(left,pageTop),new(width,size.Y-pageTop-16));
        ImGui.BeginDisabled(codexUnit is not null);
        var headerAt=ImGui.GetCursorScreenPos();float searchWidth=Math.Clamp(width*.25f,180,260);
        CodexHeading("CODEX",28);CodexText(data.Name,CodexGold);
        float headerBottom=ImGui.GetCursorScreenPos().Y;
        ImGui.SetCursorScreenPos(new(left+width-searchWidth-120,headerAt.Y+7));ImGui.SetNextItemWidth(searchWidth);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,1);ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,0);
        if(ImGui.InputTextWithHint("##codexSearch"u8,"Search the Codex"u8,codexSearch,ImGuiInputTextFlags.EscapeClearsAll))codexResetScroll=true;
        ImGui.PopStyleVar(2);codexSearch.EvaluateLength();
        codexSearchFocused=ImGui.IsItemActive();Buttons["codexSearch"]=(ImGui.GetItemRectMin()+ImGui.GetItemRectMax())*.5f;
        ImGui.SameLine(0,12);if(CodexButton("closeCodex","Close",88)){codexFull=false;codexUnit=null;}
        ImGui.SetCursorScreenPos(new(headerAt.X,headerBottom+3));
        float tabRight=left+width-16;bool firstTab=true;
        foreach(string tab in data.Tabs)
        {
            string label=tab=="wisps"?"Data Wisps":CodexTitle(tab);
            label+=tab=="rules"?"":$" {CodexCount(tab)}";float tabWidth=ImGui.CalcTextSize(label).X+26;
            if(!firstTab)CodexFlowNext(tabWidth,tabRight);firstTab=false;
            if(CodexButton("codexTab/"+tab,label,tabWidth,38,codexTab==tab)){codexTab=tab;codexResetScroll=true;}
        }
        ImGui.Separator();
        CodexSets();CodexFilters();
        ImGui.BeginChild("Codex body"u8,new(0,0));
        CodexBodyMin=ImGui.GetWindowDrawList().GetClipRectMin();CodexBodyMax=ImGui.GetWindowDrawList().GetClipRectMax();
        if(codexResetScroll){ImGui.SetScrollY(0);codexResetScroll=false;}
        switch(codexTab)
        {
            case "traits":CodexTraitList(false);break;
            case "digimon":CodexLineList();break;
            case "items":CodexItems();break;
            case "augments":CodexAugments();break;
            case "wisps":CodexWisps();break;
            case "rules":CodexRules(false);break;
        }
        if(CodexRows==0){ImGui.Spacing();CodexHeading("Nothing matches these filters.");CodexText("Search: \""+codexSearch.ToString()+"\"",CodexMuted);}
        ImGui.EndChild();ImGui.EndDisabled();
        if(codexUnit is not null)ImGui.GetWindowDrawList().AddRectFilled(new(left,pageTop),new(left+width,size.Y-16),new ImColor8(5,7,12,175));
        CodexEndWindow();
        if(codexUnit is not null)CodexUnitModal();
    }
    private void CodexSets()
    {
        var data=CodexData;
        ImGui.BeginChild("Codex sets"u8,new(0,82),ImGuiChildFlags.None,ImGuiWindowFlags.HorizontalScrollbar);
        bool first=true;
        foreach(var set in codexCatalog.Value.Variants.GroupBy(v=>v.SetId).Select(group=>group.FirstOrDefault(v=>v.Pace==data.Pace)??group.First()))
        {
            if(!first)ImGui.SameLine();first=false;
            bool locked=Session is not null&&set.SetId!=data.SetId;ImGui.BeginDisabled(locked);
            const float setWidth=198,setHeight=62;var at=ImGui.GetCursorScreenPos();bool chosen=Button("codexSet/"+set.SetId,"##codexSet/"+set.SetId,setWidth,setHeight);
            string art=set.SetId switch{"faction-wars"=>"mode-faction-wars","dragonlands"=>"mode-dragonlands","autochess"=>"mode-autochess",_=>"mode-multiplayer"};
            homeArtwork.Draw(art,at,new(setWidth,setHeight),locked?(byte)55:(byte)210,true);
            var draw=ImGui.GetWindowDrawList();draw.AddRectFilledMultiColor(at,at+new float2(setWidth,setHeight),new ImColor8(1,10,19,75),new ImColor8(1,10,19,75),new ImColor8(1,10,19,245),new ImColor8(1,10,19,245));
            draw.AddRect(at,at+new float2(setWidth,setHeight),data.SetId==set.SetId?CodexInk:new ImColor8(120,90,40));
            draw.AddText(at+new float2(9,12),locked?CodexMuted:CodexInk,set.Name);
            draw.AddText(at+new float2(9,37),CodexMuted,$"{set.Traits.Length} traits"+(Session is not null&&set.SetId==data.SetId?" / In game":""));
            if(data.SetId==set.SetId)draw.AddRectFilled(at+new float2(1,setHeight-3),at+new float2(setWidth-1,setHeight-1),CodexGold);
            ImGui.EndDisabled();
            if(chosen&&Session is null){codexVariant=set.Id;codexKind=codexCost="all";codexResetScroll=true;}
        }
        ImGui.EndChild();
    }
    private void CodexFilters()
    {
        if(codexTab is not("traits" or "digimon"))return;
        var filters=codexTab=="traits"?new[]{"all"}.Concat(CodexData.TraitKinds):new[]{"all"}.Concat(CodexData.Lines.Where(l=>l.Shop).Select(l=>l.Tier).Distinct().Order().Select(t=>t.ToString())).Concat(CodexData.Lines.Any(l=>!l.Shop)?["wild"]:[]);
        bool first=true;float right=ImGui.GetCursorScreenPos().X+ImGui.GetContentRegionAvail().X;
        foreach(string value in filters)
        {
            string label=value switch{"family"=>"Families","role"=>"Roles",_=>int.TryParse(value,out _)?value+" gold":CodexTitle(value)};
            float width=ImGui.CalcTextSize(label).X+26;if(!first)CodexFlowNext(width,right);first=false;
            if(CodexButton("codexFilter/"+value,label,width,36,(codexTab=="traits"?codexKind:codexCost)==value))
            {if(codexTab=="traits")codexKind=value;else codexCost=value;codexResetScroll=true;}
        }
        ImGui.Separator();
    }
    private void DrawCodexSide()
    {
        if(!codexSide||Session is null||Session.State.Over)return;
        CodexContext();
        if(codexSideFocus){ImGui.SetNextWindowFocus();codexSideFocus=false;}
        CodexBeginWindow("Codex side panel",new(12,Top),new(Left-24,Height),compact:true);
        CodexHeading("CODEX",22,CodexGold);ImGui.SameLine(0,6);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,new float2(4,9));
        if(CodexButton("codexExpand","Full",48,font:16))OpenCodexFull(codexUnit is not null?"digimon":codexSideRules?"rules":"traits");
        ImGui.SameLine(0,6);if(CodexButton("closeCodexSide","Close",51,font:16)){codexSide=false;codexUnit=null;}
        float buttonWidth=(ImGui.GetContentRegionAvail().X-8)/2;
        if(CodexButton("codexBoard","My board",buttonWidth,36,!codexPool&&!codexSideRules,font:16)){codexPool=false;codexSideRules=false;codexUnit=null;codexResetScroll=true;}
        ImGui.SameLine();if(CodexButton("codexPool","All in game",buttonWidth,36,codexPool&&!codexSideRules,font:16)){codexPool=true;codexSideRules=false;codexUnit=null;codexResetScroll=true;}
        if(CodexButton("codexSideRules","Rules",ImGui.GetContentRegionAvail().X,36,codexSideRules)){codexSideRules=true;codexUnit=null;codexResetScroll=true;}
        ImGui.PopStyleVar();
        ImGui.BeginChild("Codex side body"u8,new(0,0));
        CodexBodyMin=ImGui.GetWindowDrawList().GetClipRectMin();CodexBodyMax=ImGui.GetWindowDrawList().GetClipRectMax();
        if(codexResetScroll){ImGui.SetScrollY(0);codexResetScroll=false;}
        if(codexSideRules)CodexRules(true);
        else if(codexUnit is not null)
        {
            if(CodexButton("codexBack","Back to traits",ImGui.GetContentRegionAvail().X)){codexUnit=null;codexResetScroll=true;}
            else CodexUnitDetail(CodexData.Lines.Single(line=>line.Id==codexUnit));
        }
        else CodexTraitList(true);
        ImGui.EndChild();CodexEndWindow();
    }
    private void CodexGrid<T>(string id,IEnumerable<T> entries,int minWidth,Action<T> draw)
    {
        int columns=Math.Clamp((int)(ImGui.GetContentRegionAvail().X/minWidth),1,4);
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding,new float2(6,6));
        if(ImGui.BeginTable(id,columns,ImGuiTableFlags.SizingStretchSame))
        {
            foreach(var entry in entries){ImGui.TableNextColumn();draw(entry);ImGui.Spacing();}
            ImGui.EndTable();
        }
        ImGui.PopStyleVar();
    }
}

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
        bool blocked=showCollection||showFarm||showProfile||showMultiplayer||showMusicLibrary||showAudio||showGameMenu||showAbout||showSaveData||showSettings||EliminationCinematicActive||Session is not null&&Human.Offer is not null;
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
    private static void CodexText(string value,ImColor8? color=null)
    {
        if(color is {} tint)ImGui.PushStyleColor(ImGuiCol.Text,tint);
        UiText.Wrapped(value);
        if(color is not null)ImGui.PopStyleColor();
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
        HomeCard("Codex",new(16,16),new(size.X-32,size.Y-32),false);
        ImGui.BeginDisabled(codexUnit is not null);
        HomeLabel("CODEX / "+data.Name.ToUpperInvariant(),23,HomeInk);
        ImGui.SameLine(ImGui.GetWindowWidth()-100);if(HomeButton("closeCodex","Close",76,28)){codexFull=false;codexUnit=null;}
        foreach(string tab in data.Tabs)
        {
            if(tab!=data.Tabs[0])ImGui.SameLine();
            string label=tab=="wisps"?"Data Wisps":CodexTitle(tab);
            if(HomeButton("codexTab/"+tab,label+(tab=="rules"?"":$" {CodexCount(tab)}"),0,30,codexTab==tab)){codexTab=tab;codexResetScroll=true;}
        }
        ImGui.SameLine();ImGui.SetNextItemWidth(Math.Max(140,ImGui.GetContentRegionAvail().X));
        if(ImGui.InputTextWithHint("##codexSearch"u8,"Search"u8,codexSearch,ImGuiInputTextFlags.EscapeClearsAll))codexResetScroll=true;
        codexSearch.EvaluateLength();
        codexSearchFocused=ImGui.IsItemActive();Buttons["codexSearch"]=(ImGui.GetItemRectMin()+ImGui.GetItemRectMax())*.5f;
        CodexSets();CodexFilters();
        ImGui.BeginChild("Codex body"u8,new(0,0));
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
        if(CodexRows==0)CodexText("Nothing matches \""+codexSearch.ToString()+"\".",HomeMute);
        ImGui.EndChild();ImGui.EndDisabled();
        if(codexUnit is not null)ImGui.GetWindowDrawList().AddRectFilled(new(16,16),new(size.X-16,size.Y-16),new ImColor8(5,7,12,155));
        ImGui.End();
        if(codexUnit is not null)CodexUnitModal();
    }
    private void CodexSets()
    {
        var data=CodexData;
        ImGui.BeginChild("Codex sets"u8,new(0,72),ImGuiChildFlags.None,ImGuiWindowFlags.HorizontalScrollbar);
        bool first=true;
        foreach(var set in codexCatalog.Value.Variants.GroupBy(v=>v.SetId).Select(group=>group.FirstOrDefault(v=>v.Pace==data.Pace)??group.First()))
        {
            if(!first)ImGui.SameLine();first=false;
            bool locked=Session is not null&&set.SetId!=data.SetId;ImGui.BeginDisabled(locked);
            var at=ImGui.GetCursorScreenPos();bool chosen=Button("codexSet/"+set.SetId,"##codexSet/"+set.SetId,166,48);
            string art=set.SetId switch{"faction-wars"=>"mode-faction-wars","dragonlands"=>"mode-dragonlands","autochess"=>"mode-autochess",_=>"mode-multiplayer"};
            homeArtwork.Draw(art,at,new(166,48),locked?(byte)55:(byte)160,true);
            var draw=ImGui.GetWindowDrawList();draw.AddRectFilled(at,at+new float2(166,48),new ImColor8(3,12,24,135));
            draw.AddRect(at,at+new float2(166,48),data.SetId==set.SetId?HomeGold:HomeLine,3);
            draw.AddText(at+new float2(7,6),locked?HomeMute:HomeInk,set.Name);
            draw.AddText(at+new float2(7,27),HomeMute,$"{set.Traits.Length} traits"+(Session is not null&&set.SetId==data.SetId?" / In game":""));
            ImGui.EndDisabled();
            if(chosen&&Session is null){codexVariant=set.Id;codexKind=codexCost="all";codexResetScroll=true;}
        }
        ImGui.EndChild();
    }
    private void CodexFilters()
    {
        if(codexTab is not("traits" or "digimon"))return;
        var filters=codexTab=="traits"?new[]{"all"}.Concat(CodexData.TraitKinds):new[]{"all"}.Concat(CodexData.Lines.Where(l=>l.Shop).Select(l=>l.Tier).Distinct().Order().Select(t=>t.ToString())).Concat(CodexData.Lines.Any(l=>!l.Shop)?["wild"]:[]);
        bool first=true;
        foreach(string value in filters)
        {
            if(!first)ImGui.SameLine();first=false;
            string label=value switch{"family"=>"Families","role"=>"Roles",_=>int.TryParse(value,out _)?value+" gold":CodexTitle(value)};
            if(HomeButton("codexFilter/"+value,label,0,26,(codexTab=="traits"?codexKind:codexCost)==value))
            {if(codexTab=="traits")codexKind=value;else codexCost=value;codexResetScroll=true;}
        }
        ImGui.Separator();
    }
    private void DrawCodexSide()
    {
        if(!codexSide||Session is null||Session.State.Over)return;
        CodexContext();
        if(codexSideFocus){ImGui.SetNextWindowFocus();codexSideFocus=false;}
        ImGui.SetNextWindowBgAlpha(1);ImGui.PushStyleColor(ImGuiCol.WindowBg,new ImColor8(5,15,25));
        Window("Codex side panel",12,Top,208,Height,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoScrollbar);ImGui.PopStyleColor();
        HomeLabel("CODEX",14);ImGui.SameLine();
        if(Button("codexExpand","Full",44,24))OpenCodexFull(codexUnit is not null?"digimon":codexSideRules?"rules":"traits");
        ImGui.SameLine();if(Button("closeCodexSide","Close",49,24)){codexSide=false;codexUnit=null;}
        if(Button("codexBoard","My board",85,25,!codexPool&&!codexSideRules)){codexPool=false;codexSideRules=false;codexUnit=null;codexResetScroll=true;}
        ImGui.SameLine();if(Button("codexPool","All in game",91,25,codexPool&&!codexSideRules)){codexPool=true;codexSideRules=false;codexUnit=null;codexResetScroll=true;}
        if(Button("codexSideRules","Rules",0,24,codexSideRules)){codexSideRules=true;codexUnit=null;codexResetScroll=true;}
        ImGui.BeginChild("Codex side body"u8,new(0,0));
        if(codexResetScroll){ImGui.SetScrollY(0);codexResetScroll=false;}
        if(codexSideRules)CodexRules(true);
        else if(codexUnit is not null)
        {
            if(Button("codexBack","Back to traits")){codexUnit=null;codexResetScroll=true;}
            else CodexUnitDetail(CodexData.Lines.Single(line=>line.Id==codexUnit));
        }
        else CodexTraitList(true);
        ImGui.EndChild();ImGui.End();
    }
    private void CodexGrid<T>(string id,IEnumerable<T> entries,int minWidth,Action<T> draw)
    {
        int columns=Math.Clamp((int)(ImGui.GetContentRegionAvail().X/minWidth),1,4);
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding,new float2(10,9));
        if(ImGui.BeginTable(id,columns,ImGuiTableFlags.SizingStretchSame|ImGuiTableFlags.BordersInner))
        {
            foreach(var entry in entries){ImGui.TableNextColumn();draw(entry);ImGui.Spacing();}
            ImGui.EndTable();
        }
        ImGui.PopStyleVar();
    }
}

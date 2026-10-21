using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;
using System.Text.RegularExpressions;

namespace DigiTactics.Desktop;

/// <summary>Uses the same mouse and key events as a player; verifies catalog browsing never changes the live set.</summary>
internal sealed class CodexSmoke
{
    private readonly CodexCatalog catalog=new(Path.Combine(AppContext.BaseDirectory,"content/codex.json"));
    private int homeStep,liveStep,cooldown=3;
    private bool down;
    private ImGuiKey key;
    private string? liveVariant;
    private string? recipeItem;
    private string? selectedTarget;
    private float2 selectedPoint;
    private CodexLine? longUnit;
    private string? longSet;
    private float longScrollStart,longScrollRange;
    private int longScrollAttempts,setScrollAttempts;
    private bool longFooterReached;
    private string? rulesVariant;
    private readonly HashSet<string> expectedRuleSections=[],expectedRuleChips=[],expectedRuleCells=[];
    private readonly HashSet<string> seenRuleSections=[],seenRuleChips=[],seenRuleCells=[];
    private bool rulesReturning,rulesComplete,rulesTopCaptured,rulesBottomCaptured;
    private int rulesEdgeSamples,rulesScrollAttempts;
    private float rulesScrollRange,rulesScrollFurthest;
    public bool HomeDone {get;private set;}
    public bool LiveDone {get;private set;}
    public string HomePending=>$"Codex home/{homeStep}"+(homeStep==10?$" Rules {(rulesReturning?"up":"down")}: {RulesCoverage()}":"");
    private bool Ready()
    {
        if(cooldown-->0)return false;
        if(!down)return true;
        if(key!=ImGuiKey.None)ImGui.GetIO().AddKeyEvent(key,false);else ImGui.GetIO().AddMouseButtonEvent(0,false);
        down=false;key=ImGuiKey.None;cooldown=3;return false;
    }
    private void Press(ImGuiKey value){key=value;ImGui.GetIO().AddKeyEvent(value,true);down=true;}
    private bool Click(MatchScreen screen,string button)
    {
        if(!screen.Buttons.TryGetValue(button,out var point))return false;
        ImGui.GetIO().AddMousePosEvent(point.X,point.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);down=true;return true;
    }
    private bool VisibleClick(MatchScreen screen,string prefix,bool side=false)
    {
        // The body moves with header/font/viewport size. A center recorded by Button
        // can be above its child clip rectangle even while a portrait sliver is visible.
        var min=screen.CodexBodyMin+new float2(20,20);var max=screen.CodexBodyMax-new float2(20,20);
        var pair=screen.Buttons.FirstOrDefault(pair=>pair.Key.StartsWith(prefix,StringComparison.Ordinal)&&pair.Value.X>=min.X&&pair.Value.X<=max.X&&pair.Value.Y>=min.Y&&pair.Value.Y<=max.Y);
        if(pair.Key is not null)
        {
            selectedTarget=pair.Key;selectedPoint=pair.Value;
            if(prefix=="codexRecipe/"){var indices=pair.Key.Split('/');recipeItem=catalog.Variant(screen.CodexVariantId).RecipeGrid[int.Parse(indices[1])][int.Parse(indices[2])];}
            return Click(screen,pair.Key);
        }
        var center=(screen.CodexBodyMin+screen.CodexBodyMax)*.5f;
        ImGui.GetIO().AddMousePosEvent(center.X,center.Y);ImGui.GetIO().AddMouseWheelEvent(0,side?-.5f:-4);cooldown=3;return false;
    }
    private bool VisibleSetClick(MatchScreen screen,string set)
    {
        string button="codexSet/"+set;
        if(!screen.Buttons.TryGetValue(button,out var point))return false;
        // Set cards are 170 pixels wide. Scroll the actual set strip until the
        // entire card is visible, rather than clicking an offscreen button center.
        float left=screen.CodexBodyMin.X+85-1,right=screen.CodexBodyMax.X-85+1;
        if(point.X>=left&&point.X<=right){setScrollAttempts=0;return Click(screen,button);}
        if(++setScrollAttempts>30)throw new InvalidOperationException($"Codex set {set} could not be scrolled into view.");
        ImGui.GetIO().AddMousePosEvent((left+right)*.5f,point.Y);
        ImGui.GetIO().AddMouseWheelEvent(point.X<left?6:-6,0);cooldown=3;return false;
    }
    private void ScrollLongDetail(MatchScreen screen)
    {
        var center=(screen.CodexDetailViewport.Min+screen.CodexDetailViewport.Max)*.5f;
        ImGui.GetIO().AddMousePosEvent(center.X,center.Y);ImGui.GetIO().AddMouseWheelEvent(0,-8);cooldown=4;
        if(++longScrollAttempts>12)throw new InvalidOperationException($"Codex {longUnit!.Id} footer was not reachable after scrolling: {screen.CodexDetailFooterBounds}; viewport {screen.CodexDetailViewport}.");
    }
    private static bool FullFooterVisible(MatchScreen screen)
    {
        var footer=screen.CodexDetailFooterBounds;var viewport=screen.CodexDetailViewport;
        return footer.Max.Y>footer.Min.Y&&footer.Min.Y>=viewport.Min.Y-1&&footer.Max.Y<=viewport.Max.Y+1;
    }
    private void VerifyLongDetail(MatchScreen screen,bool footer=false)
    {
        if(screen.CodexUnit!=longUnit!.Id)throw new InvalidOperationException($"Codex did not open the longest description: {longUnit.Id}.");
        var text=screen.CodexDetailTextBounds;var viewport=screen.CodexDetailViewport;var bounds=screen.CodexDetailFooterBounds;
        if(text.Max.X<=text.Min.X||text.Min.X<viewport.Min.X-1||text.Max.X>viewport.Max.X+1||
            bounds.Min.X<viewport.Min.X-1||bounds.Max.X>viewport.Max.X+1||screen.CodexDetailScrollMaxX>1)
            throw new InvalidOperationException($"Codex {longUnit.Id} clips horizontally: text {text}, footer {bounds}, viewport {viewport}, horizontal range {screen.CodexDetailScrollMaxX}.");
        if(footer&&(!FullFooterVisible(screen)||longScrollRange>1&&screen.CodexDetailScrollY<=longScrollStart+.5f))
            throw new InvalidOperationException($"Codex {longUnit.Id} did not reveal its full footer by scrolling.");
    }
    private string RulesCoverage()=>$"sections {seenRuleSections.Count}/{expectedRuleSections.Count}, chips {seenRuleChips.Count}/{expectedRuleChips.Count}, table cells {seenRuleCells.Count}/{expectedRuleCells.Count}";
    private void ObserveRules(MatchScreen screen)
    {
        if(rulesVariant is null)
        {
            rulesVariant=screen.CodexVariantId;
            foreach(var rule in catalog.Variant(rulesVariant).Rules)
            {
                expectedRuleSections.Add(rule.Id);int table=0;
                foreach(var block in rule.Blocks.Where(block=>block.Kind=="table"))
                {
                    for(int row=0;row<block.Rows.Length;row++)for(int column=0;column<block.Rows[row].Length;column++)
                        expectedRuleCells.Add($"{rule.Id}/{table}/{row}/{column}");
                    table++;
                }
                // Only variants whose source includes colored round chips should
                // require them. Normal pace, for example, has none.
                foreach(Match chip in Regex.Matches(rule.Html,@"<span\b[^>]*\bclass=""[^""]*\bcx-rd\b[^""]*""[^>]*>\s*<b>([^<]+)</b>"))
                    expectedRuleChips.Add(rule.Id+"/"+chip.Groups[1].Value);
            }
        }
        if(screen.CodexVariantId!=rulesVariant||screen.CodexTab!="rules")throw new InvalidOperationException("Codex changed content during the Rules scroll check.");
        if(!screen.CodexRuleTableTextFits)throw new InvalidOperationException($"Codex Rules clips table text in {rulesVariant}: {screen.CodexRuleTableLayoutError}");
        seenRuleSections.UnionWith(screen.CodexRuleVisibleSections);
        seenRuleChips.UnionWith(screen.CodexRuleVisibleRoundChips);
        seenRuleCells.UnionWith(screen.CodexRuleVisibleTableCells);
        rulesScrollRange=Math.Max(rulesScrollRange,screen.CodexBodyScrollMaxY);
        rulesScrollFurthest=Math.Max(rulesScrollFurthest,screen.CodexBodyScrollY);
    }
    private bool ScrollRules(MatchScreen screen)
    {
        ObserveRules(screen);if(rulesComplete)return true;
        if(++rulesScrollAttempts>64)throw new InvalidOperationException($"Codex Rules could not finish scrolling {rulesVariant}: {RulesCoverage()}, position {screen.CodexBodyScrollY:0.##}/{screen.CodexBodyScrollMaxY:0.##}, returning {rulesReturning}.");
        bool edge=rulesReturning?screen.CodexBodyScrollY<=.5f:screen.CodexBodyScrollY>=screen.CodexBodyScrollMaxY-.5f;
        if(edge)
        {
            // Auto-sized cards settle across frames; require two settled samples
            // at each end before treating the body as fully traversed.
            if(++rulesEdgeSamples<2){cooldown=3;return false;}
            rulesEdgeSamples=0;
            if(!rulesReturning){rulesReturning=true;cooldown=3;return false;}
            if(!seenRuleSections.SetEquals(expectedRuleSections)||!seenRuleChips.SetEquals(expectedRuleChips)||!seenRuleCells.SetEquals(expectedRuleCells)||rulesScrollRange>1&&rulesScrollFurthest<=.5f)
                throw new InvalidOperationException($"Codex Rules full scroll missed content in {rulesVariant}: {RulesCoverage()}; missing sections [{string.Join(", ",expectedRuleSections.Except(seenRuleSections))}], chips [{string.Join(", ",expectedRuleChips.Except(seenRuleChips))}], table cells [{string.Join(", ",expectedRuleCells.Except(seenRuleCells).Take(18))}]; position {screen.CodexBodyScrollY:0.##}/{screen.CodexBodyScrollMaxY:0.##}, furthest {rulesScrollFurthest:0.##}/{rulesScrollRange:0.##}.");
            rulesComplete=true;Console.WriteLine($"Codex Rules full scroll passed: {rulesVariant}, {RulesCoverage()}, all table labels fit, scroll {rulesScrollFurthest:0.##}/{rulesScrollRange:0.##} and back to top.");return true;
        }
        rulesEdgeSamples=0;
        var center=(screen.CodexBodyMin+screen.CodexBodyMax)*.5f;
        float wheel=Math.Clamp((screen.CodexBodyMax.Y-screen.CodexBodyMin.Y-70)/160,.5f,2);
        ImGui.GetIO().AddMousePosEvent(center.X,center.Y);ImGui.GetIO().AddMouseWheelEvent(0,rulesReturning?wheel:-wheel);cooldown=3;return false;
    }
    public void HomeInput(MatchScreen screen)
    {
        if(HomeDone||!Ready())return;
        bool advance=true;
        switch(homeStep)
        {
            case 0:Press(ImGuiKey.C);break;
            case 1:advance=Click(screen,"codexFilter/family");break;
            case 2:advance=Click(screen,"codexTab/digimon");break;
            case 3:advance=VisibleClick(screen,"codexLine/");break;
            case 4:Press(ImGuiKey.Escape);break;
            case 5:advance=Click(screen,"codexTab/items");break;
            case 6:advance=VisibleClick(screen,"codexRecipe/");break;
            case 7:advance=Click(screen,"codexTab/augments");break;
            case 8:advance=Click(screen,"codexTab/wisps");break;
            case 9:advance=Click(screen,"codexTab/rules");break;
            case 10:advance=ScrollRules(screen)&&Click(screen,"codexSearch");break;
            case 11:ImGui.GetIO().AddInputCharactersUTF8("no-such-digimon-xyz"u8);cooldown=4;break;
            case 12:Press(ImGuiKey.Escape);break;
            case 13:advance=Click(screen,"codexSet/fates");break;
            case 14:advance=Click(screen,"codexSet/faction-wars");break;
            case 15:Press(ImGuiKey.Escape);break;
            case 16:
                if(screen.CodexVisible)throw new InvalidOperationException("Codex did not close to Home.");
                // Select from the catalog, so the case follows the actual longest
                // description as content changes instead of using synthetic text.
                var longest=catalog.Variants.SelectMany(variant=>variant.Lines.Select(line=>(Variant:variant,Line:line)))
                    .OrderByDescending(entry=>entry.Line.Forms.Sum(form=>form.AbilityText.Length))
                    .ThenBy(entry=>entry.Variant.SetId,StringComparer.Ordinal).ThenBy(entry=>entry.Line.Id,StringComparer.Ordinal).First();
                longUnit=longest.Line;longSet=longest.Variant.SetId;
                Press(ImGuiKey.C);break;
            case 17:advance=Click(screen,"codexTab/digimon");break;
            case 18:advance=VisibleSetClick(screen,longSet!);break;
            case 19:advance=Click(screen,"codexSearch");break;
            case 20:ImGui.GetIO().AddInputCharactersUTF8(System.Text.Encoding.UTF8.GetBytes(longUnit!.Id));cooldown=4;break;
            case 21:advance=VisibleClick(screen,"codexLine/"+longUnit!.Id);break;
            case 22:
                longScrollStart=screen.CodexDetailScrollY;longScrollRange=screen.CodexDetailScrollMaxY;
                ScrollLongDetail(screen);break;
            case 23:
                if(FullFooterVisible(screen))
                {
                    VerifyLongDetail(screen,true);longFooterReached=true;
                    Console.WriteLine($"Codex longest detail reached its full footer: {longSet}/{longUnit!.Id}, {longUnit.Forms.Sum(form=>form.AbilityText.Length)} ability characters, scroll {screen.CodexDetailScrollY:0.##}/{longScrollRange:0.##}.");
                }
                else {ScrollLongDetail(screen);advance=false;}
                break;
            case 24:advance=Click(screen,"codexCloseUnit");break;
            case 25:advance=VisibleClick(screen,"codexLine/"+longUnit!.Id);break;
            case 26:Press(ImGuiKey.Escape);break;
            case 27:advance=Click(screen,"codexSearch");break;
            case 28:Press(ImGuiKey.Escape);break;
            case 29:advance=VisibleSetClick(screen,"faction-wars");break;
            case 30:advance=Click(screen,"codexTab/rules");break;
            case 31:Press(ImGuiKey.Escape);break;
            default:
                if(screen.CodexVisible)throw new InvalidOperationException("Codex did not close to Home.");
                if(!longFooterReached||!rulesComplete)throw new InvalidOperationException("Codex longest-description footer or full Rules scroll was not tested.");
                HomeDone=true;Console.WriteLine("Codex home UI passed: all six categories, kind filtering, Digimon details, recipe links, full Rules scroll with source-derived round chips/table cells, search/clear, set browsing, longest-description footer and horizontal bounds, detail close/Escape and C/Escape.");return;
        }
        if(advance)homeStep++;
    }
    public void LiveInput(MatchScreen screen)
    {
        if(LiveDone||!Ready())return;
        liveVariant??=screen.Session!.State.Rules.Id;
        bool advance=true;
        switch(liveStep)
        {
            case 0:Press(ImGuiKey.C);break;
            case 1:advance=Click(screen,"codexPool");break;
            case 2:advance=VisibleClick(screen,"codexLine/",true);break;
            case 3:advance=Click(screen,"codexBack");break;
            case 4:advance=Click(screen,"codexSideRules");break;
            case 5:advance=Click(screen,"codexExpand");break;
            case 6:advance=Click(screen,"codexSet/fates");break;
            case 7:Press(ImGuiKey.Escape);break;
            case 8:Press(ImGuiKey.C);break;
            default:
                if(screen.CodexVisible||screen.Session!.State.Rules.Id!=liveVariant)throw new InvalidOperationException("Codex changed the running set or failed to close.");
                LiveDone=true;Console.WriteLine("Codex in-match UI passed: local board counts, pool traits, ownership/detail navigation, compact rules, full expansion and locked live set.");return;
        }
        if(advance)liveStep++;
    }
    public void Verify(MatchScreen screen,int2 size,string? capture,bool live)
    {
        if(!live&&!HomeDone&&homeStep==10&&screen.CodexFullVisible&&screen.CodexTab=="rules")
        {
            // Visibility is accumulated every rendered frame, including frames
            // between wheel events, rather than trusting offscreen draw counts.
            ObserveRules(screen);
            if(capture is not null&&!rulesBottomCaptured&&screen.CodexBodyScrollMaxY>1&&screen.CodexBodyScrollY>=screen.CodexBodyScrollMaxY-.5f)
            {
                FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"codex-rules-bottom.bmp"),size.X,size.Y);rulesBottomCaptured=true;
            }
        }
        if(cooldown!=1)return;
        if(!live&&!HomeDone)
        {
            if((homeStep is >=1 and <=15 or >=17 and <=31)&&!screen.CodexFullVisible)throw new InvalidOperationException("Codex closed before its full browsing sequence finished.");
            if(homeStep==2&&screen.CodexRows!=catalog.Variant(screen.CodexVariantId).Traits.Count(trait=>trait.Kind=="family"))throw new InvalidOperationException("Codex family filter did not match source data.");
            if(homeStep==4&&screen.CodexUnit is null)throw new InvalidOperationException("Codex Digimon card did not open details.");
            if(homeStep==6&&screen.CodexRecipeCells==0)throw new InvalidOperationException("Codex item recipes were omitted.");
            if(homeStep==7&&(recipeItem is null||!screen.Buttons.TryGetValue("codexItem/"+recipeItem,out var itemPoint)||itemPoint.Y<170||itemPoint.Y>size.Y-35))throw new InvalidOperationException("Codex recipe click did not scroll its completed item into view.");
            if(homeStep==12&&screen.CodexRows!=0)throw new InvalidOperationException("Codex search did not filter all nonmatching rules.");
            if(homeStep==13&&screen.CodexRows==0)throw new InvalidOperationException("First Escape did not clear the focused Codex search: "+screen.CodexQuery);
            if(homeStep==14&&(!screen.CodexVariantId.StartsWith("fates/",StringComparison.Ordinal)||screen.Buttons.ContainsKey("codexTab/wisps")))throw new InvalidOperationException("Codex set browsing retained an unavailable Wisps tab.");
            if(homeStep==19&&catalog.Variant(screen.CodexVariantId).SetId!=longSet)throw new InvalidOperationException("Codex did not select the longest-description unit's set.");
            if(homeStep is >=22 and <=24 or 26)VerifyLongDetail(screen,homeStep==24);
            if((homeStep is 22 or 26)&&screen.CodexDetailScrollY>1)throw new InvalidOperationException("Codex detail did not open at the top.");
            if((homeStep is 25 or 27)&&screen.CodexUnit is not null)throw new InvalidOperationException("Codex long detail did not close with its close button or Escape.");
            if(homeStep==29&&screen.CodexQuery.Length!=0)throw new InvalidOperationException("Codex long-detail search was not cleared.");
        }
        if(live&&!LiveDone&&liveStep>0)
        {
            if(screen.Session!.State.Rules.Id!=liveVariant||screen.CodexVariantId!=liveVariant)throw new InvalidOperationException("Codex browse changed a running match's set.");
            if(liveStep==1)
            {
                var session=screen.Session;var human=session.State.Players[0];var expected=TraitRules.Count(session.State.Rules,session.CombatProjection.Board(human),human.Augments.ToArray());
                if(expected.Count!=screen.CodexBoardCounts.Count||expected.Any(pair=>screen.CodexBoardCounts.GetValueOrDefault(pair.Key)!=pair.Value))throw new InvalidOperationException("Codex local board counts did not match deployed units.");
            }
            if(liveStep==3&&screen.CodexUnit is null)throw new InvalidOperationException($"Side Codex did not open {selectedTarget} at {selectedPoint}; body clip {screen.CodexBodyMin} to {screen.CodexBodyMax}.");
            if(liveStep==7&&screen.CodexVariantId!=liveVariant)throw new InvalidOperationException("Codex allowed set browsing during a running game.");
        }
        int step=live?liveStep:homeStep;
        if(capture is not null&&step>0&&(live||step!=10||!rulesTopCaptured))
        {
            FrameCapture.SaveBitmap(Path.ChangeExtension(capture,$"codex-{(live?"live":"home")}-{step}.bmp"),size.X,size.Y);
            if(!live&&step==10)rulesTopCaptured=true;
        }
    }
}

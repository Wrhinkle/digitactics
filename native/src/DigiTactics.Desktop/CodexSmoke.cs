using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;

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
    public bool HomeDone {get;private set;}
    public bool LiveDone {get;private set;}
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
        var size=ImGui.GetIO().DisplaySize;
        var pair=screen.Buttons.FirstOrDefault(pair=>pair.Key.StartsWith(prefix,StringComparison.Ordinal)&&pair.Value.X>20&&pair.Value.X<(side?220:size.X-20)&&pair.Value.Y>220&&pair.Value.Y<size.Y-(side?280:55));
        if(pair.Key is not null)
        {
            if(prefix=="codexRecipe/"){var indices=pair.Key.Split('/');recipeItem=catalog.Variant(screen.CodexVariantId).RecipeGrid[int.Parse(indices[1])][int.Parse(indices[2])];}
            return Click(screen,pair.Key);
        }
        ImGui.GetIO().AddMousePosEvent(side?100:size.X*.5f,side?350:size.Y*.6f);ImGui.GetIO().AddMouseWheelEvent(0,-4);cooldown=3;return false;
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
            case 10:advance=Click(screen,"codexSearch");break;
            case 11:ImGui.GetIO().AddInputCharactersUTF8("no-such-digimon-xyz"u8);cooldown=4;break;
            case 12:Press(ImGuiKey.Escape);break;
            case 13:advance=Click(screen,"codexSet/fates");break;
            case 14:advance=Click(screen,"codexSet/faction-wars");break;
            case 15:Press(ImGuiKey.Escape);break;
            default:
                if(screen.CodexVisible)throw new InvalidOperationException("Codex did not close to Home.");
                HomeDone=true;Console.WriteLine("Codex home UI passed: all six categories, kind filtering, Digimon details, recipe links, search/clear, set browsing and C/Escape.");return;
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
        if(cooldown!=1)return;
        if(!live&&!HomeDone)
        {
            if(homeStep is >=1 and <=15&&!screen.CodexFullVisible)throw new InvalidOperationException("Codex closed before its full browsing sequence finished.");
            if(homeStep==2&&screen.CodexRows!=catalog.Variant(screen.CodexVariantId).Traits.Count(trait=>trait.Kind=="family"))throw new InvalidOperationException("Codex family filter did not match source data.");
            if(homeStep==4&&screen.CodexUnit is null)throw new InvalidOperationException("Codex Digimon card did not open details.");
            if(homeStep==6&&screen.CodexRecipeCells==0)throw new InvalidOperationException("Codex item recipes were omitted.");
            if(homeStep==7&&(recipeItem is null||!screen.Buttons.TryGetValue("codexItem/"+recipeItem,out var itemPoint)||itemPoint.Y<170||itemPoint.Y>size.Y-35))throw new InvalidOperationException("Codex recipe click did not scroll its completed item into view.");
            if(homeStep==12&&screen.CodexRows!=0)throw new InvalidOperationException("Codex search did not filter all nonmatching rules.");
            if(homeStep==13&&screen.CodexRows==0)throw new InvalidOperationException("First Escape did not clear the focused Codex search: "+screen.CodexQuery);
            if(homeStep==14&&(!screen.CodexVariantId.StartsWith("fates/",StringComparison.Ordinal)||screen.Buttons.ContainsKey("codexTab/wisps")))throw new InvalidOperationException("Codex set browsing retained an unavailable Wisps tab.");
        }
        if(live&&!LiveDone&&liveStep>0)
        {
            if(screen.Session!.State.Rules.Id!=liveVariant||screen.CodexVariantId!=liveVariant)throw new InvalidOperationException("Codex browse changed a running match's set.");
            if(liveStep==1)
            {
                var session=screen.Session;var human=session.State.Players[0];var expected=TraitRules.Count(session.State.Rules,session.CombatProjection.Board(human),human.Augments.ToArray());
                if(expected.Count!=screen.CodexBoardCounts.Count||expected.Any(pair=>screen.CodexBoardCounts.GetValueOrDefault(pair.Key)!=pair.Value))throw new InvalidOperationException("Codex local board counts did not match deployed units.");
            }
            if(liveStep==3&&screen.CodexUnit is null)throw new InvalidOperationException("Side Codex did not open the selected Digimon.");
            if(liveStep==7&&screen.CodexVariantId!=liveVariant)throw new InvalidOperationException("Codex allowed set browsing during a running game.");
        }
        int step=live?liveStep:homeStep;
        if(capture is not null&&step>0)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,$"codex-{(live?"live":"home")}-{step}.bmp"),size.X,size.Y);
    }
}

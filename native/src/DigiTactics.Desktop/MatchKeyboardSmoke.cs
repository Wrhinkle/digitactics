using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Real mouse/keyboard routing, including focused widgets and nested overlays, on an isolated save.</summary>
internal sealed class MatchKeyboardSmoke : IDisposable
{
    private sealed record Step(string Name,Func<MatchScreen,bool> Input,Action<MatchScreen>? Verify=null);
    private readonly List<Step> steps=[];
    private readonly CustomMusicController music;
    private int index,verified=-1,cooldown=4,gold,units,freeRolls,sold,saleValue,scouted;
    private bool down,mouse,modified,locked;
    private ImGuiKey key;
    internal string Pending=>steps.ElementAtOrDefault(index)?.Name??"done";
    public MatchKeyboardSmoke(MatchScreen screen,NativeProfileStore profile,GameAudio audio)
    {
        if(profile.Exists)throw new InvalidOperationException("Keyboard smoke requires a new empty isolated profile directory.");
        if(!profile.UpdateStorage(new Dictionary<string,string>{{"dt.name","Keyboard Tamer"},{"dt.cosmetics.v1","{\"bits\":10000,\"owned\":{},\"equipped\":{}}"}}))throw new InvalidOperationException(profile.Error);
        music=new(audio,profile,Path.Combine(AppContext.BaseDirectory,"unused-keyboard-decoder.exe"));
        AddClick("homeOverview");AddKey("Enter keyboard navigation",ImGuiKey.Tab,s=>Require(s.KeyboardFocusedButton=="homeOverview","Tab did not enter native navigation."));
        AddKey("Tab navigation",ImGuiKey.Tab,s=>Require(s.KeyboardFocusedButton=="collection","Tab did not focus Collection; focused="+s.KeyboardFocusedButton));AddKey("Enter activates Collection",ImGuiKey.Enter,s=>Require(s.Buttons.ContainsKey("closeCollection"),"Tab/Enter did not open the focused Collection navigation button; focused="+s.KeyboardFocusedButton));
        AddKey("Collection Escape",ImGuiKey.Escape,s=>Require(!s.Buttons.ContainsKey("closeCollection"),"Collection Escape did not return Home."));
        AddClick("collection");AddClick("collectionCategory");
        AddKey("Combo owns Escape",ImGuiKey.Escape,s=>Require(s.Buttons.ContainsKey("closeCollection")&&!s.Buttons.ContainsKey("collectionCategory/1"),"Combo Escape also closed its Collection panel."));
        AddClick("buyCosmetic/legend/bukamon");
        AddKey("Cancel purchase",ImGuiKey.Escape,s=>Require(s.Buttons.ContainsKey("closeCollection")&&!s.Buttons.ContainsKey("confirmPurchase"),"Escape did not cancel only the purchase layer."));
        AddClick("evolution/tokomon");AddKey("Evolution Escape",ImGuiKey.Escape,s=>Require(s.Buttons.ContainsKey("closeCollection")&&!s.Buttons.ContainsKey("closeEvolution"),"Evolution Escape did not return to Collection."));
        AddKey("Back Home",ImGuiKey.Escape);AddClick("farm");AddClick("farmRoom");
        AddKey("Farm room Escape",ImGuiKey.Escape,s=>Require(s.Buttons.ContainsKey("farmLights"),"Escape did not return farm management to care."));
        AddClick("farmDigivolve");AddKey("Farm Digivolve Escape",ImGuiKey.Escape,s=>Require(s.Buttons.ContainsKey("closeFarm")&&!s.Buttons.ContainsKey("closeCollection"),"Farm Digivolve Escape lost its Farm origin."));
        AddKey("Farm Escape",ImGuiKey.Escape,s=>Require(!s.Buttons.ContainsKey("closeFarm"),"Farm Escape did not return Home."));
        AddClick("musicLibrary");AddKey("Music Escape",ImGuiKey.Escape,s=>Require(!s.Buttons.ContainsKey("closeMusicLibrary"),"Music library Escape did not close."));
        AddClick("start");AddClick("planningTimer");
        steps.Add(new("carousel",s=>
        {
            var carousel=s.Session!.Units.Carousel;
            if(!carousel.Active)return true;
            if(carousel.Turn?.Id!=0)return false;
            int available=Enumerable.Range(0,carousel.Options.Count).First(i=>carousel.Options[i].PickedBy is null);
            return Click(s,"carousel/"+available);
        }));
        AddClick("fight");AddClick("next");
        steps.Add(new("planning fixture",s=>
        {
            Require(s.Session?.Rounds.Phase==MatchPhase.Planning,"Keyboard fixture must reach a planning round.");
            Require(!s.PlanningTimerEnabled,"Keyboard fixture must disable its planning timer through the button.");
            return true;
        }));
        for(int digit=1;digit<=8;digit++)
        {
            int selected=digit;
            AddKey("Scout "+digit,(ImGuiKey)((int)ImGuiKey._1+digit-1),s=>Require(s.ViewedPlayer==Order(s)[selected-1],"Number shortcut did not follow the displayed standings order."));
        }
        AddKey("Keypad scouting",ImGuiKey.Keypad2,s=>Require(s.ViewedPlayer==Order(s)[1],"Numeric keypad did not follow scouting bindings."));
        steps.Add(new("cycle predecessor",s=>{scouted=Cycle(s,-1);return Press(ImGuiKey.Q);},s=>Require(s.ViewedPlayer==scouted,"Q did not scout the previous living player.")));
        steps.Add(new("cycle successor",s=>{scouted=Cycle(s,1);return Press(ImGuiKey.E);},s=>Require(s.ViewedPlayer==scouted,"E without a hovered unit did not scout the next player.")));
        AddKey("Space returns home",ImGuiKey.Space,s=>Require(s.ViewedPlayer==0&&s.Session!.Rounds.Phase==MatchPhase.Planning,"Space scouting return also started a battle."));
        AddClick("lock");steps.Add(new("remember focused lock",s=>{locked=s.Session!.State.Players[0].Shop.Locked;return true;}));
        AddKey("Scout while lock focused",ImGuiKey._8);
        AddKey("Space owns focused widget",ImGuiKey.Space,s=>Require(s.ViewedPlayer==0&&s.Session!.State.Players[0].Shop.Locked==locked&&s.Session.Rounds.Phase==MatchPhase.Planning,"Space activated a focused widget while returning from scouting."));
        AddKey("Scout for Escape",ImGuiKey._8);AddKey("Escape returns home",ImGuiKey.Escape,s=>Require(s.ViewedPlayer==0&&!s.Buttons.ContainsKey("resume"),"Scouting Escape opened the game menu."));
        AddKey("Escape opens menu",ImGuiKey.Escape,s=>Require(s.Buttons.ContainsKey("resume"),"Own-board Escape did not open the game menu."));
        AddKey("Escape closes menu",ImGuiKey.Escape,s=>Require(!s.Buttons.ContainsKey("resume"),"Game menu Escape did not close."));
        steps.Add(new("remember economy",s=>{Snapshot(s);return true;}));
        AddKey("Ctrl+D is not refresh",ImGuiKey.D,s=>Unchanged(s),control:true);AddKey("Ctrl+F is not XP",ImGuiKey.F,s=>Unchanged(s),control:true);
        AddKey("Scout board guard",ImGuiKey._8);
        steps.Add(new("guarded buy",s=>Click(s,BuyKey(s)),s=>Unchanged(s)));
        AddKey("return own board",ImGuiKey.Space);
        steps.Add(new("own buy",s=>Click(s,BuyKey(s)),s=>Require(s.Session!.State.Players[0].Economy.Gold<gold,"Own-board purchase stopped working.")));
        steps.Add(new("sell hovered unit",s=>
        {
            var player=s.Session!.State.Players[0];var entry=player.Roster.Entries.First(e=>!e.Unit.NoSell&&!s.Session.State.Rules.Lines.Concat(s.Session.State.Rules.ExtraLines).Single(l=>l.Id==e.Unit.Line).NoSell);
            sold=entry.Unit.Id;gold=player.Economy.Gold;saleValue=UnitValues.SellValue(s.Session.State.Rules.Lines.Concat(s.Session.State.Rules.ExtraLines).Single(l=>l.Id==entry.Unit.Line),entry.Unit.Stage);
            string target=entry.Spot.IsBoard?$"hex/{entry.Spot.Column}/{entry.Spot.Row}":$"bench/{entry.Spot.Index}";
            if(!s.Buttons.TryGetValue(target,out var point))return false;
            ImGui.GetIO().AddMousePosEvent(point.X,point.Y);return Press(ImGuiKey.E,movePointer:false);
        },s=>Require(s.ViewedPlayer==0&&!s.Session!.State.Players[0].Roster.Entries.Any(e=>e.Unit.Id==sold)&&s.Session.State.Players[0].Economy.Gold==gold+saleValue,"Contextual E did not sell exactly the hovered own unit.")));
        AddKey("Zoom in",ImGuiKey.Equal,s=>Require(Math.Abs(s.CameraState.Zoom-.8)<1e-8,"Equal did not zoom the camera in."));
        AddKey("Zoom out",ImGuiKey.Minus,s=>Require(Math.Abs(s.CameraState.Zoom-1)<1e-8,"Minus did not zoom the camera out."));
        AddKey("Zoom before reset",ImGuiKey.Equal);AddKey("Reset camera",ImGuiKey.R,s=>Require(s.CameraState.Zoom==1,"R did not reset the camera."));
        AddKey("Open soundboard",ImGuiKey.G,s=>Require(s.Buttons.ContainsKey("closeSoundboard"),"G did not open soundboard."));
        AddKey("Soundboard consumes slot",ImGuiKey._6,s=>Require(s.ViewedPlayer==0,"Soundboard slot leaked to scouting."));
        AddKey("Soundboard Escape",ImGuiKey.Escape,s=>Require(!s.Buttons.ContainsKey("closeSoundboard")&&!s.Buttons.ContainsKey("resume"),"Soundboard Escape reached the game menu."));
        AddKey("Open emotes",ImGuiKey.T,s=>Require(s.Buttons.ContainsKey("closeEmotes"),"T did not open emotes."));
        AddKey("Emote consumes slot",ImGuiKey._1,s=>Require(s.ViewedPlayer==0,"Emote slot leaked to scouting."));
        steps.Add(new("close remaining emotes",s=>s.Buttons.ContainsKey("closeEmotes")?Press(ImGuiKey.Escape):true));
        AddClick("audioSettings");AddKey("Audio blocks scouting",ImGuiKey._8,s=>Require(s.ViewedPlayer==0,"Audio settings did not own keyboard."));
        AddKey("Audio Escape",ImGuiKey.Escape,s=>Require(!s.Buttons.ContainsKey("closeAudio")&&!s.Buttons.ContainsKey("resume"),"Audio Escape reached the game menu."));
        AddKey("Open Codex",ImGuiKey.C,s=>Require(s.CodexVisible,"C did not open Codex."));
        steps.Add(new("expand Codex",s=>Click(s,"codexExpand"),s=>Require(s.CodexFullVisible,"Full button did not expand Codex; visible="+s.CodexVisible)));
        AddClick("codexSearch");
        steps.Add(new("focused search input",s=>{Snapshot(s);ImGui.GetIO().AddInputCharactersUTF8("keyboard"u8);return true;}));
        foreach(var blocked in new[]{ImGuiKey.D,ImGuiKey.F,ImGuiKey.G,ImGuiKey.T,ImGuiKey._8,ImGuiKey.Space,ImGuiKey.R})AddKey("Text owns "+blocked,blocked,s=>{Unchanged(s);Require(s.ViewedPlayer==0&&s.Session!.Rounds.Phase==MatchPhase.Planning&&s.CodexVisible,"Text entry leaked a gameplay shortcut.");});
        AddKey("Clear search",ImGuiKey.Escape,s=>Require(s.CodexFullVisible&&s.CodexQuery.Length==0,"First Escape did not clear the active search."));
        AddKey("Close full Codex",ImGuiKey.Escape,s=>Require(!s.CodexFullVisible&&s.CodexVisible&&!s.Buttons.ContainsKey("resume"),"Full Codex Escape did not return to its side panel."));
        AddKey("Close side Codex",ImGuiKey.Escape,s=>Require(!s.CodexVisible&&!s.Buttons.ContainsKey("resume"),"Codex Escape leaked to the game menu."));
        AddClick("lock");steps.Add(new("focused ready snapshot",s=>{locked=s.Session!.State.Players[0].Shop.Locked;return true;}));
        AddKey("Own Space starts combat once",ImGuiKey.Space,s=>Require(s.Session!.Rounds.Phase!=MatchPhase.Planning&&s.Session.State.Players[0].Shop.Locked==locked,"Own-board Space failed ready or activated the focused lock button."));
    }
    private static int[] Order(MatchScreen s)=>s.Session!.State.Players.OrderByDescending(p=>p.Alive).ThenBy(p=>p.Alive?-p.Health:p.Placement).ThenBy(p=>s.Session.State.Duos?p.Team??0:0).ThenBy(p=>p.Id).Select(p=>p.Id).ToArray();
    private static int Cycle(MatchScreen s,int direction)
    {
        int[] order=Order(s).Where(id=>id==0||s.Session!.State.Players[id].Alive).ToArray();return order[(Array.IndexOf(order,s.ViewedPlayer)+direction+order.Length)%order.Length];
    }
    private static string BuyKey(MatchScreen s)
    {
        var p=s.Session!.State.Players[0];int i=Enumerable.Range(0,p.Shop.Slots.Count).FirstOrDefault(i=>p.Shop.Slots[i] is not null&&p.Shop.Cost(i)<=p.Economy.Gold,-1);
        if(i<0)throw new InvalidOperationException("Keyboard buy fixture has no affordable shop unit.");return "buy/"+i;
    }
    private void Snapshot(MatchScreen s){var p=s.Session!.State.Players[0];gold=p.Economy.Gold;units=p.Roster.Entries.Count();freeRolls=p.Shop.FreeRolls;}
    private void Unchanged(MatchScreen s){var p=s.Session!.State.Players[0];Require(p.Economy.Gold==gold&&p.Roster.Entries.Count()==units&&p.Shop.FreeRolls==freeRolls,"Blocked shortcut changed economy or roster.");}
    private static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
    private void AddClick(string button)=>steps.Add(new(button,s=>Click(s,button)));
    private void AddKey(string name,ImGuiKey value,Action<MatchScreen>? verify=null,bool control=false)=>steps.Add(new(name,_=>Press(value,control),verify));
    private bool Click(MatchScreen screen,string button)
    {
        if(!screen.Buttons.TryGetValue(button,out var target))return false;
        if(target.Y<12||target.Y>=ImGui.GetIO().DisplaySize.Y-20){ImGui.GetIO().AddMousePosEvent(target.X,ImGui.GetIO().DisplaySize.Y*.65f);ImGui.GetIO().AddMouseWheelEvent(0,target.Y<12?4:-4);cooldown=3;return false;}
        ImGui.GetIO().AddMousePosEvent(target.X,target.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);down=mouse=true;return true;
    }
    private bool Press(ImGuiKey value,bool control=false,bool movePointer=true)
    {
        if(movePointer)ImGui.GetIO().AddMousePosEvent(2,2);
        if(control)ImGui.GetIO().AddKeyEvent(ImGuiKey.Mod_Ctrl,true);
        ImGui.GetIO().AddKeyEvent(value,true);key=value;down=true;modified=control;return true;
    }
    public void Input(MatchScreen screen)
    {
        if(cooldown-->0)return;
        if(down)
        {
            if(mouse)ImGui.GetIO().AddMouseButtonEvent(0,false);else ImGui.GetIO().AddKeyEvent(key,false);
            if(modified)ImGui.GetIO().AddKeyEvent(ImGuiKey.Mod_Ctrl,false);
            down=mouse=modified=false;cooldown=3;return;
        }
        if(index>=steps.Count)return;
        if(index>0&&verified<index-1){steps[index-1].Verify?.Invoke(screen);verified=index-1;}
        if(steps[index].Input(screen)){index++;cooldown=3;}
    }
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(index<steps.Count||down||cooldown>0)return false;
        steps[^1].Verify?.Invoke(screen);
        if(capture is not null)FrameCapture.SaveBitmap(capture,size.X,size.Y);
        Console.WriteLine($"Match keyboard UI: {steps.Count} mouse/key steps passed: Tab/Enter navigation, nested Escape, scouting/cycle/return, focused-widget Space ownership, contextual sell, camera, social slots, modifiers, text and own-board guards.");return true;
    }
    public void Dispose()=>music.Dispose();
}

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
    private bool soundboardCaptured,emotesCaptured;
    private PlayerOffer? protectedOffer;
    private ImGuiKey key;
    internal string Pending=>steps.ElementAtOrDefault(index)?.Name??"done";
    public MatchKeyboardSmoke(MatchScreen screen,NativeProfileStore profile,GameAudio audio)
    {
        if(profile.Exists)throw new InvalidOperationException("Keyboard smoke requires a new empty isolated profile directory.");
        if(!profile.UpdateStorage(new Dictionary<string,string>{{"dt.name","Keyboard Tamer"},{"dt.cosmetics.v1","{\"bits\":10000,\"owned\":{},\"equipped\":{}}"}}))throw new InvalidOperationException(profile.Error);
        music=new(audio,profile,Path.Combine(AppContext.BaseDirectory,"unused-keyboard-decoder.exe"));
        AddClick("homeOverview");AddKey("Enter keyboard navigation",ImGuiKey.Tab,s=>Require(s.KeyboardFocusedButton=="homeOverview","Tab did not enter native navigation."));
        AddKey("Tab reaches Modes",ImGuiKey.Tab,s=>Require(s.KeyboardFocusedButton=="clientModes","Tab did not follow the visible Modes tab; focused="+s.KeyboardFocusedButton));
        AddKey("Tab navigation",ImGuiKey.Tab,s=>Require(s.KeyboardFocusedButton=="collection","Tab did not focus Collection; focused="+s.KeyboardFocusedButton));AddKey("Enter activates Collection",ImGuiKey.Enter,s=>Require(s.Buttons.ContainsKey("closeCollection"),"Tab/Enter did not open the focused Collection navigation button; focused="+s.KeyboardFocusedButton));
        AddKey("Collection Escape",ImGuiKey.Escape,s=>Require(!s.Buttons.ContainsKey("closeCollection"),"Collection Escape did not return Home."));
        AddClick("collection");AddClick("collectionCategory");
        AddKey("Combo owns Escape",ImGuiKey.Escape,s=>Require(s.Buttons.ContainsKey("closeCollection")&&!s.Buttons.ContainsKey("collectionCategory/1"),"Combo Escape also closed its Collection panel."));
        AddClick("preview/legend/bukamon");AddClick("buyCosmetic/legend/bukamon");
        AddKey("Cancel purchase",ImGuiKey.Escape,s=>Require(s.Buttons.ContainsKey("closeCollection")&&!s.Buttons.ContainsKey("confirmPurchase"),"Escape did not cancel only the purchase layer."));
        AddClick("preview/legend/tokomon");AddClick("evolution/tokomon");AddKey("Evolution Escape",ImGuiKey.Escape,s=>Require(s.Buttons.ContainsKey("closeCollection")&&!s.Buttons.ContainsKey("closeEvolution"),"Evolution Escape did not return to Collection."));
        AddKey("Back Home",ImGuiKey.Escape);AddClick("farm");AddClick("farmRoom");
        AddKey("Farm room Escape",ImGuiKey.Escape,s=>Require(s.Buttons.ContainsKey("farmLights"),"Escape did not return farm management to care."));
        AddClick("farmDigivolve");AddKey("Farm Digivolve Escape",ImGuiKey.Escape,s=>Require(s.Buttons.ContainsKey("closeFarm")&&!s.Buttons.ContainsKey("closeCollection"),"Farm Digivolve Escape lost its Farm origin."));
        AddClick("farmDigivolve");
        steps.Add(new("Farm evolution to Home",s=>Click(s,"homeOverview"),s=>Require(!s.CollectionVisible&&!s.Buttons.ContainsKey("closeFarm")&&s.Buttons.ContainsKey("start"),"Home navigation restored the Farm origin instead of the requested Home page.")));
        AddClick("farm");AddClick("farmDigivolve");
        steps.Add(new("Farm evolution to Collection",s=>Click(s,"collection"),s=>Require(s.CollectionVisible&&!s.Buttons.ContainsKey("closeFarm")&&!s.Buttons.ContainsKey("closeEvolution"),"Collection navigation retained a hidden Farm-origin evolution.")));
        AddClick("farm");AddClick("farmDigivolve");AddClick("levelLegend");AddClick("levelLegend");AddClick("digivolve");
        steps.Add(new("Digivolution begins below shell",s=>{Require(s.EvolutionBusy,"Digivolution fixture did not begin its transformation.");Require(s.Buttons["closeEvolution"].Y>=96&&s.Buttons["closeCollection"].Y>=96,"Evolution back controls are hidden beneath client navigation.");return true;}));
        foreach(string blocked in new[]{"homeOverview","collection","closeEvolution","closeCollection"})
            steps.Add(new("Busy evolution blocks "+blocked,s=>Click(s,blocked),s=>Require(s.EvolutionBusy&&s.Buttons.ContainsKey("closeEvolution"),"The active transformation was bypassed by "+blocked+".")));
        AddKey("Busy evolution owns Escape",ImGuiKey.Escape,s=>Require(s.EvolutionBusy&&s.Buttons.ContainsKey("closeEvolution"),"Escape bypassed an active digivolution."));
        steps.Add(new("Wait for digivolution",s=>!s.EvolutionBusy));
        AddClick("closeEvolution");
        AddKey("Farm Escape",ImGuiKey.Escape,s=>Require(!s.Buttons.ContainsKey("closeFarm"),"Farm Escape did not return Home."));
        AddClick("settings");AddClick("musicLibrary");AddKey("Music Escape",ImGuiKey.Escape,s=>Require(!s.Buttons.ContainsKey("closeMusicLibrary"),"Music library Escape did not close."));AddClick("closeSettings");
        AddClick("start");
        steps.Add(new("carousel",s=>
        {
            var carousel=s.Session!.Units.Carousel;
            if(!carousel.Active)return true;
            if(carousel.Turn?.Id!=0)return false;
            int available=Enumerable.Range(0,carousel.Options.Count).First(i=>carousel.Options[i].PickedBy is null);
            return Click(s,"carousel/"+available);
        }));
        steps.Add(new("Leave completed carousel",s=>!s.Session!.Units.Carousel.Active||Click(s,"carouselGo")));
        AddClick("planningTimer");
        steps.Add(new("planning fixture",s=>
        {
            Require(s.Session?.Rounds.Phase==MatchPhase.Planning,"Keyboard fixture must reach a planning round.");
            Require(!s.PlanningTimerEnabled,"Keyboard fixture must disable its planning timer through the button.");
            return true;
        }));
        steps.Add(new("Scheduled reward fixture",s=>
        {
            var session=s.Session!;var player=session.State.Players[0];
            for(int stage=1;stage<=4&&player.Offer is null;stage++)for(int round=1;round<=7&&player.Offer is null;round++)session.Offers.Make(player,new(stage,round));
            protectedOffer=player.Offer;Require(protectedOffer is not null,"The real offer scheduler did not create a reward for the keyboard fixture.");return true;
        }));
        AddClick("peekChoices");AddKey("Pending reward owns Space",ImGuiKey.Space,s=>Require(s.Session!.Rounds.Phase==MatchPhase.Planning&&ReferenceEquals(s.Session.State.Players[0].Offer,protectedOffer)&&!s.PeekingChoices,"Space auto-picked a pending reward or failed to reveal its choices."));
        AddClick("peekChoices");steps.Add(new("Pending reward owns Fight",s=>Click(s,"fight"),s=>Require(s.Session!.Rounds.Phase==MatchPhase.Planning&&ReferenceEquals(s.Session.State.Players[0].Offer,protectedOffer)&&!s.PeekingChoices,"Fight auto-picked a pending reward or failed to reveal its choices.")));
        AddClick("offer/0");
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
        AddKey("Open soundboard",ImGuiKey.G,s=>{Require(s.Buttons.ContainsKey("closeSoundboard"),"G did not open soundboard.");CheckSocialLayout(s,false);});
        steps.Add(new("Soundboard mouse pad",s=>Click(s,"soundboard/0"),s=>Require(s.SoundboardLastPlayedSlot==0,"The first sound pad did not play through its mouse target.")));
        steps.Add(new("Soundboard rate limit",s=>Click(s,"soundboard/0"),s=>Require(s.SoundboardCooldown>0,"Two pad presses did not begin the existing soundboard cooldown.")));
        AddKey("Soundboard consumes slot",ImGuiKey._6,s=>Require(s.ViewedPlayer==0,"Soundboard slot leaked to scouting."));
        steps.Add(new("Remember lock outside pads",s=>{locked=s.Session!.State.Players[0].Shop.Locked;return true;}));
        steps.Add(new("Outside pad press passes through",s=>Click(s,"lock"),s=>Require(!s.Buttons.ContainsKey("closeSoundboard")&&s.Session!.State.Players[0].Shop.Locked!=locked,$"Outside soundboard press did not dismiss the deck and reach its target: open={s.Buttons.ContainsKey("closeSoundboard")}, locked={s.Session!.State.Players[0].Shop.Locked}, before={locked}, pointer={ImGui.GetIO().MousePos}, lock={s.Buttons["lock"]}.")));
        AddKey("Reopen soundboard",ImGuiKey.G);
        AddKey("Soundboard Escape",ImGuiKey.Escape,s=>Require(!s.Buttons.ContainsKey("closeSoundboard")&&!s.Buttons.ContainsKey("resume"),"Soundboard Escape reached the game menu."));
        steps.Add(new("Soundboard header opens",s=>Click(s,"soundboard"),s=>Require(s.Buttons.ContainsKey("closeSoundboard"),"Soundboard header did not open its deck.")));
        steps.Add(new("Soundboard header closes",s=>Click(s,"soundboard"),s=>Require(!s.Buttons.ContainsKey("closeSoundboard"),"Outside-press handling broke the soundboard header toggle.")));
        AddKey("Open emotes",ImGuiKey.T,s=>{Require(s.Buttons.ContainsKey("closeEmotes"),"T did not open emotes.");CheckSocialLayout(s,true);});
        steps.Add(new("Remember lock outside wheel",s=>{locked=s.Session!.State.Players[0].Shop.Locked;return true;}));
        steps.Add(new("Outside wheel press passes through",s=>Click(s,"lock"),s=>Require(!s.Buttons.ContainsKey("closeEmotes")&&s.Session!.State.Players[0].Shop.Locked!=locked,"Outside emote press did not dismiss the wheel and reach its target.")));
        AddKey("Reopen wheel after outside press",ImGuiKey.T);
        steps.Add(new("Wheel mute others",s=>Click(s,"muteEmotes"),s=>Require(s.OtherEmotesMuted,"The wheel mute button did not persist its preference.")));
        steps.Add(new("Wheel unmute others",s=>Click(s,"muteEmotes"),s=>Require(!s.OtherEmotesMuted,"The wheel mute button could not restore the preference.")));
        steps.Add(new("Wheel mouse emote",s=>Click(s,"emote/0"),s=>Require(!s.Buttons.ContainsKey("closeEmotes"),"Clicking the equipped wheel image did not send and close the wheel.")));
        AddKey("Reopen emotes",ImGuiKey.T);
        AddKey("Emote consumes slot",ImGuiKey._1,s=>Require(s.ViewedPlayer==0,"Emote slot leaked to scouting."));
        steps.Add(new("close remaining emotes",s=>s.Buttons.ContainsKey("closeEmotes")?Press(ImGuiKey.Escape):true));
        steps.Add(new("Emotes header opens",s=>Click(s,"emotes"),s=>Require(s.Buttons.ContainsKey("closeEmotes"),"Emotes header did not open its wheel.")));
        steps.Add(new("Emotes header closes",s=>Click(s,"emotes"),s=>Require(!s.Buttons.ContainsKey("closeEmotes"),"Outside-press handling broke the emotes header toggle.")));
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
        steps.Add(new("Finish timer fixture combat",s=>s.Session!.Rounds.Phase!=MatchPhase.Combat||Click(s,"finish")));
        steps.Add(new("Advance timer fixture round",s=>s.Session!.Rounds.Phase==MatchPhase.Planning||Click(s,"next")));
        steps.Add(new("Enable planning timer",s=>s.PlanningTimerEnabled||Click(s,"planningTimer")));
        steps.Add(new("Timer keeps automatic reward choice",s=>
        {
            var session=s.Session!;var player=session.State.Players[0];
            Require(session.Rounds.Phase==MatchPhase.Planning&&session.Rounds.RoundType!="carousel","Timer fixture did not reach a normal planning round.");
            for(int stage=1;stage<=4&&player.Offer is null;stage++)for(int round=1;round<=7&&player.Offer is null;round++)session.Offers.Make(player,new(stage,round));
            Require(player.Offer is not null,"Timer fixture has no scheduled reward.");
            s.Update(120);
            Require(player.Offer is null&&session.Rounds.Phase!=MatchPhase.Planning,"Timer expiry stopped automatically resolving pending rewards.");return true;
        }));
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
    private static void CheckSocialLayout(MatchScreen screen,bool wheel)
    {
        var bounds=wheel?screen.EmoteSlotBounds:screen.SoundboardPadBounds;var display=ImGui.GetIO().DisplaySize;
        Require(bounds.Length==6,"Social controls must expose all six equipped slots.");
        for(int i=0;i<bounds.Length;i++)
        {
            var b=bounds[i];Require(b.Min.X>=0&&b.Min.Y>=0&&b.Max.X<=display.X&&b.Max.Y<=display.Y,"A social slot is outside the supported window.");
            Require(b.Max.X-b.Min.X>=80&&b.Max.Y-b.Min.Y>=80,"A social slot is too small to read or select.");
            for(int j=0;j<i;j++){var a=bounds[j];Require(b.Min.X>=a.Max.X||a.Min.X>=b.Max.X||b.Min.Y>=a.Max.Y||a.Min.Y>=b.Max.Y,"Social slot targets overlap.");}
            if(wheel){var c=(b.Min+b.Max)*.5f-screen.EmoteWheelCenter;Require(Math.Abs(Math.Sqrt(c.X*c.X+c.Y*c.Y)-124)<.1,"Emote images are not arranged around the source wheel.");}
        }
        if(!wheel)Require(screen.SoundboardColumns is 3 or 6,"Sound pads did not use the responsive source grid.");
    }
    private void AddClick(string button)=>steps.Add(new(button,s=>Click(s,button)));
    private void AddKey(string name,ImGuiKey value,Action<MatchScreen>? verify=null,bool control=false)=>steps.Add(new(name,_=>Press(value,control),verify));
    private bool Click(MatchScreen screen,string button)
    {
        if(!screen.Buttons.TryGetValue(button,out var target))return false;
        if(button=="digivolve")
        {
            var bounds=screen.EvolutionCareBounds;
            if(target.Y<bounds.Min.Y+6||target.Y>=bounds.Max.Y-6)
            {ImGui.GetIO().AddMousePosEvent(bounds.Max.X-4,(bounds.Min.Y+bounds.Max.Y)/2);ImGui.GetIO().AddMouseWheelEvent(0,target.Y<bounds.Min.Y+6?4:-4);cooldown=3;return false;}
        }
        if(button.StartsWith("preview/",StringComparison.Ordinal))
        {
            var bounds=screen.CollectionListBounds;
            if(target.Y<bounds.Min.Y+6||target.Y>=bounds.Max.Y-6)
            {ImGui.GetIO().AddMousePosEvent((bounds.Min.X+bounds.Max.X)/2,(bounds.Min.Y+bounds.Max.Y)/2);ImGui.GetIO().AddMouseWheelEvent(0,target.Y<bounds.Min.Y+6?4:-4);cooldown=3;return false;}
        }
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
        if(capture is not null)
        {
            if(!soundboardCaptured&&screen.Buttons.ContainsKey("closeSoundboard")&&screen.SoundboardPadBounds.Length==6){FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"soundboard.bmp"),size.X,size.Y);soundboardCaptured=true;}
            if(!emotesCaptured&&screen.Buttons.ContainsKey("closeEmotes")&&screen.EmoteSlotBounds.Length==6){FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"emotes.bmp"),size.X,size.Y);emotesCaptured=true;}
        }
        if(index<steps.Count||down||cooldown>0)return false;
        steps[^1].Verify?.Invoke(screen);
        if(capture is not null)FrameCapture.SaveBitmap(capture,size.X,size.Y);
        Console.WriteLine($"Match keyboard UI: {steps.Count} mouse/key steps passed: Tab/Enter navigation, nested Escape, busy evolution routes, manual reward/timer ownership, scouting/cycle/return, focused-widget Space, contextual sell, camera, wheel/pads playback and outside-click routing, modifiers, text and own-board guards.");return true;
    }
    public void Dispose()=>music.Dispose();
}

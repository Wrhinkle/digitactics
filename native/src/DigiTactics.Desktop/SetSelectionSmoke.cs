using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Actual selection controls, profile persistence and host-resolved Steam starts on an isolated fixture.</summary>
internal sealed class SetSelectionSmoke
{
    private sealed record Step(string Button,Action<MatchScreen>? Check=null);
    private readonly List<Step> steps=[];
    private readonly ContentCatalog catalog;
    private readonly NativeProfileStore profile;
    private readonly GameAudio audio;
    private readonly SetSelection choices;
    private int index,verified=-1,cooldown=5;
    private bool down,captured,escapeDown;
    internal string Pending=>index<steps.Count?steps[index].Button:"verification";
    public SetSelectionSmoke(ContentCatalog catalog,MatchScreen screen,NativeProfileStore profile,GameAudio audio,string? steamLibrary)
    {
        if(profile.Exists)throw new InvalidOperationException("Set selection smoke requires a new empty isolated profile directory.");
        if(steamLibrary is null || !Path.GetFileName(steamLibrary).Contains("fake",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Set selection smoke requires the fake Steam library.");
        this.catalog=catalog;this.profile=profile;this.audio=audio;choices=SetSelection.Load(Path.Combine(AppContext.BaseDirectory,"content/set-selection.json"));
        screen.UsePrivateLobbyForSmoke();
        void Add(string button,Action<MatchScreen>? check=null)=>steps.Add(new(button,check));
        void Leave(){Add("gameMenu");Add("requestLeave");Add("confirmLeave",s=>Require(s.Session is null,"Local leave did not return to the picker."));}
        // The original Play page picks the set with its cards; Chaos is the last card.
        Add("clientModes",s=>Require(s.Buttons.ContainsKey("homeMode/chaos"),"Chaos is absent from set options."));
        Add("homeMode/chaos",s=>Require(s.SelectedSet=="chaos"&&profile.Storage.GetValueOrDefault("dt.set.v1")=="chaos",$"Chaos selection, source artwork or persisted choice differs: {s.SelectedSet}/{profile.Storage.GetValueOrDefault("dt.set.v1")}/{s.HomeHeroArt}."));
        Add("homePlayPace/normal",s=>Require(!s.Buttons.ContainsKey("homePlayPace/chaos")&&s.Buttons.ContainsKey("homePlayPace/hyper")&&s.Buttons.ContainsKey("homePlayPace/normal"),"Solo picker pace options differ from original."));
        Add("homePlayFormat/duos");
        Add("homeOverview");Add("start",s=>CheckMatch(s,"normal",true,false));Leave();Add("clientModes");
        Add("homePlayPace/hyper");Add("homePlayFormat/solo");
        Add("homeOverview");Add("start",s=>CheckMatch(s,"hyper",false,false));Leave();
        Add("multiplayer");Add("steamConnect");Add("steamPace");Add("steamPace/chaos",_=>Require(profile.Storage["dt.pace.v1"]=="hyper","Lobby Chaos pace replaced the persisted solo pace."));
        Add("steamCreate",s=>Require(s.LobbySetId=="chaos"&&s.LobbyPaceId=="chaos","Room creation lost unresolved choices."));
        Add("steamRoomSet");Add("steamRoomSet/underlords_ko",s=>Require(s.LobbySetId=="underlords_ko","Host cannot choose an explicit set excluded from Chaos."));
        Add("steamRoomPace");Add("steamRoomPace/normal",s=>Require(s.LobbyPaceId=="normal"&&profile.Storage["dt.pace.v1"]=="normal","Host pace change was not saved."));
        Add("steamRoomFormat");Add("steamRoomFormat/duos",s=>Require(s.LobbyFormatId=="duos"&&profile.Storage["dt.format.v1"]=="solo","Host format change was not shared or overwrote the local format."));
        Add("steamRoomSet");Add("steamRoomSet/chaos");Add("steamRoomPace");Add("steamRoomPace/chaos");
        Add("steamStart",s=>CheckMatch(s,"chaos",true,true));Add("menuEscape");Add("requestLeave");Add("confirmLeave");Add("steamBack");
        Add("clientModes");Add("homePlayFormat/duos");Add("homeOverview");
    }
    private static void Require(bool value,string message){if(!value)throw new InvalidOperationException("Set selection UI: "+message);}
    private void CheckMatch(MatchScreen screen,string pace,bool duos,bool online)
    {
        var state=screen.Session?.State??throw new InvalidOperationException("Selected match did not start.");
        var expected=choices.Resolve("chaos",pace,state.Seed);
        Require(state.Rules.Id==expected.Variant&&state.Duos==duos&&state.Chaos&&state.ChaosPace==online,"Resolved set/pace/format differs from seeded choice.");
        Require(screen.SelectedSet=="chaos","Starting a resolved match replaced the original Chaos preference.");
        if(online)
        {
            var init=screen.SteamMatchInit??throw new InvalidOperationException("Steam host did not publish init.");
            Require(init.GetProperty("setId"u8).GetString()==expected.SetId&&init.GetProperty("pace"u8).GetString()==expected.Pace&&init.GetProperty("chaos"u8).GetBoolean()&&init.GetProperty("chaosPace"u8).GetBoolean(),"Steam init did not carry resolved choices and Chaos flags.");
            Require(profile.Storage["dt.pace.v1"]=="normal","Chaos pace changed the remembered fixed pace.");
        }
    }
    public void Input(MatchScreen screen)
    {
        if(cooldown-->0)return;
        if(escapeDown){ImGui.GetIO().AddKeyEvent(ImGuiKey.Escape,false);escapeDown=false;cooldown=4;return;}
        if(down){ImGui.GetIO().AddMouseButtonEvent(0,false);down=false;cooldown=4;return;}
        if(index>0&&verified<index-1){steps[index-1].Check?.Invoke(screen);verified=index-1;}
        if(index>=steps.Count)return;
        if(screen.MatchLoadingVisible)return;
        if(steps[index].Button=="menuEscape"){ImGui.GetIO().AddKeyEvent(ImGuiKey.Escape,true);escapeDown=true;index++;cooldown=3;return;}
        // Finish the real opening choice before clicking the HUD behind its
        // modal. A click on an obscured Menu must not count as navigation.
        if(steps[index].Button is "gameMenu" or "leaveOnline" && screen.Session?.Units.Carousel is { Active:true } carousel)
        {
            string? choice=carousel.Done?"carouselGo":carousel.Turn?.Id==0
                ?"carousel/"+Enumerable.Range(0,carousel.Options.Count).First(i=>carousel.Options[i].PickedBy is null):null;
            if(choice is not null && screen.Buttons.TryGetValue(choice,out var card))
            {ImGui.GetIO().AddMousePosEvent(card.X,card.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);down=true;cooldown=3;}
            return;
        }
        if(!screen.Buttons.TryGetValue(steps[index].Button,out var point))return;
        if(!screen.RevealHomePlayControl(steps[index].Button)){cooldown=3;return;}
        if(point.Y<12||point.Y>=ImGui.GetIO().DisplaySize.Y-20){ImGui.GetIO().AddMousePosEvent(point.X,ImGui.GetIO().DisplaySize.Y*.65f);ImGui.GetIO().AddMouseWheelEvent(0,point.Y<12?4:-4);cooldown=3;return;}
        ImGui.GetIO().AddMousePosEvent(point.X,point.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);down=true;index++;cooldown=3;
    }
    public bool Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(!captured&&screen.SelectedSet=="chaos"&&screen.Session is null&&index==3&&cooldown==1)
        {if(capture is not null)FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"chaos-home.bmp"),size.X,size.Y);captured=true;}
        if(index<steps.Count||down||cooldown>0)return false;
        Require(screen.Session is null&&!screen.InSteamRoom,"Steam leave left a running match/room.");
        var restoredProfile=new NativeProfileStore(profile.DataDirectory);
        using var restored=new MatchScreen(catalog,audio,profile:restoredProfile);
        Require(restored.SelectedSet=="chaos"&&restored.SelectedPace=="normal"&&restored.SelectedDuos,"A fresh launch did not restore the original set, fixed pace and format keys.");
        Console.WriteLine($"Set selection UI: {steps.Count} mouse steps passed for Chaos Solo/Duos, saved preferences, explicit excluded set, idle-host edits and resolved Steam Chaos pace.");return true;
    }
}

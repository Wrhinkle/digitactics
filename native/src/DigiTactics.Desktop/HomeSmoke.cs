using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>Source Home pages, daily rotation, category links and pedestal input through real controls.</summary>
internal sealed class HomeSmoke(NativeProfileStore? profile=null)
{
    private sealed record Step(string Name,Func<MatchScreen,bool> Input,Action<MatchScreen>? Check=null);
    private readonly PedestalPointerSmoke pointer=new(true);
    private readonly List<Step> steps=[];
    private readonly HashSet<string> captures=[];
    private int step,checkedStep=-1,cooldown=5;
    private bool down,keyDown,initialized;
    private DateTimeOffset clock;
    private double heroTimeOffset;
    private string initialSet="";
    private string[] originalOffers=[];
    public bool Done {get;private set;}
    public string Pending=>"Home/"+(step<steps.Count?steps[step].Name:"verification");
    private static void Check(bool okay,string message){if(!okay)throw new InvalidOperationException("Home UI: "+message);}
    private bool Click(MatchScreen screen,string key,(float2 Min,float2 Max)? bounds=null)
    {
        if(!screen.Buttons.TryGetValue(key,out var point))return false;
        if(!screen.RevealHomePlayControl(key)){cooldown=4;return false;}
        if(bounds is {} box&&(point.Y<box.Min.Y+12||point.Y>box.Max.Y-12))
        {var center=(box.Min+box.Max)*.5f;ImGui.GetIO().AddMousePosEvent(center.X,center.Y);ImGui.GetIO().AddMouseWheelEvent(0,point.Y<box.Min.Y+12 ? .5f : -.5f);cooldown=4;return false;}
        ImGui.GetIO().AddMousePosEvent(point.X,point.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);down=true;return true;
    }
    private bool Escape(){ImGui.GetIO().AddKeyEvent(ImGuiKey.Escape,true);down=keyDown=true;return true;}
    private bool HeroThumb(MatchScreen screen,string id)
    {
        if(!screen.Buttons.TryGetValue("homeHeroThumb/"+id,out var point))return false;
        var bounds=screen.HomeHeroThumbBounds;
        if(point.X<bounds.Min.X+8||point.X>bounds.Max.X-8)
        {var center=(bounds.Min+bounds.Max)*.5f;ImGui.GetIO().AddMousePosEvent(center.X,center.Y);ImGui.GetIO().AddMouseWheelEvent(point.X<bounds.Min.X+8?5:-5,0);cooldown=4;return false;}
        return Click(screen,"homeHeroThumb/"+id);
    }
    private bool ModeCard(MatchScreen screen,string id)
    {
        if(!screen.Buttons.ContainsKey("homeMode/"+id))return false;
        // Click reveals the card first: the row scrolls sideways and the page vertically, as needed.
        return Click(screen,"homeMode/"+id);
    }
    private void AddClick(string key,Action<MatchScreen>? check=null)=>steps.Add(new(key,s=>Click(s,key),check));
    private void Setup(MatchScreen screen)
    {
        initialized=true;clock=DateTimeOffset.Now;screen.HomeClock=()=>clock;initialSet=screen.SelectedSet;
        var heroClock=screen.HomeHeroClock;screen.HomeHeroClock=()=>heroClock()+heroTimeOffset;
        if(profile is not null)
        {
            AddClick("collection",s=>Check(s.CollectionVisible,"Navigation history fixture did not enter Collection."));
            AddClick("profile",s=>Check(s.ProfileVisible&&!s.CollectionVisible,"Profile route failed to replace Collection."));
            AddClick("closeProfile",s=>Check(!s.ProfileVisible&&s.CollectionVisible,"Profile Back lost the preceding Collection page."));
            steps.Add(new("collection-history-back",_=>Escape(),s=>Check(!s.CollectionVisible&&!s.ProfileVisible&&s.Buttons.ContainsKey("start"),"Collection Back did not return Home.")));
            AddClick("settings");AddClick("about",s=>Check(s.AboutVisible&&!s.SettingsVisible,"About did not replace Settings."));
            AddClick("closeAbout",s=>Check(!s.AboutVisible&&s.SettingsVisible,"About Back lost Settings."));
            AddClick("closeSettings",s=>Check(!s.SettingsVisible&&s.Buttons.ContainsKey("start"),"Settings Back did not return Home."));
        }
        string arrowOrigin="";
        steps.Add(new("hero-next-arrow",s=>{arrowOrigin=s.HomeHeroId;return Click(s,"homeHeroNext");},s=>Check(s.HomeHeroId!=arrowOrigin,"Next carousel arrow did not change the slide.")));
        AddClick("homeHeroPrevious",s=>Check(s.HomeHeroId==arrowOrigin,"Previous carousel arrow did not return to the same slide."));
        steps.Add(new("hero-initial",s=>HeroThumb(s,"set-faction-wars"),s=>Check(s.HomeHeroId=="set-faction-wars"&&s.HomeHeroCount==14&&s.SelectedSet==initialSet,"Carousel thumbnails changed the selected ruleset or omitted source slides.")));
        steps.Add(new("hero-hover-timeout",s=>{var b=s.HomeHeroBounds;ImGui.GetIO().AddMousePosEvent((b.Min.X+b.Max.X)/2,b.Min.Y+18);heroTimeOffset+=8.01;return true;},s=>Check(s.HomeHeroId=="set-faction-wars","Carousel advanced while hovered.")));
        steps.Add(new("hero-deferred-refresh",s=>{ImGui.GetIO().AddMousePosEvent(5,5);heroTimeOffset+=8.01;return true;},s=>Check(s.HomeHeroId=="set-faction-wars","Blocked timeout must first refresh the same slide after eight seconds.")));
        steps.Add(new("hero-auto-advance",s=>{ImGui.GetIO().AddMousePosEvent(5,5);heroTimeOffset+=8.01;return true;},s=>Check(s.HomeHeroId=="store"&&s.SelectedSet==initialSet,"The next eight-second timeout must advance to Store without changing the ruleset.")));
        steps.Add(new("hero-store-settled",_=>{heroTimeOffset+=1;return true;}));
        if(profile is not null)
        {
            AddClick("homeHeroAction",s=>Check(s.CollectionVisible,"Featured hero CTA failed to open its Store category."));
            steps.Add(new("hero-store-return",_=>Escape(),s=>Check(!s.CollectionVisible,"Store hero did not return Home.")));
            // Leaving the Store via persistent navigation preserves the pending
            // confirmation until the next route explicitly clears it. Use the
            // visible Loot tab: the full source catalog omits its fallback hero.
            AddClick("clientStore");
            AddClick("collectionTab/0");
            steps.Add(new("loot-route-select-purchase",s=>Click(s,"preview/legend/bukamon",s.CollectionListBounds)));
            steps.Add(new("loot-route-pending-purchase",s=>Click(s,"buyCosmetic/legend/bukamon",s.CollectionDetailBounds),
                s=>Check(s.Buttons.ContainsKey("confirmPurchase"),"Store purchase confirmation did not open for the Loot route regression.")));
            AddClick("homeOverview",s=>Check(s.CollectionVisible&&s.Buttons.ContainsKey("confirmPurchase"),"Navigation escaped the pending purchase modal."));
            steps.Add(new("cancel-purchase-before-navigation",_=>Escape(),s=>Check(s.CollectionVisible&&!s.Buttons.ContainsKey("confirmPurchase"),"Escape did not dismiss exactly the purchase confirmation.")));
            AddClick("homeOverview");
            AddClick("clientLoot",s=>Check(s.CollectionLootVisible&&!s.CollectionStoreVisible&&s.DisplayedIncubator is not null
                &&s.Buttons.ContainsKey("toggleEggs")&&!s.Buttons.ContainsKey("confirmPurchase"),"Loot navigation reopened a stale Store confirmation instead of the incubator."));
            steps.Add(new("loot-route-return",_=>Escape(),s=>Check(!s.CollectionVisible,"Loot did not return Home.")));
        }
        steps.Add(new("hero-multiplayer",s=>HeroThumb(s,"mp"),s=>Check(s.HomeHeroId=="mp"&&s.SelectedSet==initialSet,"Multiplayer thumbnail did not select its source slide independently of the ruleset.")));
        steps.Add(new("hero-multiplayer-settled",_=>{heroTimeOffset+=1;return true;}));
        AddClick("homeHeroAction",s=>Check(s.Buttons.ContainsKey("steamBack"),"Multiplayer hero CTA did not open lobby discovery."));AddClick("steamBack");
        steps.Add(new("hero-chaos",s=>HeroThumb(s,"set-chaos"),s=>Check(s.HomeHeroId=="set-chaos"&&s.SelectedSet==initialSet,"Chaos thumbnail must preview without selecting the game.")));
        AddClick("homeHeroAction",s=>Check(s.HomePlayVisible&&s.SelectedSet=="chaos"&&s.Buttons.ContainsKey("homePlayStart"),"Set hero CTA must select its set and open launch configuration."));
        steps.Add(new("hero-play-return",_=>Escape(),s=>Check(s.HomePlayVisible&&!s.HomePlayPartyVisible,"Launch selection Escape did not return to Modes.")));
        AddClick("clientModes",s=>Check(s.HomePlayVisible&&!s.HomePlayPartyVisible&&s.HomePlayLayoutFits&&s.HomeModeCardCount==12&&s.Buttons.ContainsKey("homeMode/faction-wars"),"Modes navigation did not show selectable ruleset cards."));
        steps.Add(new("mode-card-faction-wars",s=>ModeCard(s,"faction-wars"),s=>Check(s.SelectedSet=="faction-wars","Ruleset card did not select Faction Wars.")));
        AddClick("homePlayConfirm",s=>Check(s.HomePlayPartyVisible&&s.HomePlayLayoutFits&&s.HomePartySeatCount==8&&s.Buttons.ContainsKey("homePlayStart"),"Mode confirmation did not reach the party/start screen."));
        AddClick("homePlayModes",s=>Check(s.HomePlayVisible&&!s.HomePlayPartyVisible,"Party Modes button did not return to mode select."));
        AddClick("clientModes");steps.Add(new("mode-card-initial",s=>ModeCard(s,initialSet),s=>Check(s.SelectedSet==initialSet&&s.HomeHeroId=="set-"+initialSet,"Ruleset card did not preserve its selection and reveal the matching hero.")));AddClick("homeOverview");
        AddClick("about",s=>Check(s.AboutVisible,"About did not open."));AddClick("closeAbout",s=>Check(!s.AboutVisible&&s.Buttons.ContainsKey("start"),"About did not return Home."));
        AddClick("homeTab/patch",s=>Check(s.HomeTab=="patch"&&s.HomeHighlights==7&&s.HomeRotationSets==11,"Patch notes omitted source highlights or rotation sets."));
        steps.Add(new("patch-scroll",s=>{var b=s.HomeArticleBounds;ImGui.GetIO().AddMousePosEvent((b.Min.X+b.Max.X)/2,(b.Min.Y+b.Max.Y)/2);ImGui.GetIO().AddMouseWheelEvent(0,-24);return true;},s=>Check(s.HomeArticleScroll>0,"Full patch notes cannot scroll through the set rotation.")));
        AddClick("homeTab/events",s=>Check(s.HomeTab=="events"&&s.HomeCountdown==HomeRotation.Label(HomeRotation.UntilMidnight(clock,TimeZoneInfo.Local)),"Events countdown diverged from local midnight."));
        if(profile is not null)
        {
            steps.Add(new("event-store",s=>Click(s,"homeEventsStore",s.HomeArticleBounds),s=>Check(s.CollectionVisible,"Events Store link failed.")));
            steps.Add(new("return-events",_=>Escape(),s=>Check(!s.CollectionVisible&&s.HomeTab=="events","Store did not preserve its Events origin.")));
            steps.Add(new("event-farm",s=>Click(s,"homeEventsFarm",s.HomeArticleBounds),s=>Check(s.Buttons.ContainsKey("closeFarm"),"Events Digi-Farm link failed.")));
            AddClick("closeFarm");
        }
        AddClick("homeTab/overview",s=>{Check(s.HomeTab=="overview"&&s.HomeFeaturedCount==4,"Overview must show all four daily offers.");originalOffers=s.HomeFeaturedKeys.ToArray();});
        steps.Add(new("midnight-rotation",s=>{clock=clock.AddDays(1);return true;},s=>
        {
            var expected=CosmeticCollection.Featured(DateOnly.FromDateTime(clock.LocalDateTime)).Select(o=>o.Kind+"/"+o.Item.Id).ToArray();
            Check(s.HomeFeaturedKeys.SequenceEqual(expected)&&!s.HomeFeaturedKeys.SequenceEqual(originalOffers),"Featured row did not refresh at the next local date.");
        }));
        steps.Add(new("restore-day",s=>{clock=DateTimeOffset.Now;return true;},s=>
        {
            var collection=new CosmeticCollection(profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"));var expected=CosmeticCollection.Featured(DateOnly.FromDateTime(clock.LocalDateTime));
            Check(s.HomeFeaturedCount==4&&s.HomeFeaturedOwned==expected.Count(o=>collection.Owns(o.Kind,o.Item.Id)),"Featured ownership badges diverged from the collection.");
        }));
        if(profile is not null)for(int i=0;i<4;i++)
        {
            int offer=i;
            steps.Add(new("featured-link-"+i,s=>Click(s,"homeFeatured/"+s.HomeFeaturedKeys[offer],s.HomeStoreBounds),s=>
            {var parts=s.HomeFeaturedKeys[offer].Split('/');Check(s.CollectionStoreVisible&&s.CollectionFilterKind==parts[0]&&s.CollectionSelectedId==parts[1]&&s.CollectionFilterIds.Contains(parts[1]),"Featured link did not select its item in the matching Store category.");}));
            steps.Add(new("featured-return-"+i,_=>Escape(),s=>Check(!s.CollectionVisible,"Featured Store Escape did not return Home.")));
        }
        steps.Add(new("overview-patch-link",s=>Click(s,"homeReadPatch",s.HomePatchBounds),s=>Check(s.HomeTab=="patch","Overview patch link failed.")));
        AddClick("homeTab/overview");steps.Add(new("set-news-codex",s=>Click(s,"codex"),s=>Check(s.CodexFullVisible,"Set mechanic news did not open Codex.")));
        steps.Add(new("return-overview",_=>Escape(),s=>Check(!s.CodexVisible&&s.HomeTab=="overview","Set news Codex did not return Home.")));
        // The subsequent planning/shop driver is the Normal-pace fixture. Select
        // it through the visible control without changing the source Hyper default.
        AddClick("clientModes");AddClick("homePlayPace/normal",s=>Check(s.SelectedPace=="normal","Could not select Normal pace for the downstream shop fixture."));AddClick("homeOverview");
    }
    public void Input(MatchScreen screen)
    {
        if(Done)return;
        if(!initialized)Setup(screen);
        if(!pointer.Input(screen)||cooldown-->0)return;
        if(down)
        {if(keyDown)ImGui.GetIO().AddKeyEvent(ImGuiKey.Escape,false);else ImGui.GetIO().AddMouseButtonEvent(0,false);down=keyDown=false;cooldown=3;return;}
        if(step>0&&checkedStep<step-1){steps[step-1].Check?.Invoke(screen);checkedStep=step-1;}
        if(step>=steps.Count)
        {
            screen.HomeClock=()=>DateTimeOffset.Now;Done=true;
            Console.WriteLine("Home UI passed: source artwork/pedestal, selectable hero thumbnails, eight-second hover/retry rotation, Store/Multiplayer/set-launch CTAs, purchase-modal containment and Loot routing, Overview/Patch notes/Events, all highlights/rotation sets, daily midnight refresh, four featured prices/ownership/category links, set-news Codex, Store/Farm and About navigation.");return;
        }
        if(steps[step].Input(screen)){step++;cooldown=3;}
    }
    public void Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(Done||cooldown!=1)return;
        if(step==0)
        {
            Check(screen.HomeLayoutFits&&screen.HomeHeroArt is not null&&screen.HomeLegendDrawCalls>0,"Selected set artwork, live Little Legend or responsive layout is missing.");
            foreach(string key in new[]{"start","multiplayer","clientModes","settings","about","homeTab/patch","homeTab/events"})
                Check(screen.Buttons.TryGetValue(key,out var point)&&point.X>=0&&point.X<size.X&&point.Y>=0&&point.Y<size.Y,"Control outside supported window: "+key);
        }
        string scene=screen.AboutVisible?"about":screen.HomePlayVisible?(screen.HomePlayPartyVisible?"hero-play":"mode-select"):screen.CodexVisible?"set-news-codex":screen.CollectionVisible?"featured-store":screen.HomeTab=="overview"&&screen.HomeHeroId is "store" or "mp"?"hero-"+screen.HomeHeroId:screen.HomeTab+(screen.HomeArticleScroll>0&&screen.HomeTab=="patch"?"-scrolled":"");
        bool enteringHero=(scene is "overview" or "hero-store" or "hero-mp")&&screen.HomeHeroAge<.8;
        if(capture is not null&&!enteringHero&&captures.Add(scene))FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"home-"+scene+".bmp"),size.X,size.Y);
    }
}

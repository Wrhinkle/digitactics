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
    private string[] originalOffers=[];
    public bool Done {get;private set;}
    private static void Check(bool okay,string message){if(!okay)throw new InvalidOperationException("Home UI: "+message);}
    private bool Click(MatchScreen screen,string key,(float2 Min,float2 Max)? bounds=null)
    {
        if(!screen.Buttons.TryGetValue(key,out var point))return false;
        if(bounds is {} box&&(point.Y<box.Min.Y+20||point.Y>box.Max.Y-24))
        {var center=(box.Min+box.Max)*.5f;ImGui.GetIO().AddMousePosEvent(center.X,center.Y);ImGui.GetIO().AddMouseWheelEvent(0,point.Y<box.Min.Y+20?4:-4);cooldown=4;return false;}
        ImGui.GetIO().AddMousePosEvent(point.X,point.Y);ImGui.GetIO().AddMouseButtonEvent(0,true);down=true;return true;
    }
    private bool Escape(){ImGui.GetIO().AddKeyEvent(ImGuiKey.Escape,true);down=keyDown=true;return true;}
    private void AddClick(string key,Action<MatchScreen>? check=null)=>steps.Add(new(key,s=>Click(s,key),check));
    private void Setup(MatchScreen screen)
    {
        initialized=true;clock=DateTimeOffset.Now;screen.HomeClock=()=>clock;
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
            {var parts=s.HomeFeaturedKeys[offer].Split('/');Check(s.CollectionVisible&&s.CollectionPreviewKind==parts[0]&&s.CollectionSelectedId==parts[1]&&s.CollectionFilterQuery==""&&s.CollectionFilterOwnership=="all"&&s.CollectionFilterRarity=="all","Featured link did not select its item/category with visible Store filters.");}));
            steps.Add(new("featured-return-"+i,_=>Escape(),s=>Check(!s.CollectionVisible,"Featured Store Escape did not return Home.")));
        }
        steps.Add(new("overview-patch-link",s=>Click(s,"homeReadPatch",s.HomeProgressBounds),s=>Check(s.HomeTab=="patch","Overview patch link failed.")));
        AddClick("homeTab/overview");steps.Add(new("set-news-codex",s=>Click(s,"homeSetCodex",s.HomeStoreBounds),s=>Check(s.CodexFullVisible,"Set mechanic news did not open Codex.")));
        steps.Add(new("return-overview",_=>Escape(),s=>Check(!s.CodexVisible&&s.HomeTab=="overview","Set news Codex did not return Home.")));
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
            Console.WriteLine("Home UI passed: source artwork/pedestal, Overview/Patch notes/Events, all highlights/rotation sets, daily midnight refresh, four featured prices/ownership/category links, set-news Codex, Store/Farm and About navigation.");return;
        }
        if(steps[step].Input(screen)){step++;cooldown=3;}
    }
    public void Verify(MatchScreen screen,int2 size,string? capture)
    {
        if(Done||cooldown!=1)return;
        if(step==0)
        {
            Check(screen.HomeLayoutFits&&screen.HomeHeroArt is not null&&screen.HomeLegendDrawCalls>0,"Selected set artwork, live Little Legend or responsive layout is missing.");
            foreach(string key in new[]{"start","multiplayer","homeRuleset","reconnectAudio","about","homeTab/patch","homeTab/events"})
                Check(screen.Buttons.TryGetValue(key,out var point)&&point.X>=0&&point.X<size.X&&point.Y>=0&&point.Y<size.Y,"Control outside supported window: "+key);
        }
        string scene=screen.AboutVisible?"about":screen.CodexVisible?"set-news-codex":screen.CollectionVisible?"featured-store":screen.HomeTab+(screen.HomeArticleScroll>0&&screen.HomeTab=="patch"?"-scrolled":"");
        if(capture is not null&&captures.Add(scene))FrameCapture.SaveBitmap(Path.ChangeExtension(capture,"home-"+scene+".bmp"),size.X,size.Y);
    }
}

using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private static readonly Lazy<HomeContent> homeContent=new(()=>HomeContent.Load(Path.Combine(AppContext.BaseDirectory,"content/home/content.json")));
    private string homeTab="overview";
    private readonly Dictionary<string,CosmeticScenePreview> homeOfferPreviews=[];
    internal Func<DateTimeOffset> HomeClock {get;set;}=()=>DateTimeOffset.Now;
    internal string HomeTab=>homeTab;
    internal int HomeHighlights {get;private set;}
    internal int HomeRotationSets {get;private set;}
    internal int HomeFeaturedCount {get;private set;}
    internal int HomeFeaturedOwned {get;private set;}
    internal string[] HomeFeaturedKeys {get;private set;}=[];
    internal string HomeCountdown {get;private set;}="";
    internal (float2 Min,float2 Max) HomeArticleBounds {get;private set;}
    internal (float2 Min,float2 Max) HomeStoreBounds {get;private set;}
    internal (float2 Min,float2 Max) HomeProgressBounds {get;private set;}
    internal (float2 Min,float2 Max) HomePatchBounds {get;private set;}
    internal float HomeArticleScroll {get;private set;}
    private DateOnly HomeDay=>DateOnly.FromDateTime(HomeClock().LocalDateTime);
    private DailyRewardProgress HomeDaily=>MatchRewards.Progress(profile?.Storage.GetValueOrDefault("dt.shop.v1"),profile?.Storage.GetValueOrDefault("dt.client.v1"),HomeDay);
    private void RefreshHomeOffers()
    {
        var day=HomeDay;if(homeOfferDay==day)return;
        ClearHomeOfferPreviews();
        homeOffers=CosmeticCollection.Featured(day).Select(offer=>(offer.Kind,offer.Item,Price:CosmeticCollection.Price(offer.Kind,offer.Item,day))).ToArray();
        HomeFeaturedKeys=homeOffers.Select(offer=>offer.Kind+"/"+offer.Item.Id).ToArray();homeOfferDay=day;
    }
    private void HomeTabs(float x,float y,float width)
    {
        HomeCard("Home tabs",new(x,y),new(width,40),false);
        ImGui.SetCursorPos(new(6,2));
        foreach(var tab in new[]{("overview","Overview"),("patch","Patch notes"),("events","Events")})
        {
            if(tab.Item1!="overview")ImGui.SameLine();
            if(HomeButton("homeTab/"+tab.Item1,tab.Item2,120,36,homeTab==tab.Item1))homeTab=tab.Item1;
        }
        HomeCountdown=HomeRotation.Label(HomeRotation.UntilMidnight(HomeClock(),TimeZoneInfo.Local));
        string reset="Daily reset in "+HomeCountdown;float resetWidth=ImGui.CalcTextSize(reset).X;
        ImGui.SetCursorPos(new(Math.Max(384,width-resetWidth-16),12));HomeLabel(reset,13,HomeMute);ImGui.End();
    }
    private void HomeHighlightsList(int count)
    {
        foreach(var highlight in homeContent.Value.Patch.Take(count))
        {HomeLabel(highlight.Title,15,HomeInk);UiText.Wrapped(highlight.Description);ImGui.Spacing();HomeHighlights++;}
    }
    private void HomePatchPreview()
    {
        ImGui.Separator();HomeLabel("WHAT'S NEW");HomeHighlightsList(3);
        if(HomeButton("homeReadPatch","Read the patch notes",-1,30))homeTab="patch";
    }
    private void HomeSetNews()
    {
        var set=HomeSets.Value[selectedSet];ImGui.Separator();HomeLabel("THIS SET: "+set.Name.ToUpperInvariant());
        if(selectedSet=="chaos")
        {
            HomeLabel("Any set",15,HomeInk);UiText.Wrapped("The seed picks the set. Everyone in the lobby plays the same one."u8);
            HomeLabel("Pool",15,HomeInk);UiText.Wrapped(string.Join(", ",SetChoices.Options.Select(option=>option.Name)));
        }
        else
        {
            HomeLabel(string.IsNullOrEmpty(set.Mechanic)?"Set mechanic":set.Mechanic,15,HomeInk);
            UiText.Wrapped(string.IsNullOrEmpty(set.MechanicDescription)?set.Description:set.MechanicDescription);
            HomeLabel("Roster",15,HomeInk);UiText.Wrapped($"{Rules.Lines.Length} shop Digimon and {Rules.Traits.Length} traits.");
        }
        if(HomeButton("homeSetCodex","Open the Codex",-1,30))OpenCodexFull();
    }
    private void HomeArticleBanner(string image,string eyebrow,string title,string description,float height=150)
    {
        var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;
        homeArtwork.Draw(image,at,new(width,height),255,true);
        ImGui.GetWindowDrawList().AddRectFilled(at,at+new float2(width,height),new ImColor8(3,12,22,160));
        ImGui.SetCursorScreenPos(at+new float2(18,16));HomeLabel(eyebrow.ToUpperInvariant(),13);
        HomeLabel(title,30,HomeInk);ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+width-36);ImGui.Text(description);ImGui.PopTextWrapPos();
        // The artwork is drawn directly, so it still needs a layout item when
        // no optional Store/Farm button follows it (the profile-less client).
        ImGui.SetCursorScreenPos(at+new float2(0,height));
        ImGui.Dummy(new(width,Math.Max(0,12-ImGui.GetStyle().ItemSpacing.Y)));
    }
    private void HomePatchNotes(float2 at,float2 extent)
    {
        HomeArticleBounds=(at,at+extent);
        HomeCard("Home patch notes",at,extent);HomeArticleBanner("hero-digivice-energy","Patch notes","The new client","A launcher for everything outside the match.");
        HomeLabel("HIGHLIGHTS");HomeHighlightsList(homeContent.Value.Patch.Length);ImGui.Separator();HomeLabel("SETS IN THE ROTATION");
        foreach(var option in SetChoices.Options)
        {
            var set=HomeSets.Value[option.Id];var rules=catalog.Variants.First(r=>r.Id.Split('/')[0]==option.Id);
            HomeLabel(set.Name,20,HomeInk);UiText.Wrapped(set.Description);
            UiText.Wrapped($"{rules.Lines.Length} Digimon / {rules.Traits.Length} traits"+(set.Mechanic.Length>0?" / "+set.Mechanic:""));
            ImGui.Separator();HomeRotationSets++;
        }
        HomeArticleScroll=ImGui.GetScrollY();ImGui.End();
    }
    private void HomeMissions(DailyRewardProgress daily)
    {
        foreach(var mission in daily.Missions)
        {
            UiText.Wrapped(mission.Name);HomeLabel($"{mission.Bits} Bits  /  "+(mission.Done?"Complete":$"Best today {mission.Current}/{mission.Goal}"),12,HomeMute);
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram,new ImColor8(49,185,202));ImGui.ProgressBar((float)mission.Current/mission.Goal,new(-1,5),""u8);ImGui.PopStyleColor();
        }
        ImGui.Separator();UiText.Wrapped(daily.Claimed?"Daily bonus claimed. Back tomorrow.":$"Your first game today earns {daily.Bits} extra Bits.");
    }
    private void HomeEventsPanel(float2 at,float2 extent)
    {
        HomeArticleBounds=(at,at+extent);
        HomeCard("Home events",at,extent);var copy=homeContent.Value.Events;var daily=HomeDaily;
        HomeArticleBanner("events-bg","Events",copy.Title,copy.Countdown.Replace("{countdown}",HomeCountdown,StringComparison.Ordinal));
        HomeLabel(copy.Missions.ToUpperInvariant());HomeMissions(daily);ImGui.Spacing();
        HomeArticleBanner("store-hero","Store",copy.Store,daily.Claimed?"Four items at 25% off, new every day. Your daily bonus is claimed.":copy.StoreDescription.Replace("{bits}",daily.Bits.ToString(),StringComparison.Ordinal),130);
        if(profile is not null&&HomeButton("homeEventsStore","Open the Store",210,32))OpenHomeStore();
        ImGui.Spacing();HomeArticleBanner("hero-file-island","Digi-Farm",copy.Farm,copy.FarmDescription,130);
        if(farm is not null&&HomeButton("homeEventsFarm","Open the Digi-Farm",210,32)){farm.Refresh(HomeClock().ToUnixTimeMilliseconds());showFarm=true;}
        HomeArticleScroll=ImGui.GetScrollY();ImGui.End();
    }
    private void OpenHomeStore(string? kind=null,string? id=null)
    {
        if(profile is null)return;
        collectionOwnedOnly=false;collectionRarity="all";collectionSearch.SetValue("");
        OpenCollectionPage(store:true,kind:kind,item:id);
    }
    private void HomeOfferArt(string kind,CosmeticItem item,CosmeticCollection collection,float2 at,float2 extent)
    {
        if(!ImGui.IsRectVisible(at,at+extent))return;
        var draw=ImGui.GetWindowDrawList();
        if(kind=="emote")emoteArtwork.Draw(item.Id,(long)(boardSceneTime*1000),at,extent.Y);
        else if(kind=="killscreen")collectionTheme.Draw(item.Id,false,boardSceneTime%3.5,at,extent);
        else if(kind is "legend" or "dance" or "board")
        {
            string key=kind+":"+item.Id;if(!homeOfferPreviews.TryGetValue(key,out var scene))homeOfferPreviews[key]=scene=new();
            string legend=kind=="legend"?item.Id:collection.Equipped("legend")?.GetValue<string>()??"tokomon";
            var view=LegendEvolution.Inspect(profile?.Storage.GetValueOrDefault("dt.legendevo.v1"),profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"),profile?.Storage.GetValueOrDefault("dt.farm.v1"),legend);
            scene.Draw(kind,item.Id,LegendEvolution.VisualKey(legend,view.Form),boardSceneTime,ClientMotion);
            draw.AddImage(new ImTextureRef{_TexID=new((nint)scene.Texture)},at,at+extent,new(0,1),new(1,0));
        }
        else
        {var center=at+extent*.5f;draw.AddCircleFilled(center,extent.Y*.4f,new ImColor8(34,54,78));draw.AddTriangleFilled(center+new float2(-5,-9),center+new float2(-5,9),center+new float2(9,0),RarityColor(item.Rarity));}
    }
    private void HomeOfferRow(string kind,CosmeticItem item,int price,CosmeticCollection collection,float width)
    {
        bool owned=collection.Owns(kind,item.Id);HomeFeaturedCount++;if(owned)HomeFeaturedOwned++;
        var at=ImGui.GetCursorScreenPos();const float height=72;
        bool clicked=HomeButton("homeFeatured/"+kind+"/"+item.Id,"",width,height);
        var draw=ImGui.GetWindowDrawList();draw.PushClipRect(at,at+new float2(width,height),true);
        draw.AddRect(at,at+new float2(width,height),RarityColor(item.Rarity),2);
        HomeOfferArt(kind,item,collection,at+new float2(5,12),new(62,46));
        ImGui.SetCursorScreenPos(at+new float2(75,6));ImGui.PushTextWrapPos(ImGui.GetCursorPosX()+width-88);HomeLabel(item.Name,14,HomeInk);ImGui.PopTextWrapPos();
        ImGui.SetCursorScreenPos(at+new float2(75,39));HomeLabel(CollectionNames[Array.IndexOf(CosmeticCollection.Kinds,kind)],11,HomeMute);
        ImGui.SetCursorScreenPos(at+new float2(75,54));HomeLabel(owned?"Owned":$"-{Math.Round(100-100d*price/Math.Max(1,item.Price),MidpointRounding.AwayFromZero)}% / {price} Bits",12,owned?new ImColor8(95,211,106):HomeGold);
        draw.PopClipRect();ImGui.SetCursorScreenPos(at+new float2(0,height+7));ImGui.Dummy(new(width,1));
        if(clicked)OpenHomeStore(kind,item.Id);
    }
    private void ClearHomeOfferPreviews(){foreach(var scene in homeOfferPreviews.Values)scene.Dispose();homeOfferPreviews.Clear();}
}

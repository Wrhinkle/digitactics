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
        HomeCard("Home tabs",new(x,y),new(width,39),false);
        var draw=ImGui.GetWindowDrawList();draw.AddRectFilled(new(x,y),new(x+width,y+39),new ImColor8(4,17,26));
        float left=0;
        foreach(var tab in new[]{("overview","OVERVIEW",110f),("patch","PATCH NOTES",146f),("events","EVENTS",98f)})
        {
            ImGui.SetCursorPos(new(left,0));
            if(ClientTextButton("homeTab/"+tab.Item1,tab.Item2,tab.Item3,39,homeTab==tab.Item1,15))homeTab=tab.Item1;
            left+=tab.Item3;
        }
        HomeCountdown=HomeRotation.Label(HomeRotation.UntilMidnight(HomeClock(),TimeZoneInfo.Local));ImGui.End();
    }
    private void HomeHighlightsList(int count)
    {
        foreach(var highlight in homeContent.Value.Patch.Take(count))
        {HomeLabel(highlight.Title,15,HomeInk);UiText.Wrapped(highlight.Description);ImGui.Spacing();HomeHighlights++;}
    }
    private void HomePatchPreview()
    {
        HomeLabel("WHAT'S NEW",14,HomeInk);
        float width=ImGui.GetContentRegionAvail().X,bodyHeight=Math.Max(34,ImGui.GetContentRegionAvail().Y-40);
        ImGui.BeginChild("Home patch excerpts"u8,new(width,bodyHeight),ImGuiChildFlags.None,ImGuiWindowFlags.NoScrollbar);
        foreach(var highlight in homeContent.Value.Patch.Take(2))
        {HomeLabel(highlight.Title,15,HomeInk);HomeTileExcerpt(highlight.Description,2);HomeHighlights++;}
        ImGui.EndChild();
        if(ClientTextButton("homeReadPatch","READ THE PATCH NOTES >",width,32,false,17))homeTab="patch";
    }
    private void HomeSetNews()
    {
        var set=HomeSets.Value[selectedSet];HomeLabel(LobbyEllipsis("THIS SET: "+set.Name.ToUpperInvariant(),ImGui.GetContentRegionAvail().X),13,HomeInk);
        float width=ImGui.GetContentRegionAvail().X,bodyHeight=Math.Max(34,ImGui.GetContentRegionAvail().Y-40);
        ImGui.BeginChild("Home set excerpt"u8,new(width,bodyHeight),ImGuiChildFlags.None,ImGuiWindowFlags.NoScrollbar);
        HomeLabel(selectedSet=="chaos"?"Any set":string.IsNullOrEmpty(set.Mechanic)?"Set mechanic":set.Mechanic,15,HomeInk);
        HomeTileExcerpt(selectedSet=="chaos"?"The seed picks the set. Everyone in the lobby plays the same one.":string.IsNullOrEmpty(set.MechanicDescription)?set.Description:set.MechanicDescription,2);
        HomeLabel("Roster",15,HomeInk);HomeTileExcerpt($"{Rules.Lines.Length} shop Digimon and {Rules.Traits.Length} traits.",1);
        ImGui.EndChild();
        if(ClientTextButton("homeSetCodex","OPEN THE CODEX >",width,32,false,17))OpenCodexFull();
    }
    private void HomeTileExcerpt(string text,int lines)
    {
        var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;
        ImGui.PushFont(NativeUiTheme.BodyFont,14);float height=Math.Min(ImGui.CalcTextSize(text,false,width).Y,ImGui.GetTextLineHeight()*lines);
        var draw=ImGui.GetWindowDrawList();draw.PushClipRect(at,at+new float2(width,height),true);
        ImGui.PushStyleColor(ImGuiCol.Text,HomeMute);ImGui.PushTextWrapPos(0);ImGui.Text(text);ImGui.PopTextWrapPos();ImGui.PopStyleColor();draw.PopClipRect();ImGui.PopFont();
        if(ImGui.IsWindowHovered()&&ImGui.IsMouseHoveringRect(at,at+new float2(width,height)))ResultTooltip(text);
        ImGui.SetCursorScreenPos(at+new float2(0,height+4));ImGui.Dummy(new(width,0));
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
        if(farm is not null&&HomeButton("homeEventsFarm","Open the Digi-Farm",210,32)){NavigateClient("farm");}
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
        var at=ImGui.GetCursorScreenPos();const float height=52;
        bool clicked=HomeButton("homeFeatured/"+kind+"/"+item.Id,"",width,height);
        var draw=ImGui.GetWindowDrawList();draw.PushClipRect(at,at+new float2(width,height),true);
        draw.AddRect(at,at+new float2(width,height),RarityColor(item.Rarity),2);
        HomeOfferArt(kind,item,collection,at+new float2(4,5),new(48,40));
        ImGui.SetCursorScreenPos(at+new float2(60,5));HomeLabel(LobbyEllipsis(item.Name,width-68),14,HomeInk);
        ImGui.SetCursorScreenPos(at+new float2(60,27));HomeLabel(owned?"Owned":price+" Bits",13,owned?new ImColor8(95,211,106):HomeGold);
        draw.PopClipRect();ImGui.SetCursorScreenPos(at+new float2(0,height+4));ImGui.Dummy(new(width,1));
        if(clicked)OpenHomeStore(kind,item.Id);
    }
    private void ClearHomeOfferPreviews(){foreach(var scene in homeOfferPreviews.Values)scene.Dispose();homeOfferPreviews.Clear();}
}

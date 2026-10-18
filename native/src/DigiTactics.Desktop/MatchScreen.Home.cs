using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private sealed record HomeSet(string Name,string Description,string Mechanic,string MechanicDescription,string Hero,string Tag);
    private static readonly Lazy<Dictionary<string,HomeSet>> HomeSets=new(()=>JsonSerializer.Deserialize<Dictionary<string,HomeSet>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"content/home/sets.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!);
    private readonly PostgameArtwork homeArtwork=new("home");
    private CosmeticScenePreview? homeLegend;
    private DateOnly homeOfferDay;
    private (string Kind,CosmeticItem Item,int Price)[] homeOffers=[];
    private bool showAbout;
    private string? homeDocumentError;
    internal bool AboutVisible=>showAbout;
    private static readonly ImColor8 HomeGold=new(200,170,110),HomeInk=new(240,230,210),HomeMute=new(165,174,184),HomeLine=new(92,78,53);
    internal int HomeLegendDrawCalls=>homeLegend?.DrawCalls??0;
    internal int HomePedestalDrawCalls=>homeLegend?.PedestalDrawCalls??0;
    internal double HomeLegendYaw=>homeLegend?.PointerYaw??0;
    internal int HomeLegendReactions=>homeLegend?.PointerReactionCount??0;
    internal string? HomeHeroArt {get;private set;}
    internal bool HomeLayoutFits {get;private set;}
    private static void HomeLabel(string value,float font=14,ImColor8? color=null)
    {
        ImGui.PushFont(default,font);ImGui.PushStyleColor(ImGuiCol.Text,color??HomeGold);ImGui.Text(value);ImGui.PopStyleColor();ImGui.PopFont();
    }
    private bool HomeButton(string key,string label,float width=0,float height=30,bool primary=false)
    {
        ImGui.PushStyleColor(ImGuiCol.Button,primary?new ImColor8(17,92,113):new ImColor8(17,29,42));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered,primary?new ImColor8(28,123,145):new ImColor8(35,52,68));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive,new ImColor8(29,108,124));ImGui.PushStyleColor(ImGuiCol.Border,HomeGold);ImGui.PushStyleColor(ImGuiCol.Text,HomeInk);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,1);ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,2);
        bool result=Button(key,label,width,height);ImGui.PopStyleVar(2);ImGui.PopStyleColor(5);return result;
    }
    private void HomeCard(string id,float2 at,float2 extent,bool scroll=true,bool bringToFront=false)
    {
        ImGui.PushStyleColor(ImGuiCol.WindowBg,new ImColor8(5,15,25,250));ImGui.PushStyleColor(ImGuiCol.Border,HomeLine);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding,2);ImGui.SetNextWindowBgAlpha(1);
        Window(id,at.X,at.Y,extent.X,extent.Y,ImGuiWindowFlags.NoTitleBar|(bringToFront?ImGuiWindowFlags.None:ImGuiWindowFlags.NoBringToFrontOnFocus)|(scroll?ImGuiWindowFlags.None:ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse));
        ImGui.PopStyleVar();ImGui.PopStyleColor(2);
    }
    private void HomePanel()
    {
        RefreshSetSelection();
        UpdateClient?.Refresh();bool update=UpdateClient?.Status?.Phase is "checking" or "downloading" or "verifying" or "ready" or "manual";
        float margin=24,gap=12,width=size.X-margin*2,right=Math.Clamp(size.X*.25f,300,340),heroWidth=width-right-gap;
        float tabsTop=width<1150?124:88,top=tabsTop+40,heroHeight=Math.Clamp((size.Y-top-margin-gap-(update?96:0))*.52f,220,380),bottom=top+heroHeight+gap,lowerHeight=size.Y-bottom-margin-(update?96:0);
        HomeLayoutFits=lowerHeight>=220 && margin+width<=size.X && bottom+lowerHeight<=size.Y;
        var background=ImGui.GetBackgroundDrawList();
        homeArtwork.Draw("hero-datastream-sky",new(0,0),new(size.X,size.Y),70,true,drawList:background);
        background.AddRectFilled(new(0,0),new(size.X,size.Y),new ImColor8(1,10,19,205));
        if(showAbout){HomeAbout();UpdatePanel();return;}
        HomeHighlights=HomeRotationSets=HomeFeaturedCount=HomeFeaturedOwned=0;
        HomeNavigation(margin,width);
        HomeTabs(margin,tabsTop,width);
        if(homeTab=="patch")HomePatchNotes(new(margin,top),new(heroWidth,heroHeight+gap+lowerHeight));
        else if(homeTab=="events")HomeEventsPanel(new(margin,top),new(heroWidth,heroHeight+gap+lowerHeight));
        else
        {
            HomeHero(new(margin,top),new(heroWidth,heroHeight));
            float tileWidth=(heroWidth-gap)/2;
            HomeProgress(new(margin,bottom),new(tileWidth,lowerHeight));
            HomeFeatured(new(margin+tileWidth+gap,bottom),new(tileWidth,lowerHeight));
        }
        HomePartner(new(margin+heroWidth+gap,top),new(right,heroHeight));
        HomeCard("Home sound",new(margin+heroWidth+gap,bottom),new(right,lowerHeight));
        HomeLabel("SOUND & MUSIC");ImGui.Separator();ImGui.PushItemWidth(130);AudioControls();ImGui.PopItemWidth();ImGui.End();
        UpdatePanel();
    }
    private void HomeNavigation(float margin,float width)
    {
        bool wrap=width<1150;
        HomeCard("Home navigation",new(margin,16),new(width,wrap?92:56),false);
        HomeLabel("DIGI-TACTICS",23,HomeInk);
        if(wrap)ImGui.SetCursorScreenPos(new(margin+8,61));else ImGui.SameLine(205);
        if(HomeButton("homeOverview","Home",70,32))homeTab="overview";ImGui.SameLine();
        if(profile is not null)
        {
            if(HomeButton("collection","Collection",105,32))showCollection=true;ImGui.SameLine();
            if(farm is not null){if(HomeButton("farm","Digi-Farm",100,32)){farm.Refresh(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());showFarm=true;}ImGui.SameLine();}
            if(HomeButton("profile","Profile",80,32))OpenProfile();ImGui.SameLine();
        }
        if(HomeButton("multiplayer","Steam multiplayer",162,32))OpenMultiplayerView();
        ImGui.SameLine();if(HomeButton("about","About",70,32))showAbout=true;
        ImGui.SameLine();if(HomeButton("codex","Codex",70,32))OpenCodexFull();
        ImGui.SameLine();if(HomeButton("settings","Settings",80,32))showSettings=true;
        SocialNavigation();
        if(profile is not null)
        {
            string balance=$"{profile.Progress.Bits??0:N0} Bits";float textWidth=ImGui.CalcTextSize(balance).X;
            ImGui.SetCursorScreenPos(new(margin+width-textWidth-18,35));HomeLabel(balance,15,HomeGold);
        }
        ImGui.End();
    }
    private void HomePartner(float2 at,float2 extent)
    {
        HomeCard("Home Little Legend",at,extent,false);HomeLabel("YOUR LITTLE LEGEND",12);
        try
        {
            string? wallet=profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"),legends=profile?.Storage.GetValueOrDefault("dt.legendevo.v1");
            var collection=new CosmeticCollection(wallet);string id=collection.Equipped("legend")?.GetValue<string>()??"tokomon";
            var view=LegendEvolution.Inspect(legends,wallet,profile?.Storage.GetValueOrDefault("dt.farm.v1"),id);
            HomeLabel(LegendEvolution.Form(view.Form).Name,21,HomeInk);
            homeLegend??=new(legendPedestal);
            float imageHeight=Math.Min(178,extent.Y-153),imageWidth=imageHeight*CosmeticScenePreview.Width/CosmeticScenePreview.Height;
            var imageAt=at+new float2((extent.X-imageWidth)/2,54);
            Buttons["homeLegend"]=imageAt+new float2(imageWidth/2,imageHeight/2);
            Buttons["homeLegend/outside"]=imageAt+new float2(-12,imageHeight/2);
            if(homeLegend.PointerItem("homeLegend",imageAt,new(imageWidth,imageHeight)) is not null)audio.PlayClient("emote",true);
            homeLegend.Draw("legend",id,LegendEvolution.VisualKey(id,view.Form),boardSceneTime,ClientMotion);
            ImGui.GetWindowDrawList().AddImage(new ImTextureRef{_TexID=new((nint)homeLegend.Texture)},imageAt,imageAt+new float2(imageWidth,imageHeight),new(0,1),new(1,0));
            ImGui.SetCursorScreenPos(at+new float2(14,extent.Y-94));HomeLabel($"Level {view.Level}  /  "+(view.Need>0?$"{view.Data}/{view.Need} Digi-Data":"Maximum level"),13,HomeMute);
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram,new ImColor8(49,185,202));ImGui.ProgressBar(view.Need>0?(float)view.Data/view.Need:1,new(extent.X-28,5),""u8);ImGui.PopStyleColor();
        }
        catch(Exception error) when(error is JsonException or InvalidDataException or InvalidOperationException or KeyNotFoundException or FormatException or OverflowException)
        {UiText.Wrapped("Your Little Legend could not be displayed: "+error.Message);}
        ImGui.SetCursorScreenPos(at+new float2(14,extent.Y-56));
        bool inRoom=networkRoom is not null || steam?.Room is not null;
        ImGui.BeginDisabled(inRoom);
        if(HomeButton("start",duos?"PLAY DUOS":"PLAY SOLO",extent.X-28,42,true))StartLocalMatch(duos);
        ImGui.EndDisabled();
        if(inRoom && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))ResultTooltip("Leave your Steam room before starting a solo game.");
        ImGui.End();
    }
    private void HomeProgress(float2 at,float2 extent)
    {
        HomeProgressBounds=(at,at+extent);
        HomeCard("Home progress",at,extent);HomeLabel("TAMER & DAILY MISSIONS");
        if(profile is not null){ImGui.PushTextWrapPos(0);ProfileControls();ImGui.PopTextWrapPos();}else {HomeLabel("Build your team. Digivolve.",17,HomeInk);ImGui.Separator();}
        try
        {
            HomeMissions(HomeDaily);HomePatchPreview();
            if(farm?.Error is {} farmError)UiText.Wrapped(farmError);
        }
        catch(Exception error) when(error is JsonException or InvalidDataException or InvalidOperationException or KeyNotFoundException or FormatException or OverflowException)
        {UiText.Wrapped("Daily progress unavailable: "+error.Message);}
        ImGui.End();
    }
    private void HomeFeatured(float2 at,float2 extent)
    {
        HomeStoreBounds=(at,at+extent);
        HomeCard("Home store",at,extent);var artAt=ImGui.GetCursorScreenPos();float imageWidth=ImGui.GetContentRegionAvail().X;
        homeArtwork.Draw("store-hero",artAt,new(imageWidth,56),255,true);ImGui.Dummy(new(imageWidth,62));HomeLabel("FEATURED TODAY");HomeLabel("25% off / new in "+HomeCountdown,12,HomeMute);
        RefreshHomeOffers();
        try
        {
            var collection=new CosmeticCollection(profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            foreach(var offer in homeOffers)HomeOfferRow(offer.Kind,offer.Item,offer.Price,collection,imageWidth);
        }
        catch(Exception error) when(error is JsonException or InvalidDataException or InvalidOperationException or KeyNotFoundException or FormatException or OverflowException)
        {UiText.Wrapped("Featured items unavailable: "+error.Message);}
        if(profile is not null && HomeButton("homeStore","Open Store",imageWidth,28))OpenHomeStore();
        HomeSetNews();
        ImGui.End();
    }
    private void DisposeHome(){ClearHomeOfferPreviews();homeLegend?.Dispose();homeLegend=null;homeArtwork.Dispose();}
    private void HomeAbout()
    {
        float width=Math.Min(720,size.X-48),height=Math.Min(430,size.Y-48);
        HomeCard("About Digi-Tactics",new((size.X-width)/2,(size.Y-height)/2),new(width,height));
        if(HomeButton("closeAbout","Back to Home",145,32)||ImGui.IsKeyPressed(ImGuiKey.Escape))showAbout=false;
        ImGui.Spacing();HomeLabel("DIGI-TACTICS",28,HomeInk);HomeLabel("Native edition powered by BRUTAL",16,HomeMute);ImGui.Separator();
        UiText.Wrapped("Audio decoding uses FFmpeg under LGPL 2.1 or later. Source and license details are included in media/MEDIA_RUNTIME_NOTICES.txt."u8);
        ImGui.Spacing();
        foreach(var document in new[]{("CREDITS.md","Credits"),("THIRD_PARTY_NOTICES.txt","Third-party notices"),("media/MEDIA_RUNTIME_NOTICES.txt","Audio runtime notices")})
        {
            string path=Path.Combine(AppContext.BaseDirectory,document.Item1);
            if(File.Exists(path)&&HomeButton("aboutDocument/"+document.Item1,document.Item2,250,30))
            {
                try{System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path){UseShellExecute=true});homeDocumentError=null;}
                catch(Exception error) when(error is System.ComponentModel.Win32Exception or IOException or InvalidOperationException){homeDocumentError="The document could not be opened: "+error.Message;}
            }
        }
        if(homeDocumentError is not null)UiText.Wrapped(homeDocumentError);
        ImGui.End();
    }
}

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
        ImGui.PushFont(font>=20?NativeUiTheme.DisplayBoldFont:NativeUiTheme.BodyFont,Math.Max(15,font));ImGui.PushStyleColor(ImGuiCol.Text,color??HomeGold);ImGui.Text(value);ImGui.PopStyleColor();ImGui.PopFont();
    }
    private bool HomeButton(string key,string label,float width=0,float height=36,bool primary=false)
    {
        ImGui.PushStyleColor(ImGuiCol.Button,primary?new ImColor8(17,92,113):new ImColor8(17,29,42));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered,primary?new ImColor8(28,123,145):new ImColor8(35,52,68));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive,new ImColor8(29,108,124));ImGui.PushStyleColor(ImGuiCol.Border,HomeGold);ImGui.PushStyleColor(ImGuiCol.Text,HomeInk);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,1);ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,2);
        bool result=Button(key,label,width,Math.Max(36,height));
        if(primary && key is "start" or "clientPlay" or "homePlayStart" or "steamStart")
        {
            var lo=ImGui.GetItemRectMin();var hi=ImGui.GetItemRectMax();var draw=ImGui.GetWindowDrawList();
            var top=NativeUiTheme.PlayTop;var bottom=NativeUiTheme.Play;
            if(ImGui.IsItemHovered()){top=new(255,226,159);bottom=NativeUiTheme.PlayMiddle;}
            ImColor8 Styled(ImColor8 color)
            {
                var rgba=color.AsByte4();
                return new(rgba.X,rgba.Y,rgba.Z,(byte)Math.Clamp(Math.Round(rgba.W*ImGui.GetStyle().Alpha),0,255));
            }
            draw.AddRectFilledMultiColor(lo,hi,Styled(top),Styled(top),Styled(bottom),Styled(bottom));
            draw.AddRect(lo,hi,Styled(NativeUiTheme.PlayBorder),2);
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,20);
            var textSize=ImGui.CalcTextSize(label);draw.AddText(lo+(hi-lo-textSize)*.5f,Styled(NativeUiTheme.PlayText),label);ImGui.PopFont();
            if(ImGui.IsItemFocused())draw.AddRect(lo+new float2(3,3),hi-new float2(3,3),Styled(HomeInk),1);
        }
        ImGui.PopStyleVar(2);ImGui.PopStyleColor(5);return result;
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
        float margin=16,gap=12,width=size.X-margin*2,right=280,heroWidth=width-right-gap;
        float tabsTop=ClientPageTop,top=tabsTop+48,available=size.Y-top-margin-(update?100:0);
        float heroHeight=Math.Min(Math.Max(350,available*.62f),available-92),bottom=top+heroHeight+gap,lowerHeight=available-heroHeight-gap;
        HomeLayoutFits=lowerHeight>=80 && margin+width<=size.X && bottom+lowerHeight<=size.Y;
        var background=ImGui.GetBackgroundDrawList();
        homeArtwork.Draw("hero-datastream-sky",new(0,0),new(size.X,size.Y),70,true,drawList:background);
        background.AddRectFilled(new(0,0),new(size.X,size.Y),new ImColor8(1,10,19,205));
        if(showAbout){HomeAbout();UpdatePanel();return;}
        HomeHighlights=HomeRotationSets=HomeFeaturedCount=HomeFeaturedOwned=0;
        HomeTabs(margin,tabsTop,width);
        if(homeTab=="patch")HomePatchNotes(new(margin,top),new(heroWidth,heroHeight+gap+lowerHeight));
        else if(homeTab=="events")HomeEventsPanel(new(margin,top),new(heroWidth,heroHeight+gap+lowerHeight));
        else
        {
            HomeHero(new(margin,top),new(heroWidth,heroHeight));
            float tileWidth=(width-gap*3)/4;
            HomeProgress(new(margin,bottom),new(tileWidth,lowerHeight));
            HomePatchBounds=(new(margin+tileWidth+gap,bottom),new(margin+tileWidth*2+gap,bottom+lowerHeight));
            HomeCard("Home patch preview",HomePatchBounds.Min,new(tileWidth,lowerHeight));
            HomePatchPreview();ImGui.End();
            HomeFeatured(new(margin+(tileWidth+gap)*2,bottom),new(tileWidth,lowerHeight));
            HomeCard("Home farm preview",new(margin+(tileWidth+gap)*3,bottom),new(tileWidth,lowerHeight));
            var artAt=ImGui.GetCursorScreenPos();float artWidth=ImGui.GetContentRegionAvail().X;
            homeArtwork.Draw("hero-file-island",artAt,new(artWidth,62),255,true);ImGui.Dummy(new(artWidth,70));
            HomeLabel("DIGI-FARM",22,HomeInk);UiText.Wrapped("Care for your Little Legends. Feed, train, and help them grow."u8);
            if(farm is not null && HomeButton("homeFarm","Visit your farm",artWidth,36)){farm.Refresh(HomeClock().ToUnixTimeMilliseconds());showFarm=true;}
            ImGui.End();
        }
        HomePartner(new(margin+heroWidth+gap,top),new(right,heroHeight));

        UpdatePanel();
    }
    private void HomeProgress(float2 at,float2 extent)
    {
        HomeProgressBounds=(at,at+extent);
        HomeCard("Home progress",at,extent);HomeLabel("TAMER & DAILY MISSIONS");
        if(profile is not null){ImGui.PushTextWrapPos(0);ProfileControls();ImGui.PopTextWrapPos();}else {HomeLabel("Build your team. Digivolve.",17,HomeInk);ImGui.Separator();}
        try
        {
            HomeMissions(HomeDaily);
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

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
        bool angled=primary&&key is "start" or "clientPlay" or "homePlayStart" or "homePlayConfirm" or "steamCreate" or "steamStart";
        ImGui.PushStyleColor(ImGuiCol.Button,angled?new ImColor8(0,0,0,0):primary?new ImColor8(17,92,113):new ImColor8(17,29,42));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered,angled?new ImColor8(0,0,0,0):primary?new ImColor8(28,123,145):new ImColor8(35,52,68));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive,angled?new ImColor8(0,0,0,0):new ImColor8(29,108,124));ImGui.PushStyleColor(ImGuiCol.Border,angled?new ImColor8(0,0,0,0):HomeGold);ImGui.PushStyleColor(ImGuiCol.Text,HomeInk);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,1);ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,0);
        // Custom artwork supplies its own label. Hiding the stock text also
        // prevents a second label showing through translucent disabled frames.
        if(angled||key=="homeHeroAction")ImGui.PushStyleColor(ImGuiCol.Text,new ImColor8(0,0,0,0));
        bool result=Button(key,label,width,Math.Max(36,height));
        if(angled||key=="homeHeroAction")ImGui.PopStyleColor();
        if(primary && key is "start" or "clientPlay" or "homePlayStart" or "homePlayConfirm" or "steamCreate" or "steamStart")
        {
            var lo=ImGui.GetItemRectMin();var hi=ImGui.GetItemRectMax();var draw=ImGui.GetWindowDrawList();bool nav=key=="clientPlay",cyan=nav||key=="homePlayConfirm";
            ImColor8 Styled(ImColor8 color)
            {
                var rgba=color.AsByte4();return new(rgba.X,rgba.Y,rgba.Z,(byte)Math.Clamp(Math.Round(rgba.W*ImGui.GetStyle().Alpha),0,255));
            }
            float cut=nav?20:10;
            float2[] polygon=nav?[lo,new(hi.X-cut,lo.Y),new(hi.X,(lo.Y+hi.Y)/2),new(hi.X-cut,hi.Y),new(lo.X,hi.Y)]:[new(lo.X+cut,lo.Y),new(hi.X,lo.Y),new(hi.X,hi.Y-cut),new(hi.X-cut,hi.Y),new(lo.X,hi.Y),new(lo.X,lo.Y+cut)];
            foreach(var point in polygon)draw.PathLineTo(point);draw.PathFillConcave(Styled(cyan?HomeGold:NativeUiTheme.Play));
            var innerLo=lo+new float2(2,2);var innerHi=hi-new float2(2,2);
            float2[] inner=nav?[innerLo,new(innerHi.X-cut,innerLo.Y),new(innerHi.X,(innerLo.Y+innerHi.Y)/2),new(innerHi.X-cut,innerHi.Y),new(innerLo.X,innerHi.Y)]:[new(innerLo.X+cut,innerLo.Y),new(innerHi.X,innerLo.Y),new(innerHi.X,innerHi.Y-cut),new(innerHi.X-cut,innerHi.Y),new(innerLo.X,innerHi.Y),new(innerLo.X,innerLo.Y+cut)];
            foreach(var point in inner)draw.PathLineTo(point);draw.PathFillConcave(Styled(cyan?new ImColor8(9,50,67):NativeUiTheme.PlayMiddle));
            if(nav)homeArtwork.Draw("client-mark",lo+new float2(10,(hi.Y-lo.Y-36)/2),new(36,36),(byte)Math.Clamp(Math.Round(255*ImGui.GetStyle().Alpha),0,255));
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,nav?24:20);var textSize=ImGui.CalcTextSize(label);
            draw.AddText(lo+(hi-lo-textSize)*.5f+new float2(nav?8:0,0),Styled(cyan?new ImColor8(190,242,255):NativeUiTheme.PlayText),label);ImGui.PopFont();
            if(ImGui.IsItemHovered())draw.AddLine(innerLo+new float2(cut,0),new(innerHi.X-cut,innerLo.Y),Styled(HomeInk),2);
            if(ImGui.IsItemFocused())draw.AddRect(lo+new float2(4,4),hi-new float2(4,4),Styled(HomeInk),0);
        }
        else if(key=="homeHeroAction")
        {
            var lo=ImGui.GetItemRectMin();var hi=ImGui.GetItemRectMax();var draw=ImGui.GetWindowDrawList();
            byte alpha=(byte)Math.Clamp(Math.Round(255*ImGui.GetStyle().Alpha),0,255);
            draw.AddRectFilledMultiColor(lo,hi,new ImColor8(15,74,92,alpha),new ImColor8(15,74,92,alpha),new ImColor8(4,22,36,alpha),new ImColor8(4,22,36,alpha));
            draw.AddRect(lo,hi,new ImColor8(200,170,110,alpha));draw.AddRect(lo+new float2(2,2),hi-new float2(2,2),new ImColor8(63,214,255,(byte)(alpha/3)));
            ImGui.PushFont(NativeUiTheme.DisplayBoldFont,18);draw.AddText(lo+(hi-lo-ImGui.CalcTextSize(label))*.5f,new ImColor8(190,242,255,alpha),label);ImGui.PopFont();
        }
        ImGui.PopStyleVar(2);ImGui.PopStyleColor(5);return result;
    }
    private void HomeCard(string id,float2 at,float2 extent,bool scroll=true,bool bringToFront=false)
    {
        ImGui.PushStyleColor(ImGuiCol.WindowBg,new ImColor8(5,15,25,250));ImGui.PushStyleColor(ImGuiCol.Border,HomeLine);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding,0);ImGui.SetNextWindowBgAlpha(1);
        Window(id,at.X,at.Y,extent.X,extent.Y,ImGuiWindowFlags.NoTitleBar|(bringToFront?ImGuiWindowFlags.None:ImGuiWindowFlags.NoBringToFrontOnFocus)|(scroll?ImGuiWindowFlags.None:ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse));
        ImGui.PopStyleVar();ImGui.PopStyleColor(2);
    }
    private void HomePanel()
    {
        if(DrawPostgame())return;
        RefreshSetSelection();
        UpdateClient?.Refresh();
        float margin=socialWindowSize.X<=1500?16:24,gap=socialWindowSize.X<=1380?8:12,width=size.X-margin*2,right=Math.Clamp(socialWindowSize.X*.21f,240,320),heroWidth=width-right-gap;
        float tabsTop=ClientPageTop+10,top=tabsTop+49,available=size.Y-top-16;
        float lowerHeight=Math.Clamp(size.Y*.25f,176,250),heroHeight=Math.Max(300,available-lowerHeight-gap);
        if(heroHeight+lowerHeight+gap>available)lowerHeight=Math.Max(130,available-heroHeight-gap);
        float bottom=top+heroHeight+gap;
        HomeLayoutFits=lowerHeight>=130&&margin+width<=size.X&&bottom+lowerHeight<=size.Y;
        if(showAbout){HomeAbout();return;}
        HomeHighlights=HomeRotationSets=HomeFeaturedCount=HomeFeaturedOwned=0;
        HomeTabs(margin,tabsTop,width);
        if(homeTab=="patch")HomePatchNotes(new(margin,top),new(heroWidth,heroHeight+gap+lowerHeight));
        else if(homeTab=="events")HomeEventsPanel(new(margin,top),new(heroWidth,heroHeight+gap+lowerHeight));
        else
        {
            HomeHero(new(margin,top),new(heroWidth,heroHeight));
            float tileWidth=(width-gap*3)/4;
            HomeCard("Home set news",new(margin,bottom),new(tileWidth,lowerHeight));
            var set=HomeSets.Value[selectedSet];var artAt=ImGui.GetCursorScreenPos();float artWidth=ImGui.GetContentRegionAvail().X;
            homeArtwork.Draw(set.Hero,artAt,new(artWidth,54),255,true);ImGui.Dummy(new(artWidth,58));HomeSetNews();ImGui.End();
            HomeFeatured(new(margin+tileWidth+gap,bottom),new(tileWidth,lowerHeight));
            HomeProgress(new(margin+(tileWidth+gap)*2,bottom),new(tileWidth,lowerHeight));
            HomePatchBounds=(new(margin+(tileWidth+gap)*3,bottom),new(margin+width,bottom+lowerHeight));
            HomeCard("Home patch preview",HomePatchBounds.Min,new(tileWidth,lowerHeight));
            artAt=ImGui.GetCursorScreenPos();artWidth=ImGui.GetContentRegionAvail().X;
            homeArtwork.Draw("hero-digivice-energy",artAt,new(artWidth,54),255,true);ImGui.Dummy(new(artWidth,58));
            HomePatchPreview();ImGui.End();
        }
        HomePartner(new(margin+heroWidth+gap,top),new(right,heroHeight));
    }
    private void HomeProgress(float2 at,float2 extent)
    {
        HomeProgressBounds=(at,at+extent);
        HomeCard("Home progress",at,extent);HomeLabel("MISSIONS",15,HomeInk);
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
        HomeCard("Home store",at,extent);float imageWidth=ImGui.GetContentRegionAvail().X;
        HomeLabel("FEATURED",15,HomeInk);
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
        if(HomeButton("closeAbout","Back",145,32)||ImGui.IsKeyPressed(ImGuiKey.Escape))BackClient();
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

using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private sealed record HomeSet(string Name,string Description,string Mechanic,string MechanicDescription,string Hero,string Tag,string? Render=null);
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
    // .home: 10px 24px 16px (16px sides at 1500 px and below, 8/10px vertical at 820 px tall), max-width 1720;
    // .home-top: 1fr | clamp(240px, 21vw, 320px); .home-tiles: clamp(176px, 25vh, 250px), 168px when short.
    private void HomePanel()
    {
        if(DrawPostgame())return;
        RefreshSetSelection();
        UpdateClient?.Refresh();
        bool shortWindow=socialWindowSize.Y<=820;
        float margin=socialWindowSize.X<=1500?16:24,gap=socialWindowSize.X<=1380?8:12;
        float width=Math.Min(size.X-margin*2,1720),left=MathF.Round((size.X-width)/2);
        float right=Math.Clamp(socialWindowSize.X*.21f,240,320),heroWidth=width-right-gap;
        float tabsTop=ClientPageTop+(shortWindow?8:10),tabsHeight=HomeTabsHeight,top=tabsTop+tabsHeight+(shortWindow?6:10),available=size.Y-top-(shortWindow?10:16);
        float lowerHeight=shortWindow?168:Math.Clamp(socialWindowSize.Y*.25f,176,250),heroHeight=Math.Max(280,available-lowerHeight-gap);
        if(heroHeight+lowerHeight+gap>available)lowerHeight=Math.Max(130,available-heroHeight-gap);
        // Chromium snaps box edges to whole pixels.
        top=MathF.Round(top);heroHeight=MathF.Round(heroHeight);lowerHeight=MathF.Round(lowerHeight);right=MathF.Round(right);heroWidth=width-right-gap;
        float bottom=top+heroHeight+gap;
        HomeLayoutFits=lowerHeight>=130&&left+width<=size.X&&bottom+lowerHeight<=size.Y;
        if(showAbout){HomeAbout();return;}
        HomeHighlights=HomeRotationSets=HomeFeaturedCount=HomeFeaturedOwned=0;
        HomeTabs(left,tabsTop,width);
        if(homeTab=="patch")HomePatchNotes(new(left,top),new(heroWidth,heroHeight+gap+lowerHeight));
        else if(homeTab=="events")HomeEventsPanel(new(left,top),new(heroWidth,heroHeight+gap+lowerHeight));
        else
        {
            HomeHero(new(left,top),new(heroWidth,heroHeight));
            float tileWidth=(width-gap*3)/4;
            float TileLeft(int i)=>MathF.Round(left+i*(tileWidth+gap));
            float2 Tile(int i)=>new(TileLeft(i),bottom);float2 TileSize(int i)=>new(MathF.Round(left+i*(tileWidth+gap)+tileWidth)-TileLeft(i),lowerHeight);
            HomeSetNews(Tile(0),TileSize(0));
            HomeFeatured(Tile(1),TileSize(1));
            HomeProgress(Tile(2),TileSize(2));
            HomePatchPreview(Tile(3),TileSize(3));
        }
        HomePartner(new(left+heroWidth+gap,top),new(right,heroHeight));
    }
    // .tile-card padding (8px 10px at 1380 px and below) inside its 1px border, and .tile-art height (48px; 40px when short).
    private float2 HomeTilePadding=>socialWindowSize.X<=1380?new(11,9):new(13,11);
    private float HomeTileArtHeight=>socialWindowSize.Y<=820?40:socialWindowSize.X<=1380?48:62;
    private void HomeProgress(float2 at,float2 extent)
    {
        HomeProgressBounds=(at,at+extent);
        ClientLayer("Home progress",at,extent);var draw=ImGui.GetWindowDrawList();
        bool hovered=ImGui.IsMouseHoveringRect(at,at+extent)&&ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        PaintTile(draw,at,at+extent,hovered);
        var pad=HomeTilePadding;float inner=extent.X-pad.X*2;
        TileHeading(draw,at+pad,inner,"Missions","Reset at midnight");
        float y=at.Y+pad.Y+NativeUiTheme.TextLine(12.5f)+6;
        draw.PushClipRect(at+new float2(pad.X,0),at+extent-new float2(pad.X,pad.Y),true);
        try
        {
            HomeMissions(HomeDaily,new(at.X+pad.X,y),inner);
            if(farm?.Error is {} farmError)CssParagraph(draw,NativeUiTheme.BodyFont,11,new(at.X+pad.X,at.Y+extent.Y-pad.Y-16),inner,NativeUiTheme.Danger,farmError,1.4f,1);
        }
        catch(Exception error) when(error is JsonException or InvalidDataException or InvalidOperationException or KeyNotFoundException or FormatException or OverflowException)
        {CssParagraph(draw,NativeUiTheme.BodyFont,12.5f,new(at.X+pad.X,y),inner,NativeUiTheme.Muted,"Daily progress unavailable: "+error.Message);}
        draw.PopClipRect();
        ImGui.End();
    }
    private void HomeFeatured(float2 at,float2 extent)
    {
        HomeStoreBounds=(at,at+extent);
        ClientLayer("Home store",at,extent);var draw=ImGui.GetWindowDrawList();
        bool hovered=ImGui.IsMouseHoveringRect(at,at+extent)&&ImGui.IsWindowHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        PaintTile(draw,at,at+extent,hovered);
        var pad=HomeTilePadding;float inner=extent.X-pad.X*2;
        TileHeading(draw,at+pad,inner,"Featured","Today only");
        RefreshHomeOffers();
        float y=at.Y+pad.Y+NativeUiTheme.TextLine(12.5f)+6;
        draw.PushClipRect(at+new float2(pad.X,0),at+extent-new float2(pad.X,pad.Y),true);
        try
        {
            var collection=new CosmeticCollection(profile?.Storage.GetValueOrDefault("dt.cosmetics.v1"));
            // renderFeatured shows the first three of the day's featured row.
            foreach(var offer in homeOffers.Take(3)){HomeOfferRow(offer.Kind,offer.Item,offer.Price,collection,new(at.X+pad.X,y),inner);y+=HomeOfferHeight+6;}
        }
        catch(Exception error) when(error is JsonException or InvalidDataException or InvalidOperationException or KeyNotFoundException or FormatException or OverflowException)
        {CssParagraph(draw,NativeUiTheme.BodyFont,12.5f,new(at.X+pad.X,y),inner,NativeUiTheme.Muted,"Featured items unavailable: "+error.Message);}
        draw.PopClipRect();
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

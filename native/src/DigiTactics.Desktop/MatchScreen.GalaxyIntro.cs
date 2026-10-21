using System.Globalization;
using System.Text.Json;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal sealed record GalaxyAnnouncement(string Title,string Name,string Description,string Color);
    internal static readonly Lazy<Dictionary<string,GalaxyAnnouncement>> GalaxyAnnouncements=new(()=>JsonSerializer.Deserialize<Dictionary<string,GalaxyAnnouncement>>(
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"content/galaxy-intros/catalog.json")),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!);
    private readonly PostgameArtwork galaxyIntroArtwork=new("galaxy-intros");
    private MatchState? galaxyIntroMatch;
    private bool galaxyIntroSeen;
    internal string? GalaxyIntroText {get;private set;}
    internal long GalaxyIntroSerial {get;private set;}
    internal bool GalaxyIntroBodyFits {get;private set;}
    internal (float2 Min,float2 Max) GalaxyIntroBounds {get;private set;}
    private GalaxyAnnouncement? CurrentGalaxyIntro()
    {
        if(Session is null)return null;
        if(!ReferenceEquals(galaxyIntroMatch,Session.State)){galaxyIntroMatch=Session.State;galaxyIntroSeen=false;}
        if(galaxyIntroSeen||Session.Rounds.Position.Stage>1||Session.State.Galaxy is not {} id||!Rules.Id.StartsWith("galaxies/",StringComparison.Ordinal))return null;
        return GalaxyAnnouncements.Value.GetValueOrDefault(id);
    }
    private static float GalaxyIntroCardHeight(GalaxyAnnouncement intro,float width)=>
        Math.Max(100,ChoiceLines(intro.Title,14,width-224).Length*19+ChoiceLines(intro.Name,22,width-224).Length*27+ChoiceLines(intro.Description,16,width-224).Length*21+24);
    private void GalaxyIntroCard(GalaxyAnnouncement intro)
    {
        var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X,height=GalaxyIntroCardHeight(intro,width);
        uint rgb=uint.Parse(intro.Color.AsSpan(1),NumberStyles.HexNumber,CultureInfo.InvariantCulture);
        var color=new ImColor8((byte)(rgb>>16),(byte)(rgb>>8),(byte)rgb);
        ImGui.Dummy(new(width,height));
        var draw=ImGui.GetWindowDrawList();
        draw.AddRectFilled(at,at+new float2(width,height),new ImColor8(255,255,255,10),10);
        draw.AddRect(at,at+new float2(width,height),color,10,thickness:2);
        galaxyIntroArtwork.Draw(Session!.State.Galaxy!,at+new float2(12,(height-72)/2),new(72,72));
        var textAt=at+new float2(96,12);float bodyWidth=width-224;
        ChoiceText(draw,textAt,bodyWidth,intro.Title.ToUpperInvariant(),14,19,HudMuted,false);
        textAt.Y+=ChoiceLines(intro.Title,14,bodyWidth).Length*19;
        ChoiceText(draw,textAt,bodyWidth,intro.Name,22,27,color,false);
        textAt.Y+=ChoiceLines(intro.Name,22,bodyWidth).Length*27;
        ChoiceText(draw,textAt,bodyWidth,intro.Description,16,21,HudInk,false);
        GalaxyIntroBodyFits=textAt.Y+ChoiceLines(intro.Description,16,bodyWidth).Length*21<=at.Y+height-12;
        ImGui.SetCursorScreenPos(new(at.X+width-104,at.Y+(height-38)/2));
        if(HudButton("galaxyIntroDismiss","Got it",88,38))galaxyIntroSeen=true;
        ImGui.SetCursorScreenPos(at+new float2(0,height+12));
        ImGui.Dummy(new(0,0));
        GalaxyIntroText=intro.Title+"\n"+intro.Name+"\n"+intro.Description;GalaxyIntroSerial++;
        GalaxyIntroBounds=(at,at+new float2(width,height));
    }
    private void StandaloneGalaxyIntro(GalaxyAnnouncement intro)
    {
        float width=Math.Min(860,size.X-48),height=GalaxyIntroCardHeight(intro,width-36)+32;
        BeginChoicePanel("Galaxy announcement",width,height);
        GalaxyIntroCard(intro);EndChoicePanel();
    }
}

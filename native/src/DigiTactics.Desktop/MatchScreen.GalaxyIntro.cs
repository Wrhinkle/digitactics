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
    private void GalaxyIntroCard(GalaxyAnnouncement intro)
    {
        var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X;
        uint rgb=uint.Parse(intro.Color.AsSpan(1),NumberStyles.HexNumber,CultureInfo.InvariantCulture);
        var color=new ImColor8((byte)(rgb>>16),(byte)(rgb>>8),(byte)rgb);
        ImGui.PushStyleColor(ImGuiCol.Border,color);
        ImGui.BeginChild("This game's Galaxy"u8,new(0,126),ImGuiChildFlags.Borders,ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        var image=ImGui.GetCursorScreenPos();galaxyIntroArtwork.Draw(Session!.State.Galaxy!,image,new(96,96));
        ImGui.Dummy(new(96,96));ImGui.SameLine(0,12);
        ImGui.BeginChild("Galaxy rule description"u8,new(Math.Max(180,width-244),106),ImGuiChildFlags.None,ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        HudText(intro.Title,15,HudMuted);HudText(intro.Name,22,color);HudText(intro.Description,16);
        GalaxyIntroBodyFits=ImGui.GetScrollMaxY()<1;
        ImGui.EndChild();
        ImGui.SetCursorScreenPos(new(at.X+width-104,at.Y+44));
        if(HudButton("galaxyIntroDismiss","Got it",88,38))galaxyIntroSeen=true;
        ImGui.EndChild();ImGui.PopStyleColor();
        GalaxyIntroText=intro.Title+"\n"+intro.Name+"\n"+intro.Description;GalaxyIntroSerial++;
        GalaxyIntroBounds=(at,at+new float2(width,126));
    }
    private void StandaloneGalaxyIntro(GalaxyAnnouncement intro)
    {
        float width=Math.Min(920,size.X-48);
        Window("Galaxy announcement",(size.X-width)/2,Top+12,width,166,ImGuiWindowFlags.NoTitleBar);
        GalaxyIntroCard(intro);ImGui.End();
    }
}

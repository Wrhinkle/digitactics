using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal int CarouselCardsDrawn { get; private set; }
    internal (float2 Min,float2 Max) CarouselChoicesBounds { get; private set; }

    private void CarouselChoices()
    {
        var carousel=Session!.Units.Carousel;
        var intro=CurrentGalaxyIntro();
        bool myTurn=carousel.Turn==Human;
        float panelWidth=Math.Min(920,size.X-48),cardHeight=216;
        int columns=Math.Max(1,(int)((panelWidth-40)/164));
        int rows=(carousel.Options.Count+columns-1)/columns;
        float panelHeight=Math.Min(size.Y-48,180+rows*(cardHeight+8)+(intro is null?0:134));
        ImGui.PushStyleColor(ImGuiCol.WindowBg,new ImColor8(5,7,12,210));
        Window("Carousel",0,0,size.X,size.Y,ImGuiWindowFlags.NoTitleBar|ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleColor();
        ImGui.SetCursorScreenPos(new((size.X-panelWidth)/2,(size.Y-panelHeight)/2));
        ImGui.PushStyleColor(ImGuiCol.ChildBg,new ImColor8(15,22,37));
        ImGui.PushStyleColor(ImGuiCol.Border,HudLine);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding,12);ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(16,16));
        ImGui.BeginChild("Carousel panel"u8,new(panelWidth,panelHeight),ImGuiChildFlags.Borders,ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleVar(2);ImGui.PopStyleColor(2);
        if(intro is not null)GalaxyIntroCard(intro);
        HudText(Session.Rounds.Position.Stage==1?"OPENING CAROUSEL":"CAROUSEL",22,HudGold);
        HudText(myTurn?"Your pick: choose one Digimon and its item.":carousel.Done?"Everyone has picked. Your team is ready.":$"Waiting for {carousel.Turn!.Name}...",16,myTurn?HudGold:HudInk);
        HudText(Session.Rounds.Position.Stage==1?"Pick order is random.":"Lowest HP picks first.",16,HudMuted);
        ImGui.Separator();
        ImGui.BeginChild("Carousel choices"u8,new(0,ImGui.GetContentRegionAvail().Y-50));
        CarouselChoicesBounds=(ImGui.GetWindowDrawList().GetClipRectMin(),ImGui.GetWindowDrawList().GetClipRectMax());
        float width=(ImGui.GetContentRegionAvail().X-(columns-1)*8)/columns;
        for(int i=0;i<carousel.Options.Count;i++)
        {
            if(i%columns!=0)ImGui.SameLine(0,8);
            var option=carousel.Options[i];var line=Rules.Lines.Single(l=>l.Id==option.Line);
            bool taken=option.PickedBy is not null;
            ImGui.BeginDisabled(taken||!myTurn);
            bool pick=HudButton("carousel/"+i,"",width,cardHeight);
            var min=ImGui.GetItemRectMin();var max=ImGui.GetItemRectMax();
            bool hovered=ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled);
            var draw=ImGui.GetWindowDrawList();var ink=taken?HudMuted:HudInk;
            draw.PushClipRect(min,max,true);
            draw.AddRectFilled(min,max,taken?new ImColor8(16,22,32):hovered?new ImColor8(35,48,69):HudSurface,8);
            draw.AddRect(min+new float2(1,1),max-new float2(1,1),tierColors[Math.Clamp(line.Tier,0,5)],8,thickness:2);
            portraits.Draw(Rules.Id,option.Line,0,new(min.X+(width-96)/2,min.Y+10),96,taken?(byte)100:(byte)255);
            if(option.Item is { } item)itemArtwork.Draw(Rules.Id,item,new(min.X+(width+52)/2,min.Y+79),34);
            HudClipText(draw,min+new float2(9,119),width-18,line.Forms[0],ink,18);
            HudClipText(draw,min+new float2(9,142),width-18,$"{line.Tier}-cost / {string.Join(", ",line.Traits.Select(Name))}",HudMuted);
            HudClipText(draw,min+new float2(9,163),width-18,option.Item is { } itemId?Name(itemId):"No item",ink);
            HudClipText(draw,min+new float2(9,188),width-18,taken?"Taken by "+option.PickedBy:myTurn?"Click to choose":"Waiting",taken||myTurn?HudGold:HudMuted);
            draw.PopClipRect();ImGui.EndDisabled();CarouselCardsDrawn++;
            if(hovered)
            {
                ImGui.SetNextWindowSize(new(380,0));ImGui.BeginTooltip();UnitDetails(option.Line,0);
                if(option.Item is { } tooltipItem){var description=text.Get(Rules.Id,tooltipItem);ImGui.Separator();HudText(description.Name,18);UiText.Wrapped(description.Description);}
                if(taken)HudText("Taken by "+option.PickedBy,16,HudGold);
                ImGui.EndTooltip();
            }
            if(pick){Act("carousel",new{i});break;}
        }
        ImGui.EndChild();
        if(carousel.Done)
        {
            if(networkRoom is not null)HudText("The next round is starting...",16,HudGold);
            else if(HudButton("carouselGo",$"Start round {Rules.Schedule!.Next(Session.Rounds.Position).Label}",260,40,true))Next();
        }
        else HudText(myTurn?"Your selection joins your bench. Its item goes to your inventory.":"Taken choices are unavailable.",16,HudMuted);
        ImGui.EndChild();ImGui.End();
    }
}

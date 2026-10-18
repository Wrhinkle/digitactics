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
        CarouselCardBounds.Clear();CarouselTextFits=true;CarouselHoveredOption=null;
        float panelWidth=Math.Min(920,size.X-48);
        int columns=Math.Max(1,(int)((panelWidth-40)/164));
        int rows=(carousel.Options.Count+columns-1)/columns;
        var lines=carousel.Options.Select(option=>Rules.Lines.Single(line=>line.Id==option.Line)).ToArray();
        var labels=carousel.Options.Select((option,i)=>new CarouselCardText(lines[i].Forms[0],$"{lines[i].Tier}-cost / {string.Join(", ",lines[i].Traits.Select(Name))}",option.Item is {} item?Name(item):"No item",option.PickedBy is {} name?"Taken by "+name:myTurn?"Click to choose":"Waiting")).ToArray();
        // Reserve scrollbar space when estimating the outer panel. Card text is
        // measured again against the actual child width before it is drawn.
        float estimatedWidth=(panelWidth-40-ImGui.GetStyle().ScrollbarSize-(columns-1)*8)/columns;
        var estimated=labels.Select(label=>MeasureCarouselCard(label,estimatedWidth)).ToArray();
        float cardsHeight=Enumerable.Range(0,rows).Sum(row=>estimated.Skip(row*columns).Take(columns).Max(card=>card.Height)+8);
        float panelHeight=Math.Min(size.Y-48,180+cardsHeight+(intro is null?0:134));
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
        var layouts=labels.Select(label=>MeasureCarouselCard(label,width)).ToArray();
        for(int i=0;i<carousel.Options.Count;i++)
        {
            if(i%columns!=0)ImGui.SameLine(0,8);
            var option=carousel.Options[i];var line=lines[i];var label=labels[i];var layout=layouts[i];
            float cardHeight=layouts.Skip(i/columns*columns).Take(columns).Max(card=>card.Height);
            bool taken=option.PickedBy is not null;
            ImGui.BeginDisabled(taken||!myTurn);
            bool pick=HudButton("carousel/"+i,"",width,cardHeight);
            var min=ImGui.GetItemRectMin();var max=ImGui.GetItemRectMax();
            CarouselCardBounds[i]=(min,max);
            bool hovered=ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled);
            var draw=ImGui.GetWindowDrawList();var ink=HudInk;
            draw.PushClipRect(min,max,true);
            draw.AddRectFilled(min,max,!taken&&hovered?new ImColor8(35,48,69):HudSurface,8);
            draw.AddRect(min+new float2(1,1),max-new float2(1,1),tierColors[Math.Clamp(line.Tier,0,5)],8,thickness:2);
            portraits.Draw(Rules.Id,option.Line,0,new(min.X+(width-96)/2,min.Y+10),96);
            if(option.Item is { } item)itemArtwork.Draw(Rules.Id,item,new(min.X+(width+52)/2,min.Y+79),34);
            draw.AddText(ImGui.GetFont(),18,min+new float2(9,119),ink,label.Name,width-18);
            draw.AddText(ImGui.GetFont(),16,min+new float2(9,layout.TraitsY),HudMuted,label.Traits,width-18);
            draw.AddText(ImGui.GetFont(),16,min+new float2(9,layout.ItemY),ink,label.Item,width-18);
            draw.AddText(ImGui.GetFont(),16,min+new float2(9,layout.StatusY),taken||myTurn?HudGold:HudMuted,label.Status,width-18);
            CarouselTextFits&=min.Y+layout.StatusY+layout.StatusHeight<=max.Y-11;
            // Source .choice.taken dims the complete card, including its item
            // and tier outline; keep disabled choices visibly unavailable.
            if(taken)draw.AddRectFilled(min,max,new ImColor8(15,22,37,153),8);
            draw.PopClipRect();ImGui.EndDisabled();CarouselCardsDrawn++;
            if(hovered)
            {
                ImGui.SetNextWindowSize(new(380,0));ImGui.BeginTooltip();UnitDetails(option.Line,0);
                if(option.Item is { } tooltipItem){var description=text.Get(Rules.Id,tooltipItem);ImGui.Separator();HudText(description.Name,18);UiText.Wrapped(description.Description);}
                if(taken)HudText("Taken by "+option.PickedBy,16,HudGold);
                ImGui.EndTooltip();
                CarouselHoveredOption=i;
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

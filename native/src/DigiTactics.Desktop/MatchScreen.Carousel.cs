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
        float panelWidth=Math.Min(900,size.X-48);
        int columns=Math.Max(1,(int)((panelWidth-36)/172));
        int rows=(carousel.Options.Count+columns-1)/columns;
        var lines=carousel.Options.Select(option=>Rules.Lines.Single(line=>line.Id==option.Line)).ToArray();
        var labels=carousel.Options.Select((option,i)=>new CarouselCardText(lines[i].Forms[0],$"{lines[i].Tier}-cost · {string.Join(", ",lines[i].Traits.Select(Name))}",option.Item is {} item?Name(item):"No item",option.PickedBy is {} name?"Taken by "+name:"")).ToArray();
        // Reserve scrollbar space when estimating the outer panel. Card text is
        // measured again against the actual child width before it is drawn.
        float estimatedWidth=(panelWidth-36-ImGui.GetStyle().ScrollbarSize-(columns-1)*12)/columns;
        var estimated=labels.Select(label=>MeasureCarouselCard(label,estimatedWidth)).ToArray();
        float cardsHeight=Enumerable.Range(0,rows).Sum(row=>estimated.Skip(row*columns).Take(columns).Max(card=>card.Height)+12);
        string subtitle=(myTurn?"Your pick: one Digimon and its item.":carousel.Done?"Everyone has picked.":$"Waiting for {carousel.Turn!.Name}...")+" "+(Session.Rounds.Position.Stage==1?"Pick order is random.":"Lowest HP picks first.");
        float headingHeight=ChoiceLines(subtitle,16,panelWidth-36).Length*21+39;
        float panelHeight=Math.Min(size.Y-48,32+headingHeight+cardsHeight+(carousel.Done?52:0)+(intro is null?0:GalaxyIntroCardHeight(intro,panelWidth-36)+12));
        BeginChoicePanel("Carousel",panelWidth,panelHeight);
        if(intro is not null)GalaxyIntroCard(intro);
        ChoiceCenteredLabel(Session.Rounds.Position.Stage==1?"Opening Carousel":"Carousel",22,HudInk);
        ChoiceCenteredLabel(subtitle,16,myTurn?HudGold:HudMuted);
        ImGui.Dummy(new(0,4));
        ImGui.BeginChild("Carousel choices"u8,new(0,Math.Max(1,ImGui.GetContentRegionAvail().Y-(carousel.Done?52:0))));
        CarouselChoicesBounds=(ImGui.GetWindowDrawList().GetClipRectMin(),ImGui.GetWindowDrawList().GetClipRectMax());
        float available=ImGui.GetContentRegionAvail().X;
        float width=(available-(columns-1)*12)/columns,rowOrigin=ImGui.GetCursorScreenPos().X;
        var layouts=labels.Select(label=>MeasureCarouselCard(label,width)).ToArray();
        for(int i=0;i<carousel.Options.Count;i++)
        {
            int rowCount=Math.Min(columns,carousel.Options.Count-i/columns*columns);
            if(i%columns==0)ImGui.SetCursorScreenPos(new(rowOrigin+(available-rowCount*width-(rowCount-1)*12)/2,ImGui.GetCursorScreenPos().Y));
            else ImGui.SameLine(0,12);
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
            ChoiceCardSurface(draw,min,max,tierColors[Math.Clamp(line.Tier,0,5)],!taken&&hovered);
            portraits.Draw(Rules.Id,option.Line,0,new(min.X+(width-96)/2,min.Y+10),96);
            if(option.Item is { } item)itemArtwork.Draw(Rules.Id,item,new(min.X+(width+52)/2,min.Y+79),34);
            ChoiceText(draw,min+new float2(9,119),width-18,label.Name,18,22,ink,true);
            ChoiceText(draw,min+new float2(9,layout.TraitsY),width-18,label.Traits,16,20,HudMuted,true);
            ChoiceText(draw,min+new float2(9,layout.ItemY),width-18,label.Item,16,20,ink,true);
            ChoiceText(draw,min+new float2(9,layout.StatusY),width-18,label.Status,16,20,HudGold,true);
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
            ImGui.Dummy(new(0,6));
            if(networkRoom is not null)ChoiceCenteredLabel("The next round is starting...",16,HudGold);
            else
            {
                ImGui.SetCursorPosX((ImGui.GetWindowSize().X-260)/2);
                if(HudButton("carouselGo",$"Start round {Rules.Schedule!.Next(Session.Rounds.Position).Label}",260,40,true))Next();
            }
        }
        EndChoicePanel();
    }
}

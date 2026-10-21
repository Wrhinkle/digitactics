using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal int CarouselCardsDrawn { get; private set; }
    internal (float2 Min,float2 Max) CarouselChoicesBounds { get; private set; }

    // ui.js renderModal carousel: "Opening Carousel", the pick-order line and .choice.small cards in tier colours.
    private void CarouselChoices()
    {
        var carousel=Session!.Units.Carousel;
        var intro=CurrentGalaxyIntro();
        bool myTurn=carousel.Turn==Human;
        CarouselCardBounds.Clear();CarouselTextFits=true;CarouselHoveredOption=null;
        var lines=carousel.Options.Select(option=>Rules.Lines.Single(line=>line.Id==option.Line)).ToArray();
        var labels=carousel.Options.Select((option,i)=>new CarouselCardText(lines[i].Forms[0],$"{lines[i].Tier}-cost · {string.Join(", ",lines[i].Traits.Select(Name))}",option.Item is {} item?Name(item):"No item",option.PickedBy is {} name?"Taken by "+name:"")).ToArray();
        float maxInner=ModalMaxWidth-2*ModalPadX-2,cardWidth=CarouselCardWidth;
        int columns=Math.Max(1,Math.Min(carousel.Options.Count,(int)((maxInner+ModalCardGap)/(cardWidth+ModalCardGap))));
        int rows=(carousel.Options.Count+columns-1)/columns;
        var layouts=labels.Select(label=>MeasureCarouselCard(label,cardWidth)).ToArray();
        float cardsHeight=Enumerable.Range(0,rows).Sum(row=>layouts.Skip(row*columns).Take(columns).Max(card=>card.Height))+Math.Max(0,rows-1)*ModalCardGap;
        string title=Session.Rounds.Position.Stage==1?"Opening Carousel":"Carousel";
        string lead=myTurn?"Your pick:":"",rest=(myTurn?" one Digimon and its item.":carousel.Done?"Everyone has picked.":$"Waiting for {carousel.Turn!.Name}...")+" "+(Session.Rounds.Position.Stage==1?"Pick order is random.":"Lowest HP picks first.");
        float contentWidth=carousel.Options.Count>columns?maxInner:Math.Min(maxInner,Math.Max(columns*(cardWidth+ModalCardGap)-ModalCardGap,HudWidth(ModalDisplay,22,title,.88f)));
        float headHeight=22*ModalLine;
        float subWidth=HudWidth(ModalWeight(700),13,lead)+HudWidth(ModalWeight(400),13,rest);
        float subHeight=4+13*ModalLine*Math.Max(1,MathF.Ceiling(subWidth/contentWidth))+12;
        float footerHeight=carousel.Done?12+(networkRoom is not null?13*ModalLine:MathF.Round(14*ModalLine+14)):0;
        float introHeight=intro is null?0:GalaxyIntroCardHeight(intro,contentWidth)+12;
        float cardsOffset=1+ModalPadY+introHeight+headHeight+subHeight;
        float height=Math.Min(Height-20,cardsOffset+cardsHeight+footerHeight+ModalPadY+1);
        float viewportHeight=Math.Max(1,height-cardsOffset-footerHeight-ModalPadY-1);
        bool scrolls=cardsHeight>viewportHeight+.5f;
        float width=Math.Min(ModalMaxWidth,contentWidth+2*ModalPadX+2+(scrolls?ImGui.GetStyle().ScrollbarSize:0));
        BeginChoicePanel("Carousel",width,height);
        var panelAt=ImGui.GetWindowPos();var draw=ImGui.GetWindowDrawList();
        if(intro is not null)GalaxyIntroCard(intro);
        float innerWidth=width-2*ModalPadX-2,x=panelAt.X+1+ModalPadX,y=ImGui.GetCursorScreenPos().Y;
        HudRunCentered(draw,ModalDisplay,22,x+innerWidth/2,y,headHeight,title,CssText,.88f);
        y+=headHeight+4;
        float subX=x+Math.Max(0,(innerWidth-subWidth)/2);
        if(lead.Length>0)subX+=HudRun(draw,ModalWeight(700),13,subX,y,13*ModalLine,lead,CssGold);
        if(subWidth<=innerWidth)HudRun(draw,ModalWeight(400),13,subX,y,13*ModalLine,rest,CssMuted);
        else ModalText(draw,ModalWeight(400),13,x,y,innerWidth,(lead+rest).Trim(),CssMuted,true);
        var cardsAt=new float2(x,MathF.Round(panelAt.Y+cardsOffset));
        ImGui.SetCursorScreenPos(cardsAt);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));
        ImGui.PushStyleColor(ImGuiCol.ChildBg,new ImColor8(0,0,0,0));
        ImGui.BeginChild("Carousel choices"u8,new(innerWidth,viewportHeight),ImGuiChildFlags.None,scrolls?ImGuiWindowFlags.None:ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleColor();ImGui.PopStyleVar();
        CarouselChoicesBounds=(ImGui.GetWindowPos(),ImGui.GetWindowPos()+ImGui.GetWindowSize());
        var cardsDraw=ImGui.GetWindowDrawList();
        float available=ImGui.GetContentRegionAvail().X,rowOrigin=ImGui.GetCursorScreenPos().X,rowY=ImGui.GetCursorScreenPos().Y,contentTop=rowY;
        for(int i=0;i<carousel.Options.Count;i++)
        {
            int rowCount=Math.Min(columns,carousel.Options.Count-i/columns*columns);
            if(i>0&&i%columns==0)rowY+=layouts.Skip(i-columns).Take(columns).Max(card=>card.Height)+ModalCardGap;
            var at=new float2(rowOrigin+Math.Max(0,(available-rowCount*cardWidth-(rowCount-1)*ModalCardGap)/2)+(i%columns)*(cardWidth+ModalCardGap),rowY);
            var option=carousel.Options[i];var line=lines[i];var label=labels[i];var layout=layouts[i];
            float cardHeight=layouts.Skip(i/columns*columns).Take(columns).Max(card=>card.Height);
            bool taken=option.PickedBy is not null;
            ImGui.SetCursorScreenPos(at);
            ImGui.BeginDisabled(taken||!myTurn);
            bool pick=ImGui.InvisibleButton("carousel/"+i,new(cardWidth,cardHeight));
            bool hovered=ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled);
            ImGui.EndDisabled();
            Buttons["carousel/"+i]=at+new float2(cardWidth,cardHeight)*.5f;
            bool lift=hovered&&!taken;
            if(lift)at.Y-=2;
            var min=at;var max=at+new float2(cardWidth,cardHeight);
            CarouselCardBounds[i]=(min,max);
            // .choice.taken: opacity .4 for the whole card.
            float alpha=taken?.4f:1;var under=HudHex("0b1626");
            Rgb Paint(Rgb c)=>HudOver(lift?HudBright(c):c,under,alpha);
            ChoiceCardSurface(cardsDraw,min,max,tierColors[Math.Clamp(line.Tier,0,5)],lift,alpha);
            var portraitAt=new float2(min.X+(cardWidth-96)/2,min.Y+2+8);
            HudPortraitTile(cardsDraw,portraitAt,96,Paint(FamilyColor(option.Line)),10);
            portraits.Draw(Rules.Id,option.Line,0,portraitAt,96,(byte)(255*alpha));
            if(option.Item is { } item)itemArtwork.Draw(Rules.Id,item,portraitAt+new float2(96+8-34,96+6-34),34,(byte)(255*alpha));
            float inner=cardWidth-4-16,textX=min.X+2+8;
            ModalText(cardsDraw,ModalWeight(700),14,textX,min.Y+2+8+96+4,inner,label.Name,Paint(CssText),true);
            ModalText(cardsDraw,ModalWeight(400),11,textX,min.Y+layout.TraitsY,inner,label.Traits,Paint(CssMuted),true);
            ModalText(cardsDraw,ModalWeight(400),11,textX,min.Y+layout.ItemY,inner,label.Item,Paint(CssText),true);
            if(label.Status.Length>0)ModalText(cardsDraw,ModalWeight(400),11,textX,min.Y+layout.StatusY,inner,label.Status,Paint(CssGold),true);
            CarouselTextFits&=layout.StatusY+layout.StatusHeight<=cardHeight-8+.1f;
            CarouselCardsDrawn++;
            if(hovered)
            {
                ImGui.SetNextWindowSize(new(380,0));ImGui.BeginTooltip();UnitDetails(option.Line,0);
                if(option.Item is { } tooltipItem){var description=text.Get(Rules.Id,tooltipItem);ImGui.Separator();HudText(description.Name,18);UiText.Wrapped(description.Description);}
                if(taken)HudText("Taken by "+option.PickedBy,16,HudGold);
                ImGui.EndTooltip();
                CarouselHoveredOption=i;
            }
            if(pick&&!taken&&myTurn){Act("carousel",new{i});break;}
        }
        ImGui.SetCursorScreenPos(new(rowOrigin,contentTop+cardsHeight));ImGui.Dummy(new(0,0));
        ImGui.EndChild();
        if(carousel.Done)
        {
            float footY=cardsAt.Y+viewportHeight+12;
            if(networkRoom is not null)HudRunCentered(draw,ModalWeight(400),13,x+innerWidth/2,footY,13*ModalLine,"The next round is starting...",CssText);
            else
            {
                string label=$"Start round {Rules.Schedule!.Next(Session.Rounds.Position).Label}";
                if(ModalButton("carouselGo",label,new(x+(innerWidth-ModalButtonWidth(label,true))/2,footY),true))Next();
            }
        }
        EndChoicePanel();
    }
}

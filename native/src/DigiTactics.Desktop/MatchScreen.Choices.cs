using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private PlayerOffer? displayedOffer;
    private bool peekChoices;
    internal int OfferArtworkCount { get; private set; }
    internal bool PeekingChoices => peekChoices;

    // .choice (210px, left aligned, 56px icon) and .choice.aug (230px, centred, 64px art).
    private readonly record struct ChoiceCardLayout(float ArtY,float ArtSize,float TitleY,float DescriptionY,float DescriptionHeight,float RecipeY,float Height);
    private static ChoiceCardLayout MeasureChoice(DisplayEntry choice,bool artwork,bool centered,float width,string? recipe)
    {
        float inner=width-4-24,top=2+(centered?16:12);
        float artSize=centered?64:56,artY=top+(centered?2:4);
        float y=artwork?artY+artSize+(centered?8:4)+6:top;
        float titleY=y;
        y+=ModalTextHeight(ModalWeight(700),centered?17:15,choice.Name,inner)+6;
        float descriptionY=y,descriptionHeight=ModalTextHeight(ModalWeight(400),centered?13:12,choice.Description,inner);
        y+=descriptionHeight;
        float recipeY=y+6;
        if(recipe is not null)y=recipeY+ModalTextHeight(ModalWeight(400),12,recipe,inner);
        return new(artY,artSize,titleY,descriptionY,descriptionHeight,recipeY,y+12+2);
    }

    private void Choices()
    {
        OfferArtworkCount=0;
        DisplayedChoiceArt.Clear();DisplayedOfferTitle=DisplayedOfferDescription=null;
        ChoiceCardBounds.Clear();ChoiceTextFits=true;ChoiceScrollMaxY=0;
        GalaxyIntroText=null;
        if(IsSpectating){if(Session!.Rounds.Phase==MatchPhase.Planning&&CurrentGalaxyIntro() is {} watchedIntro)StandaloneGalaxyIntro(watchedIntro);return;}
        if(Session!.Units.Carousel.Active){CarouselChoices();return;}
        var offer=Human.Offer;
        if(!ReferenceEquals(offer,displayedOffer)){displayedOffer=offer;peekChoices=false;}
        if(Session.Rounds.Phase!=MatchPhase.Planning)return;
        var intro=CurrentGalaxyIntro();
        if(offer is null){if(intro is not null)StandaloneGalaxyIntro(intro);return;}
        var heading=LiveOfferHeading(offer);
        DisplayedOfferTitle=heading.Name;DisplayedOfferDescription=heading.Description;
        bool centered=offer.Kind is "augment" or "power";
        var choices=offer.Options.Select(id=>text.Choice(Rules,offer.Kind,id)).ToArray();
        var artworks=offer.Options.Select(id=>ChoiceArtwork.Resolve(Rules.Id,offer.Kind,id)).ToArray();
        var items=offer.Options.Select(id=>offer.Kind is "armory" or "itemTier"?Rules.Items.FirstOrDefault(item=>item.Id==id):null).ToArray();
        var recipes=items.Select(item=>item is {Component:false,Ingredients:{Length:>0} ingredients}?"From "+string.Join(" + ",ingredients.Select(Name)):null).ToArray();
        float cardWidth=centered?230:210;
        // .mpanel shrinks to its content up to min(860px, arena - 24px); the .choices row wraps beyond that.
        float maxInner=ModalMaxWidth-2*ModalPadX-2;
        int columns=Math.Max(1,Math.Min(offer.Options.Length,(int)((maxInner+ModalCardGap)/(cardWidth+ModalCardGap))));
        float rowWidth=columns*(cardWidth+ModalCardGap)-ModalCardGap;
        var layouts=choices.Select((choice,i)=>MeasureChoice(choice,items[i] is not null||artworks[i]?.File is not null,centered,cardWidth,recipes[i])).ToArray();
        int rows=(offer.Options.Length+columns-1)/columns;
        float cardsHeight=Enumerable.Range(0,rows).Sum(row=>layouts.Skip(row*columns).Take(columns).Max(card=>card.Height))+Math.Max(0,rows-1)*ModalCardGap;
        float headWidth=HudWidth(ModalDisplay,22,heading.Name,.88f)+12+ModalButtonWidth(peekChoices?"Show choices":"View board");
        float contentWidth=offer.Options.Length>columns?maxInner:Math.Max(rowWidth,Math.Min(maxInner,Math.Max(headWidth,HudWidth(ModalWeight(400),13,heading.Description))));
        float headHeight=MathF.Round(14*ModalLine+14);
        float introHeight=intro is null?0:GalaxyIntroCardHeight(intro,contentWidth)+12;
        float subHeight=4+ModalTextHeight(ModalWeight(400),13,heading.Description,contentWidth)+12;
        float footerHeight=offer.Rerolls>0?12+headHeight:0;
        float cardsOffset=1+ModalPadY+introHeight+headHeight+subHeight;
        float naturalHeight=cardsOffset+cardsHeight+footerHeight+ModalPadY+1;
        float height=peekChoices?2+2*ModalPadY+headHeight:Math.Min(Height-20,naturalHeight);
        float viewportHeight=Math.Max(1,height-cardsOffset-footerHeight-ModalPadY-1);
        bool scrolls=!peekChoices&&cardsHeight>viewportHeight+.5f;
        float width=Math.Min(ModalMaxWidth,contentWidth+2*ModalPadX+2+(scrolls?ImGui.GetStyle().ScrollbarSize:0));
        BeginChoicePanel(heading.Name+"##reward",width,height,peekChoices);
        var panelAt=ImGui.GetWindowPos();var draw=ImGui.GetWindowDrawList();
        if(!peekChoices&&intro is not null)GalaxyIntroCard(intro);
        // .mhead: h2 (Chakra Petch 700, .04em) with View board beside it, centred as a group.
        var headAt=new float2(panelAt.X+1+ModalPadX,ImGui.GetCursorScreenPos().Y);
        float innerWidth=width-2*ModalPadX-2;
        float headX=headAt.X+Math.Max(0,(innerWidth-headWidth)/2);
        int? headingTier=centered&&offer.Options.Length>0?artworks[0]?.Tier??1:null;
        HudRun(draw,ModalDisplay,22,headX,headAt.Y,headHeight,heading.Name,headingTier is {} ht?ChoiceTierColor(ht):CssText,0,.88f);
        if(ModalButton("peekChoices",peekChoices?"Show choices":"View board",new(headX+HudWidth(ModalDisplay,22,heading.Name,.88f)+12,headAt.Y)))peekChoices=!peekChoices;
        if(peekChoices){EndChoicePanel();return;}
        // .msub: 13px muted, margin 4px 0 12px.
        ModalText(draw,ModalWeight(400),13,headAt.X,headAt.Y+headHeight+4,innerWidth,heading.Description,CssMuted,true);
        var cardsAt=new float2(headAt.X,MathF.Round(panelAt.Y+cardsOffset));
        ImGui.SetCursorScreenPos(cardsAt);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));
        ImGui.PushStyleColor(ImGuiCol.ChildBg,new ImColor8(0,0,0,0));
        ImGui.BeginChild("Reward comparisons"u8,new(innerWidth,viewportHeight),ImGuiChildFlags.None,scrolls?ImGuiWindowFlags.None:ImGuiWindowFlags.NoScrollbar|ImGuiWindowFlags.NoScrollWithMouse);
        ImGui.PopStyleColor();ImGui.PopStyleVar();
        ChoiceViewportBounds=(ImGui.GetWindowPos(),ImGui.GetWindowPos()+ImGui.GetWindowSize());
        var cardsDraw=ImGui.GetWindowDrawList();
        float rowOrigin=ImGui.GetCursorScreenPos().X,rowY=ImGui.GetCursorScreenPos().Y,contentTop=rowY,available=ImGui.GetContentRegionAvail().X;
        for(int i=0;i<offer.Options.Length;i++)
        {
            int rowCount=Math.Min(columns,offer.Options.Length-i/columns*columns);
            if(i>0&&i%columns==0)rowY+=layouts.Skip(i-columns).Take(columns).Max(card=>card.Height)+ModalCardGap;
            var at=new float2(rowOrigin+Math.Max(0,(available-rowCount*cardWidth-(rowCount-1)*ModalCardGap)/2)+(i%columns)*(cardWidth+ModalCardGap),rowY);
            string id=offer.Options[i];var choice=choices[i];var item=items[i];var layout=layouts[i];
            var originalArt=artworks[i];DisplayedChoiceArt[i]=originalArt;
            float cardHeight=layouts.Skip(i/columns*columns).Take(columns).Max(card=>card.Height);
            int? tier=centered?originalArt?.Tier??1:null;
            var border=tier is {} ct?ChoiceTierColor(ct):CssLine2;
            ImGui.SetCursorScreenPos(at);
            bool pick=ImGui.InvisibleButton("offer/"+i,new(cardWidth,cardHeight));
            bool hovered=ImGui.IsItemHovered();
            Buttons["offer/"+i]=at+new float2(cardWidth,cardHeight)*.5f;
            if(hovered)at.Y-=2;
            var min=at;var max=at+new float2(cardWidth,cardHeight);
            ChoiceCardBounds[i]=(min,max);
            ChoiceCardSurface(cardsDraw,min,max,border,hovered);
            float inner=cardWidth-4-24,textX=min.X+2+12;
            if(item is not null){itemArtwork.Draw(Rules.Id,id,new(min.X+(cardWidth-layout.ArtSize)/2,min.Y+layout.ArtY),layout.ArtSize);OfferArtworkCount++;}
            else if(originalArt?.File is {} file){choiceArtwork.Draw(file,new(min.X+(cardWidth-layout.ArtSize)/2,min.Y+layout.ArtY),new(layout.ArtSize,layout.ArtSize));OfferArtworkCount++;}
            var titleInk=tier is {} tt?ChoiceTierColor(tt):CssText;
            ModalText(cardsDraw,ModalWeight(700),centered?17:15,textX,min.Y+layout.TitleY,inner,choice.Name,hovered?HudBright(titleInk):titleInk,centered);
            ModalText(cardsDraw,ModalWeight(400),centered?13:12,textX,min.Y+layout.DescriptionY,inner,choice.Description,hovered?HudBright(CssMuted):CssMuted,centered);
            ChoiceTextFits&=layout.DescriptionY+layout.DescriptionHeight<=cardHeight-12+.1f;
            if(recipes[i] is {} recipe)ModalText(cardsDraw,ModalWeight(400),12,textX,min.Y+layout.RecipeY,inner,recipe,CssMuted,false);
            if(hovered){ImGui.SetNextWindowSize(new(380,0));ImGui.BeginTooltip();HudText(choice.Name,20);HudText(choice.Description);ImGui.EndTooltip();}
            if(pick){Act("offer",new{i});break;}
        }
        ImGui.SetCursorScreenPos(new(rowOrigin,contentTop+cardsHeight));
        ImGui.Dummy(new(0,0));
        ChoiceScrollMaxY=ImGui.GetScrollMaxY();
        ImGui.EndChild();
        if(offer.Rerolls>0)
        {
            string label=$"Reroll choices ({offer.Rerolls})";
            if(ModalButton("offerReroll",label,new(panelAt.X+(width-ModalButtonWidth(label))/2,cardsAt.Y+viewportHeight+12)))Act("offerReroll");
        }
        EndChoicePanel();
    }
}

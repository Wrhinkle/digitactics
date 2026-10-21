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
        const float panelPadding=16,headingHeight=38,headingGap=8,cardsGap=12,viewportInset=1;
        float cardWidth=centered?244:230;
        int columns=Math.Max(1,Math.Min(offer.Options.Length,(int)((Math.Min(860,size.X-48)-36+12)/(cardWidth+12))));
        // Reserve both border insets in addition to the scrollbar. Without them
        // a scrollable three-column row was two pixels wider than its viewport.
        float width=Math.Min(size.X-48,columns*(cardWidth+12)-12+36+ImGui.GetStyle().ScrollbarSize+2*viewportInset);
        var layouts=choices.Select((choice,i)=>MeasureChoice(choice,items[i] is not null||artworks[i]?.File is not null,cardWidth,recipes[i])).ToArray();
        int rows=(offer.Options.Length+columns-1)/columns;
        float cardsHeight=Enumerable.Range(0,rows).Sum(row=>layouts.Skip(row*columns).Take(columns).Max(card=>card.Height))+Math.Max(0,rows-1)*12;
        float introHeight=intro is null?0:GalaxyIntroCardHeight(intro,width-36)+12;
        float descriptionHeight=ChoiceLines(heading.Description,16,width-36).Length*21;
        float footerHeight=offer.Rerolls>0?12+38:0;
        float cardsOffset=panelPadding+introHeight+headingHeight+headingGap+descriptionHeight+cardsGap;
        float height=peekChoices?72:Math.Min(size.Y-48,cardsOffset+cardsHeight+2*viewportInset+footerHeight+panelPadding);
        bool showingChoices=!peekChoices;
        BeginChoicePanel(heading.Name+"##reward",width,height,peekChoices);
        var panelAt=ImGui.GetWindowPos();
        if(!peekChoices&&intro is not null)GalaxyIntroCard(intro);
        int? headingTier=centered&&offer.Options.Length>0?artworks[0]?.Tier??1:null;
        var at=ImGui.GetCursorScreenPos();float headingWidth=ImGui.GetFont().CalcTextSizeA(22,float.MaxValue,0,heading.Name).X;
        float headerWidth=headingWidth+12+138;
        ImGui.SetCursorScreenPos(new(at.X+Math.Max(0,(ImGui.GetContentRegionAvail().X-headerWidth)/2),at.Y));
        var textAt=ImGui.GetCursorScreenPos();
        ImGui.GetWindowDrawList().AddText(ImGui.GetFont(),22,textAt+new float2(0,7),headingTier is {} ht?ChoiceTierColor(ht):HudInk,heading.Name);
        ImGui.Dummy(new(headingWidth,38));ImGui.SameLine(0,12);
        if(HudButton("peekChoices",peekChoices?"Show choices":"View board",138))peekChoices=!peekChoices;
        if(peekChoices||!showingChoices){EndChoicePanel();return;}
        ImGui.SetCursorScreenPos(new(at.X,at.Y+headingHeight+headingGap));
        ChoiceCenteredLabel(heading.Description);
        // Use the same explicit offsets for sizing and drawing. Dummy/Text item
        // spacing previously consumed unmeasured height and clipped short offers.
        var cardsAt=new float2(at.X,panelAt.Y+cardsOffset);
        float viewportHeight=Math.Max(1,height-cardsOffset-footerHeight-panelPadding);
        ImGui.SetCursorScreenPos(cardsAt);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(0,0));
        ImGui.BeginChild("Reward comparisons"u8,new(0,viewportHeight));
        ImGui.PopStyleVar();
        ChoiceViewportBounds=(ImGui.GetWindowDrawList().GetClipRectMin(),ImGui.GetWindowDrawList().GetClipRectMax());
        ImGui.SetCursorScreenPos(ImGui.GetCursorScreenPos()+new float2(viewportInset,viewportInset));
        float rowOrigin=ImGui.GetCursorScreenPos().X,rowY=ImGui.GetCursorScreenPos().Y,contentTop=rowY,available=ImGui.GetContentRegionAvail().X-viewportInset;
        for(int i=0;i<offer.Options.Length;i++)
        {
            int rowCount=Math.Min(columns,offer.Options.Length-i/columns*columns);
            if(i>0&&i%columns==0)rowY+=layouts.Skip(i-columns).Take(columns).Max(card=>card.Height)+12;
            ImGui.SetCursorScreenPos(new(rowOrigin+Math.Max(0,(available-rowCount*cardWidth-(rowCount-1)*12)/2)+(i%columns)*(cardWidth+12),rowY));
            string id=offer.Options[i];var choice=choices[i];var item=items[i];var layout=layouts[i];
            var originalArt=artworks[i];DisplayedChoiceArt[i]=originalArt;
            float cardHeight=layouts.Skip(i/columns*columns).Take(columns).Max(card=>card.Height);
            int? tier=centered?originalArt?.Tier??1:null;
            var choiceColor=tier is {} ct?ChoiceTierColor(ct):HudInk;
            bool pick=HudButton("offer/"+i,"",cardWidth,cardHeight);
            var min=ImGui.GetItemRectMin();var max=ImGui.GetItemRectMax();var draw=ImGui.GetWindowDrawList();
            ChoiceCardBounds[i]=(min,max);
            draw.PushClipRect(min+new float2(1,1),max-new float2(1,1),true);
            ChoiceCardSurface(draw,min,max,tier is not null?choiceColor:ImGui.IsItemHovered()?new ImColor8(200,170,110):new ImColor8(49,66,95),ImGui.IsItemHovered());
            if(item is not null){itemArtwork.Draw(Rules.Id,id,new(min.X+(cardWidth-64)/2,min.Y+18),64);OfferArtworkCount++;}
            else if(originalArt?.File is {} file){choiceArtwork.Draw(file,new(min.X+(cardWidth-64)/2,min.Y+18),new(64,64));OfferArtworkCount++;}
            ChoiceText(draw,min+new float2(14,layout.TitleY),cardWidth-28,choice.Name,20,24,choiceColor,centered);
            ChoiceText(draw,min+new float2(14,layout.DescriptionY),cardWidth-28,choice.Description,16,21,HudMuted,centered);
            ChoiceTextFits&=layout.DescriptionY+layout.DescriptionHeight<=cardHeight-16+.1f;
            if(recipes[i] is {} recipe)ChoiceText(draw,min+new float2(14,layout.RecipeY),cardWidth-28,recipe,16,21,HudMuted,false);
            draw.PopClipRect();
            if(ImGui.IsItemHovered()){ImGui.SetNextWindowSize(new(380,0));ImGui.BeginTooltip();HudText(choice.Name,20);HudText(choice.Description);ImGui.EndTooltip();}
            if(pick){Act("offer",new{i});break;}
        }
        ImGui.SetCursorScreenPos(new(rowOrigin,contentTop+cardsHeight));
        ImGui.Dummy(new(0,viewportInset));
        ChoiceScrollMaxY=ImGui.GetScrollMaxY();
        ImGui.EndChild();
        if(offer.Rerolls>0)
        {
            ImGui.SetCursorScreenPos(new(panelAt.X+(width-210)/2,cardsAt.Y+viewportHeight+12));
            if(HudButton("offerReroll",$"Reroll choices ({offer.Rerolls})",210))Act("offerReroll");
        }
        EndChoicePanel();
    }
}

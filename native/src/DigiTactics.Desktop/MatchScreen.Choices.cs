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
        ChoiceCardBounds.Clear();ChoiceTextFits=true;
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
        float width=Math.Min(960,size.X-48),height=peekChoices?96:Math.Min(size.Y-48,520+(intro is null?0:134));
        Window(heading.Name+"##reward",(size.X-width)/2,peekChoices?Top+8:(size.Y-height)/2,width,height,ImGuiWindowFlags.NoTitleBar);
        if(!peekChoices&&intro is not null)GalaxyIntroCard(intro);
        int? headingTier=offer.Kind is "augment" or "power"&&offer.Options.Length>0?ChoiceArtwork.Resolve(Rules.Id,offer.Kind,offer.Options[0])?.Tier??1:null;
        HudText(heading.Name.ToUpperInvariant(),22,headingTier is {} ht?ChoiceTierColor(ht):HudGold);
        if(HudButton("peekChoices",peekChoices?"Show choices":"View board",150))peekChoices=!peekChoices;
        if(peekChoices){ImGui.End();return;}
        if(offer.Rerolls>0){ImGui.SameLine();if(HudButton("offerReroll",$"Reroll choices ({offer.Rerolls})",210)){Act("offerReroll");ImGui.End();return;}}
        HudText(heading.Description,16,HudMuted);ImGui.Separator();
        int columns=Math.Max(1,Math.Min(offer.Options.Length,(int)(width/230)));
        ImGui.BeginChild("Reward comparisons"u8,new(0,0));
        float cardWidth=(ImGui.GetContentRegionAvail().X-(columns-1)*12)/columns;
        ChoiceViewportBounds=(ImGui.GetWindowDrawList().GetClipRectMin(),ImGui.GetWindowDrawList().GetClipRectMax());
        var choices=offer.Options.Select(id=>text.Choice(Rules,offer.Kind,id)).ToArray();
        var artworks=offer.Options.Select(id=>ChoiceArtwork.Resolve(Rules.Id,offer.Kind,id)).ToArray();
        var items=offer.Options.Select(id=>offer.Kind is "armory" or "itemTier"?Rules.Items.FirstOrDefault(item=>item.Id==id):null).ToArray();
        var layouts=choices.Select((choice,i)=>MeasureChoice(choice,items[i] is not null||artworks[i]?.File is not null,cardWidth)).ToArray();
        for(int i=0;i<offer.Options.Length;i++)
        {
            if(i%columns!=0)ImGui.SameLine(0,12);
            string id=offer.Options[i];var choice=choices[i];var item=items[i];var layout=layouts[i];
            var originalArt=artworks[i];DisplayedChoiceArt[i]=originalArt;
            float cardHeight=layouts.Skip(i/columns*columns).Take(columns).Max(card=>card.Height);
            int? tier=offer.Kind is "augment" or "power"?originalArt?.Tier??1:null;
            var choiceColor=tier is {} ct?ChoiceTierColor(ct):HudInk;
            bool pick=HudButton("offer/"+i,"",cardWidth,cardHeight);
            var min=ImGui.GetItemRectMin();var max=ImGui.GetItemRectMax();var draw=ImGui.GetWindowDrawList();
            ChoiceCardBounds[i]=(min,max);
            draw.PushClipRect(min+new float2(1,1),max-new float2(1,1),true);
            if(tier is not null)draw.AddRect(min+new float2(1,1),max-new float2(1,1),choiceColor,6,thickness:2);
            if(item is not null){itemArtwork.Draw(Rules.Id,id,new(min.X+(cardWidth-72)/2,min.Y+18),72);OfferArtworkCount++;}
            else if(originalArt?.File is {} file){choiceArtwork.Draw(file,new(min.X+(cardWidth-72)/2,min.Y+18),new(72,72));OfferArtworkCount++;}
            draw.AddText(ImGui.GetFont(),20,min+new float2(14,layout.TitleY),choiceColor,choice.Name,cardWidth-28);
            draw.PushClipRect(min+new float2(14,layout.DescriptionY),new(max.X-14,max.Y-64),true);
            draw.AddText(ImGui.GetFont(),16,min+new float2(14,layout.DescriptionY),HudMuted,choice.Description,cardWidth-28);
            draw.PopClipRect();
            ChoiceTextFits&=min.Y+layout.DescriptionY+layout.DescriptionHeight<=max.Y-64+.1f;
            if(item is {Component:false,Ingredients:{Length:>0}ingredients})
                HudClipText(draw,new(min.X+14,max.Y-54),cardWidth-28,"From "+string.Join(" + ",ingredients.Select(Name)),HudMuted);
            HudClipText(draw,new(min.X+14,max.Y-25),cardWidth-28,"Click to choose",HudGold);
            draw.PopClipRect();
            if(ImGui.IsItemHovered()){ImGui.SetNextWindowSize(new(380,0));ImGui.BeginTooltip();HudText(choice.Name,20);HudText(choice.Description);ImGui.EndTooltip();}
            if(pick){Act("offer",new{i});break;}
        }
        ImGui.EndChild();ImGui.End();
    }
}

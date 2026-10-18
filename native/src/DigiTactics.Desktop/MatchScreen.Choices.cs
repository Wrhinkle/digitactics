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
        if(IsSpectating)return;
        if(Session!.Units.Carousel.Active){CarouselChoices();return;}
        var offer=Human.Offer;
        if(!ReferenceEquals(offer,displayedOffer)){displayedOffer=offer;peekChoices=false;}
        if(offer is null||Session.Rounds.Phase!=MatchPhase.Planning)return;
        var heading=text.Offer(Rules.Id,offer.Kind);
        float width=Math.Min(960,size.X-48),height=peekChoices?96:Math.Min(size.Y-48,520);
        Window(heading.Name+"##reward",(size.X-width)/2,peekChoices?Top+8:(size.Y-height)/2,width,height,ImGuiWindowFlags.NoTitleBar);
        HudText(heading.Name.ToUpperInvariant(),22,HudGold);
        if(HudButton("peekChoices",peekChoices?"Show choices":"View board",150))peekChoices=!peekChoices;
        if(peekChoices){ImGui.End();return;}
        if(offer.Rerolls>0){ImGui.SameLine();if(HudButton("offerReroll",$"Reroll choices ({offer.Rerolls})",210)){Act("offerReroll");ImGui.End();return;}}
        HudText(heading.Description,16,HudMuted);ImGui.Separator();
        int columns=Math.Max(1,Math.Min(offer.Options.Length,(int)(width/230)));
        float cardWidth=(ImGui.GetContentRegionAvail().X-(columns-1)*12)/columns;
        ImGui.BeginChild("Reward comparisons"u8,new(0,0));
        for(int i=0;i<offer.Options.Length;i++)
        {
            if(i%columns!=0)ImGui.SameLine(0,12);
            string id=offer.Options[i];var choice=text.Choice(Rules,offer.Kind,id);
            var item=offer.Kind is "armory" or "itemTier"?Rules.Items.FirstOrDefault(item=>item.Id==id):null;
            string? artwork=item is not null?id:offer.Kind=="augment"&&ItemArtwork.Has(Rules.Id,"aug:"+id)?"aug:"+id:null;
            bool pick=HudButton("offer/"+i,"",cardWidth,340);
            var min=ImGui.GetItemRectMin();var max=ImGui.GetItemRectMax();var draw=ImGui.GetWindowDrawList();
            draw.PushClipRect(min+new float2(1,1),max-new float2(1,1),true);
            if(artwork is not null){itemArtwork.Draw(Rules.Id,artwork,new(min.X+(cardWidth-72)/2,min.Y+18),72);OfferArtworkCount++;}
            else
            {
                var center=min+new float2(cardWidth/2,52);
                draw.AddTriangleFilled(center+new float2(0,-25),center+new float2(25,0),center+new float2(-25,0),HudGold);
                draw.AddTriangleFilled(center+new float2(25,0),center+new float2(0,25),center+new float2(-25,0),HudGold);
            }
            draw.AddText(ImGui.GetFont(),20,min+new float2(14,104),HudInk,choice.Name,cardWidth-28);
            draw.PushClipRect(min+new float2(14,160),new(max.X-14,min.Y+280),true);
            draw.AddText(ImGui.GetFont(),16,min+new float2(14,160),HudMuted,choice.Description,cardWidth-28);
            draw.PopClipRect();
            if(item is {Component:false,Ingredients:{Length:>0}ingredients})
                HudClipText(draw,min+new float2(14,286),cardWidth-28,"From "+string.Join(" + ",ingredients.Select(Name)),HudMuted);
            HudClipText(draw,min+new float2(14,315),cardWidth-28,"Click to choose",HudGold);
            draw.PopClipRect();
            if(ImGui.IsItemHovered()){ImGui.SetNextWindowSize(new(380,0));ImGui.BeginTooltip();HudText(choice.Name,20);HudText(choice.Description);ImGui.EndTooltip();}
            if(pick){Act("offer",new{i});break;}
        }
        ImGui.EndChild();ImGui.End();
    }
}

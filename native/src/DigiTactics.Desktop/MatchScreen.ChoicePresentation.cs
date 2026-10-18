using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private readonly PostgameArtwork choiceArtwork=new("choice-art");
    internal string? DisplayedOfferTitle {get;private set;}
    internal string? DisplayedOfferDescription {get;private set;}
    internal Dictionary<int,ChoiceArt?> DisplayedChoiceArt {get;}=[];
    internal Dictionary<int,(float2 Min,float2 Max)> ChoiceCardBounds {get;}=[];
    internal (float2 Min,float2 Max) ChoiceViewportBounds {get;private set;}
    internal bool ChoiceTextFits {get;private set;}
    private readonly record struct ChoiceCardLayout(float TitleY,float DescriptionY,float DescriptionHeight,float Height);
    private static ChoiceCardLayout MeasureChoice(DisplayEntry choice,bool artwork,float width)
    {
        var font=ImGui.GetFont();float bodyWidth=width-28,titleY=artwork?104:28;
        float titleHeight=font.CalcTextSizeA(20,float.MaxValue,bodyWidth,choice.Name).Y;
        float descriptionY=Math.Max(artwork?160:94,titleY+titleHeight+16);
        float descriptionHeight=font.CalcTextSizeA(16,float.MaxValue,bodyWidth,choice.Description).Y;
        return new(titleY,descriptionY,descriptionHeight,Math.Max(340,descriptionY+descriptionHeight+64));
    }
    private static string ChoiceTierName(int tier)=>tier switch{1=>"Silver",2=>"Gold",3=>"Prismatic",_=>"Silver"};
    private static ImColor8 ChoiceTierColor(int tier)=>tier switch{2=>new(255,207,74),3=>new(229,139,255),_=>new(201,209,220)};
    private DisplayEntry LiveOfferHeading(PlayerOffer offer)
    {
        var heading=text.Offer(Rules.Id,offer.Kind);
        if(offer.Kind=="augment")
        {
            int tier=offer.Options.Length>0?ChoiceArtwork.Resolve(Rules.Id,"augment",offer.Options[0])?.Tier??1:1;
            return heading with{Name=ChoiceTierName(tier)+" Augment",Description="Pick one augment. It lasts the whole game."};
        }
        if(offer.Kind=="power")return heading with{Description="Pick your Tamer power. It ranks up later in the game."};
        if(offer.Kind is "armory" or "itemTier")return heading with{Description="Pick one item. It goes to your item bench."};
        if(offer.Kind=="upgrade"&&Session!.State.GizmosGates is {Length:>0} gates)
            return heading with{Description=heading.Description.TrimEnd()+" Gates: "+string.Join(", ",gates.Select(ChoiceTierName))+"."};
        return heading;
    }
}

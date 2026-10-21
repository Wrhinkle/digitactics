using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal Dictionary<int,(float2 Min,float2 Max)> CarouselCardBounds {get;}=[];
    internal bool CarouselTextFits {get;private set;}
    internal int? CarouselHoveredOption {get;private set;}
    private readonly record struct CarouselCardText(string Name,string Traits,string Item,string Status);
    private readonly record struct CarouselCardLayout(float TraitsY,float ItemY,float StatusY,float StatusHeight,float Height);
    private static CarouselCardLayout MeasureCarouselCard(CarouselCardText text,float width)
    {
        float bodyWidth=width-18;
        float traitsY=119+ChoiceLines(text.Name,18,bodyWidth).Length*22+4;
        float itemY=traitsY+ChoiceLines(text.Traits,16,bodyWidth).Length*20+4;
        float statusY=itemY+ChoiceLines(text.Item,16,bodyWidth).Length*20+(text.Status.Length>0?8:0);
        float statusHeight=ChoiceLines(text.Status,16,bodyWidth).Length*20;
        return new(traitsY,itemY,statusY,statusHeight,statusY+statusHeight+12);
    }
}

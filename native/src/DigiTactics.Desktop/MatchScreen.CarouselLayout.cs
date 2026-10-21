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
    // .choice.small: 140px wide, 8px padding, 96px portrait, 14px name, 11px lines, 4px gaps.
    private const float CarouselCardWidth=140;
    private static CarouselCardLayout MeasureCarouselCard(CarouselCardText text,float width)
    {
        float inner=width-4-16,y=2+8+96+4;
        y+=ModalTextHeight(ModalWeight(700),14,text.Name,inner)+4;
        float traitsY=y;y+=ModalTextHeight(ModalWeight(400),11,text.Traits,inner)+4;
        float itemY=y;y+=ModalTextHeight(ModalWeight(400),11,text.Item,inner);
        float statusY=y+(text.Status.Length>0?4:0),statusHeight=ModalTextHeight(ModalWeight(400),11,text.Status,inner);
        return new(traitsY,itemY,statusY,statusHeight,MathF.Ceiling(statusY+statusHeight+8+2));
    }
}

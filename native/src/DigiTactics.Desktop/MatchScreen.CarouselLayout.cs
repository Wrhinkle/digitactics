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
        var font=ImGui.GetFont();float bodyWidth=width-18;
        float traitsY=119+font.CalcTextSizeA(18,float.MaxValue,bodyWidth,text.Name).Y+4;
        float itemY=traitsY+font.CalcTextSizeA(16,float.MaxValue,bodyWidth,text.Traits).Y+4;
        float statusY=itemY+font.CalcTextSizeA(16,float.MaxValue,bodyWidth,text.Item).Y+8;
        float statusHeight=font.CalcTextSizeA(16,float.MaxValue,bodyWidth,text.Status).Y;
        return new(traitsY,itemY,statusY,statusHeight,statusY+statusHeight+12);
    }
}

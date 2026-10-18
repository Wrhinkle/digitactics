using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal IReadOnlyList<HudTimelineRound> DisplayedTimeline {get;private set;}=[];
    internal (float2 Min,float2 Max) TimelineBounds {get;private set;}
    internal string? TimelineTooltip {get;private set;}
    private void RoundTimeline()
    {
        matchHud.Capture(Session!);TimelineTooltip=null;
        DisplayedTimeline=matchHud.Timeline(Session!,viewed);
        float width=Math.Min(388,size.X-662),cell=(width-(DisplayedTimeline.Count-1)*6)/DisplayedTimeline.Count;
        var at=new float2(size.X-248-width,27);TimelineBounds=(at,at+new float2(width,32));
        for(int i=0;i<DisplayedTimeline.Count;i++)
        {
            var round=DisplayedTimeline[i];var start=at+new float2(i*(cell+6),0);
            ImGui.SetCursorScreenPos(start);ImGui.InvisibleButton("roundTimeline/"+round.Round,new(cell,32));
            Buttons["roundTimeline/"+round.Round]=(ImGui.GetItemRectMin()+ImGui.GetItemRectMax())*.5f;
            var draw=ImGui.GetWindowDrawList();var fill=round.Current?new ImColor8(44,53,80):round.Outcome=="w"?new ImColor8(30,74,51):round.Outcome=="l"?new ImColor8(77,31,39):new ImColor8(22,28,42);
            draw.AddRectFilled(start,start+new float2(cell,32),fill,4);draw.AddRect(start,start+new float2(cell,32),round.Current?HudGold:HudLine,4,thickness:round.Current?2:1);
            string label=round.Kind=="carousel"?"C":round.Kind=="pve"?"PvE":"PvP";
            ImGui.PushFont(default,15);var textSize=ImGui.CalcTextSize(label);draw.AddText(start+(new float2(cell,32)-textSize)*.5f,HudInk,label);ImGui.PopFont();
            if(round.Offer is {} offer)draw.AddCircleFilled(start+new float2(cell-2,1),4,offer is "augment" or "power"?new ImColor8(229,139,255):new ImColor8(79,179,255));
            if(ImGui.IsItemHovered())
            {
                string detail=round.Round+" / "+(round.Kind=="carousel"?"Carousel":round.Kind=="pve"?"Wild Digimon":"Player battle");
                if(round.Offer is {} kind)detail+=" / "+text.Offer(Rules.Id,kind).Name;
                if(round.Outcome is {} outcome)detail+=" / "+(outcome=="w"?"Won":outcome=="l"?"Lost":"Draw");
                TimelineTooltip=detail;ResultTooltip(detail);
            }
        }
    }
}

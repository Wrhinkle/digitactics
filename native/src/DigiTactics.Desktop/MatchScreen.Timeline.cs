using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal IReadOnlyList<HudTimelineRound> DisplayedTimeline {get;private set;}=[];
    internal (float2 Min,float2 Max) TimelineBounds {get;private set;}
    internal string? TimelineTooltip {get;private set;}
    private float RoundTrackWidth()=>Math.Max(0,matchHud.Timeline(Session!,viewed).Count()*(RoundCellWidth+4)-4);
    // #stageTrack .rt cells: 26x22 (22 wide at 1360px and below), 10px/800 labels, offer dots on the corner.
    private void RoundTimeline(float x,float center)
    {
        matchHud.Capture(Session!);TimelineTooltip=null;
        DisplayedTimeline=matchHud.Timeline(Session!,viewed);
        float cell=RoundCellWidth,width=Math.Max(0,DisplayedTimeline.Count*(cell+4)-4);
        var at=new float2(x,center-11);TimelineBounds=(at,at+new float2(width,22));
        var draw=ImGui.GetWindowDrawList();
        for(int i=0;i<DisplayedTimeline.Count;i++)
        {
            var round=DisplayedTimeline[i];var start=at+new float2(i*(cell+4),0);var end=start+new float2(cell,22);
            ImGui.SetCursorScreenPos(start);ImGui.InvisibleButton("roundTimeline/"+round.Round,new(cell,22));
            Buttons["roundTimeline/"+round.Round]=(start+end)*.5f;
            var (fill,border,ink)=round.Current?(HudHex("2c3550"),CssGold,CssText)
                :round.Outcome=="w"?(HudHex("1e4a33"),HudHex("2f7a52"),HudHex("bff5d3"))
                :round.Outcome=="l"?(HudHex("4d1f27"),HudHex("8a3340"),HudHex("ffc9cf"))
                :round.Outcome=="d"?(HudHex("3a3a46"),CssLine2,CssMuted):(CssPanel2,CssLine2,CssMuted);
            draw.AddRectFilled(start,end,fill,5);
            draw.AddRect(start,end,border,5,thickness:round.Current?2:1);
            string label=round.Kind=="carousel"?"C":round.Kind=="pve"?"PvE":"PvP";
            HudRunCentered(draw,HudWeight(800),10,(start.X+end.X)/2,start.Y+1,20,label,ink);
            if(round.Offer is {} offer)
            {
                var dot=end-new float2(0,22);
                draw.AddCircleFilled(dot,5,offer is "augment" or "power"?HudHex("e58bff"):HudHex("4fb3ff"));
                draw.AddCircle(dot,5,HudHex("0b0e15"));
            }
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

using Brutal.ImGuiApi;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

/// <summary>Real pointer coverage for restored standings cards and the round timeline.</summary>
internal sealed class MatchHudSmoke
{
    private int step,wait=3;
    internal bool Done=>step==10;
    internal string? CaptureRequested {get;set;}
    internal void Input(MatchScreen screen)
    {
        if(Done||wait-->0)return;
        var io=ImGui.GetIO();var match=screen.Session!;
        if(step==0)
        {
            if(screen.StandingAvatarCount!=match.State.Players.Count)throw new InvalidOperationException("Standings lost a player's Little Legend avatar.");
            if(screen.DisplayedTimeline.Count!=match.State.Rules.Schedule!.RoundsInStage(match.Rounds.Position.Stage)||screen.DisplayedTimeline.Count(r=>r.Current)!=1)
                throw new InvalidOperationException("Round timeline omitted a scheduled/current round.");
            if(screen.TimelineBounds.Min.X<60||screen.TimelineBounds.Max.X>screen.HeaderOpponentBounds.Min.X-110)
                throw new InvalidOperationException("Round timeline overlaps the round heading or Fight control.");
            if(screen.HeaderOpponentName!="Wild Digimon"||screen.HeaderOpponentBounds.Min.X<screen.TimelineBounds.Max.X||screen.HeaderOpponentBounds.Max.Y>60)
                throw new InvalidOperationException("PvE opponent card is missing or overlaps the round timeline or utility controls.");
        }
        if(step is 0 or 2 or 4)
        {
            int seat=step==2?7:0;var point=screen.Buttons["view/"+seat];var bounds=screen.ScoutingBounds;
            if(point.Y<bounds.Min.Y+22||point.Y>bounds.Max.Y-22)
            {
                io.AddMousePosEvent((bounds.Min.X+bounds.Max.X)*.5f,(bounds.Min.Y+bounds.Max.Y)*.5f);
                io.AddMouseWheelEvent(0,point.Y<bounds.Min.Y+22?1:-1);wait=3;return;
            }
            io.AddMousePosEvent(point.X,point.Y);if(step!=0)io.AddMouseButtonEvent(0,true);
        }
        else if(step==1)
        {
            if(screen.StandingTooltipSeat!=0)throw new InvalidOperationException("Standing hover did not show the selected Tamer's team summary.");
            var tooltip=screen.StandingTooltipBounds;
            if(tooltip.Min.X<0||tooltip.Min.Y<0||tooltip.Max.X>screen.ScoutingBounds.Min.X||tooltip.Max.Y>io.DisplaySize.Y)throw new InvalidOperationException("Standing tooltip covers the roster or leaves the viewport.");
            CaptureRequested="standings-hover";
        }
        else if(step is 3 or 5)io.AddMouseButtonEvent(0,false);
        else if(step==6)
        {
            if(screen.ViewedPlayer!=0)throw new InvalidOperationException("Scouting cards did not return to your own board.");
            var round=screen.DisplayedTimeline.First();var point=screen.Buttons["roundTimeline/"+round.Round];io.AddMousePosEvent(point.X,point.Y);
        }
        else if(step==7)
        {
            if(screen.TimelineTooltip?.StartsWith(screen.DisplayedTimeline[0].Round+" / ",StringComparison.Ordinal)!=true)
                throw new InvalidOperationException("Timeline hover did not identify the round and battle type.");
            CaptureRequested="timeline";
        }
        else if(step==8){var point=screen.Buttons["headerOpponent"];io.AddMousePosEvent(point.X,point.Y);}
        else if(step==9)
        {
            if(screen.HeaderOpponentTooltip!="Opponent / Wild Digimon")throw new InvalidOperationException("Opponent header hover lost its full name and battle context.");
            CaptureRequested="opponent";Console.WriteLine("Match HUD UI passed: all avatars, readable timeline bounds and round hover, opponent name card, player team tooltip, scrolling to last Tamer and returning to own board.");
        }
        if(step==4&&screen.ViewedPlayer!=7)throw new InvalidOperationException("Last standings card could not be scouted through its real pointer target.");
        step++;wait=3;
    }
}

using DigiTactics.Rules;
using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal double? NetworkPlanningDuration => networkReplica?.Match?.PlanMs / 1000;
    internal double? NetworkPlanningRemaining { get; private set; }
    private int? networkCountdownSecond;
    private long? networkCountdownPhase;
    private MatchPlanningSession? networkCountdownSession;
    private void UpdateNetworkCountdown()
    {
        NetworkPlanningRemaining = null;
        if (networkRoom is null || networkReplica?.Match is not { } match || Session is null || Session.State.Over || !Human.Alive ||
            Session.Rounds.Phase != MatchPhase.Planning || Session.Rounds.RoundType == "carousel")
        {
            networkCountdownSecond = null; networkCountdownPhase = null; networkCountdownSession = null; return;
        }
        if (!ReferenceEquals(networkCountdownSession, Session) || networkCountdownPhase != match.PhaseAt)
        {
            networkCountdownSession = Session; networkCountdownPhase = match.PhaseAt; networkCountdownSecond = null;
        }
        NetworkPlanningRemaining = Math.Max(0,(match.PhaseAt + match.PlanMs - networkRoom.ServerTime(networkClock())) / 1000);
        if (CountdownCue.Select(networkCountdownSecond, NetworkPlanningRemaining.Value) is { } cue) audio.Play(cue);
        networkCountdownSecond = (int)Math.Ceiling(NetworkPlanningRemaining.Value);
    }
    private double? resultCountdown;
    private MatchPlanningSession? clockSession;
    private string? clockRound;
    internal bool PlanningTimerEnabled { get; private set; } = true;
    internal double PlanningRemaining { get; private set; }
    private void UpdatePlanningCountdown(double delta)
    {
        if (Session is null || networkRoom is not null) return;
        if (!ReferenceEquals(clockSession, Session) || clockRound != Session.Rounds.Position.Label)
        {
            clockSession = Session; clockRound = Session.Rounds.Position.Label;
            PlanningRemaining = WatchingAfterElimination ? 4 : Rules.PlanningSeconds;
        }
        if (Session.State.Over || Session.Rounds.Phase != MatchPhase.Planning ||
            (!WatchingAfterElimination && (!PlanningTimerEnabled || !Human.Alive || Session.Rounds.RoundType == "carousel"))) return;
        int previous = (int)Math.Ceiling(PlanningRemaining);
        PlanningRemaining = Math.Max(0, PlanningRemaining - Math.Max(0,delta));
        if (CountdownCue.Select(previous, PlanningRemaining) is { } cue) audio.Play(cue);
        if (PlanningRemaining <= 0) Fight(autoResolveOffers:true);
    }
    private void PlanningTimerControls()
    {
        if (Session!.State.Over || !Human.Alive) return;
        if(networkRoom is not null)return;
        ImGui.SameLine();
        if(Button("planningTimer",PlanningTimerEnabled?"Timer on":"Timer off",78,34))
        {
            PlanningTimerEnabled=!PlanningTimerEnabled;
            PlanningRemaining=Rules.PlanningSeconds;
        }
    }
    private void UpdateResultCountdown(double delta)
    {
        if (networkRoom is not null || Session is null || Session.State.Over || (!Human.Alive && !WatchingAfterElimination) ||
            Session.Rounds.Phase != MatchPhase.Result || resultCountdown is null || (Session.Rounds.Results.Count == 0 && !WatchingAfterElimination)) return;
        resultCountdown -= Math.Max(0, delta);
        if (resultCountdown <= 0) Next();
    }
}

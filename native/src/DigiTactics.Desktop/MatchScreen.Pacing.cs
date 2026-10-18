using DigiTactics.Rules;
using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal double? NetworkPlanningDuration => networkReplica?.Match?.PlanMs / 1000;
    internal double? NetworkPlanningRemaining { get; private set; }
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
            PlanningRemaining = Rules.PlanningSeconds;
        }
        if (!PlanningTimerEnabled || Session.State.Over || !Human.Alive || Session.Rounds.Phase != MatchPhase.Planning || Session.Rounds.RoundType == "carousel") return;
        int previous = (int)Math.Ceiling(PlanningRemaining);
        PlanningRemaining = Math.Max(0, PlanningRemaining - Math.Max(0,delta));
        int remaining = (int)Math.Ceiling(PlanningRemaining);
        if (remaining != previous && remaining is > 0 and <= 5) audio.Play(remaining == 1 ? "tick_last" : "tick_normal");
        if (PlanningRemaining <= 0) Fight();
    }
    private void PlanningTimerControls()
    {
        NetworkPlanningRemaining = null;
        if (Session!.State.Over || !Human.Alive) return;
        if (networkRoom is not null)
        {
            if (networkReplica?.Match is { } match && Session.Rounds.Phase == MatchPhase.Planning && Session.Rounds.RoundType != "carousel")
            {
                NetworkPlanningRemaining = Math.Max(0,(match.PhaseAt + match.PlanMs - networkRoom.ServerTime(networkClock())) / 1000);
                ImGui.SameLine(); ImGui.Text($"{Math.Ceiling(NetworkPlanningRemaining.Value)}s");
            }
            return;
        }
        ImGui.SameLine();
        if (Button("planningTimer", PlanningTimerEnabled ? "Timer on" : "Timer off"))
        {
            PlanningTimerEnabled = !PlanningTimerEnabled;
            PlanningRemaining = Rules.PlanningSeconds;
        }
        if (PlanningTimerEnabled && Session.Rounds.Phase == MatchPhase.Planning && Session.Rounds.RoundType != "carousel")
        {
            ImGui.SameLine(); ImGui.Text($"{Math.Max(0,Math.Ceiling(PlanningRemaining))}s");
        }
    }
    private void UpdateResultCountdown(double delta)
    {
        if (networkRoom is not null || Session is null || Session.State.Over || !Human.Alive ||
            Session.Rounds.Phase != MatchPhase.Result || resultCountdown is null || Session.Rounds.Results.Count == 0) return;
        resultCountdown -= Math.Max(0, delta);
        if (resultCountdown <= 0) Next();
    }
}

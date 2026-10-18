using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private double? resultCountdown;
    private void UpdateResultCountdown(double delta)
    {
        if (networkRoom is not null || Session is null || Session.State.Over || !Human.Alive ||
            Session.Rounds.Phase != MatchPhase.Result || resultCountdown is null || Session.Rounds.Results.Count == 0) return;
        resultCountdown -= Math.Max(0, delta);
        if (resultCountdown <= 0) Next();
    }
}

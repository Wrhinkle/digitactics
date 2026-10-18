using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal bool WatchingAfterElimination => networkRoom is null && Session is not null && Session.State.PlayOut && !Human.Alive;
    private bool CanWatchAfterElimination => networkRoom is null && Session is not null && Session.State.Over && !Human.Alive &&
        (Session.State.Duos ? Session.State.Teams.Count(t => t.Alive) : Session.State.Players.Count(p => p.Alive)) > 1;
    private int LivingWatchSeat() => Session!.State.Players[viewed].Alive ? viewed : Session.State.Players.First(p => p.Alive).Id;
    private void ContinueAfterElimination()
    {
        if (!CanWatchAfterElimination || !Session!.State.PlayOn()) return;
        Next();
        audio.Music("planning");
        status = $"Placement #{Human.Placement}. Watching the remaining teams.";
    }
}

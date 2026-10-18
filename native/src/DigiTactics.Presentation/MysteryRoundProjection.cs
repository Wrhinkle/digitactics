using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Presentation;

public static class MysteryRoundProjection
{
    public static MysteryRound From(MatchPlanningSession session, int seat, bool live = true)
    {
        bool participated = false;
        if (seat >= 0 && seat < session.State.Players.Count)
        {
            var player = session.State.Players[seat];
            participated = session.Rounds.Results.Any(result =>
                (result.Match.TeamA is { } teamA ? player.Team == teamA : result.Match.A == player.Id)
                || !result.Match.Ghost && (result.Match.TeamB is { } teamB ? player.Team == teamB : result.Match.B == player.Id));
        }
        return new(session.State.Rules.Id.Split('/')[0], session.State.Duos, session.State.Seed, seat, session.Rounds.Position.Stage, session.Rounds.Position.Round, participated, live);
    }
}

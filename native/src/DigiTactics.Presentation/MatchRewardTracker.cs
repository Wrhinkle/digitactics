using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Presentation;

/// <summary>Read-only reward statistics. Each settlement contributes at most once.</summary>
public sealed class MatchRewardTracker(MatchPlanningSession session)
{
    private IReadOnlyList<MatchRoundResult>? seen;
    private int wins, kills, stars = 1, level;
    public RewardStats Stats => new(wins, kills, stars, level, session.State.Players[0].Placement);
    public void Update()
    {
        var state = session.State; var me = state.Players[0];
        if (me.Alive)
        {
            level = Math.Max(level, me.Economy.Level);
            foreach (var entry in me.Roster.Entries) stars = Math.Max(stars, entry.Unit.Stage + 1);
        }
        var results = session.Rounds.Results;
        if (ReferenceEquals(results, seen) || results.Count == 0) return;
        seen = results;
        foreach (var result in results)
        {
            var match = result.Match;
            if (match.Pve) continue;
            bool a = match.A == me.Id || match.TeamA is { } ta && state.Teams[ta].Members.Contains(me);
            bool b = match.B == me.Id || match.TeamB is { } tb && state.Teams[tb].Members.Contains(me);
            if (a && result.AWon)
            {
                wins++;
                if (match.B is { } opponent && !match.Ghost && !state.Players[opponent].Alive && result.DamageB > 0) kills++;
            }
            if (b && !match.Ghost && result.BWon)
            {
                wins++;
                if (!state.Players[match.A].Alive && result.DamageA > 0) kills++;
            }
        }
    }
}

using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed record PlayerRoundResult(MatchRoundResult Result, bool SideA)
{
    public bool Won => SideA ? Result.AWon : Result.BWon;
    public bool Lost => SideA ? Result.BWon : Result.AWon;
    public string Title => Won ? "Victory" : Lost ? "Defeat" : "Draw";
    public string Sound => Won ? "win" : Lost ? "loss" : "round";
    public int HealthLoss => SideA ? Result.DamageA : Result.DamageB;

    public static PlayerRoundResult? Find(IEnumerable<MatchRoundResult> results, MatchPlayer player)
    {
        foreach (var result in results)
        {
            bool a = result.Match.TeamA is { } teamA ? player.Team == teamA : result.Match.A == player.Id;
            bool b = !result.Match.Ghost && (result.Match.TeamB is { } teamB ? player.Team == teamB : result.Match.B == player.Id);
            if (a || b) return new(result, a);
        }
        return null;
    }
}

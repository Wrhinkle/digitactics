using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal static class PlayerRoundResultChecks
{
    public static void Run(ContentCatalog catalog)
    {
        var rules = catalog.Variants[0];
        foreach (bool duos in new[] { false, true })
        {
            var session = new MatchPlanningSession(rules,143,Enumerable.Range(0,8).Select(i => new MatchSeat("Tamer " + i,i==0)).ToArray(),duos);
            var player = session.State.Players[0]; int? ownTeam = player.Team;
            int? otherTeam = duos ? session.State.Teams.First(team => team.Id != ownTeam).Id : null;
            MatchRoundResult Result(bool ghost, bool localA, int winner) => new(new(localA?0:2,localA?2:0,false,ghost,winner,[],new Dictionary<string,int>(),new Dictionary<string,int>(),localA?ownTeam:otherTeam,localA?otherTeam:ownTeam));
            foreach (bool localA in new[] { false,true }) foreach (int winner in new[] { -1,0,1 })
            {
                var actual = Result(false,localA,winner);
                var selected = PlayerRoundResult.Find([Result(true,false,1),actual],player);
                string expected = winner == (localA?0:1) ? "win" : winner == -1 ? "round" : "loss";
                if (selected?.Result != actual || selected.SideA != localA || selected.Sound != expected)
                    throw new InvalidOperationException("Result presentation selected a ghost opponent or the wrong side/outcome.");
            }
            if (PlayerRoundResult.Find([Result(true,false,1)],player) is not null) throw new InvalidOperationException("Ghost-only result was treated as the player's fight.");
        }
        Console.WriteLine("Player result checks passed: Solo/Duos, both sides, win/loss/draw and ghost-opponent exclusion.");
    }
}

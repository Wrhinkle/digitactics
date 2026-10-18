using DigiTactics.Rules;

internal static class PlayOnChecks
{
    public static int Run(ContentCatalog catalog)
    {
        int checks = 0;
        void Check(bool value, string reason) { checks++; if (!value) throw new InvalidOperationException(reason); }
        foreach (var rules in catalog.Variants)
        foreach (bool duo in new[] { false, true })
        {
            var session = new MatchPlanningSession(rules, 143, Enumerable.Range(0, 8).Select(i => new MatchSeat($"Player {i}", i == 0)).ToArray(), duo);
            var state = session.State;
            Check(!state.PlayOn() && !state.PlayOut, "A live match must not resume.");
            if (duo) state.DamageTeam(state.Players[0].Team!.Value, 10000);
            else state.Players[0].Health = -10000;
            state.Eliminate();
            int placement = state.Players[0].Placement;
            Check(state.Over && !state.Players[0].Alive, "Local elimination must end the match initially.");
            Check(state.PlayOn() && !state.Over && state.PlayOut, "Eliminated player can watch remaining opponents.");
            Check(!state.PlayOn(), "Resume must be idempotent.");
            state.Eliminate();
            Check(!state.Over && state.Players[0].Placement == placement, "Spectating must preserve placement and allow remaining teams to play.");
            if (duo)
                foreach (var team in state.Teams.Where(t => t.Alive).Skip(1).ToArray()) state.DamageTeam(team.Id, 10000);
            else
                foreach (var player in state.Players.Where(p => p.Alive).Skip(1)) player.Health = -10000;
            state.Eliminate();
            Check(state.Over && !state.PlayOn(), "A decided match cannot resume.");
            Check(state.Players[0].Placement == placement, "Final winner must not change eliminated placement.");
        }
        Console.WriteLine($"Elimination continuation: {checks} checks across all variants and Solo/Duos passed.");
        return checks;
    }
}

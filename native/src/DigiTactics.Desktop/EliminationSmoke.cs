using Brutal.ImGuiApi;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed class EliminationSmoke
{
    private int step, placement, rounds;
    public bool Done { get; private set; }
    public string Pending=>$"Elimination/{step}/round {rounds+1}";
    public void Input(MatchScreen screen)
    {
        var session = screen.Session!;
        void Click(string key) { var p = screen.Buttons[key]; ImGui.GetIO().AddMousePosEvent(p.X,p.Y); ImGui.GetIO().AddMouseButtonEvent(0,true); }
        switch (step)
        {
            case 0:
                // Deterministic knockout fixture; continuation itself uses rendered controls.
                if (session.State.Duos) session.State.DamageTeam(session.State.Players[0].Team!.Value,10000);
                else session.State.Players[0].Health = -10000;
                session.StartCombat(); session.FinishCombat();
                placement = session.State.Players[0].Placement;
                if (!session.State.Over) throw new InvalidOperationException("Elimination fixture did not end the local match.");
                step++; break;
            case 1: Click("spectateRemaining"); step++; break;
            case 2: ImGui.GetIO().AddMouseButtonEvent(0,false); step++; break;
            case 3:
                if (!screen.WatchingAfterElimination || session.State.Over || !session.State.Players[screen.ViewedPlayer].Alive)
                    throw new InvalidOperationException("Spectate did not resume play and follow a surviving player.");
                if (session.Rounds.Phase != MatchPhase.Planning || screen.PlanningRemaining is <= 0 or > 4)
                    throw new InvalidOperationException("Eliminated planning timer is not four seconds.");
                screen.Update(5,5); step++; break;
            case 4:
                if (session.Rounds.Phase == MatchPhase.Combat) Click("finish");
                step++; break;
            case 5: ImGui.GetIO().AddMouseButtonEvent(0,false); step++; break;
            case 6:
                screen.Update(2.3,2.3);
                if (session.Rounds.Phase != MatchPhase.Planning || session.State.Players[0].Placement != placement)
                    throw new InvalidOperationException("Eliminated spectator stalled at results or changed placement.");
                if (++rounds < 5) step = 3;
                else { Done = true; Console.WriteLine("Elimination UI: Spectate, survivor following, four-second planning and five automatic round transitions passed."); }
                break;
        }
    }
}

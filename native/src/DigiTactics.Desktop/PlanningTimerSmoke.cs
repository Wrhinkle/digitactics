using Brutal.ImGuiApi;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed class PlanningTimerSmoke
{
    private int step;
    public bool Input(MatchScreen screen)
    {
        void Toggle()
        {
            var point = screen.Buttons["planningTimer"];
            ImGui.GetIO().AddMousePosEvent(point.X,point.Y); ImGui.GetIO().AddMouseButtonEvent(0,true);
        }
        switch (step++)
        {
            case 0: Toggle(); break;
            case 1: ImGui.GetIO().AddMouseButtonEvent(0,false); break;
            case 2:
                if (screen.PlanningTimerEnabled) throw new InvalidOperationException("Planning timer toggle did not turn off.");
                double remaining = screen.PlanningRemaining;
                screen.Update(2,2);
                if (screen.PlanningRemaining != remaining || screen.Session!.Rounds.Phase != MatchPhase.Planning)
                    throw new InvalidOperationException("Disabled planning timer advanced.");
                Toggle(); break;
            case 3: ImGui.GetIO().AddMouseButtonEvent(0,false); break;
            case 4:
                if (!screen.PlanningTimerEnabled || screen.PlanningRemaining < screen.Session!.State.Rules.PlanningSeconds - .5)
                    throw new InvalidOperationException("Re-enabling the planning timer did not reset its duration.");
                screen.Update(screen.PlanningRemaining - .2);
                if (screen.Session.Rounds.Phase != MatchPhase.Planning) throw new InvalidOperationException("Planning timer started combat early.");
                screen.Update(.25);
                if (screen.Session.Rounds.Phase != MatchPhase.Combat) throw new InvalidOperationException("Expired planning timer did not start combat.");
                return true;
        }
        return false;
    }
}

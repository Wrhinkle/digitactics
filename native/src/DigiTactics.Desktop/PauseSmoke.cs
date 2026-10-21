using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal sealed class PauseSmoke(bool keyboardResume = false)
{
    private int step;
    public bool Input(MatchScreen screen)
    {
        void Click(string key) { var p = screen.Buttons[key]; ImGui.GetIO().AddMousePosEvent(p.X,p.Y); ImGui.GetIO().AddMouseButtonEvent(0,true); }
        switch (step++)
        {
            // The original top bar has no Pause button: Menu, then the menu's Pause game.
            case 0: Click("gameMenu"); break;
            case 1: ImGui.GetIO().AddMouseButtonEvent(0,false); break;
            case 2: Click("menuPause"); break;
            case 3: ImGui.GetIO().AddMouseButtonEvent(0,false); break;
            case 4:
                if (!screen.IsPaused || screen.Buttons.ContainsKey("fight") || screen.Buttons.ContainsKey("reroll"))
                    throw new InvalidOperationException("Pause did not replace match-changing controls.");
                var session = screen.Session!;
                var phase = session.Rounds.Phase; var round = session.Rounds.Position;
                var times = session.Battles.Select(b => b.Combat.Time).ToArray();
                double planning = screen.PlanningRemaining;
                screen.Update(10,10);
                if (session.Rounds.Phase != phase || session.Rounds.Position != round || screen.PlanningRemaining != planning || !times.SequenceEqual(session.Battles.Select(b => b.Combat.Time)))
                    throw new InvalidOperationException("Paused match advanced combat or a countdown.");
                if (keyboardResume) ImGui.GetIO().AddKeyEvent(ImGuiKey.Escape,true); else Click("resume"); break;
            case 5: ImGui.GetIO().AddMouseButtonEvent(0,false); ImGui.GetIO().AddKeyEvent(ImGuiKey.Escape,false); break;
            case 6:
                if (screen.IsPaused) throw new InvalidOperationException("Resume did not restore the match.");
                return true;
        }
        return false;
    }
}

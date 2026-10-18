using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal sealed class GameMenuSmoke
{
    private int step;
    public bool Done { get; private set; }
    public bool Confirming => step is 5 or 11;
    public void Input(MatchScreen screen)
    {
        if (Done) return;
        void Click(string key) { var p = screen.Buttons[key]; ImGui.GetIO().AddMousePosEvent(p.X,p.Y); ImGui.GetIO().AddMouseButtonEvent(0,true); }
        switch (step++)
        {
            case 0: ImGui.GetIO().AddKeyEvent(ImGuiKey.Escape,true); break;
            case 1: ImGui.GetIO().AddKeyEvent(ImGuiKey.Escape,false); break;
            case 2:
                if (screen.IsPaused || screen.Buttons.ContainsKey("fight")) throw new InvalidOperationException("Game menu paused play implicitly or retained match controls.");
                Click("requestLeave"); break;
            case 3: ImGui.GetIO().AddMouseButtonEvent(0,false); break;
            case 4:
                if (screen.Session is null || !screen.Buttons.ContainsKey("confirmLeave")) throw new InvalidOperationException("Leave request ended the match before confirmation.");
                break;
            case 5: Click("resume"); break;
            case 6: ImGui.GetIO().AddMouseButtonEvent(0,false); break;
            case 7:
                if (screen.Session is null || !screen.Buttons.ContainsKey("gameMenu")) throw new InvalidOperationException("Cancel leave did not return to the match.");
                Click("gameMenu"); break;
            case 8: ImGui.GetIO().AddMouseButtonEvent(0,false); break;
            case 9: Click("requestLeave"); break;
            case 10: ImGui.GetIO().AddMouseButtonEvent(0,false); break;
            case 11: Click("confirmLeave"); break;
            case 12: ImGui.GetIO().AddMouseButtonEvent(0,false); break;
            case 13:
                if (screen.Session is not null || !screen.Buttons.ContainsKey("start")) throw new InvalidOperationException("Confirmed leave did not return to the main menu.");
                Done = true; Console.WriteLine("Game menu UI passed: Escape, cancel leave, confirmed leave and return to main menu."); break;
        }
    }
}

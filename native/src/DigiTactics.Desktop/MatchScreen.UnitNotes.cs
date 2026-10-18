using Brutal.ImGuiApi;
using DigiTactics.Presentation;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private readonly UnitNotes unitNotes=new(Path.Combine(AppContext.BaseDirectory,"content/unit-notes.json"));
    internal IReadOnlyList<string> DetailedUnitNotes {get;private set;}=[];
    internal long UnitNoteRenderSerial {get;private set;}
    private void DrawUnitNotes(CombatEntry unit)=>DrawUnitNotes(unitNotes.Describe(Rules.Id,unit));
    private void DrawUnitNotes(string[] notes)
    {
        DetailedUnitNotes=notes;
        UnitNoteRenderSerial++;
        if(notes.Length==0)return;
        ImGui.Separator();
        foreach(string note in notes)UiText.Wrapped(note);
    }
}

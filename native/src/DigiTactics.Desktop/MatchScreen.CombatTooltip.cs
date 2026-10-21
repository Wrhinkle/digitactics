using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;
using DigiTactics.Presentation;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal CombatUnit? DetailedCombatUnit { get; private set; }
    private void CombatTooltip(bool inside, float2 mouse)
    {
        DetailedCombatUnit = null;
        if (battle is null) return;
        var targets = battle.InspectionTargets(Width, Height).ToArray();
        foreach (var target in targets) Buttons["combatUnit/" + target.Unit.Id] = new(Left + target.Point.X, Top + target.Point.Y);
        if (!inside || arenaDragMoved || itemDragMoved || cameraDrag is not null) return;
        var selected=CombatPicking.Pick(targets.Select(target=>new CombatPickTarget(target.Unit.Id,target.Point.X,target.Point.Y)),mouse.X-Left,mouse.Y-Top);
        if(selected is null) return;
        var unit=targets.First(target=>target.Unit.Id==selected).Unit;
        DetailedCombatUnit = unit;
        // tooltipFor with live stats beside the cursor.
        ShowTip(CombatTip(unit),TipSide.Cursor);
    }
}

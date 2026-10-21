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
        BeginInspectionTooltip(390);
        UnitInspectionHeader(unit.Line,unit.Stage,unit.Traits);
        UnitInspectionStats([
            ("HP",$"{unit.Health:0}/{unit.MaxHealth:0}"),("Mana",$"{unit.Mana:0}/{unit.MaxMana:0}"),
            ("AD",$"{unit.AttackDamage:0.#}"),("AS",$"{unit.EffectiveAttackSpeed:0.##}"),
            ("Armor",$"{unit.Armor:0.#}"),("MR",$"{unit.MagicResist:0.#}"),
            ("Range",$"{unit.Range:0.#}"),("AP",$"{unit.AbilityPower:0.#}")]);
        if(unit.Shield>0)CodexText($"Shield {unit.Shield:0}",CodexGold,15);
        var ability = text.Ability(Rules.Id, unit.Line.Id, unit.Stage, unit.AbilityPower);
        UnitInspectionAbility(ability.Name,ability.Description);
        if(ability.ScaledDescription is null)CodexText("Ability values shown at base ability power.",CodexMuted,14);
        ImGui.PushStyleColor(ImGuiCol.Text,CodexAbility);
        DrawUnitNotes(unit.Entry);
        ImGui.PopStyleColor();UnitInspectionItems(unit.Items);
        EndInspectionTooltip();
    }
}

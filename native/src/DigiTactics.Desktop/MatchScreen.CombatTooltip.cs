using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal CombatUnit? DetailedCombatUnit { get; private set; }
    private void CombatTooltip(bool inside, float2 mouse)
    {
        DetailedCombatUnit = null;
        if (battle is null) return;
        var targets = battle.InspectionTargets(Width, Height).OrderBy(target => target.Depth).ToArray();
        foreach (var target in targets) Buttons["combatUnit/" + target.Unit.Id] = new(Left + target.Point.X, Top + target.Point.Y);
        if (!inside || arenaDragMoved || itemDragMoved || cameraDrag is not null) return;
        var hit = targets.FirstOrDefault(target => Math.Abs(mouse.X - Left - target.Point.X) < 30 && Math.Abs(mouse.Y - Top - target.Point.Y) < 25);
        if (hit.Unit is not { } unit) return;
        DetailedCombatUnit = unit;
        ImGui.SetNextWindowSize(new(360,0)); ImGui.BeginTooltip();
        UiText.Wrapped(unit.Line.Forms[unit.Stage] + " " + new string('*', unit.Stage + 1));
        UiText.Wrapped(string.Join(" / ", unit.Traits.Select(Name)));
        UiText.Wrapped($"HP {unit.Health:0}/{unit.MaxHealth:0} / Shield {unit.Shield:0}");
        UiText.Wrapped($"Attack {unit.AttackDamage:0.#} / Speed {unit.EffectiveAttackSpeed:0.##} / Range {unit.Range:0.#}");
        UiText.Wrapped($"Armor {unit.Armor:0.#} / Magic resist {unit.MagicResist:0.#}");
        UiText.Wrapped($"Mana {unit.Mana:0.#}/{unit.MaxMana:0.#} / Ability power {unit.AbilityPower:0.#}");
        var ability = text.Ability(Rules.Id, unit.Line.Id, unit.Stage, unit.AbilityPower);
        ImGui.Separator(); UiText.Wrapped(ability.Name);
        if (ability.ScaledDescription is null) UiText.Wrapped("Ability description at base ability power:"u8);
        UiText.Wrapped(ability.Description);
        foreach (string id in unit.Items)
        {
            ImGui.Separator();
            var position = ImGui.GetCursorScreenPos(); ImGui.Dummy(new(24,24));
            itemArtwork.Draw(Rules.Id,id,position,24);
            ImGui.SameLine(); UiText.Wrapped(Name(id));
            UiText.Wrapped(text.Get(Rules.Id,id).Description);
        }
        ImGui.EndTooltip();
    }
}

using Brutal.ImGuiApi;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal int? DetailedUnit { get; private set; }
    internal IReadOnlyList<string> DetailedItems { get; private set; } = [];
    private void UnitDetails(PlanningUnit unit)
    {
        DetailedUnit = unit.Id; DetailedItems = unit.Items;
        UnitDetails(unit.Line, unit.Stage, unit.Items, unit.BonusTraits,unit.Mods);
    }
    private void UnitDetails(CombatEntry unit)=>UnitDetails(unit.Line,unit.Stage,unit.Items,unit.BonusTraits,notes:unitNotes.Describe(Rules.Id,unit));
    private void UnitDetails(string lineId, int stage, string[]? items = null, Dictionary<string,int>? bonusTraits = null,System.Text.Json.JsonElement? mods=null,string[]? notes=null)
    {
        var line = Rules.Lines.Concat(Rules.ExtraLines).Single(l => l.Id == lineId);
        var stats = UnitValues.Stats(line, stage);
        UiText.Wrapped($"{line.Forms[stage]}  {new string('*', stage + 1)}");
        UiText.Wrapped($"{line.Tier}-cost / {line.Attribute}");
        UiText.Wrapped(string.Join(" -> ", line.Forms));
        var traits = TraitRules.UnitTraits(new(lineId, stage, 0, 0, Items: items), Rules)
            .Concat((bonusTraits ?? []).Keys.Where(id => Rules.Traits.Any(trait => trait.Id == id))).Distinct();
        UiText.Wrapped(string.Join(" / ", traits.Select(Name)));
        UiText.Wrapped($"Base stats: {stats.MaxHealth} HP, {stats.AttackDamage} attack, {stats.AttackSpeed:0.##} attacks/s");
        UiText.Wrapped($"{stats.Armor} armor / {stats.MagicResist} magic resist / {stats.Range} range");
        UiText.Wrapped($"Mana {stats.StartMana}/{stats.MaxMana}");
        var ability = text.Ability(Rules.Id, lineId, stage);
        UiText.Wrapped(ability.Name);
        UiText.Wrapped(ability.Description);
        DrawUnitNotes(notes??unitNotes.Describe(Rules.Id,lineId,mods));
        foreach (string id in items ?? [])
        {
            ImGui.Separator();
            var description = text.Get(Rules.Id, id);
            var position = ImGui.GetCursorScreenPos(); ImGui.Dummy(new(28, 28));
            itemArtwork.Draw(Rules.Id, id, position, 28);
            ImGui.SameLine(); UiText.Wrapped(description.Name);
            UiText.Wrapped(description.Description);
        }
    }
    private void UnitTooltip(PlanningUnit unit)
    {
        if (!ImGui.IsItemHovered()) return;
        ImGui.SetNextWindowSize(new(340, 0)); ImGui.BeginTooltip(); UnitDetails(unit); ImGui.EndTooltip();
    }
}

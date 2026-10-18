using Brutal.ImGuiApi;
using Brutal.Numerics;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal int ItemTooltipRecipes { get; private set; }
    private void ItemTooltip(string id)
    {
        var item = Rules.Items.Single(item => item.Id == id);
        var description = text.Get(Rules.Id, id);
        ImGui.SetNextWindowSize(new(380, 0)); ImGui.BeginTooltip();
        var min = ImGui.GetCursorScreenPos();
        ImGui.Dummy(new(350, 52));
        itemArtwork.Draw(Rules.Id, id, min, 48);
        var draw = ImGui.GetWindowDrawList();
        draw.AddText(new(min.X + 58, min.Y + 5), new ImColor8(235,240,248), description.Name);
        string category = item.Component ? "Component" : item.Consumable ? "Consumable" : item.Emblem is not null ? "Crest" : "Completed item";
        draw.AddText(new(min.X + 58, min.Y + 28), new ImColor8(155,170,190), category);
        UiText.Wrapped(description.Description);
        ItemTooltipRecipes = 0;
        if (item.Component)
        {
            ImGui.SeparatorText("Combines with"u8);
            foreach (var component in Rules.Items.Where(candidate => candidate.Component))
            {
                string key = string.CompareOrdinal(id, component.Id) <= 0 ? id + "+" + component.Id : component.Id + "+" + id;
                if (!Rules.Recipes!.TryGetValue(key, out var result)) continue;
                var position = ImGui.GetCursorScreenPos(); ImGui.Dummy(new(24, 24));
                itemArtwork.Draw(Rules.Id, component.Id, position, 24);
                ImGui.SameLine(); UiText.Wrapped(Name(component.Id) + " -> " + Name(result));
                ItemTooltipRecipes++;
            }
        }
        else
        {
            var recipes = Rules.Recipes!.Where(recipe => recipe.Value == id).Select(recipe => string.Join(" + ", recipe.Key.Split('+').Select(Name))).ToArray();
            if (recipes.Length > 0) { ImGui.Separator(); UiText.Wrapped("Made from " + string.Join(" or ", recipes)); ItemTooltipRecipes = recipes.Length; }
        }
        ImGui.EndTooltip();
    }
}

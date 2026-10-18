using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal IReadOnlyDictionary<string,int> DisplayedTraits { get; private set; } = new Dictionary<string,int>();
    internal bool CombinedTraits { get; private set; }
    internal int TraitTooltipLevels { get; private set; }
    internal IReadOnlyList<int> TraitTooltipUnits { get; private set; } = [];

    private void TraitSidebar(MatchPlayer player)
    {
        TraitTooltipLevels = 0;
        TraitTooltipUnits = [];
        CombinedTraits = Session!.State.Duos && Session.Rounds.RoundType == "pvp" && player.Team is not null;
        var counts = CombinedTraits
            ? Session.CombatProjection.Team(Session.State.Teams[player.Team!.Value]).Effects.Counts!
            : TraitRules.Count(Rules, Session.CombatProjection.Board(player), player.Augments.ToArray());
        var rows = counts.Where(pair => Rules.Traits.Any(trait => trait.Id == pair.Key))
            .Select(pair => { var trait = Rules.Traits.Single(trait => trait.Id == pair.Key); return (Trait: trait, Count: pair.Value, Level: TraitRules.Level(trait, pair.Value)); })
            .OrderByDescending(row => row.Level).ThenByDescending(row => row.Count);
        // Solo uses the source's ID tie-break; combined counts retain their team traversal order.
        var ordered = CombinedTraits ? rows.ToArray() : rows.ThenBy(row => row.Trait.Id, StringComparer.Ordinal).ToArray();
        DisplayedTraits = ordered.ToDictionary(row => row.Trait.Id, row => row.Count);
        UiText.Wrapped((CombinedTraits ? "Combined team traits" : "Traits") + $" - {ordered.Count(row => row.Level > 0)} active");
        foreach (var row in ordered)
        {
            var color = row.Level == 0 ? new ImColor8(140,150,165) : row.Level >= row.Trait.Breakpoints.Length ? new ImColor8(255,207,74)
                : row.Level >= 2 ? new ImColor8(201,209,220) : new ImColor8(205,151,101);
            int next = row.Trait.Breakpoints.FirstOrDefault(point => point > row.Count);
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            UiText.Wrapped($"{Name(row.Trait.Id)}  {(next > 0 ? $"{row.Count}/{next}" : row.Count.ToString())}");
            ImGui.PopStyleColor();
            var min = ImGui.GetItemRectMin(); var max = ImGui.GetItemRectMax();
            Buttons["trait/" + row.Trait.Id] = new((min.X + max.X) / 2, (min.Y + max.Y) / 2);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetNextWindowSize(new(340,0)); ImGui.BeginTooltip();
                ImGui.Text(Name(row.Trait.Id));
                ImGui.Text("Breakpoints: " + string.Join(" / ", row.Trait.Breakpoints));
                UiText.Wrapped(text.Get(Rules.Id, row.Trait.Id).Description);
                ImGui.Separator();
                for (int i = 0; i < row.Trait.Breakpoints.Length; i++)
                {
                    var effect = text.TraitBreakpoint(Rules.Id, row.Trait.Id, i);
                    ImGui.PushStyleColor(ImGuiCol.Text, i < row.Level ? new ImColor8(255,220,140) : new ImColor8(145,155,170));
                    UiText.Wrapped($"({effect.Name}) {effect.Description}");
                    ImGui.PopStyleColor();
                    TraitTooltipLevels++;
                }
                var board = CombinedTraits ? Session.CombatProjection.Team(Session.State.Teams[player.Team!.Value]).Units : Session.CombatProjection.Board(player);
                var contributors = TraitContributors.Select(Rules, board, row.Trait.Id, CombinedTraits);
                TraitTooltipUnits = contributors.Select(unit => unit.Uid).ToArray();
                if (contributors.Length > 0) ImGui.SeparatorText("On the board"u8);
                foreach (var unit in contributors)
                {
                    var position = ImGui.GetCursorScreenPos(); ImGui.Dummy(new(32,32));
                    portraits.Draw(Rules.Id, unit.Line, unit.Stage, position, 32);
                    ImGui.SameLine();
                    var line = Rules.Lines.Concat(Rules.ExtraLines).Single(line => line.Id == unit.Line);
                    UiText.Wrapped(line.Forms[unit.Stage] + " " + new string('*', unit.Stage + 1));
                }
                ImGui.EndTooltip();
            }
        }
        if (ordered.Length == 0) UiText.Wrapped("Place Digimon on the board to activate traits."u8);
    }
}

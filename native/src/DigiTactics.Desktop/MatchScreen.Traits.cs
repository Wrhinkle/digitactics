using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal IReadOnlyDictionary<string,int> DisplayedTraits { get; private set; } = new Dictionary<string,int>();
    internal int InlineTraitEffects { get; private set; }
    internal int InlineTraitPortraits { get; private set; }
    internal bool CombinedTraits { get; private set; }
    internal int TraitTooltipLevels { get; private set; }
    internal IReadOnlyList<int> TraitTooltipUnits { get; private set; } = [];
    internal (float2 Min,float2 Max) TraitCardsBounds { get; private set; }

    private void TraitSidebar(MatchPlayer player)
    {
        InlineTraitEffects = 0;
        InlineTraitPortraits = 0;
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
        HudText(CombinedTraits ? "TEAM TRAITS" : "TRAITS",22);
        HudText($"{ordered.Count(row => row.Level > 0)} active"+(player.Id==Human.Id?"":" / "+player.Name),16,HudMuted);
        var board = CombinedTraits ? Session.CombatProjection.Team(Session.State.Teams[player.Team!.Value]).Units : Session.CombatProjection.Board(player);
        // Keep long trait lists from pushing combat controls out of the sidebar.
        ImGui.BeginChild("Trait cards"u8, new(0, Math.Clamp(Height * .55f, 160, 360)));
        TraitCardsBounds=(ImGui.GetWindowDrawList().GetClipRectMin(),ImGui.GetWindowDrawList().GetClipRectMax());
        foreach (var row in ordered)
        {
            var contributors = TraitContributors.Select(Rules, board, row.Trait.Id, CombinedTraits);
            var color = row.Level == 0 ? new ImColor8(140,150,165) : row.Level >= row.Trait.Breakpoints.Length ? new ImColor8(255,207,74)
                : row.Level >= 2 ? new ImColor8(201,209,220) : new ImColor8(205,151,101);
            int next = row.Trait.Breakpoints.FirstOrDefault(point => point > row.Count);
            ImGui.BeginGroup();
            float width=ImGui.GetContentRegionAvail().X;
            bool open=HudButton("trait/"+row.Trait.Id,"",width,44,row.Level>0);
            var min = ImGui.GetItemRectMin(); var max = ImGui.GetItemRectMax();
            var draw=ImGui.GetWindowDrawList();var badge=min+new float2(8,6);
            draw.AddRectFilled(badge+new float2(0,7),badge+new float2(28,25),color);
            draw.AddTriangleFilled(badge+new float2(0,7),badge+new float2(14,0),badge+new float2(28,7),color);
            draw.AddTriangleFilled(badge+new float2(0,25),badge+new float2(28,25),badge+new float2(14,32),color);
            HudClipText(draw,badge+new float2(7,8),22,row.Count.ToString(),new ImColor8(16,19,26));
            HudClipText(draw,min+new float2(44,13),width-92,Name(row.Trait.Id),row.Level>0?HudInk:HudMuted);
            HudClipText(draw,new(max.X-42,min.Y+13),36,next>0?$"/{next}":"MAX",color);
            if(open)OpenCodexPanel(row.Trait.Id);
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
            for (int i = 0; i < row.Trait.Breakpoints.Length; i++)
            {
                if (i > 0) ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Text, i < row.Level ? color : new ImColor8(140,150,165));
                ImGui.PushFont(default,16);ImGui.Text(row.Trait.Breakpoints[i].ToString());ImGui.PopFont();
                ImGui.PopStyleColor();
            }
            if (row.Level > 0)
            {
                HudText(text.TraitBreakpoint(Rules.Id, row.Trait.Id, row.Level - 1).Description,16,HudMuted);
                InlineTraitEffects++;
            }
            int columns = Math.Max(1, (int)(ImGui.GetContentRegionAvail().X / 36));
            for (int i = 0; row.Level>0&&i < contributors.Length; i++)
            {
                if (i % columns != 0) ImGui.SameLine(0, 4);
                var unit = contributors[i];
                var position = ImGui.GetCursorScreenPos(); ImGui.Dummy(new(32,32));
                portraits.Draw(Rules.Id, unit.Line, unit.Stage, position, 32);
                InlineTraitPortraits++;
                Buttons[$"traitUnit/{row.Trait.Id}/{unit.Uid}"]=position+new float2(16,16);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetNextWindowSize(new(340,0)); ImGui.BeginTooltip();
                    UnitDetails(unit);
                    ImGui.EndTooltip();
                }
            }
            ImGui.EndGroup();ImGui.Spacing();
        }
        if (ordered.Length == 0) UiText.Wrapped("Place Digimon on the board to activate traits."u8);
        ImGui.EndChild();
    }
}

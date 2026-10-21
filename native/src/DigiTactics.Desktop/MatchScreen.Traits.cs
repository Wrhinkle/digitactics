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
        HudText(CombinedTraits ? "TEAM TRAITS" : "TRAITS",15,HudMuted);
        ImGui.SameLine();HudText($"{ordered.Count(row => row.Level > 0)} active",13,HudMuted);
        var board = CombinedTraits ? Session.CombatProjection.Team(Session.State.Teams[player.Team!.Value]).Units : Session.CombatProjection.Board(player);
        // Keep long trait lists from pushing combat controls out of the sidebar.
        ImGui.BeginChild("Trait cards"u8, new(0, Math.Max(160,ImGui.GetContentRegionAvail().Y-96)));
        TraitCardsBounds=(ImGui.GetWindowDrawList().GetClipRectMin(),ImGui.GetWindowDrawList().GetClipRectMax());
        foreach (var row in ordered)
        {
            var contributors = TraitContributors.Select(Rules, board, row.Trait.Id, CombinedTraits);
            var color = row.Level == 0 ? new ImColor8(140,150,165) : row.Level >= row.Trait.Breakpoints.Length ? new ImColor8(255,207,74)
                : row.Level >= 2 ? new ImColor8(201,209,220) : new ImColor8(205,151,101);
            int next = row.Trait.Breakpoints.FirstOrDefault(point => point > row.Count);
            ImGui.BeginGroup();
            float width=ImGui.GetContentRegionAvail().X;
            int columns=Math.Max(1,(int)((width-14)/27));
            int breakpointColumns=Math.Max(1,(int)((width-52)/26));
            int breakpointRows=(row.Trait.Breakpoints.Length+breakpointColumns-1)/breakpointColumns;
            float effectY=34+breakpointRows*21,portraitY=effectY+20;
            float cardHeight=row.Level>0?portraitY+MathF.Ceiling((float)contributors.Length/columns)*27+3:effectY;
            bool open=HudButton("trait/"+row.Trait.Id,"",width,cardHeight);
            var min=ImGui.GetItemRectMin();var max=ImGui.GetItemRectMax();
            var draw=ImGui.GetWindowDrawList();var badge=min+new float2(7,7);
            draw.AddRectFilled(min,max,ImGui.IsItemHovered()?new ImColor8(21,30,49):new ImColor8(15,22,37),6);
            draw.AddRect(min,max,HudLine,6);
            var badgeColor=row.Level>0?color:new ImColor8(42,49,66);
            draw.AddRectFilled(badge+new float2(0,8),badge+new float2(30,27),badgeColor);
            draw.AddTriangleFilled(badge+new float2(0,8),badge+new float2(15,0),badge+new float2(30,8),badgeColor);
            draw.AddTriangleFilled(badge+new float2(0,27),badge+new float2(30,27),badge+new float2(15,35),badgeColor);
            HudClipText(draw,badge+new float2(8,9),22,row.Count.ToString(),row.Level>0?new ImColor8(16,19,26):HudMuted,16);
            HudClipText(draw,min+new float2(45,7),width-89,Name(row.Trait.Id),row.Level>0?HudInk:HudMuted,16);
            HudClipText(draw,new(max.X-40,min.Y+8),34,next>0?$"{row.Count}/{next}":row.Count.ToString(),color,14);
            float chipX=min.X+45;
            for(int i=0;i<row.Trait.Breakpoints.Length;i++)
            {
                var chip=new float2(chipX+i%breakpointColumns*26,min.Y+31+i/breakpointColumns*21);string label=row.Trait.Breakpoints[i].ToString();
                draw.AddRectFilled(chip,chip+new float2(22,17),i<row.Level?color:new ImColor8(34,42,59),3);
                HudClipText(draw,chip+new float2(5,1),18,label,i<row.Level?new ImColor8(16,19,26):HudMuted,13);
            }
            if(row.Level>0)
            {
                HudClipText(draw,min+new float2(7,effectY),width-14,text.TraitBreakpoint(Rules.Id,row.Trait.Id,row.Level-1).Description,HudGold,13);
                InlineTraitEffects++;
            }
            if(open)OpenCodexPanel(row.Trait.Id);
            if (ImGui.IsItemHovered() && (row.Level==0||ImGui.GetMousePos().Y<min.Y+portraitY))
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
            for(int i=0;row.Level>0&&i<contributors.Length;i++)
            {
                var unit=contributors[i];var position=min+new float2(7+i%columns*27,portraitY+i/columns*27);
                portraits.Draw(Rules.Id,unit.Line,unit.Stage,position,23);InlineTraitPortraits++;
                Buttons[$"traitUnit/{row.Trait.Id}/{unit.Uid}"]=position+new float2(11.5f,11.5f);
                if(ImGui.IsWindowHovered()&&ImGui.IsMouseHoveringRect(position,position+new float2(23,23)))
                {
                    ImGui.SetNextWindowSize(new(340,0));ImGui.BeginTooltip();UnitDetails(unit);ImGui.EndTooltip();
                }
            }
            ImGui.EndGroup();ImGui.Spacing();
        }
        if (ordered.Length == 0) UiText.Wrapped("Place Digimon on the board to activate traits."u8);
        ImGui.EndChild();
    }
}

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
        // .ph heading, then .trait rows: hex badge, name and count, breakpoint chips and effect, contributors.
        var headerAt=ImGui.GetCursorScreenPos();float railWidth=ImGui.GetContentRegionAvail().X;
        string heading=CombinedTraits?"Combined team traits":player.Id==localSeat||localSeat<0?"Traits":player.Name+"'s traits";
        float headerHeight=HudSectionHeader(ImGui.GetWindowDrawList(),headerAt.X,headerAt.Y,railWidth,heading,$"{ordered.Count(row => row.Level > 0)} active");
        ImGui.Dummy(new(railWidth,headerHeight));
        var board = CombinedTraits ? Session.CombatProjection.Team(Session.State.Teams[player.Team!.Value]).Units : Session.CombatProjection.Board(player);
        // Keep long trait lists from pushing combat controls out of the sidebar.
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(0,0));
        ImGui.PushStyleColor(ImGuiCol.ChildBg,new ImColor8(0,0,0,0));
        ImGui.BeginChild("Trait cards"u8, new(0, Math.Max(160,ImGui.GetContentRegionAvail().Y-(selectedUnit is null?0:96))));
        ImGui.PopStyleColor();
        TraitCardsBounds=(ImGui.GetWindowDrawList().GetClipRectMin(),ImGui.GetWindowDrawList().GetClipRectMax());
        var codexTraits=codexCatalog.Value.Variant(Rules.Id).Traits;
        foreach (var row in ordered)
        {
            var contributors = TraitContributors.Select(Rules, board, row.Trait.Id, CombinedTraits);
            var tierColor = row.Level == 0 ? HudHex("2a3142") : row.Level >= row.Trait.Breakpoints.Length ? CssGold : row.Level >= 2 ? HudHex("c8d2e0") : HudHex("c98a52");
            var traitColor = codexTraits.FirstOrDefault(t => t.Id == row.Trait.Id) is {} codexTrait ? HudCss(codexTrait.Color) : HudHex("8f9ab0");
            int next = row.Trait.Breakpoints.FirstOrDefault(point => point > row.Count);
            bool active=row.Level>0;
            float width=ImGui.GetContentRegionAvail().X;
            int columns=Math.Max(1,(int)((width-16+3)/25));
            float padY=active?6:4;
            float portraitRows=active&&contributors.Length>0?MathF.Ceiling((float)contributors.Length/columns):0;
            float bodyHeight=34.4f+(portraitRows>0?2+3+portraitRows*22+(portraitRows-1)*3:0);
            float cardHeight=bodyHeight+2*padY+2;
            var min=HudSnap(ImGui.GetCursorScreenPos());var max=min+new float2(width,cardHeight);ImGui.SetCursorScreenPos(min);
            bool open=ImGui.InvisibleButton("trait/"+row.Trait.Id,new(width,cardHeight));
            Buttons["trait/"+row.Trait.Id]=(min+max)*.5f;
            bool hovered=ImGui.IsItemHovered();
            var draw=ImGui.GetWindowDrawList();
            draw.AddRectFilled(min,max,hovered?CssPanel2:CssPanel,8);
            draw.AddRect(min,max,hovered?CssLine2:CssLine,8);
            // .tb: 32x34 pointy hexagon, a 3px frame in the tier colour around the trait colour.
            var badge=min+new float2(1+7,1+padY+.2f);
            if(active&&row.Level>=row.Trait.Breakpoints.Length)HudRadial(draw,badge+new float2(16,17),25,CssGold,.45f,8);
            HudHexagon(draw,badge,new(32,34),tierColor);
            HudHexagon(draw,badge+new float2(3,3),new(26,28),active?HudMix(traitColor,tierColor,.95f):HudHex("1b2130"));
            HudRunCentered(draw,HudWeight(900),15,badge.X+16,badge.Y,34,row.Count.ToString(),active?CssInk:HudHex("aab3c5"));
            float textX=min.X+1+7+34+8,textRight=max.X-1-7;
            string count=next>0?$"{row.Count}/{next}":row.Count.ToString();
            float countWidth=HudRunRight(draw,HudWeight(active?800:600),13,textRight,badge.Y,17.55f,count,active?CssText:CssMuted);
            HudRun(draw,HudWeight(700),13,textX,badge.Y,17.55f,Name(row.Trait.Id),active?CssText:HudHex("aab3c5"),textRight-textX-countWidth-6);
            float chipX=textX,chipY=badge.Y+17.55f+2;
            for(int i=0;i<row.Trait.Breakpoints.Length;i++)
            {
                string label=row.Trait.Breakpoints[i].ToString();bool on=i<row.Level;
                float chipWidth=HudWidth(HudWeight(700),11,label)+8;
                draw.AddRectFilled(new(chipX,chipY),new(chipX+chipWidth,chipY+14.85f),on?traitColor:HudHex("222a3b"),3);
                HudRun(draw,HudWeight(700),11,chipX+4,chipY,14.85f,label,on?CssInk:HudHex("8691a6"));
                chipX+=chipWidth+3;
            }
            if(active)
            {
                HudRun(draw,HudWeight(400),11,chipX+3,chipY,14.85f,text.TraitBreakpoint(Rules.Id,row.Trait.Id,row.Level-1).Description,CssGold,Math.Max(0,textRight-chipX-3));
                InlineTraitEffects++;
            }
            float portraitY=padY+1+34.4f+2+3;
            if(open&&(!active||ImGui.GetMousePos().Y<min.Y+portraitY))OpenCodexPanel(row.Trait.Id);
            if (hovered && (!active||ImGui.GetMousePos().Y<min.Y+portraitY))
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
            // .tu: 22px portraits with a 1px tier border.
            for(int i=0;active&&i<contributors.Length;i++)
            {
                var unit=contributors[i];var position=min+new float2(1+7+i%columns*25,portraitY+i/columns*25);
                var unitLine=Rules.Lines.Concat(Rules.ExtraLines).Single(l=>l.Id==unit.Line);
                HudPortraitTile(draw,position,22,FamilyColor(unit.Line),4);
                portraits.Draw(Rules.Id,unit.Line,unit.Stage,position,22);InlineTraitPortraits++;
                draw.AddRect(position,position+new float2(22,22),tierColors[Math.Clamp(unitLine.Tier,0,5)],4);
                Buttons[$"traitUnit/{row.Trait.Id}/{unit.Uid}"]=position+new float2(11,11);
                if(ImGui.IsWindowHovered()&&ImGui.IsMouseHoveringRect(position,position+new float2(22,22)))
                {
                    ImGui.SetNextWindowSize(new(340,0));ImGui.BeginTooltip();UnitDetails(unit);ImGui.EndTooltip();
                }
            }
            ImGui.Dummy(new(width,active?4:3));
        }
        if (ordered.Length == 0)
        {
            // .emptyhint: 13px muted, padding 6px 2px.
            var at=ImGui.GetCursorScreenPos();float width=ImGui.GetContentRegionAvail().X-4;
            var lines=WrapHud(HudWeight(400),13,"Place Digimon on the board to activate traits.",width);
            for(int i=0;i<lines.Length;i++)HudRun(ImGui.GetWindowDrawList(),HudWeight(400),13,at.X+2,at.Y+6+i*17.55f,17.55f,lines[i],CssMuted);
            ImGui.Dummy(new(width,12+lines.Length*17.55f));
        }
        ImGui.EndChild();
        ImGui.PopStyleVar();
    }
}

using Brutal.ImGuiApi;
using Brutal.Numerics;
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
        var traits = TraitRules.UnitTraits(new(lineId, stage, 0, 0, Items: items), Rules)
            .Concat((bonusTraits ?? []).Keys.Where(id => Rules.Traits.Any(trait => trait.Id == id))).Distinct();
        ImGui.PushFont(default,16);ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(6,5));
        UnitInspectionHeader(line,stage,traits);
        UnitInspectionStats([
            ("HP",$"{stats.MaxHealth:0}"),("Mana",$"{stats.StartMana:0}/{stats.MaxMana:0}"),
            ("AD",$"{stats.AttackDamage:0.#}"),("AS",$"{stats.AttackSpeed:0.##}"),
            ("Armor",$"{stats.Armor:0.#}"),("MR",$"{stats.MagicResist:0.#}"),("Range",$"{stats.Range:0.#}")]);
        var ability = text.Ability(Rules.Id, lineId, stage);
        UnitInspectionAbility(ability.Name,ability.Description);
        ImGui.PushStyleColor(ImGuiCol.Text,CodexAbility);
        DrawUnitNotes(notes??unitNotes.Describe(Rules.Id,lineId,mods));
        ImGui.PopStyleColor();
        UnitInspectionItems(items??[]);
        ImGui.PopStyleVar();ImGui.PopFont();
    }
    // The original tooltip is a portrait header, trait chips, paired stat rows,
    // then the ability and equipped items. Keep this hierarchy in every caller
    // (shop, board, bench, carousel and the selected-unit sidebar).
    private void UnitInspectionHeader(UnitLine line,int stage,IEnumerable<string> traits)
    {
        var variant=codexCatalog.Value.Variant(Rules.Id);var entry=variant.Lines.FirstOrDefault(value=>value.Id==line.Id);
        var at=ImGui.GetCursorScreenPos();const float portraitSize=52;
        ImGui.Dummy(new(portraitSize,portraitSize));portraits.Draw(Rules.Id,line.Id,stage,at,portraitSize);
        ImGui.GetWindowDrawList().AddRect(at,at+new float2(portraitSize,portraitSize),entry is null?CodexGold:CodexColor(entry.Color),5,ImDrawFlags.None,2);
        ImGui.SameLine(0,10);ImGui.BeginGroup();
        CodexHeading(line.Forms[stage],20);
        string stageName=entry?.Forms.ElementAtOrDefault(stage)?.StageName??$"{stage+1}-star";
        CodexText($"{stageName} · {line.Tier}-cost",CodexMuted,14);
        ImGui.SameLine(0,4);UnitInspectionStars(stage+1);ImGui.EndGroup();
        CodexText(string.Join(" → ",line.Forms),CodexMuted,14);
        float right=ImGui.GetCursorScreenPos().X+ImGui.GetContentRegionAvail().X;bool first=true;
        foreach(string id in traits)
        {
            var trait=variant.Traits.FirstOrDefault(value=>value.Id==id);string name=trait?.Name??Name(id);
            InspectionChip(name,trait is null?CodexMuted:CodexColor(trait.Color),ref first,right);
        }
        InspectionChip(line.Attribute,entry is null?CodexGold:CodexColor(entry.AttributeColor),ref first,right);
        ImGui.Spacing();
    }
    private static void InspectionChip(string value,ImColor8 color,ref bool first,float right)
    {
        ImGui.PushFont(default,14);float width=ImGui.CalcTextSize(value).X+14;ImGui.PopFont();
        if(!first&&ImGui.GetItemRectMax().X+5+width<=right)ImGui.SameLine(0,5);
        CodexPill(value,color);first=false;
    }
    private static void UnitInspectionStars(int count)
    {
        var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(count*16,14));var draw=ImGui.GetWindowDrawList();
        for(int star=0;star<count;star++)
        {
            var center=at+new float2(star*16+7,7);
            for(int point=0;point<10;point++)
            {
                float angle=(point*36-90)*MathF.PI/180,next=((point+1)*36-90)*MathF.PI/180;
                float radius=point%2==0?7:3.2f,nextRadius=point%2==0?3.2f:7;
                draw.AddTriangleFilled(center,center+new float2(MathF.Cos(angle),MathF.Sin(angle))*radius,center+new float2(MathF.Cos(next),MathF.Sin(next))*nextRadius,CodexGold);
            }
        }
    }
    private static void UnitInspectionStats((string Label,string Value)[] stats)
    {
        // A narrow selected-unit sidebar gets one pair per row. Hover cards
        // retain the old two-pair table, with values aligned for quick reading.
        int columns=ImGui.GetContentRegionAvail().X>=240?4:2;
        ImGui.PushFont(default,15);ImGui.PushStyleVar(ImGuiStyleVar.CellPadding,new float2(2,2));
        if(ImGui.BeginTable("Unit inspection stats"u8,columns,ImGuiTableFlags.SizingStretchSame))
        {
            foreach(var stat in stats)
            {
                ImGui.TableNextColumn();CodexText(stat.Label,CodexMuted,15);
                ImGui.TableNextColumn();float valueWidth=ImGui.CalcTextSize(stat.Value).X;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX()+Math.Max(0,ImGui.GetContentRegionAvail().X-valueWidth));
                ImGui.PushFont(NativeUiTheme.DisplayBoldFont,15);ImGui.PushStyleColor(ImGuiCol.Text,CodexInk);
                ImGui.Text(stat.Value);ImGui.PopStyleColor();ImGui.PopFont();
            }
            ImGui.EndTable();
        }
        ImGui.PopStyleVar();ImGui.PopFont();
    }
    private static void UnitInspectionAbility(string name,string description)
    {
        ImGui.Separator();CodexHeading(name,18,new ImColor8(63,214,255));CodexText(description,CodexBody,16);
    }
    private void UnitInspectionItems(IEnumerable<string> items)
    {
        foreach(string id in items)
        {
            ImGui.Separator();var description=text.Get(Rules.Id,id);
            var at=ImGui.GetCursorScreenPos();ImGui.Dummy(new(28,28));itemArtwork.Draw(Rules.Id,id,at,28);
            ImGui.SameLine(0,8);ImGui.BeginGroup();CodexHeading(description.Name,16);CodexText(description.Description,CodexMuted,15);ImGui.EndGroup();
        }
    }
    private static void BeginInspectionTooltip(float width,ImColor8? border=null,bool itemCard=false)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(12,10));ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(6,5));
        ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding,itemCard?8:0);ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize,itemCard?2:1);
        ImGui.PushStyleColor(ImGuiCol.PopupBg,itemCard?new ImColor8(22,29,44):CodexPanel);ImGui.PushStyleColor(ImGuiCol.Border,border??new ImColor8(120,90,40));
        ImGui.SetNextWindowSize(new(width,0));ImGui.BeginTooltip();
    }
    private static void EndInspectionTooltip()
    {
        ImGui.EndTooltip();ImGui.PopStyleColor(2);ImGui.PopStyleVar(4);
    }
    private void UnitTooltip(PlanningUnit unit)
    {
        if (!ImGui.IsItemHovered()) return;
        BeginInspectionTooltip(360);UnitDetails(unit);EndInspectionTooltip();
    }
}

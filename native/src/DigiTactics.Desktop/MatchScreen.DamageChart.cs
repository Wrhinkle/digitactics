using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private int chartMetric;
    internal int ChartMetric => chartMetric;
    internal CombatSimulation? ChartCombat => battle?.Combat;
    internal IReadOnlyList<int> ChartUnits { get; private set; } = [];
    private static readonly string[] chartLabels = ["Dealt", "Taken", "Healed"];
    private static readonly ImColor8[] chartColors = [new(255,179,90), new(138,151,180), new(90,223,138)];

    private void DamageChart()
    {
        ChartUnits = [];
        if (battle is null) return;
        ImGui.PushStyleColor(ImGuiCol.ChildBg,new ImColor8(15,22,37));
        ImGui.PushStyleColor(ImGuiCol.Border,HudLine);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,new float2(3,4));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,new float2(8,8));
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding,8);
        ImGui.BeginChild("Damage chart"u8,new(0,0),ImGuiChildFlags.Borders|ImGuiChildFlags.AutoResizeY);
        ImGui.PopStyleColor(2);ImGui.PopStyleVar(2);
        HudText("DAMAGE",15,HudMuted);ImGui.SameLine();HudText("this fight",13,HudMuted);
        for (int i = 0; i < chartLabels.Length; i++)
        {
            if (i > 0) ImGui.SameLine();
            ImGui.PushFont(default,14);
            ImGui.PushStyleColor(ImGuiCol.Button,new ImColor8(24,37,56));
            ImGui.PushStyleColor(ImGuiCol.Border,chartMetric==i?new ImColor8(63,214,255):HudLine);
            ImGui.PushStyleColor(ImGuiCol.Text,HudInk);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,1);ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,4);
            if (Button("chart/" + i, chartLabels[i],(ImGui.GetContentRegionAvail().X-(2-i)*3)/(3-i),22)) chartMetric = i;
            ImGui.PopStyleVar(2);ImGui.PopStyleColor(3);ImGui.PopFont();
        }
        double Value(CombatUnit unit) => chartMetric switch { 1 => unit.Taken, 2 => unit.Healed, _ => unit.Dealt };
        var units = battle.Combat.Units.Where(u => u.Team == (battle.Mirrored ? 1 : 0)).OrderByDescending(Value).ToArray();
        ChartUnits = units.Select(u => u.Id).ToArray();
        double maximum = Math.Max(1, units.Select(Value).DefaultIfEmpty().Max());
        float width = ImGui.GetContentRegionAvail().X;
        foreach (var unit in units)
        {
            var min = ImGui.GetCursorScreenPos();
            ImGui.Dummy(new(width, 24));
            bool hovered = ImGui.IsItemHovered();
            var draw = ImGui.GetWindowDrawList();
            var ink = unit.Alive ? new ImColor8(235,240,248) : new ImColor8(135,145,160);
            portraits.Draw(Rules.Id, unit.Line.Id, unit.Stage, min, 22, unit.Alive ? (byte)255 : (byte)100);
            string value = UnitValues.Round(Value(unit)).ToString(System.Globalization.CultureInfo.InvariantCulture);
            ImGui.PushFont(default,14);float valueWidth = ImGui.CalcTextSize(value).X;ImGui.PopFont();
            HudClipText(draw,min+new float2(28,0),width-valueWidth-36,unit.Line.Forms[unit.Stage],ink,14);
            HudClipText(draw,min+new float2(width-valueWidth,4),valueWidth+1,value,ink,14);
            float barWidth = Math.Max(1,width-28-valueWidth-8);
            draw.AddRectFilled(new(min.X+28,min.Y+15),new(min.X+28+barWidth,min.Y+22),new ImColor8(40,50,65),3);
            draw.AddRectFilled(new(min.X+28,min.Y+15),new(min.X+28+barWidth*(float)(Value(unit)/maximum),min.Y+22),chartColors[chartMetric],3);
            if (hovered)
            {
                ImGui.BeginTooltip(); ImGui.Text(unit.Line.Forms[unit.Stage]);
                ImGui.Text($"Dealt {UnitValues.Round(unit.Dealt)} / Taken {UnitValues.Round(unit.Taken)} / Healed {UnitValues.Round(unit.Healed)}");
                ImGui.EndTooltip();
            }
        }
        ImGui.EndChild();ImGui.PopStyleVar();
    }
}

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
        ImGui.SeparatorText("Damage - this fight"u8);
        for (int i = 0; i < chartLabels.Length; i++)
        {
            if (i > 0) ImGui.SameLine();
            if (Button("chart/" + i, chartLabels[i], 55, 23, chartMetric == i)) chartMetric = i;
        }
        double Value(CombatUnit unit) => chartMetric switch { 1 => unit.Taken, 2 => unit.Healed, _ => unit.Dealt };
        var units = battle.Combat.Units.Where(u => u.Team == (battle.Mirrored ? 1 : 0)).OrderByDescending(Value).ToArray();
        ChartUnits = units.Select(u => u.Id).ToArray();
        double maximum = Math.Max(1, units.Select(Value).DefaultIfEmpty().Max());
        float width = ImGui.GetContentRegionAvail().X;
        foreach (var unit in units)
        {
            var min = ImGui.GetCursorScreenPos();
            ImGui.Dummy(new(width, 35));
            bool hovered = ImGui.IsItemHovered();
            var draw = ImGui.GetWindowDrawList();
            var ink = unit.Alive ? new ImColor8(235,240,248) : new ImColor8(135,145,160);
            portraits.Draw(Rules.Id, unit.Line.Id, unit.Stage, min, 32, unit.Alive ? (byte)255 : (byte)100);
            string value = UnitValues.Round(Value(unit)).ToString(System.Globalization.CultureInfo.InvariantCulture);
            float valueWidth = ImGui.CalcTextSize(value).X;
            draw.PushClipRect(new(min.X+36,min.Y), new(min.X+width-valueWidth-5,min.Y+18),true);
            draw.AddText(ImGui.GetFont(),12,new(min.X+36,min.Y+2),ink,unit.Line.Forms[unit.Stage]);
            draw.PopClipRect();
            draw.AddText(new(min.X+width-valueWidth,min.Y+1),ink,value);
            float barWidth = Math.Max(1,width-36);
            draw.AddRectFilled(new(min.X+36,min.Y+23),new(min.X+width,min.Y+29),new ImColor8(40,50,65));
            draw.AddRectFilled(new(min.X+36,min.Y+23),new(min.X+36+barWidth*(float)(Value(unit)/maximum),min.Y+29),chartColors[chartMetric]);
            if (hovered)
            {
                ImGui.BeginTooltip(); ImGui.Text(unit.Line.Forms[unit.Stage]);
                ImGui.Text($"Dealt {UnitValues.Round(unit.Dealt)} / Taken {UnitValues.Round(unit.Taken)} / Healed {UnitValues.Round(unit.Healed)}");
                ImGui.EndTooltip();
            }
        }
    }
}

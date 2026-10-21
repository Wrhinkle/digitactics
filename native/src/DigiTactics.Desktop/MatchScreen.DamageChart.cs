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
    private static readonly Rgb[] chartColors = [HudHex("ffb35a"), HudHex("8a97b4"), HudHex("5adf8a")];

    // #dmgChart: a panel card with the .ph heading, three tabs and one row per unit (22px portrait, name, bar, value).
    private void DamageChart()
    {
        ChartUnits = [];
        if (battle is null) return;
        double Value(CombatUnit unit) => chartMetric switch { 1 => unit.Taken, 2 => unit.Healed, _ => unit.Dealt };
        var units = battle.Combat.Units.Where(u => u.Team == (battle.Mirrored ? 1 : 0)).OrderByDescending(Value).ToArray();
        ChartUnits = units.Select(u => u.Id).ToArray();
        double maximum = Math.Max(1, units.Select(Value).DefaultIfEmpty().Max());
        ImGui.Dummy(new(0,10-ImGui.GetStyle().ItemSpacing.Y));
        var min=HudSnap(ImGui.GetCursorScreenPos());float width=ImGui.GetContentRegionAvail().X;
        float header=2+11*1.35f+6,tabs=11*1.35f+4+2,rowsHeight=units.Length*(22+4);
        float height=1+8+header+tabs+6+rowsHeight-(units.Length>0?4:0)+8+1;
        var max=min+new float2(width,height);
        var draw=ImGui.GetWindowDrawList();
        draw.AddRectFilled(min,max,CssPanel,8);draw.AddRect(min,max,CssLine,8);
        float x=min.X+9,inner=width-18,y=min.Y+9;
        y+=HudSectionHeader(draw,x,y,inner,"Damage","this fight");
        float tabWidth=(inner-6)/3;
        for (int i = 0; i < chartLabels.Length; i++)
        {
            var tabMin=new float2(x+i*(tabWidth+3),y);var tabMax=tabMin+new float2(tabWidth,tabs);
            ImGui.SetCursorScreenPos(tabMin);
            if(ImGui.InvisibleButton("chart/"+i,tabMax-tabMin))chartMetric=i;
            Buttons["chart/"+i]=(tabMin+tabMax)*.5f;
            bool on=chartMetric==i,hovered=ImGui.IsItemHovered();
            draw.AddRectFilled(tabMin,tabMax,on||hovered?CssPanel3:CssPanel2,4);
            draw.AddRect(tabMin,tabMax,on||hovered?HudHex("3fd6ff"):CssLine2,4);
            HudRunCentered(draw,HudWeight(400),11,(tabMin.X+tabMax.X)/2,tabMin.Y+3,11*1.35f,chartLabels[i],on?HudHex("ffffff"):CssText);
        }
        y+=tabs+6;
        foreach (var unit in units)
        {
            var row=new float2(x,y);
            ImGui.SetCursorScreenPos(row);ImGui.Dummy(new(inner,22));
            bool hovered = ImGui.IsItemHovered();
            float alpha=unit.Alive?1:.5f;
            HudPortraitTile(draw,row,22,HudOver(FamilyColor(unit.Line.Id),CssPanel,alpha),4);
            portraits.Draw(Rules.Id, unit.Line.Id, unit.Stage, row, 22, unit.Alive ? (byte)255 : (byte)128);
            string value = UnitValues.Round(Value(unit)).ToString(System.Globalization.CultureInfo.InvariantCulture);
            HudRunRight(draw,HudWeight(700),12,row.X+inner,row.Y,22,value,HudOver(CssText,CssPanel,alpha));
            float middle=row.X+22+6,middleWidth=inner-22-6-6-38;
            float textTop=row.Y+(22-11*1.35f-1-7)/2;
            HudRun(draw,HudWeight(400),11,middle,textTop,11*1.35f,unit.Line.Forms[unit.Stage],HudOver(CssMuted,CssPanel,alpha),middleWidth);
            var bar=new float2(middle,textTop+11*1.35f+1);
            draw.AddRectFilled(bar,bar+new float2(middleWidth,7),HudOver(HudHex("262e40"),CssPanel,alpha),3);
            float fraction=(float)(Value(unit)/maximum);
            if(fraction>0)draw.AddRectFilled(bar,bar+new float2(middleWidth*fraction,7),HudOver(chartColors[chartMetric],CssPanel,alpha),3);
            if (hovered)
            {
                ImGui.BeginTooltip(); ImGui.Text(unit.Line.Forms[unit.Stage]);
                ImGui.Text($"Dealt {UnitValues.Round(unit.Dealt)} / Taken {UnitValues.Round(unit.Taken)} / Healed {UnitValues.Round(unit.Healed)}");
                ImGui.EndTooltip();
            }
            y+=22+4;
        }
        ImGui.SetCursorScreenPos(min);ImGui.Dummy(new(width,height));
    }
}

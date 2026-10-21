using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    // #income: 12px/600 muted beside the gold, centered in the 26px #goldBox line.
    private void IncomeDisplay(float x,float lineTop)
    {
        var income = Session!.Rounds.PreviewIncome(Human);
        string label=$"+{income.Total} next round";
        var face=HudWeight(600);
        float width=HudRun(ImGui.GetWindowDrawList(),face,12,x,lineTop,26,label,CssMuted);
        ImGui.SetCursorScreenPos(new(x,lineTop+5));ImGui.InvisibleButton("income"u8,new(Math.Max(1,width),16));
        if (!ImGui.IsItemHovered()) return;
        ImGui.BeginTooltip();
        HudText("ROUND INCOME",22,HudGold);
        ImGui.Text($"Base income: {income.Base}");
        ImGui.Text($"Interest: {income.Interest}");
        ImGui.Text($"Streak: {income.Streak}");
        if (income.Augment != 0) ImGui.Text($"Augments: {income.Augment}");
        if (income.Bonus != 0) ImGui.Text($"Bonus: {income.Bonus}");
        ImGui.Separator();
        if (Rules.Economy!.Interest is { } rate)
            UiText.Wrapped($"Earn 1 interest per {rate.Per} gold held, up to {rate.Max}. Current gold: {Human.Economy.Gold}.");
        else ImGui.Text("No interest in this mode."u8);
        UiText.Wrapped($"Wins pay {Rules.Economy.WinGold} gold. Income changes with your gold, streak, and round.");
        ImGui.EndTooltip();
    }
}

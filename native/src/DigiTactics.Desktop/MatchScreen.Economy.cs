using Brutal.ImGuiApi;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    private void IncomeDisplay()
    {
        var income = Session!.Rounds.PreviewIncome(Human);
        ImGui.SameLine(); HudText($"+{income.Total} next round",16,HudGold);
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

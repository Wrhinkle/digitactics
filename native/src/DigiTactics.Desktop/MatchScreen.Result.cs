using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal sealed partial class MatchScreen
{
    internal string? ResultTitle { get; private set; }
    internal int ResultHealthLoss { get; private set; }

    private void ResultBanner()
    {
        ResultTitle = null; ResultHealthLoss = 0;
        if (Session!.Rounds.Phase != MatchPhase.Result) return;
        var player = Human;
        var own = PlayerRoundResult.Find(Session.Rounds.Results, player);
        if (own is null) return;
        var result = own.Result; bool a = own.SideA, won = own.Won, lost = own.Lost;
        ResultTitle = own.Title;
        ResultHealthLoss = own.HealthLoss;
        string opponent = result.Match.Pve ? "Wild Digimon" : Session.State.Players[(a ? result.Match.B : result.Match.A)!.Value].Name;
        Window("Round result", Left + 12, Top + 12, Width - 24, 80, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoInputs);
        ImGui.PushStyleColor(ImGuiCol.Text, won ? new ImColor8(100,235,170) : lost ? new ImColor8(255,120,120) : new ImColor8(230,235,245));
        ImGui.Text(ResultTitle + " vs " + opponent); ImGui.PopStyleColor();
        var details = new List<string>();
        if (ResultHealthLoss > 0) details.Add($"-{ResultHealthLoss} HP");
        if (won && !result.Match.Pve) details.Add($"+{Rules.Economy!.WinGold} gold");
        if (a && result.Reward is { Orbs: > 0 } reward) details.Add($"{reward.Orbs} loot orb{(reward.Orbs == 1 ? "" : "s")} dropped");
        if (details.Count > 0) UiText.Wrapped(string.Join(" / ", details));
        ImGui.End();
    }
}

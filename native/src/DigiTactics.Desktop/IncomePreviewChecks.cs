using DigiTactics.Rules;

namespace DigiTactics.Desktop;

internal static class IncomePreviewChecks
{
    public static void Run(ContentCatalog catalog)
    {
        int checks = 0;
        foreach (var rules in catalog.Variants)
            foreach (bool duos in new[] { false, true })
            {
                var session = new MatchPlanningSession(rules, 143, Enumerable.Range(0,8).Select(i => new MatchSeat("Tamer " + i, i == 0)).ToArray(), duos);
                var player = session.State.Players[0]; player.Economy.Credit(30);
                string Snapshot() => System.Text.Json.JsonSerializer.Serialize(new { session.State.Random.State,
                    Players = session.State.Players.Select(p => new { p.Economy.Gold, p.Economy.Level, p.Economy.Xp, p.Economy.Streak, p.Items, p.Augments, p.Globals, p.Shop.FreeRolls, p.Shop.Slots }) });
                string before = Snapshot(); var expected = session.Rounds.PreviewIncome(player);
                for (int i = 0; i < 10; i++)
                    if (session.Rounds.PreviewIncome(player) != expected) throw new InvalidOperationException("Income preview changed without a gameplay action.");
                if (before != Snapshot()) throw new InvalidOperationException("Income preview mutated economy, inventory, shop or random state.");
                if (rules.Id.StartsWith("faction-wars/", StringComparison.Ordinal))
                {
                    player.Augments.Add("income");
                    if (session.Rounds.PreviewIncome(player).Total != expected.Total + 2) throw new InvalidOperationException("Income preview omitted its mode augment.");
                    if (duos)
                    {
                        var partner = session.Units.Portal.Partner(player)!; partner.Economy.Credit(30); player.Augments.Add("duo_interest");
                        if (session.Rounds.PreviewIncome(player).Total != expected.Total + 5) throw new InvalidOperationException("Income preview omitted partner interest.");
                    }
                }
                checks++;
            }
        Console.WriteLine($"Income preview checks passed: {checks} mode/format combinations and mode/partner bonuses.");
    }
}

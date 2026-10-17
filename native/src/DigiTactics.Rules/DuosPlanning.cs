namespace DigiTactics.Rules;

/// <summary>Shared Duos planning augment effects; the dispatcher invokes them alongside mode mechanics.</summary>
public sealed class DuosPlanning(MatchState state, MatchPortal portal)
{
    private readonly MatchLoot loot = new(state);
    public void Picked(MatchPlayer player, string id)
    {
        var partner = portal.Partner(player);
        if (id == "duo_gear")
        {
            player.GiveItem(loot.RandomComponent()!); player.GiveItem(loot.RandomComponent()!);
            if (partner is { Alive: true }) partner.GiveItem(loot.RandomComponent()!);
        }
        else if (id == "duo_trove")
        {
            player.GiveItem(loot.RandomCompleted()!); player.Economy.Credit(5);
            if (partner is { Alive: true }) { partner.GiveItem(loot.RandomCompleted()!); partner.Economy.Credit(5); }
        }
    }
    public IncomeBreakdown Income(MatchPlayer player, IncomeBreakdown income) =>
        player.Augments.Contains("duo_interest") && portal.Partner(player) is { Alive: true } partner
            ? income with { Augment = income.Augment + Math.Min(3, Math.Max(0, partner.Economy.Gold) / 10) } : income;
    public void Sent(MatchPlayer player, PortalEvent transfer)
    {
        if (player.Augments.Contains("duo_wide_portal") && transfer.From == player && transfer.To.Alive) transfer.To.Economy.Credit(1);
    }
    public void PartnerCombined(MatchPlayer player, RoundPosition round)
    {
        if (!player.Augments.Contains("duo_proud")) return;
        int number = round.Round;
        for (int stage = 1; stage < round.Stage; stage++) number += state.Rules.Schedule!.RoundsInStage(stage);
        if (player.ProudPartnerRound != number) { player.ProudPartnerRound = number; player.ProudPartnerCount = 0; }
        if (player.ProudPartnerCount >= 3) return;
        player.ProudPartnerCount++; player.Economy.Credit(2);
    }
    public void RoundStart(MatchPlayer player, RoundPosition round, MatchPhase phase, string roundType)
    {
        if (!player.Augments.Contains("duo_care") || portal.Partner(player) is not { Alive: true } partner || roundType == "carousel") return;
        int[] tiers = round.Stage <= 2 ? [1, 2] : round.Stage <= 4 ? [1, 2, 3] : [2, 3, 4];
        portal.Gift(player, partner, tiers, phase);
    }
}

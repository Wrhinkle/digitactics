namespace DigiTactics.Rules;

public sealed record DragonShopCut(int Level, double Chance);

/// <summary>Doot odds, Stargazer guarantees and Dragon replacements run in shared shop order.</summary>
public sealed class DragonlandsShop(MatchState state, MatchPlayer player) : IShopRollHooks
{
    private readonly Dictionary<string, UnitLine> lines = state.Rules.Lines.ToDictionary(l => l.Id);
    public ShopRollRequest BeforeRoll(ShopRollRequest request)
    {
        double doots = player.Mechanics.DragonDoots;
        if (doots == 0) return request;
        var odds = request.Odds?.ToArray() ?? Array.ConvertAll(state.Rules.Odds[Math.Min(state.Rules.Odds.Length - 1, request.Level)]!, x => (double)x);
        double remaining = doots;
        for (int i = 0; i < 2 && remaining > 0; i++) { double moved = Math.Min(remaining, odds[i]); odds[i] -= moved; remaining -= moved; }
        double total = doots - remaining;
        if (total != 0)
        {
            int lower = odds[4] > 0 ? 3 : odds[3] > 0 ? 2 : 1;
            odds[lower] += total * .6; odds[lower + 1] += total * .4;
        }
        return request with { Odds = odds };
    }
    public void Rolled(PlayerShop shop)
    {
        var excluded = player.Roster.MaxedLines();
        if (player.Mechanics.DragonStarShop)
        {
            player.Mechanics.DragonStarShop = false;
            var odds = state.Rules.Odds[Math.Min(state.Rules.Odds.Length - 1, player.Economy.Level)]!;
            var candidates = state.Rules.Lines.Where(l => l.Traits.Contains("STAR") && odds[l.Tier - 1] > 0 &&
                state.Pool.Remaining(l.Id) > 0 && !excluded.Contains(l.Id) && l.Id != shop.Slots[0]).ToArray();
            if (candidates.Length > 0) shop.ReplaceOffer(0, candidates[(int)(state.Random.Next() * candidates.Length)].Id);
        }
        if (state.Rules.DragonShopCut is { } cut && player.Economy.Level >= cut.Level)
        {
            for (int i = 0; i < shop.Slots.Count; i++)
            {
                if (shop.Slots[i] is not { } id || lines[id].Slots != 2 || state.Random.Next() >= cut.Chance) continue;
                var candidates = state.Rules.Lines.Where(l => l.Tier == lines[id].Tier && l.Slots != 2 && state.Pool.Remaining(l.Id) > 0 && !excluded.Contains(l.Id)).ToArray();
                int total = candidates.Sum(l => state.Pool.Remaining(l.Id));
                if (total == 0) continue;
                double pick = state.Random.Next() * total; string chosen = candidates[^1].Id;
                foreach (var line in candidates) { pick -= state.Pool.Remaining(line.Id); if (pick < 0) { chosen = line.Id; break; } }
                shop.ReplaceOffer(i, chosen);
            }
        }
        for (int i = 0; i < shop.Slots.Count; i++)
            if (shop.Slots[i] is { } id && lines[id].Slots == 2) shop.SetMetadata(i, new(lines[id].Price, "Dragon"));
    }
}

namespace DigiTactics.Rules;

/// <summary>Current mode shop listeners in augment, global-item, then set order.</summary>
public sealed class MatchShopRolls
{
    private readonly MatchState state;
    private readonly Func<RoundPosition> position;
    private readonly string mode;
    private readonly FatesPlanning? fates;

    public MatchShopRolls(MatchState state, Func<RoundPosition> position)
    {
        this.state = state; this.position = position;
        mode = state.Rules.Id.Split('/')[0];
        if (mode == "fates") fates = new(state);
    }
    // Call from match initialization to cover starting shops as well as later refreshes.
    public void Install()
    {
        foreach (var player in state.Players)
            player.Shop.RollHooks = new PlayerHooks(this, player, mode == "dragonlands" ? new(state, player) : null);
    }
    private sealed class PlayerHooks(MatchShopRolls owner, MatchPlayer player, DragonlandsShop? dragons) : IShopRollHooks
    {
        public ShopRollRequest BeforeRoll(ShopRollRequest request)
        {
            if (owner.mode == "gizmos" && player.Augments.Contains("giz_a_highend")) request = request with { Level = request.Level + 1 };
            foreach (var id in player.Globals)
                if (owner.mode == "autochess" && id == "ac_r_recruiter" || owner.mode is "underlords" or "underlords_ko" && id == "ul_g_criminal")
                    request = request with { Level = Math.Min(10, request.Level + 1) };
            if (owner.mode == "galaxies" && owner.state.Galaxy == "server") request = request with { Size = 6 };
            return dragons?.BeforeRoll(request) ?? request;
        }
        public void Rolled(PlayerShop shop)
        {
            if (owner.mode == "gizmos" && player.Augments.Contains("giz_a_ticket") && owner.state.Random.Next() < .5) shop.FreeRolls++;
            if (owner.fates is { } fates) fates.OfferPartner(player, owner.position());
            dragons?.Rolled(shop);
        }
    }
}

namespace DigiTactics.Rules;

/// <summary>Applies selected augments and powers through the same unit lifecycle as other match actions.</summary>
public sealed class MatchOfferEffects : IMatchOfferHooks
{
    private readonly Action<MatchPlayer, string> augment;
    private readonly UnderlordsPlanning? underlords;
    private readonly DuosPlanning duos;

    public MatchOfferEffects(MatchState state, MatchUnitLifecycle units, Func<MatchPhase> phase)
    {
        void Combine(MatchPlayer player) => units.Combine(player, phase());
        Action<MatchPlayer, string> Bind(Action<MatchPlayer, string, Action<MatchPlayer>> effect) =>
            (player, id) => effect(player, id, Combine);
        duos = new(state, units.Portal);
        augment = state.Rules.Id.Split('/')[0] switch
        {
            "faction-wars" => new FactionWarsPlanning(state).AugmentPicked,
            "coliseum" => new ColiseumPlanning(state).AugmentPicked,
            "gizmos" => new GizmosPlanning(state).AugmentPicked,
            "galaxies" => new GalaxiesPlanning(state).AugmentPicked,
            "reckoning" => new ReckoningPlanning(state).AugmentPicked,
            "fates" => Bind(new FatesPlanning(state).AugmentPicked),
            "dragonlands" => Bind(new DragonlandsPlanning(state).AugmentPicked),
            "arcane" => Bind(new ArcanePlanning(state).AugmentPicked),
            "underlords" or "underlords_ko" or "autochess" => (_, _) => { },
            _ => throw new NotSupportedException("Unknown offer effect mode: " + state.Rules.Id)
        };
        if (state.Rules.Id.StartsWith("underlords", StringComparison.Ordinal)) underlords = new(state);
    }
    public void AugmentPicked(MatchPlayer player, string id)
    {
        duos.Picked(player, id);
        augment(player, id);
    }
    public void PowerPicked(MatchPlayer player, string id)
    {
        if (underlords is null) throw new NotSupportedException("This mode has no Tamer power pick effects.");
        underlords.PowerPicked(player, id);
    }
    // All catalog offer kinds and custom armories are implemented in MatchOffers/ModeOfferRewards.
    public string[] Armory(MatchPlayer player, RoundPosition round) => throw new NotSupportedException("Unimplemented custom armory.");
    public string[] CustomOptions(MatchPlayer player, RoundPosition round, OfferDefinition definition, IReadOnlyList<string> exclude) =>
        throw new NotSupportedException("Unimplemented offer kind: " + definition.Kind);
    public void CustomChosen(MatchPlayer player, PlayerOffer offer, string id) => throw new NotSupportedException("Unimplemented offer choice: " + offer.Kind);
}
